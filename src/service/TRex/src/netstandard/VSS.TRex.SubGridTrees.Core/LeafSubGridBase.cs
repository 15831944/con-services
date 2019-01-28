﻿using System.Diagnostics;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.SubGridTrees
{
    /// <summary>
    /// Base class for any sub grid derivative that contains data at the bottom layer of a sub grid tree
    /// </summary>
    public class LeafSubGridBase : SubGrid
    {
        /// <summary>
        /// LatestCellPassesOutOfDate notes whether there is 'latest' call pass information that has been changed and 
        /// required persistence.
        /// </summary>
        protected bool latestCellPassesOutOfDate;
        public bool LatestCellPassesOutOfDate => latestCellPassesOutOfDate;

        public override void SetDirty(bool value)
        {
            Debug.Assert(value, "Can only mark sub grid as dirty via public interface (not unset it!)");

            base.SetDirty(value);

            if (Dirty)
                latestCellPassesOutOfDate = true;
        }

        public LeafSubGridBase(ISubGridTree owner,
                               ISubGrid parent,
                               byte level,
                               double cellSize,
                               int indexOriginOffset) : this(owner, parent, level)
        {
        }

        public LeafSubGridBase(ISubGridTree owner,
                               ISubGrid parent,
                               byte level) : base(owner, parent, level)
        {
        }

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public LeafSubGridBase() : base(null, null, SubGridTreeConsts.SubGridTreeLevels)
        {

        }
    }
}
