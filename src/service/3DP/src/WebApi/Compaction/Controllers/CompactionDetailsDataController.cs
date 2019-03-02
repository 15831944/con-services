﻿using System;
using System.Net;
using System.Threading.Tasks;
#if RAPTOR
using ASNodeDecls;
#endif
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Compaction.Executors;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Factories.ProductionData;
using VSS.Productivity3D.WebApi.Models.Report.Executors;
using VSS.Productivity3D.WebApi.Models.Report.Models;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Controller for getting Raptor production data for details requests.
  /// </summary>
  [ProjectVerifier]
  [ResponseCache(Duration = 900, VaryByQueryKeys = new[] { "*" })]
  public class CompactionDetailsDataController : CompactionDataBaseController
  {
    /// <summary>
    /// Default constructor.
    /// </summary>
    public CompactionDetailsDataController(IConfigurationStore configStore, IFileListProxy fileListProxy, ICompactionSettingsManager settingsManager, IProductionDataRequestFactory requestFactory, ITRexCompactionDataProxy trexCompactionDataProxy)
      : base(configStore, fileListProxy, settingsManager, requestFactory, trexCompactionDataProxy)
    { }

    /// <summary>
    /// Get CMV % change from Raptor for the specified project and date range.
    /// </summary>
    [Route("api/v2/cmv/percentchange")]
    [HttpGet]
    public async Task<CompactionCmvPercentChangeResult> GetCmvPercentChange(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid)
    {
      Log.LogInformation("GetCmvPercentChange: " + Request.QueryString);

      var (isValidFilterForProjectExtents, filter) = await ValidateFilterAgainstProjectExtents(projectUid, filterUid);
      if (!isValidFilterForProjectExtents)
      {
        return new CompactionCmvPercentChangeResult();
      }

      var projectSettings = await GetProjectSettingsTargets(projectUid);
      var liftSettings = SettingsManager.CompactionLiftBuildSettings(projectSettings);

      if (filter == null) await GetCompactionFilter(projectUid, filterUid);

      double[] cmvChangeSummarySettings = SettingsManager.CompactionCmvPercentChangeSettings(projectSettings);
      var projectId = await GetLegacyProjectId(projectUid);

      var request = new CMVChangeSummaryRequest(projectId, projectUid, null, liftSettings, filter, -1, cmvChangeSummarySettings);
      request.Validate();
      
      try
      {
        var result = RequestExecutorContainerFactory
          .Build<CMVChangeSummaryExecutor>(LoggerFactory,
#if RAPTOR
            RaptorClient, 
#endif
            configStore: ConfigStore, trexCompactionDataProxy: TRexCompactionDataProxy, customHeaders: CustomHeaders)
          .Process(request) as CMVChangeSummaryResult;
        var returnResult = new CompactionCmvPercentChangeResult(result);
        Log.LogInformation("GetCmvPercentChange result: " + JsonConvert.SerializeObject(returnResult));

        await SetCacheControlPolicy(projectUid);

        return returnResult;
      }
      catch (ServiceException exception)
      {
#if RAPTOR
        var statusCode = (TASNodeErrorStatus)exception.GetResult.Code == TASNodeErrorStatus.asneFailedToRequestDatamodelStatistics ? HttpStatusCode.NoContent : HttpStatusCode.BadRequest;

        throw new ServiceException(statusCode,
          new ContractExecutionResult(exception.GetResult.Code, exception.GetResult.Message));
#else
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, exception.Message));
#endif
      }
      finally
      {
        Log.LogInformation("GetCmvPercentChange returned: " + Response.StatusCode);
      }
    }

    /// <summary>
    /// Get CMV details from Raptor for the specified project and date range. 
    /// </summary>
    [Route("api/v2/cmv/details/targets")]
    [HttpGet]
    [Obsolete("Use 'cmv/details' for v2 and 'compaction/cmv/detailed' for v1 result")]
    public async Task<CompactionCmvDetailedResult> GetCmvDetailsTargets(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid)
    {
      Log.LogInformation("GetCmvDetailsTargets: " + Request.QueryString);

#if !RAPTOR
      throw new ServiceException(HttpStatusCode.BadRequest,
        new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "TRex unsupported request"));
