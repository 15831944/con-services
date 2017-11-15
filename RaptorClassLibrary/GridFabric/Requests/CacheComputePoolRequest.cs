﻿using Apache.Ignite.Core.Cluster;
using Apache.Ignite.Core.Compute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.GridFabric.Requests;

namespace VSS.VisionLink.Raptor.Designs.GridFabric.Requests
{
    /// <summary>
    ///  Represents a request that can be made against the design profiler cluster group in the Raptor grid
    /// </summary>
    public class CacheComputePoolRequest : BaseRaptorRequest
    {
        /// <summary>
        /// Default no-arg constructor that sets up cluster and compute projections available for use
        /// </summary>
        public CacheComputePoolRequest() : base("PSNode")
        {
        }
    }
}
