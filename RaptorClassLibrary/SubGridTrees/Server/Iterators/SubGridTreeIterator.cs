﻿
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Interfaces;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;
using VSS.VisionLink.Raptor.SubGridTrees.Utilities;

namespace VSS.VisionLink.Raptor.SubGridTrees.Server.Iterators
{
    /*
    This unit implements an iterator for subgrid trees.It's purpose is to allow iteration
    over the sub grids in the same was as the scansubgrid() methods on the TSubGridTree
    descendants, except that mediation of the scanning is not performed by the SubGridTree
    class, but by the iterator.

    This iterator allows a separate thread to perform a scan over a subgrid tree without
    causing significant thread interoperation issues as the scanning state is separate
    */

    // TSubGridTreeIteratorStateIndex records iteration progress across a subgrid
    public struct SubGridTreeIteratorStateIndex
    {
        ISubGrid subGrid;

        private void SetSubGrid(ISubGrid Value)
        {
            subGrid = Value;

            XIdx = -1;
            YIdx = -1;
        }

        // The current X/Y index of the cell at this point in the iteration
        public int XIdx;
        public int YIdx;

        // The subgrid (at any level) being iterated across
        public ISubGrid SubGrid { get { return subGrid; } set { SetSubGrid(value); } }

        public void Initialise()
        {
            SubGrid = null;
            XIdx = -1;
            YIdx = -1;
        }

        public bool NextCell()
        {
            if (YIdx == -1)
            {
                YIdx = 0;
            }

            XIdx++;
            if (XIdx == SubGridTree.SubGridTreeDimension)
            {
                YIdx++;
                XIdx = 0;
            }

            return YIdx < SubGridTree.SubGridTreeDimension;
        }

        public bool AtLastCell() => (XIdx >= SubGridTree.SubGridTreeDimensionMinus1) && (YIdx >= SubGridTree.SubGridTreeDimensionMinus1);
    }

    public class SubGridEnumerator
    {
        public SubGridTreeIterator Iterator { get; set; } = null;

        public ISubGrid GetCurrent() => Iterator.CurrentSubGrid;

        public bool MoveNext() => Iterator.ReturnedFirstItemInIteration ? Iterator.MoveToNextSubGrid() : Iterator.MoveToFirstSubGrid();

        public ISubGrid Current { get { return GetCurrent(); } }
    }

    public class SubGridTreeIterator
    {
        // FGrid is the grid that the iterator will return subgrids from. If a scanner is set then
        // the grid will be taken from the supplied scanner
        public ISubGridTree Grid { get; set; } = null;

        // FScanner is the scanning delegate through which additional control may be excecised
        // over pruning of the nodes and leaves that will be scanned
        public SubGridTreeScannerBase Scanner { get; set; } = null;

        // FCurrent is a reference to the current subgrid that the iterator is currently
        // up to in the sub grid tree scan. By definition, this subgrid is locked for
        // the period that FCurrent references it.
        public ISubGrid CurrentSubGrid { get; set; } = null;

        // IterationState records the progress of the iteration by recording the path through
        // the subgrid tree which marks the progress of the iteration
        SubGridTreeIteratorStateIndex[] iterationState = new SubGridTreeIteratorStateIndex[SubGridTree.SubGridTreeLevels];

        IStorageProxy[] SpatialStorageProxy = null;

        // FStorageClasses controls the storage classes that will be retrieved from the database
        // for each subgrid in the iteration
        //   FStorageClasses : TICSubGridCellStorageClasses;

        // FLockToken is an optional locktoken supplied by the owner of the iterator.
        // If supplied, the iterator will lock each subgrid returned as a part of the
        // iteration set until the next subgrid is selected in the iteration, or the
        // iterator is destroyed
        //    FLockToken : Integer;

        // SubGridsInServerDiskStore indicates that the subgrid tree we are iterating
        // over is persistently stored on disk. In this case we must use the server
        // interface for accessing these subgrids. If this property is false then the
        // subgrid tree is completely self-contained
        private bool SubGridsInServerDiskStore;

        // ReturnCachedItemsOnly allows the caller of the iterator to restrict the
        // iteration to the items that are currently in the cache.
        private bool returnCachedItemsOnly = false;

