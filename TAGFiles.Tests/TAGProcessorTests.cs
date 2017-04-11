﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor.TAGFiles.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Events;
using VSS.VisionLink.Raptor.SubGridTrees.Server;
using VSS.VisionLink.Raptor.Types;
using VSS.VisionLink.Raptor.Geometry;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.Tests
{
    [TestClass()]
    public class TAGProcessorTests
    {
        [TestMethod()]
        public void Test_TAGProcessor_Creation()
        {
            var SiteModel = new SiteModel();
            var Machine = new Machine();
            var Events = new ProductionEventChanges(SiteModel, Machine.ID);
            var SiteModelGridAggregator = new ServerSubGridTree(SiteModel);
            var MachineTargetValueChangesAggregator = new ProductionEventChanges(SiteModel, long.MaxValue);

            TAGProcessor processor = new TAGProcessor(SiteModel, Machine, Events, SiteModelGridAggregator, MachineTargetValueChangesAggregator);
        }

        [TestMethod()]
        public void Test_TAGProcessor_ProcessEpochContext()
        {
            var SiteModel = new SiteModel();
            var Machine = new Machine();
            var Events = new ProductionEventChanges(SiteModel, Machine.ID);
            var SiteModelGridAggregator = new ServerSubGridTree(SiteModel);
            var MachineTargetValueChangesAggregator = new ProductionEventChanges(SiteModel, long.MaxValue);

            TAGProcessor processor = new TAGProcessor(SiteModel, Machine, Events, SiteModelGridAggregator, MachineTargetValueChangesAggregator);

            // Set the blade left and right tip locations to a trivial epoch, the epoch and do it again to trigger a swathing scan, then 
            // check to see if it generated anything!

            Fence interpolationFence = new Fence();
            interpolationFence.SetRectangleFence(0, 0, 1, 1);

            DateTime StartTime = new DateTime(2000, 1, 1, 1, 1, 1);
            processor.DataLeft = new XYZ(0, 0, 5);
            processor.DataRight = new XYZ(1, 0, 5);
            processor.DataTime = StartTime;

            Assert.IsTrue(processor.ProcessEpochContext(), "ProcessEpochContext returned false in default TAGProcessor state (1)");

            DateTime EndTime = new DateTime(2000, 1, 1, 1, 1, 3);
            processor.DataLeft = new XYZ(0, 1, 5);
            processor.DataRight = new XYZ(1, 1, 5);

            Assert.IsTrue(processor.ProcessEpochContext(), "ProcessEpochContext returned false in default TAGProcessor state (2)");

            Assert.IsTrue(processor.ProcessedCellPassesCount == 9, "ProcessedCellPassesCount incorrect ({0}), should be 0",
                          processor.ProcessedCellPassesCount);

            Assert.IsTrue(processor.ProcessedEpochCount == 2, "ProcessedEpochCount incorrect ({0}), should be 0",
                          processor.ProcessedEpochCount);
        }

        [TestMethod()]
        public void Test_TAGProcessor_DoPostProcessFileAction()
        {
            var SiteModel = new SiteModel();
            var Machine = new Machine();
            var Events = new ProductionEventChanges(SiteModel, Machine.ID);
            var SiteModelGridAggregator = new ServerSubGridTree(SiteModel);
            var MachineTargetValueChangesAggregator = new ProductionEventChanges(SiteModel, long.MaxValue);

            TAGProcessor processor = new TAGProcessor(SiteModel, Machine, Events, SiteModelGridAggregator, MachineTargetValueChangesAggregator);

            // Set the state of the processor to emulate the end of processing this TAG file at which point the processor should emit
            // a "Stop recording event". In this instance, the NoGPSModeSet flag will also be true which should trigger emission of 
            // a 'NoGPS' GPS mode state event and a 'UTS' positioning technology state event

            DateTime eventDate = new DateTime(2000, 1, 1, 1, 1, 1);
            processor.DataTime = eventDate;
            processor.DoPostProcessFileAction(true);

            Assert.IsTrue(MachineTargetValueChangesAggregator.GPSModeStateEvents.Last().State == GPSMode.NoGPS &&
                          MachineTargetValueChangesAggregator.GPSModeStateEvents.Last().Date == eventDate,
                          "DoPostProcessFileAction did not set GPSMode event");

            Assert.IsTrue(MachineTargetValueChangesAggregator.PositioningTechStateEvents.Last().State == PositioningTech.UTS &&
                          MachineTargetValueChangesAggregator.PositioningTechStateEvents.Last().Date == eventDate,
                          "DoPostProcessFileAction did not set positioning tech event");

            Assert.IsTrue(MachineTargetValueChangesAggregator.StartEndRecordedDataEvents.Last().State == ProductionEventType.EndRecordedData &&
                          MachineTargetValueChangesAggregator.StartEndRecordedDataEvents.Last().Date == eventDate,
                          "DoPostProcessFileAction did not set end recorded data event");
        }

        [TestMethod()]
        public void Test_TAGProcessor_DoEpochPreProcessAction()
        {
            var SiteModel = new SiteModel();
            var Machine = new Machine();
            var Events = new ProductionEventChanges(SiteModel, Machine.ID);
            var SiteModelGridAggregator = new ServerSubGridTree(SiteModel);
            var MachineTargetValueChangesAggregator = new ProductionEventChanges(SiteModel, long.MaxValue);

            TAGProcessor processor = new TAGProcessor(SiteModel, Machine, Events, SiteModelGridAggregator, MachineTargetValueChangesAggregator);

            Assert.IsTrue(processor.DoEpochPreProcessAction(), "EpochPreProcessAction returned false in default TAGProcessor state");

            // Current PreProcessAction activity is limited to handling proofing runs. This will be handled by proofing run tests elsewhere
        }
    }
}