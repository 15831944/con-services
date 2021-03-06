﻿using System;
using FluentAssertions;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Events;
using VSS.TRex.Geometry;
using VSS.TRex.Machines;
using VSS.TRex.Machines.Interfaces;
using VSS.TRex.SiteModels;
using VSS.TRex.Storage.Models;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.TAGFiles.Classes.Processors;
using VSS.TRex.TAGFiles.Types;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using VSS.TRex.Types.CellPasses;
using Xunit;

namespace TAGFiles.Tests
{
        public class TAGProcessorTests : IClassFixture<DITagFileFixture>
    {
        [Fact]
        public void Test_TAGProcessor_Creation()
        {
            var SiteModel = new SiteModel(StorageMutability.Immutable);
            var Machine = new Machine();
            var SiteModelGridAggregator = new ServerSubGridTree(SiteModel.ID, StorageMutability.Mutable);
            var MachineTargetValueChangesAggregator = new ProductionEventLists(SiteModel, MachineConsts.kNullInternalSiteModelMachineIndex);

            TAGProcessor processor = new TAGProcessor(SiteModel, Machine, SiteModelGridAggregator, MachineTargetValueChangesAggregator);

            Assert.NotNull(processor);
        }

        [Fact]
        public void Test_TAGProcessor_TestZeroValuesInvalid()
        {
            var SiteModel = new SiteModel(StorageMutability.Immutable);
            var Machine = new Machine();
            var SiteModelGridAggregator = new ServerSubGridTree(SiteModel.ID, StorageMutability.Mutable);
            var MachineTargetValueChangesAggregator = new ProductionEventLists(SiteModel, MachineConsts.kNullInternalSiteModelMachineIndex);
            TAGProcessor processor = new TAGProcessor(SiteModel, Machine, SiteModelGridAggregator, MachineTargetValueChangesAggregator);
            Assert.True(processor.ICMDPValues.GetLatest() == CellPassConsts.NullMDP, "MDP Initial value incorrect");
            Assert.True(processor.ICCCVValues.GetLatest() == CellPassConsts.NullCCV, "CCV Initial value incorrect");
            Assert.True(processor.ICCCAValues.GetLatest() == CellPassConsts.NullCCA, "CCA Initial value incorrect");
            processor.SetICMDPValue(0);
            processor.SetICCCVValue(0);
            processor.SetICCCAValue(0);
            Assert.True(processor.ICMDPValues.GetLatest() == CellPassConsts.NullMDP, "Zero should not be a valid for MDP");
            Assert.True(processor.ICCCVValues.GetLatest() == CellPassConsts.NullCCV, "Zero should not be a valid for CCV");
            Assert.True(processor.ICCCAValues.GetLatest() == CellPassConsts.NullCCA, "Zero should not be a valid for CCA");
        }

        [Fact]
        public void Test_TAGProcessor_ProcessEpochContext_WithValidPosition()
        {
            var SiteModel = new SiteModel(StorageMutability.Immutable);
             SiteModel.IgnoreInvalidPositions = false;

            var Machine = new Machine();
            var SiteModelGridAggregator = new ServerSubGridTree(SiteModel.ID, StorageMutability.Mutable);
            var MachineTargetValueChangesAggregator = new ProductionEventLists(SiteModel, MachineConsts.kNullInternalSiteModelMachineIndex);

            TAGProcessor processor = new TAGProcessor(SiteModel, Machine, SiteModelGridAggregator, MachineTargetValueChangesAggregator);

            // Set the blade left and right tip locations to a trivial epoch, the epoch and do it again to trigger a swathing scan, then 
            // check to see if it generated anything!

            Fence interpolationFence = new Fence();
            interpolationFence.SetRectangleFence(0, 0, 1, 1);

            DateTime StartTime = DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 1), DateTimeKind.Utc);
            processor.DataLeft = new XYZ(0, 0, 5);
            processor.DataRight = new XYZ(1, 0, 5);
            processor.DataTime = StartTime;

            Assert.True(processor.ProcessEpochContext(), "ProcessEpochContext returned false in default TAGProcessor state (1)");

