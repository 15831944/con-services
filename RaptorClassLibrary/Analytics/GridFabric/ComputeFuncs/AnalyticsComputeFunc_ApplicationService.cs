﻿using Apache.Ignite.Core.Compute;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Analytics.GridFabric.Arguments;
using VSS.VisionLink.Raptor.Analytics.GridFabric.Reponses;
using VSS.VisionLink.Raptor.GridFabric.ComputeFuncs;
using VSS.VisionLink.Raptor.GridFabric.Grids;
using VSS.VisionLink.Raptor.GridFabric.Requests;
using VSS.VisionLink.Raptor.GridFabric.Requests.Interfaces;
using VSS.VisionLink.Raptor.Servers;

namespace VSS.VisionLink.Raptor.Analytics.GridFabric.ComputeFuncs
{
    /// <summary>
    /// This compute func operates in the context of an application server that reaches out to the compute cluster to 
    /// perform subgrid processing.
    /// </summary>
    public class AnalyticsComputeFunc_ApplicationService<TArgument, TResponse, TRequest> : BaseRaptorComputeFunc_Aggregative<TArgument, TResponse>
        where TArgument : class, new()
        where TResponse : class, IResponseAggregateWith<TResponse>, new()
        where TRequest : class, IComputeFunc<TArgument, TResponse>, new()
    {
        [NonSerialized]
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override TResponse Invoke(TArgument arg)
        {
            Log.Info("In AnalyticsComputeFunc_ApplicationService.Invoke()");

            try
            {
                GenericASNodeRequest<TArgument, TRequest, TResponse> request = new GenericASNodeRequest<TArgument, TRequest, TResponse>();

                Log.Info("Executing AnalyticsComputeFunc_ApplicationService.Execute()");

                return request.Execute(arg);
            }
            finally
            {
                Log.Info("Exiting AnalyticsComputeFunc_ApplicationService.Invoke()");
            }
        }

    }
}
