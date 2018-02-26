﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Extensions;
using VSS.Productivity3D.Common.Filters.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.WebApi.Models.Compaction.Executors;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Factories.ProductionData;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Controller for getting Raptor production data for report requests
  /// </summary>
  [ResponseCache(Duration = 900, VaryByQueryKeys = new[] { "*" })]
  public class CompactionReportController : BaseController
  {
    /// <summary>
    /// Raptor client for use by executor
    /// </summary>
    private readonly IASNodeClient raptorClient;

    /// <summary>
    /// Logger for logging
    /// </summary>
    private readonly ILogger log;

    /// <summary>
    /// Logger factory for use by executor
    /// </summary>
    private readonly ILoggerFactory logger;

    /// <summary>
    /// The request factory
    /// </summary>
    private readonly IProductionDataRequestFactory requestFactory;

    /// <summary>
    /// For retrieving user preferences
    /// </summary>
    private readonly IPreferenceProxy prefProxy;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="raptorClient">The raptor client</param>
    /// <param name="logger">The logger.</param>
    /// <param name="exceptionHandler">The exception handler.</param>
    /// <param name="configStore">Configuration store.</param>
    /// <param name="fileListProxy">The file list proxy.</param>
    /// <param name="projectSettingsProxy">The project settings proxy.</param>
    /// <param name="filterServiceProxy">The filter service proxy.</param>
    /// <param name="settingsManager">The compaction settings manager.</param>
    /// <param name="requestFactory">The request factory.</param>
    /// <param name="prefProxy">The User Preferences proxy.</param>
    public CompactionReportController(IASNodeClient raptorClient, ILoggerFactory logger, IServiceExceptionHandler exceptionHandler, IConfigurationStore configStore,
      IFileListProxy fileListProxy, IProjectSettingsProxy projectSettingsProxy, IFilterServiceProxy filterServiceProxy, ICompactionSettingsManager settingsManager,
      IProductionDataRequestFactory requestFactory, IPreferenceProxy prefProxy) :
      base(logger.CreateLogger<BaseController>(), exceptionHandler, configStore, fileListProxy, projectSettingsProxy, filterServiceProxy, settingsManager)
    {
      this.raptorClient = raptorClient;
      this.logger = logger;
      log = logger.CreateLogger<CompactionReportController>();
      this.requestFactory = requestFactory;
      this.prefProxy = prefProxy;
    }

    /// <summary>
    /// Returns a Grid Report for the Project constrained by the input parameters.
    /// </summary>
    /// <param name="projectUid">The project unique identifier.</param>
    /// <param name="filterUid">The filter UID to apply to the report results</param>
    /// <param name="reportElevation">Exclude/include Elevation data in the report.</param>
    /// <param name="reportCmv">Exclude/include CMV data in the report.</param>
    /// <param name="reportMdp">Exclude/include MDP data in the report.</param>
    /// <param name="reportPassCount">Exclude/include Pass Count data in the report.</param>
    /// <param name="reportTemperature">Exclude/include Temperature data in the report.</param>
    /// <param name="reportCutFill">Exclude/include Cut/Fill data in the report.</param>
    /// <param name="cutfillDesignUid">The cut/fill design file unique identifier if Cut/Fill data is included in the report.</param>
    /// <param name="gridInterval">The grid spacing interval for the sampled points.</param>
    /// <param name="gridReportOption">Grid report option. Whether it is defined automatically or by user specified parameters.</param>
    /// <param name="startNorthing">The Northing ordinate of the location to start gridding from.</param>
    /// <param name="startEasting">The Easting ordinate of the location to start gridding from.</param>
    /// <param name="endNorthing">The Northing ordinate of the location to end gridding at.</param>
    /// <param name="endEasting">The Easting ordinate of the location to end gridding at.</param>
    /// <param name="azimuth">The orientation of the grid, expressed in radians</param>
    /// <returns>An instance of the <see cref="ContractExecutionResult"/> class.</returns>
    [Route("api/v2/report/grid")]
    [HttpGet]
    public async Task<CompactionReportResult> GetReportGrid(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid,
      [FromQuery] bool reportElevation,
      [FromQuery] bool reportCmv,
      [FromQuery] bool reportMdp,
      [FromQuery] bool reportPassCount,
      [FromQuery] bool reportTemperature,
      [FromQuery] bool reportCutFill,
      [FromQuery] Guid? cutfillDesignUid,
      [FromQuery] double? gridInterval,
      [FromQuery] GridReportOption gridReportOption,
      [FromQuery] double startNorthing,
      [FromQuery] double startEasting,
      [FromQuery] double endNorthing,
      [FromQuery] double endEasting,
      [FromQuery] double azimuth)
    {
      log.LogInformation("GetReportGrid: " + Request.QueryString);

      var projectId = GetProjectId(projectUid);
      var filter = await GetCompactionFilter(projectUid, filterUid);
      var cutFillDesign = await GetAndValidateDesignDescriptor(projectUid, cutfillDesignUid, true);
      var projectSettings = await GetProjectSettingsTargets(projectUid);

      var reportGridRequest = await requestFactory.Create<CompactionReportGridRequestHelper>(r => r
          .ProjectId(projectId)
          .Headers(CustomHeaders)
          .ProjectSettings(projectSettings)
          .Filter(filter))
        .SetRaptorClient(raptorClient)
        .CreateCompactionReportGridRequest(
          reportElevation,
          reportCmv,
          reportMdp,
          reportPassCount,
          reportTemperature,
          reportCutFill,
          cutFillDesign,
          gridInterval,
          gridReportOption,
          startNorthing,
          startEasting,
          endNorthing,
          endEasting,
          azimuth
        );

      reportGridRequest.Validate();

      return WithServiceExceptionTryExecute(() =>
        RequestExecutorContainerFactory
          .Build<CompactionReportGridExecutor>(logger, raptorClient, null, ConfigStore)
          .Process(reportGridRequest) as CompactionReportResult
      );
    }

    /// <summary>
    /// Get station offset report data for the given filter and design files.
    /// </summary>
    /// <returns>Returns the station offset report results as JSON.</returns>
    [Route("api/v2/report/stationoffset")]
    [HttpGet]
    public async Task<CompactionReportResult> GetStationOffsetReport(
      [FromQuery] Guid projectUid,
      [FromQuery] bool reportElevation,
      [FromQuery] bool reportCmv,
      [FromQuery] bool reportMdp,
      [FromQuery] bool reportPassCount,
      [FromQuery] bool reportTemperature,
      [FromQuery] bool reportCutFill,
      [FromQuery] Guid filterUid,
      [FromQuery] Guid? cutfillDesignUid,
      [FromQuery] Guid? alignmentUid,
      [FromQuery] double crossSectionInterval,
      [FromQuery] double startStation,
      [FromQuery] double endStation,
      [FromQuery] double[] offsets)
    {
      log.LogInformation("GetStationOffset: " + Request.QueryString);

      var projectId = GetProjectId(projectUid);
      var filter = await GetCompactionFilter(projectUid, filterUid);
      var cutFillDesignDescriptor = await GetAndValidateDesignDescriptor(projectUid, cutfillDesignUid);
      var alignmentDescriptor = await GetAndValidateDesignDescriptor(projectUid, alignmentUid);
      var projectSettings = await GetProjectSettingsTargets(projectUid);
      var userPreferences = await GetUserPreferences();

      // Add 0.0 value to the offsets array, remove any duplicates and sort contents by ascending order...
      log.LogDebug("About to sort offsets");
      var updatedOffsets = offsets?.AddZeroDistinctSortBy();

      log.LogDebug("Creating request");
      var reportRequest = requestFactory.Create<CompactionReportStationOffsetRequestHelper>(r => r
        .ProjectId(projectId)
        .Headers(CustomHeaders)
        .ProjectSettings(projectSettings)
        .Filter(filter))
      .CreateRequest(
        reportElevation,
        reportCmv,
        reportMdp,
        reportPassCount,
        reportTemperature,
        reportCutFill,
        cutFillDesignDescriptor,
        alignmentDescriptor,
        crossSectionInterval,
        startStation,
        endStation,
        updatedOffsets,
        userPreferences);

      log.LogDebug("Validating request");
      reportRequest.Validate();

      return WithServiceExceptionTryExecute(() =>
        RequestExecutorContainerFactory
          .Build<CompactionReportStationOffsetExecutor>(logger, raptorClient, null, ConfigStore)
          .Process(reportRequest) as CompactionReportResult
      );
    }

    private async Task<UserPreferenceData> GetUserPreferences()
    {
      var userPreferences = await prefProxy.GetUserPreferences(this.CustomHeaders);
      if (userPreferences == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
            "Failed to retrieve preferences for current user"));
      }
      return userPreferences;
    }
  }
}