        // ReturnedFirstItemInIteration keeps track of whether the first item in the iteration
        // has been returned to the caller.
        public bool ReturnedFirstItemInIteration = false;

        //    FDataStoreCache: TICDataStoreCache;

        private void SetCurrentSubGrid(ISubGrid Value)
        {
            /* TODO ... Locking semantics not defined for Ignite
            if (FLockToken != -1 && CurrentSubGrid != null &&
            CurrentSubGrid.Locked && CurrentSubGrid.LockToken == FLockToken)
            {
            //      SIGLogMessage.PublishNoODS(Self, Format('TSubGridTreeIteratorBase.SetCurrentSubGrid: Unlocking %s with lock token %d (%d)', [FCurrentSubGrid.Moniker, FCurrentSubGrid.LockToken, FLockToken]), slmcDebug);
            CurrentSubGrid.ReleaseLock(FLockToken);
            }
            */

            CurrentSubGrid = Value;

            /*
            if (FLockToken != -1 && CurrentSubGrid != null)
              {
                //      SIGLogMessage.PublishNoODS(Self, Format('TSubGridTreeIteratorBase.SetCurrentSubGrid: Locking %s with lock token %d (%d)', [FCurrentSubGrid.Moniker, FCurrentSubGrid.LockToken, FLockToken]), slmcDebug);
                CurrentSubGrid.AcquireLock(FLockToken);
            }
            */
        }

        protected void InitialiseIterator()
        {
            for (int I = 1; I < SubGridTree.SubGridTreeLevels; I++)
            {
                iterationState[I].Initialise();
            }

            if (Scanner != null)
            {
                iterationState[1].SubGrid = Scanner.Grid.Root;
            }
            else
            {
                if (Grid != null)
                {
                    iterationState[1].SubGrid = Grid.Root;
                }
                else
                {
                    Debug.Assert(false, "Subgrid iterators require either a scanner or a grid to work from");
                }
            }
        }

        protected virtual ISubGrid GetCurrentSubGrid() => CurrentSubGrid;

        public SubGridTreeIterator(IStorageProxy[] spatialStorageProxy,
                                   bool subGridsInServerDiskStore)
        {
            //            FDataStoreCache = ADataStoreCache;
            //          FStorageClasses = [icsscAllPasses];
            //          FLockToken = -1;

            SubGridsInServerDiskStore = subGridsInServerDiskStore;
            SpatialStorageProxy = spatialStorageProxy;
        }

        public void CurrentSubgridDestroyed() => CurrentSubGrid = null;

        public SubGridEnumerator GetEnumerator()
        {
            CurrentSubGrid = null;
            return new SubGridEnumerator() { Iterator = this };
        }

