﻿using System;
using System.IO;
using System.Threading.Tasks;
#if RAPTOR
using ASNodeDecls;
using ASNodeRaptorReports;
#endif
using Microsoft.Extensions.Logging;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.Productivity3D.WebApi.Models.Compaction.AutoMapper;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Report.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Executors
{
  /// <summary>
  /// Processes the request to get station-offset details.
  /// </summary>
  public class CompactionReportStationOffsetExecutor : RequestExecutorContainer
  {
    public CompactionReportStationOffsetExecutor()
    {
      ProcessErrorCodes();
    }

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      try
      {
        var request = item as CompactionReportStationOffsetRequest;

        if (request == null)
          ThrowRequestTypeCastException<CompactionReportStationOffsetRequest>();
#if RAPTOR
        if (configStore.GetValueBool("ENABLE_TREX_GATEWAY_STATIONOFFSET") ?? false)
        {
#endif
          var responseData = await trexCompactionDataProxy.SendDataPostRequestWithStreamResponse(AutoMapperUtility.Automapper.Map<CompactionReportStationOffsetTRexRequest>(request), "/report/stationoffset", customHeaders);

          return responseData.Length > 0
            ? ConvertTRexStationOffsetResult(request, responseData)
            : CreateNullStationOffsetReturnedResult();
#if RAPTOR
        }

        return ProcessWithRaptor(request);
#endif
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
    }

    private ContractExecutionResult CreateNullStationOffsetReturnedResult()
    {
      return new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
        "Null stationOffset stream returned");
    }

    private CompactionReportResult ConvertTRexStationOffsetResult(CompactionReportStationOffsetRequest request, Stream stream)
    {
      log.LogDebug($"{nameof(ConvertTRexStationOffsetResult)}: Retrieving response data from TRex");

      var stationOffsetReportResult = new StationOffsetReportResultPackager(ReportType.StationOffset);
      stationOffsetReportResult.Read((stream as MemoryStream)?.ToArray());

      var stationRows = new StationRow[stationOffsetReportResult.GriddedData.NumberOfRows];

      for (var i = 0; i < stationOffsetReportResult.GriddedData.NumberOfRows; i++)
      {
        var station = stationOffsetReportResult.GriddedData.Rows[i];
        var stationRow = StationRow.Create(station, request);

        for (var j = 0; j < station.Offsets.Count; j++)
          stationRow.Offsets[j] = StationOffsetRow.CreateRow(station.Offsets[j], request);

        stationRows[i] = stationRow;
      }

      var startAndEndTime = request.Filter.StartUtc ?? DateTime.UtcNow;
      var stationOffsetReport = new StationOffsetReport(startAndEndTime, startAndEndTime, stationRows, request);

      return CompactionReportResult.CreateExportDataResult(stationOffsetReport, 1);
    }

#if RAPTOR
    private ContractExecutionResult ProcessWithRaptor(CompactionReportStationOffsetRequest request)
    {
      var filterSettings = RaptorConverters.ConvertFilter(request.Filter, request.ProjectId, raptorClient);
      var cutfillDesignDescriptor = RaptorConverters.DesignDescriptor(request.DesignFile);
      var alignmentDescriptor = RaptorConverters.DesignDescriptor(request.AlignmentFile);
      var userPreferences =
        ExportRequestHelper.ConvertToRaptorUserPreferences(request.UserPreferences, request.ProjectTimezone);

      var options = RaptorConverters.convertOptions(null, request.LiftBuildSettings, 0,
        request.Filter?.LayerType ?? FilterLayerMethod.None, DisplayMode.Height, false);

      log.LogDebug($"{nameof(ProcessWithRaptor)}: About to call GetReportStationOffset");

      var args = ASNode.StationOffsetReport.RPC.__Global.Construct_StationOffsetReport_Args(
        request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID,
        (int)CompactionReportType.StationOffset,
        ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor(Guid.NewGuid(), 0,
          TASNodeCancellationDescriptorType.cdtProdDataReport),
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
        options
      );

      int raptorResult = raptorClient.GetReportStationOffset(args, out var responseData);

      if (raptorResult == 1) // icsrrNoError
      {
        return responseData.Length > 0
          ? ConvertStationOffsetResult(request, responseData)
          : CreateNullStationOffsetReturnedResult();
      }

      throw CreateServiceException<CompactionReportStationOffsetExecutor>();
    }

    private CompactionReportResult ConvertStationOffsetResult(CompactionReportStationOffsetRequest request, Stream stream)
    {
      log.LogDebug($"{nameof(ConvertStationOffsetResult)}");

      // Unpack the data for the report and construct a stream containing the result
      var reportPackager = new TRaptorReportsPackager(TRaptorReportType.rrtStationOffset)
      {
        ReturnCode = TRaptorReportReturnCode.rrrcUnknownError
      };

      log.LogDebug($"{nameof(ConvertStationOffsetResult)}: Retrieving response data");
      reportPackager.ReadFromStream(stream);

      var stationRows = new StationRow[reportPackager.StationOffsetReport.NumberOfStations];

      for (var i = 0; i < reportPackager.StationOffsetReport.NumberOfStations; i++)
      {
        var station = reportPackager.StationOffsetReport.Stations[i];
        var stationRow = StationRow.Create(station, request);

        for (var j = 0; j < station.NumberOfOffsets; j++)
          stationRow.Offsets[j] = StationOffsetRow.CreateRow(station.Offsets[j], request);

        stationRows[i] = stationRow;
      }

      var startAndEndTime = request.Filter.StartUtc ?? DateTime.UtcNow;
      var stationOffsetReport = new StationOffsetReport(startAndEndTime, startAndEndTime, stationRows, request);

      return CompactionReportResult.CreateExportDataResult(stationOffsetReport, 1);
    }
#endif

    protected sealed override void ProcessErrorCodes()
    {
#if RAPTOR
      RaptorResult.AddErrorMessages(ContractExecutionStates);
#endif
    }

  }
}
