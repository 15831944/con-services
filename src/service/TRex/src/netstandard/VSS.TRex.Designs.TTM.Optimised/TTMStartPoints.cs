﻿using System.IO;
using System.Reflection;

namespace VSS.TRex.Designs.TTM.Optimised
{
  /// <summary>
  ///  The collection of start points defined in the TTM model at the time it was created
  /// </summary>
  public class TTMStartPoints
  {
    /// <summary>
    /// The array of start points read in from the TTM file.
    /// </summary>
    public TTMStartPoint[] Items;

    public int SizeOf() => Items.Length * TTMStartPoint.SizeOf();
    /// <summary>
    /// Reads the collection of start points from the TTM file using the provided reader
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="header"></param>
    public void Read(BinaryReader reader, TTMHeader header)
    {
      Items = new TTMStartPoint[header.NumberOfStartPoints];

      void ReadStartPoint(ref TTMStartPoint startPoint)
      {
        startPoint.Y = Utilities.ReadFloat(reader, header.VertexCoordinateSize) + header.NorthingOffsetValue;
        startPoint.X = Utilities.ReadFloat(reader, header.VertexCoordinateSize) + header.EastingOffsetValue;

        startPoint.Triangle = Utilities.ReadInteger(reader, header.TriangleNumberSize) - 1;
      }

      int loopLimit = header.NumberOfStartPoints;
      for (int i = 0; i < loopLimit; i++)
      {
        long RecPos = reader.BaseStream.Position;
        ReadStartPoint(ref Items[i]);
        reader.BaseStream.Position = RecPos + header.StartPointRecordSize;
      }
    }
  }
}
