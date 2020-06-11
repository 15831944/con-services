﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.DataOcean.Client;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Pegasus.Client;
using VSS.Productivity3D.Filter.Abstractions.Interfaces;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces.Repository;
using VSS.Productivity3D.Scheduler.Abstractions;
using VSS.TCCFileAccess;
using VSS.TRex.Gateway.Common.Abstractions;
using VSS.WebApi.Common;

namespace VSS.MasterData.Project.WebAPI.Common.Executors
{
  public static class RequestExecutorContainerFactory
  {
    /// <summary>
    /// Builds this instance for specified executor type.
    /// </summary>
    /// <typeparam name="TExecutor">The type of the executor.</typeparam>
    public static TExecutor Build<TExecutor>(
      ILoggerFactory logger, IConfigurationStore configStore, IServiceExceptionHandler serviceExceptionHandler,
      string customerUid = null, string userId = null, string userEmailAddress = null, IHeaderDictionary headers = null,
      IProductivity3dV1ProxyCoord productivity3dV1ProxyCoord = null, IProductivity3dV2ProxyNotification productivity3dV2ProxyNotification = null,
      IProductivity3dV2ProxyCompaction productivity3dV2ProxyCompaction = null,
      ITransferProxyFactory persistantTransferProxyFactory = null, IFilterServiceProxy filterServiceProxy = null, ITRexImportFileProxy tRexImportFileProxy = null,
      IProjectRepository projectRepo = null, IFileRepository fileRepo = null,
      IHttpContextAccessor httpContextAccessor = null, IDataOceanClient dataOceanClient = null,
      ITPaaSApplicationAuthentication authn = null, ISchedulerProxy schedulerProxy = null, IPegasusClient pegasusClient = null,
      ICwsProjectClient cwsProjectClient = null, ICwsDeviceClient cwsDeviceClient = null,
      ICwsDesignClient cwsDesignClient = null, ICwsProfileSettingsClient cwsProfileSettingsClient = null,
      IWebRequest gracefulClient = null
      )
      where TExecutor : RequestExecutorContainer, new()
    {
      ILogger log = null;
      if (logger != null)
      {
        log = logger.CreateLogger<RequestExecutorContainer>();
      }

      var executor = new TExecutor();

      executor.Initialise(
        log, configStore, serviceExceptionHandler, customerUid, userId, userEmailAddress, headers,
        productivity3dV1ProxyCoord, productivity3dV2ProxyNotification, productivity3dV2ProxyCompaction,
        persistantTransferProxyFactory, filterServiceProxy, tRexImportFileProxy, projectRepo, fileRepo,
        httpContextAccessor, dataOceanClient, authn, schedulerProxy, pegasusClient, cwsProjectClient, cwsDeviceClient,
        cwsDesignClient, cwsProfileSettingsClient, gracefulClient
        );

      return executor;
    }
  }
}
