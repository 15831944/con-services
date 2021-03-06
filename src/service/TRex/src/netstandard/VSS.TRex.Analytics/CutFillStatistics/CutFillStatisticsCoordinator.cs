﻿using System;
using Remotion.Linq.Parsing;
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
    /// <summary>
    /// Constructs the aggregator from the supplied argument to be used for the cut/fill statistics analytics request
    /// Create the aggregator to collect and reduce the results. As a part of this locate the
    /// design instance representing the design the cut/fill information is being calculated against
    /// and supply that to the aggregator
    /// </summary>
    public override AggregatorBase ConstructAggregator(CutFillStatisticsArgument argument)
    {
      if (argument.Offsets == null || argument.Offsets.Length != 7)
      {
        throw new ArgumentException($"Argument.offsets is null or not 7 elements as expected: {argument.Offsets?.Length.ToString() ?? "<null>"}");
      }

      return new CutFillStatisticsAggregator
      {
        SiteModelID = argument.ProjectID,
        CellSize = SiteModel.CellSize,
        Offsets = argument.Offsets,
        Counts = new long[argument.Offsets.Length]
      };
    }

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
      CutFillDesign = argument.ReferenceDesign,
      LiftParams = argument.LiftParams
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
