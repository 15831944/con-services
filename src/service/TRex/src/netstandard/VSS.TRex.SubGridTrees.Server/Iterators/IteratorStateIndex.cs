﻿using System;
using System.Collections;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.Common.Utilities;

namespace VSS.TRex.SubGridTrees.Server.Iterators
{
    /// <summary>
    /// TSubGridSegmentIteratorStateIndex records iteration progress across a sub grid
    /// </summary>
    public class IteratorStateIndex : IIteratorStateIndex
    {
        public DateTime StartSegmentTime { get; set; } = Consts.MIN_DATETIME_AS_UTC;
        public DateTime EndSegmentTime { get; set; } = Consts.MAX_DATETIME_AS_UTC;

        public IterationDirection IterationDirection { get; set; } = IterationDirection.Forwards;

        public IServerLeafSubGrid SubGrid { get; set; }

        private ISubGridDirectory _Directory;

        public ISubGridDirectory Directory
        {
            get => _Directory;
            
            set
            {
                _Directory = value;

                if (_Directory != null)
                {
                    InitialNumberOfSegments = _Directory.SegmentDirectory.Count;
                }
            }
        }

        private int InitialNumberOfSegments;

        public double MinIterationElevation { get; set; } = Consts.NullDouble;
        public double MaxIterationElevation { get; set; } = Consts.NullDouble;

        private bool RestrictSegmentIterationBasedOnElevationRange;

        // The current index of the segment at this point in the iteration
        public int Idx { get; set; }

        public bool HasMachineRestriction = false;

        public BitArray MachineIDSet { get; set; }

        // The sub grid whose segments are being iterated across

        public void Initialise()
        {
            InitialNumberOfSegments = _Directory.SegmentDirectory.Count;

            Idx = IterationDirection == IterationDirection.Forwards ? -1 : InitialNumberOfSegments;
        }

        public bool NextSegment()
        {
            bool Result;

            if (InitialNumberOfSegments != _Directory.SegmentDirectory.Count)
                throw new TRexSubGridProcessingException("Number of segments in sub grid has changed since the iterator was initialised");

            do
            {
                Idx = IterationDirection == IterationDirection.Forwards ? ++Idx : --Idx;
                bool SegmentIndexInRange = Range.InRange(Idx, 0, _Directory.SegmentDirectory.Count - 1);

                if (!SegmentIndexInRange)
                  return false;

                var SegmentInfo = _Directory.SegmentDirectory[Idx];

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
                             Range.InRange(MinIterationElevation, SegmentInfo.MinElevation,
                               SegmentInfo.MaxElevation) ||
                             Range.InRange(MaxIterationElevation, SegmentInfo.MinElevation, SegmentInfo.MaxElevation);
                  }
                  else if (SegmentInfo.Segment?.PassesData != null)
                  {
                    // The elevation range information we use here is accessed via
                    // the entropic compression information used to compress the attributes held
                    // in the segment. If the segment has not been loaded yet then this information
                    // is not available. In this case don't perform the test, but allow the segment
                    // to be loaded and the passes in it processed according to the current filter.
                    // If the segment has been loaded then access this information and determine
                    // if there is any need to extract cell passes from this segment. If not, just move
                    // to the next segment

                    SegmentInfo.Segment.PassesData.GetSegmentElevationRange(out double SegmentMinElev,
                      out double SegmentMaxElev);
                    if (SegmentMinElev != Consts.NullDouble && SegmentMaxElev != Consts.NullDouble)
                    {
                      // Save the computed elevation range values for this segment
                      SegmentInfo.MinElevation = SegmentMinElev;
                      SegmentInfo.MaxElevation = SegmentMaxElev;

                      Result =
                        Range.InRange(SegmentInfo.MinElevation, MinIterationElevation, MaxIterationElevation) ||
                        Range.InRange(SegmentInfo.MaxElevation, MinIterationElevation, MaxIterationElevation) ||
                        Range.InRange(MinIterationElevation, SegmentInfo.MinElevation, SegmentInfo.MaxElevation) ||
                        Range.InRange(MaxIterationElevation, SegmentInfo.MinElevation, SegmentInfo.MaxElevation);
                    }
                    else
                    {
                      Result = false;
                    }
                  }

                  if (Result && HasMachineRestriction && SegmentInfo.Segment?.PassesData != null)
                  {
                    // Check to see if this segment has any machines that match the
                    // machine restriction. If not, advance to the next segment
                    bool HasMachinesOfInterest = false;
                    var segmentMachineIDSet = SegmentInfo.Segment.PassesData.GetMachineIDSet();

                    if (segmentMachineIDSet != null)
                    {
                      for (int i = 0; i < MachineIDSet.Count; i++)
                      {
                        HasMachinesOfInterest = MachineIDSet[i] && segmentMachineIDSet[i];
                        if (HasMachinesOfInterest)
                          break;
                      }

                      Result = HasMachinesOfInterest;
                    }
                  }
                }
            }
            while (!Result);

            return true;
        }

        public bool AtLastSegment()
        {
            if (IterationDirection == IterationDirection.Forwards)
            {
                return (Idx >= _Directory.SegmentDirectory.Count - 1) ||
                         (_Directory.SegmentDirectory[Idx + 1].StartTime > EndSegmentTime);
            }

            return (Idx <= 0) || (Directory.SegmentDirectory[Idx - 1].EndTime <= StartSegmentTime);
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

        public void SetMachineRestriction(BitArray machineIDSet)
        {
            MachineIDSet = machineIDSet;
        }

        public void SegmentListExtended()
        {
            if (IterationDirection != IterationDirection.Forwards)
                throw new TRexSubGridProcessingException("Extension of segment list only valid if iterator is traveling forwards through the list");

            // Reset the number of segments now expected in the segment.
            InitialNumberOfSegments = _Directory.SegmentDirectory.Count;
        }
    }
}
