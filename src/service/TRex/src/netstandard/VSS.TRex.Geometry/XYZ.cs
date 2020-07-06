﻿using System;
using System.Runtime.CompilerServices;

/*
  Triangle and vector calculations

  Function              Description
  ------------------------------------------------------------------------------
  NextSide              Returns the index of the next side on a triangle,
                        i.e. 1 -> 2, 2 -> 3, 3 -> 1
  PrevSide              Returns the index of the previous side on a triangle,
                        i.e. 1 -> 3, 2 -> 1, 3 -> 2
  XYZ                   Returns a populated XYZ record
  Subtract              Performs vector subtraction V1 - V0
  Add                   Performs vector addition V1 + V2
  Multiply              Multiplies a vector by a scale factor
  Get2DLength           Gets the XY distance between two points
  Get3DLength           Gets the XYZ distance between two points
  CoordsEqual           Returns True if vectors are exactly equal
  DotProduct            Returns a dot product relative to an origin
  PerpDotProduct        Returns a dot product relative to an origin where one
                        of the vectors is rotated 90 degrees
  PointOnRight          Returns True if the point is on the right hand side of
                        the line
  GetPointOffset        Returns the distance from the line to the point. Positive
                        offsets are to the right.
  CrossProduct          The cross product of two vectors relative to an origin
  VectorLength          The 3D length of a vector
  GetTriArea            Area of a triangle defined by three points
  GetTriangleHeight     Calculates the height of a point on a triangle
  GetTriCentroid        Returns the triangle centroid
  PointInTriangle       Returns True if the point is inside the triangle
  PointInTriangleInclusive Returns True if the point is inside the triangle, or on its boundary
  UnitVector            Returns a vector of length 1 in the same direction
*/

namespace VSS.TRex.Geometry
{
  /// <summary>
  /// An XYZ defines a standard point in 3 dimensional cartesian space
  /// </summary>
  public struct XYZ
  {
    /// <summary>
    /// Specifiers for the X, Y and Z dimensions
    /// </summary>
    public double X, Y, Z;

    /// <summary>
    /// Is this point null in the plan (X & Y) dimensions
    /// </summary>
    public bool IsNull => X == Consts.NullDouble || Y == Consts.NullDouble || Z == Consts.NullDouble;

    /// <summary>
    /// Is this point null in the plan (X & Y) dimensions
    /// </summary>
    public bool IsNullInPlan => X == Consts.NullDouble || Y == Consts.NullDouble;

    /// <summary>
    /// Display human readable version of the XYZ fields
    /// </summary>
    public override string ToString() => Z == Consts.NullDouble ? $"X:{X:F6}, Y:{Y:F6}, Z:Null, " : $"X:{X:F6}, Y:{Y:F6}, Z:{Z:F6}";

    /// <summary>
    /// XYZ constructor taking X,Y and Z dimensions
    /// </summary>
    public XYZ(double x, double y, double z)
    {
      X = x;
      Y = y;
      Z = z;
    }

    /// <summary>
    /// XYZ constructor taking X & Y dimensions, while defaulting the Z dimension to null
    /// </summary>
    public XYZ(double x, double y)
    {
      X = x;
      Y = y;
      Z = Consts.NullDouble;
    }

    /// <summary>
    /// Extract the three components of the XYZ structure into three reference parameters
    /// </summary>
    public void Explode(out double x, out double y, out double z)
    {
      x = X;
      y = Y;
      z = Z;
    }

    /// <summary>
    /// Create an XYZ instance initialized to NullDoubles for X, Y, and Z
    /// </summary>
    public static XYZ Null => new XYZ(Consts.NullDouble, Consts.NullDouble, Consts.NullDouble);

    /// <summary>
    /// Move side to the next side on a triangle (labeled 1, 2, & 3)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int NextSide(int side) => (side + 1) % 3;

    /// <summary>
    /// Move side to the previous side on a triangle (labeled 1, 2, & 3)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int PrevSide(int side) => (side + 2) % 3;

    /// <summary>
    /// Compare two XYZ points for equality
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(XYZ V1, XYZ V2) => V1.Equals(V2);

    /// <summary>
    /// Compare two XYZ points for inequality
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(XYZ V1, XYZ V2) => !V1.Equals(V2);

    /// <summary>
    /// Subtract two XYZ points
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XYZ operator -(XYZ V1, XYZ V0) => new XYZ(V1.X - V0.X, V1.Y - V0.Y, V1.Z - V0.Z);

    /// <summary>
    /// Add two XYZ points
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XYZ operator +(XYZ V1, XYZ V0) => new XYZ(V1.X + V0.X, V1.Y + V0.Y, V1.Z + V0.Z);

