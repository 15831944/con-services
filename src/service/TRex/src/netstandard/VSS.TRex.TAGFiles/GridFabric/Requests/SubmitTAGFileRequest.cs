﻿using System.Threading.Tasks;
using Apache.Ignite.Core.Compute;
using VSS.TRex.TAGFiles.GridFabric.Arguments;
using VSS.TRex.TAGFiles.GridFabric.ComputeFuncs;
using VSS.TRex.TAGFiles.GridFabric.Responses;

namespace VSS.TRex.TAGFiles.GridFabric.Requests
{
  /// <summary>
  /// Supports submitting a single TAG file to be considered for processing depending on TAG File Authorization checks.
  /// </summary>
  public class SubmitTAGFileRequest : TAGFileProcessingPoolRequest<SubmitTAGFileRequestArgument, SubmitTAGFileResponse>
  {
    /// <summary>
    /// Local reference to the compute func used to execute the submission request on the grid.
    /// </summary>
    private IComputeFunc<SubmitTAGFileRequestArgument, SubmitTAGFileResponse> func;

    /// <summary>
    /// No-arg constructor that creates a default TAG file submission request with a singleton ComputeFunc
    /// </summary>
    public SubmitTAGFileRequest()
    {
      // Construct the function to be used
      func = new SubmitTAGFileComputeFunc();
    }

    /// <summary>
    /// Processes a set of TAG files from a machine into a project synchronously
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    public override SubmitTAGFileResponse Execute(SubmitTAGFileRequestArgument arg) => Compute.Apply(func, arg);

    /// <summary>
    /// Processes a set of TAG files from a machine into a project asynchronously
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    public override Task<SubmitTAGFileResponse> ExecuteAsync(SubmitTAGFileRequestArgument arg) => Compute.ApplyAsync(func, arg);
  }
}
