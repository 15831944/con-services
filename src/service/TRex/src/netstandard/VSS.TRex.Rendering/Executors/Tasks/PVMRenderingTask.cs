﻿using System;
using Microsoft.Extensions.Logging;
using System.Reflection;
using VSS.TRex.Pipelines.Tasks;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Rendering.Executors.Tasks
{
  /// <summary>
  /// A Task specialized towards rendering subgrid based information onto Plan View Map tiles
  /// </summary>
  public class PVMRenderingTask : PipelinedSubGridTask, IPVMRenderingTask
  {
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

        /// <summary>
        /// The tile renderer responsible for processing sub grid information into tile based thematic rendering
        /// </summary>
        public PlanViewTileRenderer TileRenderer { get; set; }

        public PVMRenderingTask()
        { }

        public override bool TransferResponse(object response)
        {
            // Log.InfoFormat("Received a SubGrid to be processed: {0}", (response as IClientLeafSubGrid).Moniker());

            if (!base.TransferResponse(response))
                return false;

            if (!(response is IClientLeafSubGrid[] subGridResponses) || subGridResponses.Length == 0)
              {
                Log.LogWarning("No subgrid responses returned");
                return false;
              }

            foreach (var subGrid in subGridResponses)
            {
              if (subGrid == null)
                continue;

              if (!TileRenderer.Displayer.RenderSubGrid(subGrid))
                return false;
            }

            return true;
        }
    }
}
