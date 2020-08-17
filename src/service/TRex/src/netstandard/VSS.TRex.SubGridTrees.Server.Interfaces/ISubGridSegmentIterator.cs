﻿using System;
using System.Collections;
using VSS.TRex.Storage.Interfaces;

namespace VSS.TRex.SubGridTrees.Server.Interfaces
{
    public interface ISubGridSegmentIterator
    {
        int CurrentSegmentIndex { get; }
        ISubGridCellPassesDataSegment CurrentSubGridSegment { get; set; }
        ISubGridDirectory Directory { get; set; }
        bool IsFirstSegmentInTimeOrder { get; }
        IterationDirection IterationDirection { get; set; }
        IIteratorStateIndex IterationState { get; }
        int NumberOfSegmentsScanned { get; set; }
        bool RetrieveAllPasses { get; set; }
        bool RetrieveLatestData { get; set; }
        bool ReturnCachedItemsOnly { get; set; }
        bool ReturnDirtyOnly { get; set; }

        IStorageProxy StorageProxyForSubGridSegments { get; }

        IServerLeafSubGrid SubGrid { get; set; }

        void InitialiseIterator();
        bool MoveNext();
        bool MoveToFirstSubGridSegment();
        bool MoveToNextSubGridSegment();
        void SegmentListExtended();
        void SetIteratorElevationRange(double minElevation, double maxElevation);
        void SetTimeRange(DateTime startTime, DateTime endTime);
        void SetMachineRestriction(BitArray machineIdSet);
    }
}
