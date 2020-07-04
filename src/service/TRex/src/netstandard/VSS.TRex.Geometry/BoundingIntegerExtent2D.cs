﻿using System;

namespace VSS.TRex.Geometry
{
  public struct BoundingIntegerExtent2D : IEquatable<BoundingIntegerExtent2D>
  {
    public int MinX, MinY, MaxX, MaxY;

    /// <summary>
    /// Calculates the number of cells this integer extent covers
    /// </summary>
    public long Area() => SizeX * (long)SizeY;

    /// <summary>
    /// Assign the context of another 3D bounding extent to this one
    /// </summary>
    public void Assign(BoundingIntegerExtent2D source)
    {
      MinX = source.MinX;
      MinY = source.MinY;
      MaxX = source.MaxX;
      MaxY = source.MaxY;
    }

    /// <summary>
    /// Produce as human readable form of the state in this bounding extent
    /// </summary>
    public override string ToString() => $"MinX: {MinX}, MinY:{MinY}, MaxX: {MaxX}, MaxY:{MaxY}";

    /// <summary>
    /// Construct a 2D bounding extent from the supplied parameters
    /// </summary>
    public BoundingIntegerExtent2D(int AMinX, int AMinY, int AMaxX, int AMaxY)
    {
      MinX = AMinX;
      MinY = AMinY;
      MaxX = AMaxX;
      MaxY = AMaxY;
    }

    /// <summary>
    /// Determine is this bounding extent encloses the extent provided as a parameter
    /// </summary>
    public bool Encloses(BoundingIntegerExtent2D AExtent)
    {
      return IsValidExtent && AExtent.IsValidExtent &&
             (MinX <= AExtent.MinX) && (MinY <= AExtent.MinY) &&
             (MaxX >= AExtent.MaxX) && (MaxY >= AExtent.MaxY);
    }

    /// <summary>
    /// Expand the extent covered in X & Y isotropically using the supplied Delta
    /// </summary>
    public void Expand(int Delta)
    {
      MinX -= Delta;
      MaxX += Delta;
      MinY -= Delta;
      MaxY += Delta;
    }

    /// <summary>
    /// Include the integer location coordinate specified by the X and Y parameters into the 2D bounding extent
    /// </summary>
    public void Include(int X, int Y)
    {
      if (MinX > X) MinX = X;
      if (MaxX < X) MaxX = X;
      if (MinY > Y) MinY = Y;
      if (MaxY < Y) MaxY = Y;
    }

    /// <summary>
    /// Include the extent contained in the parameter into the 2D bounding extent
    /// </summary>
    public void Include(BoundingIntegerExtent2D Extent)
    {
      if (Extent.IsValidExtent)
      {
        Include(Extent.MinX, Extent.MinY);
        Include(Extent.MaxX, Extent.MaxY);
      }
    }

    /// <summary>
    /// Determine if the 2D bounding extent includes the coordinate given by the X and Y parameters
    /// </summary>
    /// <returns>A boolean indicating where the bounding extent includes the given position</returns>
    public bool Includes(int x, int y) => x >= MinX && x <= MaxX && y >= MinY && y <= MaxY;

    public bool Includes(uint x, uint y) => x >= MinX && x <= MaxX && y >= MinY && y <= MaxY;

    /// <summary>
    /// Determine if the extent defined is valid in that it does not define a negative area
    /// </summary>
    public bool IsValidExtent => MaxX >= MinX && MaxY >= MinY;

    /// <summary>
    /// Move the 2D bounding extent int he X and Y dimensions by the delta X & Y supplied in the parameters
    /// </summary>
    public void Offset(int DX, int DY)
    {
      MinX += DX;
      MaxX += DX;
      MinY += DY;
      MaxY += DY;
    }

    public void SetInverted()
    {
      MinX = int.MaxValue;
      MaxX = int.MinValue;
      MinY = int.MaxValue;
      MaxY = int.MinValue;
    }

    /// <summary>
    /// Compute the size of the X dimension in the bounding extents
    /// </summary>
    public int SizeX => MaxX - MinX;

    /// <summary>
    /// Compute the size of the Y dimension in the bounding extents
    /// </summary>
    public int SizeY => MaxY - MinY;

    /// <summary>
    /// Determine if this bounding extent equals another bounding extent instance
    /// </summary>
    public bool Equals(BoundingIntegerExtent2D extent) => MinX == extent.MinX && MinY == extent.MinY && MaxX == extent.MaxX && MaxY == extent.MaxY;

    /// <summary>
    /// Creates a new 2D bounding extents structure with the corner points 'inverted'
    /// </summary>
    public static BoundingIntegerExtent2D Inverted()
    {
      BoundingIntegerExtent2D result = new BoundingIntegerExtent2D();
      result.SetInverted();

      return result;
    }
  }
}
