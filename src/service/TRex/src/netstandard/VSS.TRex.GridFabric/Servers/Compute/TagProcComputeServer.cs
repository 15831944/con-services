﻿using Apache.Ignite.Core;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Servers.Client;

namespace VSS.TRex.GridFabric.Servers.Compute
{
    /// <summary>
    /// Defines a representation of a server responsible for performing TRex related compute operations using
    /// the Ignite In Memory Data Grid
    /// </summary>
    public class TagProcComputeServer : MutableCacheComputeServer
    {
        //private static readonly ILogger Log = Logging.Logger.CreateLogger<TagProcComputeServer>();

        /// <summary>
        /// A client reference to the immutable data grid for the TAG file processing logic to write immutable versions
        /// of the data being processed from TAG files into.
        /// </summary>
        public ImmutableClientServer ImmutableClientServer { get; }

        public override void ConfigureTRexGrid(IgniteConfiguration cfg)
        {
            base.ConfigureTRexGrid(cfg);

            cfg.UserAttributes.Add($"{ServerRoles.ROLE_ATTRIBUTE_NAME}-{ServerRoles.TAG_PROCESSING_NODE}", "True");
            cfg.UserAttributes.Add($"{ServerRoles.ROLE_ATTRIBUTE_NAME}-{ServerRoles.DATA_MUTATION_ROLE}", "True");
        }

        /// <summary>
        /// Constructor for the TRex cache compute server node. Responsible for starting all Ignite services and creating the grid
        /// and cache instance in preparation for client access by business logic running on the node.
        /// </summary>
        public TagProcComputeServer()
        {
            ImmutableClientServer = new ImmutableClientServer(ServerRoles.TAG_PROCESSING_NODE_CLIENT);
        }
    }
}
