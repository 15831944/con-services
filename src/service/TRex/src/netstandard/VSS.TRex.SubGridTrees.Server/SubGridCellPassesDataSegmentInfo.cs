﻿using System;
using System.IO;
using VSS.TRex.Common;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.SubGridTrees.Server.Interfaces;

namespace VSS.TRex.SubGridTrees.Server
{
  public class SubGridCellPassesDataSegmentInfo : ISubGridCellPassesDataSegmentInfo
  {
        /// <summary>
        /// The version number of this segment when it is stored in the persistent layer, defined
        /// as the number of ticks in DateTime.UtcNow at the time it is written.
        /// </summary>
        public long Version { get; set; }

        public ISubGridCellPassesDataSegment Segment { get; set; }
        public DateTime StartTime { get; set; } = Consts.MIN_DATETIME_AS_UTC;
        public DateTime EndTime { get; set; } = Consts.MAX_DATETIME_AS_UTC;

        public double MinElevation { get; set; } = Consts.NullDouble;
        public double MaxElevation { get; set; } = Consts.NullDouble;

        public ISubGridSpatialAffinityKey AffinityKey(Guid projectUID)
        {
          return new SubGridSpatialAffinityKey(Version, projectUID, Segment.Owner.OriginX, Segment.Owner.OriginY, 
                                               Segment.SegmentInfo.StartTime.Ticks, Segment.SegmentInfo.EndTime.Ticks);
        }

        public bool ExistsInPersistentStore { get; set; }

        public DateTime MidTime => DateTime.FromOADate((StartTime.ToOADate() + EndTime.ToOADate()) / 2);

        public SubGridCellPassesDataSegmentInfo()
        {
            Touch();
        }

        public SubGridCellPassesDataSegmentInfo(DateTime startTime, DateTime endTime,
                                                ISubGridCellPassesDataSegment segment) : this()
        {
            StartTime = startTime;
            EndTime = endTime;
            Segment = segment;
        }

        /// <summary>
        /// IncludesTimeWithinBounds determines if ATime is strictly greater than
        /// the start time and strictly less than the end time of this segment.
        /// It is not intended to resolve boundary edge cases where ATime is exactly
        /// equal to the start or end time of the segment
        /// </summary>
        public bool IncludesTimeWithinBounds(DateTime time)
        {
            var testTime = time.Ticks;
            return testTime > StartTime.Ticks && testTime < EndTime.Ticks;
        }

        /// <summary>
        /// Returns a string representing the segment identifier for this segment within this sub grid. The identifier
        /// is based on the time range this segment is responsible for storing cell passes for.
        /// </summary>
        public string SegmentIdentifier() => StartTime.Ticks.ToString() + "-" + EndTime.Ticks.ToString(); // 30% faster than $"{StartTime.Ticks}-{EndTime.Ticks}"

        /// <summary>
        /// Returns the 'filename', and string that encodes the segment version, spatial location and time range it 
        /// is responsible for. 
        /// </summary>
        public string FileName(int OriginX, int OriginY) => $"{Version}-{OriginX:d10}-{OriginY:d10}-{SegmentIdentifier()}";

        public void Write(BinaryWriter writer)
        {
            writer.Write(Version);
            writer.Write(StartTime.ToBinary());
            writer.Write(EndTime.ToBinary());
            writer.Write(MinElevation);
            writer.Write(MaxElevation);
       }

        public void Read(BinaryReader reader)
        {
            Version = reader.ReadInt64();
            StartTime = DateTime.FromBinary(reader.ReadInt64());
            EndTime = DateTime.FromBinary(reader.ReadInt64());
            MinElevation = reader.ReadDouble();
            MaxElevation = reader.ReadDouble();
        }

        /// <summary>
        /// Updates the version of the segment to reflect the current date time
        /// </summary>
        public void Touch()
        {
          Version = DateTime.UtcNow.Ticks;
        }

        public override string ToString()
        {
          return $"ID: {SegmentIdentifier()}, MinElev: {MinElevation}, MaxElev: {MaxElevation}, ExistsInPersistentStore?:{ExistsInPersistentStore}, AllPasses?:{Segment?.HasAllPasses ?? false}, LatestData?:{Segment?.HasLatestData ?? false}";
        }
    }
}
