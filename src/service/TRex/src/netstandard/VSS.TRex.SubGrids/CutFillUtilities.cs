﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Core.Utilities;

namespace VSS.TRex.SubGrids
{
  /// <summary>
  /// Contains methods relevant to supporting Cut/Fill operations, such a computing cut/fill elevation sub grids
  /// </summary>
  public static class CutFillUtilities
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger("CutFillUtilities");

    /// <summary>
    /// Calculates a cut/fill sub grid from a production data elevation sub grid and an elevation sub grid computed from a referenced design,
    /// replacing the elevations in the first sub grid with the resulting cut fill values
    /// </summary>
    public static (bool executionResult, DesignProfilerRequestResult profilerRequestResult) ComputeCutFillSubGrid(ISiteModel siteModel, IClientLeafSubGrid SubGrid, IDesignWrapper designWrapper)
    {
      (bool executionResult, DesignProfilerRequestResult profilerRequestResult) result = (false, DesignProfilerRequestResult.UnknownError);

      if (designWrapper?.Design == null)
        return result;

      var getDesignHeightsResult = designWrapper.Design.GetDesignHeightsViaLocalCompute(siteModel, designWrapper.Offset, SubGrid.OriginAsCellAddress(), SubGrid.CellSize);

      result.profilerRequestResult = getDesignHeightsResult.errorCode;

      if (result.profilerRequestResult != DesignProfilerRequestResult.OK && result.profilerRequestResult != DesignProfilerRequestResult.NoElevationsInRequestedPatch)
      {
        _log.LogError($"Design profiler sub grid elevation request for {SubGrid.OriginAsCellAddress()} failed with error {result.profilerRequestResult}");
        return result;
      }

      ComputeCutFillSubGrid((IClientHeightLeafSubGrid) SubGrid, getDesignHeightsResult.designHeights);

      result.executionResult = true;
      return result;
    }

    /// <summary>
    /// Calculates a cut/fill sub grid from two elevation sub grids, replacing the elevations
    /// in the first sub grid with the resulting cut fill values
    /// </summary>
    public static void ComputeCutFillSubGrid(IClientHeightLeafSubGrid subGrid1, IClientHeightLeafSubGrid subGrid2)
    {
      SubGridUtilities.SubGridDimensionalIterator((I, J) =>
      {
        if (subGrid1.Cells[I, J] != Consts.NullHeight)
        {
          if (subGrid2.Cells[I, J] != Consts.NullHeight)
            subGrid1.Cells[I, J] -= subGrid2.Cells[I, J];
          else
            subGrid1.Cells[I, J] = Consts.NullHeight;
        }
      });
    } 
  }
}
