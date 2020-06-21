﻿using System;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MimeTypes;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.Http;

namespace VSS.AWS.TransferProxy
{
  public class TransferProxy : ITransferProxy
  {
    private readonly string awsBucketName;
    private readonly TimeSpan awsLinkExpiry;
    private readonly ILogger logger;
    private readonly string awsProfile;

    private const int MAXIMUM_EXPIRY_DAYS_FOR_PRESIGNED_URL = 7;

    public TransferProxy(IConfigurationStore configStore, ILogger<TransferProxy> log, string storageKey)
    {
      logger = log;
      if (string.IsNullOrEmpty(storageKey))
      {
        throw new ArgumentException($"Missing environment variable {storageKey}", nameof(storageKey));
      }
      awsBucketName = configStore.GetValueString(storageKey);
      //AWS profile used for debugging - set value to "fsm-okta"
      awsProfile = configStore.GetValueString("AWS_PROFILE", string.Empty);

      awsLinkExpiry = configStore.GetValueTimeSpan("AWS_PRESIGNED_URL_EXPIRY") ??
                      TimeSpan.FromDays(MAXIMUM_EXPIRY_DAYS_FOR_PRESIGNED_URL);

      logger.LogInformation("AWS S3 Now using Assumed Roles");
    }

    private TransferUtility GetTransferUtility()
    {
      return new TransferUtility(GetS3Client());
    }

    private IAmazonS3 GetS3Client()
    {
      if (string.IsNullOrEmpty(awsProfile))
        return new AmazonS3Client(RegionEndpoint.USWest2);
      return new AmazonS3Client(new StoredProfileAWSCredentials(awsProfile), RegionEndpoint.USWest2);
    }

    public async Task<FileStreamResult> DownloadFromBucket(string s3Key, string bucketName)
    {
      using (var transferUtil = GetTransferUtility())
      {
        var stream = await transferUtil.OpenStreamAsync(bucketName, s3Key);
        string mimeType;
        try
        {
          var extension = Path.GetExtension(s3Key);

          mimeType = MimeTypeMap.GetMimeType(extension);
        }
        catch (ArgumentException)
        {
          // Unknown or invalid extension, not bad but we don't know the content type
          mimeType = ContentTypeConstants.ApplicationOctetStream; // binary data....
        }

        return new FileStreamResult(stream, mimeType);
      }
    }

    /// <summary>
    /// Create a task to download a file from S3 storage
    /// </summary>
    /// <param name="s3Key">Key to the data to be downloaded</param>
    /// <returns>FileStreamResult if the file exists</returns>
    public Task<FileStreamResult> Download(string s3Key) => DownloadFromBucket(s3Key, awsBucketName);

    public void UploadToBucket(Stream stream, string s3Key, string bucketName)
    {
      using (var transferUtil = GetTransferUtility())
      {
        transferUtil.Upload(stream, bucketName, s3Key);
      }
    }

    public void Upload(Stream stream, string s3Key)
    {
      UploadToBucket(stream, s3Key, awsBucketName);
    }

    /// <summary>
    /// Upload a file to S3. The filepath will contain the extension matched to the content type if not already present
    /// </summary>
    /// <param name="stream">Content stream to upload</param>
    /// <param name="s3Key">s3 key (filename) to content</param>
    /// <param name="contentType">Content Type of the data being uploaded (defines extension of the filename)</param>
    /// <returns>Path to the file (could be renamed based on the contentType)</returns>
    /// <exception cref="ArgumentException">If invalid contentType is passed in</exception>
    public string Upload(Stream stream, string s3Key, string contentType)
    {
      var extension = Path.GetExtension(s3Key);
      var expectedExtension = MimeTypeMap.GetExtension(contentType);

      var path = s3Key;

      if (string.Compare(expectedExtension, extension, StringComparison.InvariantCultureIgnoreCase) != 0)
      {
        // Do we want to replace the existing extension???
        path = s3Key + expectedExtension;
      }

      Upload(stream, path);
      return path;
    }

    public string GeneratePreSignedUrl(string s3Key)
    {
      var request = new GetPreSignedUrlRequest
      {
        BucketName = awsBucketName,
        Key = s3Key,
        Expires = DateTime.Now.Add(awsLinkExpiry)
      };

      using (var s3Client = GetS3Client())
      {
        return s3Client.GetPreSignedURL(request);
      }
    }

    public bool RemoveFromBucket(string s3Key)
    {
      using (var s3Client = GetS3Client())
      {
        s3Client.DeleteObjectAsync(awsBucketName, s3Key);
        return true;
      }
    }
  }
}
