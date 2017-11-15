﻿using ASNodeDecls;
using ASNodeRaptorReports;
using Microsoft.Extensions.Logging;
using SVOICFilterSettings;
using System;
using System.Net;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Filters.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Executors
{
  /// <summary>
  /// The executor, which passes the report grid request to Raptor.
  /// </summary>
  /// 
  public class CompactionReportGridExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Processes the report grid request by passing the request to Raptor and returning the result.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>An instance of the CompactionReportGridResult class if successful.</returns>
    /// 
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      ContractExecutionResult result = null;

      try
      {
        CompactionReportGridRequest request = item as CompactionReportGridRequest;

        TICFilterSettings raptorFilter =
          RaptorConverters.ConvertFilter(request.FilterID, request.Filter, request.projectId);

        log.LogDebug("About to call GetReportGrid");

        var args = ASNode.GridReport.RPC.__Global.Construct_GridReport_Args(
          request.projectId ?? -1,
          (int)CompactionReportType.Grid,
          ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor(Guid.NewGuid(), 0,
            TASNodeCancellationDescriptorType.cdtProdDataReport),
          RaptorConverters.DesignDescriptor(request.DesignFile),
          request.GridInterval,
          request.ReportElevation,
          request.ReportCutFill,
          request.ReportCMV,
          request.ReportMDP,
          request.ReportPassCount,
          request.ReportTemperature,
          (int)request.GridReportOption,
          request.StartNorthing,
          request.StartEasting,
          request.EndNorthing,
          request.EndEasting,
          request.Azimuth,
          raptorFilter,
          RaptorConverters.ConvertLift(request.LiftBuildSettings, raptorFilter.LayerMethod),
          new SVOICOptionsDecls.TSVOICOptions() // ICOptions, need to resolve what this should be
        );

        int returnedResult = raptorClient.GetReportGrid(args, out var responseData);

        log.LogDebug("Completed call to GetReportGrid");

        if (returnedResult == 1) // icsrrNoError
        {
          try
          {
            // Unpack the data for the report and construct a stream containing the result
            TRaptorReportsPackager reportPackager = new TRaptorReportsPackager(TRaptorReportType.rrtGridReport)
            {
              ReturnCode = TRaptorReportReturnCode.rrrcUnknownError
            };

            reportPackager.GridReport.ElevationReport = request.ReportElevation;
            reportPackager.GridReport.CutFillReport = request.ReportCutFill;
            reportPackager.GridReport.CMVReport = request.ReportCMV;
            reportPackager.GridReport.MDPReport = request.ReportMDP;
            reportPackager.GridReport.PassCountReport = request.ReportPassCount;
            reportPackager.GridReport.TemperatureReport = request.ReportTemperature;

            log.LogDebug("Retrieving response data");

            reportPackager.ReadFromStream(responseData);

            var gridRows = new GridRow[reportPackager.GridReport.NumberOfRows];

            // Populate an array of grid rows from the data
            //foreach (TGridRow row in reportPackager.GridReport.Rows)
            for (var i = 0; i < reportPackager.GridReport.NumberOfRows; i++)
            {
              gridRows[i] = GridRow.CreateGridRow(
                reportPackager.GridReport.Rows[i].Northing,
                reportPackager.GridReport.Rows[i].Easting,
                reportPackager.GridReport.Rows[i].Elevation,
                reportPackager.GridReport.Rows[i].CutFill,
                reportPackager.GridReport.Rows[i].CMV,
                reportPackager.GridReport.Rows[i].MDP,
                reportPackager.GridReport.Rows[i].PassCount,
                reportPackager.GridReport.Rows[i].Temperature);

              gridRows[i].SetReportFlags(
                request.ReportElevation,
                request.ReportCutFill,
                request.ReportCMV,
                request.ReportMDP,
                request.ReportPassCount,
                request.ReportTemperature);
            }

            var startTime = request.Filter.StartUtc ?? DateTime.Now;
            var endTime = request.Filter.EndUtc ?? DateTime.Now;

            var gridReport = GridReport.CreateGridReport(startTime, endTime, gridRows);

            result = CompactionReportGridResult.CreateExportDataResult(gridReport.ToJsonString(), (short)returnedResult);
          }
          catch (Exception ex)
          {
            throw new ServiceException(HttpStatusCode.NoContent,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                "Failed to retrieve received grid report data: " + ex.Message));
          }
        }
        else
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
              "Failed to get requested grid report data"));
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
