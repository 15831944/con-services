﻿using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.ComputeFuncs;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.Exports.Surfaces;
using VSS.TRex.Exports.Surfaces.GridFabric;
using VSS.TRex.SubGrids.GridFabric.Arguments;
using VSS.TRex.SubGrids.GridFabric.ComputeFuncs;
using VSS.TRex.SubGrids.Interfaces;
using VSS.TRex.SubGrids.Responses;
using VSS.TRex.Tests.TestFixtures;

namespace VSS.TRex.Tests.Exports.Surfaces.GridFabric
{
  public class TINSurfaceRequestTestsBase
  {
    private void AddApplicationGridRouting() 
      => IgniteMock.Immutable.AddApplicationGridRouting<TINSurfaceRequestComputeFunc, TINSurfaceRequestArgument, TINSurfaceResult>();

    private void AddClusterComputeGridRouting()
    {
      IgniteMock.Immutable.AddClusterComputeGridRouting<SubGridsRequestComputeFuncProgressive<SubGridsRequestArgument, SubGridRequestsResponse>, SubGridsRequestArgument, SubGridRequestsResponse>();
      IgniteMock.Immutable.AddClusterComputeGridRouting<SubGridProgressiveResponseRequestComputeFunc, ISubGridProgressiveResponseRequestComputeFuncArgument, bool>();
    }

    private void AddDesignProfilerGridRouting() => IgniteMock.Immutable.AddApplicationGridRouting
      <CalculateDesignElevationPatchComputeFunc, CalculateDesignElevationPatchArgument, CalculateDesignElevationPatchResponse>();

    protected void AddGridRouting()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();
      AddDesignProfilerGridRouting();
    }
  }
}
