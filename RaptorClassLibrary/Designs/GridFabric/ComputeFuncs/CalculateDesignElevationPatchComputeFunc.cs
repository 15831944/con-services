﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apache.Ignite.Core.Compute;
using VSS.Velociraptor.DesignProfiling.GridFabric.Arguments;
using VSS.VisionLink.Raptor.SubGridTrees.Client;
using VSS.Velociraptor.DesignProfiling.Executors;
using log4net;
using System.Reflection;
using VSS.VisionLink.Raptor.GridFabric.ComputeFuncs;

namespace VSS.Velociraptor.DesignProfiling.GridFabric.ComputeFuncs
{
    [Serializable]
    public class CalculateDesignElevationPatchComputeFunc : /*BaseRaptorComputeFunc, */ IComputeFunc<CalculateDesignElevationPatchArgument, byte [] /* ClientHeightLeafSubGrid */>
    {
        [NonSerialized]
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public byte[] /*ClientHeightLeafSubGrid */Invoke(CalculateDesignElevationPatchArgument arg)
        {
            try
            {
                Log.InfoFormat("CalculateDesignElevationPatchComputeFunc: Arg = {0}", arg);

                CalculateDesignElevationPatch Executor = new CalculateDesignElevationPatch(arg);

                return Executor.Execute().ToByteArray();
            }
            catch (Exception E)
            {
                Log.InfoFormat("Exception:", E);
                return null; // Todo .....
            }
        }
    }
}
