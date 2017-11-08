﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Common;

/*
  Triangle and vector calculations

  Function              Description
  ------------------------------------------------------------------------------
  NextSide              Returns the index of the next side on a triangle,
                        i.e. 1 -> 2, 2 -> 3, 3 -> 1
  PrevSide              Returns the index of the previous side on a triangle,
                        i.e. 1 -> 3, 2 -> 1, 3 -> 2
  XYZ                   Returns a populated TXYZ record
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
  VectorLength          Thje 3D length of a vector
  GetTriArea            Area of a triangle defined by three points
  GetTriangleHeight     Calculates the height of a point on a triangle
  GetTriCentroid        Returns the triangle centroid
  PointInTriangle       Returns True if the point is inside the triangle
  PointInTriangleInclusive Returns True if the point is inside the triangle, or on its boundary
  UnitVector            Returns a vector of length 1 in the same direction
*/

namespace VSS.VisionLink.Raptor.Geometry
{
    /// <summary>
    /// An XYZ defines a standard point in 3 dimensional cartesian space
    /// </summary>
    public struct XYZ : IEquatable<XYZ>
    {
        /// <summary>
        /// Specifiers for the X, Y and Z dimensions
        /// </summary>
        public double X, Y, Z;

        /// <summary>
        /// Is this point null in the plan (X & Y) dimensions
        /// </summary>
        public bool IsNull => (X == Consts.NullDouble) || (Y == Consts.NullDouble) || (Z == Consts.NullDouble);

        /// <summary>
        /// Is this point null in the plan (X & Y) dimensions
        /// </summary>
        public bool IsNullInPlan => (X == Consts.NullDouble) || (Y == Consts.NullDouble);
        
        /// <summary>
        /// Display human readable version of the XYZ fields
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("X:{0:F3}, Y:{1:F3}, Z:{2:F3}", X, Y, Z);
        }

        /// <summary>
        /// XYZ constructor taking X,Y and Z dimensions
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="Z"></param>
        public XYZ(double X, double Y, double Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }

