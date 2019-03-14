﻿using System;
using System.IO;
using System.Linq;
using VSS.TRex.Common.Utilities.Interfaces;

namespace VSS.TRex.SubGridTrees.Client.Types
{
  public struct ClientCellProfileAllPassesLeafSubgridRecord : IBinaryReaderWriter, IEquatable<ClientCellProfileAllPassesLeafSubgridRecord>
  {
    public int TotalPasses { get; set; }

    public ClientCellProfileLeafSubgridRecord[] CellPasses { get; set; }

    public void Clear()
    {
      TotalPasses = 0;
      CellPasses = new ClientCellProfileLeafSubgridRecord[0];
    }

    public static ClientCellProfileAllPassesLeafSubgridRecord Null()
    {
      var record = new ClientCellProfileAllPassesLeafSubgridRecord();
      record.Clear();
      return record;
    }

    public void Read(BinaryReader reader)
    {
      TotalPasses = reader.ReadInt32();

      if (TotalPasses > 0)
      {
        CellPasses = new ClientCellProfileLeafSubgridRecord[TotalPasses];

        for (int i = 0; i < TotalPasses; i++)
          CellPasses[i].Read(reader);
      }
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(TotalPasses);
      for (int i = 0; i < TotalPasses; i++)
        CellPasses[i].Write(writer);
    }

    public void Write(BinaryWriter writer, byte[] buffer) => Write(writer);

    public bool Equals(ClientCellProfileAllPassesLeafSubgridRecord other)
    {
      return TotalPasses == other.TotalPasses && 
             ((other.TotalPasses == 0 && TotalPasses == 0) ||
             !CellPasses.Where((x, i) => !x.Equals(other.CellPasses[i])).Any());
    }

    public override bool Equals(object obj)
    {
      return !ReferenceEquals(null, obj)
        && (obj is ClientCellProfileAllPassesLeafSubgridRecord other && Equals(other));
    }
  }
}
