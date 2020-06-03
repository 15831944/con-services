﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.Http;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Scheduler.Abstractions;
using VSS.Productivity3D.WebApi.Models.Compaction.Executors;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Productivity3D.WebApi.Models.Factories.ProductionData;
using VSS.Productivity3D.WebApi.Models.Report.Models;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Controller for getting Raptor production data for summary and details requests
  /// </summary>
  //NOTE: do not cache responses as large amount of data
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
  [ProjectVerifier]
  public class CompactionExportController : BaseController<CompactionExportController>
  {
    private readonly IPreferenceProxy prefProxy;
    private readonly IProductionDataRequestFactory requestFactory;
    private const int FIVE_MIN_SCHEDULER_TIMEOUT = 300000;

    private readonly ITransferProxy transferProxy;

    /// <summary>
    /// 
    /// Default constructor.
    /// </summary>
    public CompactionExportController(
      IConfigurationStore configStore, IFileImportProxy fileImportProxy, ICompactionSettingsManager settingsManager,
      IProductionDataRequestFactory requestFactory, IPreferenceProxy prefProxy,
      ITransferProxy transferProxy) :
      base(configStore, fileImportProxy, settingsManager)
    {
      this.prefProxy = prefProxy;
      this.requestFactory = requestFactory;
      this.transferProxy = transferProxy;
    }

    #region Schedule Exports

    /// <summary>
    /// Schedules the veta export job and returns JobId.
    /// </summary>
    [Route("api/v2/export/veta/schedulejob")]
    [HttpGet]
    public ScheduleResult ScheduleVetaJob(
      [FromServices] ISchedulerProxy scheduler,
      [FromQuery] Guid projectUid,
      [FromQuery] string fileName,
      [FromQuery] string machineNames,
      [FromQuery] Guid? filterUid,
      [FromQuery] CoordType coordType = CoordType.Northeast)
    {
      //TODO: Do we need to validate the parameters here as well as when the export url is called?

      //The URL to get the export data is here in this controller, construct it based on this request
      var exportDataUrl =
        $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/api/v2/export/veta?projectUid={projectUid}&fileName={fileName}&coordType={coordType}";

      if (filterUid.HasValue)
      {
        exportDataUrl = $"{exportDataUrl}&filterUid={filterUid}";
      }

      if (!string.IsNullOrEmpty(machineNames))
      {
        exportDataUrl = $"{exportDataUrl}&machineNames={machineNames}";
      }

      return ScheduleJob(exportDataUrl, fileName, scheduler);
    }



    /// <summary>
    /// Schedules the snakepit export job and returns JobId.
    /// </summary>
    [Route("api/v2/export/snakepit/schedulejob")]
    [HttpGet]
    public ScheduleResult ScheduleSnakepitJob(
      [FromServices] ISchedulerProxy scheduler,
      [FromQuery] Guid projectUid,
      [FromQuery] string fileName
    )
    {
      //The URL to get the export data is in snakepit construct url from configuration
      var snakepitHost = ConfigStore.GetValueString("SNAKEPIT_HOST", null);
      if (!string.IsNullOrEmpty(snakepitHost))
      {
        var exportDataUrl =
          $"{HttpContext.Request.Scheme}://{snakepitHost}/export{HttpContext.Request.QueryString.ToString()}";

        return ScheduleJob(exportDataUrl, fileName, scheduler, 3 * FIVE_MIN_SCHEDULER_TIMEOUT);
      }

      throw new ServiceException(HttpStatusCode.InternalServerError,
        new ContractExecutionResult(
          ContractExecutionStatesEnum.InternalProcessingError,
          "Missing SNAKEPIT_HOST environment variable"
        )
      );
    }

    /// <summary>
    /// Schedules the Machine passes export job and returns JobId.
    /// </summary>
    [Route("api/v2/export/machinepasses/schedulejob")]
    [HttpGet]
    public ScheduleResult ScheduleMachinePassesJob(
      [FromQuery] Guid projectUid,
      [FromQuery] int coordType,
      [FromQuery] int outputType,
      [FromQuery] bool restrictOutput,
      [FromQuery] bool rawDataOutput,
      [FromQuery] string fileName,
      [FromQuery] Guid? filterUid,
      [FromServices] ISchedulerProxy scheduler)
    {
      //TODO: Do we need to validate the parameters here as well as when the export url is called?

      //The URL to get the export data is here in this controller, construct it based on this request
      var exportDataUrl =
        $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/api/v2/export/machinepasses?projectUid={projectUid}&fileName={fileName}&filterUid={filterUid}" +
        $"&coordType={coordType}&outputType={outputType}&restrictOutput={restrictOutput}&rawDataOutput={rawDataOutput}";
      return ScheduleJob(exportDataUrl, fileName, scheduler);
    }

    /// <summary>
    /// Schedules the surface export job and returns JobId.
    /// </summary>
    [Route("api/v2/export/surface/schedulejob")]
    [HttpGet]
    public ScheduleResult ScheduleSurfaceJob(
      [FromQuery] Guid projectUid,
      [FromQuery] string fileName,
      [FromQuery] double? tolerance,
      [FromQuery] Guid? filterUid,
      [FromServices] ISchedulerProxy scheduler)
    {
      //TODO: Do we need to validate the parameters here as well as when the export url is called?

      var exportDataUrl =
        $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/api/v2/export/surface?projectUid={projectUid}&fileName={fileName}&filterUid={filterUid}&tolerance={tolerance}";
      return ScheduleJob(exportDataUrl, fileName, scheduler);
    }

    /// <summary>
    /// Schedule an export job wit the scheduler
    /// </summary>
    private ScheduleResult ScheduleJob(string exportDataUrl, string fileName, ISchedulerProxy scheduler,
      int? timeout = null)
    {
      if (timeout == null)
      {
        var configStoreTimeout = ConfigStore.GetValueInt("SCHEDULED_JOB_TIMEOUT");
        timeout = configStoreTimeout > 0 ? configStoreTimeout : FIVE_MIN_SCHEDULER_TIMEOUT;
      }

      var request = new ScheduleJobRequest { Url = exportDataUrl, Filename = fileName, Timeout = timeout };

      return WithServiceExceptionTryExecute(() => new ScheduleResult
      {
        JobId = scheduler.ScheduleExportJob(request, Request.Headers.GetCustomHeaders()).Result?.JobId
      });
    }

    #endregion

    #region Exports

    /// <summary>
    /// Gets an export of production data in cell grid format report for import to VETA.
    /// </summary>
    [HttpGet("api/v2/export/veta")]
    public async Task<FileResult> GetExportReportVeta(
      [FromQuery] Guid projectUid,
      [FromQuery] string fileName,
      [FromQuery] string machineNames,
      [FromQuery] Guid? filterUid,
      [FromQuery] CoordType coordType = CoordType.Northeast)
    {
      Log.LogInformation($"{nameof(GetExportReportVeta)}: {Request.QueryString}");

      var projectTask = ((RaptorPrincipal)User).GetProject(projectUid);
      var projectSettings = GetProjectSettingsTargets(projectUid);
      var filterTask = GetCompactionFilter(projectUid, filterUid);
      var userPreferences = GetUserPreferences();

      await Task.WhenAll(projectTask, projectSettings, filterTask, userPreferences);

      var project = projectTask.Result;
      var filter = filterTask.Result;

      var startEndDate = await GetDateRange(project.ShortRaptorProjectId, filter);

      var exportRequest = requestFactory.Create<ExportRequestHelper>(r => r
          .ProjectUid(projectUid)
          .ProjectId(project.ShortRaptorProjectId)
          .Headers(CustomHeaders)
          .ProjectSettings(projectSettings.Result)
          .Filter(filter))
#if RAPTOR
        .SetRaptorClient(RaptorClient)
#endif
        .SetUserPreferences(userPreferences.Result)
        .SetProjectDescriptor(project)
        .CreateExportRequest(
          startEndDate.startUtc,
          startEndDate.endUtc,
          coordType,
          ExportTypes.VedaExport,
          fileName,
          false,
          true,
          OutputTypes.VedaAllPasses,
          machineNames);

      exportRequest.Validate();

      var result = await WithServiceExceptionTryExecuteAsync(() => RequestExecutorContainerFactory
          .Build<CompactionExportExecutor>(LoggerFactory,
#if RAPTOR
            RaptorClient,
#endif
            configStore: ConfigStore,
            trexCompactionDataProxy: TRexCompactionDataProxy,
            customHeaders: CustomHeaders)
          .ProcessAsync(exportRequest)) as CompactionExportResult;

#if RAPTOR
      var fileStream = new FileStream(result.FullFileName, FileMode.Open);
#else
      var fileStream =
 (await transferProxy.DownloadFromBucket(result.FullFileName, ConfigStore.GetValueString("AWS_TEMPORARY_BUCKET_NAME"))).FileStream;
#endif

      Log.LogInformation($"GetExportReportVeta completed: ExportData size={fileStream.Length}");
      return new FileStreamResult(fileStream, ContentTypeConstants.ApplicationZip);
    }

    /// <summary>
    /// Gets an export of production data in cell grid format report.
    /// </summary>
    /// <param name="projectUid">Project unique identifier.</param>
    /// <param name="coordType">Either Northing/Easting or Latitude/Longitude.</param>
    /// <param name="outputType">Either all passes/last for pass machine passes export or all passes/final pass for export for VETA</param>
    /// <param name="restrictOutput">Output .CSV file is restricted to 65535 rows if it is true.</param>
    /// <param name="rawDataOutput">Column headers in an output .CSV file's are in the dBase format.</param>
    /// <param name="fileName">Output file name.</param>
    /// <param name="filterUid">The filter uid to apply to the export results</param>
    [Route("api/v2/export/machinepasses")]
    [HttpGet]
    public async Task<FileResult> GetExportReportMachinePasses(
      [FromQuery] Guid projectUid,
      [FromQuery] int coordType,
      [FromQuery] int outputType,
      [FromQuery] bool restrictOutput,
      [FromQuery] bool rawDataOutput,
      [FromQuery] string fileName,
      [FromQuery] Guid? filterUid)
    {
      Log.LogInformation($"{nameof(GetExportReportMachinePasses)}: {Request.QueryString}");

      var projectTask = ((RaptorPrincipal)User).GetProject(projectUid);
      var projectSettings = GetProjectSettingsTargets(projectUid);
      var filterTask = GetCompactionFilter(projectUid, filterUid);

      var userPreferences = Task.FromResult(UserPreferenceData.GetDefaultSIPreferences());
// For all of application tokens we would use SI instead of the defaul Imperial generated by VSS. This will be changed with moving to CEC profile service
      if (!((RaptorPrincipal) User).IsApplication)
        userPreferences = GetUserPreferences();


      await Task.WhenAll(projectTask, projectSettings, filterTask, userPreferences);
      var project = projectTask.Result;
      var filter = filterTask.Result;

      var startEndDate = await GetDateRange(project.ShortRaptorProjectId, filter);

      var exportRequest = requestFactory.Create<ExportRequestHelper>(r => r
          .ProjectUid(projectUid)
          .ProjectId(project.ShortRaptorProjectId)
          .Headers(CustomHeaders)
          .ProjectSettings(projectSettings.Result)
          .Filter(filter))
        .SetUserPreferences(userPreferences.Result)
#if RAPTOR
        .SetRaptorClient(RaptorClient)
#endif
        .SetProjectDescriptor(project)
        .CreateExportRequest(
          startEndDate.startUtc,
          startEndDate.endUtc,
          (CoordType)coordType,
          ExportTypes.PassCountExport,
          fileName,
          restrictOutput,
          rawDataOutput,
          (OutputTypes)outputType,
          string.Empty);

      exportRequest.Validate();

      var result = await WithServiceExceptionTryExecuteAsync(() => RequestExecutorContainerFactory.Build<CompactionExportExecutor>(LoggerFactory,
#if RAPTOR
            RaptorClient,
#endif
            configStore: ConfigStore,
            trexCompactionDataProxy: TRexCompactionDataProxy,
            customHeaders: CustomHeaders)
          .ProcessAsync(exportRequest)) as CompactionExportResult;


#if RAPTOR
      var fileStream = new FileStream(result.FullFileName, FileMode.Open);
#else

      // TRex stores the exported file on s3 at: AWS_TEMPORARY_BUCKET_NAME e.g. vss-exports-stg/prod
      //           this bucket is more temporary than other buckets (designs and tagFiles)
      //
      // the response fullFileName is in format: "project/{projectUId}/TRexExport/{request.FileName}__{uniqueTRexUid}.zip",
      //                                    e.g. "project/f13f2458-6666-424f-a995-4426a00771ae/TRexExport/blahDeBlahAmy__70b0f407-67a8-42f6-b0ef-1fa1d36fc71c.zip"
      var fileStream =
 (await transferProxy.DownloadFromBucket(result.FullFileName, ConfigStore.GetValueString("AWS_TEMPORARY_BUCKET_NAME"))).FileStream;
#endif

      Log.LogInformation($"GetExportReportMachinePasses completed: ExportData size={fileStream.Length}");
      return new FileStreamResult(fileStream, ContentTypeConstants.ApplicationZip);
    }

    /// <summary>
    /// Gets an export of 3D project data in .TTM file format report.
    /// </summary>
    /// <param name="projectUid">Project unique identifier.</param>
    /// <param name="fileName">Output file name.</param>
    /// <param name="tolerance">Controls triangulation density in the output .TTM file.</param>
    /// <param name="filterUid">The filter uid to apply to the export results</param>
    [Route("api/v2/export/surface")]
    [HttpGet]
    public async Task<FileResult> GetExportReportSurface(
      [FromQuery] Guid projectUid,
      [FromQuery] string fileName,
      [FromQuery] double? tolerance,
      [FromQuery] Guid? filterUid)
    {
      const double surfaceExportTolerance = 0.05;

      Log.LogInformation("GetExportReportSurface: " + Request.QueryString);

      var project = ((RaptorPrincipal)User).GetProject(projectUid);
      var projectSettings = GetProjectSettingsTargets(projectUid);
      var filter = GetCompactionFilter(projectUid, filterUid);
      var userPreferences = GetUserPreferences();

      await Task.WhenAll(project, projectSettings, filter, userPreferences);

      tolerance = tolerance ?? surfaceExportTolerance;

      var exportRequest = requestFactory.Create<ExportRequestHelper>(r => r
          .ProjectUid(projectUid)
          .ProjectId(project.Result.ShortRaptorProjectId)
          .Headers(CustomHeaders)
          .ProjectSettings(projectSettings.Result)
          .Filter(filter.Result))
        .SetUserPreferences(userPreferences.Result)
#if RAPTOR
        .SetRaptorClient(RaptorClient)
#endif
        .SetProjectDescriptor(project.Result)
        .CreateExportRequest(
          null, //startUtc,
          null, //endUtc,
          CoordType.Northeast,
          ExportTypes.SurfaceExport,
          fileName,
          false,
          false,
          OutputTypes.VedaAllPasses,
          string.Empty,
          tolerance.Value);

      exportRequest.Validate();

      var result = await WithServiceExceptionTryExecuteAsync(() => RequestExecutorContainerFactory.Build<CompactionExportExecutor>(LoggerFactory,
#if RAPTOR
            RaptorClient,
#endif
            configStore: ConfigStore,
            trexCompactionDataProxy: TRexCompactionDataProxy, customHeaders: CustomHeaders)
          .ProcessAsync(exportRequest)) as CompactionExportResult;

      var fileStream = new FileStream(result.FullFileName, FileMode.Open);

      Log.LogInformation($"GetExportReportSurface completed: ExportData size={fileStream.Length}");
      return new FileStreamResult(fileStream, ContentTypeConstants.ApplicationZip);
    }

    #endregion

    /// <summary>
    /// Get user preferences
    /// </summary>
    private async Task<UserPreferenceData> GetUserPreferences()
    {
      var userPreferences = await prefProxy.GetUserPreferences(CustomHeaders);
      if (userPreferences == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
            "Failed to retrieve preferences for current user"));
      }

      return userPreferences;
    }

    /// <summary>
    /// Gets the date range for the export.
    /// </summary>
    private async Task<(DateTime startUtc, DateTime endUtc)> GetDateRange(long projectId, FilterResult filter)
    {
#if RAPTOR
      if (filter?.StartUtc == null || !filter.EndUtc.HasValue)
      {
        //Special case of project extents where start and end UTC not set in filter for Raptor performance.
        //But need to set here for export.
        var projectUid = await ((RaptorPrincipal)User).GetProjectUid(projectId);
        var result = await ProjectStatisticsHelper.GetProjectStatisticsWithFilterSsExclusions(projectUid, projectId,
          filter?.SurveyedSurfaceExclusionList, filter?.ExcludedSurveyedSurfaceUids);

        var startUtc = filter?.StartUtc ?? result.startTime;
        var endUtc = filter?.EndUtc ?? result.endTime;
        return (startUtc, endUtc);
      }

      return (filter.StartUtc.Value, filter.EndUtc.Value);
#else
// TRex determines this date range within the export API call
        return (DateTime.MinValue, DateTime.MinValue);
#endif
    }
  }
}
