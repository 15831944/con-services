﻿using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.Models;
using VSS.TRex.GridFabric.Arguments;

namespace VSS.TRex.Designs.GridFabric.Arguments
{
  public class CalculateDesignProfileArgument : BaseApplicationServiceRequestArgument
  {
    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// The path along which the profile will be calculated
    /// </summary>
    public WGS84Point StartPoint { get; set; } = new WGS84Point();
    public WGS84Point EndPoint { get; set; } = new WGS84Point();

    public bool PositionsAreGrid { get; set; }

    /// <summary>
    /// The cell stepping size to move between points in the patch being interpolated
    /// </summary>
    public double CellSize { get; set; }

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public CalculateDesignProfileArgument()
    {
    }

    /// <summary>
    /// Constructor taking the full state of the elevation patch computation operation
    /// </summary>
    public CalculateDesignProfileArgument(Guid projectUid,
                                          double cellSize,
                                          Guid designUid,
                                          double offset,
                                          WGS84Point startPoint,
                                          WGS84Point endPoint,
                                          bool positionsAreGrid = false) : this()
    {
      ProjectID = projectUid;
      CellSize = cellSize;
      ReferenceDesign.DesignID = designUid;
      ReferenceDesign.Offset = offset;
      StartPoint = startPoint;
      EndPoint = endPoint;
      PositionsAreGrid = positionsAreGrid;
    }

    /// <summary>
    /// Overloaded ToString to add argument properties
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return base.ToString() + $" -> ProjectUID:{ProjectID}, CellSize:{CellSize}, Design:{ReferenceDesign?.DesignID}, Offset: {ReferenceDesign?.Offset}, StartPoint: {StartPoint}, EndPoint {EndPoint}";
    }

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      base.InternalToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteDouble(CellSize);

      writer.WriteBoolean(StartPoint != null);
      StartPoint?.ToBinary(writer);

      writer.WriteBoolean(EndPoint != null);
      EndPoint?.ToBinary(writer);

      writer.WriteBoolean(PositionsAreGrid);

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
        CellSize = reader.ReadDouble();

        StartPoint = new WGS84Point();
        if (reader.ReadBoolean())
          StartPoint.FromBinary(reader);

        EndPoint = new WGS84Point();
        if (reader.ReadBoolean())
          EndPoint.FromBinary(reader);

        PositionsAreGrid = reader.ReadBoolean();
      }
    }
  }
}
