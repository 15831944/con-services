﻿using System;
using VSS.TRex.Cells;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.Geometry;

namespace VSS.TRex.SiteModels.Interfaces
{
  public interface ISiteProofingRun : IBinaryReaderWriter
  {
    short MachineID { get; }
    string Name { get; }
    DateTime StartTime { get; set; }
    DateTime EndTime { get; set; }
    BoundingWorldExtent3D Extents { get; set; }
    bool MatchesCellPass(CellPass cellPass);
  }
}
