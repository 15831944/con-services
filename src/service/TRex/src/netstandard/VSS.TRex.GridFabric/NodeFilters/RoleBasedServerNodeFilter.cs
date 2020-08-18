﻿using Apache.Ignite.Core.Cluster;

namespace VSS.TRex.GridFabric.NodeFilters
{
    /// <summary>
    /// Defines a server node filter that filters nodes based on a defined role attribute
    /// </summary>
    public class RoleBasedServerNodeFilter : RoleBasedNodeFilter
    {
        /// <summary>
        ///  Default no-arg constructor
        /// </summary>
        public RoleBasedServerNodeFilter()
        {
        }

        /// <summary>
        /// Constructor accepting the name of the role to filter nodes with
        /// </summary>
        public RoleBasedServerNodeFilter(string role) : base(role)
        {
        }

        /// <summary>
        /// Implementation of the filter that is provided with node references to determine if they match the filter
        /// </summary>
        public override bool Invoke(IClusterNode node)
        {
            return !node.IsClient && base.Invoke(node);
        }
    }
}
