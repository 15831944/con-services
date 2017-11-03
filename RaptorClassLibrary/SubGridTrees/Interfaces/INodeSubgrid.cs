﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Geometry;
using VSS.VisionLink.Raptor.SubGridTrees.Types;

namespace VSS.VisionLink.Raptor.SubGridTrees.Interfaces
{
    public interface INodeSubGrid : ISubGrid
    {
        void DeleteSubgrid(byte SubGridX, byte SubGridY, bool DeleteIfLocked);

        bool GetSubGridContainingCell(uint CellX, uint CellY, out byte SubGridX, out byte SubGridY);

        void ForEachSubGrid(Func<ISubGrid, SubGridProcessNodeSubGridResult> functor,
            byte minSubGridCellX = 0,
            byte minSubGridCellY = 0,
            byte maxSubGridCellX = SubGridTree.SubGridTreeDimensionMinus1,
            byte maxSubGridCellY = SubGridTree.SubGridTreeDimensionMinus1);

        void ForEachSubGrid(Func<byte, byte, ISubGrid, SubGridProcessNodeSubGridResult> functor,
            byte minSubGridCellX = 0,
            byte minSubGridCellY = 0,
            byte maxSubGridCellX = SubGridTree.SubGridTreeDimensionMinus1,
            byte maxSubGridCellY = SubGridTree.SubGridTreeDimensionMinus1);

        bool ScanSubGrids(BoundingIntegerExtent2D Extent,
                          Func<ISubGrid, bool> leafFunctor = null,
                          Func<ISubGrid, SubGridProcessNodeSubGridResult> nodeFunctor = null);

        int CountChildren();
    }
}
