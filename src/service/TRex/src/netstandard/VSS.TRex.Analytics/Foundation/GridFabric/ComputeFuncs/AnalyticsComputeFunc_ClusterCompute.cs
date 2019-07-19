﻿using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Nito.AsyncEx.Synchronous;
using VSS.TRex.Analytics.Foundation.Coordinators;
using VSS.TRex.Analytics.Foundation.GridFabric.Responses;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.GridFabric.Interfaces;

namespace VSS.TRex.Analytics.Foundation.GridFabric.ComputeFuncs
{
    public class AnalyticsComputeFunc_ClusterCompute<TArgument, TResponse, TCoordinator> : BaseComputeFunc, IComputeFunc<TArgument, TResponse>
        where TArgument : BaseApplicationServiceRequestArgument
        where TResponse : BaseAnalyticsResponse, IAggregateWith<TResponse>, new()
        where TCoordinator : BaseAnalyticsCoordinator<TArgument, TResponse>, new()
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

        public AnalyticsComputeFunc_ClusterCompute()
        {
        }

        /// <summary>
        /// Invoke the statistics request locally on this node
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public TResponse Invoke(TArgument arg)
        {
            Log.LogInformation("In AnalyticsComputeFunc_ClusterCompute.Invoke()");

            try
            {
                Log.LogInformation("Executing AnalyticsComputeFunc_ClusterCompute.Execute()");

                var coordinator = new TCoordinator();
                return coordinator.ExecuteAsync(arg).WaitAndUnwrapException();
            }
            finally
            {
                Log.LogInformation("Exiting AnalyticsComputeFunc_ClusterCompute.Invoke()");
            }
        }
    }
}