    /// <summary>
    /// Multiply an XYZ point by a factor. All dimensions are scaled.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XYZ operator *(XYZ V, double factor) => new XYZ(V.X * factor, V.Y * factor, V.Z * factor);

    /// <summary>
    /// Determine if one XYZ equals another
    /// </summary>
    public bool Equals(XYZ other) => X == other.X && Y == other.Y && Z == other.Z;

    /// <summary>
    /// Calculate 2D length between two XYZ points
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Get2DLength(XYZ p1, XYZ p2) => Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));

    /// <summary>
    /// Calculate 3D length between two XYZ points
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Get3DLength(XYZ p1, XYZ p2) => Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2) + Math.Pow(p2.Z - p1.Z, 2));

    /// <summary>
    /// Calculate dot product of two vectors from Origin to Pt1 and Origin to Pt2
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double DotProduct(XYZ Origin, XYZ Pt1, XYZ Pt2) => (Pt1.X - Origin.X) * (Pt2.X - Origin.X) + (Pt1.Y - Origin.Y) * (Pt2.Y - Origin.Y);

    /// <summary>
    /// Calculate perpendicular dot product of two vectors from Origin to Pt1 and Origin to Pt2
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double PerpDotProduct(XYZ Origin, XYZ Pt1, XYZ Pt2) => (Pt1.Y - Origin.Y) * (Pt2.X - Origin.X) - (Pt1.X - Origin.X) * (Pt2.Y - Origin.Y);

    /// <summary>
    /// Determine if the XYZ Pt is on the right hand side of the vector line defined by Line1 and LiIne2
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool PointOnRight(XYZ Line1, XYZ Line2, XYZ Pt) => PerpDotProduct(Line1, Line2, Pt) > 0;

    /// <summary>
    /// Determine if the XYZ Pt is on the right hand side of, or directly on top of, the vector line defined by Line1 and LiIne2
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool PointOnOrOnRight(XYZ Line1, XYZ Line2, XYZ Pt) => PerpDotProduct(Line1, Line2, Pt) >= 0;

    /// <summary>
    /// Calculate the perpendicular distance between XYZ point P2 and the line given by the XYZ points Line1 and Line2
    /// </summary>
    public static double GetPointOffset(XYZ Line1, XYZ Line2, XYZ Pt)
    {
      double Len = Get3DLength(Line1, Line2);
      return Len == 0 ? Get3DLength(Line1, Pt) : PerpDotProduct(Line1, Line2, Pt) / Len;
    }

    /// <summary>
    /// Calculate dot product of two vectors from Org to PtA and Org to PtB
    /// </summary>
    public static XYZ CrossProduct(XYZ Org, XYZ PtA, XYZ PtB)
    {
      XYZ A = PtA - Org;
      XYZ B = PtB - Org;

      return new XYZ(A.Y * B.Z - B.Y * A.Z, A.Z * B.X - B.Z * A.X, A.X * B.Y - B.X * A.Y);
    }

    /// <summary>
    /// Compute the vector length defined as length from origin (0, 0, 0) and XYZ point V
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double VectorLength(XYZ V) => Math.Sqrt(V.X * V.X + V.Y * V.Y + V.Z * V.Z);

    /// <summary>
    /// Calculate the are of a triangle defined by three XYZ points
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double GetTriArea(XYZ P1, XYZ P2, XYZ P3) => VectorLength(CrossProduct(P1, P2, P3)) / 2;

    /// <summary>
    /// Internal function to calculate an intersection on the X axis
    /// </summary>
    private static double GetInt(double _X1, double _Y1, double _X2, double _Y2, double _X)
    {
      const double MIN_EPSILON = -0.000000001; // Allow a nanometer tolerance on triangle edges...
      const double MAX_EPSILON = 1.000000001; // Allow a nanometer tolerance on triangle edges...

      if (Math.Abs(_X1 - _X2) < 0.0000000001) // Consider them the same
        return Consts.NullDouble;

      double Fraction = (_X - _X1) / (_X2 - _X1);

      return Fraction >= MIN_EPSILON && Fraction <= MAX_EPSILON ? _Y1 + Fraction * (_Y2 - _Y1) : Consts.NullDouble;
    }

    /// <summary>
    /// Internal function to determine if there is an intersection on the Y axis
    /// </summary>
    private static bool GetYInt(XYZ P1, XYZ P2, double X, out double Y, out double Z)
    {
      const double MIN_EPSILON = -0.000000001; // Allow a nanometer tolerance on triangle edges...
      const double MAX_EPSILON = 1.000000001; // Allow a nanometer tolerance on triangle edges...

      // Compute the first GetYInt long handed...
      if (Math.Abs(P1.X - P2.X) < 0.0000000001) // Consider them the same
      {
        Y = Consts.NullDouble;
        Z = Consts.NullDouble;
        return false;
      }

      double Fraction = (X - P1.X) / (P2.X - P1.X);

      if (Fraction >= MIN_EPSILON && Fraction <= MAX_EPSILON)
        Y = P1.Y + Fraction * (P2.Y - P1.Y);
      else
      {
        Y = Consts.NullDouble;
        Z = Consts.NullDouble;
        return false;
      }

      // Compute the second leveraging the results of the first
      Z = P1.Z + Fraction * (P2.Z - P1.Z);
      return true;
    }

    /// <summary>
    /// Calculate the height on a triangle given by three XYZ points at the location given by X & Y
    /// </summary>
    public static double GetTriangleHeight(XYZ P1, XYZ P2, XYZ P3, double X, double Y)
    {
      bool GetInt1 = GetYInt(P1, P2, X, out double Y1, out double Z1);
      bool GetInt2 = GetYInt(P2, P3, X, out double Y2, out double Z2);

      // Note: In some cases we may actually work out that we have intersects
      // with all three edges (yes, it can happen). Thus, if we fail to get a
      // height interpolated from a pair of intersects, and we have the third
      // intersect on hand, we'll give that one a go too...
      if (GetInt1 && GetInt2)
      {
        double Result = GetInt(Y1, Z1, Y2, Z2, Y);

        if (Result != Consts.NullDouble)
          return Result;
      }

      bool GetInt3 = GetYInt(P1, P3, X, out double Y3, out double Z3);

      if (GetInt1 && GetInt3)
        return GetInt(Y1, Z1, Y3, Z3, Y);

      if (GetInt2 && GetInt3)
        return GetInt(Y2, Z2, Y3, Z3, Y);

      return Consts.NullDouble;
    }

    /// <summary>
    /// Calculate the height on a triangle given by three XYZ points at the location given by X & Y
    /// </summary>
    public static double GetTriangleHeightEx(ref XYZ P1, ref XYZ P2, ref XYZ P3, double X, double Y)
    {
      const double MIN_EPSILON = -0.000000001; // Allow a nanometer tolerance on triangle edges...
      const double MAX_EPSILON = 1.000000001; // Allow a nanometer tolerance on triangle edges...

      double Fraction;
      bool GetInt1 = false, GetInt2 = false;
      double Y1 = 0, Z1 = 0, Y2 = 0, Z2 = 0, Y3, Z3;

      ////////////////////////////////////////////////////////////////////////////
      // bool GetInt1 = GetYInt(ref P1, ref P2, X, out double Y1, out double Z1);
      ////////////////////////////////////////////////////////////////////////////

      if (Math.Abs(P1.X - P2.X) > 0.0000000001) // Consider them not the same
      {
        Fraction = (X - P1.X) / (P2.X - P1.X);

        if (Fraction >= MIN_EPSILON && Fraction <= MAX_EPSILON)
        {
          Y1 = P1.Y + Fraction * (P2.Y - P1.Y);
          Z1 = P1.Z + Fraction * (P2.Z - P1.Z);
          GetInt1 = true;
        }
      }

      ////////////////////////////////////////////////////////////////////////////
      // bool GetInt2 = GetYInt(ref P2, ref P3, X, out double Y2, out double Z2);
      ////////////////////////////////////////////////////////////////////////////

      if (Math.Abs(P2.X - P3.X) > 0.0000000001) // Consider them not the same
      {
        Fraction = (X - P2.X) / (P3.X - P2.X);

        if (Fraction >= MIN_EPSILON && Fraction <= MAX_EPSILON)
        {
          Y2 = P2.Y + Fraction * (P3.Y - P2.Y);
          Z2 = P2.Z + Fraction * (P3.Z - P2.Z);
          GetInt2 = true;
        }
      }

      double Result = Consts.NullDouble;

      ///////////////////////////////////////////////////////////////////////////
      // Note: In some cases we may actually work out that we have intersects
      // with all three edges (yes, it can happen). Thus, if we fail to get a
      // height interpolated from a pair of intersects, and we have the third
      // intersect on hand, we'll give that one a go too...
      if (GetInt1 && GetInt2)
      {
        ////////////////////////////////////////////////////////////////////////
        // double Result = GetInt(Y1, Z1, Y2, Z2, Y);
        /////////////////////////////////////////////////////////////////////////
        if (Math.Abs(Y1 - Y2) > 0.0000000001) // Consider them NOT the same
        {
          Fraction = (Y - Y1) / (Y2 - Y1);
          Result = Fraction >= MIN_EPSILON && Fraction <= MAX_EPSILON ? Z1 + Fraction * (Z2 - Z1) : Consts.NullDouble;
        }
        /////////////////////////////////////////////////////////////////////////
      }

      if (Result != Consts.NullDouble)
        return Result;

      ////////////////////////////////////////////////////////////////////////////
      //bool GetInt3 = (GetYInt(ref P1, ref P3, X, out double Y3, out double Z3))
      ////////////////////////////////////////////////////////////////////////////
      if (Math.Abs(P1.X - P3.X) > 0.0000000001) // Consider them NOT the same
      {
        Fraction = (X - P1.X) / (P3.X - P1.X);

        if (Fraction >= MIN_EPSILON && Fraction <= MAX_EPSILON)
        {
          Y3 = P1.Y + Fraction * (P3.Y - P1.Y);

          if (GetInt1)
          {
            //////////////////////////////////////////////////////////////////////
            // return GetInt(Y1, Z1, Y3, Z3, Y);
            //////////////////////////////////////////////////////////////////////
            if (Math.Abs(Y1 - Y3) > 0.0000000001) // Consider them NOT the same
            {
              Fraction = (Y - Y1) / (Y3 - Y1);

              if (Fraction >= MIN_EPSILON && Fraction <= MAX_EPSILON)
              {
                Z3 = P1.Z + Fraction * (P3.Z - P1.Z);
                Result = Z1 + Fraction * (Z3 - Z1);
              }

              if (Result != Consts.NullDouble)
                return Result;
            }
            //////////////////////////////////////////////////////////////////////
          }

          if (GetInt2)
          {
            //////////////////////////////////////////////////////////////////////
            // return GetInt(Y2, Z2, Y3, Z3, Y);
            //////////////////////////////////////////////////////////////////////
            if (Math.Abs(Y2 - Y3) >= 0.0000000001) // Consider them NOT the same
            {
              Fraction = (Y - Y2) / (Y3 - Y2);

              if (Fraction >= MIN_EPSILON && Fraction <= MAX_EPSILON)
              {
                Z3 = P1.Z + Fraction * (P3.Z - P1.Z);
                Result = Z2 + Fraction * (Z3 - Z2);
              }

              if (Result != Consts.NullDouble)
                return Result;
            }
            //////////////////////////////////////////////////////////////////////
          }
        }
      }

      return Result;
    }

    /// <summary>
    /// Determine if the given point (X, Y) is inside the triangle defined by three XYZ points
    /// </summary>
    public static bool PointInTriangle(XYZ P1, XYZ P2, XYZ P3, double X, double Y)
    {
      XYZ TestPos = new XYZ(X, Y, 0);
      bool IsOnRight = PointOnRight(P1, P2, TestPos);

      return IsOnRight == PointOnRight(P2, P3, TestPos) && IsOnRight == PointOnRight(P3, P1, TestPos);
    }

    /// <summary>
    /// Determine if the given point (X, Y) is inside the triangle defined by three XYZ points. This method will return
    /// true if the point is coincident with any of the three side on the triangle
    /// </summary>
    public static bool PointInTriangleInclusive(XYZ P1, XYZ P2, XYZ P3, double X, double Y)
    {
      XYZ TestPos = new XYZ(X, Y, 0);
      bool IsOnRight = PointOnOrOnRight(P1, P2, TestPos);

      if (IsOnRight == PointOnOrOnRight(P2, P3, TestPos) && IsOnRight == PointOnOrOnRight(P3, P1, TestPos))
        return true;

      // Check left-hand winding...
      bool IsOnLeft = PointOnOrOnRight(P2, P1, TestPos);

      return IsOnLeft == PointOnOrOnRight(P1, P3, TestPos) && IsOnLeft == PointOnOrOnRight(P3, P2, TestPos);
    }

    /// <summary>
    /// Calculate the scaled unit vector from the vector given by (0, 0, 0) -> XYZ point V
    /// </summary>
    public static XYZ UnitVector(XYZ V) => V * (1 / VectorLength(V));

    /// <summary>
    /// Returns the triangle centroid
    /// </summary>
    public static XYZ GetTriCentroid(XYZ P1, XYZ P2, XYZ P3)
    {
      return new XYZ((P1.X + P2.X + P3.X) / 3,
        (P1.Y + P2.Y + P3.Y) / 3,
        (P1.Z + P2.Z + P3.Z) / 3);
    }
  }
}
