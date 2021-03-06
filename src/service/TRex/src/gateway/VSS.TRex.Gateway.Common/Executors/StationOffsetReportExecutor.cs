﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.TRex.Filters;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.Gateway.Common.ResultHandling;
using VSS.TRex.Reports.StationOffset.GridFabric.Arguments;
using VSS.TRex.Reports.StationOffset.GridFabric.Requests;
using VSS.TRex.Reports.StationOffset.GridFabric.Responses;

namespace VSS.TRex.Gateway.Common.Executors
{
  public class StationOffsetReportExecutor : BaseExecutor
  {
    public StationOffsetReportExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler exceptionHandler) : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public StationOffsetReportExecutor()
    {
    }

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = CastRequestObjectTo<CompactionReportStationOffsetTRexRequest>(item);
      var siteModel = GetSiteModel(request.ProjectUid);
      var filter = ConvertFilter(request.Filter, siteModel);
      
      var tRexRequest = new StationOffsetReportRequest_ApplicationService();
      var stationOffsetReportRequestArgument_ApplicationService = AutoMapperUtility.Automapper.Map<StationOffsetReportRequestArgument_ApplicationService>(request);
      stationOffsetReportRequestArgument_ApplicationService.Filters = new FilterSet(filter);
      
      var response = await tRexRequest.ExecuteAsync(stationOffsetReportRequestArgument_ApplicationService);
      var result = new StationOffsetReportResult()
      {
        ReturnCode = response?.ReturnCode ?? ReportReturnCode.UnknownError,
        ReportType = ReportType.StationOffset,
        GriddedData = AutoMapperUtility.Automapper.Map<StationOffsetReportData_ApplicationService>(request)
      };
      result.GriddedData.NumberOfRows = response?.StationOffsetReportDataRowList.Count ?? 0;
      result.GriddedData.Rows.AddRange(response?.StationOffsetReportDataRowList ?? new List<StationOffsetReportDataRow_ApplicationService>());
      return new GriddedReportDataResult(result.Write());
    }

    /// <summary>
    /// Processes the tile request synchronously.
    /// </summary>
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }
  }
}
