﻿using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;
using VSS.TRex.Profiling.Models;
using VSS.TRex.Common.Utilities;

namespace VSS.TRex.Designs.TTM.Optimised.Profiling
{
  /// <summary>
  /// Implements support for computing profile lines across a TIN surface expressed in the
  /// VSS.TRex.Designs.TTM.Optimised schema
  /// </summary>
  public class OptimisedTTMProfiler : IOptimisedTTMProfiler
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<OptimisedTTMProfiler>();

    private readonly TrimbleTINModel TTM;

    private readonly IOptimisedSpatialIndexSubGridTree Index;
    private readonly int[] Indices;

    /// <summary>
    /// Creates an empty profiler context
    /// </summary>
    public OptimisedTTMProfiler(TrimbleTINModel ttm,
                                IOptimisedSpatialIndexSubGridTree index,
                                int [] indices)
    {
      TTM = ttm;
      Index = index;
      Indices = indices;
    }

    private void AddEndIntercept(XYZ point, List<XYZS> intercepts, double station)
    {
      if (!Index.CalculateIndexOfCellContainingPosition(point.X, point.Y, out int cellX, out int cellY))
      {
        Log.LogWarning($"No cell address computable for end point location {point.X}:{point.Y}");
        return;
      }

      var subGrid = Index.LocateSubGridContaining(cellX, cellY);

      if (subGrid == null) // No triangles in this 'node' sub grid
        return;

      if (!(subGrid is TriangleArrayReferenceSubGrid referenceSubGrid))
      {
        Log.LogCritical($"Sub grid is not a TriangleArrayReferenceSubGrid, is is a {subGrid.GetType()}");
        return;
      }

      // Get the cell representing a leaf sub grid and determine if there is a triangle at the point location
      subGrid.GetSubGridCellIndex(cellX, cellY, out byte subGridX, out byte subGridY);

      TriangleArrayReference referenceList = referenceSubGrid.Items[subGridX, subGridY];

      int endIndex = referenceList.TriangleArrayIndex + referenceList.Count;

      for (int i = referenceList.TriangleArrayIndex; i < endIndex; i++)
      {
        double height = XYZ.GetTriangleHeightEx
         (ref TTM.Vertices.Items[TTM.Triangles.Items[Indices[i]].Vertex0],
          ref TTM.Vertices.Items[TTM.Triangles.Items[Indices[i]].Vertex1],
          ref TTM.Vertices.Items[TTM.Triangles.Items[Indices[i]].Vertex2], point.X, point.Y);

        if (height != Common.Consts.NullDouble)
        {
          intercepts.Add(new XYZS(point.X, point.Y, height, station, Indices[i]));
          return;
        }
      }
    }

