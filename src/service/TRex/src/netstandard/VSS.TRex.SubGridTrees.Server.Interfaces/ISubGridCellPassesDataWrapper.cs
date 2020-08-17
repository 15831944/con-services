﻿using System;
using System.Collections.Generic;
using VSS.TRex.GridFabric.Interfaces;

namespace VSS.TRex.SubGridTrees.Server.Interfaces
{
  public interface ISubGridCellPassesDataWrapper
  {
    IServerLeafSubGrid Owner { get; set; }
    ISubGridCellPassesDataSegments PassesData { get; set; }
    void Clear();
    void Initialise();
    ISubGridCellPassesDataSegment SelectSegment(DateTime time);
    bool CleaveSegment(ISubGridCellPassesDataSegment cleavingSegment,
      List<ISubGridCellPassesDataSegment> newSegmentsFromCleaving,
      List<ISubGridSpatialAffinityKey> persistedClovenSegments,
      int subGridSegmentPassCountLimit = 0);

    bool MergeSegments(ISubGridCellPassesDataSegment mergeToSegment,
      ISubGridCellPassesDataSegment mergeFromSegment);

    void RemoveSegment(ISubGridCellPassesDataSegment segment);
  }
}
