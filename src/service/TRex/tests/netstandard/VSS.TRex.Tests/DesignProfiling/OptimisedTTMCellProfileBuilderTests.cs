﻿using VSS.TRex.Designs.TTM.Optimised.Profiling;
using VSS.TRex.Geometry;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.DesignProfiling
{
  public class OptimisedTTMCellProfileBuilderTests : IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void Test_OptimisedTTMCellProfileBuilder_Creation()
    {
      var builder = new OptimisedTTMCellProfileBuilder(1.0, true);
    }

    /// <summary>
    /// Tests the testing helper tool does the right thing!
    /// </summary>
    [Fact]
    public void Test_OptimisedTTMDesignBuilder_OneTriangle()
    {
      var oneTriangleModel = OptimisedTTMDesignBuilder.CreateOptimisedTTM_WithFlatUnitTriangleAtOrigin(0.0);

      Assert.True(oneTriangleModel.Vertices.Items.Length == 3, "Invalid number of vertices for single triangle model");
      Assert.True(oneTriangleModel.Triangles.Items.Length == 1, "Invalid number of triangles for single triangle model");

      OptimisedTTMDesignBuilder.CreateOptimisedIndexForModel(oneTriangleModel, out var tree, out var indices);

      Assert.NotNull(tree);
      Assert.NotNull(indices);

      Assert.True(indices.Length == 2, $"Number of indices [{indices.Length}] incorrect, should be 2");
    }

    [Fact]
    public void Test_ProfilerBuilder_OneTriangle()
    {
      const int expectedInterceptCount = 8;

      // Create a model with a single triangle at (0, 0), (0, 1), (1, 0)
      var oneTriangleModel = OptimisedTTMDesignBuilder.CreateOptimisedTTM_WithFlatUnitTriangleAtOrigin(0.0);
      OptimisedTTMDesignBuilder.CreateOptimisedIndexForModel(oneTriangleModel, out var tree, out var indices);

      var builder = new OptimisedTTMCellProfileBuilder(tree.CellSize, true);

      // Build a profile line from (-100, -100) to (100, 100) to bisect the single triangle 
      var result = builder.Build(new [] {new XYZ(-100, -100), new XYZ(100, 100)}, 0);

      Assert.True(result, "Build() failed");

      Assert.True(builder.VtHzIntercepts.Count == expectedInterceptCount, $"Intercept count [{builder.VtHzIntercepts.Count}] wrong, expected {expectedInterceptCount}");
    }
  }
}
