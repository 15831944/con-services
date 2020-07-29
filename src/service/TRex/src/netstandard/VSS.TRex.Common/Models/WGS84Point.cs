﻿using System;
using Apache.Ignite.Core.Binary;

namespace VSS.TRex.Common.Models
{
  /// <summary>
  /// A two dimensional point defining Latitude and Longitude in the WGS84 datum
  /// </summary>
  public class WGS84Point : IEquatable<WGS84Point>
  {
    // Note: Lat and Lon expressed as radians, Height as meters
    public double Lat;
    public double Lon;
    public double Height = Consts.NullDouble;

    public WGS84Point() {  }
    public WGS84Point(double lon, double lat) { Lat = lat; Lon = lon; }
    public WGS84Point(double lon, double lat, double height) { Lat = lat; Lon = lon; Height = height; }

    public bool Equals(WGS84Point other)
    {
      if (other == null) return false;
      return Math.Abs(other.Lat - Lat) < Consts.TOLERANCE_DECIMAL_DEGREE &&
             Math.Abs(other.Lon - Lon) < Consts.TOLERANCE_DECIMAL_DEGREE &&
             Math.Abs(other.Height - Height) < Consts.TOLERANCE_DIMENSION;
    }

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    public void ToBinary(IBinaryRawWriter writer)
    {
      writer.WriteDouble(Lat);
      writer.WriteDouble(Lon);
      writer.WriteDouble(Height);
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    public void FromBinary(IBinaryRawReader reader)
    {
      Lat = reader.ReadDouble();
      Lon = reader.ReadDouble();
      Height = reader.ReadDouble();
    }

    public override string ToString()
    {
      return base.ToString() + $"Lat {Lat}, Lon {Lon}, Height {Height}";
    }
  }
}
