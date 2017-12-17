﻿using Apache.Ignite.Core.Cluster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Servers;

namespace VSS.VisionLink.Raptor.GridFabric.NodeFilters
{
    /// <summary>
    /// Defines a node filter that filters nodes based on a defined role attribute
    /// </summary>
    public class RoleBasedClientNodeFilter : RoleBasedNodeFilter
    {
        /// <summary>
        ///  Default no-arg constructor
        /// </summary>
        public RoleBasedClientNodeFilter()
        {
        }

        /// <summary>
        /// Constructor accepting the name of the role to filter nodes with
        /// </summary>
        /// <param name="role"></param>
        public RoleBasedClientNodeFilter(string role) : base(role)
        {
        }

        /// <summary>
        /// Implementation of the filter that is provided with node references to determine if they match the filter
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public override bool Invoke(IClusterNode node)
        {
            return node.IsClient && base.Invoke(node);
        }
    }
}
