﻿using System;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SubGridTrees.Core.Utilities;

namespace VSS.TRex.SubGridTrees.Server.Utilities
{
    public static class SegmentCellPassAdopter
    {
        /// <summary>
        /// Causes segment to adopt all cell passes from sourceSegment where those cell passes were 
        /// recorded at or later than a specific date
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="sourceSegment"></param>
        /// <param name="atAndAfterTime"></param>
        public static void AdoptCellPassesFrom(ISubGridCellSegmentPassesDataWrapper segment,
            ISubGridCellSegmentPassesDataWrapper sourceSegment,
            DateTime atAndAfterTime)
        {
            SubGridUtilities.SubGridDimensionalIterator((i, j) =>
            {
                int thePassCount = (int)sourceSegment.PassCount(i, j);

                if (thePassCount == 0)
                    return;

                int countInCell = 0;

                for (uint PassIndex = 0; PassIndex < thePassCount; PassIndex++)
                {
                    if (sourceSegment.PassTime(i, j, PassIndex) < atAndAfterTime)
                        countInCell++;
                    else
                        break; // No more passes in the cell will satisfy the condition
                }

                // countInCell represents the number of cells that should remain in the
                // source segment. The remainder are to be moved to this segment

                int adoptedPassCount = thePassCount - countInCell;
                if (adoptedPassCount > 0)
                {
                    // Copy the adopted passes from the 'from' cell to the 'to' cell
                    uint OldCellPassCount = segment.PassCount(i, j);

                    for (uint PassIndex = OldCellPassCount; PassIndex < OldCellPassCount + adoptedPassCount; PassIndex++)
                        segment.AddPass(i, j, sourceSegment.Pass(i, j, PassIndex));

                    // Set the new number of passes and reset the length of the cell passes
                    // in the cell the passes were adopted from
                    sourceSegment.SegmentPassCount -= adoptedPassCount;
                    sourceSegment.AllocatePasses(i, j, (uint)countInCell);
                }
            });
        }
    }
}
