﻿using System;
using VSS.VisionLink.Raptor.Pipelines;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.Executors.Tasks.Interfaces
{
    public interface ITask
    {
        void Cancel();
        bool TransferResponse(object response);

        GridDataType GridDataType { get; set; }

        string RaptorNodeID { get; set; }

        SubGridPipelineBase PipeLine { get; set; }
    }
}