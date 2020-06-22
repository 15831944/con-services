﻿using Apache.Ignite.Core.Compute;
using Nito.AsyncEx.Synchronous;
using VSS.TRex.TAGFiles.Executors;
using VSS.TRex.TAGFiles.GridFabric.Arguments;
using VSS.TRex.TAGFiles.GridFabric.Responses;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.TAGFiles.Models;

namespace VSS.TRex.TAGFiles.GridFabric.ComputeFuncs
{
  public class SubmitTAGFileComputeFunc : BaseComputeFunc, IComputeFunc<SubmitTAGFileRequestArgument, SubmitTAGFileResponse>
  {
    /// <summary>
    /// Default no-arg constructor that orients the request to the available TAG processing server nodes on the mutable grid projection
    /// </summary>
    public SubmitTAGFileComputeFunc()
    {
    }

    /// <summary>
    /// The Invoke method for the compute func - calls the TAG file processing executor to do the work
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    public SubmitTAGFileResponse Invoke(SubmitTAGFileRequestArgument arg)
    {
      var executor = new SubmitTAGFileExecutor();
      return executor.ExecuteAsync(arg.ProjectID, arg.AssetID, arg.TAGFileName, arg.TagFileContent, arg.TCCOrgID, arg.TreatAsJohnDoe, 
        arg.Flags.HasFlag(TAGFileSubmissionFlags.AddToArchive)).WaitAndUnwrapException();
    }
  }
}
