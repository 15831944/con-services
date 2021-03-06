﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Proxies;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.WebApi.Models.ProductionData.Contracts;
using VSS.Productivity3D.WebApi.Models.ProductionData.Executors;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;
using VSS.TRex.Gateway.Common.Abstractions;

namespace VSS.Productivity3D.WebApi.ProductionData.Controllers
{
  /// <summary>
  /// Controller for the ProfileProductionData resource.
  /// </summary>
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
  public class ProfileProductionDataController : Controller, IProfileProductionDataContract
  {
#if RAPTOR
    /// <summary>
    /// Raptor client for use by executor
    /// </summary>
    private readonly IASNodeClient raptorClient;
#endif
    /// <summary>
    /// LoggerFactory factory for use by executor
    /// </summary>
    private readonly ILoggerFactory logger;

    /// <summary>
    /// Where to get environment variables, connection string etc. from
    /// </summary>
    protected readonly IConfigurationStore ConfigStore;

    /// <summary>
    /// The TRex Gateway proxy for use by executor.
    /// </summary>
    protected readonly ITRexCompactionDataProxy TRexCompactionDataProxy;

    /// <summary>
    /// Gets the custom headers for the request.
    /// </summary>
    protected IHeaderDictionary CustomHeaders => Request.Headers.GetCustomHeaders();

    private readonly IFileImportProxy FileImportProxy;

    private string UserId => User.Identity.Name;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public ProfileProductionDataController(
#if RAPTOR
      IASNodeClient raptorClient, 
#endif
      ILoggerFactory logger, IConfigurationStore configStore, ITRexCompactionDataProxy trexCompactionDataProxy, IFileImportProxy fileImportProxy)
    {
#if RAPTOR
      this.raptorClient = raptorClient;
#endif
      this.logger = logger;
      ConfigStore = configStore;
      TRexCompactionDataProxy = trexCompactionDataProxy;
      FileImportProxy = fileImportProxy;
    }

    /// <summary>
    /// Posts a profile production data request to a Raptor's data model/project.
    /// </summary>
    /// <param name="request">Profile production data request structure.></param>
    /// <returns>
    /// Returns JSON structure wtih operation result as profile calculations./>
    /// </returns>
    [PostRequestVerifier]
    [ProjectVerifier]
    [Route("api/v1/profiles/productiondata")]
    [HttpPost]
    public async Task<ProfileResult> Post([FromBody] ProfileProductionDataRequest request)
    {
      request.Validate();

      return await RequestExecutorContainerFactory.Build<ProfileProductionDataExecutor>(logger,
#if RAPTOR
          raptorClient,
#endif
          configStore: ConfigStore, trexCompactionDataProxy: TRexCompactionDataProxy, customHeaders: CustomHeaders,
          userId: UserId, fileImportProxy: FileImportProxy)
        .ProcessAsync(request) as ProfileResult;
    }
  }
}
