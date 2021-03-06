﻿using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.SubGridTrees.Types
{
  /// <summary>
  /// Contains the location of, and reference to, a child cell within this sub grid.
  /// These items form a sparse list of child sub grids that is more space efficient than
  /// maintaining a full 32x32 array of references where the vast majority are null
  /// (no data in child sub grid)
  /// </summary>
  public readonly struct SubGridTreeSparseCellRecord
  {
    /// <summary>
    /// X ordinate of the in sub grid address of the cell
    /// </summary>
    public readonly byte CellX;

    /// <summary>
    /// Y ordinate of the in sub grid address of the cell
    /// </summary>
    public readonly byte CellY;

    /// <summary>
    /// Reference to the cell at the X, Y location
    /// </summary>
    public readonly ISubGrid Cell;

    public SubGridTreeSparseCellRecord(byte cellX, byte cellY, ISubGrid cell)
    {
      CellX = cellX;
      CellY = cellY;
      Cell = cell;
    }

    /// <summary>
    /// Calculate size of this structure. Assume the ISubGrid reference si the size of a long (8 bytes)
    /// </summary>
    public static int SizeOf() => 2 * sizeof(byte) + sizeof(long);
  }
}
