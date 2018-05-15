﻿using VSS.TRex.Executors.Tasks.Interfaces;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Executors.Tasks
{
    /// <summary>
    /// ITaskBase/TaskBase is an interface/base class implementation that other classes 
    /// may extend to provide specific handling logic for responses from queries, such as 
    /// subgrids and profile sections that require additional processing to arrive at 
    /// the final result (such as a rendered tile)
    /// </summary>
    public abstract class TaskBase : ITask
    {
        /// <summary>
        /// The request descriptor assigned to the task.
        /// </summary>
        public long RequestDescriptor = -1;

        /// <summary>
        /// Determines if the processing of the task activities as been cancelled by external control
        /// </summary>
        public bool IsCancelled;

        /// <summary>
        /// The type of grid data being processed by this task
        /// </summary>
        public GridDataType GridDataType { get; set; } = GridDataType.All;

        /// <summary>
        /// The raptor node wanting to recieve the results of task bases subgrid requests to the PSNode clustered processing layer
        /// </summary>
        public string RaptorNodeID { get; set; } = string.Empty;

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public TaskBase()
        {
        }

        /// <summary>
        /// Constructor accepting a request descriptor identifying the overall request this task is associated with
        /// </summary>
        /// <param name="requestDescriptor"></param>
        /// <param name="raptorNodeID"></param>
        /// <param name="gridDataType"></param>
        public TaskBase(long requestDescriptor, string raptorNodeID, GridDataType gridDataType)
        {
            RequestDescriptor = requestDescriptor;
            RaptorNodeID = raptorNodeID;
            GridDataType = gridDataType;
        }

        /// <summary>
        /// TransferReponse is the sink for responses received from the processing layers.
        /// </summary>
        /// <param name="response"></param>
        public abstract bool TransferResponse(object response);

        /// <summary>
        /// TransferReponses is the sink for sets of responses received from the processing layers.
        /// </summary>
        /// <param name="responses"></param>
        public abstract bool TransferResponses(object [] responses);

        /// <summary>
        /// Cancel sets the cancelled flag to true for the processing engine to take note of and 
        /// take any required actions to cancel an active request.
        /// </summary>
        public virtual void Cancel() => IsCancelled = true;

        /// <summary>
        /// A reference to a subgrid processing pipeline associated with this task
        /// </summary>
        public ISubGridPipelineBase PipeLine { get; set; }
    }
}
