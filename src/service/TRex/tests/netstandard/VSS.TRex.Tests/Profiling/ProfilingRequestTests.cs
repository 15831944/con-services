﻿using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.Common.Models;
using VSS.TRex.Common.Records;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.ComputeFuncs;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.Designs.Models;
using VSS.TRex.Filters;
using VSS.TRex.Profiling;
using VSS.TRex.Profiling.GridFabric.Arguments;
using VSS.TRex.Profiling.GridFabric.ComputeFuncs;
using VSS.TRex.Profiling.GridFabric.Requests;
using VSS.TRex.Profiling.GridFabric.Responses;
using VSS.TRex.Profiling.Models;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGrids.GridFabric.ComputeFuncs;
using VSS.TRex.SubGrids.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using VSS.TRex.Types.CellPasses;
using Xunit;

namespace VSS.TRex.Tests.Profiling
{
  [UnitTestCoveredRequest(RequestType = typeof(ProfileRequest_ApplicationService_ProfileCell))]
  [UnitTestCoveredRequest(RequestType = typeof(ProfileRequest_ApplicationService_SummaryVolumeProfileCell))]
  public class ProfilingRequestTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {

    private void AddDesignProfilerGridRouting()
    {
      //This is specific to cell datum i.e. what the cell datum cluster compute will call in the design profiler
      IgniteMock.Immutable.AddApplicationGridRouting<CalculateDesignElevationPatchComputeFunc, CalculateDesignElevationPatchArgument, CalculateDesignElevationPatchResponse>();
    }

    private void AddApplicationGridRouting()
    {
      IgniteMock.Immutable.AddApplicationGridRouting<ProfileRequestComputeFunc_ApplicationService<ProfileCell>, ProfileRequestArgument_ApplicationService, ProfileRequestResponse<ProfileCell>>();
      IgniteMock.Immutable.AddApplicationGridRouting<ProfileRequestComputeFunc_ApplicationService<SummaryVolumeProfileCell>, ProfileRequestArgument_ApplicationService, ProfileRequestResponse<SummaryVolumeProfileCell>>();
    }

    private void AddClusterComputeGridRouting()
    {
      IgniteMock.Immutable.AddClusterComputeGridRouting<ProfileRequestComputeFunc_ClusterCompute<ProfileCell>, ProfileRequestArgument_ClusterCompute, ProfileRequestResponse<ProfileCell>>();
      IgniteMock.Immutable.AddClusterComputeGridRouting<ProfileRequestComputeFunc_ClusterCompute<SummaryVolumeProfileCell>, ProfileRequestArgument_ClusterCompute, ProfileRequestResponse<SummaryVolumeProfileCell>>();
      IgniteMock.Immutable.AddClusterComputeGridRouting<SubGridProgressiveResponseRequestComputeFunc, ISubGridProgressiveResponseRequestComputeFuncArgument, bool>();
    }

    private void AddRoutings()
    {
      AddDesignProfilerGridRouting();
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();
    }

    [Fact]
    public void Creation_ProfileCell()
    {
      var req = new ProfileRequest_ApplicationService_ProfileCell();

      req.Should().NotBeNull();
    }

    [Fact]
    public void Creation_SummaryVolumeProfileCell()
    {
      var req = new ProfileRequest_ApplicationService_SummaryVolumeProfileCell();

      req.Should().NotBeNull();
    }

