﻿using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.GridFabric.Responses;
using VSS.TRex.Pipelines.Interfaces.Tasks;
using VSS.TRex.SubGrids.GridFabric.Requests;

namespace VSS.TRex.Pipelines
{
  /// <summary>
  /// Defines a generic class that decorates progressive pipeline semantics with the desired argument and request response
  /// </summary>
  /// <typeparam name="TSubGridsRequestArgument"></typeparam>
  /// <typeparam name="TSubGridRequestsResponse"></typeparam>
  public class SubGridPipelineProgressive<TSubGridsRequestArgument, TSubGridRequestsResponse> : 
    SubGridPipelineBase<TSubGridsRequestArgument, TSubGridRequestsResponse, SubGridRequestsProgressive<TSubGridsRequestArgument, TSubGridRequestsResponse>>
    where TSubGridsRequestArgument : SubGridsRequestArgument, new()
    where TSubGridRequestsResponse : SubGridRequestsResponse, IAggregateWith<TSubGridRequestsResponse>, new()
  {
    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public SubGridPipelineProgressive()
    {
    }

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    /// <param name="task"></param>
    public SubGridPipelineProgressive(ITRexTask task) : base(task)
    {
    }
  }
}
