﻿using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.WebApi.Compaction.Controllers;
using VSS.Productivity3D.WebApi.Models.Compaction.Executors;
using VSS.Productivity3D.WebApi.Models.ProductionData.Executors.CellPass;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;
using VSS.TRex.Gateway.Common.Abstractions;

namespace VSS.Productivity3D.WebApi.ProductionData.Controllers
{
  /// <summary>
  /// Cell and cell patches controller.
  /// </summary>
  [ProjectVerifier]
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
  public class CellController : BaseController<CellController>
  {
#if RAPTOR
    private readonly IASNodeClient raptorClient;
#endif
    private readonly ILoggerFactory logger;
    private readonly ITRexCompactionDataProxy trexCompactionDataProxy;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public CellController(
#if RAPTOR
      IASNodeClient raptorClient, 
#endif
      ILoggerFactory logger, 
      IConfigurationStore configStore, 
      ITRexCompactionDataProxy trexCompactionDataProxy, 
      IFileImportProxy fileImportProxy,
      ICompactionSettingsManager settingsManager) 
      : base(configStore, fileImportProxy, settingsManager)
    {
#if RAPTOR
      this.raptorClient = raptorClient;
#endif
      this.logger = logger;
      this.trexCompactionDataProxy = trexCompactionDataProxy;
    }

    /// <summary>
    /// Retrieve passes for a single cell and process them according to the provided filter and layer analysis parameters
    /// </summary>
    /// <param name="request">The request representation for the operation</param>
    /// <returns>A representation of the cell that contains summary information relative to the cell as a whole, a collection of layers derived from layer analysis and the collection of cell passes that met the filter conditions.</returns>
    /// <executor>CellPassesExecutor</executor>
    [PostRequestVerifier]
    [Route("api/v1/productiondata/cells/passes")]
    [HttpPost]
    public async Task<CellPassesResult> CellPasses([FromBody]CellPassesRequest request)
    {
      request.Validate();
      
      return await RequestExecutorContainerFactory.Build<CellPassesExecutor>(logger,
#if RAPTOR
        raptorClient,
#endif
        configStore: ConfigStore, trexCompactionDataProxy: trexCompactionDataProxy).ProcessAsync(request) as CellPassesResult;
    }

    /// <summary>
    /// Requests a single thematic datum value from a single cell. Examples are elevation, compaction. temperature etc. The request body contains all necessary parameters.
    /// The cell may be identified by either WGS84 lat/long coordinates or by project grid coordinates.
    /// </summary>
    /// <param name="request">The request body parameters for the request.</param>
    /// <returns>The requested thematic value expressed as a floating point number. Interpretation is dependant on the thematic domain.</returns>
    [PostRequestVerifier]
    [Route("api/v1/productiondata/cells/datum")]
    [HttpPost]
    public async Task<CellDatumResult> Post([FromBody]CellDatumRequest request)
    {
      request.Validate();
      return await RequestExecutorContainerFactory.Build<CellDatumExecutor>(logger,
#if RAPTOR
        raptorClient,
#endif
        configStore: ConfigStore, trexCompactionDataProxy: trexCompactionDataProxy).ProcessAsync(request) as CellDatumResult;
    }

    /// <summary>
    /// Requests cell passes information in patches (raw Raptor data output)
    /// </summary>
    [PostRequestVerifier]
    [Route("api/v1/productiondata/patches")]
    [HttpPost]
    public ContractExecutionResult Post([FromBody]PatchRequest request)
    {
      request.Validate();
#if RAPTOR
      return RequestExecutorContainerFactory.Build<PatchExecutor>(logger, raptorClient).Process(request);
#else
      throw new ServiceException(HttpStatusCode.BadRequest,
        new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "TRex unsupported request"));
#endif
    }

    /// <summary>
    /// Requests cell passes information in patches but returning co-ordinates relative to the world origin rather than cell origins.
    /// </summary>
    [PostRequestVerifier]
    [Route("api/v1/productiondata/patches/worldorigin")]
    [HttpPost]
    public ContractExecutionResult GetSubGridPatchesAsWorldOrigins([FromBody]PatchRequest request)
    {
      request.Validate();
#if RAPTOR
      return RequestExecutorContainerFactory.Build<CompactionPatchExecutor>(logger, raptorClient).Process(request);
#else
      throw new ServiceException(HttpStatusCode.BadRequest,
        new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "TRex unsupported request"));
#endif
    }
  }
}
