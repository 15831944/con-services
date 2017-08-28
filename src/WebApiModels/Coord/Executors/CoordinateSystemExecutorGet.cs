﻿using ASNodeDecls;
using VLPDDecls;
using VSS.Productivity3D.Common.Models;

namespace VSS.Productivity3D.WebApiModels.Coord.Executors
{
  /// <summary>
  /// Get coordinate system definition file executor.
  /// </summary>
  /// 
  public class CoordinateSystemExecutorGet : CoordinateSystemExecutor
  {
    protected override TASNodeErrorStatus SendRequestToPDSClient(object item)
    {
      TCoordinateSystemSettings tempCoordSystemSettings;

      ProjectID request = item as ProjectID;
      TASNodeErrorStatus code = raptorClient.RequestCoordinateSystemDetails(request.projectId ?? -1, out tempCoordSystemSettings);

      if (code == TASNodeErrorStatus.asneOK)
      {
        coordSystemSettings = tempCoordSystemSettings;
      }

      return code;
    }
  }
}