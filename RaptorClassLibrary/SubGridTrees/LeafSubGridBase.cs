﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;

namespace VSS.VisionLink.Raptor.SubGridTrees
{
    public class LeafSubGridBase : SubGrid
    {
        /// <summary>
        /// LatestCellPassesOutOfDate notes whether there is 'latest' call pass information that has been changed and 
        /// required persistence.
        /// </summary>
        protected bool latestCellPassesOutOfDate = false;
        public bool LatestCellPassesOutOfDate { get { return latestCellPassesOutOfDate; } }

        public override void SetDirty(bool value)
        {
            Debug.Assert(value = true, "Can only mark subgrid as dirty via public interface (not unset it!)");

            base.SetDirty(value);

            if (Dirty)
            {
                latestCellPassesOutOfDate = true;
            }
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
        public LeafSubGridBase() : base(null, null, SubGridTree.SubGridTreeLevels)
        {

        }
    }
}