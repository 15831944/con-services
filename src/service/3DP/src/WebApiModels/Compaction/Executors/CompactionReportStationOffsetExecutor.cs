﻿using ASNodeDecls;
using ASNodeRaptorReports;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models.Reports;

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

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      try
      {
        var request = item as CompactionReportStationOffsetRequest;

        if (request == null)
          ThrowRequestTypeCastException<CompactionReportStationOffsetRequest>();

        bool.TryParse(configStore.GetValueString("ENABLE_TREX_GATEWAY_STATIONOFFSET"), out var useTrexGateway);

        MemoryStream responseData;
        var success = 1;

        if (useTrexGateway)
        {
          responseData = (MemoryStream) trexCompactionDataProxy.SendStationOffsetRequest(request, customHeaders).Result;
        }
        else
        {
          var filterSettings = RaptorConverters.ConvertFilter(request.FilterID, request.Filter, request.ProjectId);
          var cutfillDesignDescriptor = RaptorConverters.DesignDescriptor(request.DesignFile);
          var alignmentDescriptor = RaptorConverters.DesignDescriptor(request.AlignmentFile);
          var userPreferences =
            ExportRequestHelper.ConvertUserPreferences(request.UserPreferences, request.ProjectTimezone);

          var options = RaptorConverters.convertOptions(null, request.LiftBuildSettings, 0,
            request.Filter?.LayerType ?? FilterLayerMethod.None, DisplayMode.Height, false);

          log.LogDebug("About to call GetReportStationOffset");

          var args = ASNode.StationOffsetReport.RPC.__Global.Construct_StationOffsetReport_Args(
            request.ProjectId ?? -1,
            (int) CompactionReportType.StationOffset,
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
            (int) GridReportOption.Unused,
            0, 0, 0, 0, 0, 0, 0, // Northings, Eastings and Direction values are not used on Station Offset report.
            filterSettings,
            RaptorConverters.ConvertLift(request.LiftBuildSettings, filterSettings.LayerMethod),
            options
          );

          int returnedResult = raptorClient.GetReportStationOffset(args, out responseData);

          if (returnedResult != success)
            throw CreateServiceException<CompactionReportStationOffsetExecutor>();
        }

        log.LogDebug("Completed call to GetReportStationOffset");
        
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
              stationRow.Offsets[j] = StationOffsetRow.CreateRow(station.Offsets[j], request);
            }

            stationRows[i] = stationRow;
          }

          var startAndEndTime = request.Filter.StartUtc ?? DateTime.Now;
          var stationOffsetReport = StationOffsetReport.CreateReport(startAndEndTime, startAndEndTime, stationRows, request);

          return CompactionReportResult.CreateExportDataResult(stationOffsetReport, (short)success);
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
    }
    protected sealed override void ProcessErrorCodes()
    {
      RaptorResult.AddErrorMessages(ContractExecutionStates);
    }
  }
}
