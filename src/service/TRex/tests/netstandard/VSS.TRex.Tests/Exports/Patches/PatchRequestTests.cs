﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Cells;
using VSS.TRex.Designs.Models;
using VSS.TRex.Exports.Patches.GridFabric;
using VSS.TRex.Filters;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGrids.GridFabric.Arguments;
using VSS.TRex.SubGrids.GridFabric.ComputeFuncs;
using VSS.TRex.SubGrids.Interfaces;
using VSS.TRex.SubGrids.Responses;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Exports.Patches
{
  [UnitTestCoveredRequest(RequestType = typeof(PatchRequest))]
  public class PatchRequestTests: IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private const float HEIGHT_INCREMENT_0_5 = 0.5f;

    private void AddApplicationGridRouting() => IgniteMock.Immutable.AddApplicationGridRouting<PatchRequestComputeFunc, PatchRequestArgument, PatchRequestResponse>();

    private void AddClusterComputeGridRouting()
    {
      IgniteMock.Immutable.AddClusterComputeGridRouting<SubGridsRequestComputeFuncProgressive<SubGridsRequestArgument, SubGridRequestsResponse>, SubGridsRequestArgument, SubGridRequestsResponse>();
      IgniteMock.Immutable.AddClusterComputeGridRouting<SubGridProgressiveResponseRequestComputeFunc, ISubGridProgressiveResponseRequestComputeFuncArgument, bool>();
    }

    [Fact]
    public void Test_PatchRequest_Creation()
    {
      var request = new PatchRequest();

      request.Should().NotBeNull();
    }

    private ISiteModel BuildModelForSingleCellPatch(float heightIncrement)
    {
      var baseTime = DateTime.UtcNow;
      var baseHeight = 1.0f;

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var bulldozerMachineIndex = siteModel.Machines.Locate("Bulldozer", false).InternalSiteModelMachineIndex;

      var cellPasses = Enumerable.Range(0, 10).Select(x =>
        new CellPass
        {
          InternalSiteModelMachineIndex = bulldozerMachineIndex,
          Time = baseTime.AddMinutes(x),
          Height = baseHeight + x * heightIncrement,
          PassType = PassType.Front
        }).ToArray();

      DITAGFileAndSubGridRequestsFixture.AddSingleCellWithPasses
        (siteModel, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, cellPasses, 1, cellPasses.Length);
      DITAGFileAndSubGridRequestsFixture.ConvertSiteModelToImmutable(siteModel);

      return siteModel;
    }

    private PatchRequestArgument SimplePatchRequestArgument(Guid projectUid)
    {
      return new PatchRequestArgument
      {
        DataPatchNumber = 0,
        DataPatchSize = 100,
        Filters = new FilterSet(new CombinedFilter()),
        Mode = DisplayMode.Height,
        ProjectID = projectUid,
        ReferenceDesign = new DesignOffset(),
        TRexNodeID = Guid.NewGuid()
      };
    }

    [Fact]
    public async Task Test_PatchRequest_Execute_EmptySiteModel()
    {
      AddApplicationGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var request = new PatchRequest();

      var response = await request.ExecuteAsync(SimplePatchRequestArgument(siteModel.ID));

      response.Should().NotBeNull();
      response.SubGrids.Should().BeNull();
    }

    [Fact]
    public async Task Test_PatchRequest_Execute_SingleTAGFileSiteModel()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var tagFiles = new[]
      {
        Path.Combine(TestHelper.CommonTestDataPath, "TestTAGFile.tag"),
      };

      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _);
      var request = new PatchRequest();
      var response = await request.ExecuteAsync(SimplePatchRequestArgument(siteModel.ID));

      response.Should().NotBeNull();
      response.SubGrids.Should().NotBeNull();
      response.SubGrids.Count.Should().Be(12);

      response.SubGrids.ForEach(x => x.Should().BeOfType<ClientHeightAndTimeLeafSubGrid>());

      int nonNullCellCount = 0;
      response.SubGrids.ForEach(x => nonNullCellCount += ((ClientHeightAndTimeLeafSubGrid)x).CountNonNullCells());
      nonNullCellCount.Should().Be(3054);
    }
    
    [Fact]
    public async Task Test_PatchRequest_ExecuteAndConvert_SingleTAGFileSiteModel()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var tagFiles = new[]
      {
        Path.Combine(TestHelper.CommonTestDataPath, "TestTAGFile.tag"),
      };

      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _);
      var request = new PatchRequest();
      var result = await request.ExecuteAndConvertToResult(SimplePatchRequestArgument(siteModel.ID));

      result.Should().NotBeNull();
      result.Patch.Should().NotBeNull();
      result.Patch.Length.Should().Be(12);
      Math.Round(result.Patch[0].SubGridOriginX, 4).Should().Be(537667.84);
      Math.Round(result.Patch[0].SubGridOriginY, 4).Should().Be(5427390.08);
      Math.Round(result.Patch[0].ElevationOrigin, 4).Should().Be(41.397);
      result.Patch[0].TimeOrigin.Should().Be(1361929472);

      result.Patch[0].Data[13, 26].ElevationOffset.Should().Be(uint.MaxValue);
      result.Patch[0].Data[13, 26].TimeOffset.Should().Be(uint.MaxValue);
      result.Patch[0].Data[13, 27].ElevationOffset.Should().Be(59);
      result.Patch[0].Data[13, 27].TimeOffset.Should().Be(48000000);
      result.Patch[0].Data[13, 28].ElevationOffset.Should().Be(63);
      result.Patch[0].Data[13, 28].TimeOffset.Should().Be(50000000);

      Math.Round(result.Patch[1].SubGridOriginX, 4).Should().Be(537667.84);
      Math.Round(result.Patch[1].SubGridOriginY, 4).Should().Be(5427400.96);
      Math.Round(result.Patch[1].ElevationOrigin, 4).Should().Be(41.451);
      result.Patch[1].TimeOrigin.Should().Be(4237436768);

      Math.Round(result.Patch[2].SubGridOriginX, 4).Should().Be(537667.84);
      Math.Round(result.Patch[2].SubGridOriginY, 4).Should().Be(5427411.84);
      Math.Round(result.Patch[2].ElevationOrigin, 4).Should().Be(41.505);
      result.Patch[2].TimeOrigin.Should().Be(7369472);
    }

    [Fact]
    public async Task Test_PatchRequest_Execute_SingleCellSiteModel()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var siteModel = BuildModelForSingleCellPatch(HEIGHT_INCREMENT_0_5);

      var request = new PatchRequest();
      var response = await request.ExecuteAsync(SimplePatchRequestArgument(siteModel.ID));

      response.Should().NotBeNull();
      response.SubGrids.Should().NotBeNull();
      response.SubGrids.Count.Should().Be(1);
      response.SubGrids[0].CountNonNullCells().Should().Be(1);
      response.SubGrids[0].Should().BeOfType<ClientHeightAndTimeLeafSubGrid>();
      ((ClientHeightAndTimeLeafSubGrid)response.SubGrids[0]).Cells[0, 0].Should().BeApproximately(5.5f, 0.000001f);
    }

    [Fact]
    public async Task ExecuteAndConvertToResult()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var siteModel = BuildModelForSingleCellPatch(HEIGHT_INCREMENT_0_5);

      var request = new PatchRequest();
      var result = await request.ExecuteAndConvertToResult(SimplePatchRequestArgument(siteModel.ID));

      result.Should().NotBeNull();
      result.Patch.Should().NotBeNull();
      result.Patch.Length.Should().Be(1);

      result.Patch[0].ElevationOrigin.Should().Be(5.5f);
      result.Patch[0].Data[0, 0].ElevationOffset.Should().Be(0);
    }

    [Fact]
    public async Task PatchResult_ConstructResultData()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var siteModel = BuildModelForSingleCellPatch(HEIGHT_INCREMENT_0_5);

      var request = new PatchRequest();
      var result = await request.ExecuteAndConvertToResult(SimplePatchRequestArgument(siteModel.ID));

      var bytes = result.ConstructResultData();
      bytes.Should().NotBeNull();
    }
  }
}
