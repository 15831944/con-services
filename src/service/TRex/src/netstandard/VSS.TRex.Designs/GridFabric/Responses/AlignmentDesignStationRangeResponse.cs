﻿using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Geometry;

namespace VSS.TRex.Designs.GridFabric.Responses
{
  /// <summary>
  /// Represents the response to a request for a alignment design station range values.
  /// </summary>
  public class AlignmentDesignStationRangeResponse : BaseDesignRequestResponse
  {
    private const byte VERSION_NUMBER = 1;

    public double StartStation { get; set; }

    public double EndStation { get; set; }

    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      base.InternalToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteDouble(StartStation);
      writer.WriteDouble(EndStation);
    }

    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      base.InternalFromBinary(reader);

      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        StartStation = reader.ReadDouble();
        EndStation = reader.ReadDouble();
      }
    }
  }
}
