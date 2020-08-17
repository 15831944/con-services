﻿using System;
using System.IO;
using FluentAssertions;
using VSS.TRex.Designs;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using Xunit;
using Xunit.Abstractions;

namespace VSS.TRex.DesignProfiling.Tests
{
  public class TTMDesignTests : IClassFixture<DILoggingFixture>
  {
    private readonly ITestOutputHelper output;
    public const string testFilePath = "TestData";
    public const string testFileName = "Bug36372.ttm";

    public TTMDesignTests(ITestOutputHelper output)
    {
      this.output = output;
    }

    private static TTMDesign design;

    private void LoadTheDesign(string filePath = testFilePath, string fileName = testFileName)
    {
      lock (this)
      {
        if (design == null)
        {
          design = new TTMDesign(SubGridTreeConsts.DefaultCellSize);
          design.LoadFromFile(Path.Combine(filePath, fileName));
        }
      }
    }

    [Fact(Skip = "not implemented")]
    public void ComputeFilterPatchTest()
    {
    }

    [Fact()]
    public void TTMDesignTest()
    {
      try
      {
        TTMDesign localDesign = new TTMDesign(SubGridTreeConsts.DefaultCellSize);

        Assert.NotNull(localDesign);
      }
      catch (Exception)
      {
        Assert.False(true);
      }
    }

    [Fact()]
    public void GetExtentsTest()
    {
      LoadTheDesign();

      design.GetExtents(out double x1, out double y1, out double x2, out double y2);

      Assert.NotEqual(x1, Common.Consts.NullReal);
      Assert.NotEqual(y1, Common.Consts.NullReal);
      Assert.NotEqual(x2, Common.Consts.NullReal);
      Assert.NotEqual(y2, Common.Consts.NullReal);
    }

    [Fact()]
    public void GetHeightRangeTest()
    {
      LoadTheDesign();

      design.GetHeightRange(out double z1, out double z2);

      Assert.NotEqual(z1, Common.Consts.NullReal);
      Assert.NotEqual(z2, Common.Consts.NullReal);
      Assert.True(z2 >= z1, "Z2 is below Z1");
    }

    [Fact(Skip = "not implemented")]
    public void HasElevationDataForSubGridPatchTest()
    {
    }

    [Fact(Skip = "not implemented")]
    public void HasElevationDataForSubGridPatchTest1()
    {
    }

    [Fact(Skip = "not implemented")]
    public void HasFiltrationDataForSubGridPatchTest()
    {
    }

    [Fact(Skip = "not implemented")]
    public void HasFiltrationDataForSubGridPatchTest1()
    {
    }

    [Theory]
    [InlineData(247500.0, 193350.0, 29.875899875665258)]
    public void InterpolateHeightTest(double probeX, double probeY, double expectedZ)
    {
      LoadTheDesign();

      int Hint = -1;

      bool result = design.InterpolateHeight(ref Hint, probeX, probeY, 0, out double Z);

      Assert.True(result, "Height interpolation returned false");

      Assert.True(Math.Abs(Z - expectedZ) < 0.001, $"Interpolated height value is incorrect, expected {expectedZ}");
    }

    [Theory]
    [InlineData(247500.0, 193350.0)]
    public void InterpolateHeightsTest(double probeX, double probeY)
    {
      LoadTheDesign();
      var Patch = new float[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension];

      bool result = design.InterpolateHeights(Patch, probeX, probeY, SubGridTreeConsts.DefaultCellSize, 0);

      Assert.True(result, "Heights interpolation returned false");
    }

    [Theory(Skip = "Performance test - should be moved into a benchmarking utility context")]
    [InlineData(247500.0, 193350.0)]
    public void InterpolateHeightsTestPerf(double probeX, double probeY)
    {
      LoadTheDesign();

      var Patch = new float[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension];

      DateTime _start = DateTime.Now;
      for (int i = 0; i < 10000; i++)
        design.InterpolateHeights(Patch, probeX, probeY, SubGridTreeConsts.DefaultCellSize, 0);
      DateTime _end = DateTime.Now;

      output.WriteLine($"Perf Test: Duration for 10000 patch requests: {_end - _start}");
      Assert.True(true);
    }

    [Fact]
    public void LoadFromFileTest()
    {
      LoadTheDesign();

      design.SubGridOverlayIndex().Should().NotBeNull();

      design.Data.Triangles.Items.Length.Should().Be(67251);
      design.Data.Vertices.Items.Length.Should().Be(34405);
      design.Data.StartPoints.Items.Length.Should().Be(16);
      design.Data.Edges.Items.Length.Should().Be(1525);

      design.SubGridOverlayIndex().SizeOf().Should().Be(12724);

      design.SizeInCache().Should().Be(2214624);
    }

    [Fact]
    public void SubgridOverlayIndexTest()
    {
      LoadTheDesign();

      design.SubGridOverlayIndex().Should().NotBeNull();

      design.Data.Triangles.Items.Length.Should().Be(67251);
      design.Data.Vertices.Items.Length.Should().Be(34405);
      design.Data.StartPoints.Items.Length.Should().Be(16);
      design.Data.Edges.Items.Length.Should().Be(1525);

      design.SubGridOverlayIndex().SizeOf().Should().Be(12724);

      design.SizeInCache().Should().Be(2214624);
    }

    private void LoadTheGiantDesign()
    {
      lock (this)
      {
        if (design == null)
        {
          design = new TTMDesign(SubGridTreeConsts.DefaultCellSize);
          design.LoadFromFile(@"C:\Users\rwilson\Downloads\5644616_oba9c0bd14_FRL.ttm");
        }
      }
    }

    [Fact(Skip = "Performance test - should be moved into a benchmarking utility context")]
    public void ScanAllElevationsOverGiantDesign()
    {
      DateTime _start = DateTime.Now;
      LoadTheGiantDesign();
      TimeSpan loadTime = DateTime.Now - _start;

      var Patch = new float[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension];

      int numPatches = 0;
      _start = DateTime.Now;

      design.SpatialIndexOptimised.ScanAllSubGrids(leaf =>
      {
        numPatches++;
        double cellSize = leaf.Owner.CellSize;
        leaf.CalculateWorldOrigin(out double originX, out double originY);

        leaf.ForEach((x, y) => { design.InterpolateHeights(Patch, originX + x * cellSize, originY + y * cellSize, cellSize / SubGridTreeConsts.SubGridTreeDimension, 0); });

        return true;
      });

      TimeSpan lookupTime = DateTime.Now - _start;

      output.WriteLine($"Perf Test: Duration for {numPatches} patch requests, load = {loadTime}, lookups = {lookupTime}");
      Assert.True(true);
    }
  }
}
