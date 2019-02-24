﻿using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Microsoft.Extensions.Logging;
using System;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Storage.Caches;
using VSS.TRex.Storage.Models;
using VSS.TRex.TAGFiles.Models;

namespace VSS.TRex.TAGFiles.Classes.Queues
{
    /// <summary>
    /// Represents a buffered queue of TAG files awaiting processing. The queue of TAG files is stored in a 
    /// partitioned Ignite cache based on the ProjectUID
    /// </summary>
    public class TAGFileBufferQueue : ITAGFileBufferQueue
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger<TAGFileBufferQueue>();

        /// <summary>
        /// The Ignite cache reference that holds the TAG files. This cache is keyed on the TAG file name and uses the
        /// ProjectUID field in the queue item to control affinity placement of the TAG files themselves
        /// The key is a string that 
        /// </summary>
        private ICache<ITAGFileBufferQueueKey, TAGFileBufferQueueItem> QueueCache;

        /// <summary>
        /// Creates or obtains a reference to an already created TAG file buffer queue
        /// </summary>
        private void InstantiateCache()
        {
            IIgnite ignite = DIContext.Obtain<ITRexGridFactory>()?.Grid(StorageMutability.Mutable) ?? Ignition.GetIgnite(TRexGrids.MutableGridName());

            QueueCache = ignite.GetCache<ITAGFileBufferQueueKey, TAGFileBufferQueueItem>(TRexCaches.TAGFileBufferQueueCacheName());

            if (QueueCache == null)
            {
                Log.LogInformation($"Failed to get Ignite cache {TRexCaches.TAGFileBufferQueueCacheName()}");
                throw new ArgumentException("Ignite cache not available");
            }
        }

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public TAGFileBufferQueue()
        {
            InstantiateCache();
        }

        /// <summary>
        /// Adds a new TAG file to the buffer queue.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>If an element with this key already exists in the cache this method will false, true otherwise</returns>
        public bool Add(ITAGFileBufferQueueKey key, TAGFileBufferQueueItem value)
        {
            return QueueCache.PutIfAbsent(key, value);
        }
    }
}
