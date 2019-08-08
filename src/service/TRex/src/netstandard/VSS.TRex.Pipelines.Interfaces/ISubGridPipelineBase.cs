﻿using System;
using System.Threading.Tasks;
using VSS.TRex.Common.Models;
using VSS.TRex.Common.Types;
using VSS.TRex.Designs.Models;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Pipelines.Interfaces.Tasks;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Pipelines.Interfaces
{
    /// <summary>
    /// Interface for pipeline state and control for tasks and requests to access
    /// </summary>
    public interface ISubGridPipelineBase
    {
        /// <summary>
        /// The type of grid data to be selected from the data model
        /// </summary>
        GridDataType GridDataType { get; set; }

        ITRexTask PipelineTask { get; set; }

        /// <summary>
        /// Notes if the underlying query needs to include surveyed surface information in its results
        /// </summary>
        bool IncludeSurveyedSurfaceInformation { get; set; }

        /// <summary>
        /// The request descriptor ID for this request
        /// </summary>
        Guid RequestDescriptor { get; set; }

        /// <summary>
        /// A restriction on the cells that are returned via the query that intersects with the spatial selection filtering and criteria
        /// </summary>
        // BoundingIntegerExtent2D OverrideSpatialCellRestriction { get; set; }

        AreaControlSet AreaControlSet { get; set; }

        /// <summary>
        /// Advise the pipeline processing has been aborted
        /// </summary>
        void Abort();

        /// <summary>
        /// Determine if the pipeline has been aborted
        /// </summary>
        bool Aborted { get; }

        /// <summary>
        /// Determine if the pipeline was proactively terminated
        /// </summary>
        bool Terminated { get; set;  }

        /// <summary>
        /// Advise the pipeline that all processing activities have been completed
        /// </summary>
        bool PipelineCompleted { get; set; }

        /// <summary>
        /// Date model the pipeline is operating on
        /// </summary>
        Guid DataModelID { get; set; }

        /// <summary>
        /// Advise the client of the pipeline that a group of numProcessed sub grids has been processed
        /// </summary>
        void SubGridsProcessed(long numProcessed);

        /// <summary>
        /// The set of filter the pipeline requests are operating under
        /// </summary>
        IFilterSet FilterSet { get; set; }

        /// <summary>
        /// Map of all sub grids requiring information be requested from them
        /// </summary>
        ISubGridTreeBitMask OverallExistenceMap { get; set; }
      
        /// <summary>
        /// Map of all sub grids that specifically require production data to be requested for them
        /// </summary>
        ISubGridTreeBitMask ProdDataExistenceMap { get; set; }
      
        /// <summary>
        /// Map of all sub grids that require elevation data to be extracted from a design surface
        /// </summary>
        ISubGridTreeBitMask DesignSubGridOverlayMap { get; set; }

        /// <summary>
        /// Initiates processing of the pipeline
        /// </summary>
        bool Initiate();

        /// <summary>
        /// Wait for the pipeline to completes operations, or abort at expiration of time to live timeout
        /// </summary>
        Task<bool> WaitForCompletion();

        IRequestAnalyser RequestAnalyser { get; set; }

        long SubGridsRemainingToProcess { get; }

        DesignOffset ReferenceDesign { get; set; }

        ILiftParameters LiftParams { get; set; }

        int MaxNumberOfPassesToReturn { get; set; }
    }

  }
}
