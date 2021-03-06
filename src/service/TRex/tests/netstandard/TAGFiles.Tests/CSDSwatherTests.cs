﻿using System;
using FluentAssertions;
using VSS.TRex.Events;
using VSS.TRex.Geometry;
using VSS.TRex.Machines;
using VSS.TRex.Machines.Interfaces;
using VSS.TRex.SiteModels;
using VSS.TRex.Storage;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.TAGFiles.Classes.Processors;
using VSS.TRex.TAGFiles.Classes.Swather;
using VSS.TRex.TAGFiles.Types;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;


namespace TAGFiles.Tests
{
  public class CSDSwatherTests : IClassFixture<DITagFileFixture>
  {

    [Fact]
    public void Test_CSDSwather_Creation()
    {
      var siteModel = new SiteModel(StorageMutability.Immutable);
      var machine = new Machine();
      machine.MachineType = MachineType.CutterSuctionDredge;
      var grid = new ServerSubGridTree(siteModel.ID, StorageMutability.Mutable);
      var fence = new Fence();
      var SiteModelGridAggregator = new ServerSubGridTree(siteModel.ID, StorageMutability.Mutable);
      var MachineTargetValueChangesAggregator = new ProductionEventLists(siteModel, MachineConsts.kNullInternalSiteModelMachineIndex);
      var processor = new TAGProcessor(siteModel, machine, SiteModelGridAggregator, MachineTargetValueChangesAggregator);
      var swather = new CSDSwather(processor, MachineTargetValueChangesAggregator, siteModel, grid, fence);

      Assert.True(swather != null, "CSDSwather not created as expected");
    }

    private void CreateSwathContext(double V00x, double V00y, double V00z,
      double V01x, double V01y, double V01z,
      double V10x, double V10y, double V10z,
      double V11x, double V11y, double V11z,
      out SimpleTriangle HeightInterpolator1,
      out SimpleTriangle HeightInterpolator2,
      out SimpleTriangle TimeInterpolator1,
      out SimpleTriangle TimeInterpolator2)
    {
      // Create four corner vertices for location of the processing context
      var V00 = new XYZ(V00x, V00y, V00z);
      var V01 = new XYZ(V01x, V01y, V01z);
      var V10 = new XYZ(V10x, V10y, V10z);
      var V11 = new XYZ(V11x, V11y, V11z);

      // Create four corner vertices for time of the processing context (with two epochs three seconds apart
      var T00 = new XYZ(V00x, V00y, new DateTime(2000, 1, 1, 1, 1, 0, 0, DateTimeKind.Utc).ToOADate());
      var T01 = new XYZ(V01x, V01y, new DateTime(2000, 1, 1, 1, 1, 0, 0, DateTimeKind.Utc).ToOADate());
      var T10 = new XYZ(V10x, V10y, new DateTime(2000, 1, 1, 1, 1, 3, 0, DateTimeKind.Utc).ToOADate());
      var T11 = new XYZ(V11x, V11y, new DateTime(2000, 1, 1, 1, 1, 3, 0, DateTimeKind.Utc).ToOADate());

      // Create the height and time interpolation triangles
      HeightInterpolator1 = new SimpleTriangle(V00, V01, V10);
      HeightInterpolator2 = new SimpleTriangle(V01, V11, V10);
      TimeInterpolator1 = new SimpleTriangle(T00, T01, T10);
      TimeInterpolator2 = new SimpleTriangle(T01, T11, T10);
    }