        /// <summary>
        /// XYZ constructor taking X & Y dimensions, while defaulting the Z dimension to null
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        public XYZ(double X, double Y)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Consts.NullDouble;
        }

        /// <summary>
        /// Extract the three components of the XYZ structure into three reference parameters
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void Explode(out double x, out double y, out double z)
        {
            x = X;
            y = Y;
            z = Z;
        }

        /// <summary>
        /// Create an XYZ instance initialised to NullDoubles for X, Y, and Z
        /// </summary>
        /// <returns></returns>
        public static XYZ Null => new XYZ(Consts.NullDouble, Consts.NullDouble, Consts.NullDouble);

        //        public static implicit operator XYZ(double X, double Y, double Z) => new XYZ(X, Y, Z);

        /// <summary>
        /// Move side to the next side on a triangle (labelled 1, 2, & 3)
        /// </summary>
        /// <param name="side"></param>
        /// <returns></returns>
        public static int NextSide(int side) => (side % 3) + 1;

        /// <summary>
        /// Move side to the previous side on a triangle (labelled 1, 2, & 3)
        /// </summary>
        /// <param name="side"></param>
        /// <returns></returns>
        public static int PrevSide(int side) => ((side + 1) % 3) + 1;

        /// <summary>
        /// Compare two XYZ points for equality
        /// </summary>
        /// <param name="V1"></param>
        /// <param name="V2"></param>
        /// <returns></returns>
        public static bool operator ==(XYZ V1, XYZ V2) => V1.Equals(V2);

        /// <summary>
        /// Compare two XYZ points for inequality
        /// </summary>
        /// <param name="V1"></param>
        /// <param name="V2"></param>
        /// <returns></returns>
        public static bool operator !=(XYZ V1, XYZ V2) => !V1.Equals(V2);

        /// <summary>
        /// Subtract two XYZ points
        /// </summary>
        /// <param name="V1"></param>
        /// <param name="V0"></param>
        /// <returns></returns>
        public static XYZ operator -(XYZ V1, XYZ V0) => new XYZ(V1.X - V0.X, V1.Y - V0.Y, V1.Z - V0.Z);

        /// <summary>
        /// Add two XYZ points
        /// </summary>
        /// <param name="V1"></param>
        /// <param name="V0"></param>
        /// <returns></returns>
        public static XYZ operator +(XYZ V1, XYZ V0) => new XYZ(V1.X + V0.X, V1.Y + V0.Y, V1.Z + V0.Z);

        /// <summary>
        /// Multiply an XYZ point by a factor. All dimensions are scaled.
        /// </summary>
        /// <param name="V"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        public static XYZ operator *(XYZ V, double factor) => new XYZ(V.X * factor, V.Y * factor, V.Z * factor);

        /// <summary>
        /// Determine if one XYZ equals another
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(XYZ other) => (X == other.X) && (Y == other.Y) && (Z == other.Z);

        /// <summary>
        /// Calculate 2D length between two XYZ points
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static double Get2DLength(XYZ p1, XYZ p2) => Math.Sqrt(Math.Pow(p2.X-p1.X, 2) + Math.Pow(p2.Y-p1.Y, 2));

        /// <summary>
        /// Calculate 3D length between two XYZ points
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static double Get3DLength(XYZ p1, XYZ p2) => Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2) + Math.Pow(p2.Z - p1.Y, 2));

        /// <summary>
        /// Calculate dot product of two vectors from Origin to Pt1 and Origin to Pt2
        /// </summary>
        /// <param name="Origin"></param>
        /// <param name="Pt1"></param>
        /// <param name="Pt2"></param>
        /// <returns></returns>
        public static double DotProduct(XYZ Origin, XYZ Pt1, XYZ Pt2) => (Pt1.X - Origin.X) * (Pt2.X - Origin.X) + (Pt1.Y - Origin.Y) * (Pt2.Y - Origin.Y);

        /// <summary>
        /// Calculate perpendicular dot product of two vectors from Origin to Pt1 and Origin to Pt2
        /// </summary>
        /// <param name="Origin"></param>
        /// <param name="Pt1"></param>
        /// <param name="Pt2"></param>
        /// <returns></returns>
        public static double PerpDotProduct(XYZ Origin, XYZ Pt1, XYZ Pt2) => (Pt1.Y - Origin.Y) * (Pt2.X - Origin.X) - (Pt1.X - Origin.X) * (Pt2.Y - Origin.Y);

        /// <summary>
        /// Determine if the XYZ Pt is on the right hand side of the vector line defined by Line1 and LiIne2
        /// </summary>
        /// <param name="Line1"></param>
        /// <param name="Line2"></param>
        /// <param name="Pt"></param>
        /// <returns></returns>
        public static bool PointOnRight(XYZ Line1, XYZ Line2, XYZ Pt) => PerpDotProduct(Line1, Line2, Pt) > 0;

        /// <summary>
        /// Determine if the XYZ Pt is on the right hand side of, or directly on top of, the vector line defined by Line1 and LiIne2
        /// </summary>
        /// <param name="Line1"></param>
        /// <param name="Line2"></param>
        /// <param name="Pt"></param>
        /// <returns></returns>
        public static bool PointOnOrOnRight(XYZ Line1, XYZ Line2, XYZ Pt) => PerpDotProduct(Line1, Line2, Pt) >= 0;

        /// <summary>
        /// Calculate the perpendicular distance between XYZ point P2 and the line given by the XYZ points Line1 and Line2
        /// </summary>
        /// <param name="Line1"></param>
        /// <param name="Line2"></param>
        /// <param name="Pt"></param>
        /// <returns></returns>
        public static double GetPointOffset (XYZ Line1, XYZ Line2, XYZ Pt)
        {
            double Len = Get3DLength(Line1, Line2);
            return Len == 0 ? Get3DLength(Line1, Pt) : PerpDotProduct(Line1, Line2, Pt) / Len;
        }


        /// <summary>
        /// Calculate dot product of two vectors from Org to PtA and Org to PtB
        /// </summary>
        /// <param name="Origin"></param>
        /// <param name="Pt1"></param>
        /// <param name="Pt2"></param>
        /// <returns></returns>
        public static XYZ CrossProduct(XYZ Org, XYZ PtA, XYZ PtB)
        {
            XYZ A = PtA - Org;
            XYZ B = PtB - Org;

            return new XYZ(A.Y * B.Z - B.Y * A.Z, A.Z * B.X - B.Z * A.X, A.X * B.Y - B.X * A.Y);
        }

        /// <summary>
        /// Compute the vector length defined as length from origin (0, 0, 0) and XYZ point V
        /// </summary>
        /// <param name="V"></param>
        /// <returns></returns>
        public static double VectorLength(XYZ V) => Math.Sqrt(V.X * V.X + V.Y * V.Y + V.Z * V.Z);

        /// <summary>
        /// Calcualte the are of a triangle defined by three XYZ points
        /// </summary>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="P3"></param>
        /// <returns></returns>
        public static double GetTriArea(XYZ P1, XYZ P2, XYZ P3) => VectorLength(CrossProduct(P1, P2, P3)) / 2;

        /// <summary>
        /// Internal function to calculate an intersection on the X axis
        /// </summary>
        /// <param name="_X1"></param>
        /// <param name="_Y1"></param>
        /// <param name="_X2"></param>
        /// <param name="_Y2"></param>
        /// <param name="_X"></param>
        /// <returns></returns>
        private static double GetInt(double _X1, double _Y1, double _X2, double _Y2, double _X)
        {
            const double epsylon = 0.000000001; // Allow a nanometer tolerance on triangle edges...

            if (Math.Abs(_X1 - _X2) < 0.0000000001)  // Consider them the same
            {
                return Consts.NullDouble;
            }

            double Fraction = (_X - _X1) / (_X2 - _X1);

            return ((Fraction >= 0 - epsylon) && (Fraction <= 1 + epsylon)) ? _Y1 + Fraction * (_Y2 - _Y1) : Consts.NullDouble;
        }

        /// <summary>
        /// Internal function to determine is there is an intesection on the Y axis
        /// </summary>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="Z"></param>
        /// <returns></returns>
        private static bool GetYInt(XYZ P1, XYZ P2, double X, out double Y, out double Z)
        {
            Y = GetInt(P1.X, P1.Y, P2.X, P2.Y, X);

            if (Y != Consts.NullDouble)
            {
                Z = GetInt(P1.X, P1.Z, P2.X, P2.Z, X);
                return true;
            }
            else
            {
                Z = Consts.NullDouble;
                return false;
            }
        }

        /// <summary>
        /// Calculate the height on a triangle given by three XYZ points at the location given by X & Y
        /// </summary>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="P3"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        public static double GetTriangleHeight(XYZ P1, XYZ P2, XYZ P3, double X, double Y)
        {
            double Result = Consts.NullDouble;

            bool GetInt1 = GetYInt(P1, P2, X, out double Y1, out double Z1);
            bool GetInt2 = GetYInt(P2, P3, X, out double Y2, out double Z2);
            bool GetInt3 = GetYInt(P1, P3, X, out double Y3, out double Z3);

            // Note: In some cases we may actually work out that we have intersects
            // with all three edges (yes, it can happen). Thus, if we fail to get a
            // height interpolated from a pair of intersects, and we have the third
            // intersect on hand, we'll give that one a go too...
            if (GetInt1 && GetInt2)
            {
                Result = GetInt(Y1, Z1, Y2, Z2, Y);
                if ((Result == Consts.NullDouble) && GetInt3)
                {
                    Result = GetInt(Y1, Z1, Y3, Z3, Y);
                }
            }
            else if (GetInt1 && GetInt3)
            {
                Result = GetInt(Y1, Z1, Y3, Z3, Y);
                if ((Result == Consts.NullDouble) && GetInt2)
                {
                    Result = GetInt(Y2, Z2, Y3, Z3, Y);
                }
            }
            else if (GetInt2 && GetInt3)
            {
                Result = GetInt(Y2, Z2, Y3, Z3, Y);
                if ((Result == Consts.NullDouble) && GetInt1)
                {
                    Result = GetInt(Y1, Z1, Y3, Z3, Y);
                }
            }

            return Result;
        }

        /// <summary>
        /// Determine if the given point (X, Y) is inside the triangle defined by three XYZ points
        /// </summary>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="P3"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        public static bool PointInTriangle(XYZ P1, XYZ P2, XYZ P3, double X, double Y)
        {
            XYZ TestPos = new XYZ(X, Y, 0);
            bool IsOnRight = PointOnRight(P1, P2, TestPos);

            return (IsOnRight == PointOnRight(P2, P3, TestPos)) && (IsOnRight == PointOnRight(P3, P1, TestPos));
        }

        /// <summary>
        /// Determine if the given point (X, Y) is inside the triangle defined by three XYZ points. This method will return
        /// true if the point is coincident with any of the three side on the triangle
        /// </summary>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="P3"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        public static bool PointInTriangleInclusive(XYZ P1, XYZ P2, XYZ P3, double X, double Y)
        {
            XYZ TestPos = new XYZ(X, Y, 0);
            bool IsOnRight = PointOnOrOnRight(P1, P2, TestPos);

            return (IsOnRight == PointOnOrOnRight(P2, P3, TestPos)) && (IsOnRight == PointOnOrOnRight(P3, P1, TestPos));
        }

        /// <summary>
        /// Calculate the scaled unit vector from the vector given by (0, 0, 0) -> XYZ point V
        /// </summary>
        /// <param name="V"></param>
        /// <returns></returns>
        public static XYZ UnitVector(XYZ V) => V * (1 / VectorLength(V));

        /// <summary>
        /// Default override for the GetHashCode method. This returns the base GetHashCode() result.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Overloaded Equals() accepting generic object type parameter
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return Equals((XYZ)obj);
        }
    }
}
