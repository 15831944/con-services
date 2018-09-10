﻿using VSS.TRex.Analytics.Foundation.GridFabric.Responses;
using VSS.TRex.Analytics.Foundation.Interfaces;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.MDPStatistics.GridFabric
{
  /// <summary>
  /// The response state returned from a MDP statistics request
  /// </summary>
  public class MDPStatisticsResponse : StatisticsAnalyticsResponse, IAggregateWith<MDPStatisticsResponse>, IAnalyticsOperationResponseResultConversion<MDPStatisticsResult>
  {
    /// <summary>
    /// Holds last known good target MDP value.
    /// </summary>
    public short LastTargetMDP { get; set; }

    /// <summary>
    /// Aggregate a set of MDP statistics into this set and return the result.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    protected override void AggregateBaseDataWith(StatisticsAnalyticsResponse other)
    {
      base.AggregateBaseDataWith(other);

      LastTargetMDP = ((MDPStatisticsResponse)other).LastTargetMDP;

    }

    public MDPStatisticsResponse AggregateWith(MDPStatisticsResponse other)
    {
      return base.AggregateWith(other) as MDPStatisticsResponse;
    }


    public MDPStatisticsResult ConstructResult()
    {
      return new MDPStatisticsResult
      {
        IsTargetMDPConstant = IsTargetValueConstant,
        ConstantTargetMDP = LastTargetMDP,
        AboveTargetPercent = ValueOverTargetPercent,
        WithinTargetPercent = ValueAtTargetPercent,
        BelowTargetPercent = ValueUnderTargetPercent,
        TotalAreaCoveredSqMeters = SummaryProcessedArea,

        ReturnCode = MissingTargetValue ? SummaryCellsScanned == 0 ? MissingTargetDataResultType.NoResult : MissingTargetDataResultType.PartialResult : MissingTargetDataResultType.NoProblems,

        Counts = Counts,
        ResultStatus = ResultStatus
      };
    }
  }
}
