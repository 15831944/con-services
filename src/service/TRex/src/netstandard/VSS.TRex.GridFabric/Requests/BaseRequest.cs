﻿using System.Threading.Tasks;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.GridFabric.Interfaces;

namespace VSS.TRex.GridFabric.Requests
{
  /// <summary>
  /// The base class for requests. This provides common aspects such as the injected Ignite instance
  /// </summary>
  public abstract class BaseRequest<TArgument, TResponse> : BaseIgniteClass, IBaseRequest<TArgument, TResponse>, IBaseRequest
  {
    /// <summary>
    /// Constructor accepting a role for the request that may identify a cluster group of nodes in the grid
    /// </summary>
    /// <param name="gridName"></param>
    /// <param name="role"></param>
    public BaseRequest(string gridName, string role) : base(gridName, role)
    {
    }

    public virtual TResponse Execute(TArgument arg)
    {
      // No implementation in base class - complain if we are called
      throw new TRexException($"{nameof(Execute)} invalid to call.");
    }

    public virtual Task<TResponse> ExecuteAsync(TArgument arg)
    {
      // No implementation in base class - complain if we are called
      throw new TRexException($"{nameof(ExecuteAsync)} invalid to call.");
    }
  }
}
