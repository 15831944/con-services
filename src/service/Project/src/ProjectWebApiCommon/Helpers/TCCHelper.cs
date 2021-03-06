﻿using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Extensions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.TCCFileAccess;
using VSS.TCCFileAccess.Models;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Helpers
{
  /// <summary>
  ///
  /// </summary>
  public class TccHelper
  {
    /// <summary>
    /// get file content from TCC
    ///     note that is is intended to be used for small, DC files only.
    ///     If/when it is needed for large files, 
    ///           e.g. surfaces, you should use a smaller buffer and loop to read.
    /// </summary>
    public static async Task<byte[]> GetFileContentFromTcc(BusinessCenterFile businessCentreFile,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler, IFileRepository fileRepo)
    {
      Stream memStream = null;
      var tccPath = $"{businessCentreFile.Path}/{businessCentreFile.Name}";
      byte[] coordSystemFileContent = null;
      int numBytesRead = 0;

      try
      {
        log.LogInformation(
          $"GetFileContentFromTcc: getBusinessCentreFile fielspaceID: {businessCentreFile.FileSpaceId} tccPath: {tccPath}");
        memStream = await fileRepo.GetFile(businessCentreFile.FileSpaceId, tccPath).ConfigureAwait(false);

        if (memStream != null && memStream.CanRead && memStream.Length > 0)
        {
          coordSystemFileContent = new byte[memStream.Length];
          int numBytesToRead = (int) memStream.Length;
          numBytesRead = memStream.Read(coordSystemFileContent, 0, numBytesToRead);
        }
        else
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError,
            80, $" isAbleToRead: {memStream != null && memStream.CanRead} bytesReturned: {memStream?.Length ?? 0}");
        }
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 79, e.Message);
      }
      finally
      {
        memStream?.Dispose();
      }

      log.LogInformation(
        $"GetFileContentFromTcc: numBytesRead: {numBytesRead} coordSystemFileContent.Length {coordSystemFileContent?.Length ?? 0}");
      return coordSystemFileContent;
    }

    /// <summary>
    /// get file content from TCC
    ///     note that is is intended to be used for small, DC files only.
    ///     If/when it is needed for large files, 
    ///           e.g. surfaces, you should use a smaller buffer and loop to read.
    /// </summary>
    public static async Task<Stream> GetFileStreamFromTcc(BusinessCenterFile businessCentreFile,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler, IFileRepository fileRepo)
    {
      Stream memStream = null;
      var tccPath = $"{businessCentreFile.Path}/{businessCentreFile.Name}";

      try
      {
        log.LogInformation(
          $"GetFileStreamFromTcc: getBusinessCentreFile fielspaceID: {businessCentreFile.FileSpaceId} tccPath: {tccPath}");
        memStream = await fileRepo.GetFile(businessCentreFile.FileSpaceId, tccPath).ConfigureAwait(false);

        if (memStream == null || !memStream.CanRead || memStream.Length < 1)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError,
            80, $" isAbleToRead: {memStream != null && memStream.CanRead} bytesReturned: {memStream?.Length ?? 0}");
        }
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 79, e.Message);
      }

      log.LogInformation($"GetFileStreamFromTcc: Successfully read memstream. bytesReturned: {memStream?.Length ?? 0}");
      return memStream;
    }

    /// <summary>
    /// Get the FileCreated and Updated UTCs
    ///    and checks that the file exists.
    /// </summary>
    /// <returns></returns>
    public static async Task<DirResult> GetFileInfoFromTccRepository(BusinessCenterFile sourceFile,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler, IFileRepository fileRepo)
    {
      DirResult fileEntry = null;

      try
      {
        log.LogInformation(
          $"GetFileInfoFromTccRepository: GetFileList filespaceID: {sourceFile.FileSpaceId} tccPathSource: {sourceFile.Path} sourceFile.Name: {sourceFile.Name}");

        var dirResult = await fileRepo.GetFileList(sourceFile.FileSpaceId, sourceFile.Path, sourceFile.Name);

        log.LogInformation(
          $"GetFileInfoFromTccRepository: GetFileList dirResult: {JsonConvert.SerializeObject(dirResult)}");


        if (dirResult == null || dirResult.entries.Length == 0)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 94, "fileRepo.GetFileList");
        }
        else
        {
          fileEntry = dirResult.entries.FirstOrDefault(f =>
            !f.isFolder && (string.Compare(f.entryName, sourceFile.Name, true, CultureInfo.InvariantCulture) == 0));
          if (fileEntry == null)
          {
            serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 94,
              "fileRepo.GetFileList");
          }
        }
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 94, "fileRepo.GetFileList",
          e.Message);
      }

      return fileEntry;
    }

    /// <summary>
    /// Copies importedFile between filespaces in TCC
    ///     From FilespaceIDBcCustomer\BC Data to FilespaceIdVisionLink\CustomerUID\ProjectUID
    ///   returns filespaceID; path and filename which identifies it uniquely in TCC
    ///   this may be a create or update, so ok if it already exists
    /// </summary>
    /// <returns></returns>
    public static async Task<FileDescriptor> CopyFileWithinTccRepository(ImportedFileTbc sourceFile,
      string customerUid, string projectUid, string dstFileSpaceId,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler, IFileRepository fileRepo)
    {
      var srcTccPathAndFile = $"{sourceFile.Path}/{sourceFile.Name}";
      var destTccPath = $"/{customerUid}/{projectUid}";

      string tccDestinationFileName = sourceFile.Name;
      if (sourceFile.ImportedFileTypeId == ImportedFileType.SurveyedSurface)
        tccDestinationFileName =
          tccDestinationFileName.IncludeSurveyedUtcInName(sourceFile.SurfaceFile.SurveyedUtc);

      var destTccPathAndFile = $"/{customerUid}/{projectUid}/{tccDestinationFileName}";
      var tccCopyFileResult = false;

      try
      {
        // The filename already contains the surveyUtc where appropriate
        log.LogInformation(
          $"CopyFileWithinTccRepository: srcFileSpaceId: {sourceFile.FileSpaceId} destFileSpaceId {dstFileSpaceId} srcTccPathAndFile {srcTccPathAndFile} destTccPathAndFile {destTccPathAndFile}");

        // check for exists first to avoid an misleading exception in our logs.
        var folderAlreadyExists = await fileRepo.FolderExists(dstFileSpaceId, destTccPath).ConfigureAwait(false);
        if (folderAlreadyExists == false)
          await fileRepo.MakeFolder(dstFileSpaceId, destTccPath).ConfigureAwait(false);

        // this creates folder if it doesn't exist, and upserts file if it does
        tccCopyFileResult = await fileRepo
          .CopyFile(sourceFile.FileSpaceId, dstFileSpaceId, srcTccPathAndFile, destTccPathAndFile)
          .ConfigureAwait(false);
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 92, "fileRepo.PutFile",
          e.Message);
      }

      if (tccCopyFileResult == false)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 92);
      }

      var fileDescriptorTarget =
        FileDescriptor.CreateFileDescriptor(dstFileSpaceId, destTccPath, tccDestinationFileName);
      log.LogInformation(
        $"CopyFileWithinTccRepository: fileDescriptorTarget {JsonConvert.SerializeObject(fileDescriptorTarget)}");
      return fileDescriptorTarget;
    }
  }
}