#endif
      CMVRequest request = await GetCmvRequest(projectUid, filterUid);
      request.Validate();

      try
      {
        var result = RequestExecutorContainerFactory
          .Build<DetailedCMVExecutor>(LoggerFactory,
#if RAPTOR
            RaptorClient, 
#endif
            configStore: ConfigStore, trexCompactionDataProxy: TRexCompactionDataProxy, customHeaders: CustomHeaders)
          .Process(request) as CMVDetailedResult;

        var returnResult = new CompactionCmvDetailedResult(result, null);
        Log.LogInformation("GetCmvDetailsTargets result: " + JsonConvert.SerializeObject(returnResult));

        return returnResult;
      }
      catch (ServiceException exception)
      {
#if RAPTOR
        var statusCode = (TASNodeErrorStatus)exception.GetResult.Code == TASNodeErrorStatus.asneFailedToRequestDatamodelStatistics ? HttpStatusCode.NoContent : HttpStatusCode.BadRequest;

        throw new ServiceException(statusCode,
          new ContractExecutionResult(exception.GetResult.Code, exception.GetResult.Message));
#else
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, exception.Message));
#endif
      }
      finally
      {
        Log.LogInformation("GetCmvDetailsTargets returned: " + Response.StatusCode);
      }
    }

    /// <summary>
    /// Get CMV details from Raptor for the specified project and date range.
    /// </summary>
    [Route("api/v2/cmv/details")]
    [HttpGet]
    public async Task<CompactionCmvDetailedResult> GetCmvDetails(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid)
    {
      Log.LogInformation("GetCmvDetails: " + Request.QueryString);

      CMVRequest request = await GetCmvRequest(projectUid, filterUid, true);
      request.Validate();

      try
      {
        var result1 = RequestExecutorContainerFactory
          .Build<DetailedCMVExecutor>(LoggerFactory,
#if RAPTOR
            RaptorClient, 
#endif
            configStore: ConfigStore, trexCompactionDataProxy: TRexCompactionDataProxy, customHeaders:CustomHeaders)
          .Process(request) as CMVDetailedResult;

        if (result1 != null && result1.ConstantTargetCmv == -1)
        {
          var result2 = RequestExecutorContainerFactory
            .Build<SummaryCMVExecutor>(LoggerFactory,
#if RAPTOR
              RaptorClient, 
#endif
              configStore: ConfigStore, trexCompactionDataProxy: TRexCompactionDataProxy, customHeaders: CustomHeaders)
            .Process(request) as CMVSummaryResult;

          if (result2 != null && result2.HasData())
          {
            result1.ConstantTargetCmv = result2.ConstantTargetCmv;
            result1.IsTargetCmvConstant = result2.IsTargetCmvConstant;
          }
        }

        var returnResult = new CompactionCmvDetailedResult(result1, request.CmvSettings);

        Log.LogInformation("GetCmvDetails result: " + JsonConvert.SerializeObject(returnResult));

        return returnResult;
      }
      catch (ServiceException exception)
      {
#if RAPTOR
        var statusCode = (TASNodeErrorStatus)exception.GetResult.Code == TASNodeErrorStatus.asneFailedToRequestDatamodelStatistics ? HttpStatusCode.NoContent : HttpStatusCode.BadRequest;

        throw new ServiceException(statusCode,
          new ContractExecutionResult(exception.GetResult.Code, exception.GetResult.Message));
#else
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, exception.Message));
#endif
      }
      finally
      {
        Log.LogInformation("GetCmvDetails returned: " + Response.StatusCode);
      }
    }

    /// <summary>
    /// Get pass count details from Raptor for the specified project and date range.
    /// </summary>
    [Route("api/v2/passcounts/details")]
    [HttpGet]
    public async Task<CompactionPassCountDetailedResult> GetPassCountDetails(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid)
    {
      Log.LogInformation("GetPassCountDetails: " + Request.QueryString);

      var passCountsRequest = await GetPassCountRequest(projectUid, filterUid, isSummary: false);
      passCountsRequest.Validate();

      try
      {
        var result = RequestExecutorContainerFactory
                     .Build<DetailedPassCountExecutor>(LoggerFactory,
#if RAPTOR
            RaptorClient, 
#endif
            configStore: ConfigStore, trexCompactionDataProxy: TRexCompactionDataProxy, customHeaders: CustomHeaders)
                     .Process(passCountsRequest) as PassCountDetailedResult;

        var returnResult = new CompactionPassCountDetailedResult(result);

        Log.LogInformation("GetPassCountDetails result: " + JsonConvert.SerializeObject(returnResult));

        return returnResult;
      }
      catch (ServiceException exception)
      {
#if RAPTOR
        var statusCode = (TASNodeErrorStatus)exception.GetResult.Code == TASNodeErrorStatus.asneFailedToRequestDatamodelStatistics ? HttpStatusCode.NoContent : HttpStatusCode.BadRequest;

        throw new ServiceException(statusCode,
          new ContractExecutionResult(exception.GetResult.Code, exception.GetResult.Message));
#else
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, exception.Message));
#endif
      }
      finally
      {
        Log.LogInformation("GetPassCountDetails returned: " + Response.StatusCode);
      }
    }

    /// <summary>
    /// Get cut-fill details from Raptor for the specified project and date range.
    /// </summary>
    [Route("api/v2/cutfill/details")]
    [HttpGet]
    public async Task<CompactionCutFillDetailedResult> GetCutFillDetails(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid,
      [FromQuery] Guid cutfillDesignUid)
    {
      Log.LogInformation("GetCutFillDetails: " + Request.QueryString);

      var projectSettings = await GetProjectSettingsTargets(projectUid);
      var cutFillDesign = await GetAndValidateDesignDescriptor(projectUid, cutfillDesignUid);
      var filter = await GetCompactionFilter(projectUid, filterUid);
      var projectId = await GetLegacyProjectId(projectUid);

      var cutFillRequest = RequestFactory.Create<CutFillRequestHelper>(r => r
          .ProjectUid(projectUid)
          .ProjectId(projectId)
          .Headers(CustomHeaders)
          .ProjectSettings(projectSettings)
          .Filter(filter)
          .DesignDescriptor(cutFillDesign))
        .Create();

      cutFillRequest.Validate();

      return WithServiceExceptionTryExecute(() =>
        RequestExecutorContainerFactory
          .Build<CompactionCutFillExecutor>(LoggerFactory,
#if RAPTOR
            RaptorClient, 
#endif
            configStore: ConfigStore, trexCompactionDataProxy: TRexCompactionDataProxy, customHeaders: CustomHeaders)
          .Process(cutFillRequest) as CompactionCutFillDetailedResult);
    }

    /// <summary>
    /// Get temperature details from Raptor for the specified project and date range.
    /// </summary>
    [Route("api/v2/temperature/details")]
    [HttpGet]
    public async Task<CompactionTemperatureDetailResult> GetTemperatureDetails(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid)
    {
      Log.LogInformation("GetTemperatureDetails: " + Request.QueryString);

      var projectSettings = await GetProjectSettingsTargets(projectUid);
      var filter = await GetCompactionFilter(projectUid, filterUid);
      var projectId = await GetLegacyProjectId(projectUid);
      var temperatureSettings = SettingsManager.CompactionTemperatureSettings(projectSettings, false);
      var liftSettings = SettingsManager.CompactionLiftBuildSettings(projectSettings);

      var detailsRequest = RequestFactory.Create<TemperatureRequestHelper>(r => r
          .ProjectUid(projectUid)
          .ProjectId(projectId)
          .Headers(CustomHeaders)
          .ProjectSettings(projectSettings)
          .Filter(filter))
        .CreateTemperatureDetailsRequest();

      detailsRequest.Validate();

      var summaryRequest = new TemperatureRequest(projectId, projectUid, null,
        temperatureSettings, liftSettings, filter, -1, null, null, null);
      summaryRequest.Validate();

      try
      {
        var result1 = WithServiceExceptionTryExecute(() =>
          RequestExecutorContainerFactory
            .Build<DetailedTemperatureExecutor>(LoggerFactory,
#if RAPTOR
              RaptorClient, 
#endif
              configStore: ConfigStore, trexCompactionDataProxy: TRexCompactionDataProxy, customHeaders: CustomHeaders)
            .Process(detailsRequest) as CompactionTemperatureDetailResult);

        //When TRex done for temperature details, assume it will set target in details call
        if (result1 != null && result1.TemperatureTarget == null)
        {
          var result2 = RequestExecutorContainerFactory
            .Build<SummaryTemperatureExecutor>(LoggerFactory,
#if RAPTOR
              RaptorClient, 
#endif
              configStore: ConfigStore,
              trexCompactionDataProxy: TRexCompactionDataProxy, customHeaders: CustomHeaders)
            .Process(summaryRequest) as TemperatureSummaryResult;
          result1.SetTargets(result2?.TargetData);
        }

        return result1;
      }
      catch (ServiceException exception)
      {
        Log.LogError($"{nameof(GetTemperatureDetails)}: {exception.GetResult.Message} ({exception.GetResult.Code})");
#if RAPTOR
        var statusCode = (TASNodeErrorStatus)exception.GetResult.Code == TASNodeErrorStatus.asneFailedToRequestDatamodelStatistics ? HttpStatusCode.NoContent : HttpStatusCode.BadRequest;

        throw new ServiceException(statusCode,
          new ContractExecutionResult(exception.GetResult.Code, exception.GetResult.Message));
#else
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, exception.Message));
#endif
      }
      finally
      {
        Log.LogInformation("GetTemperatureDetails returned: " + Response.StatusCode);
      }
    }
  }
}
