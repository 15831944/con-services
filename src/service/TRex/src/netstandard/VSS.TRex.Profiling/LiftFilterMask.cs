﻿using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Profiling.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Profiling
{
  /// <summary>
  /// Provides support for determining inclusion masks for sub grid cell selection and processing based on spatial, positional
  /// and design based spatial selection criteria from filters
  /// </summary>
  public static class LiftFilterMask<T> where T : class, IProfileCellBase
  {
    // ReSharper disable once StaticMemberInGenericType
    private static readonly ILogger _log = Logging.Logger.CreateLogger("LiftFilterMask");

    private static void ConstructSubGridSpatialAndPositionalMask(ISubGridTree tree,
      SubGridCellAddress currentSubGridOrigin,
      List<T> profileCells, 
      SubGridTreeBitmapSubGridBits mask,
      int fromProfileCellIndex, 
      ICellSpatialFilter cellFilter)
    {
      mask.Clear();

      // From current position to end...
      for (var cellIdx = fromProfileCellIndex; cellIdx < profileCells.Count; cellIdx++)
      {
        var profileCell = profileCells[cellIdx];
        var thisSubGridOrigin = new SubGridCellAddress(
          profileCell.OTGCellX & ~SubGridTreeConsts.SubGridLocalKeyMask,
          profileCell.OTGCellY & ~SubGridTreeConsts.SubGridLocalKeyMask);

        if (!currentSubGridOrigin.Equals(thisSubGridOrigin))
          break;

        var cellX = (byte)(profileCell.OTGCellX & SubGridTreeConsts.SubGridLocalKeyMask);
        var cellY = (byte)(profileCell.OTGCellY & SubGridTreeConsts.SubGridLocalKeyMask);

        if (cellFilter.HasSpatialOrPositionalFilters)
        {
          tree.GetCellCenterPosition(profileCell.OTGCellX, profileCell.OTGCellY,
            out var cellCenterX, out var cellCenterY);
          if (cellFilter.IsCellInSelection(cellCenterX, cellCenterY))
            mask.SetBit(cellX, cellY);
        }
        else
          mask.SetBit(cellX, cellY);
      }
    }

    public static bool ConstructSubGridCellFilterMask(ISiteModel siteModel,
      SubGridCellAddress currentSubGridOrigin,
      List<T> profileCells,
      SubGridTreeBitmapSubGridBits mask,
      int fromProfileCellIndex,
      ICellSpatialFilter cellFilter,
      IDesign surfaceDesignMaskDesign)
    {
      ConstructSubGridSpatialAndPositionalMask(siteModel.Grid, currentSubGridOrigin, profileCells, mask, fromProfileCellIndex, cellFilter);

      // If the filter contains an alignment design mask filter then compute this and AND it with the
      // mask calculated in the step above to derive the final required filter mask

      if (cellFilter.HasAlignmentDesignMask())
      {
        if (cellFilter.AlignmentFence.IsNull()) // Should have been done in ASNode but if not
          throw new ArgumentException($"Spatial filter does not contained pre-prepared alignment fence for design {cellFilter.AlignmentDesignMaskDesignUID}");

        var tree = siteModel.Grid;

        // Go over set bits and determine if they are in Design fence boundary
        mask.ForEachSetBit((X, Y) =>
        {
          tree.GetCellCenterPosition(currentSubGridOrigin.X + X, currentSubGridOrigin.Y + Y, out var cx, out var cy);
          if (!cellFilter.AlignmentFence.IncludesPoint(cx, cy))
          {
            mask.ClearBit(X, Y); // remove interest as its not in design boundary
          }
        });
      }

      if (surfaceDesignMaskDesign != null)
      {
        var getFilterMaskResult = surfaceDesignMaskDesign.GetFilterMaskViaLocalCompute(siteModel, currentSubGridOrigin, siteModel.CellSize);

        if (getFilterMaskResult.errorCode == DesignProfilerRequestResult.OK || getFilterMaskResult.errorCode == DesignProfilerRequestResult.NoElevationsInRequestedPatch)
        {
          if (getFilterMaskResult.filterMask == null)
          {
            _log.LogWarning("FilterMask null in response from surfaceDesignMaskDesign.GetFilterMask, ignoring it's contribution to filter mask");
          }
          else
          {
            mask.AndWith(getFilterMaskResult.filterMask);
          }
        }
        else
        {
          _log.LogError($"Call (B2) to {nameof(ConstructSubGridCellFilterMask)} returned error result {getFilterMaskResult.errorCode} for {cellFilter.SurfaceDesignMaskDesignUid}");
          return false;
        }
      }

      return true;
    }

    public static (bool executionResult, DesignProfilerRequestResult filterDesignErrorCode)
      InitialiseFilterContext(ISiteModel siteModel, ICellPassAttributeFilter passFilter,
        ICellPassAttributeFilterProcessingAnnex passFilterAnnex, ProfileCell profileCell, IDesignWrapper passFilterElevRangeDesign)
    {
      (bool executionResult, DesignProfilerRequestResult filterDesignErrorCode) result = (false, DesignProfilerRequestResult.UnknownError);

      // If the elevation range filter uses a design then the design elevations
      // for the sub grid need to be calculated and supplied to the filter

      if (passFilter.HasElevationRangeFilter && passFilterElevRangeDesign != null)
      {
        var getDesignHeightsResult = passFilterElevRangeDesign.Design.GetDesignHeightsViaLocalCompute(siteModel, passFilterElevRangeDesign.Offset, new SubGridCellAddress(profileCell.OTGCellX, profileCell.OTGCellY), siteModel.CellSize);

        result.filterDesignErrorCode = getDesignHeightsResult.errorCode;

        if (result.filterDesignErrorCode != DesignProfilerRequestResult.OK || getDesignHeightsResult.designHeights == null)
        {
          if (result.filterDesignErrorCode == DesignProfilerRequestResult.NoElevationsInRequestedPatch)
            _log.LogInformation(
              "Lift filter by design. Call to RequestDesignElevationPatch failed due to no elevations in requested patch.");
          else
            _log.LogWarning(
              $"Lift filter by design. Call to RequestDesignElevationPatch failed due to no TDesignProfilerRequestResult return code {result.filterDesignErrorCode}.");
          return result;
        }

        passFilterAnnex.InitializeElevationRangeFilter(passFilter, getDesignHeightsResult.designHeights.Cells);
      }

      result.executionResult = true;
      return result;
    }
  }
}
