﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Common;
using VSS.VisionLink.Raptor.SiteModels;
using VSS.VisionLink.Raptor.Utilities;

namespace VSS.VisionLink.Raptor.SubGridTrees.Server.Iterators
{
    /// <summary>
    /// TSubGridSegmentIteratorStateIndex records iteration progress across a subgrid
    /// </summary>
    public class IteratorStateIndex
    {
        public DateTime StartSegmentTime = DateTime.MinValue;
        public DateTime EndSegmentTime = DateTime.MaxValue;

        public IterationDirection IterationDirection { get; set; } = Iterators.IterationDirection.Forwards;

        public ServerSubGridTreeLeaf SubGrid { get; set; } = null;

        SubGridDirectory _Directory = null;

        public SubGridDirectory Directory
        {
            get
            {
                return _Directory;
            }
            set
            {
                _Directory = value;

                if (_Directory != null)
                {
                    InitialNumberOfSegments = _Directory.SegmentDirectory.Count;
                }
            }
        }

        int InitialNumberOfSegments = 0;

        public double MinIterationElevation = Consts.NullDouble;
        public double MaxIterationElevation = Consts.NullDouble;

        bool RestrictSegmentIterationBasedOnElevationRange = false;

        // The current index of the segment at this point in the iteration
        public int Idx = 0;

        public bool HasMachineRestriction = false;

        // Machine retriction not implemented
        // byte[] MachineIDSets { get; set; } = null;

        public SiteModel SiteModelReference { get; set; } = null;

        // The subgrid whose segments are being iterated across

        public void Initialise()
        {
            InitialNumberOfSegments = _Directory.SegmentDirectory.Count;

            Idx = IterationDirection == IterationDirection.Forwards ? -1 : InitialNumberOfSegments;
        }

