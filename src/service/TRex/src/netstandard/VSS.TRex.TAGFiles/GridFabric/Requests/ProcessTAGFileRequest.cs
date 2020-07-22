﻿using System;
using System.Threading.Tasks;
using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using VSS.TRex.TAGFiles.GridFabric.Arguments;
using VSS.TRex.TAGFiles.GridFabric.ComputeFuncs;
using VSS.TRex.TAGFiles.GridFabric.Responses;

namespace VSS.TRex.TAGFiles.GridFabric.Requests
{
  /// <summary>
  /// Provides a request to process one or more TAG files into a project
  /// </summary>
  public class ProcessTAGFileRequest : TAGFileProcessingPoolRequest<ProcessTAGFileRequestArgument, ProcessTAGFileResponse>
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<ProcessTAGFileRequest>();

    /// <summary>
    /// Local reference to the compute func used to execute the processing request on the grid.
    /// </summary>
    private readonly IComputeFunc<ProcessTAGFileRequestArgument, ProcessTAGFileResponse> func;

    /// <summary>
    /// No-arg constructor that creates a default TAG file submission request with a singleton ComputeFunc
    /// </summary>
    public ProcessTAGFileRequest()
    {
      // Construct the function to be used
      func = new ProcessTAGFileComputeFunc();
    }

    /// <summary>
    /// Processes a set of TAG files from a machine into a project synchronously
    /// </summary>
    public override ProcessTAGFileResponse Execute(ProcessTAGFileRequestArgument arg)
    {
      try
      {
        // Send the appropriate response to the caller
        return Compute.Apply(func, arg);
      }
      catch (Exception e)
      {
        _log.LogError(e, $"Exception occured during execution of {nameof(Execute)}");
        return null;
      }
    }

    /// <summary>
    /// Processes a set of TAG files from a machine into a project asynchronously
    /// </summary>
    public override Task<ProcessTAGFileResponse> ExecuteAsync(ProcessTAGFileRequestArgument arg)
    {
      try
      {
        // Send the appropriate response to the caller
        return Compute.ApplyAsync(func, arg);
      }
      catch (Exception e)
      {
        _log.LogError(e, $"Exception occured during execution of {nameof(Execute)}");
        return Task.FromResult<ProcessTAGFileResponse>(null);
      }
    }
  }
}
