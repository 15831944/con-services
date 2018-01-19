﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.Velociraptor.DesignProfiling.GridFabric.Arguments;
using VSS.Velociraptor.DesignProfiling.GridFabric.Requests;
using VSS.VisionLink.Raptor.Servers.Client;
using VSS.VisionLink.Raptor.SubGridTrees.Client;

namespace VSS.Velociraptor.DesignProfiling.Servers.Client
{
    public class CalculateDesignElevationsServer : RaptorMutableClientServer
    {
        public CalculateDesignElevationsServer() : base("DesignProfiler")
        {
        }

        /// <summary>
        /// Creates a new instance of a design elevation server. 
        /// </summary>
        /// <returns></returns>
        public static CalculateDesignElevationsServer NewInstance()
        {
            return new CalculateDesignElevationsServer();
        }

        /// <summary>
        /// Compute a design elevation patch
        /// </summary>
        /// <param name="argument"></param>
        /// <returns></returns>
        public ClientHeightLeafSubGrid ComputeDesignElvations(CalculateDesignElevationPatchArgument argument)
        {
            DesignElevationPatchRequest request = new DesignElevationPatchRequest();

            return request.Execute(argument);
        }
    }
}
