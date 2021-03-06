﻿using VSS.TRex.Analytics.CMVStatistics.GridFabric;
using VSS.TRex.Analytics.Foundation;
using VSS.TRex.Analytics.Foundation.Aggregators;
using VSS.TRex.Analytics.Foundation.Coordinators;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.CMVStatistics
{
  /// <summary>
  /// Computes CMV statistics. Executes in the 'application service' layer and acts as the coordinator
  /// for the request onto the cluster compute layer.
  /// </summary>
  public class CMVStatisticsCoordinator : BaseAnalyticsCoordinator<CMVStatisticsArgument, CMVStatisticsResponse>
  {
   // private static readonly ILogger Log = Logging.Logger.CreateLogger<CMVStatisticsCoordinator>();

    /// <summary>
    /// Constructs the aggregator from the supplied argument to be used for the CMV statistics analytics request
    /// Create the aggregator to collect and reduce the results.
    /// </summary>
    /// <param name="argument"></param>
    /// <returns></returns>
    public override AggregatorBase ConstructAggregator(CMVStatisticsArgument argument) => new CMVStatisticsAggregator
    {
      SiteModelID = argument.ProjectID,
      CellSize = SiteModel.CellSize,
      OverrideMachineCMV = argument.Overrides.OverrideMachineCCV,
      OverridingMachineCMV = argument.Overrides.OverridingMachineCCV,
      CMVPercentageRange = argument.Overrides.CMVRange,
      DetailsDataValues = argument.CMVDetailValues,
      Counts = argument.CMVDetailValues != null ? new long[argument.CMVDetailValues.Length] : null
    };

    /// <summary>
    /// Constructs the computor from the supplied argument and aggregator for the CMV statistics analytics request
    /// </summary>
    /// <param name="argument"></param>
    /// <param name="aggregator"></param>
    /// <returns></returns>
    public override AnalyticsComputor ConstructComputor(CMVStatisticsArgument argument, AggregatorBase aggregator) => new AnalyticsComputor
    {
      RequestDescriptor = RequestDescriptor,
      SiteModel = SiteModel,
      Aggregator = aggregator,
      Filters = argument.Filters,
      IncludeSurveyedSurfaces = true,
      RequestedGridDataType = GridDataType.CCV,
      LiftParams = argument.LiftParams
    };

    /// <summary>
    /// Pull the required counts information from the internal CMV statistics aggregator state
    /// </summary>
    /// <param name="aggregator"></param>
    /// <param name="response"></param>
    public override void ReadOutResults(AggregatorBase aggregator, CMVStatisticsResponse response)
    {
      var tempAggregator = (DataStatisticsAggregator) aggregator;

      response.CellSize = tempAggregator.CellSize;
      response.SummaryCellsScanned = tempAggregator.SummaryCellsScanned;

      response.CellsScannedOverTarget = tempAggregator.CellsScannedOverTarget;
      response.CellsScannedUnderTarget = tempAggregator.CellsScannedUnderTarget;
      response.CellsScannedAtTarget = tempAggregator.CellsScannedAtTarget;

      response.IsTargetValueConstant = tempAggregator.IsTargetValueConstant;
      response.MissingTargetValue = tempAggregator.MissingTargetValue;

      response.LastTargetCMV = ((CMVStatisticsAggregator)aggregator).LastTargetCMV;

      response.Counts = tempAggregator.Counts;
    }
  }
}
