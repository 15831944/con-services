﻿using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using VSS.AWS.TransferProxy;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.TRex.Common;
using VSS.TRex.DI;
using VSS.TRex.TAGFiles.Classes.Validator;

namespace VSS.TRex.TAGFiles.Classes
{
  /// <summary>
  /// Static class for archiving processed tagfiles
  /// </summary>
  public static class TagFileRepository
  {
    const string S3DirectorySeparator = "/";

    private static readonly ILogger _log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    private static readonly bool enableArchivingMetadata = DIContext.Obtain<IConfigurationStore>().GetValueBool("ENABLE_TAGFILE_ARCHIVING_METADATA", Consts.ENABLE_TAGFILE_ARCHIVING_METADATA);

    private static string MakePath(TagFileDetail td)
    {
      var config = DIContext.Obtain<IConfigurationStore>();
      string tagFileArchiveFolder = config.GetValueString("TAGFILE_ARCHIVE_FOLDER");
      if (!string.IsNullOrEmpty(tagFileArchiveFolder))
        return Path.Combine(tagFileArchiveFolder, td.projectId.ToString(), td.assetId.ToString());

      return Path.Combine(Path.GetTempPath(), "TRexIgniteData", "TagFileArchive", td.projectId.ToString(), td.assetId.ToString());
    }

    /// <summary>
    /// Archives successfully processed tag files to S3
    /// </summary>
    /// <param name="tagDetail"></param>
    /// <returns></returns>
    public static bool ArchiveTagfileS3(TagFileDetail tagDetail)
    {
      try
      {
        var s3FullPath = $"{tagDetail.projectId}{S3DirectorySeparator}{tagDetail.assetId}{S3DirectorySeparator}{tagDetail.tagFileName}";
        var proxy = DIContext.Obtain<ITransferProxyFactory>().NewProxy(TransferProxyType.TAGFiles);
        using var stream = new MemoryStream(tagDetail.tagFileContent);
        proxy.Upload(stream, s3FullPath);
        return true;
      }

      catch (System.Exception ex)
      {
        _log.LogError(ex, $"Exception occured archiving tagfilesaving {tagDetail.tagFileName}. Asset{tagDetail.assetId}, error:{ex.Message}");
        return false;
      }
    }

    /// <summary>
    /// Archives successfully processed tag files to a local location
    /// Note! S3 option perferred for WorksOS operation, however this function could be useful for a future selfcontained setup
    /// </summary>
    /// <param name="tagDetail"></param>
    /// <returns></returns>
    public static bool ArchiveTagfileLocal(TagFileDetail tagDetail)
    {

      string thePath = MakePath(tagDetail);
      if (!Directory.Exists(thePath))
        Directory.CreateDirectory(thePath);

      string fType = "tagfile";

      string ArchiveTAGFilePath = Path.Combine(thePath, tagDetail.tagFileName);

      // We don't keep duplicates in TRex
      if (File.Exists(ArchiveTAGFilePath))
        File.Delete(ArchiveTAGFilePath);

      try
      {
        using (FileStream file = new FileStream(ArchiveTAGFilePath, FileMode.Create, System.IO.FileAccess.Write))
        {
          file.Write(tagDetail.tagFileContent, 0, tagDetail.tagFileContent.Length);
        }

        _log.LogDebug($"Tagfile archived to {ArchiveTAGFilePath}");

        /* This feature is not required in TRex. Plus not sure if under netcore the serializer is working probably so commented out for now
          leaving code here in case we change our minds in future

        IConfigurationStore config = DIContext.Obtain<IConfigurationStore>();
        if (config.GetValue<bool>("ENABLE_TAGFILE_ARCHIVING_METADATA", false))
        {
          fType = "metafile";
          string ArchiveTAGFileMetaDataPath = Path.ChangeExtension(ArchiveTAGFilePath, ".xml");

          if (File.Exists(ArchiveTAGFileMetaDataPath))
            File.Delete(ArchiveTAGFileMetaDataPath);

          // TAG file MetaData
          TAGFileMetaData tmd = new TAGFileMetaData()
          {
            projectId = tagDetail.projectId,
            assetId = tagDetail.assetId,
            tccOrgId = tagDetail.tccOrgId,
            tagFileName = tagDetail.tagFileName,
            IsJohnDoe = tagDetail.IsJohnDoe
          };

          using (FileStream file = new FileStream(ArchiveTAGFileMetaDataPath, FileMode.Create,
                  System.IO.FileAccess.Write))
          {
            new XmlSerializer(typeof(TAGFileMetaData)).Serialize(file, tmd);
          }
          

        } */

        // Another process should move tag files eventually to S3 bucket

        return true;
      }

      catch (System.Exception e)
      {
        _log.LogWarning($"Exception occured saving {fType}. error:{e.Message}");
        return false;
      }
    }


    public static bool MoveToUnableToProcess(TagFileDetail tagDetail)
    {
      // todo: Should be moved to a common location. To preserve state we could save all details as a json file which includes the state and binary content. 
      return true;
    }

    /// <summary>
    /// Returns tag file content and meta data for an archived tag file. Input requires filename and project id
    /// </summary>
    /// <param name="tagDetail"></param>
    /// <returns></returns>
    public static TagFileDetail GetTagFileLocal(TagFileDetail tagDetail)
    {
      // just requires the project id and tag file name to be set
      string ArchiveTAGFilePath = Path.Combine(MakePath(tagDetail), tagDetail.tagFileName);
      string ArchiveTAGFileMetaDataPath = Path.ChangeExtension(ArchiveTAGFilePath, ".xml");

      if (!File.Exists(ArchiveTAGFilePath))
        return tagDetail;

      using (FileStream file = new FileStream(ArchiveTAGFilePath, FileMode.Open, FileAccess.Read))
      {
        tagDetail.tagFileContent = new byte[(int)file.Length];
        file.Read(tagDetail.tagFileContent, 0, (int)file.Length);
      }

      // load xml data ArchiveTagFileMetaDataPath and put into tagDetail
      // if using location only for metadata then you would have to extract it from the path

      if (enableArchivingMetadata && File.Exists(ArchiveTAGFileMetaDataPath))
      {
        FileStream ReadFileStream = new FileStream(ArchiveTAGFileMetaDataPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        XmlSerializer SerializerObj = new XmlSerializer(typeof(TAGFileMetaData));

        // Load the object saved above by using the Deserialize function
        TAGFileMetaData tmd = (TAGFileMetaData)SerializerObj.Deserialize(ReadFileStream);
        tagDetail.IsJohnDoe = tmd.IsJohnDoe;
        tagDetail.projectId = tmd.projectId;
        tagDetail.assetId = tmd.assetId;
        tagDetail.tagFileName = tmd.tagFileName;
        tagDetail.tccOrgId = tmd.tccOrgId;

        // Cleanup
        ReadFileStream.Close();
      }

      return tagDetail;
    }

  }
}
