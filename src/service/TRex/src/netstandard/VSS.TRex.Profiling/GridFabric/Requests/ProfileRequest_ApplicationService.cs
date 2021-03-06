﻿using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Requests;
using VSS.TRex.Profiling.GridFabric.Arguments;
using VSS.TRex.Profiling.GridFabric.ComputeFuncs;
using VSS.TRex.Profiling.GridFabric.Responses;
using VSS.TRex.Profiling.Interfaces;

namespace VSS.TRex.Profiling.GridFabric.Requests
{
  /// <summary>
  /// Defines the contract for the profile request made to the applications service
  /// </summary>
  public abstract class ProfileRequest_ApplicationService<T> : GenericASNodeRequest<ProfileRequestArgument_ApplicationService, ProfileRequestComputeFunc_ApplicationService<T>, ProfileRequestResponse<T>> where T:class, IProfileCellBase, new()
  {
    public ProfileRequest_ApplicationService() : base(TRexGrids.ImmutableGridName(), ServerRoles.ASNODE_PROFILER)
    {
    }
  }
}