        protected ISubGrid LocateNextSubgridInIteration()
        {
            int LevelIdx = 1;
            ISubGrid SubGrid;
            bool AllowedToUseSubgrid;
            SubGridTreeBitmapSubGridBits DummyExistanceMap = new SubGridTreeBitmapSubGridBits(SubGridTreeBitmapSubGridBits.SubGridBitsCreationOptions.Unfilled);

            ISubGrid Result = null;

            Debug.Assert(iterationState[1].SubGrid != null, "No root subgrid node assigned to iteration state");

            // Scan the levels in the iteration state until we find the lowest one we are up to
            // (This is identified by a null subgrid reference.

            while (LevelIdx < SubGridTree.SubGridTreeLevels && iterationState[LevelIdx].SubGrid != null)
            {
                LevelIdx++;
            }

            // Pop up to the previous level as that is the level from which we can choose a subgrid to desend into
            LevelIdx--;

            // Locate the next subgrid node...
            do
            {
                while (iterationState[LevelIdx].NextCell()) // do
                {
                    if (LevelIdx == SubGridTree.SubGridTreeLevels - 1)
                    {
                        // It's a leaf subgrid we are looking for - check the existance map
                        if (SubGridsInServerDiskStore)
                        {
                            if (returnCachedItemsOnly)
                            {
                                SubGrid = iterationState[LevelIdx].SubGrid.GetSubGrid((byte)iterationState[LevelIdx].XIdx, (byte)iterationState[LevelIdx].YIdx);
                            }
                            else
                            {
                                uint CellX = (uint)(iterationState[LevelIdx].SubGrid.OriginX + iterationState[LevelIdx].XIdx * SubGridTree.SubGridTreeDimension);
                                uint CellY = (uint)(iterationState[LevelIdx].SubGrid.OriginY + iterationState[LevelIdx].YIdx * SubGridTree.SubGridTreeDimension);

                                SubGrid = SubGridUtilities.LocateSubGridContaining
                                           (//SpatialStorageProxy[SubGridCellAddress.ToSpatialDivisionDescriptor(CellX, CellY, RaptorConfig.numSpatialProcessingDivisions)],
                                            iterationState[LevelIdx].SubGrid.Owner as ServerSubGridTree,
                                            //null, //FDataStoreCache,
                                            CellX, CellY,
                                            SubGridTree.SubGridTreeLevels,
                                            -1 /*FLockToken*/, false, false);
                            }
                        }
                        else
                        {
                            SubGrid = iterationState[LevelIdx].SubGrid.GetSubGrid((byte)iterationState[LevelIdx].XIdx, (byte)iterationState[LevelIdx].YIdx);
                        }
                    }
                    else
                    {
                        SubGrid = iterationState[LevelIdx].SubGrid.GetSubGrid((byte)iterationState[LevelIdx].XIdx, (byte)iterationState[LevelIdx].YIdx);
                    }

                    if (SubGrid != null)
                    {
                        AllowedToUseSubgrid = false;

                        // Are we allowed to do anything with it?
                        if (SubGrid.IsLeafSubGrid())
                        {
                            AllowedToUseSubgrid = Scanner == null || Scanner.OnProcessLeafSubgrid(SubGrid);
                        }
                        else
                        {
                            if (Scanner != null)
                            {
                                switch (Scanner.OnProcessNodeSubgrid(SubGrid, DummyExistanceMap))
                                {
                                    case Types.SubGridProcessNodeSubGridResult.OK:
                                        AllowedToUseSubgrid = true;
                                        break;

                                    case Types.SubGridProcessNodeSubGridResult.DontDescendFurther:
                                        break;

                                    case Types.SubGridProcessNodeSubGridResult.TerminateProcessing:
                                        Scanner.Abort();
                                        return Result;
                                }
                            }
                            else
                            {
                                AllowedToUseSubgrid = true;
                            }
                        }

                        if (!AllowedToUseSubgrid)
                        {
                            continue;
                        }

                        if (SubGrid.IsLeafSubGrid())
                        {
                            // It's a leaf subgrid - so use it
                            return SubGrid;
                        }
                        else
                        {
                            // It's a node subgrid that contains other node subgrids or leaf subgrids - descend into it
                            LevelIdx++;
                            iterationState[LevelIdx].SubGrid = SubGrid;
                        }
                    }
                }

                LevelIdx--;
            } while (LevelIdx > 0);

            return Result;
        } 

       // MoveToFirstSubGrid moves to the first subgrid in the tree that satisfies the scanner
        public bool MoveToFirstSubGrid()
        {
            InitialiseIterator();

            ReturnedFirstItemInIteration = true;

            return MoveToNextSubGrid();
        }

        // MoveToNextSubGrid moves to the next subgrid in the tree that satisfies the scanner
        public bool MoveToNextSubGrid()
        {
            ISubGrid SubGrid;

            bool Result = false;

            if (!ReturnedFirstItemInIteration)
            {
                MoveToFirstSubGrid();
            }

            do
            {
                SubGrid = LocateNextSubgridInIteration();

                if (SubGrid == null) // We are at the end of the iteration
                {
                    CurrentSubGrid = null;
                    return false;
                }

                Result = Scanner == null || (Scanner.OnProcessLeafSubgrid(SubGrid) && !Scanner.Aborted);
            } while (!Result && !(Scanner != null && Scanner.Aborted));

            CurrentSubGrid = Result ? SubGrid : null;

            return Result;
        }

        public void Reset() => ReturnedFirstItemInIteration = false;

    }
}
