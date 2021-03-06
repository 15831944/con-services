﻿using System;
using System.Collections.Generic;
using System.IO;

namespace VSS.TRex.Designs.TTM
{
  public class TTMStartPoints : List<TTMStartPoint>
  {
    public void Write(BinaryWriter writer, TTMHeader header)
    {
      for (int i = 0; i < Count; i++)
        this[i].Write(writer, header);
    }

    public void Read(BinaryReader reader, TTMHeader header, Triangles triangles)
    {
      Capacity = header.NumberOfStartPoints;

      for (int i = 0; i < header.NumberOfStartPoints; i++)
      {
        long RecPos = reader.BaseStream.Position;
        TTMStartPoint Pt = new TTMStartPoint(0, 0, null);
        Add(Pt);
        Pt.Read(reader, header, triangles);
        reader.BaseStream.Position = RecPos + header.StartPointRecordSize;
      }
    }
  }
}
