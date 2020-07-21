﻿using Apache.Ignite.Core.Compute;
using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.GridFabric.Models;
using VSS.TRex.SubGrids.Executors;
using VSS.TRex.SubGrids.GridFabric.Arguments;
using VSS.TRex.SubGrids.Responses;

namespace VSS.TRex.SubGrids.GridFabric.ComputeFuncs
{
  /// <summary>
  /// The closure/function that implements sub grid request processing on compute nodes
  /// </summary>
  public abstract class SubGridsRequestComputeFuncBase<TSubGridsRequestArgument, TSubGridRequestsResponse> : BaseComputeFunc, IComputeFunc<TSubGridsRequestArgument, TSubGridRequestsResponse>
    where TSubGridsRequestArgument : SubGridsRequestArgument
    where TSubGridRequestsResponse : SubGridRequestsResponse, new()
  {
    // ReSharper disable once StaticMemberInGenericType
    private static readonly ILogger _log = Logging.Logger.CreateLogger<SubGridsRequestComputeFuncBase<TSubGridsRequestArgument, TSubGridRequestsResponse>>();

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    protected SubGridsRequestComputeFuncBase()
    {
    }

    protected abstract SubGridsRequestComputeFuncBase_Executor_Base<TSubGridsRequestArgument, TSubGridRequestsResponse> GetExecutor();

    /// <summary>
    /// Invoke function called in the context of the cluster compute node
    /// </summary>
    public TSubGridRequestsResponse Invoke(TSubGridsRequestArgument arg)
    {
      TSubGridRequestsResponse result;

      _log.LogInformation("#In# SubGridsRequestComputeFunc.invoke()");

      try
      {
        try
        {
          var executor = GetExecutor();

          executor.UnpackArgument(arg);

          result = executor.Execute();
        }
        finally
        {
          _log.LogInformation("Out SubGridsRequestComputeFunc.invoke()");
        }
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception occurred in base sub grid request compute function");

        return new TSubGridRequestsResponse {ResponseCode = SubGridRequestsResponseResult.Unknown};
      }

      return result;
    }
  }
}
