﻿using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.DI;
using VSS.TRex.ExistenceMaps.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Common.Utilities;

namespace VSS.TRex.Pipelines
{
  /// <summary>
  /// RequestAnalyzer examines the set of parameters defining a request and determines the full set of sub grids
  /// the need to be requested, and the production data/surveyed surface aspects of those requests.
  /// Its implementation was modeled on the activities of the Legacy SVO ICSubGridSubmissionThread class.
  /// </summary>
  public class RequestAnalyser : IRequestAnalyser
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<RequestAnalyser>();

    private IExistenceMaps existenceMaps;
    private IExistenceMaps GetExistenceMaps() => existenceMaps ??= DIContext.Obtain<IExistenceMaps>();

    /// <summary>
    /// The pipeline that has initiated this request analysis
    /// </summary>
    public ISubGridPipelineBase Pipeline { get; set; }

    /// <summary>
    /// The resulting bitmap sub grid tree mask of all sub grids containing production data that need to be requested
    /// </summary>
    public ISubGridTreeBitMask ProdDataMask { get; set; }

    /// <summary>
    /// The resulting bitmap sub grid tree mask of all sub grids containing production data that need to be requested
    /// </summary>
    public ISubGridTreeBitMask SurveyedSurfaceOnlyMask { get; set; }

    /// <summary>
    /// A cell coordinate level (rather than world coordinate) boundary that acts as an optional final override of the spatial area
    /// within which sub grids are being requested
    /// </summary>
    public BoundingIntegerExtent2D OverrideSpatialCellRestriction = BoundingIntegerExtent2D.Inverted();

    /// <summary>
    /// The bounding world extents constraining the query, derived from filter, design and other spatial restrictions
    /// and criteria related to the query parameters
    /// </summary>
    public BoundingWorldExtent3D WorldExtents { get; set; } = BoundingWorldExtent3D.Inverted();

    public long TotalNumberOfSubGridsAnalysed { get; set; }
    public long TotalNumberOfSubGridsToRequest { get; set; }
    public long TotalNumberOfCandidateSubGrids { get; set; }

    protected bool ScanningFullWorldExtent;

    /// <summary>
    /// Indicates if the request analyzer is only counting the sub grid requests that will be made
    /// </summary>
    private bool CountingRequestsOnly { get; set; } = false;

    /// <summary>
    /// Indicates if only a single page of sub grid requests will be processed
    /// </summary>
    public bool SubmitSinglePageOfRequests { get; set; } = false;

    /// <summary>
    /// The number of sub grids present in a requested page of sub grids
    /// </summary>
    public int SinglePageRequestSize { get; set; } = -1;

    /// <summary>
    /// The page number of the page of sub grids to be requested
    /// </summary>
    public int SinglePageRequestNumber { get; set; } = -1;

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public RequestAnalyser()
    {
      ProdDataMask = new SubGridTreeSubGridExistenceBitMask();
      SurveyedSurfaceOnlyMask = new SubGridTreeSubGridExistenceBitMask();
    }

    /// <summary>
    /// Constructor accepting the pipeline (analyzer client) and the bounding world coordinate extents within which sub grids
    /// are being requested
    /// </summary>
    public RequestAnalyser(ISubGridPipelineBase pipeline, BoundingWorldExtent3D worldExtents) : this()
    {
      Pipeline = pipeline;
      WorldExtents = worldExtents;
    }

    /// <summary>
    /// Performs the donkey work of the request analysis
    /// </summary>
    protected void PerformScanning()
    {
      TotalNumberOfSubGridsToRequest = 0;
      TotalNumberOfSubGridsAnalysed = 0;
      TotalNumberOfCandidateSubGrids = 0;

      var FilterRestriction = new BoundingWorldExtent3D();

      // Compute a filter spatial restriction on the world extents of the request
      if (WorldExtents.IsValidPlanExtent)
        FilterRestriction.Assign(WorldExtents);
      else
        FilterRestriction.SetMaximalCoverage();

      Pipeline.FilterSet.ApplyFilterAndSubsetBoundariesToExtents(FilterRestriction);

      // Combine the overall existence map with the existence maps from any surface design filter aspects in 
      // the filter set supplied with the request.
      foreach (var filter in Pipeline.FilterSet.Filters)
      {
        if (filter != null && filter.SpatialFilter.IsDesignMask)
        {
          _log.LogDebug($"Has Design {filter.SpatialFilter.SurfaceDesignMaskDesignUid}, ANDing with OverallExistMap");

          var mask = GetExistenceMaps().GetSingleExistenceMap(Pipeline.DataModelID, Consts.EXISTENCE_MAP_DESIGN_DESCRIPTOR, filter.SpatialFilter.SurfaceDesignMaskDesignUid);

          if (mask != null)
            Pipeline.OverallExistenceMap.SetOp_AND(mask);
          else
            throw new Exception($"{nameof(RequestAnalyser)}: Failed to get existence map for surface design ID:{filter.SpatialFilter.SurfaceDesignMaskDesignUid}");
        }
      }

      ScanningFullWorldExtent = !WorldExtents.IsValidPlanExtent || WorldExtents.IsMaximalPlanConverage;

      if (ScanningFullWorldExtent)
        Pipeline.OverallExistenceMap.ScanSubGrids(Pipeline.OverallExistenceMap.FullCellExtent(), SubGridEvent);
      else
        Pipeline.OverallExistenceMap.ScanSubGrids(FilterRestriction, SubGridEvent);
    }