    /// <summary>
    /// Computes a profile line across the TIN surface between two points
    /// </summary>
    /// <param name="startPoint"></param>
    /// <param name="endPoint"></param>
    /// <param name="startStation"></param>
    /// <returns></returns>
    private List<XYZS> Compute(XYZ startPoint, XYZ endPoint, double startStation)
    {
      // 1. Determine the set of sub grids the profile line cross using the same logic used to
      // compute cell cross by production data profiling

      // ...
      var cellProfileBuilder = new OptimisedTTMCellProfileBuilder(Index.CellSize, true);
      if (!cellProfileBuilder.Build(new [] {startPoint, endPoint}, startStation))
        return null;

      // 2. Iterate across each sub grid in turn locating all triangles in that sub grid
      // that intersect the line and sorting them according to the distance of the closest
      // intercept from the start of the line

      // Get the resulting vertical and horizontal intercept list
      var VtHzIntercepts = cellProfileBuilder.VtHzIntercepts;

      // Iterate through the intercepts looking for ones that hit a sub grid in the TTM
      // spatial index that contains triangles

      var intercepts = new List<XYZS>();
      var plane = new Plane();
        
      // Add an initial intercept if the start point is located within a triangle
      AddEndIntercept(startPoint, intercepts, startStation);

      int prevCellX = int.MinValue, preCellY = int.MinValue;

      for (int interceptIndex = 0; interceptIndex < VtHzIntercepts.Count; interceptIndex++)
      {
        InterceptRec intercept = VtHzIntercepts.Items[interceptIndex];

        if (!Index.CalculateIndexOfCellContainingPosition(intercept.MidPointX, intercept.MidPointY, out int cellX, out int cellY))
        {
          Log.LogWarning($"No cell address computable for location {intercept.MidPointX}:{intercept.MidPointY}");
          continue;
        }

        // Make sure we are not repeating a sub grid (can happen as a result of how the initial HZ/Vt intersects are constructed
        if (prevCellX == cellX && preCellY == cellY)
        {
          // This sub grid has just been processed...
          continue;
        }

        prevCellX = cellX;
        preCellY = cellY;

        var subGrid = Index.LocateSubGridContaining(cellX, cellY);

        if (subGrid == null)
        {
          // No triangles are present in this 'node' sub grid. Move to the next sub grid, the implicit gap will be 
          // picked up when triangle intercepts are aggregated together to form the final profile line
          continue;
        }

        if (!(subGrid is TriangleArrayReferenceSubGrid referenceSubGrid))
        {
          Log.LogCritical($"Sub grid is not a TriangleArrayReferenceSubGrid, is is a {subGrid.GetType()}");
          continue;
        }

        subGrid.GetSubGridCellIndex(cellX, cellY, out byte subGridX, out byte subGridY);

        TriangleArrayReference referenceList = referenceSubGrid.Items[subGridX, subGridY];

        if (referenceList.Count == 0)
        {
          // There are no triangles in this 'leaf' sub grid
          continue;
        }

        // Locate all triangles in this sub grid that intersect the profile line
        var endIndex = referenceList.TriangleArrayIndex + referenceList.Count;
        for (int i = referenceList.TriangleArrayIndex; i < endIndex; i++)
        {
          int triIndex = Indices[i];
          Triangle tri = TTM.Triangles.Items[triIndex];

          // Does this triangle intersect the line?
          XYZ v0 = TTM.Vertices.Items[tri.Vertex0];
          XYZ v1 = TTM.Vertices.Items[tri.Vertex1];
          XYZ v2 = TTM.Vertices.Items[tri.Vertex2];

          bool planeInited = false;

          if (LineIntersection.LinesIntersect(startPoint.X, startPoint.Y, endPoint.X, endPoint.Y, v0.X, v0.Y, v1.X, v1.Y, out double intersectX, out double intersectY, true, out bool linesAreColinear))
          {
            // If the lines are co-linear there is nothing more to do. The two vertices need to be added and no more checking is required
            if (linesAreColinear)
            {
              intercepts.Add(new XYZS(v0.X, v0.Y, v0.Z, startStation + MathUtilities.Hypot(startPoint.X - v0.X, startPoint.Y - v0.Y), triIndex));
              intercepts.Add(new XYZS(v1.X, v1.Y, v1.Z, startStation + MathUtilities.Hypot(startPoint.X - v1.X, startPoint.Y - v1.Y), triIndex));
              continue;
            }

            planeInited = true;
            plane.Init(v0, v1, v2);

            // Otherwise, add the intercept location to the list
            intercepts.Add(new XYZS(intersectX, intersectY, plane.Evaluate(intersectX, intersectY), startStation + MathUtilities.Hypot(startPoint.X - intersectX, startPoint.Y - intersectY), triIndex));
          }

          if (LineIntersection.LinesIntersect(startPoint.X, startPoint.Y, endPoint.X, endPoint.Y, v0.X, v0.Y, v2.X, v2.Y, out intersectX, out intersectY, true, out linesAreColinear))
          {
            // If the lines are co-linear there is nothing more to do. The two vertices need to be added and no more checking is required
            if (linesAreColinear)
            {
              intercepts.Add(new XYZS(v0.X, v0.Y, v0.Z, startStation + MathUtilities.Hypot(startPoint.X - v0.X, startPoint.Y - v0.Y), triIndex));
              intercepts.Add(new XYZS(v2.X, v2.Y, v2.Z, startStation + MathUtilities.Hypot(startPoint.X - v2.X, startPoint.Y - v2.Y), triIndex));
              continue;
            }

            if (!planeInited)
            {
              planeInited = true;
              plane.Init(v0, v1, v2);
            }

            // Otherwise, add the intercept location to the list
            intercepts.Add(new XYZS(intersectX, intersectY, plane.Evaluate(intersectX, intersectY), startStation + MathUtilities.Hypot(startPoint.X - intersectX, startPoint.Y - intersectY), triIndex));
          }

          if (LineIntersection.LinesIntersect(startPoint.X, startPoint.Y, endPoint.X, endPoint.Y, v1.X, v1.Y, v2.X, v2.Y, out intersectX, out intersectY, true, out linesAreColinear))
          {
            // If the lines are co-linear there is nothing more to do. The two vertices need to be added and no more checking is required
            if (linesAreColinear)
            {
              intercepts.Add(new XYZS(v1.X, v1.Y, v1.Z, startStation + MathUtilities.Hypot(startPoint.X - v1.X, startPoint.Y - v1.Y), triIndex));
              intercepts.Add(new XYZS(v2.X, v2.Y, v2.Z, startStation + MathUtilities.Hypot(startPoint.X - v2.X, startPoint.Y - v2.Y), triIndex));
              continue;
            }

            if (!planeInited)
            {
              plane.Init(v0, v1, v2);
            }

            // Otherwise, add the intercept location to the list
            intercepts.Add(new XYZS(intersectX, intersectY, plane.Evaluate(intersectX, intersectY), startStation + MathUtilities.Hypot(startPoint.X - intersectX, startPoint.Y - intersectY), triIndex));
          }
        }
      }

      // Add an end intercept if the start point is located within a triangle
      AddEndIntercept(endPoint, intercepts, startStation + MathUtilities.Hypot(startPoint.X - endPoint.X, startPoint.Y - endPoint.Y));

      return intercepts;
    }

