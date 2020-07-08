﻿using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx.Synchronous;
using VSS.TRex.Filters;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.Volumes.Executors;
using VSS.TRex.Volumes.GridFabric.Arguments;
using VSS.TRex.Volumes.GridFabric.Responses;

namespace VSS.TRex.Volumes.GridFabric.ComputeFuncs
{
  /// <summary>
  /// The progressive volumes compute function that runs in the context of the cluster compute nodes. This function
  /// performs a volumes calculation across the partitions on this node only.
  /// </summary>
  public class ProgressiveVolumesRequestComputeFunc_ClusterCompute : BaseComputeFunc, IComputeFunc<ProgressiveVolumesRequestArgument, ProgressiveVolumesResponse>
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<ProgressiveVolumesRequestComputeFunc_ClusterCompute>();

    /// <summary>
    /// Default no-arg constructor that orients the request to the available servers on the immutable grid projection
    /// </summary>
    public ProgressiveVolumesRequestComputeFunc_ClusterCompute()
    {
    }

    /// <summary>
    /// Invoke the progressive volumes request locally on this node
    /// </summary>
    public ProgressiveVolumesResponse Invoke(ProgressiveVolumesRequestArgument arg)
    {
      _log.LogInformation("In ProgressiveVolumesRequestComputeFunc_ClusterCompute.Invoke()");

      try
      {
        var volumes = new ComputeProgressiveVolumes_Coordinator
        (arg.ProjectID,
          arg.LiftParams,
          arg.VolumeType,
          arg.Filters.Filters.Length == 0 ? new CombinedFilter() : arg.Filters.Filters[0],
          arg.BaseDesign,
          arg.TopDesign,
          arg.AdditionalSpatialFilter,
          arg.CutTolerance,
          arg.FillTolerance,
          arg.StartDate,
          arg.EndDate,
          arg.Interval);

        _log.LogInformation("Executing volumes.ExecuteAsync()");

        return volumes.ExecuteAsync().WaitAndUnwrapException();
      }
      finally
      {
        _log.LogInformation("Exiting ProgressiveVolumesRequestComputeFunc_ClusterCompute.Invoke()");
      }
    }
  }
}
