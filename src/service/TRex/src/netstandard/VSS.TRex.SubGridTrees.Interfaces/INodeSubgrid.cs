﻿using System;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Types;

namespace VSS.TRex.SubGridTrees.Interfaces
{
  public interface INodeSubGrid : ISubGrid
  {
    void DeleteSubGrid(int SubGridX, int SubGridY);

    new ISubGrid GetSubGrid(int X, int Y);

    new void SetSubGrid(int X, int Y, ISubGrid Value);

    ISubGrid GetSubGridContainingCell(uint CellX, uint CellY);

    void ForEachSubGrid(Func<ISubGrid, SubGridProcessNodeSubGridResult> functor);

    void ForEachSubGrid(Func<ISubGrid, SubGridProcessNodeSubGridResult> functor,
      byte minSubGridCellX,
      byte minSubGridCellY,
      byte maxSubGridCellX,
      byte maxSubGridCellY);

    void ForEachSubGrid(Func<byte, byte, ISubGrid, SubGridProcessNodeSubGridResult> functor);

    void ForEachSubGrid(Func<byte, byte, ISubGrid, SubGridProcessNodeSubGridResult> functor,
      byte minSubGridCellX,
      byte minSubGridCellY,
      byte maxSubGridCellX,
      byte maxSubGridCellY);

    bool ScanSubGrids(BoundingIntegerExtent2D Extent,
      Func<ISubGrid, bool> leafFunctor = null,
      Func<ISubGrid, SubGridProcessNodeSubGridResult> nodeFunctor = null);

    int CountChildren();
  }
}
