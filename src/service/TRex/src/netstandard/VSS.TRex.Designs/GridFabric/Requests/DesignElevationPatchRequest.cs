﻿using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.ComputeFuncs;
using VSS.TRex.Designs.GridFabric.Responses;

namespace VSS.TRex.Designs.GridFabric.Requests
{
  public class DesignElevationPatchRequest : GenericDesignProfilerRequest<CalculateDesignElevationPatchArgument, CalculateDesignElevationPatchComputeFunc, CalculateDesignElevationPatchResponse>
  {
  }
}
