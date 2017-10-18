﻿using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Discovery.Tcp;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.GridFabric.Grids;

namespace VSS.VisionLink.Raptor.Servers.Compute
{
    /// <summary>
    /// A server type that represents a server useful for context processing sets of SubGrid information. This is essentially an analogue of
    /// the PSNode servers in legacy Raptor and contains both a cache of data and processing against it in response to client context server requests.
    /// Note: These servers typically access the immutable representations of the spatial data for performance reasons, as configured
    /// in the server constructor.
    /// </summary>
    public class RaptorSubGridProcessingServer : RaptorCacheComputeServer
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override void ConfigureRaptorGrid(IgniteConfiguration cfg)
        {
            base.ConfigureRaptorGrid(cfg);

            cfg.UserAttributes.Add("Role", "PSNode");
            cfg.UserAttributes.Add("SpatialDivision", RaptorServerConfig.Instance().SpatialSubdivisionDescriptor);

//            (cfg.DiscoverySpi as TcpDiscoverySpi).LocalPort = 47500 + (int)RaptorServerConfig.Instance().SpatialSubdivisionDescriptor;
        }

        public override void ConfigureNonSpatialMutableCache(CacheConfiguration cfg)
        {
            base.ConfigureNonSpatialMutableCache(cfg);
        }

        public override void ConfigureNonSpatialImmutableCache(CacheConfiguration cfg)
        {
            base.ConfigureNonSpatialImmutableCache(cfg);
        }

        public override void ConfigureMutableSpatialCache(CacheConfiguration cfg)
        {
            base.ConfigureMutableSpatialCache(cfg);
        }

        public override void ConfigureImmutableSpatialCache(CacheConfiguration cfg)
        {
            base.ConfigureImmutableSpatialCache(cfg);
        }

        /// <summary>
        /// Overridden spatial cache instantiation method. This method never creates a new cache but will only get an already existing spatial data cache
        /// </summary>
        /// <param name="CacheCfg"></param>
        /// <returns></returns>
        public override ICache<String, byte[]> InstantiateSpatialCacheReference(CacheConfiguration CacheCfg)
        {
            return base.InstantiateSpatialCacheReference(CacheCfg);
            // return raptorGrid.GetCache<String, byte[]>(CacheCfg.Name);
        }

        /// <summary>
        /// Overridden raptor cache instantiation method. This method never creates a new cache but will only get an already existing spatial data cache
        /// </summary>
        /// <param name="CacheCfg"></param>
        /// <returns></returns>
        public override ICache<String, byte[]> InstantiateRaptorCacheReference(CacheConfiguration CacheCfg)
        {
            return base.InstantiateRaptorCacheReference(CacheCfg);
            // return raptorGrid.GetCache<String, MemoryStream>(CacheCfg.Name);
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public RaptorSubGridProcessingServer() : base()
        {
        }

        /// <summary>
        /// Sets up the loocal server configuration to reflect the requirements of subgrid processing
        /// </summary>
        public override void SetupServerSpecificConfiguration()
        {
            // Enable use of immutable data pools when processing requests
            RaptorServerConfig.Instance().UseMutableSpatialData = false;
            RaptorServerConfig.Instance().UseMutableNonSpatialData = false;

            Log.InfoFormat("RaptorSubGridProcessingServer initialisation: Spatial subdivision descriptor = {0}", RaptorServerConfig.Instance().SpatialSubdivisionDescriptor);
        }
    }
}
