﻿using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.Analytics.PassCountStatistics.GridFabric
{
  /// <summary>
  /// Sends a request to the grid for a Pass Count statistics request to be executed
  /// </summary>
  public class PassCountStatisticsRequest_ApplicationService : GenericASNodeRequest<PassCountStatisticsArgument, PassCountStatisticsComputeFunc_ApplicationService, PassCountStatisticsResponse>
  {
    public PassCountStatisticsRequest_ApplicationService() : base(TRexGrids.ImmutableGridName(), ServerRoles.ASNODE)
    {
    }
  }
}
