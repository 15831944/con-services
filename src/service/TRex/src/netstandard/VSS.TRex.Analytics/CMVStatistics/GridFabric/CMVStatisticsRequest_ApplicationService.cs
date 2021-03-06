﻿using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.Analytics.CMVStatistics.GridFabric
{
  /// <summary>
  /// Sends a request to the grid for a CMV statistics request to be executed
  /// </summary>
  public class CMVStatisticsRequest_ApplicationService : GenericASNodeRequest<CMVStatisticsArgument, CMVStatisticsComputeFunc_ApplicationService, CMVStatisticsResponse>
  {
    public CMVStatisticsRequest_ApplicationService() : base(TRexGrids.ImmutableGridName(), ServerRoles.ASNODE)
    {
    }
  }
}
