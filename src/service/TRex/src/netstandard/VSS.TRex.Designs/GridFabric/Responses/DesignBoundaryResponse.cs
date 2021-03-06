﻿using System.Collections.Generic;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.ExtensionMethods;

namespace VSS.TRex.Designs.GridFabric.Responses
{
  /// <summary>
  /// Represents the response to a request for a polygonal boundary comprising grid coordinate vertices 
  /// that represents the boundary of an area of a design.
  /// </summary>
  public class DesignBoundaryResponse : BaseDesignRequestResponse
  {
    private const byte VERSION_NUMBER = 1;

    public List<Fence> Boundary { get; set; }

    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      base.InternalToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteBoolean(Boundary != null);

      if (Boundary != null)
      {
        writer.WriteInt(Boundary.Count);

        foreach (var fence in Boundary)
          fence.ToBinary(writer);
      }
    }

    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      base.InternalFromBinary(reader);

      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        if (reader.ReadBoolean())
        {
          Boundary = new List<Fence>();

          var fencesCount = reader.ReadInt();

          for (var i = 0; i < fencesCount; i++)
          {
            var fence = new Fence();
            fence.FromBinary(reader);
            Boundary.Add(fence);
          }
        }
      }
    }
  }
}
