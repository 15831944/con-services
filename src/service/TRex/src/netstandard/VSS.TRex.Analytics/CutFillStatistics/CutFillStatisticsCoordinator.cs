﻿using Microsoft.Extensions.Logging;
using VSS.TRex.Analytics.CutFillStatistics.GridFabric;
using VSS.TRex.Analytics.Foundation;
using VSS.TRex.Analytics.Foundation.Aggregators;
using VSS.TRex.Analytics.Foundation.Coordinators;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.CutFillStatistics
{
  /// <summary>
  /// Computes cut fill statistics. Executes in the 'application service' layer and acts as the coordinator
  /// for the request onto the cluster compute layer.
  /// </summary>
  public class CutFillStatisticsCoordinator : BaseAnalyticsCoordinator<CutFillStatisticsArgument, CutFillStatisticsResponse>
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<CutFillStatisticsCoordinator>();

    /// <summary>
    /// Constructs the aggregator from the supplied argument to be used for the cut/fill statistics analytics request
    /// Create the aggregator to collect and reduce the results. As a part of this locate the
    /// design instance representing the design the cut/fill information is being calculated against
    /// and supply that to the aggregator
    /// </summary>
    /// <param name="argument"></param>
    /// <returns></returns>
    public override AggregatorBase ConstructAggregator(CutFillStatisticsArgument argument) => new CutFillStatisticsAggregator
    {
      RequiresSerialisation = true,
      SiteModelID = argument.ProjectID,
      CellSize = SiteModel.Grid.CellSize,
      Offsets = argument.Offsets,
      Counts = argument.Offsets != null ? new long[argument.Offsets.Length] : null
    };

    /// <summary>
    /// Constructs the computor from the supplied argument and aggregator for the cut fill statistics analytics request
    /// </summary>
    /// <param name="argument"></param>
    /// <param name="aggregator"></param>
    /// <returns></returns>
    public override AnalyticsComputor ConstructComputor(CutFillStatisticsArgument argument,
                                                        AggregatorBase aggregator) => new AnalyticsComputor
    {
      RequestDescriptor = RequestDescriptor,
      SiteModel = SiteModel,
      Aggregator = aggregator,
      Filters = argument.Filters,
      IncludeSurveyedSurfaces = true,
      RequestedGridDataType = GridDataType.CutFill,
      CutFillDesignID = argument.DesignID
    };

    /// <summary>
    /// Pull the required counts information from the internal cut fill aggregator state
    /// </summary>
    /// <param name="aggregator"></param>
    /// <param name="response"></param>
    public override void ReadOutResults(AggregatorBase aggregator, CutFillStatisticsResponse response)
    {
      response.Counts = ((CutFillStatisticsAggregator)aggregator).Counts;
    }
  }
}
