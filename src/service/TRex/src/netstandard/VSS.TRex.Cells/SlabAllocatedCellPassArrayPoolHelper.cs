﻿using VSS.TRex.DI;
using VSS.TRex.IO;

namespace VSS.TRex.Cells
{
  public static class SlabAllocatedCellPassArrayPoolHelper
  {
    private static ISlabAllocatedArrayPool<CellPass> _cache;
    public static ISlabAllocatedArrayPool<CellPass> Caches => _cache ?? (_cache = DIContext.Obtain<ISlabAllocatedArrayPool<CellPass>>());
  }
}
