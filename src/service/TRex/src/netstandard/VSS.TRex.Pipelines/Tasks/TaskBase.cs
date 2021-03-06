﻿using System;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.Pipelines.Interfaces.Tasks;
using VSS.TRex.Types;

namespace VSS.TRex.Pipelines.Tasks
{
  /// <summary>
  /// ITaskBase/TaskBase is an interface/base class implementation that other classes 
  /// may extend to provide specific handling logic for responses from queries, such as 
  /// sub grids and profile sections that require additional processing to arrive at 
  /// the final result (such as a rendered tile)
  /// </summary>
  public abstract class TaskBase : ITRexTask
  {
    /// <summary>
    /// The request descriptor assigned to the task.
    /// </summary>
    public Guid RequestDescriptor { get; set; } = Guid.Empty;

    /// <summary>
    /// Determines if the processing of the task activities as been cancelled by external control
    /// </summary>
    public bool IsCancelled { get; set; }

    /// <summary>
    /// The type of grid data being processed by this task
    /// </summary>
    public GridDataType GridDataType { get; set; } = GridDataType.All;

    /// <summary>
    /// The node wanting to receive the results of task bases sub grid requests to the PSNode clustered processing layer
    /// </summary>
    public Guid TRexNodeID { get; set; } = Guid.Empty;

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public TaskBase()
    {
    }

    /// <summary>
    /// TransferResponse is the sink for responses received from the processing layers.
    /// </summary>
    /// <param name="response"></param>
    public abstract bool TransferResponse(object response);

    /// <summary>
    /// TransferResponses is the sink for sets of responses received from the processing layers.
    /// </summary>
    /// <param name="responses"></param>
    public abstract bool TransferResponses(object[] responses);

    /// <summary>
    /// Cancel sets the cancelled flag to true for the processing engine to take note of and 
    /// take any required actions to cancel an active request.
    /// </summary>
    public virtual void Cancel() => IsCancelled = true;

    /// <summary>
    /// A reference to a sub grid processing pipeline associated with this task
    /// </summary>
    public ISubGridPipelineBase PipeLine { get; set; }

    public abstract void Dispose();
  }
}
