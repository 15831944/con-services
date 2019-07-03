﻿using VSS.TRex.CellDatum.GridFabric.Arguments;
using VSS.TRex.CellDatum.GridFabric.ComputeFuncs;
using VSS.TRex.CellDatum.GridFabric.Responses;
using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.CellDatum.GridFabric.Requests
{
  /// <summary>
  /// Sends a request to the grid to identify the cell, display information and other configuration information to determine a datum value for the cell
  /// </summary>
  public class CellPassesRequest_ClusterCompute : GenericPSNodeSpatialAffinityRequest<CellPassesRequestArgument_ClusterCompute, CellPassesRequestComputeFunc_ClusterCompute, CellPassesResponse>
  {
  }
}
