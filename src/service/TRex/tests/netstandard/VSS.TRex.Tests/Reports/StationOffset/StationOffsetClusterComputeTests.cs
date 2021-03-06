﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.Common.Models;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.Reports.StationOffset.Executors;
using VSS.TRex.Reports.StationOffset.GridFabric.Arguments;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SubGridTrees.Types;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Reports.StationOffset
{
  public class StationOffsetClusterComputeTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    [Fact]
    public async Task CalculateFromTAGFileDerivedModel()
    {
      var tagFiles = Directory.GetFiles(Path.Combine("TestData", "TAGFiles", "ElevationMappingMode-KettlewellDrive"), "*.tag").ToArray();
      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out var _);

      // For test purposes, create an imaginary 'road' that passes through at least 100 of the
      //  on-null cells in the site model, which also have passCount data
      var points = new List<StationOffsetPoint>();
      double station = 0;

      siteModel.ExistenceMap.ScanAllSetBitsAsSubGridAddresses(addr =>
      {
        if (points.Count > 100)
          return;

        var subGrid = TRex.SubGridTrees.Server.Utilities.SubGridUtilities.LocateSubGridContaining(
          siteModel.PrimaryStorageProxy, siteModel.Grid,
          addr.X, addr.Y, siteModel.Grid.NumLevels, false, false);

        subGrid.CalculateWorldOrigin(out var originX, out var originY);

        ((IServerLeafSubGrid) subGrid).Directory.GlobalLatestCells.PassDataExistenceMap.ForEachSetBit(
          (x, y) =>
          {
            points.Add(new StationOffsetPoint(station += 1, 0,
              originY + y * siteModel.CellSize + siteModel.CellSize / 2,
              originX + x * siteModel.CellSize + siteModel.CellSize / 2));
          });
      });

      var executor = new ComputeStationOffsetReportExecutor_ClusterCompute
      (new StationOffsetReportRequestArgument_ClusterCompute
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(new CombinedFilter()),
        Points = points,
        ReportElevation = true,
        ReportCmv = true,
        ReportMdp = true,
        ReportPassCount = true,
        ReportTemperature = true
      });

      var result = await executor.ExecuteAsync();

      result.ResultStatus.Should().Be(RequestErrorStatus.OK);
      result.ReturnCode.Should().Be(ReportReturnCode.NoError);
      result.StationOffsetRows.Count.Should().Be(points.Count);
      result.StationOffsetRows[0].Northing.Should().Be(808525.44000000006);
      result.StationOffsetRows[0].Easting.Should().Be(376730.88);
      result.StationOffsetRows[0].Elevation.Should().Be(68.630996704101562);// Mutable representation result ==> (68.6305160522461);
      result.StationOffsetRows[0].CutFill.Should().Be(Consts.NullHeight);
      result.StationOffsetRows[0].Cmv.Should().Be(CellPassConsts.NullCCV);
      result.StationOffsetRows[0].Mdp.Should().Be(CellPassConsts.NullMDP);
      result.StationOffsetRows[0].PassCount.Should().Be(1);
      result.StationOffsetRows[0].Temperature.Should().Be((short)CellPassConsts.NullMaterialTemperatureValue);

      result.StationOffsetRows.FirstOrDefault(x => x.CutFill != Consts.NullHeight).Should().Be(null);
      result.StationOffsetRows.FirstOrDefault(x => x.Cmv != CellPassConsts.NullCCV).Should().Be(null);
      result.StationOffsetRows.FirstOrDefault(x => x.Mdp != CellPassConsts.NullMDP).Should().Be(null);
      result.StationOffsetRows.FirstOrDefault(x => x.Temperature != (short)CellPassConsts.NullMaterialTemperatureValue).Should().Be(null);
    }

    [Fact]
    public async Task CalculateFromTAGFileDerivedModel_NoPoints()
    {
      var tagFiles = Directory.GetFiles(Path.Combine("TestData", "TAGFiles", "ElevationMappingMode-KettlewellDrive"), "*.tag").ToArray();
      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out var processedTasks);

      // Ask for a point that does not exist in the model the response should be a row with null values (???)
      var executor = new ComputeStationOffsetReportExecutor_ClusterCompute
      (new StationOffsetReportRequestArgument_ClusterCompute
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(new CombinedFilter()),
        Points = new List<StationOffsetPoint>(),
        ReportElevation = true
      });

      var result = await executor.ExecuteAsync();

      result.ResultStatus.Should().Be(RequestErrorStatus.OK);
      result.ReturnCode.Should().Be(ReportReturnCode.NoData);
      result.StationOffsetRows.Count.Should().Be(0);
    }

    [Fact]
    public async Task CalculateFromTAGFileDerivedModel_ShouldHaveNoPointValues()
    {
      var tagFiles = Directory.GetFiles(Path.Combine("TestData", "TAGFiles", "ElevationMappingMode-KettlewellDrive"), "*.tag").ToArray();
      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out var processedTasks);

      // Ask for a point that does not exist in the model the response should be a row with null values (???)
      var executor = new ComputeStationOffsetReportExecutor_ClusterCompute
      (new StationOffsetReportRequestArgument_ClusterCompute
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(new CombinedFilter()),
        Points = new List<StationOffsetPoint> { new StationOffsetPoint(0, 0, 0, 0) },
        ReportElevation = true
      });

      var result = await executor.ExecuteAsync();

      result.ResultStatus.Should().Be(RequestErrorStatus.OK);
      result.ReturnCode.Should().Be(ReportReturnCode.NoError);
      result.StationOffsetRows.Count.Should().Be(1);
      result.StationOffsetRows[0].Northing.Should().Be(0);
      result.StationOffsetRows[0].Easting.Should().Be(0);
      result.StationOffsetRows[0].Elevation.Should().Be(Consts.NullHeight);
    }

    [Fact]
    public async Task CalculateFromManuallyGeneratedSubGrid()
    {
      var siteModel = CreateSiteModelWithSingleCellForTesting();
      
      var executor = new ComputeStationOffsetReportExecutor_ClusterCompute
      (new StationOffsetReportRequestArgument_ClusterCompute
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(new CombinedFilter()),
        Points = new List<StationOffsetPoint> { new StationOffsetPoint(0, 0, 0, 0) },
        ReportElevation = true,
        ReportCmv = true,
        ReportMdp = true,
        ReportPassCount = true,
        ReportTemperature = true
      });

      var result = await executor.ExecuteAsync();

      result.ResultStatus.Should().Be(RequestErrorStatus.OK);
      result.ReturnCode.Should().Be(ReportReturnCode.NoError);
      result.StationOffsetRows.Count.Should().Be(1);
      result.StationOffsetRows[0].Northing.Should().Be(0);
      result.StationOffsetRows[0].Easting.Should().Be(0);
      result.StationOffsetRows[0].Elevation.Should().Be(MINIMUM_HEIGHT);
      result.StationOffsetRows[0].CutFill.Should().Be(Consts.NullHeight);
      result.StationOffsetRows[0].Cmv.Should().Be(CCV_Test);
      result.StationOffsetRows[0].Mdp.Should().Be(MDP_Test);
      result.StationOffsetRows[0].PassCount.Should().Be(3);
      result.StationOffsetRows[0].Temperature.Should().Be((short)Temperature_Test);
    }

    private readonly DateTime BASE_TIME = DateTime.UtcNow;
    private const int TIME_INCREMENT_SECONDS = 10; // seconds
    private const float BASE_HEIGHT = 100.0f;
    private const float HEIGHT_DECREMENT = -0.1f;
    private const float MINIMUM_HEIGHT = BASE_HEIGHT + HEIGHT_DECREMENT * (PASSES_IN_DECREMENTING_ELEVATION_LIST - 1);

    private const int PASSES_IN_DECREMENTING_ELEVATION_LIST = 3;
    private const short CCV_Test = 34;
    private const short MDP_Test = 56;
    private const ushort Temperature_Test = 134;

    /// <summary>
    /// Set up a model with a single sub grid with a single cell containing 3 cell passes
    /// </summary>
    /// <returns></returns>
    private ISiteModel CreateSiteModelWithSingleCellForTesting()
    {
      var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);

      // Switch to mutable storage representation to allow creation of content in the site model
      siteModel.StorageRepresentationToSupply.Should().Be(StorageMutability.Immutable);
      siteModel.SetStorageRepresentationToSupply(StorageMutability.Mutable);

      siteModel.Machines.CreateNew("Test Machine", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());

      // vibrationState is needed to get cmv values
      siteModel.MachinesTargetValues[0].VibrationStateEvents.PutValueAtDate(BASE_TIME,VibrationState.On);
      siteModel.MachinesTargetValues[0].AutoVibrationStateEvents.PutValueAtDate(BASE_TIME, AutoVibrationState.Manual);

      // Construct the sub grid to hold the cell being tested
      IServerLeafSubGrid leaf = siteModel.Grid.ConstructPathToCell(SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridPathConstructionType.CreateLeaf) as IServerLeafSubGrid;
      leaf.Should().NotBeNull();

      leaf.AllocateLeafFullPassStacks();
      leaf.CreateDefaultSegment();
      leaf.AllocateFullPassStacks(leaf.Directory.SegmentDirectory.First());
      leaf.AllocateLeafLatestPassGrid();

      // Add the leaf to the site model existence map
      siteModel.ExistenceMap[leaf.OriginX >> SubGridTreeConsts.SubGridIndexBitsPerLevel, leaf.OriginY >> SubGridTreeConsts.SubGridIndexBitsPerLevel] = true;
      siteModel.Grid.CountLeafSubGridsInMemory().Should().Be(1);

      // Add three passes, each separated by 10 seconds and descending by 100mm each pass
      for (int i = 0; i < PASSES_IN_DECREMENTING_ELEVATION_LIST; i++)
      {
        leaf.AddPass(0, 0, new CellPass
        {
          InternalSiteModelMachineIndex = 0,
          Time = BASE_TIME.AddSeconds(i * TIME_INCREMENT_SECONDS),
          Height = BASE_HEIGHT + i * HEIGHT_DECREMENT,
          PassType = PassType.Front,
          CCV = CCV_Test,
          MDP = MDP_Test,
          MaterialTemperature = Temperature_Test
        });
      }

      var cellPasses = leaf.Cells.PassesData[0].PassesData.ExtractCellPasses(0, 0);
      cellPasses.Passes.Count.Should().Be(PASSES_IN_DECREMENTING_ELEVATION_LIST);

      // Assign global latest cell pass to the appropriate pass
      leaf.Directory.GlobalLatestCells[0, 0] = cellPasses.Passes.Last();

      // Ensure the pass data existence map records the existence of a non null value in the cell
      leaf.Directory.GlobalLatestCells.PassDataExistenceMap[0, 0] = true;

      // Count the number of non-null elevation cells to verify a correct setup
      long totalCells = 0;
      siteModel.Grid.Root.ScanSubGrids(siteModel.Grid.FullCellExtent(), x => {
        totalCells += leaf.Directory.GlobalLatestCells.PassDataExistenceMap.CountBits();
        return true;
      });
      totalCells.Should().Be(1);

      return siteModel;
    }
  }
}
