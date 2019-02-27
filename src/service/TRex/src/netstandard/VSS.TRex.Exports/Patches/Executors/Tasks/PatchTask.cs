﻿using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Reflection;
using VSS.TRex.Pipelines.Tasks;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Exports.Patches.Executors.Tasks
{
  /// <summary>
  /// The task responsible for receiving sub grids to be aggregated into a Patch response
  /// </summary>
  public class PatchTask : PipelinedSubGridTask
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    /// <summary>
    /// The collection of sub grids being collected for a patch response
    /// </summary>
    public List<IClientLeafSubGrid> PatchSubGrids = new List<IClientLeafSubGrid>();

    public PatchTask()
    { }

    /// <summary>
    /// Constructs the patch task
    /// </summary>
    /// <param name="requestDescriptor"></param>
    /// <param name="tRexNodeId"></param>
    /// <param name="gridDataType"></param>
    public PatchTask(Guid requestDescriptor, string tRexNodeId, GridDataType gridDataType) : base(requestDescriptor, tRexNodeId, gridDataType)
    {
    }

    /// <summary>
    /// Accept a sub grid response from the processing engine and incorporate into the result for the request.
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    public override bool TransferResponse(object response)
    {
      // Log.InfoFormat("Received a SubGrid to be processed: {0}", (response as IClientLeafSubGrid).Moniker());

      if (!base.TransferResponse(response))
        return false;

      if (!(response is IClientLeafSubGrid[] subGridResponses) || subGridResponses.Length == 0)
      {
        Log.LogWarning("No sub grid responses returned");
        return false;
      }

      foreach (var subGrid in subGridResponses)
      {
        if (subGrid == null)
          continue;

        PatchSubGrids.Add(subGrid);
      }

      return true;
    }
  }
}
