﻿using System;
using System.IO;
using System.Net;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.FileAccess.WebAPI.Models.Models;
using VSS.Productivity3D.FileAccess.WebAPI.Models.ResultHandling;
using VSS.TCCFileAccess;

namespace VSS.Productivity3D.FileAccess.WebAPI.Models.Executors
{
  public class RawFileAccessExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    public RawFileAccessExecutor(ILoggerFactory logger, IConfigurationStore configStore, IFileRepository fileAccess)
      : base(logger, configStore, fileAccess)
    { }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public RawFileAccessExecutor()
    { }

    /// <summary>
    /// Processes the raw file access request by getting the file from TCC and returning its contents as bytes.
    /// </summary>
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      bool success = false;
      byte[] data = null;

      FileDescriptor request = item as FileDescriptor;
      log.LogInformation("RawFileAccessExecutor: {0}: {1}\\{2}", request.FilespaceId, request.Path, request.FileName);

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
            log.LogInformation("RawFileAccessExecutor: Succeeded in reading {0}: {1}\\{2}",request.FilespaceId, request.Path, request.FileName);
          }
          else
          {
            log.LogInformation("RawFileAccessExecutor: Failed to read {0}: {1}\\{2} (stream is 0 length)",request.FilespaceId, request.Path, request.FileName);
          }
        }
        else
        {
          log.LogInformation("Unable to log into TCC as RawFileAccessExecutor user.");
        }
      }
      catch (Exception ex)
      {
        log.LogError(null, ex, "***ERROR*** FileAccessExecutor: Failed on getting {0} file from TCC!",request.FileName);
      }

      if (success)
      {
        return RawFileAccessResult.CreateRawFileAccessResult(data);
      }

      throw new ServiceException(HttpStatusCode.BadRequest,new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Failed to download file from TCC"));
    }

    protected override void ProcessErrorCodes()
    {
      //Nothing to do
    }

    private void DownloadFile(IFileRepository fileAccess, FileDescriptor file, Stream stream)
    {
      string fullName = string.IsNullOrEmpty(file.FileName) ? file.Path : Path.Combine(file.Path, file.FileName);
      fullName = fullName.Replace(Path.DirectorySeparatorChar, '/');
      log.LogDebug("DownloadFile: {0}: {1} {2}", file.FilespaceId, file.Path, file.FileName);
      var downloadFileResult = fileAccess.GetFile(file.FilespaceId, fullName).Result;
      log.LogDebug("DownloadFile Result length: {0}", downloadFileResult.Length);
      if (downloadFileResult != null && downloadFileResult.Length > 0)
      {
        downloadFileResult.Seek(0, SeekOrigin.Begin);
        downloadFileResult.CopyTo(stream);
      }
    }
  }
}
