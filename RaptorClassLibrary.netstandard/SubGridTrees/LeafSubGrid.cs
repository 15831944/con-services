﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;

namespace VSS.VisionLink.Raptor.SubGridTrees
{
    /// <summary>
    /// LeafSubgrid is the true base class from which to derive varieties of leaf subgrid that support different data types
    /// and use cases.
    /// </summary>
    public class LeafSubGrid : LeafSubGridBase, ILeafSubGrid
    {
        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public LeafSubGrid()
        {

        }

        public LeafSubGrid(ISubGridTree owner,
                           ISubGrid parent,
                           byte level) : base(owner, parent, level)
        {
            // Assert level = tree.NumLevels (leaves are only at the tips)
            if (owner != null && level != owner.NumLevels)
            {
                throw new ArgumentException("Requested level for leaf subgrid <> number of levels in tree", "level");
            }
        }

        public override bool IsEmpty()
        {
            for (byte I = 0; I < SubGridTree.SubGridTreeDimension; I++)
            {
                for (byte J = 0; J < SubGridTree.SubGridTreeDimension; J++)
                {
                    if (CellHasValue(I, J))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
