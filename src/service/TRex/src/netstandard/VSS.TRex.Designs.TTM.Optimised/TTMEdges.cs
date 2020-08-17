﻿using System.IO;
using Microsoft.AspNetCore.Http.Features;

namespace VSS.TRex.Designs.TTM.Optimised
{
  /// <summary>
  /// Contains the set of triangles that form the edge of the TIN. Note, in the optimised model, while these are read
  /// neighbour information is not meaning there is not understanding of which side of the triangle is the edge
  /// </summary>
  public class TTMEdges
  {
    /// <summary>
    /// The collection of edge triangles
    /// </summary>
    public int[] Items;

    /// <summary>
    /// Reads in the collection of edges from the TIN model using the provided reader
    /// </summary>
    public void Read(BinaryReader reader, TTMHeader header)
    {
      Items = new int[header.NumberOfEdgeRecords];

      int loopLimit = header.NumberOfEdgeRecords;
      for (int i = 0; i < loopLimit; i++)
      {
        long RecPos = reader.BaseStream.Position;
        Items[i] = Utilities.ReadInteger(reader, header.TriangleNumberSize) - 1;
        reader.BaseStream.Position = RecPos + header.EdgeRecordSize;
      }
    }

    public int SizeOf() => Items.Length * sizeof(int);
  }
}
