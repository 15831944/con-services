﻿using System;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Common;
using VSS.TRex.Designs.Models;
using VSS.TRex.Filters;
using VSS.TRex.Geometry;
using VSS.TRex.Rendering.GridFabric.Arguments;
using VSS.TRex.Tests.BinarizableSerialization.Analytics;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Rendering
{
  public class ToFromBinary_TileRenderRequestArgument : IClassFixture<AnalyticsTestsDIFixture>
  {
    [Fact]
    public void Test_TileRenderRequestArgument_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<TileRenderRequestArgument>("Empty TileRenderRequestArgument not same after round trip serialisation");
    }

    [Fact]
    public void Test_TileRenderRequestArgument()
    {
      var argument = new TileRenderRequestArgument
      {
        ProjectID = Guid.NewGuid(),
        Filters = new FilterSet(new CombinedFilter(), new CombinedFilter()),
        ReferenceDesign = new DesignOffset(Guid.NewGuid(), 1.5),
        Mode = DisplayMode.Height,
        CoordsAreGrid = true,
        PixelsX = 100,
        PixelsY = 200,
        Extents = BoundingWorldExtent3D.Inverted(),
        VolumeType = VolumeComputationType.BetweenDesignAndFilter
      };

      SimpleBinarizableInstanceTester.TestClass(argument, "Custom TileRenderRequestArgument not same after round trip serialisation");
    }
  }
}
