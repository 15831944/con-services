﻿using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache.Configuration;
using System;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.Storage.Models;

namespace VSS.TRex.GridFabric.Servers
{
    /// <summary>
    /// A base class for deriving server and client instances that interact with the Ignite In Memory Data Grid
    /// </summary>
    public abstract class IgniteServer : IDisposable
    {
        public const string IGNITE_PUBLIC_THREAD_POOL_SIZE = "IGNITE_PUBLIC_THREAD_POOL_SIZE";
        public const int DEFAULT_IGNITE_PUBLIC_THREAD_POOL_SIZE = 500;

        public const string IGNITE_SYSTEM_THREAD_POOL_SIZE = "IGNITE_SYSTEM_THREAD_POOL_SIZE";
        public const int DEFAULT_IGNITE_SYSTEM_THREAD_POOL_SIZE = 500;

        public const string IGNITE_JVM_MAX_HEAP_SIZE_MB = "IGNITE_JVM_MAX_HEAP_SIZE_MB";
        public const int DEFAULT_IGNITE_JVM_MAX_HEAP_SIZE_MB = 1 * 1024;

        public const string IGNITE_JVM_INITIAL_HEAP_SIZE_MB = "IGNITE_JVM_INITIAL_HEAP_SIZE_MB";
        public const int DEFAULT_IGNITE_JVM_INITIAL_HEAP_SIZE_MB = 512;

        public const string PROGRESSIVE_REQUEST_CUSTOM_POOL_SIZE = "PROGRESSIVE_REQUEST_CUSTOM_POOL_SIZE";
        public const int DEFAULT_PROGRESSIVE_REQUEST_CUSTOM_POOL_SIZE = 8;

        /// <summary>
        /// The mutable Ignite grid reference maintained by this server instance
        /// </summary>
        protected IIgnite mutableTRexGrid = null;

        /// <summary>
        /// The immutable Ignite grid reference maintained by this server instance
        /// </summary>
        protected IIgnite immutableTRexGrid = null;

        /// <summary>
        /// A unique identifier for this server that may be used by business logic executing on other nodes in the grid to locate it if needed for messaging
        /// </summary>
        public Guid TRexNodeID = Guid.Empty;

        /// <summary>
        /// Base configuration for the grid
        /// </summary>
        /// <param name="cfg"></param>
        public virtual void ConfigureTRexGrid(IgniteConfiguration cfg)
        {
        }

        /// <summary>
        /// Base configuration for the mutable non-spatial cache
        /// </summary>
        /// <param name="cfg"></param>
        public virtual void ConfigureNonSpatialMutableCache(CacheConfiguration cfg)
        {
            cfg.DataRegionName = DataRegions.MUTABLE_NONSPATIAL_DATA_REGION;
        }

        /// <summary>
        /// Base configuration for the immutable non-spatial cache
        /// </summary>
        /// <param name="cfg"></param>
        public virtual void ConfigureNonSpatialImmutableCache(CacheConfiguration cfg)
        {
            cfg.DataRegionName = DataRegions.IMMUTABLE_NONSPATIAL_DATA_REGION;
        }

        /// <summary>
        /// Base configuration for the mutable spatial cache
        /// </summary>
        /// <param name="cfg"></param>
        public virtual void ConfigureMutableSpatialCache(CacheConfiguration cfg)
        {
            cfg.DataRegionName = DataRegions.MUTABLE_SPATIAL_DATA_REGION;
        }

        /// <summary>
        /// Base configuration for the immutable spatial cache
        /// </summary>
        /// <param name="cfg"></param>
        public virtual void ConfigureImmutableSpatialCache(CacheConfiguration cfg)
        {
            cfg.DataRegionName = DataRegions.IMMUTABLE_SPATIAL_DATA_REGION;
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
          DIContext.Obtain<ITRexGridFactory>()?.StopGrids();
        }
    }
}