    /// <summary>
    /// Computes a profile across a surface along a series of line segments
    /// </summary>
    /// <param name="points"></param>
    /// <returns></returns>
    public List<XYZS> Compute(XYZ[] points)
    {
      if (points == null || points.Length < 2)
        throw new ArgumentException("Points list cannot be null or contain less than 2 points");

      var intercepts = new List<XYZS>();
      double runningStation = 0;

      for (int i = 0; i < points.Length - 1; i++)
      {
        var subIntercepts = Compute(points[i], points[i + 1], runningStation);

        if (subIntercepts.Count > 0)
          intercepts.AddRange(subIntercepts);

        runningStation += MathUtilities.Hypot(points[i + 1].X - points[i].X, points[i + 1].Y - points[i].Y);
      }

      // Locate any gaps in the resulting set of intercepts. A gap is defined by two successive intercepts
      // with different station values and different triangle indexes. In the same pass, also remove any
      // duplicated intercepts

      if (intercepts.Count <= 1)
        return intercepts;

      // Sort the computed intercepts into station order
      intercepts.Sort((a, b) => a.Station.CompareTo(b.Station));

      // Assemble all duplicates at the same station, replacing them with a single intercept.
      // If the triangle indices of these duplicates match none of the triangle indices in the 
      // previous group of duplicates then there is a gap.

      int[] thisTriIndices = new int[1000];
      int[] nextTriIndices = new int[1000];

      var curatedIntercepts = new List<XYZS>
      {
        intercepts[0] // THe first element is always a member of the resulting profile
      };

      // Seed duplicates with the first triangle index
      int thisDuplicateCount = 0;
      thisTriIndices[thisDuplicateCount++] = intercepts[0].TriIndex;

      // Accumulate duplicates matching the first element in triIndices
      int interceptIndex = 1;
      while (interceptIndex < intercepts.Count &&
             Math.Abs(intercepts[interceptIndex].Station - intercepts[interceptIndex - 1].Station) < double.Epsilon)
      {
        thisTriIndices[thisDuplicateCount++] = intercepts[interceptIndex].TriIndex;
        interceptIndex++;

        if (thisDuplicateCount >= thisTriIndices.Length)
          Array.Resize(ref thisTriIndices, thisTriIndices.Length + 1000);
      }

      // Walk through remainder of intercepts, creating a duplicates list and comparing it to the 
      // previous duplicates list to determine if there is a gap (ie: no matching triangles).
      while (interceptIndex < intercepts.Count)
      {
        // Accumulate duplicates matching the first element in triIndices
        int nextDuplicateCount = 0;
        int next_interceptIndex = interceptIndex;

        nextTriIndices[nextDuplicateCount++] = intercepts[interceptIndex++].TriIndex;

        while (interceptIndex < intercepts.Count &&
               Math.Abs(intercepts[interceptIndex].Station - intercepts[interceptIndex - 1].Station) < double.Epsilon)
        {
          nextTriIndices[nextDuplicateCount++] = intercepts[interceptIndex].TriIndex;
          interceptIndex++;

          if (nextDuplicateCount >= nextTriIndices.Length)
            Array.Resize(ref nextTriIndices, nextTriIndices.Length + 1000);
        }

        // thisTriIndices and nextTriIndices now contain arrays of triangle indexed involved in both
        // groups of intercepts. If there is a common triangle index between them, there is no gap...
        bool isAGap = true;
        for (int thisIndex = 0; thisIndex < thisDuplicateCount; thisIndex++)
        {
          for (int nextIndex = 0; nextIndex < nextDuplicateCount; nextIndex++)
          {
            if (thisTriIndices[thisIndex] == nextTriIndices[nextIndex])
            {
              isAGap = false;
              break;
            }
          }

          if (!isAGap)
            break;
        }

        if (isAGap)
        {
          // There is a gap, add the gap intercept
          curatedIntercepts.Add(new XYZS(intercepts[interceptIndex - 1])
          {
            Z = Common.Consts.NullDouble,
            Station = intercepts[interceptIndex - 1].Station + 0.000000001
          });
        }

        // add the intercept for the group of duplicates
        curatedIntercepts.Add(intercepts[next_interceptIndex]);

        // Roll the 'next' tri indices collection to 'this' indices collection
        MinMax.Swap(ref thisTriIndices, ref nextTriIndices);
        thisDuplicateCount = nextDuplicateCount;
      }

      return curatedIntercepts;
    }
  }
}
