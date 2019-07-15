﻿using System;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Microsoft.Extensions.Logging;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.SiteModelChangeMaps.Interfaces.GridFabric.Queues;
using VSS.TRex.Storage.Caches;
using VSS.TRex.Storage.Models;

namespace VSS.TRex.SiteModelChangeMaps.GridFabric.Queues
{
  /// <summary>
  /// Represents a buffered queue of site model spatial change sets. The queue is stored in a
  /// partitioned Ignite cache based on the ProjectUID
  /// </summary>
  public class SiteModelChangeBufferQueue
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<SiteModelChangeBufferQueue>();

    /// <summary>
    /// The Ignite cache reference that holds the TAG files. This cache is keyed on the TAG file name and uses the
    /// ProjectUID field in the queue item to control affinity placement of the TAG files themselves.
    /// </summary>
    private ICache<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem> QueueCache;

    /// <summary>
    /// Creates or obtains a reference to an already created TAG file buffer queue
    /// </summary>
    private void InstantiateCache()
    {
      var ignite = DIContext.Obtain<ITRexGridFactory>()?.Grid(StorageMutability.Mutable) ??
                   Ignition.GetIgnite(TRexGrids.MutableGridName());

      QueueCache = ignite.GetCache<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>(
          TRexCaches.SiteModelChangeBufferQueueCacheName());

      if (QueueCache == null)
      {
        Log.LogInformation($"Failed to get Ignite cache {TRexCaches.SiteModelChangeBufferQueueCacheName()}");
        throw new ArgumentException("Ignite cache not available");
      }
    }

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public SiteModelChangeBufferQueue()
    {
      InstantiateCache();
    }

    /// <summary>
    /// Adds a new TAG file to the buffer queue.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns>If an element with this key already exists in the cache this method will false, true otherwise</returns>
    public bool Add(ISiteModelChangeBufferQueueKey key, SiteModelChangeBufferQueueItem value)
    {
      return QueueCache.PutIfAbsent(key, value);
    }
  }
}

