﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Analytics.PassCountStatistics;
using VSS.TRex.Analytics.PassCountStatistics.GridFabric;
using VSS.TRex.Common.Models;
using VSS.TRex.Filters;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.Types;
using PassCountStatisticsResult = VSS.TRex.Analytics.PassCountStatistics.PassCountStatisticsResult;
using SummaryResult = VSS.Productivity3D.Productivity3D.Models.Compaction.ResultHandling.PassCountSummaryResult;
using TargetPassCountRange = VSS.Productivity3D.Productivity3D.Models.Compaction.TargetPassCountRange;


namespace VSS.TRex.Gateway.Common.Executors
{
  /// <summary>
  /// Processes the request to get Pass Count summary.
  /// </summary>
  public class SummaryPassCountExecutor : BaseExecutor
  {
    public SummaryPassCountExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler exceptionHandler)
      : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public SummaryPassCountExecutor()
    {
    }

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as PassCountSummaryRequest;

      if (request == null)
        ThrowRequestTypeCastException< PassCountSummaryRequest>();

      var siteModel = GetSiteModel(request.ProjectUid);

      var filter = ConvertFilter(request.Filter, siteModel);

      var operation = new PassCountStatisticsOperation();
      var passCountSummaryResult = await operation.ExecuteAsync(
        new PassCountStatisticsArgument
        {
          ProjectID = siteModel.ID,
          Filters = new FilterSet(filter),
          Overrides = AutoMapperUtility.Automapper.Map<OverrideParameters>(request.Overrides),
          LiftParams = ConvertLift(request.LiftSettings, request.Filter?.LayerType)
        }
      );

      if (passCountSummaryResult != null)
      {
        if (passCountSummaryResult.ResultStatus == RequestErrorStatus.OK)
          return ConvertResult(passCountSummaryResult);

        throw CreateServiceException<SummaryPassCountExecutor>(passCountSummaryResult.ResultStatus);
      }

      throw CreateServiceException<SummaryPassCountExecutor>();
    }

    private SummaryResult ConvertResult(PassCountStatisticsResult summary)
    {
      return new SummaryResult(
        new TargetPassCountRange(summary.ConstantTargetPassCountRange.Min, summary.ConstantTargetPassCountRange.Max),
        summary.IsTargetPassCountConstant,
        summary.WithinTargetPercent,
        summary.AboveTargetPercent,
        summary.BelowTargetPercent,
        (short)summary.ReturnCode,
        summary.TotalAreaCoveredSqMeters);
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
