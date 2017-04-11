﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor.TAGFiles.Classes.Swather;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Events;
using VSS.VisionLink.Raptor.SubGridTrees;
using VSS.VisionLink.Raptor.Geometry;
using VSS.VisionLink.Raptor.SubGridTrees.Server;
using VSS.VisionLink.Raptor.Interfaces;
using VSS.VisionLink.Raptor.SubGridTrees.Types;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;
using VSS.VisionLink.Raptor.Machines;
using VSS.VisionLink.Raptor.SiteModels;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.Swather.Tests
{
    [TestClass()]
    public class TerrainSwatherTests
    {
        [TestMethod()]
        public void Test_TerrainSwather_Creation()
        {
            var siteModel = new SiteModel();
            var machine = new Machine();
            var events = new ProductionEventChanges(siteModel, machine.ID);
            var grid = new ServerSubGridTree(siteModel);
            var fence = new Fence();
            var SiteModelGridAggregator = new ServerSubGridTree(siteModel);
            var MachineTargetValueChangesAggregator = new ProductionEventChanges(siteModel, long.MaxValue);
            var processor = new TAGProcessor(siteModel, machine, events, SiteModelGridAggregator, MachineTargetValueChangesAggregator);

            TerrainSwather swather = new TerrainSwather(processor, events, siteModel, grid, machine.ID, fence);

            Assert.IsTrue(swather != null & swather.MachineID == machine.ID,
                "TerrainSwather not created as expected");
        }

        [TestMethod()]
        public void Test_TerrainSwather_PerformSwathing()
        {
            var siteModel = new SiteModel();
            var machine = new Machine();
            var events = new ProductionEventChanges(siteModel, machine.ID);
            var grid = new ServerSubGridTree(siteModel);
            var SiteModelGridAggregator = new ServerSubGridTree(siteModel);
            var MachineTargetValueChangesAggregator = new ProductionEventChanges(siteModel, long.MaxValue);
            var processor = new TAGProcessor(siteModel, machine, events, SiteModelGridAggregator, MachineTargetValueChangesAggregator);

            var fence = new Fence();
            fence.SetRectangleFence(0, 0, 10, 2);

            TerrainSwather swather = new TerrainSwather(processor, events, siteModel, grid, machine.ID, fence);

            // Create four corner vertices for location of the processing context
            var V00 = new XYZ(0, 0, 0);
            var V01 = new XYZ(0, 2, 0);
            var V10 = new XYZ(10, 0, 0);
            var V11 = new XYZ(10, 2, 0);

            // Create four corner vertices for time of the processing context (with two epochs three seconds apart
            var T00 = new XYZ(0, 0, new DateTime(2000, 1, 1, 1, 1, 0).ToOADate());
            var T01 = new XYZ(0, 2, new DateTime(2000, 1, 1, 1, 1, 0).ToOADate());
            var T10 = new XYZ(10, 0, new DateTime(2000, 1, 1, 1, 1, 3).ToOADate());
            var T11 = new XYZ(10, 2, new DateTime(2000, 1, 1, 1, 1, 3).ToOADate());

            // Create the height and time interpolation triangles
            var HeightInterpolator1 = new SimpleTriangle(V00, V01, V10);
            var HeightInterpolator2 = new SimpleTriangle(V01, V11, V10);
            var TimeInterpolator1 = new SimpleTriangle(T00, T01, T10);
            var TimeInterpolator2 = new SimpleTriangle(T01, T11, T10);

            // Compute swath with full cell pass on the front (blade) mesurement location
            bool swathResult = swather.PerformSwathing(HeightInterpolator1, HeightInterpolator2, TimeInterpolator1, TimeInterpolator2, false, Raptor.Types.PassType.Front);

            // Did the swathing operation succeed?
            Assert.IsTrue(swathResult, "Performaswathing failed");

            // Did it produce the expected set of swathed cells?
            Assert.IsTrue(grid.Root.CountChildren() == 1, "Root node does not have a single child");

            // Focr computation of the latest pass information which aids locating cells with non-null values
            try
            {
                grid.Root.ScanSubGrids(grid.FullCellExtent(), x =>
                {
                    ((ServerSubGridTreeLeaf)x).ComputeLatestPassInformation(true);
                    return true;
                });
            }
            catch (Exception E)
            {
                Assert.Fail("Exception {0} occured computing latest cell information", E);
            }

            uint cellX, cellY;
            grid.CalculateIndexOfCellContainingPosition(grid.CellSize / 2, grid.CellSize / 2, out cellX, out cellY);

            IServerLeafSubGrid subgrid = (IServerLeafSubGrid)grid.LocateSubGridContaining(cellX, cellY);

            int nonNullCellCount = 0;
            try
            {
                grid.Root.ScanSubGrids(grid.FullCellExtent(), x =>
                {
                    nonNullCellCount += ((IServerLeafSubGrid)x).CountNonNullCells();
                    return true;
                });
            }
            catch (Exception E)
            {
                Assert.Fail("Exception {0} occured counting non-null cells", E);
            }

            Assert.IsTrue(nonNullCellCount == 174, "nonNullCellCount is incorrect = {0}, expected 174", nonNullCellCount);

            // Iterate over the cells and confirm their content is as expected

            
        }
    }
}
