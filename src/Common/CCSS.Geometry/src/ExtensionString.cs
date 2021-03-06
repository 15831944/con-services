﻿using System.Collections.Generic;
using System.Linq;

namespace CCSS.Geometry
{
  internal static class ExtensionString
  {
    private static readonly Dictionary<string, string> _replacements = new Dictionary<string, string>();

    static ExtensionString()
    {
      _replacements["LINESTRING"] = "";
      _replacements["CIRCLE"] = "";
      _replacements["POLYGON"] = "";
      _replacements["POINT"] = "";
      _replacements["("] = "";
      _replacements[")"] = "";
    }

    public static List<Point> ClosePolygonIfRequired(this List<Point> s)
    {
      if (Equals(s.First(), s.Last()))
        return s;
      s.Add(s.First());
      return s;
    }

    public static string ToPolygonWKT(this List<Point> s)
    {
      var internalString = s.Select(p => p.WKTSubstring).Aggregate((i, j) => $"{i},{j}");
      return $"POLYGON(({internalString}))";
    }

    public static string ToPolygonWKT(this List<List<double[]>> list)
    {
      // Always just a single 2D array in the list which is the CWS polygon coordinates
      var coords = list[0];
      var rowCount = coords.Count;
      var wktCoords = new List<string>();
      for (var i = 0; i < rowCount; i++)
      {
        wktCoords.Add($"{coords[i][0]} {coords[i][1]}");
      }

      var internalString = wktCoords.Aggregate((i, j) => $"{i},{j}");
      return $"POLYGON(({internalString}))";
    }

    public static List<Point> ParseGeometryData(this string s)
    {
      if (string.IsNullOrEmpty(s))
        return new List<Point>();

      foreach (string to_replace in _replacements.Keys)
      {
        s = s.Replace(to_replace, _replacements[to_replace]);
      }

      var pointsArray = s.Split(',').Select(str => str.Trim()).ToArray();

      //gets x and y coordinates split by space, trims whitespace at pos 0, converts to double array
      var coordinates = pointsArray.Select(point => point.Trim().Split(null)
        .Where(v => !string.IsNullOrWhiteSpace(v)).ToArray());
      return coordinates.Select(p => new Point() {X = double.Parse(p[0]), Y = double.Parse(p[1])}).ToList();
    }
  }
}