    private ISiteModel BuildModelForSingleCell()
    {
      var baseTime = DateTime.UtcNow;

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var bulldozerMachineIndex = siteModel.Machines.Locate("Bulldozer", false).InternalSiteModelMachineIndex;

      //This is required to get CCV
      siteModel.MachinesTargetValues[0].VibrationStateEvents.PutValueAtDate(Consts.MIN_DATETIME_AS_UTC, VibrationState.On);

      //Set machine targets
      siteModel.MachinesTargetValues[bulldozerMachineIndex].TargetCCVStateEvents.PutValueAtDate(Consts.MIN_DATETIME_AS_UTC, 123);
      siteModel.MachinesTargetValues[bulldozerMachineIndex].TargetMDPStateEvents.PutValueAtDate(Consts.MIN_DATETIME_AS_UTC, 321);
      siteModel.MachinesTargetValues[bulldozerMachineIndex].TargetPassCountStateEvents.PutValueAtDate(Consts.MIN_DATETIME_AS_UTC, 4);
      siteModel.MachinesTargetValues[bulldozerMachineIndex].TargetMinMaterialTemperature.PutValueAtDate(Consts.MIN_DATETIME_AS_UTC, 652);
      siteModel.MachinesTargetValues[bulldozerMachineIndex].TargetMaxMaterialTemperature.PutValueAtDate(Consts.MIN_DATETIME_AS_UTC, 655);

      siteModel.MachinesTargetValues[bulldozerMachineIndex].SaveMachineEventsToPersistentStore(siteModel.PrimaryStorageProxy);

      //Set up cell passes
      var cellPasses = Enumerable.Range(0, 10).Select(x =>
        new CellPass
        {
          InternalSiteModelMachineIndex = bulldozerMachineIndex,
          Time = baseTime.AddMinutes(x),
          Height = x,
          CCV = (short)(123 + x),
          MachineSpeed = (ushort)(456 + x),
          MDP = (short)(321 + x),
          MaterialTemperature = (ushort)(652 + x),
          PassType = PassType.Front
        }).ToArray();

      DITAGFileAndSubGridRequestsFixture.AddSingleCellWithPasses
        (siteModel, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, cellPasses, 1, cellPasses.Length);

      DITAGFileAndSubGridRequestsFixture.ConvertSiteModelToImmutable(siteModel);

      return siteModel;
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ProfileCell_SingleCell_NoDesign(bool withOverrides)
    {
      AddRoutings();

      var sm = BuildModelForSingleCell();

      var overrides = withOverrides
        ? new OverrideParameters
        {
          OverrideMachineCCV = true,
          OverridingMachineCCV = 987,
          OverrideMachineMDP = true,
          OverridingMachineMDP = 789,
          OverrideTargetPassCount = true,
          OverridingTargetPassCountRange = new PassCountRangeRecord(5, 6),
          OverrideTemperatureWarningLevels = true,
          OverridingTemperatureWarningLevels = new TemperatureWarningLevelsRecord(400, 1200),
          TargetMachineSpeed = new MachineSpeedExtendedRecord(777, 888)
        }
        : new OverrideParameters();

      var arg = new ProfileRequestArgument_ApplicationService
      {
        ProjectID = sm.ID,
        ProfileTypeRequired = GridDataType.Height,
        ProfileStyle = ProfileStyle.CellPasses,
        PositionsAreGrid = true,
        Filters = new FilterSet(new CombinedFilter()),
        ReferenceDesign = null,
        StartPoint = new WGS84Point(-1.0, sm.Grid.CellSize / 2),
        EndPoint = new WGS84Point(1.0, sm.Grid.CellSize / 2),
        ReturnAllPassesAndLayers = false,
        Overrides = overrides
      };

      var svRequest = new ProfileRequest_ApplicationService_ProfileCell();
      var response = await svRequest.ExecuteAsync(arg);

      response.Should().NotBeNull();
      response.ResultStatus.Should().Be(RequestErrorStatus.OK);
      response.GridDistanceBetweenProfilePoints.Should().Be(2.0);

      response.ProfileCells.Count.Should().Be(2);

      var expectedTargetCCV = (short)(withOverrides ? 987 : 123);
      var expectedPrevTargetCCV = (short)(withOverrides ? 987 : CellPassConsts.NullCCV);
      var expectedTargetMDP = (short)(withOverrides ? 789 : 321);
      var expectedMinTemp = (ushort)(withOverrides ? 400 : 652);
      var expectedMaxTemp = (ushort)(withOverrides ? 1200 : 655);
      var expectedMinPassCount = (ushort)(withOverrides ? 5 : 4);
      var expectedMaxPassCount = (ushort)(withOverrides ? 6 : 4);

      response.ProfileCells[0].CellFirstElev.Should().Be(0);
      response.ProfileCells[0].CellLastElev.Should().Be(9);
      response.ProfileCells[0].CellLowestElev.Should().Be(0);
      response.ProfileCells[0].CellHighestElev.Should().Be(9);
      response.ProfileCells[0].CellCCV.Should().Be(132);//123+9
      response.ProfileCells[0].CellCCVElev.Should().Be(9);
      response.ProfileCells[0].CellTargetCCV.Should().Be(expectedTargetCCV);
      response.ProfileCells[0].CellPreviousMeasuredCCV.Should().Be(131);
      response.ProfileCells[0].CellPreviousMeasuredTargetCCV.Should().Be(expectedPrevTargetCCV);
      response.ProfileCells[0].CellMDP.Should().Be(330);//321+9
      response.ProfileCells[0].CellMDPElev.Should().Be(9);
      response.ProfileCells[0].CellTargetMDP.Should().Be(expectedTargetMDP);
      response.ProfileCells[0].TopLayerPassCountTargetRangeMin.Should().Be(expectedMinPassCount);
      response.ProfileCells[0].TopLayerPassCountTargetRangeMax.Should().Be(expectedMaxPassCount);
      response.ProfileCells[0].TopLayerPassCount.Should().Be(10);
      response.ProfileCells[0].CellMinSpeed.Should().Be(456);
      response.ProfileCells[0].CellMaxSpeed.Should().Be(465);//456+9
      response.ProfileCells[0].CellMaterialTemperatureWarnMin.Should().Be(expectedMinTemp);
      response.ProfileCells[0].CellMaterialTemperatureWarnMax.Should().Be(expectedMaxTemp);
      response.ProfileCells[0].CellMaterialTemperature.Should().Be(661);//652+9
      response.ProfileCells[0].CellMaterialTemperatureElev.Should().Be(9);

      response.ProfileCells[1].CellFirstElev.Should().Be(CellPassConsts.NullHeight);
      response.ProfileCells[1].CellLastElev.Should().Be(CellPassConsts.NullHeight);
      response.ProfileCells[1].CellLowestElev.Should().Be(CellPassConsts.NullHeight);
      response.ProfileCells[1].CellHighestElev.Should().Be(CellPassConsts.NullHeight);
      response.ProfileCells[1].CellCCV.Should().Be(CellPassConsts.NullCCV);
      response.ProfileCells[1].CellCCVElev.Should().Be(CellPassConsts.NullHeight);
      response.ProfileCells[1].CellTargetCCV.Should().Be(CellPassConsts.NullCCV);
      response.ProfileCells[1].CellPreviousMeasuredCCV.Should().Be(CellPassConsts.NullCCV);
      response.ProfileCells[1].CellPreviousMeasuredTargetCCV.Should().Be(CellPassConsts.NullCCV);
      response.ProfileCells[1].CellMDP.Should().Be(CellPassConsts.NullMDP);
      response.ProfileCells[1].CellMDPElev.Should().Be(CellPassConsts.NullHeight);
      response.ProfileCells[1].CellTargetMDP.Should().Be(CellPassConsts.NullMDP);
      response.ProfileCells[1].TopLayerPassCountTargetRangeMin.Should().Be(CellPassConsts.NullPassCountValue);
      response.ProfileCells[1].TopLayerPassCountTargetRangeMax.Should().Be(CellPassConsts.NullPassCountValue);
      response.ProfileCells[1].TopLayerPassCount.Should().Be(CellPassConsts.NullPassCountValue);
      //Note: MinSpeed of Null and MaxSpeed of 0 are the defaults meaning no speed values
      response.ProfileCells[1].CellMinSpeed.Should().Be(CellPassConsts.NullMachineSpeed);
      response.ProfileCells[1].CellMaxSpeed.Should().Be(0);
      response.ProfileCells[1].CellMaterialTemperatureWarnMin.Should().Be(CellPassConsts.NullMaterialTemperatureValue);
      response.ProfileCells[1].CellMaterialTemperatureWarnMax.Should().Be(CellPassConsts.NullMaterialTemperatureValue);
      response.ProfileCells[1].CellMaterialTemperature.Should().Be(CellPassConsts.NullMaterialTemperatureValue);
      response.ProfileCells[1].CellMaterialTemperatureElev.Should().Be(CellPassConsts.NullHeight);
    }

    [Theory]
    [InlineData(VolumeComputationType.BetweenFilterAndDesign, 0.0f, 0.0f, Consts.NullHeight, 3)]
    [InlineData(VolumeComputationType.BetweenDesignAndFilter, 0.0f, Consts.NullHeight, 9.0f, 3)]
    [InlineData(VolumeComputationType.BetweenFilterAndDesign, 10.0f, 0.0f, Consts.NullHeight, 3)]
    [InlineData(VolumeComputationType.BetweenDesignAndFilter, 10.0f, Consts.NullHeight, 9.0f, 3)]
    public async Task SummaryVolumeProfileCell_SingleCell_FlatDesignAtOrigin_FilterToDesignOrDesignToFilter(VolumeComputationType volumeComputationType, float designElevation,
      float lastPassElevation1, float lastPassElevation2, int checkCellIndex)
    {
      AddRoutings();

      var sm = BuildModelForSingleCell();
      var design = DITAGFileAndSubGridRequestsWithIgniteFixture.ConstructSingleFlatTriangleDesignAboutOrigin(ref sm, designElevation);

      var arg = new ProfileRequestArgument_ApplicationService
      {
        ProjectID = sm.ID,
        ProfileTypeRequired = GridDataType.Height,
        ProfileStyle = ProfileStyle.SummaryVolume,
        PositionsAreGrid = true,
        Filters = new FilterSet(
          new CombinedFilter
          {
            AttributeFilter = new CellPassAttributeFilter { ReturnEarliestFilteredCellPass = true }
          },
          new CombinedFilter()),
        ReferenceDesign = new DesignOffset(design, 0),
        StartPoint = new WGS84Point(-1.0, sm.Grid.CellSize / 2),
        EndPoint = new WGS84Point(1.0, sm.Grid.CellSize / 2),
        ReturnAllPassesAndLayers = false,
        VolumeType = volumeComputationType
      };

      // Compute a profile from the bottom left of the screen extents to the top right 
      var svRequest = new ProfileRequest_ApplicationService_SummaryVolumeProfileCell();
      var response = await svRequest.ExecuteAsync(arg);

      response.Should().NotBeNull();
      response.ResultStatus.Should().Be(RequestErrorStatus.OK);
      response.ProfileCells.Count.Should().Be(6);
      response.ProfileCells[checkCellIndex].DesignElev.Should().Be(designElevation);
      response.ProfileCells[checkCellIndex].LastCellPassElevation1.Should().Be(lastPassElevation1);
      response.ProfileCells[checkCellIndex].LastCellPassElevation2.Should().Be(lastPassElevation2);
      response.ProfileCells[checkCellIndex].InterceptLength.Should().BeApproximately(sm.Grid.CellSize, 0.001);
      response.ProfileCells[checkCellIndex].OTGCellX.Should().Be(SubGridTreeConsts.DefaultIndexOriginOffset);
      response.ProfileCells[checkCellIndex].OTGCellY.Should().Be(SubGridTreeConsts.DefaultIndexOriginOffset);
    }
  }
}
