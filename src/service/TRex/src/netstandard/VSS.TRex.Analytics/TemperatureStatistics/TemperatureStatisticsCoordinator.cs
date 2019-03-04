﻿using System.Reflection;
using Microsoft.Extensions.Logging;
using VSS.TRex.Analytics.Foundation;
using VSS.TRex.Analytics.Foundation.Aggregators;
using VSS.TRex.Analytics.Foundation.Coordinators;
using VSS.TRex.Analytics.TemperatureStatistics.GridFabric;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.TemperatureStatistics
{
	/// <summary>
	/// Computes Temperature statistics. Executes in the 'application service' layer and acts as the coordinator
	/// for the request onto the cluster compute layer.
	/// </summary>
	public class TemperatureStatisticsCoordinator : BaseAnalyticsCoordinator<TemperatureStatisticsArgument, TemperatureStatisticsResponse>
	{
		private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

		/// <summary>
		/// Constructs the aggregator from the supplied argument to be used for the Temperature statistics analytics request
		/// Create the aggregator to collect and reduce the results.
		/// </summary>
		/// <param name="argument"></param>
		/// <returns></returns>
		public override AggregatorBase ConstructAggregator(TemperatureStatisticsArgument argument) => new TemperatureStatisticsAggregator
		{
			RequiresSerialisation = true,
			SiteModelID = argument.ProjectID,
			//LiftBuildSettings := LiftBuildSettings;
			CellSize = SiteModel.CellSize,
			OverrideTemperatureWarningLevels = argument.OverrideTemperatureWarningLevels,
			OverridingTemperatureWarningLevels = argument.OverridingTemperatureWarningLevels,
		  DetailsDataValues = argument.TemperatureDetailValues,
		  Counts = argument.TemperatureDetailValues != null ? new long[argument.TemperatureDetailValues.Length] : null
    };

		/// <summary>
		/// Constructs the computor from the supplied argument and aggregator for the Temperature statistics analytics request
		/// </summary>
		/// <param name="argument"></param>
		/// <param name="aggregator"></param>
		/// <returns></returns>
		public override AnalyticsComputor ConstructComputor(TemperatureStatisticsArgument argument, AggregatorBase aggregator) => new AnalyticsComputor()
		{
			RequestDescriptor = RequestDescriptor,
			SiteModel = SiteModel,
			Aggregator = aggregator,
			Filters = argument.Filters,
			IncludeSurveyedSurfaces = true,
			RequestedGridDataType = GridDataType.Temperature
		};

		/// <summary>
		/// Pull the required counts information from the internal Temperature aggregator state
		/// </summary>
		/// <param name="aggregator"></param>
		/// <param name="response"></param>
		public override void ReadOutResults(AggregatorBase aggregator, TemperatureStatisticsResponse response)
		{
		  var tempAggregator = (DataStatisticsAggregator)aggregator;

      response.CellSize = tempAggregator.CellSize;
      response.SummaryCellsScanned = tempAggregator.SummaryCellsScanned;

      response.CellsScannedOverTarget = tempAggregator.CellsScannedOverTarget;
			response.CellsScannedUnderTarget = tempAggregator.CellsScannedUnderTarget;
			response.CellsScannedAtTarget = tempAggregator.CellsScannedAtTarget;

      response.IsTargetValueConstant = tempAggregator.IsTargetValueConstant;
      response.MissingTargetValue = tempAggregator.MissingTargetValue;

			response.LastTempRangeMin = ((TemperatureStatisticsAggregator) aggregator).LastTempRangeMin;
			response.LastTempRangeMax = ((TemperatureStatisticsAggregator)aggregator).LastTempRangeMax;

		  response.Counts = tempAggregator.Counts;

    }
	}
}
