﻿using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Executors.Tasks.Interfaces;
using VSS.VisionLink.Raptor.GridFabric.Requests;
using VSS.VisionLink.Raptor.Pipelines;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.Executors.Tasks
{
    /// <summary>
    /// A base class implementing activities that accept subgrids from a pipelined subgrid query process
    /// </summary>
    public class PipelinedSubGridTask : TaskBase, ITask 
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Primary task constructor
        /// </summary>
        /// <param name="requestDescriptor"></param>
        /// <param name="raptorNodeID"></param>
        /// <param name="gridDataType"></param>
        public PipelinedSubGridTask(long requestDescriptor, string raptorNodeID, GridDataType gridDataType) : base(requestDescriptor, raptorNodeID, gridDataType)
        {
        }

        /// <summary>
        /// Transfers a single subgrid response from a query context into the task processing context
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public override bool TransferResponse(object response)
        {
            if (PipeLine != null && !PipeLine.Aborted /*&& PipeLine.OperationNode != null*/)
            {
                // PipeLine.OperationNode.AddSubGridToOperateOn(response);
                return true;
            }
            else
            {
                Log.InfoFormat(" WARNING: PipelinedSubGridTask.TransferSubgridResponse: No pipeline available to submit grouped result for request {0}", RequestDescriptor);
                return false;
            }
        }

        public override void Cancel()
        {
            if (PipeLine == null)
            {
                return;
            }

            try
            {
                try
                {
                    Log.Debug("WARNING: Aborting pipeline due to cancellation");
                    PipeLine.Abort();
                }
                catch
                {
                    // Just in case the pipeline commits suicide before other related tasks are
                    // cancelled (and so also inform the pipeline that it is cancelled), swallow
                    // any exception generated for the abort request.
                }
            }
            finally
            {
                Log.Info("Nulling pipeline reference");
                PipeLine = null;
            }
        }

        /// <summary>
        /// Transfers a single subgrid response from a query context into the task processing context
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public override bool TransferResponses(object[] responses)
        {
            if (PipeLine != null && !PipeLine.Aborted /*&& PipeLine.OperationNode != null*/)
            {
                // PipeLine.OperationNode.AddSubGridToOperateOn(response);
                return true;
            }
            else
            {
                Log.InfoFormat(" WARNING: PipelinedSubGridTask.TransferSubgridResponse: No pipeline available to submit grouped result for request {0}", RequestDescriptor);
                return false;
            }
        }
    }
}
