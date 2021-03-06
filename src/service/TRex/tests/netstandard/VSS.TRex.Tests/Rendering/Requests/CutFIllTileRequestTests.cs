﻿using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Common;
using VSS.TRex.DataSmoothing;
using VSS.TRex.Designs.Models;
using VSS.TRex.Filters;
using VSS.TRex.Rendering.GridFabric.Requests;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Rendering.Requests
{
  [UnitTestCoveredRequest(RequestType = typeof(TileRenderRequest))]
  public class CutFIllTileRequestTests : TileRequestTestsBase, IClassFixture<DIRenderingFixture>
  {
    private readonly DIRenderingFixture _fixture;

    public CutFIllTileRequestTests(DIRenderingFixture fixture)
    {
      _fixture = fixture;
    }

    public enum ElevationSource
    {
      MinimumElevation,
      MaximumElevation
    }
    [Theory]
    [InlineData(ElevationSource.MinimumElevation, false, ConvolutionMaskSize.Mask3X3, NullInfillMode.NoInfill)]
    [InlineData(ElevationSource.MaximumElevation, false, ConvolutionMaskSize.Mask3X3, NullInfillMode.NoInfill)]
    [InlineData(ElevationSource.MinimumElevation, true, ConvolutionMaskSize.Mask3X3, NullInfillMode.NoInfill)]
    [InlineData(ElevationSource.MaximumElevation, true, ConvolutionMaskSize.Mask3X3, NullInfillMode.NoInfill)]
    [InlineData(ElevationSource.MinimumElevation, true, ConvolutionMaskSize.Mask3X3, NullInfillMode.InfillNullValues)]
    [InlineData(ElevationSource.MaximumElevation, true, ConvolutionMaskSize.Mask3X3, NullInfillMode.InfillNullValues)]
    [InlineData(ElevationSource.MinimumElevation, true, ConvolutionMaskSize.Mask3X3, NullInfillMode.InfillNullValuesOnly)]
    [InlineData(ElevationSource.MaximumElevation, true, ConvolutionMaskSize.Mask3X3, NullInfillMode.InfillNullValuesOnly)]
    [InlineData(ElevationSource.MinimumElevation, true, ConvolutionMaskSize.Mask5X5, NullInfillMode.NoInfill)]
    [InlineData(ElevationSource.MaximumElevation, true, ConvolutionMaskSize.Mask5X5, NullInfillMode.NoInfill)]
    [InlineData(ElevationSource.MinimumElevation, true, ConvolutionMaskSize.Mask5X5, NullInfillMode.InfillNullValues)]
    [InlineData(ElevationSource.MaximumElevation, true, ConvolutionMaskSize.Mask5X5, NullInfillMode.InfillNullValues)]
    [InlineData(ElevationSource.MinimumElevation, true, ConvolutionMaskSize.Mask5X5, NullInfillMode.InfillNullValuesOnly)]
    [InlineData(ElevationSource.MaximumElevation, true, ConvolutionMaskSize.Mask5X5, NullInfillMode.InfillNullValuesOnly)]
    public async Task Test_CutFillTile_TAGFile_FlatDesign(ElevationSource elevationSource, bool useSmoothing, ConvolutionMaskSize maskSize, NullInfillMode nullInfillMode)
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();
      AddDesignProfilerGridRouting();

      _fixture.smoothingActive = useSmoothing;
      _fixture.smootherMaskSize = maskSize;
      _fixture.smootherNullInfillMode = nullInfillMode;

      // Construct a site model from a single TAG file
      var tagFiles = new[] { Path.Combine(TestHelper.CommonTestDataPath, "TestTAGFile.tag") };
      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _);

      // Add a flat design to the site model at the minimum elevation in the site model

      var elevation = elevationSource switch
      {
        ElevationSource.MinimumElevation => (float) siteModel.SiteModelExtent.MinZ,
        ElevationSource.MaximumElevation => (float) siteModel.SiteModelExtent.MaxZ,
        _ => (float)siteModel.SiteModelExtent.MinZ
      };

      var designUid = DITAGFileAndSubGridRequestsWithIgniteFixture.ConstructFlatTTMDesignEncompassingSiteModel(ref siteModel, elevation);
      var referenceDesign = new DesignOffset(designUid, 0.0);
      var palette = PVMPaletteFactory.GetPalette(siteModel, DisplayMode.CutFill, siteModel.SiteModelExtent);
      var request = new TileRenderRequest();
      var arg = SimpleTileRequestArgument(siteModel, DisplayMode.CutFill, palette, volumeType: VolumeComputationType.BetweenFilterAndDesign);

      // Add the cut/fill design reference to the request, and set the rendering extents to the cell in question,
      // with an additional 1 meter border around the cell
      arg.ReferenceDesign = referenceDesign;
      arg.Extents = siteModel.SiteModelExtent;
      arg.Extents.Expand(1.0, 1.0);

      var response = await request.ExecuteAsync(arg);

      var fileName = @$"Flat-Elevation-{elevation}-Smoothing-{useSmoothing}-{maskSize}-{nullInfillMode}.bmp";
      var path = Path.Combine("TestData", "RenderedTiles", "CutFillTile", fileName);

      var saveFileName = ""; // @$"C:\Temp\Flat-Elevation-{elevation}-Smoothing-{useSmoothing}-{maskSize}-{nullInfillMode}.bmp";

      CheckSimpleRenderTileResponse(response, DisplayMode.CutFill, saveFileName, path);
    }

    [Fact]
    public async Task Test_CutFillTile_TAGFile_FilterToFilter()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();
      AddDesignProfilerGridRouting();

      // Construct a site model from a single TAG file
      var tagFiles = new[] { Path.Combine(TestHelper.CommonTestDataPath, "TestTAGFile.tag") };
      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _);
      var palette = PVMPaletteFactory.GetPalette(siteModel, DisplayMode.CutFill, siteModel.SiteModelExtent);
      var request = new TileRenderRequest();

      var arg = SimpleTileRequestArgument(siteModel, DisplayMode.CutFill, palette, volumeType:VolumeComputationType.Between2Filters);

      // Create a pair of filters to support first -> last cell pass comparison
      arg.Filters = new FilterSet(new CombinedFilter(), new CombinedFilter());
      arg.Filters.Filters[0].AttributeFilter.ReturnEarliestFilteredCellPass = true;

      // Add the cut/fill design reference to the request, and set the rendering extents to the cell in question,
      // with an additional 1 meter border around the cell
      arg.Extents = siteModel.SiteModelExtent;
      arg.Extents.Expand(1.0, 1.0);

      var response = await request.ExecuteAsync(arg);
      response.Should().NotBeNull();
      response.ResultStatus.Should().Be(RequestErrorStatus.OK);

      var fileName = "FilterToFilter-CutFill-TestTAGFile.bmp";
      var path = Path.Combine("TestData", "RenderedTiles", "CutFillTile", fileName);

      var saveFileName = ""; // @"C:\Temp\FilterToFilter-CutFill-TestTAGFile.bmp";

      CheckSimpleRenderTileResponse(response, DisplayMode.CutFill, saveFileName, path);
    }
  }
}
