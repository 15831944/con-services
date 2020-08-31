﻿using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Reports.Gridded;

namespace VSS.TRex.Reports.StationOffset.GridFabric.Arguments
{
  /// <summary>
  /// The argument to be supplied to the grid request
  /// </summary>
  public class StationOffsetReportRequestArgument_ApplicationService : BaseApplicationServiceRequestArgumentReport
  {
    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// This design contains the center line which will be sampled along
    /// </summary>
    public Guid AlignmentDesignUid { get; set; }

    /// <summary>
    /// The spacing interval for the sampled points. Setting to 1.0 will cause points to be spaced 1.0 meters apart.
    /// </summary>
    /// 
    public double CrossSectionInterval { get; set; }

    /// <summary>
    /// Start point along the center line in the AlignmentUid design
    /// </summary>
    /// 
    public double StartStation { get; set; }

    /// <summary>
    /// End point along the center line in the AlignmentUid design
    /// </summary>
    /// 
    public double EndStation { get; set; }

    /// <summary>
    /// Offsets left and right (or on) the center line in the AlignmentUid design
    /// </summary>
    /// 
    public double[] Offsets { get; set; } = new double[0];

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      base.InternalToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteGuid(AlignmentDesignUid);
      writer.WriteDouble(CrossSectionInterval);
      writer.WriteDouble(StartStation);
      writer.WriteDouble(EndStation);
      writer.WriteDoubleArray(Offsets);
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      base.InternalFromBinary(reader); 
      
      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        AlignmentDesignUid = reader.ReadGuid() ?? Guid.Empty;
        CrossSectionInterval = reader.ReadDouble();
        StartStation = reader.ReadDouble();
        EndStation = reader.ReadDouble();
        Offsets = reader.ReadDoubleArray();
      }
    }
  }
}
