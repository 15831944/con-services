﻿using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.ExtensionMethods;

namespace VSS.TRex.CellDatum.GridFabric.Arguments
{
  public class CellPassesRequestArgument_ClusterCompute : BaseApplicationServiceRequestArgument
  {
    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// The grid point in the project coordinate system to identify the cell from. 
    /// </summary>
    public XYZ NEECoords { get; set; }

    /// <summary>
    /// On the ground coordinates for the cell
    /// </summary>
    public int OTGCellX { get; set; }
    public int OTGCellY { get; set; }

    public CellPassesRequestArgument_ClusterCompute()
    {
      
    }

    public CellPassesRequestArgument_ClusterCompute(Guid siteModelID,
      XYZ neeCoords,
      int otgCellX,
      int otgCellY,
      IFilterSet filters)
    {
      ProjectID = siteModelID;
      NEECoords = neeCoords;
      OTGCellX = otgCellX;
      OTGCellY = otgCellY;
      Filters = filters;
    }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      base.InternalToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      NEECoords.ToBinary(writer);
      writer.WriteInt(OTGCellX);
      writer.WriteInt(OTGCellY);
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      base.InternalFromBinary(reader);

      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        NEECoords = new XYZ();
        NEECoords = NEECoords.FromBinary(reader);

        OTGCellX = reader.ReadInt();
        OTGCellY = reader.ReadInt();
      }
    }
  }
}
