﻿using System;
using System.Diagnostics;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.ExtensionMethods;

namespace VSS.TRex.Filters
{
  public class CellSpatialFilterModel : ICellSpatialFilterModel
  {
    /// <summary>
    /// The fence used for polygon based spatial filtering
    /// </summary>
    public Fence Fence { get; set; } = new Fence();

    /// <summary>
    /// The fence used to represent the spatial restriction derived from an alignment filter expressed as a 
    /// station and offset range with respect tot he alignment center line geometry expressed as a polygon
    /// </summary>
    public Fence AlignmentFence { get; set; } = new Fence(); // contains alignment boundary to help speed up filtering on alignment files

    // Positional based filtering

    /// <summary>
    /// The X ordinate of the positional spatial filter
    /// </summary>
    public double PositionX { get; set; } = Consts.NullDouble;

    /// <summary>
    /// The Y ordinate of the positional spatial filter
    /// </summary>
    public double PositionY { get; set; } = Consts.NullDouble;

    /// <summary>
    /// The radius of the positional spatial filter for point-radius positional filters
    /// </summary>
    public double PositionRadius { get; set; } = Consts.NullDouble;

    /// <summary>
    /// Determines if the point-radius should be applied as a square rather than a circle
    /// </summary>
    public bool IsSquare { get; set; }

    /// <summary>
    /// OverrideSpatialCellRestriction provides a rectangular, cell address based,
    /// restrictive boundary that overrides all other cell selection filter considerations
    /// in that it is always evaluated first. This is useful in contexts such as Web Map Service
    /// tile generation where the tile region itself is an overriding constraint
    /// on the data that needs to be queried
    /// </summary>
    public BoundingIntegerExtent2D OverrideSpatialCellRestriction { get; set; }

    // <summary>
    // A design that acts as a spatial filter for cell selection. Only cells that have center locations that lie over the 
    // design recorded in DesignFilter will be included
    // </summary>
    //    public Guid DesignFilterUID = Guid.Empty;
    //        public DesignDescriptor DesignFilter = DesignDescriptor.Null(); 

    /// <summary>
    /// The starting station of the parametrically defined alignment spatial filter
    /// </summary>
    public double? StartStation { get; set; }

    /// <summary>
    /// The ending station of the parametrically defined alignment spatial filter
    /// </summary>
    public double? EndStation { get; set; }

    /// <summary>
    /// The left offset of the parametrically defined alignment spatial filter
    /// </summary>
    public double? LeftOffset { get; set; }

    /// <summary>
    /// The right offset of the parametrically defined alignment spatial filter
    /// </summary>
    public double? RightOffset { get; set; }

    /// <summary>
    /// CoordsAreGrid controls whether the plan (XY/NE) coordinates in the spatial filters are to 
    /// be interpreted as rectangular cartesian coordinates or as WGS84 latitude/longitude coordinates
    /// </summary>
    public bool CoordsAreGrid { get; set; }

    /// <summary>
    /// Restricts cells to spatial fence
    /// </summary>
    public bool IsSpatial { get; set; }

    /// <summary>
    /// Restricts cells to spatial fence
    /// </summary>
    public bool IsPositional { get; set; }

    /// <summary>
    /// Using a loaded surface design to 'mask' the cells that should be included in the filter
    /// </summary>
    public bool IsDesignMask { get; set; }

    /// <summary>
    /// A design that acts as a spatial filter for cell selection. Only cells that have center locations that lie over the 
    /// design recorded in DesignMask will be included
    /// </summary>
    public Guid SurfaceDesignMaskDesignUid { get; set; } = Guid.Empty;

    /// <summary>
    /// Using a load alignment design to 'mask' the cells that should be included in the filter
    /// </summary>
    public bool IsAlignmentMask { get; set; }

    /// <summary>
    /// The design used as an alignment mask spatial filter
    /// </summary>
    public Guid AlignmentMaskDesignUID { get; set; } = Guid.Empty;

    /// <summary>
    /// Serialize out the state of the cell spatial filter using the Ignite IBinarizable serialisation
    /// </summary>
    /// <param name="writer"></param>
    public void ToBinary(IBinaryRawWriter writer)
    {
      const byte versionNumber = 1;

      writer.WriteByte(versionNumber);

      writer.WriteBoolean(Fence != null);
      Fence?.ToBinary(writer);

      writer.WriteBoolean(AlignmentFence != null);
      AlignmentFence?.ToBinary(writer);

      writer.WriteDouble(PositionX);
      writer.WriteDouble(PositionY);
      writer.WriteDouble(PositionRadius);
      writer.WriteBoolean(IsSquare);

      OverrideSpatialCellRestriction.ToBinary(writer);

      writer.WriteBoolean(StartStation.HasValue);
      if (StartStation.HasValue)
        writer.WriteDouble(StartStation.Value);

      writer.WriteBoolean(EndStation.HasValue);
      if (EndStation.HasValue)
        writer.WriteDouble(EndStation.Value);

      writer.WriteBoolean(LeftOffset.HasValue);
      if (LeftOffset.HasValue)
        writer.WriteDouble(LeftOffset.Value);

      writer.WriteBoolean(RightOffset.HasValue);
      if (RightOffset.HasValue)
        writer.WriteDouble(RightOffset.Value);

      writer.WriteBoolean(CoordsAreGrid);
      writer.WriteBoolean(IsSpatial);
      writer.WriteBoolean(IsPositional);

      writer.WriteBoolean(IsDesignMask);
      writer.WriteGuid(SurfaceDesignMaskDesignUid);

      writer.WriteBoolean(IsAlignmentMask);
      writer.WriteGuid(AlignmentMaskDesignUID);
    }

