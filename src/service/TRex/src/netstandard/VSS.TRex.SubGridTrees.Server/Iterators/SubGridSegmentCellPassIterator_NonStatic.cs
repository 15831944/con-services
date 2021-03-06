﻿using VSS.TRex.Cells;
using VSS.TRex.SubGridTrees.Server.Interfaces;

namespace VSS.TRex.SubGridTrees.Server.Iterators
{
    /// <summary>
    /// Iterates through cells in sub grids in a cell pass by cell pass manner.
    /// This version only operates on non-static cell passes
    /// </summary>
    public class SubGridSegmentCellPassIterator_NonStatic : SubGridSegmentCellPassIterator_Base
    {
        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public SubGridSegmentCellPassIterator_NonStatic()
        {
        }

        /// <summary>
        /// Construct a cell pass iterator using a given segment iterator and an optional maximum number of passes to return
        /// in the course of the iteration
        /// </summary>
        /// <param name="iterator"></param>
        /// <param name="maxNumberOfPassesToReturn"></param>
        public SubGridSegmentCellPassIterator_NonStatic(ISubGridSegmentIterator iterator, int maxNumberOfPassesToReturn = int.MaxValue) : base(iterator, maxNumberOfPassesToReturn)
        {
        }

        /// <summary>
        /// Provides non-static cell pass specific initialization for the next segment
        /// </summary>
        /// <param name="direction"></param>
        protected override void InitialiseForNewSegment(IterationDirection direction)
        {
            if (SegmentIterator.IterationDirection == IterationDirection.Forwards)
            {
                cellInSegmentIndex = -1;
                finishCellInSegmentIndex = SegmentIterator.CurrentSubGridSegment.PassesData.PassCount(CellX, CellY);

                cellPassIterationDirectionIncrement = 1;
            }
            else
            {
                cellInSegmentIndex = SegmentIterator.CurrentSubGridSegment.PassesData.PassCount(CellX, CellY);
                finishCellInSegmentIndex = -1;

                cellPassIterationDirectionIncrement = -1;
            }
        }

        /// <summary>
        /// Provides non-static cell pass extract semantics for the current cell pass in the iteration
        /// </summary>
        /// <returns></returns>
        protected override CellPass ExtractCellPass()
        {
            return SegmentIterator.CurrentSubGridSegment.PassesData.ExtractCellPass(CellX, CellY, cellInSegmentIndex);
        }
    }
}
