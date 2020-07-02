﻿using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using System.Collections.Generic;
using VSS.TRex.DI;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.ExistenceMaps.Interfaces;
using VSS.TRex.GridFabric;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Storage.Models;

namespace VSS.TRex.ExistenceMaps.Servers
{
  /// <summary>
    /// A server representing access operations for existence maps derived from topological surfaces such as TTM designs
    /// and surveyed surfaces
    /// </summary>
    public class ExistenceMapServer : IExistenceMapServer
    {
        /// <summary>
        /// A cache that holds the existence maps derived from design files (eg: TTM files)
        /// Each existence map is stored in it's serialized byte stream from. It does not define the grid per se, but does
        /// define a cache that is used within the grid to stored existence maps
        /// </summary>
        private readonly ICache<INonSpatialAffinityKey, ISerialisedByteArrayWrapper> _designTopologyExistenceMapsCache;

        /// <summary>
        /// Default no-arg constructor that creates the Ignite cache within the server
        /// </summary>
        public ExistenceMapServer()
        {
            var ignite = DIContext.Obtain<ITRexGridFactory>()?.Grid(StorageMutability.Immutable);
            
            _designTopologyExistenceMapsCache = ignite?.GetOrCreateCache<INonSpatialAffinityKey, ISerialisedByteArrayWrapper>(ConfigureDesignTopologyExistenceMapsCache());

            if (_designTopologyExistenceMapsCache == null)
                throw new TRexException($"Failed to get or create Ignite cache {TRexCaches.DesignTopologyExistenceMapsCacheName()}, ignite reference is {ignite}");
        }

        /// <summary>
        /// Configure the parameters of the existence map cache
        /// </summary>
        private CacheConfiguration ConfigureDesignTopologyExistenceMapsCache()
        {
            return new CacheConfiguration
            {
                Name = TRexCaches.DesignTopologyExistenceMapsCacheName(),

                // cfg.CopyOnRead = false;   Leave as default as should have no effect with 2.1+ without on heap caching enabled
                KeepBinaryInStore = true,

                // Replicate the maps across nodes
                CacheMode = CacheMode.Replicated,

                Backups = 0,  // No backups need as it is a replicated cache

                DataRegionName = DataRegions.SPATIAL_EXISTENCEMAP_DATA_REGION
            };
        }

        /// <summary>
        /// Get a specific existence map given its key
        /// </summary>
        public ISerialisedByteArrayWrapper GetExistenceMap(INonSpatialAffinityKey key)
        {
            try
            {
                return _designTopologyExistenceMapsCache.Get(key);
            }
            catch (KeyNotFoundException)
            {
                // If the key is not present, return a null/empty array
                return null;
            }
        }

        /// <summary>
        /// Set or update a given existence map given its key.
        /// </summary>
        public void SetExistenceMap(INonSpatialAffinityKey key, ISerialisedByteArrayWrapper map)
        {
            _designTopologyExistenceMapsCache.Put(key, map);
        }
    }
}