        public bool NextSegment()
        {
            //{$IFDEF STATIC_CELL_PASSES}
            //HasMachinesOfInterest: Boolean;
            //{$ENDIF}
            SubGridCellPassesDataSegmentInfo SegmentInfo;
            bool SegmentIndexInRange;
            // double SegmentMinElev, SegmentMaxElev;

            bool Result = false;

            if (InitialNumberOfSegments != _Directory.SegmentDirectory.Count)
            {
                // TODO add when login avbailable
                // SIGLogMessage.PublishNoODS(Nil, 'Number of segments in subgrid has changed since the iterator was initialised', slmcAssert);
                return false;
            }

            do
            {
                Idx = IterationDirection == IterationDirection.Forwards ? Idx + 1 : Idx - 1;

                SegmentIndexInRange = Range.InRange(Idx, 0, _Directory.SegmentDirectory.Count - 1);

                if (!SegmentIndexInRange)
                {
                    return false;
                }

                SegmentInfo = _Directory.SegmentDirectory[Idx];

                Result = Range.InRange(SegmentInfo.StartTime, StartSegmentTime, EndSegmentTime) ||
                         Range.InRange(SegmentInfo.EndTime, StartSegmentTime, EndSegmentTime) ||
                         Range.InRange(StartSegmentTime, SegmentInfo.StartTime, SegmentInfo.EndTime) ||
                         Range.InRange(EndSegmentTime, SegmentInfo.StartTime, SegmentInfo.EndTime);

                // If there is an elevation range restriction is place then check to see if the
                // segment contains any cell passes in the elevation range
                if (Result && RestrictSegmentIterationBasedOnElevationRange)
                {
                    if (SegmentInfo.MinElevation != Consts.NullDouble && SegmentInfo.MaxElevation != Consts.NullDouble)
                    {
                        Result = Range.InRange(SegmentInfo.MinElevation, MinIterationElevation, MaxIterationElevation) ||
                                 Range.InRange(SegmentInfo.MaxElevation, MinIterationElevation, MaxIterationElevation) ||
                                 Range.InRange(MinIterationElevation, SegmentInfo.MinElevation, SegmentInfo.MaxElevation) ||
                                 Range.InRange(MaxIterationElevation, SegmentInfo.MinElevation, SegmentInfo.MaxElevation);
                    }
                    else
                      if (SegmentInfo.Segment != null && SegmentInfo.Segment.PassesData != null)

                    {
                        Debug.Assert(false, "Static cell pass information not yet supported");

                        /* TODO
                        // The elevation range information we use here is accessed via
                        // the entropic compression information used to compress the attributes held
                        // in the segment. If the segment has not been loaded yet then this information
                        // is not available. In this case don't perform the test, but allow the segment
                        // to be loaded and the passes in it processed according to the current filter.
                        // If the segment has been loaded then access this information and determine
                        // if there is any need to extract cell passes from this segment. If not, just move
                        // to the next segment

                        SegmentInfo.Segment.PassesData.GetSegmentElevationRange(out SegmentMinElev, out SegmentMaxElev);
                        if (SegmentMinElev != Consts.NullDouble && SegmentMaxElev != Consts.NullDouble)
                        {
                            // Save the computed elevation range values for this segment
                            SegmentInfo.MinElevation = SegmentMinElev;
                            SegmentInfo.MaxElevation = SegmentMaxElev;

                            Result = Range.InRange(SegmentInfo.MinElevation, MinIterationElevation, MaxIterationElevation) ||
                                                Range.InRange(SegmentInfo.MaxElevation, MinIterationElevation, MaxIterationElevation) ||
                                                Range.InRange(MinIterationElevation, SegmentInfo.MinElevation, SegmentInfo.MaxElevation) ||
                                                Range.InRange(MaxIterationElevation, SegmentInfo.MinElevation, SegmentInfo.MaxElevation);
                        }
                        else
                        {
                            Result = false;
                        }
                        */
                    }

                    /* TODO Machine restriction not currently implemented
                                {$IFDEF STATIC_CELL_PASSES}
                                if Result and HasMachineRestriction and Assigned(Segment)and Assigned(Segment.PassesData) then
                                  begin
                                    // Check to see if this segment has any machines that match the
                                    // machine restriction. If not, advance to the next segment
                                    HasMachinesOfInterest:= False;
                                for I := Low(MachineIDSets) to High(MachineIDSets) do
                                        begin
                                          HasMachinesOfInterest := (MachineIDSets[I] * Segment.PassesData.MachineIDSets[I]) <> [];
                                if HasMachinesOfInterest then
                                  Break;
                                end;

                                Result:= HasMachinesOfInterest;
                                end;
                                {$ENDIF}
                    */
                }
            }
            while (!Result);

            return Result;
        }

        public bool AtLastSegment()
        {
            if (IterationDirection == IterationDirection.Forwards)
            {
                return (Idx >= _Directory.SegmentDirectory.Count - 1) ||
                         (_Directory.SegmentDirectory[Idx + 1].StartTime > EndSegmentTime);
            }
            else
            {
                return (Idx <= 0) || (Directory.SegmentDirectory[Idx - 1].EndTime <= StartSegmentTime);
            }
        }

        public void SetTimeRange(DateTime startSegmentTime, DateTime endSegmentTime)
        {
            StartSegmentTime = startSegmentTime;
            EndSegmentTime = endSegmentTime;
        }

        public void SetIteratorElevationRange(double minIterationElevation, double maxIterationElevation)
        {
            MinIterationElevation = minIterationElevation;
            MaxIterationElevation = maxIterationElevation;

            RestrictSegmentIterationBasedOnElevationRange = (minIterationElevation != Consts.NullDouble) && (MaxIterationElevation != Consts.NullDouble);
        }

        public void SetMachineRestriction()
        {
            // Machine restriction not currently implemented  
        }

        public void SegmentListExtended()
        {
            if (IterationDirection != IterationDirection.Forwards)
            {
                // TODO add when logging available
                ///SIGLogMessage.PublishNoODS(Nil, 'Extension of segment list only valid if iterator is travelling forwards through the list', slmcAssert);
                return;
            }

            // Reset the number of segments now expected in the segment.
            InitialNumberOfSegments = _Directory.SegmentDirectory.Count;
        }
    }
}
