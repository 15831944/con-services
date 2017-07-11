﻿using MasterDataProxies.ResultHandling;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using VSS.GenericConfiguration;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.TCCFileAccess;
using WebApiModels.FileAccess.ResultHandling;

namespace WebApiModels.FileAccess.Executors
{
  public class RawFileAccessExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="raptorClient"></param>
    /// 
    public RawFileAccessExecutor(ILoggerFactory logger, IConfigurationStore configStore, IFileRepository fileAccess)
      : base(logger, configStore, fileAccess)
    {
      // ...
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public RawFileAccessExecutor()
    {
      // ...
    }

    /// <summary>
    /// Processes the raw file access request by getting the file from TCC and returning its contents as bytes.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a RawFileAccessResult if successful</returns>      
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      bool success = false;
      byte[] data = null;

      FileDescriptor request = item as FileDescriptor;
      log.LogInformation("RawFileAccessExecutor: {0}: {1}\\{2}", request.filespaceId, request.path, request.fileName);

      try
      {
        if (fileAccess != null)
        {
          MemoryStream stream = new MemoryStream();
          DownloadFile(fileAccess, request, stream);

          if (stream.Length > 0)
          {
            stream.Position = 0;
            data = new byte[stream.Length];
            stream.Read(data, 0, (int)stream.Length);
            success = true;
            log.LogInformation("RawFileAccessExecutor: Succeeded in reading {0}: {1}\\{2}",
                request.filespaceId, request.path, request.fileName);
          }
          else
          {
            log.LogInformation("RawFileAccessExecutor: Failed to read {0}: {1}\\{2} (stream is 0 length)",
                request.filespaceId, request.path, request.fileName);
          }
        }
        else
        {
          log.LogInformation("Unable to log into TCC as RawFileAccessExecutor user.");
        }
      }
      catch (Exception ex)
      {
        log.LogError(null, ex, "***ERROR*** FileAccessExecutor: Failed on getting {0} file from TCC!",
            request.fileName);
      }

      if (success)
      {
        return RawFileAccessResult.CreateRawFileAccessResult(data);
      }

      throw new ServiceException(HttpStatusCode.BadRequest,
        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Failed to download file from TCC"));
    }

    protected override void ProcessErrorCodes()
    {
      //Nothing to do
    }

    private void DownloadFile(IFileRepository fileAccess, FileDescriptor file, Stream stream)
    {
      string fullName = string.IsNullOrEmpty(file.fileName) ? file.path : Path.Combine(file.path, file.fileName);
      fullName = fullName.Replace(Path.DirectorySeparatorChar, '/');

      var downloadFileResult = fileAccess.GetFile(file.filespaceId, fullName).Result;

      if (downloadFileResult != null && downloadFileResult.Length > 0)
      {
        downloadFileResult.Seek(0, SeekOrigin.Begin);
        downloadFileResult.CopyTo(stream);
      }
    }
  }
}