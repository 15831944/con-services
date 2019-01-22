﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.TRex.Filters;
using VSS.TRex.Gateway.Common.ResultHandling;
using VSS.TRex.Reports.Gridded;
using VSS.TRex.Reports.Gridded.GridFabric;

namespace VSS.TRex.Gateway.Common.Executors
{
  public class GriddedReportExecutor : BaseExecutor
  {
    public GriddedReportExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler exceptionHandler) : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public GriddedReportExecutor()
    {
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      var request = item as CompactionReportGridTRexRequest;

      if (request == null)
      {
        ThrowRequestTypeCastException<CompactionReportGridTRexRequest>();
        return null; // to keep compiler happy
      }

        var siteModel = GetSiteModel(request.ProjectUid);
      var filter = ConvertFilter(request.Filter, siteModel);

      GriddedReportRequest tRexRequest = new GriddedReportRequest();

      GriddedReportRequestResponse response = tRexRequest.Execute(new GriddedReportRequestArgument
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(filter),
        ReferenceDesignUID = request.CutFillDesignUid ?? Guid.Empty, // only present if ReportCutFill required
        ReportElevation = request.ReportElevation,
        ReportCutFill = request.ReportCutFill,
        ReportCMV = request.ReportCmv,
        ReportMDP = request.ReportMdp,
        ReportPassCount = request.ReportPassCount,
        ReportTemperature = request.ReportTemperature,
        GridInterval = request.GridInterval,
        GridReportOption = request.GridReportOption,
        StartNorthing = request.StartNorthing,
        StartEasting = request.StartEasting,
        EndNorthing = request.EndNorthing,
        EndEasting = request.EndEasting,
        Azimuth = request.Azimuth
      });

      var result = new GriddedReportResult()
      {
        ReturnCode = response.ReturnCode,
        ReportType = ReportType.Gridded,
        GriddedData = new GriddedReportData()
        {
          ElevationReport = request.ReportElevation,
          CutFillReport = request.ReportCutFill,
          CmvReport = request.ReportCmv,
          MdpReport = request.ReportMdp,
          PassCountReport = request.ReportPassCount,
          TemperatureReport = request.ReportTemperature,
          NumberOfRows = response.GriddedReportDataRowList.Count
        }
      };
      result.GriddedData.Rows.AddRange(response.GriddedReportDataRowList);
      return new GriddedReportDataResult(result.Write());
    }

    /// <summary>
    /// Processes the request asynchronously.
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      throw new NotImplementedException();
    }
  }
}
