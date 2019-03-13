﻿using System.Collections.Generic;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Geometry;

namespace VSS.TRex.GridFabric.ExtensionMethods
{
  /// <summary>
  /// Provides extension methods supporting IBinarizable style Ignite grid serialisation for a collection of common object in TRex
  /// </summary>
  public static class FromToBinary
  {
    /// <summary>
    /// An extension method providing a ToBinary() semantic to Fence
    /// </summary>
    public static void ToBinary(this Fence item, IBinaryRawWriter writer)
    {
      const byte VERSION_NUMBER = 1;

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      bool isNull = item.IsNull();     
      writer.WriteBoolean(isNull);

      if (isNull)
        return;

      writer.WriteBoolean(item.IsRectangle);

      writer.WriteInt(item.NumVertices);
      foreach (var point in item.Points)
      {
        writer.WriteDouble(point.X);
        writer.WriteDouble(point.Y);
        writer.WriteDouble(point.Z);
      }
    }

    /// <summary>
    /// An extension method providing a FromBinary() semantic to Fence
    /// </summary>
    public static void FromBinary(this Fence item, IBinaryRawReader reader)
    {
      const byte VERSION_NUMBER = 1;

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      bool isNull = reader.ReadBoolean();
      if (isNull)
        return;

      item.IsRectangle = reader.ReadBoolean();

      item.Points = new List<FencePoint>(reader.ReadInt());
      for (int i = 0; i < item.Points.Capacity; i++)
         item.Points.Add(new FencePoint(reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble()));

      item.UpdateExtents();
    }

    /// <summary>
    /// An extension method providing a ToBinary() semantic to BoundingWorldExtent3D
    /// </summary>
    public static void ToBinary(this BoundingWorldExtent3D item, IBinaryRawWriter writer)
    {
      const byte VERSION_NUMBER = 1;

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteDouble(item.MinX);
      writer.WriteDouble(item.MinY);
      writer.WriteDouble(item.MinZ);
      writer.WriteDouble(item.MaxX);
      writer.WriteDouble(item.MaxY);
      writer.WriteDouble(item.MaxZ);
    }

    /// <summary>
    /// An extension method providing a ToBinary() semantic to BoundingWorldExtent3D
    /// </summary>
    public static void FromBinary(this BoundingWorldExtent3D item, IBinaryRawReader reader)
    {
      const byte VERSION_NUMBER = 1;

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      item.MinX = reader.ReadDouble();
      item.MinY = reader.ReadDouble();
      item.MinZ = reader.ReadDouble();
      item.MaxX = reader.ReadDouble();
      item.MaxY = reader.ReadDouble();
      item.MaxZ = reader.ReadDouble();
    }

    /// <summary>
    /// An extension method providing a ToBinary() semantic to BoundingIntegerExtent2D
    /// </summary>
    public static void ToBinary(this BoundingIntegerExtent2D item, IBinaryRawWriter writer)
    {
      const byte VERSION_NUMBER = 1;

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteInt(item.MinX);
      writer.WriteInt(item.MinY);
      writer.WriteInt(item.MaxX);
      writer.WriteInt(item.MaxY);
    }

    /// <summary>
    /// An extension method providing a ToBinary() semantic to BoundingIntegerExtent2D
    /// </summary>
    public static BoundingIntegerExtent2D FromBinary(this BoundingIntegerExtent2D item, IBinaryRawReader reader)
    {
      const byte VERSION_NUMBER = 1;

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      item.MinX = reader.ReadInt();
      item.MinY = reader.ReadInt();
      item.MaxX = reader.ReadInt();
      item.MaxY = reader.ReadInt();

      return item;
    }

    /// <summary>
    /// An extension method providing a ToBinary() semantic to XYZ
    /// </summary>
    public static void ToBinary(this XYZ item, IBinaryRawWriter writer)
    {
      const byte VERSION_NUMBER = 1;

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteDouble(item.X);
      writer.WriteDouble(item.Y);
      writer.WriteDouble(item.Z);
    }

    /// <summary>
    /// An extension method providing a FromBinary() semantic to XYZ
    /// </summary>
    public static XYZ FromBinary(this XYZ item, IBinaryRawReader reader)
    {
      const byte VERSION_NUMBER = 1;

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      item.X = reader.ReadDouble();
      item.Y = reader.ReadDouble();
      item.Z = reader.ReadDouble();

      return item;
    }

    /// <summary>
    /// An extension method providing a ToBinary() semantic to XYZ
    /// </summary>
    public static void ToBinaryUnversioned(this XYZ item, IBinaryRawWriter writer)
    {
      writer.WriteDouble(item.X);
      writer.WriteDouble(item.Y);
      writer.WriteDouble(item.Z);
    }

    /// <summary>
    /// An extension method providing a FromBinary() semantic to XYZ
    /// </summary>
    public static XYZ FromBinaryUnversioned(this XYZ item, IBinaryRawReader reader)
    {
      item.X = reader.ReadDouble();
      item.Y = reader.ReadDouble();
      item.Z = reader.ReadDouble();

      return item;
    }
  }
}
