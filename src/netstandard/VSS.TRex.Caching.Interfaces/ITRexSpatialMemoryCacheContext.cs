﻿using System;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Caching.Interfaces
{
  public interface ITRexSpatialMemoryCacheContext
  {
    ITRexSpatialMemoryCache OwnerMemoryCache { get; }

    IGenericSubGridTree_Int ContextTokens { get; }

    ITRexSpatialMemoryCacheStorage<ITRexMemoryCacheItem> MRUList { get; }

    int TokenCount { get; }

    bool Add(ITRexMemoryCacheItem element);

    void Remove(ITRexMemoryCacheItem element);

    ITRexMemoryCacheItem Get(uint originX, uint originY);

    void RemoveFromContextTokensOnly(ITRexMemoryCacheItem item);

    TimeSpan CacheDurationTime { get; }
  }
}
