﻿using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Pipelines.Interfaces
{
  public interface IRequestAnalyser
  {
    /// <summary>
    /// The pipeline that has initiated this request analysis
    /// </summary>
    ISubGridPipelineBase Pipeline { get; set; }

    /// <summary>
    /// The resulting bitmap subgrid tree mask of all subgrids containing production data that need to be requested
    /// </summary>
    ISubGridTreeBitMask ProdDataMask { get; set; }

    /// <summary>
    /// The resulting bitmap subgrid tree mask of all subgrids containing production data that need to be requested
    /// </summary>
    ISubGridTreeBitMask SurveydSurfaceOnlyMask { get; set; }

    /// <summary>
    /// Indicates if only a single page of subgrid requests will be processed
    /// </summary>
    bool SubmitSinglePageOfRequests { get; set; }

    /// <summary>
    /// The number of subgrids present in a requested page of subgrids
    /// </summary>
    int SinglePageRequestSize { get; set; }

    /// <summary>
    /// The page number of the page of subgrids to be requested
    /// </summary>
    int SinglePageRequestNumber { get; set; }

    /// <summary>
    /// The spatial extents derived from the parameters when building the pipeline
    /// </summary>
    BoundingWorldExtent3D WorldExtents { get; set; }

    /// <summary>
    /// The executor method for the analyser
    /// </summary>
    /// <returns></returns>
    bool Execute();

    /// <summary>
    /// Counts the number of subgrids that will be submitted to the processing engine given the request parameters
    /// supplied to th erequest analyser.
    /// </summary>
    /// <returns></returns>
    long CountOfSubgridsThatWillBeSubmitted();

    long TotalNumberOfSubgridsAnalysed { get; set; }
    long TotalNumberOfSubgridsToRequest { get; set; }
    long TotalNumberOfCandidateSubgrids { get; set; }
  }
}
