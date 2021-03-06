﻿using System;
using VSS.TRex.QuantizedMesh.Models;

namespace VSS.TRex.QuantizedMesh.MeshUtils
{
  public static class HorizonOcclusionPoint
  {

    // Constants taken from http://cesiumjs.org/2013/04/25/Horizon-culling/
    private static Double radiusX = 6378137.0;
    private static Double radiusY = 6378137.0;
    private static Double radiusZ = 6356752.3142451793;
    private static Double rX = 1.0 / radiusX;
    private static Double rY = 1.0 / radiusY;
    private static Double rZ = 1.0 / radiusZ;

    /*
    wgs84_a = radiusX               # Semi-major axis
      wgs84_b = radiusZ          # Semi-minor axis
    wgs84_e2 = 0.0066943799901975848  # First eccentricity squared
    wgs84_a2 = wgs84_a** 2           # To speed things up a bit
    wgs84_b2 = wgs84_b** 2
    */

    public static Double DotProduct(Vector3 pt1, Vector3 pt2)
    {
      return pt1.X * pt2.X + pt1.Y * pt2.Y + pt1.Z * pt2.Z;
    }

    public static Vector3 CrossProduct(Vector3 A, Vector3 B)
    {
      return new Vector3(A.Y * B.Z - B.Y * A.Z, A.Z * B.X - B.Z * A.X, A.X * B.Y - B.X * A.Y);
    }

    // Functions assumes ellipsoid scaled coordinates
    public static Double ComputeMagnitude(Vector3 point, Vector3 sphereCenter)
    {
      var magnitudeSquared = Cartesian3D.MagnitudeSquared(point);
      var magnitude = Math.Sqrt(magnitudeSquared);
      var direction = Cartesian3D.MultiplyByScalar(point, 1 / magnitude);
      magnitudeSquared = Math.Max(1.0, magnitudeSquared);
      magnitude = Math.Max(1.0, magnitude);
      var cosAlpha = DotProduct(direction, sphereCenter);
      var sinAlpha = Cartesian3D.Magnitude(CrossProduct(direction, sphereCenter));
      var cosBeta = 1.0 / magnitude;
      var sinBeta = Math.Sqrt(magnitudeSquared - 1.0) * cosBeta;
      return 1.0 / (cosAlpha * cosBeta - sinAlpha * sinBeta);

    }

    // from https://cesiumjs.org/2013/05/09/Computing-the-horizon-occlusion-point/
    public static Vector3 FromPoints(ref Vector3[] points, BBSphere boundingSphere)
    {
      if (points.Length < 1)
      {
        throw new ArgumentException("Your list of points must contain at least 2 points");
      }

      // Bring coordinates to ellipsoid scaled coordinates
      for (int i = 0; i < points.Length; i++)
      {
        points[i].X = points[i].X * rX;
        points[i].Y = points[i].Y * rY;
        points[i].Z = points[i].Z * rZ;
      }

      boundingSphere.Center.X = boundingSphere.Center.X * rX;
      boundingSphere.Center.Y = boundingSphere.Center.Y * rY;
      boundingSphere.Center.Z = boundingSphere.Center.Z * rZ;

      Double maxMagnitude = double.NegativeInfinity;
      for (int i = 0; i < points.Length; i++)
      {
        //magnitudes.Add(ComputeMagnitude(points[i], boundingSphere.Center));
        var magnitude = ComputeMagnitude(points[i], boundingSphere.Center);
        if (magnitude > maxMagnitude)
          maxMagnitude = magnitude;
      }

      return Cartesian3D.MultiplyByScalar(boundingSphere.Center, maxMagnitude);
    }



  }
}