    /// <summary>
    /// Serialize in the state of the cell spatial filter using the Ignite IBinarizable serialisation
    /// </summary>
    /// <param name="reader"></param>
    public void FromBinary(IBinaryRawReader reader)
    {
      const byte versionNumber = 1;
      byte readVersionNumber = reader.ReadByte();

      if (readVersionNumber != versionNumber)
        throw new TRexSerializationVersionException(versionNumber, readVersionNumber);

      if (reader.ReadBoolean())
        (Fence ?? (Fence = new Fence())).FromBinary(reader);

      if (reader.ReadBoolean())
        (AlignmentFence ?? (AlignmentFence = new Fence())).FromBinary(reader);

      PositionX = reader.ReadDouble();
      PositionY = reader.ReadDouble();
      PositionRadius = reader.ReadDouble();
      IsSquare = reader.ReadBoolean();

      OverrideSpatialCellRestriction = OverrideSpatialCellRestriction.FromBinary(reader);

      StartStation = reader.ReadBoolean() ? reader.ReadDouble() : (double?) null;
      EndStation = reader.ReadBoolean() ? reader.ReadDouble() : (double?)null;
      LeftOffset = reader.ReadBoolean() ? reader.ReadDouble() : (double?)null;
      RightOffset = reader.ReadBoolean() ? reader.ReadDouble() : (double?)null;

      CoordsAreGrid = reader.ReadBoolean();
      IsSpatial = reader.ReadBoolean();
      IsPositional = reader.ReadBoolean();

      IsDesignMask = reader.ReadBoolean();
      SurfaceDesignMaskDesignUid = reader.ReadGuid() ?? Guid.Empty;
      IsAlignmentMask = reader.ReadBoolean();
      AlignmentMaskDesignUID = reader.ReadGuid() ?? Guid.Empty;
    }

    /// <summary>
    /// Override equality comparision function with a protected access
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    protected bool Equals(CellSpatialFilterModel other)
    {
      return Equals(Fence, other.Fence) && 
             Equals(AlignmentFence, other.AlignmentFence) && 
             PositionX.Equals(other.PositionX) && 
             PositionY.Equals(other.PositionY) && 
             PositionRadius.Equals(other.PositionRadius) && 
             IsSquare == other.IsSquare && 
             OverrideSpatialCellRestriction.Equals(other.OverrideSpatialCellRestriction) && 
             StartStation.Equals(other.StartStation) && 
             EndStation.Equals(other.EndStation) && 
             LeftOffset.Equals(other.LeftOffset) && 
             RightOffset.Equals(other.RightOffset) && 
             CoordsAreGrid == other.CoordsAreGrid && 
             IsSpatial == other.IsSpatial && 
             IsPositional == other.IsPositional && 
             IsDesignMask == other.IsDesignMask && 
             SurfaceDesignMaskDesignUid.Equals(other.SurfaceDesignMaskDesignUid) && 
             IsAlignmentMask == other.IsAlignmentMask && 
             AlignmentMaskDesignUID.Equals(other.AlignmentMaskDesignUID);
    }

    /// <summary>
    /// Equality comparision function with a public access.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(ICellSpatialFilterModel other)
    {
      return Equals(other as CellSpatialFilterModel);
    }

    /// <summary>
    /// Overrides generic object equals implementation to call custom implementation
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((CellSpatialFilterModel) obj);
    }

    /// <summary>
    /// Gets hash code.
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = (Fence != null ? Fence.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ (AlignmentFence != null ? AlignmentFence.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ PositionX.GetHashCode();
        hashCode = (hashCode * 397) ^ PositionY.GetHashCode();
        hashCode = (hashCode * 397) ^ PositionRadius.GetHashCode();
        hashCode = (hashCode * 397) ^ IsSquare.GetHashCode();
        hashCode = (hashCode * 397) ^ OverrideSpatialCellRestriction.GetHashCode();
        hashCode = (hashCode * 397) ^ StartStation.GetHashCode();
        hashCode = (hashCode * 397) ^ EndStation.GetHashCode();
        hashCode = (hashCode * 397) ^ LeftOffset.GetHashCode();
        hashCode = (hashCode * 397) ^ RightOffset.GetHashCode();
        hashCode = (hashCode * 397) ^ CoordsAreGrid.GetHashCode();
        hashCode = (hashCode * 397) ^ IsSpatial.GetHashCode();
        hashCode = (hashCode * 397) ^ IsPositional.GetHashCode();
        hashCode = (hashCode * 397) ^ IsDesignMask.GetHashCode();
        hashCode = (hashCode * 397) ^ SurfaceDesignMaskDesignUid.GetHashCode();
        hashCode = (hashCode * 397) ^ IsAlignmentMask.GetHashCode();
        hashCode = (hashCode * 397) ^ AlignmentMaskDesignUID.GetHashCode();
        return hashCode;
      }
    }
  }
}
