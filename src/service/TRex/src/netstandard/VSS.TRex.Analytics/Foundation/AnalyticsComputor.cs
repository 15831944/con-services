﻿using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using VSS.TRex.Analytics.Foundation.GridFabric.Responses;
using VSS.TRex.DI;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.Interfaces;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.Pipelines.Interfaces.Tasks;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.Foundation
{
    /// <summary>
    /// The base class the implements the analytics computation framework 
    /// </summary>
    public class AnalyticsComputor
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

        /// <summary>
        /// The Aggregator to use for calculation of analytics
        /// </summary>
        public ISubGridRequestsAggregator Aggregator { get; set; }

        /// <summary>
        /// The Sitemodel from which the volume is being calculated
        /// </summary>
        public ISiteModel SiteModel { get; set; }

        /// <summary>
        /// Identifier for the design to be used as the basis for any required cut fill operations
        /// </summary>
        public Guid CutFillDesignID { get; set; } = Guid.Empty;

        /// <summary>
        /// The underlying grid data type required to satisfy the processing requirements of this analytics computor
        /// </summary>
        public GridDataType RequestedGridDataType { get; set; } = GridDataType.All;

        /// <summary>
        ///  Default no-arg constructor
        /// </summary>
        public AnalyticsComputor()
        {
        }

        public Guid RequestDescriptor { get; set; } = Guid.Empty;

        public IFilterSet Filters { get; set; }

        public bool IncludeSurveyedSurfaces { get; set; }

        /// <summary>
        /// Primary method called to begin analytics computation
        /// </summary>
        /// <returns></returns>
        public bool ComputeAnalytics(BaseAnalyticsResponse response)
        {
          // TODO: add when lift build setting supported
          // FAggregateState.LiftBuildSettings := FLiftBuildSettings;

          IPipelineProcessor processor = DIContext.Obtain<IPipelineProcessorFactory>().NewInstanceNoBuild
          (requestDescriptor: RequestDescriptor,
            dataModelID: SiteModel.ID,
            gridDataType: RequestedGridDataType,
            response: response,
            filters: Filters,
            cutFillDesignID: CutFillDesignID,
            task: DIContext.Obtain<Func<PipelineProcessorTaskStyle, ITRexTask>>()(PipelineProcessorTaskStyle.AggregatedPipelined),
            pipeline: DIContext.Obtain<Func<PipelineProcessorPipelineStyle, ISubGridPipelineBase>>()(PipelineProcessorPipelineStyle.DefaultAggregative),
            requestAnalyser: DIContext.Obtain<IRequestAnalyser>(),
            requestRequiresAccessToDesignFileExistenceMap: CutFillDesignID != Guid.Empty,
            requireSurveyedSurfaceInformation: IncludeSurveyedSurfaces,
            overrideSpatialCellRestriction: BoundingIntegerExtent2D.Inverted()
          );

          // Assign the provided aggregator into the pipelined sub grid task
          ((IAggregatedPipelinedSubGridTask) processor.Task).Aggregator = Aggregator;

          if (!processor.Build())
          {
            Log.LogError($"Failed to build pipeline processor for request to model {SiteModel.ID}");
            return false;
          }

          processor.Process();

          return response.ResultStatus == RequestErrorStatus.OK;
        }
    }
}
