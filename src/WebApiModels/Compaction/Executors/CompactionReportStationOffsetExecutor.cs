﻿using ASNodeDecls;
using ASNodeRaptorReports;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Filters.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Executors
{
  /// <summary>
  /// The executor, which passes the report grid request to Raptor.
  /// </summary>
  public class CompactionReportStationOffsetExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Processes the report grid request by passing the request to Raptor and returning the result.
    /// </summary>
    /// <returns>Returns an instance of the <see cref="CompactionReportResult"/> class if successful.</returns>
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      log.LogDebug($"Start CompactionReportStationOffsetExecutor: {JsonConvert.SerializeObject(item)}");

      ContractExecutionResult result;

      try
      {
        var request = item as CompactionReportStationOffsetRequest;
        if (request == null)
        {
          throw new ServiceException(HttpStatusCode.InternalServerError,
            new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
              "Request item is not compatible with Station Offset request."));
        }

        log.LogDebug("About to convert filter");
        var filterSettings = RaptorConverters.ConvertFilter(request.FilterID, request.Filter, request.projectId);
        log.LogDebug("About to convert cut-fill design");
        var cutfillDesignDescriptor = RaptorConverters.DesignDescriptor(request.DesignFile);
        log.LogDebug("About to convet alignment file");
        var alignmentDescriptor = RaptorConverters.DesignDescriptor(request.AlignmentFile);
        log.LogDebug("About to convert user preferences");
        var userPreferences = ExportRequestHelper.ConvertUserPreferences(request.UserPreferences);

        log.LogDebug("About to call GetReportStationOffset");

        var args = ASNode.StationOffsetReport.RPC.__Global.Construct_StationOffsetReport_Args(
          request.projectId ?? -1,
          (int)CompactionReportType.StationOffset,
          ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor(Guid.NewGuid(), 0, TASNodeCancellationDescriptorType.cdtProdDataReport),
          userPreferences,
          alignmentDescriptor,
          cutfillDesignDescriptor,
          request.StartStation,
          request.EndStation,
          request.Offsets,
          request.CrossSectionInterval,
          request.ReportElevation,
          request.ReportCutFill,
          request.ReportCMV,
          request.ReportMDP,
          request.ReportPassCount,
          request.ReportTemperature,
          (int)GridReportOption.Unused,
          0, 0, 0, 0, 0, 0, 0, // Northings, Eastings and Direction values are not used on Station Offset report.
          filterSettings,
          RaptorConverters.ConvertLift(request.LiftBuildSettings, filterSettings.LayerMethod),
          new SVOICOptionsDecls.TSVOICOptions() // ICOptions, need to resolve what this should be
        );

        int returnedResult = raptorClient.GetReportStationOffset(args, out var responseData);

        log.LogDebug("Completed call to GetReportStationOffset");

        var success = 1;

        if (returnedResult != success)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
              "Failed to get requested station offset report data"));
        }

        try
        {
          // Unpack the data for the report and construct a stream containing the result
          TRaptorReportsPackager reportPackager = new TRaptorReportsPackager(TRaptorReportType.rrtStationOffset)
          {
            ReturnCode = TRaptorReportReturnCode.rrrcUnknownError
          };

          log.LogDebug("Retrieving response data");

          reportPackager.ReadFromStream(responseData);

          var stationRows = new StationRow[reportPackager.StationOffsetReport.NumberOfStations];

          for (var i = 0; i < reportPackager.StationOffsetReport.NumberOfStations; i++)
          {
            var station = reportPackager.StationOffsetReport.Stations[i];
            var stationRow = StationRow.Create(station, request);

            for (var j = 0; j < station.NumberOfOffsets; j++)
            {
              var stationOffsetRow = StationOffsetRow.CreateRow(station.Offsets[j]);

              stationOffsetRow.SetReportFlags(request);
              stationRow.Offsets[j] = stationOffsetRow;
            }

            stationRow.SetStatisticsReportFlags();

            stationRows[i] = stationRow;
          }

          var startAndEndTime = request.Filter.StartUtc ?? DateTime.Now;
          var stationOffsetReport = StationOffsetReport.CreateReport(startAndEndTime, startAndEndTime, stationRows, request);

          result = CompactionReportResult.CreateExportDataResult(stationOffsetReport, (short)returnedResult);
        }
        catch (Exception ex)
        {
          throw new ServiceException(HttpStatusCode.NoContent,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              "Failed to retrieve received station offset report data: " + ex.Message));
        }
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }

      return result;
    }
  }
}