﻿using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Cache.Eviction;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.GridFabric.Grids;

namespace VSS.VisionLink.Raptor.Servers.Client
{
    /// <summary>
    /// Defines a representation of a client able to request Raptor related compute operations using
    /// the Ignite In Memory Data Grid. All client type server classes should descend from this class.
    /// </summary>
    public class RaptorClientServer : RaptorIgniteServer
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public RaptorClientServer(string role) : base()
        {
            if (raptorGrid == null)
            {
                // Attempt to attach to an already existing Ignite instance
                raptorGrid = Ignition.TryGetIgnite(RaptorGrids.RaptorGridName());

                // If there was no connection obtained, attempt to create a new instance
                if (raptorGrid == null)
                {
                    RaptorNodeID = Guid.NewGuid().ToString();

                    Log.InfoFormat("Creating new Ignite node with Role = {0} & RaptorNodeID = {1}", role, RaptorNodeID);

                    IgniteConfiguration cfg = new IgniteConfiguration()
                    {
                        GridName = RaptorGrids.RaptorGridName(),
                        IgniteInstanceName = RaptorGrids.RaptorGridName(),
                        ClientMode = true,

                        JvmInitialMemoryMb = 512, // Set to minimum advised memory for Ignite grid JVM of 512Mb
                        JvmMaxMemoryMb = 4 * 1024, // Set max to 4Gb
  
                        UserAttributes = new Dictionary<string, object>() { { "Role", role }, { "RaptorNodeID", RaptorNodeID } }
                    };

                    try
                    {
                        raptorGrid = Ignition.Start(cfg);
                    }
                    catch (Exception e)
                    {
                        Log.InfoFormat("Creation of new Ignite node with Role = {0} & RaptorNodeID = {1} failed with exception {2}", role, RaptorNodeID, e);
                    }
                    finally
                    {
                        Log.InfoFormat("Completed creation of new Ignite node with Role = {0} & RaptorNodeID = {1}", role, RaptorNodeID);
                    }
                }
            }
        }

        public override ICache<String, MemoryStream> InstantiateRaptorCacheReference(CacheConfiguration CacheCfg)
        {
            return raptorGrid.GetCache<String, MemoryStream>(CacheCfg.Name);
        }

        public override ICache<String, MemoryStream> InstantiateSpatialCacheReference(CacheConfiguration CacheCfg)
        {
            return raptorGrid.GetCache<String, MemoryStream>(CacheCfg.Name);
        }       
    }
}