            DateTime EndTime = DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 3), DateTimeKind.Utc);
            processor.DataLeft = new XYZ(0, 1, 5);
            processor.DataRight = new XYZ(1, 1, 5);
            processor.DataTime = EndTime;

            Assert.True(processor.ProcessEpochContext(), "ProcessEpochContext returned false in default TAGProcessor state (2)");

            Assert.Equal(9, processor.ProcessedCellPassesCount);

            Assert.Equal(2, processor.ProcessedEpochCount);
        }

        [Fact]
        public void Test_TAGProcessor_ProcessEpochContext_WithoutValidTimestamp()
        {
            var SiteModel = new SiteModel(StorageMutability.Immutable);
            SiteModel.IgnoreInvalidPositions = false;
        
            var Machine = new Machine();
            var SiteModelGridAggregator = new ServerSubGridTree(SiteModel.ID, StorageMutability.Mutable);
            var MachineTargetValueChangesAggregator = new ProductionEventLists(SiteModel, MachineConsts.kNullInternalSiteModelMachineIndex);
        
            TAGProcessor processor = new TAGProcessor(SiteModel, Machine, SiteModelGridAggregator, MachineTargetValueChangesAggregator);
        
            // Set the blade left and right tip locations to a trivial epoch, the epoch and do it again to trigger a swathing scan, then 
            // check to see if it generated anything!
        
            Fence interpolationFence = new Fence();
            interpolationFence.SetRectangleFence(0, 0, 1, 1);
            processor.DataLeft = new XYZ(0, 0, 5);
            processor.DataRight = new XYZ(1, 0, 5);
        
            Assert.False(processor.ProcessEpochContext(), "ProcessEpochContext returned true without a valid epoch timestamp");
        }

        [Fact]
        public void Test_TAGProcessor_DoPostProcessFileAction()
        {
            var SiteModel = new SiteModel(StorageMutability.Immutable);
            var Machine = new Machine();
            var SiteModelGridAggregator = new ServerSubGridTree(SiteModel.ID, StorageMutability.Mutable);
            var MachineTargetValueChangesAggregator = new ProductionEventLists(SiteModel, MachineConsts.kNullInternalSiteModelMachineIndex);

            TAGProcessor processor = new TAGProcessor(SiteModel, Machine, SiteModelGridAggregator, MachineTargetValueChangesAggregator);

            // Set the state of the processor to emulate the end of processing this TAG file at which point the processor should emit
            // a "Stop recording event". In this instance, the NoGPSModeSet flag will also be true which should trigger emission of 
            // a 'NoGPS' GPS mode state event and a 'UTS' positioning technology state event

            DateTime eventDate = DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 1), DateTimeKind.Utc);

            // Setting the first data time will create the start event
            processor.DataTime = eventDate;

            DateTime eventDate2 = eventDate.AddMinutes(1);
            processor.DataTime = eventDate2;
            processor.DoPostProcessFileAction(true);

            Assert.True(MachineTargetValueChangesAggregator.GPSModeStateEvents.LastStateValue() == GPSMode.NoGPS &&
                          MachineTargetValueChangesAggregator.GPSModeStateEvents.LastStateDate() == eventDate,
                          "DoPostProcessFileAction did not set GPSMode event");

            Assert.True(MachineTargetValueChangesAggregator.PositioningTechStateEvents.LastStateValue() == PositioningTech.UTS &&
                          MachineTargetValueChangesAggregator.PositioningTechStateEvents.LastStateDate() == eventDate,
                          "DoPostProcessFileAction did not set positioning tech event");

            Assert.True(MachineTargetValueChangesAggregator.StartEndRecordedDataEvents.LastStateValue() == ProductionEventType.EndEvent /*EndRecordedData*/ &&
                          MachineTargetValueChangesAggregator.StartEndRecordedDataEvents.LastStateDate() == eventDate2,
                          "DoPostProcessFileAction did not set end recorded data event");
        }

        [Fact]
        public void Test_TAGProcessor_DoEpochPreProcessAction()
        {
            var SiteModel = new SiteModel(StorageMutability.Immutable);
            var Machine = new Machine();
            var SiteModelGridAggregator = new ServerSubGridTree(SiteModel.ID, StorageMutability.Mutable);
            var MachineTargetValueChangesAggregator = new ProductionEventLists(SiteModel, MachineConsts.kNullInternalSiteModelMachineIndex);

            TAGProcessor processor = new TAGProcessor(SiteModel, Machine, SiteModelGridAggregator, MachineTargetValueChangesAggregator);

            Assert.True(processor.DoEpochPreProcessAction(), "EpochPreProcessAction returned false in default TAGProcessor state");

            // Current PreProcessAction activity is limited to handling proofing runs. This will be handled by proofing run tests elsewhere
        }

        [Fact()]
        public void Test_TAGProcessor_DoEpochStateEvent()
        {
          var processor = new TAGProcessor();
      
          Action act = () => processor.DoEpochStateEvent(EpochStateEvent.Unknown);
          act.Should().Throw<TRexTAGFileProcessingException>().WithMessage("*Unknown epoch state event type*");
        }
    }
}
