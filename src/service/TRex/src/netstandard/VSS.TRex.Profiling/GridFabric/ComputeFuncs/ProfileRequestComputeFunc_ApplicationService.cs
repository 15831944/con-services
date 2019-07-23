﻿using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx.Synchronous;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.Profiling.Executors;
using VSS.TRex.Profiling.GridFabric.Arguments;
using VSS.TRex.Profiling.GridFabric.Responses;
using VSS.TRex.Profiling.Interfaces;

namespace VSS.TRex.Profiling.GridFabric.ComputeFuncs
{
  /// <summary>
  /// The logic Ignite executes on application service node for profile requests
  /// </summary>
  public class ProfileRequestComputeFunc_ApplicationService<T> : BaseComputeFunc, IComputeFunc<ProfileRequestArgument_ApplicationService, ProfileRequestResponse<T>> where T: class, IProfileCellBase, new()
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<ProfileRequestComputeFunc_ApplicationService<T>>();

    /// <summary>
    /// Delegates processing of the profile like to the cluster compute layer, then aggregates together the fractional responses
    /// received from each participating node in the query
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    public ProfileRequestResponse<T> Invoke(ProfileRequestArgument_ApplicationService arg)
    {
      Log.LogInformation("In Invoke()");

      try
      {
        var executor = new ComputeProfileExecutor_ApplicationService<T>();
        return executor.ExecuteAsync(arg).WaitAndUnwrapException();
      }
      finally
      {
        Log.LogInformation("Out Invoke()");
      }
    }
  }
}

