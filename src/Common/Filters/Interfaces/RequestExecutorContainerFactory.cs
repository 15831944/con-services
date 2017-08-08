﻿using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.TCCFileAccess;

namespace VSS.Productivity3D.Common.Filters.Interfaces
{
  public class RequestExecutorContainerFactory
  {
    /// <summary>
    /// Builds this instance for specified executor type.
    /// </summary>
    /// <typeparam name="TExecutor">The type of the executor.</typeparam>
    /// <returns></returns>
    public static TExecutor Build<TExecutor>(ILoggerFactory logger, IASNodeClient raptorClient = null,
      ITagProcessor tagProcessor = null, IConfigurationStore configStore = null, IFileRepository fileRepo = null,
      ITileGenerator tileGenerator = null, List<FileData> fileList = null)
      where TExecutor : RequestExecutorContainer, new()
    {
      ILogger log = null;
      if (logger != null)
      {
        log = logger.CreateLogger<RequestExecutorContainer>();
      }

      var executor = new TExecutor();

      executor.Initialise(
      log,
      raptorClient,
      tagProcessor,
      configStore,
      fileRepo,
      tileGenerator,
      fileList);

      return executor;
    }
  }
}