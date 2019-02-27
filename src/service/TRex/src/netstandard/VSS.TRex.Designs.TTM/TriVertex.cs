﻿using System;
using VSS.TRex.Geometry;

namespace VSS.TRex.Designs.TTM
{
  /// <summary>
  /// Implements a vertex at the corner of triangles in the TTM mesh
  /// </summary>
  public class TriVertex
  {
    /// <summary>
    /// A 'tag' used for various purposes in TTM processing
    /// </summary>
    public int Tag { get; set; }

    /// <summary>
    /// Gets the X, Y, Z location of the vertex as a XYZ instance
    /// </summary>
    /// <returns></returns>
    private XYZ GetXYZ() => new XYZ(X, Y, Z);

    /// <summary>
    /// Sets the location of the vertex from a XYZ instance
    /// </summary>
    /// <param name="Value"></param>
    private void SetXYZ(XYZ Value)
    {
      X = Value.X;
      Y = Value.Y;
      Z = Value.Z;
    }

    /// <summary>
    /// Constructor accepting the X, Y and Z coordinate location for the vertex
    /// </summary>
    /// <param name="aX"></param>
    /// <param name="aY"></param>
    /// <param name="aZ"></param>
    public TriVertex(double aX, double aY, double aZ)
    {
      Tag = 0;

      X = aX;
      Y = aY;
      Z = aZ;
    }

    /// <summary>
    /// Constructor accepting the X, Y and Z coordinate location for the vertex
    /// </summary>
    /// <param name="vertex"></param>
    public TriVertex(XYZ vertex)
    {
      Tag = 0;

      X = vertex.X;
      Y = vertex.Y;
      Z = vertex.Z;
    }

    /// <summary>
    /// The X ordinate location of the vertex
    /// </summary>
    public double X { get; set; }

    /// <summary>
    /// The Y ordinate location of the vertex
    /// </summary>
    public double Y { get; set; }

    /// <summary>
    /// The Z ordinate location of the vertex
    /// </summary>    
    public double Z { get; set; }

    /// <summary>
    /// Expands a set 3D ordinate limits by the location of this vertex
    /// </summary>
    /// <param name="MinX"></param>
    /// <param name="MinY"></param>
    /// <param name="MinZ"></param>
    /// <param name="MaxX"></param>
    /// <param name="MaxY"></param>
    /// <param name="MaxZ"></param>
    public void AdjustLimits(ref double MinX, ref double MinY, ref double MinZ, ref double MaxX, ref double MaxY, ref double MaxZ)
    {
      if (X < MinX) MinX = X;
      if (X > MaxX) MaxX = X;
      if (Y < MinY) MinY = Y;
      if (Y > MaxY) MaxY = Y;
      if (Z < MinZ) MinZ = Z;
      if (Z > MaxZ) MaxZ = Z;
    }

    /// <summary>
    /// Determine if an X, Y, Z coordinate is the same as the coordinate of this vertex within a given tolerance
    /// </summary>
    /// <param name="aX"></param>
    /// <param name="aY"></param>
    /// <param name="aZ"></param>
    /// <param name="Tolerance"></param>
    /// <returns></returns>
    public bool IsEqual(double aX, double aY, double aZ, double Tolerance)
    {
      return Math.Sqrt(Math.Pow(X - aX, 2) + Math.Pow(Y - aY, 2) + Math.Pow(Z - aZ, 2)) <= Tolerance;
    }

    /// <summary>
    /// Determine if another TriVertex coordinate is the same as the coordinate of this vertex within a given tolerance
    /// </summary>
    /// <param name="Other"></param>
    /// <param name="Tolerance"></param>
    /// <returns></returns>
    public bool IsEqual(TriVertex Other, double Tolerance)
    {
      return Other == this || IsEqual(Other.X, Other.Y, Other.Z, Tolerance);
    }

    /// <summary>
    /// Property representing the X, Y, Z location of this vertex as a XYZ instance
    /// </summary>
    public XYZ XYZ { get => GetXYZ(); set => SetXYZ(value); }

    /// <summary>
    /// Converts the location of the vertex to a string
    /// </summary>
    /// <returns></returns>
    public string AsText() => $"Tag:{Tag}, X={X:F3}, Y={Y:F3}, Z={Z:F3}";

    /// <summary>
    /// Overridden ToString()
    /// </summary>
    /// <returns></returns>
    public override string ToString() => AsText();
  }
}
