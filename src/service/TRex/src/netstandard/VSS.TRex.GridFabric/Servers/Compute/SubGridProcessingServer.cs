﻿using Apache.Ignite.Core;
using VSS.TRex.GridFabric.Models.Servers;

namespace VSS.TRex.GridFabric.Servers.Compute
{
    /// <summary>
    /// A server type that represents a server useful for context processing sets of SubGrid information. This is essentially an analogue of
    /// the PSNode servers in legacy TRex and contains both a cache of data and processing against it in response to client context server requests.
    /// Note: These servers typically access the immutable representations of the spatial data for performance reasons, as configured
    /// in the server constructor.
    /// </summary>
    public class SubGridProcessingServer : ImmutableCacheComputeServer
    {
        public override void ConfigureTRexGrid(IgniteConfiguration cfg)
        {
            base.ConfigureTRexGrid(cfg);

            cfg.UserAttributes.Add($"{ServerRoles.ROLE_ATTRIBUTE_NAME}-{ServerRoles.PSNODE}", "True");
            cfg.UserAttributes.Add($"{ServerRoles.ROLE_ATTRIBUTE_NAME}-{ServerRoles.RECEIVES_SITEMODEL_CHANGE_EVENTS}", "True");
        }

    /// <summary>
    /// Default constructor
    /// </summary>
    public SubGridProcessingServer() : base()
        {
          SetupServerSpecificConfiguration();
        }

        /// <summary>
        /// Sets up the local server configuration to reflect the requirements of sub grid processing
        /// </summary>
        public void SetupServerSpecificConfiguration()
        {
        }
    }
}
