﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.Filters
{
    /// <summary>
    /// FilteredValueAssignmentContext provides a context for collecting filtered values from production data in the 
    /// process of applying spatial, temporal and attributal filtering constraints.
    /// </summary>
    public class FilteredValueAssignmentContext
    {
        /// <summary>
        /// Local structure used to define the probe point positions for design lookups during filtering
        /// </summary>
        public struct ProbePoint
        {
            public double XOffset, YOffset;

            public ProbePoint(double xOffset, double yOffset)
            {
                XOffset = xOffset;
                YOffset = yOffset;
            }

            public void SetOffsets(double xOffset, double yOffset)
            {
                XOffset = xOffset;
                YOffset = yOffset;
            }
        }

        public FilteredSinglePassInfo FilteredValue;

        public FilteredSinglePassInfo PreviousFilteredValue;

        //      CellProfile: TICProfileCell;
        //      LiftBuildSettings: TICLiftBuildSettings;


        /// <summary>
        /// ProbePositions is used to store the real world positions used to probe into
        /// cells when computing floating type seived bitmasks during subgrid querying.
        /// The positions are stored as offsets from the real world origin of the subgrid
        /// As many queries do not need access to probe positions the arfray is created
        /// only if InitialiseProbePositions() is called
        /// </summary>
        public ProbePoint[,] ProbePositions = null;

        /// <summary>
        /// Constructor...
        /// </summary>
        public FilteredValueAssignmentContext()
        {
            FilteredValue.Clear();
            PreviousFilteredValue.Clear();
//            CellProfile:= Nil;
//            LiftBuildSettings:= Nil;
        }

        public void InitialiseProbePositions()
        {
            ProbePositions = new ProbePoint[SubGridTree.SubGridTreeDimension, SubGridTree.SubGridTreeDimension];
        }
    }
}
