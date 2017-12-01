﻿using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Resource;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.GridFabric.Caches;
using VSS.VisionLink.Raptor.GridFabric.Grids;
using VSS.VisionLink.Raptor.Storage;

namespace VSS.VisionLink.Raptor.ExistenceMaps.Servers
{
    /// <summary>
    /// A server representing access operations for existance maps derived from topologic surfaces such as TTM designs
    /// and surveyed surfaces
    /// </summary>
    public class ExistenceMapServer
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Ignite instance to be used in the server
        /// </summary>
        [InstanceResource]
        private readonly IIgnite ignite = null;

        /// <summary>
        /// A cache that holds the existance maps derived from designfiles (eg: TTM files)
        /// Each existence map is stored in it's serialised byte stream from. It does not define the grid per se, but does
        /// define a cache that is used within the grid to stored existence maps
        /// </summary>
        protected ICache<String, byte[]> DesignTopologyExistanceMapsCache = null;

        /// <summary>
        /// Internal static instance variable for the server
        /// </summary>
        private static ExistenceMapServer _Instance = null;

        /// <summary>
        /// Creates or returns the singleton instance
        /// </summary>
        /// <returns></returns>
        public static ExistenceMapServer Instance() => _Instance ?? (_Instance = new ExistenceMapServer());

        /// <summary>
        /// Default no-arg constructor that creates the Ignite cache within the server
        /// </summary>
        public ExistenceMapServer()
        {
            if (ignite == null)
            {
                ignite = Ignition.TryGetIgnite(RaptorGrids.RaptorGridName());
            }

            if (ignite == null)
            {
                Log.InfoFormat($"Failed to get Ignite reference in {this}");
                throw new ArgumentException("No Ignite instance available");
            }

            try
            {
                DesignTopologyExistanceMapsCache = ignite.GetCache<String, byte[]>(RaptorCaches.DesignTopologyExistenceMapsCacheName());
            }
            catch // Exception is thrown if the cache does not exist
            {
                DesignTopologyExistanceMapsCache = ignite.GetOrCreateCache<String, byte[]>(ConfigureDesignTopologyExistanceMapsCache());
            }

            if (DesignTopologyExistanceMapsCache == null)
            {
                Log.InfoFormat($"Failed to get or create Ignite cache {RaptorCaches.DesignTopologyExistenceMapsCacheName()}");
                throw new ArgumentException("Ignite cache not available");
            }
        }

        /// <summary>
        /// Configure the parameters of the existence map cache
        /// </summary>
        /// <param name="cfg"></param>
        public CacheConfiguration ConfigureDesignTopologyExistanceMapsCache()
        {
            return new CacheConfiguration()
            {
                Name = RaptorCaches.DesignTopologyExistenceMapsCacheName(),

                // cfg.CopyOnRead = false;   Leave as default as should have no effect with 2.1+ without on heap caching enabled
                KeepBinaryInStore = false,

                // Replicate the maps across nodes
                CacheMode = CacheMode.Replicated,

                // No backups for now
                Backups = 0,

                DataRegionName = DataRegions.SPATIAL_EXISTENCEMAP_DATA_REGION
            };
        }

        /// <summary>
        /// Get a specific existance map given its key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public byte [] GetExistenceMap(string key)
        {
            try
            {
                return DesignTopologyExistanceMapsCache.Get(key);
            }
            catch (KeyNotFoundException)
            {
                // If the key is not present, return a null/empty array
                return null;
            }
            catch
            {
                // For other oddities, throw it!
                throw;
            }            
        }

        /// <summary>
        /// Set or update a given existence map given its key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="map"></param>
        public void SetExistenceMap(string key, byte [] map)
        {
            try
            {
                DesignTopologyExistanceMapsCache.Put(key, map);
            }
            catch
            {
                // Something bad happened, bring it to attention of authorities
                throw;
            }
        }
    }
}
