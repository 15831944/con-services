﻿using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.ExtensionMethods;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Designs.GridFabric.Arguments
{
  /// <summary>
  /// Contains the parameters for addition and modification of designs in a project
  /// </summary>
  public class AddTTMDesignArgument : BaseRequestArgument
  {
    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// The project the request is relevant to
    /// </summary>
    public Guid ProjectID { get; set; }

    /// <summary>
    /// The descriptor of the design being added or modified
    /// </summary>
    public DesignDescriptor DesignDescriptor { get; set; }

    /// <summary>
    /// The boundaing rectangle conputed for the design
    /// </summary>
    public BoundingWorldExtent3D Extents { get; set; }
      
    /// <summary>
    /// The spatial sub grid existence map for the area coveed by the design
    /// </summary>
    public ISubGridTreeBitMask ExistenceMap { get; set; }

    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      base.InternalToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteGuid(ProjectID);
      writer.WriteBoolean(DesignDescriptor != null);
      DesignDescriptor?.ToBinary(writer);

      writer.WriteBoolean(Extents != null);
      Extents?.ToBinary(writer);

      writer.WriteBoolean(ExistenceMap != null);

      if (ExistenceMap != null)
      {
        writer.WriteByteArray(ExistenceMap.ToBytes());
      }
    }

    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      base.InternalFromBinary(reader);

      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        ProjectID = reader.ReadGuid() ?? Guid.Empty;

        if (reader.ReadBoolean())
          (DesignDescriptor = new DesignDescriptor()).FromBinary(reader);

        if (reader.ReadBoolean())
          (Extents = new BoundingWorldExtent3D()).FromBinary(reader);

        if (reader.ReadBoolean())
        {
          (ExistenceMap = new SubGridTreeSubGridExistenceBitMask()).FromBytes(reader.ReadByteArray());
        }
      }
    }
  }
}
