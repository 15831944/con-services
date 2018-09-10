﻿using System;
using System.Collections;
using Microsoft.Extensions.Logging;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SubGridTrees.Server.Interfaces;

namespace VSS.TRex.SubGridTrees.Server.Iterators
{
    /// <summary>
    /// This supports the idea of iterating through the segments in a subgrid in a way that hides all the details
    /// of how this is done...
    /// </summary>
    public class SubGridSegmentIterator : ISubGridSegmentIterator
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger(nameof(SubGridSegmentIterator));

        // CurrentSubgridSegment is a reference to the current subgrid segment that the iterator is currently
        // up to in the sub grid tree scan. 
        private ISubGridCellPassesDataSegment CurrentSubgridSegment { get; set; } = null;

        // IterationState records the progress of the iteration by recording the path through
        // the subgrid tree which marks the progress of the iteration
        public IIteratorStateIndex IterationState { get; set; } = new IteratorStateIndex();

        public IStorageProxy StorageProxy { get; set; }

        public bool RetrieveLatestData { get; set; } = false;
        public bool RetrieveAllPasses { get; set; } = true;

        protected ISubGridCellPassesDataSegment LocateNextSubgridSegmentInIteration()
        {
            ISubGridCellPassesDataSegment Result = null;

            if (IterationState.SubGrid == null)
            {
                Log.LogCritical("No subgrid node assigned to iteration state");
                return null;
            }

            while (IterationState.NextSegment())
            {
                ISubGridCellPassesDataSegmentInfo SegmentInfo = IterationState.Directory.SegmentDirectory[IterationState.Idx];

                if (SegmentInfo.Segment != null)
                {
                    Result = SegmentInfo.Segment;
                }
                else
                {
                    if (ReturnDirtyOnly)
                    {
                        // if there is no segment present in the cache then it can't be dirty, so is
                        // not a candidate to be returned by the iterator
                        continue;
                    }

                    if (ReturnCachedItemsOnly)
                    {
                        // The caller is only interested in segments that are present in the cache,
                        // so we do not need to read it from the persistent store
                        continue;
                    }

                    // This additional check to determine if the segment is defined
                    // is necesary to check if an earlier thread through this code has
                    // already allocated the new segment
                    if (SegmentInfo.Segment == null)
                    {
                        IterationState.SubGrid.AllocateSegment(SegmentInfo);
                    }

                    Result = SegmentInfo.Segment;

                    if (Result == null)
                    {
                        Log.LogCritical("IterationState.SubGrid.Cells.AllocateSegment failed to create a new segment");
                        return null;
                    }
                }

                if (!Result.Dirty && ReturnDirtyOnly)
                {
                    // The segment is not dirty, and the iterator has been instructed only to return
                    // dirty segments, so ignore this one
                    Result = null;
                    continue;
                }

                // TODO There is no caching layer yet. This will function as if ReturnCachedItemsOnly was set to true for now 
                if (!Result.Dirty && !ReturnCachedItemsOnly && 
                    (RetrieveAllPasses && !Result.HasAllPasses || RetrieveLatestData && !Result.HasLatestData))
                  {
                    // This additional check to determine if the required storage classes
                    // are present is necesary to check if an earlier thread through this code has
                    // already allocated them

                    if (!Result.Dirty && (RetrieveAllPasses && !Result.HasAllPasses || RetrieveLatestData && !Result.HasLatestData))
                    {
                        if ((IterationState.SubGrid.Owner as IServerSubGridTree).LoadLeafSubGridSegment
                            (StorageProxy,
                             new SubGridCellAddress(IterationState.SubGrid.OriginX, IterationState.SubGrid.OriginY),
                             RetrieveLatestData, RetrieveAllPasses, // StorageClasses,
                             IterationState.SubGrid,
                             Result))
                        {
                            /* TODO: no separate cache - it is in ignite
                            // The segment is now loaded and available for use and should be touched
                            // to link it into the cache segment MRU management
                            if (Result != null && Result.Owner.PresentInCache)
                            {
                               DataStoreInstance.GridDataCache.SubgridSegmentTouched(Result);
                            }
                            */
                        }
                        else
                        {
                            /* TODO: no separate cache - it is in ignite
                            // The segment is failed to load, however it may have been created
                            // to hold the data being read. The failure handling will have backtracked
                            // out any allocations made within the segment, but it is safer to include
                            // it into the cache and allow it to be managed there than to
                            // independently remove it here
                            if (Result != null && Result.Owner.PresentInCache)
                            {
                                DataStoreInstance.GridDataCache.SubgridSegmentTouched(Result);
                            }
                            */

                            // Segment failed to be loaded. Multiple messages will have been posted to the log.
                            // Move to the next item in the iteration
                            Result = null; 
                            continue;
                        }
                    }
                }

                if (Result != null)
                {
                    // We have a candidate to return as the next item in the iteration
                    break;
                }
            }
            return Result;
        }

