﻿using System;
using System.Collections.Generic;
using System.IO;

namespace VSS.TRex.SubGridTrees.Server.Interfaces
{
  public interface ISubGridDirectory : IDisposable
  {
    bool IsMutable { get; set; }

    bool ExistsInPersistentStore { get; }

    List<ISubGridCellPassesDataSegmentInfo> SegmentDirectory { get; set; }

    ISubGridCellLatestPassDataWrapper GlobalLatestCells { get; set;  }

    void AllocateGlobalLatestCells();
    void CreateDefaultSegment();
    void Clear();
    void Write(BinaryWriter writer);
    void Read(BinaryReader reader);
    void ReadUnversioned(BinaryReader reader);

    void DumpSegmentDirectoryToLog();
  }
}
