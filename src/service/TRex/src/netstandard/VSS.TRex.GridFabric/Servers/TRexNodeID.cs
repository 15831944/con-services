﻿using System;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.Storage.Models;

namespace VSS.TRex.Servers
{
    /// <summary>
    /// Determines and caches the TRexNodeId set in the attributes for the local Ignite node 
    /// </summary>
    public static class TRexNodeID
    {
        /// <summary>
        /// Local storage for the 
        /// </summary>
        private static readonly Guid[] tRexNodeIDs = {Guid.Empty, Guid.Empty}; 

        public static Guid ThisNodeID(StorageMutability mutability)
        {
            if (tRexNodeIDs[(int)mutability].Equals(Guid.Empty))
            {
                tRexNodeIDs[(int)mutability] = Guid.Parse(DIContext.Obtain<ITRexGridFactory>().Grid(mutability).GetCluster().GetLocalNode().GetAttribute<string>("TRexNodeId"));
            }

            return tRexNodeIDs[(int)mutability];
        }
    }
}
