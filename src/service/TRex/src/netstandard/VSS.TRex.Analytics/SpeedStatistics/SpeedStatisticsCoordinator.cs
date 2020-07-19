﻿using VSS.TRex.Analytics.Foundation;
using VSS.TRex.Analytics.Foundation.Aggregators;
using VSS.TRex.Analytics.Foundation.Coordinators;
using VSS.TRex.Analytics.SpeedStatistics.GridFabric;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.SpeedStatistics
{
	/// <summary>
	/// Computes Speed statistics. Executes in the 'application service' layer and acts as the coordinator
	/// for the request onto the cluster compute layer.
	/// </summary>
  public class SpeedStatisticsCoordinator : BaseAnalyticsCoordinator<SpeedStatisticsArgument, SpeedStatisticsResponse>
  {
    // private static readonly ILogger Log = Logging.Logger.CreateLogger<SpeedStatisticsCoordinator>();

    /// <summary>
    /// Constructs the aggregator from the supplied argument to be used for the Speed statistics analytics request
    /// Create the aggregator to collect and reduce the results.
    /// </summary>
    /// <param name="argument"></param>
    /// <returns></returns>
    public override AggregatorBase ConstructAggregator(SpeedStatisticsArgument argument) => new SpeedStatisticsAggregator
		{
			SiteModelID = argument.ProjectID,
			CellSize = SiteModel.CellSize,
			TargetMachineSpeed = argument.Overrides.TargetMachineSpeed
		};

		/// <summary>
		/// Constructs the computor from the supplied argument and aggregator for the Speed statistics analytics request
		/// </summary>
		/// <param name="argument"></param>
		/// <param name="aggregator"></param>
		/// <returns></returns>
		public override AnalyticsComputor ConstructComputor(SpeedStatisticsArgument argument, AggregatorBase aggregator) => new AnalyticsComputor
		{
			RequestDescriptor = RequestDescriptor,
			SiteModel = SiteModel,
			Aggregator = aggregator,
			Filters = argument.Filters,
			IncludeSurveyedSurfaces = true,
			RequestedGridDataType = GridDataType.MachineSpeedTarget,
      LiftParams = argument.LiftParams
    };

		/// <summary>
		/// Pull the required counts information from the internal Speed aggregator state
		/// </summary>
		/// <param name="aggregator"></param>
		/// <param name="response"></param>
		public override void ReadOutResults(AggregatorBase aggregator, SpeedStatisticsResponse response)
		{
		  var tempAggregator = (DataStatisticsAggregator)aggregator;

      response.CellSize = tempAggregator.CellSize;
		  response.SummaryCellsScanned = tempAggregator.SummaryCellsScanned;

		  response.CellsScannedOverTarget = tempAggregator.CellsScannedOverTarget;
		  response.CellsScannedUnderTarget = tempAggregator.CellsScannedUnderTarget;
		  response.CellsScannedAtTarget = tempAggregator.CellsScannedAtTarget;

		  response.IsTargetValueConstant = tempAggregator.IsTargetValueConstant;
		  response.MissingTargetValue = tempAggregator.MissingTargetValue;
		}
	}
}
