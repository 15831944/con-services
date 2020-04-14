﻿namespace VSS.Productivity3D.TagFileAuth.Models.ResultsHandling
{
  public class TWGS84Point
  {
    // Note: Lat and Lon expressed as radians
    public double Lat;
    public double Lon;

    public TWGS84Point(double ALon, double ALat) { Lat = ALat; Lon = ALon; }

    public override bool Equals(object obj)
    {
      var otherPoint = obj as TWGS84Point;
      if (otherPoint == null) return false;
      return otherPoint.Lat == this.Lat
            && otherPoint.Lon == this.Lon
            ;
    }
    public override int GetHashCode() { return 0; }

  }
}