    [Fact]
    public void Test_CSDSwather_PerformSwathing()
    {
      var siteModel = new SiteModel(StorageMutability.Immutable);
      var machine = new VSS.TRex.Machines.Machine();
      machine.MachineType = MachineType.CutterSuctionDredge;
      var grid = new ServerSubGridTree(siteModel.ID, StorageMutability.Mutable);
      var SiteModelGridAggregator = new ServerSubGridTree(siteModel.ID, StorageMutability.Mutable);
      var MachineTargetValueChangesAggregator = new ProductionEventLists(siteModel, MachineConsts.kNullInternalSiteModelMachineIndex);
      var processor = new TAGProcessor(siteModel, machine, SiteModelGridAggregator, MachineTargetValueChangesAggregator);
      var fence = new Fence();
      fence.SetRectangleFence(0, 0, 10, 2);

      CSDSwather swather = new CSDSwather(processor, MachineTargetValueChangesAggregator, siteModel, grid, fence);
      // blades rotated
      CreateSwathContext(2758.1, 0, 0,
                         2762.1, 0, 0,
                         2757.1, 0, 0,
                         2761.1, 0, 0,
                         out SimpleTriangle HeightInterpolator1,
                         out SimpleTriangle HeightInterpolator2,
                         out SimpleTriangle TimeInterpolator1,
                         out SimpleTriangle TimeInterpolator2);

      // Compute swath with full cell pass on the front (blade) measurement location
      bool swathResult = swather.PerformSwathing(HeightInterpolator1, HeightInterpolator2, TimeInterpolator1, TimeInterpolator2, false, PassType.Front, MachineSide.None);

      // Did the swathing operation succeed?
      Assert.True(swathResult, "Perform swathing failed");

      // Computation of the latest pass information which aids locating cells with non-null values
      try
      {
        IStorageProxy storageProxy = StorageProxy.Instance(StorageMutability.Mutable);
        grid.Root.ScanSubGrids(grid.FullCellExtent(), x =>
        {
          ((IServerLeafSubGrid)x).ComputeLatestPassInformation(true, storageProxy);
          return true;
        });
      }
      catch (Exception E)
      {
        Assert.False(true, $"Exception {E} occured computing latest cell information");
      }

      grid.CalculateIndexOfCellContainingPosition(grid.CellSize / 2, grid.CellSize / 2, out int _, out int _);

      int nonNullCellCount = 0;
      try
      {
        grid.Root.ScanSubGrids(grid.FullCellExtent(), x =>
        {
          nonNullCellCount += ((IServerLeafSubGrid)x).CountNonNullCells();
          return true;
        });
      }
      catch (Exception e)
      {
        Assert.False(true, $"Exception {e} occured counting non-null cells");
      }

      Assert.Equal(148, nonNullCellCount);

    }

    [Fact]
    public void Test_CSDSwather_SwathExtentTooLarge()
    {
      var siteModel = new SiteModel(StorageMutability.Immutable);
      var machine = new Machine();
      var grid = new ServerSubGridTree(siteModel.ID, StorageMutability.Mutable);
      var SiteModelGridAggregator = new ServerSubGridTree(siteModel.ID, StorageMutability.Mutable);
      var MachineTargetValueChangesAggregator = new ProductionEventLists(siteModel, MachineConsts.kNullInternalSiteModelMachineIndex);
      var processor = new TAGProcessor(siteModel, machine, SiteModelGridAggregator, MachineTargetValueChangesAggregator);

      var fence = new Fence(new BoundingWorldExtent3D(0, 0, 10000, 2));
      var swather = new CSDSwather(processor, MachineTargetValueChangesAggregator, siteModel, grid, fence);

      CreateSwathContext(0, 0, 0,
        10000, 0, 0,
        0, 2, 0,
        10000, 2, 0,
        out SimpleTriangle HeightInterpolator1,
        out SimpleTriangle HeightInterpolator2,
        out SimpleTriangle TimeInterpolator1,
        out SimpleTriangle TimeInterpolator2);

      bool swathResult = swather.PerformSwathing(HeightInterpolator1, HeightInterpolator2, TimeInterpolator1, TimeInterpolator2, false, PassType.Front, MachineSide.None);
      swathResult.Should().BeTrue();
      processor.ProcessedEpochCount.Should().Be(0);
      processor.ProcessedCellPassesCount.Should().Be(0);
    }

  }
}