    /// <summary>
    /// The executor method for the analyzer
    /// </summary>
    public bool Execute()
    {
      if (Pipeline == null)
        throw new ArgumentException("No owning pipeline", nameof(Pipeline));

      if (Pipeline.FilterSet == null)
        throw new ArgumentException("No filters in pipeline", nameof(Pipeline.FilterSet));

      if (Pipeline.ProdDataExistenceMap == null)
        throw new ArgumentException("Production Data Existence Map should have been specified", nameof(Pipeline.ProdDataExistenceMap));

      PerformScanning();

      return true;
    }

    /// <summary>
    /// Performs scanning operations across sub grids, determining if they should be included in the request
    /// </summary>
    protected bool SubGridEvent(ISubGrid SubGrid)
    {
      // The given sub grid is a leaf sub grid containing a bit mask recording sub grid inclusion in the overall sub grid map 
      // being iterated over. This map includes, production data only sub grids, surveyed surface data only sub grids and
      // sub grids that will have both types of data retrieved for them. The analyzer needs to separate out the two in terms
      // of the masks of sub grids that needs to be queried, one for production data (and optionally surveyed surface data) and 
      // one for surveyed surface data only. 
      // Get the matching sub grid from the production data only bit mask sub grid tree and use this sub grid to be able to separate
      // the two sets of sub grids

      var ProdDataSubGrid = Pipeline.ProdDataExistenceMap.LocateSubGridContaining(SubGrid.OriginX, SubGrid.OriginY) as SubGridTreeLeafBitmapSubGrid;

      byte ScanMinXb, ScanMinYb, ScanMaxXb, ScanMaxYb;
      var OTGCellSize = SubGrid.Owner.CellSize / SubGridTreeConsts.SubGridTreeDimension;
      var CastSubGrid = (SubGridTreeLeafBitmapSubGrid) SubGrid;

      if (ScanningFullWorldExtent)
      {
        ScanMinXb = 0;
        ScanMinYb = 0;
        ScanMaxXb = SubGridTreeConsts.SubGridTreeDimensionMinus1;
        ScanMaxYb = SubGridTreeConsts.SubGridTreeDimensionMinus1;
      }
      else
      {
        // Calculate the range of cells in this sub grid we need to scan and request. The steps below
        // calculate the on-the-ground cell indices of the world coordinate bounding extents, then
        // determine the sub grid indices of the cell within this sub grid that contains those
        // cells, then determines the sub grid extents in this sub grid to scan over
        // Remember, each on-the-ground element (bit) in the existence map represents an
        // entire on-the-ground sub grid (32x32 OTG cells) in the matching sub grid tree.

        // Expand the number of cells scanned to create the rendered tile by a single cell width
        // on all sides to ensure the boundaries of tiles are rendered right to the edge of the tile.

        SubGrid.Owner.CalculateIndexOfCellContainingPosition(WorldExtents.MinX - OTGCellSize,
          WorldExtents.MinY - OTGCellSize,
          out var ScanMinX, out var ScanMinY);
        SubGrid.Owner.CalculateIndexOfCellContainingPosition(WorldExtents.MaxX + OTGCellSize,
          WorldExtents.MaxY + OTGCellSize,
          out var ScanMaxX, out var ScanMaxY);

        ScanMinX = Math.Max(CastSubGrid.OriginX, ScanMinX);
        ScanMinY = Math.Max(CastSubGrid.OriginY, ScanMinY);
        ScanMaxX = Math.Min(ScanMaxX, CastSubGrid.OriginX + SubGridTreeConsts.SubGridTreeDimensionMinus1);
        ScanMaxY = Math.Min(ScanMaxY, CastSubGrid.OriginY + SubGridTreeConsts.SubGridTreeDimensionMinus1);

        SubGrid.GetSubGridCellIndex(ScanMinX, ScanMinY, out ScanMinXb, out ScanMinYb);
        SubGrid.GetSubGridCellIndex(ScanMaxX, ScanMaxY, out ScanMaxXb, out ScanMaxYb);
      }

      // Iterate over the sub range of cells (bits) in this sub grid and request the matching sub grids

      for (byte I = ScanMinXb; I <= ScanMaxXb; I++)
      {
        for (byte J = ScanMinYb; J <= ScanMaxYb; J++)
        {
          if (CastSubGrid.Bits.BitSet(I, J))
          {
            TotalNumberOfCandidateSubGrids++;

            // If there is a design sub grid overlay map supplied to the renderer then
            // check to see if this sub grid is in the map, and if so then continue. If it is
            // not in the map then it does not need to be considered. Design sub grid overlay
            // indices contain a single bit for each on the ground sub grid (32x32 cells),
            // which means they are only 5 levels deep. This means the (OriginX + I, OriginY + J)
            // origin coordinates correctly identify the single bits that denote the sub grids.

            if (Pipeline.DesignSubGridOverlayMap != null)
            {
              if (!Pipeline.DesignSubGridOverlayMap.GetCell(SubGrid.OriginX + I, SubGrid.OriginY + J))
                continue;
            }

            // If there is a spatial filter in play then determine if the sub grid about to be requested intersects the spatial filter extent

            var SubGridSatisfiesFilter = true;
            foreach (var filter in Pipeline.FilterSet.Filters)
            {
              if (filter != null)
              {
                var spatialFilter = filter.SpatialFilter;

                if (spatialFilter.IsSpatial && spatialFilter.Fence != null && spatialFilter.Fence.NumVertices > 0)
                {
                  SubGridSatisfiesFilter =
                    spatialFilter.Fence.IntersectsExtent(
                      CastSubGrid.Owner.GetCellExtents(CastSubGrid.OriginX + I, CastSubGrid.OriginY + J));
                }
                else
                {
                  if (spatialFilter.IsPositional)
                  {
                    CastSubGrid.Owner.GetCellCenterPosition(CastSubGrid.OriginX + I, CastSubGrid.OriginY + J,
                      out var centerX, out var centerY);

                    SubGridSatisfiesFilter = MathUtilities.Hypot(spatialFilter.PositionX - centerX, spatialFilter.PositionY - centerY) <
                                             spatialFilter.PositionRadius + (Math.Sqrt(2) * CastSubGrid.Owner.CellSize) / 2;
                  }
                }

                if (!SubGridSatisfiesFilter)
                  break;
              }
            }

            if (SubGridSatisfiesFilter)
            {
              TotalNumberOfSubGridsAnalysed++;

              if (SubmitSinglePageOfRequests)
              {
                if ((TotalNumberOfSubGridsAnalysed - 1) / SinglePageRequestSize < SinglePageRequestNumber)
                  continue;

                if ((TotalNumberOfSubGridsAnalysed - 1) / SinglePageRequestSize > SinglePageRequestNumber)
                  return false; // Returning false halts scanning of sub grids
              }

              // Add the leaf sub grid identified by the address below, along with the production data and surveyed surface
              // flags to the sub grid tree being used to aggregate all the sub grids that need to be queried for the request

              TotalNumberOfSubGridsToRequest++;

              // If a single page of sub grids is being requested determine if the sub grid in question is a 
              // part of the page, and if the page has been filled yet.
              if (CountingRequestsOnly)
                continue;

              // Set the ProdDataMask for the production data
              if (ProdDataSubGrid?.Bits.BitSet(I, J) == true)
              {
                ProdDataMask.SetCell(CastSubGrid.OriginX + I, CastSubGrid.OriginY + J, true);
              }
              else
              {
                // Note: This is ONLY recording the sub grids that have surveyed surface data required, but not production data 
                // as a delta to the production data requests
                SurveyedSurfaceOnlyMask.SetCell(CastSubGrid.OriginX + I, CastSubGrid.OriginY + J, true);
              }
            }
          }
        }
      }

      return true;
    }

    /// <summary>
    /// Counts the number of sub grids that will be submitted to the processing engine given the request parameters
    /// supplied to the request analyzer.
    /// </summary>
    public long CountOfSubGridsThatWillBeSubmitted()
    {
      try
      {
        CountingRequestsOnly = true;

        return Execute() ? TotalNumberOfSubGridsToRequest : -1;
      }
      finally
      {
        CountingRequestsOnly = false;
      }
    }
  }
}
