﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Productivity3D.WebApi.Models.ProductionData.Contracts;
using VSS.Productivity3D.WebApi.Models.ProductionData.Executors;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApi.ProductionData.Controllers
{
  /// <summary>
  /// 
  /// </summary>
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
  [ProjectVerifier]
  public class EditDataController : IEditDataContract
  {
#if RAPTOR
    private readonly ITagProcessor tagProcessor;
  private readonly IASNodeClient raptorClient;
#endif
    private readonly ILoggerFactory logger;
    private readonly IConfigurationStore configStore;

    /// <summary>
    /// Constructor with dependency injection
    /// </summary>
    public EditDataController(
#if RAPTOR
      IASNodeClient raptorClient, 
      ITagProcessor tagProcessor, 
#endif
      ILoggerFactory logger,
      IConfigurationStore configStore)
    {
#if RAPTOR
      this.raptorClient = raptorClient;
      this.tagProcessor = tagProcessor;
#endif
      this.logger = logger;
      this.configStore = configStore;
    }

    /// <summary>
    /// Gets a list of edits or overrides of the production data for a project and machine.
    /// </summary>
    /// <returns>A list of the edits applied to the production data for the project and machine.</returns>
    [PostRequestVerifier]
    [Route("api/v1/productiondata/getedits")]
    [HttpPost]
    public EditDataResult PostEditDataAcquire([FromBody] GetEditDataRequest request)
    {
      request.Validate();
#if RAPTOR
      return RequestExecutorContainerFactory.Build<GetEditDataExecutor>(logger, raptorClient, tagProcessor).Process(request) as EditDataResult;
#else
      throw new ServiceException(HttpStatusCode.BadRequest,
        new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "TRex unsupported request"));
#endif
    }

    /// <summary>
    /// Applies an edit to production data to correct data that has been recorded wrongly in Machines by Operator.
    /// </summary>
    [Obsolete("This is a BusinessCenter endpoint. It is not expected that this endpoint will have a v2")]
    [PostRequestVerifier]
    [Route("api/v1/productiondata/edit")]
    [HttpPost]
    public async Task<ContractExecutionResult> Post([FromBody]EditDataRequest request)
    {
      request.Validate();
#if RAPTOR
      if (!request.undo)
      {
        //Validate against existing data edits
        GetEditDataRequest getRequest = GetEditDataRequest.CreateGetEditDataRequest(request.ProjectId ?? -1,
            request.dataEdit.assetId);
        EditDataResult editResult = PostEditDataAcquire(getRequest);
        ValidateNoOverlap(editResult.dataEdits, request.dataEdit);
        //Validate request date range within production data date range
        await ValidateDates(request.ProjectId ?? -1, request.dataEdit);
      }

      return RequestExecutorContainerFactory.Build<EditDataExecutor>(logger, raptorClient, tagProcessor).Process(request);
#else
      // see NOTE above
      throw new ServiceException(HttpStatusCode.BadRequest,
        new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "TRex unsupported request"));
#endif
    }

    /// <summary>
    /// Validates new edit does not overlap any existing edit of the same type for the same machine.
    /// </summary>
    private void ValidateNoOverlap(List<ProductionDataEdit> existingEdits, ProductionDataEdit newEdit)
    {
      if (existingEdits != null && existingEdits.Count > 0)
      {
        var overlapEdits = (from e in existingEdits
                            where
                                ((!string.IsNullOrEmpty(e.onMachineDesignName) &&
                                  !string.IsNullOrEmpty(newEdit.onMachineDesignName)) ||
                                 (e.liftNumber.HasValue && newEdit.liftNumber.HasValue)) &&
                                 e.assetId == newEdit.assetId &&
                                !(e.endUTC <= newEdit.startUTC || e.startUTC >= newEdit.endUTC)
                            select e).ToList();

        if (overlapEdits.Count > 0)
        {
          string message = string.Empty;
          foreach (var oe in overlapEdits)
          {
            message = $"{message}\nMachine: {oe.assetId}, Override Period: {oe.startUTC}-{oe.endUTC}, Edited Value: {(string.IsNullOrEmpty(oe.onMachineDesignName) ? oe.onMachineDesignName : (oe.liftNumber?.ToString() ?? string.Empty))}";
          }
          throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                $"Data edit overlaps: {message}"));
        }
      }
    }

    /// <summary>
    /// Validates new edit is within production data date range for the project
    /// </summary>
    private async Task ValidateDates(long projectId, ProductionDataEdit dataEdit)
    {
#if RAPTOR
      var projectStatisticsHelper = new ProjectStatisticsHelper(logger, configStore, null, null, raptorClient);
      var stats = await projectStatisticsHelper.GetProjectStatisticsWithExclusions(projectId, new long[0]) as ProjectStatisticsResult;
      if (stats == null)
        throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                "Can not validate request - check ReportSvc configuration."));
      if (dataEdit.startUTC < stats.startTime || dataEdit.endUTC > stats.endTime)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                string.Format("Data edit outside production data date range: {0}-{1}", stats.startTime, stats.endTime)));
      }
#else
      throw new ServiceException(HttpStatusCode.BadRequest,
        new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "TRex unsupported request"));
#endif
    }
  }
}