        public ISubGridCellPassesDataSegment CurrentSubGridSegment { get; set; }

        // property StorageClasses : TICSubGridCellStorageClasses read FStorageClasses write FStorageClasses;

        /// <summary>
        /// ReturnDirtyOnly allows the iterator to only return segments in the subgrid that are dirty
        /// </summary>
        public bool ReturnDirtyOnly { get; set; }

        public IterationDirection IterationDirection { get { return IterationState.IterationDirection; } set { IterationState.IterationDirection = value; } }

        /// <summary>
        /// Allows the caller of the iterator to restrict the
        /// iteration to the items that are currently in the cache.
        /// </summary>
        public bool ReturnCachedItemsOnly { get; set; } = false;

        /// <summary>
        ///  The subgrid whose segments are being iterated across
        /// </summary>
        public IServerLeafSubGrid SubGrid
        {
            get
            {
                return IterationState.SubGrid;
            }
            set
            {
                /*
                // Ensure subgrids are either locked, or segments are not required to be notified as
                // touched to the cache (which is the case for subgrids that are in intermediary
                // subgrid trees in TAG file processing)
                if (!(!MarkReturnedSegmentsAsTouched || Value.Locked)))
                {
                    SIGLogMessage.PublishNoODS(Self, Format('Subgrid %s supplied to iterator is not locked', [Value.Moniker]), slmcAssert);
                    return;
                }
                */

                IterationState.SubGrid = value;
            }
        }

        public ISubGridDirectory Directory { get { return IterationState.Directory; } set { IterationState.Directory = value; } }

        public bool MarkReturnedSegmentsAsTouched { get; set; }

        public int NumberOfSegmentsScanned { get; set; }

        public SubGridSegmentIterator(IServerLeafSubGrid subGrid, IStorageProxy storageProxy)
        {
            MarkReturnedSegmentsAsTouched = true;
            SubGrid = subGrid;
            Directory = subGrid?.Directory;
            StorageProxy = storageProxy;
        }

        public SubGridSegmentIterator(IServerLeafSubGrid subgrid, ISubGridDirectory directory, IStorageProxy storageProxy) : this(subgrid, storageProxy)
        {
            Directory = directory;
        }

        public void SetTimeRange(DateTime startTime, DateTime endTime) => IterationState.SetTimeRange(startTime, endTime);

        public bool MoveNext() => CurrentSubGridSegment == null ? MoveToFirstSubGridSegment() : MoveToNextSubGridSegment();

        // MoveToFirstSubGridSegment moves to the first segment in the subgrid
        public bool MoveToFirstSubGridSegment()
        {
            NumberOfSegmentsScanned = 0;

            InitialiseIterator();

            return MoveToNextSubGridSegment();
        }

        // MoveToNextSubGridSegment moves to the next segment in the subgrid
        public bool MoveToNextSubGridSegment()
        {
            ISubGridCellPassesDataSegment SubGridSegment = LocateNextSubgridSegmentInIteration();

            if (SubGridSegment == null) // We are at the end of the iteration
            {
                CurrentSubGridSegment = null;
                return false;
            }

            CurrentSubGridSegment = SubGridSegment;

            NumberOfSegmentsScanned++;

            return true;
        }

        public void CurrentSubgridSegmentDestroyed() => CurrentSubGridSegment = null;

        public void InitialiseIterator() => IterationState.Initialise();

        public bool IsFirstSegmentInTimeOrder => IterationState.Idx == 0;

        // SegmentListExtended advises the iterator that the segment list has grown to include
        // new segments. The mening of this operation is that the segment the iterator is
        // currently pointingh at is still valid, but some number of segments have been
        // inserted into the list of segments. The iterator should continue from the current
        // iterator location in the list, but note the additional number of sements in the list.
        public void SegmentListExtended() => IterationState.SegmentListExtended();

        public int CurrentSegmentIndex => IterationState.Idx;

        public void SetIteratorElevationRange(double minElevation, double maxElevation) => IterationState.SetIteratorElevationRange(minElevation, maxElevation);

        public void SetMachineRestriction(BitArray machineIDSet) => IterationState.SetMachineRestriction(machineIDSet);
    }
}
