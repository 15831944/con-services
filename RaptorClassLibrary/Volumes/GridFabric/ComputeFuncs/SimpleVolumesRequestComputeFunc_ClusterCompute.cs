﻿using Apache.Ignite.Core.Compute;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.GridFabric.ComputeFuncs;
using VSS.VisionLink.Raptor.GridFabric.Grids;
using VSS.VisionLink.Raptor.Servers;
using VSS.VisionLink.Raptor.Volumes.Executors;
using VSS.VisionLink.Raptor.Volumes.GridFabric.Arguments;
using VSS.VisionLink.Raptor.Volumes.GridFabric.Responses;

namespace VSS.VisionLink.Raptor.Volumes.GridFabric.ComputeFuncs
{
    /// <summary>
    /// The cimple volumes compute function that runs in the context of the cluster compute nodes. This function
    /// performs a volumes calculation across the paritions on this node only.
    /// </summary>
    public class SimpleVolumesRequestComputeFunc_ClusterCompute : BaseRaptorComputeFunc, IComputeFunc<SimpleVolumesRequestArgument, SimpleVolumesResponse>
    {
        [NonSerialized]
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Default no-arg constructor that orients the request to the available PSNODE servers on the immutable grid projection
        /// </summary>
        public SimpleVolumesRequestComputeFunc_ClusterCompute() : base(RaptorGrids.RaptorImmutableGridName(), ServerRoles.PSNODE)
        {
        }

        /// <summary>
        /// Invoke the simple volumes request locally on this node
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public SimpleVolumesResponse Invoke(SimpleVolumesRequestArgument arg)
        {
            Log.Info("In SimpleVolumesRequestComputeFunc_ClusterCompute.Invoke()");

            try
            {
                ComputeSimpleVolumes_Coordinator simpleVolumes = new ComputeSimpleVolumes_Coordinator
                    (arg.SiteModelID,
                     arg.VolumeType,
                     arg.BaseFilter,
                     arg.TopFilter,
                     arg.BaseDesignID,
                     arg.TopDesignID,
                     arg.AdditionalSpatialFilter,
                     arg.CutTolerance, 
                     arg.FillTolerance);

                Log.Info("Executing simpleVolumes.Execute()");

                return simpleVolumes.Execute();
            }
            finally
            {
                Log.Info("Exiting SimpleVolumesRequestComputeFunc_ClusterCompute.Invoke()");
            }
        }
    }
}
