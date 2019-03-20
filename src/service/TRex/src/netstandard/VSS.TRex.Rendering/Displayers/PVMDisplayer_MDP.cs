﻿using System.Drawing;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Rendering.Displayers
{
  /// <summary>
  /// Plan View Map displayer renderer for MDP information presented as rendered tiles
  /// </summary>
  public class PVMDisplayer_MDP : PVMDisplayerBase
  {
    /// <summary>
    /// The flag is to indicate whether or not the machine MDP target to be user overrides.
    /// </summary>
    private const bool UseMachineTargetMDP = false;

    /// <summary>
    /// Default overriding MDP target value.
    /// </summary>
    private const short AbsoluteTargetMDP = 50;

    /// <summary>
    /// Renders MDP summary data as tiles. 
    /// </summary>
    /// <param name="subGrid"></param>
    /// <returns></returns>
    protected override bool DoRenderSubGrid<T>(ISubGrid subGrid)
    {
      return base.DoRenderSubGrid<ClientMDPLeafSubGrid>(subGrid);
    }

    /// <summary>
    /// Queries the data at the current cell location and determines the colour that should be displayed there.
    /// </summary>
    /// <returns></returns>
    protected override Color DoGetDisplayColour()
    {
      var cellValue = ((ClientMDPLeafSubGrid)SubGrid).Cells[east_col, north_row];

      if (cellValue.MeasuredMDP == CellPassConsts.NullMDP)
        return Color.Empty;

      var targetMDPValue = cellValue.TargetMDP;

      // If we are not using the machine target MDP value then we need to replace the
      // target MDP report from the machine, with the override value specified in the options
      if (!UseMachineTargetMDP)
        targetMDPValue = AbsoluteTargetMDP;

      return ((MDPPalette)Palette).ChooseColour(cellValue.MeasuredMDP, targetMDPValue);
    }
  }
}
