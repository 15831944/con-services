﻿using System;
using System.Collections.Generic;
using System.Text;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApi.Models.MapHandling;

namespace VSS.Productivity3D.WebApiTests.MapHandling
{
  public class TestUtils
  {
    public static string GetWicketFromPoints(List<Point> points)
    {
      if (points.Count == 0)
        return string.Empty;

      var polygonWkt = new StringBuilder("POLYGON((");
      foreach (var point in points)
      {
        polygonWkt.Append(String.Format("{0} {1},", point.Longitude, point.Latitude));
      }
      if (points[0] != points[points.Count - 1])
      {
        polygonWkt.Append(String.Format("{0} {1},", points[0].Longitude, points[0].Latitude));
      }
      return polygonWkt.ToString().TrimEnd(',') + "))";
    }
  }
}
