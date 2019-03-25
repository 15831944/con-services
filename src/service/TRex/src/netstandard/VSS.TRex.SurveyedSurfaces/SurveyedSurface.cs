﻿using System;
using System.IO;
using VSS.TRex.Common;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Common.Utilities.ExtensionMethods;
using VSS.TRex.Common.Utilities.Interfaces;

namespace VSS.TRex.SurveyedSurfaces
{
  /// <summary>
  /// Defines all the state information describing a surveyed surface based on a design descriptor
  /// </summary>
  public class SurveyedSurface : IEquatable<ISurveyedSurface>, IBinaryReaderWriter, ISurveyedSurface
  {
    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// Readonly property exposing the surveyed surface ID
    /// </summary>
    public Guid ID { get; set; }

    /// <summary>
    /// Readonly property exposing the design descriptor for the underlying topology surface
    /// </summary>
    public DesignDescriptor DesignDescriptor;

    /// <summary>
    /// Readonly attribute for AsAtData
    /// </summary>
    public DateTime AsAtDate { get; set; }

    /// <summary>
    /// 3D extents bounding box enclosing the underlying design represented by the design descriptor (excluding any vertical offset(
    /// </summary>
    BoundingWorldExtent3D extents = new BoundingWorldExtent3D();

    /// <summary>
    /// Serialises state to a binary writer
    /// </summary>
    /// <param name="writer"></param>
    public void Write(BinaryWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.Write(ID.ToByteArray());
      DesignDescriptor.Write(writer);
      writer.Write(AsAtDate.ToBinary());
      extents.Write(writer);
    }

    /// <summary>
    /// Serialises state to a binary writer with a supplied intermediary buffer
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="buffer"></param>
    public void Write(BinaryWriter writer, byte[] buffer) => Write(writer);

    /// <summary>
    /// Serialises state in from a binary reader
    /// </summary>
    /// <param name="reader"></param>
    public void Read(BinaryReader reader)
    {
      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      ID = reader.ReadGuid();
      DesignDescriptor.Read(reader);
      AsAtDate = DateTime.FromBinary(reader.ReadInt64());
      extents.Read(reader);
    }

    /// <summary>
    /// Returns the real world 3D enclosing extents for the surveyed surface topology, including any configured vertical offset
    /// </summary>
    public BoundingWorldExtent3D Extents
    {
      get
      {
        BoundingWorldExtent3D result = new BoundingWorldExtent3D(extents);

        // Incorporate any vertical offset from the underlying design the surveyed surface is based on
        result.Offset(DesignDescriptor.Offset);

        return result;
      }
    }

    /// <summary>
    /// No-arg constructor
    /// </summary>
    public SurveyedSurface()
    {
      DesignDescriptor = DesignDescriptor.Null();
    }

    /// <summary>
    /// Constructor accepting full surveyed surface state
    /// </summary>
    /// <param name="iD">The unique identifier for the surveyed surface in this site model</param>
    /// <param name="designDescriptor"></param>
    /// <param name="asAtDate"></param>
    /// <param name="extents"></param>
    public SurveyedSurface(Guid iD,
      DesignDescriptor designDescriptor,
      DateTime asAtDate,
      BoundingWorldExtent3D extents)
    {
      ID = iD;
      DesignDescriptor = designDescriptor;
      AsAtDate = asAtDate;
      this.extents = extents;
    }

    /// <summary>
    /// Produces a deep clone of the surveyed surface
    /// </summary>
    /// <returns></returns>
    public ISurveyedSurface Clone() => new SurveyedSurface(ID, DesignDescriptor, AsAtDate, new BoundingWorldExtent3D(extents));

    /// <summary>
    /// ToString() for SurveyedSurface
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return
        $"ID:{ID}, DesignID:{DesignDescriptor.DesignID} {AsAtDate}; {DesignDescriptor.Folder};{DesignDescriptor.FileName} {DesignDescriptor.Offset:F3} [{extents}]";
    }

    public DesignDescriptor Get_DesignDescriptor() => DesignDescriptor;

    /// <summary>
    /// Determine if two surveyed surfaces are equal
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(ISurveyedSurface other)
    {
      return (ID == other.ID) &&
             DesignDescriptor.Equals(other.Get_DesignDescriptor()) &&
             (AsAtDate == other.AsAtDate) &&
             (extents.Equals(other.Extents));
    }
  }
}
