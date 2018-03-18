﻿using VSS.VisionLink.Raptor.TAGFiles.Classes.Integrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;
using VSS.VisionLink.Raptor.SubGridTrees.Server;
using VSS.VisionLink.Raptor.Events;
using VSS.VisionLink.Raptor.SubGridTrees;
using VSS.VisionLink.Raptor.Executors;
using System.IO;
using VSS.VisionLink.Raptor.TAGFiles.Tests;
using VSS.VisionLink.Raptor.Interfaces;
using VSS.VisionLink.Raptor.Storage;
using VSS.VisionLink.Raptor.SiteModels;
using VSS.VisionLink.Raptor.Machines;
using Xunit;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.Integrator.Tests
{
        public class AggregatedDataIntegratorWorkerTests
    {
        [Fact()]
        public void Test_AggregatedDataIntegratorWorker_AggregatedDataIntegratorWorkerTest()
        {
            Assert.Fail();
        }

        [Fact()]
        public void Test_AggregatedDataIntegratorWorker_AggregatedDataIntegratorWorkerTest1()
        {
            Assert.Fail();
        }

        [Fact()]
        public void Test_AggregatedDataIntegratorWorker_ProcessTask()
        {
            // Convert a TAG file usign a TAGFileConverter into a mini-site model
            TAGFileConverter converter = new TAGFileConverter();

            Assert.True(converter.Execute(new FileStream(TAGTestConsts.TestDataFilePath() + "TAGFiles\\TestTAGFile.tag", FileMode.Open, FileAccess.Read)),
                "Converter execute returned false");

            // Create the site model and machine etc to aggregate the processed TAG file into
            SiteModel siteModel = new SiteModel("TestName", "TestDesc", 1, 1.0, null);
            Machine machine = new Machine(null, "TestName", "TestHardwareID", 0, 0, 0, false);
            //            ISubGridFactory factory = new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>();
            //            ServerSubGridTree tree = new ServerSubGridTree(siteModel);
            //            ProductionEventChanges events = new ProductionEventChanges(siteModel, machine.ID);

            // Create the integrator and add the processed TAG file to its processing list
            AggregatedDataIntegrator integrator = new AggregatedDataIntegrator();

            integrator.AddTaskToProcessList(siteModel, machine, converter.SiteModelGridAggregator, converter.ProcessedCellPassCount, converter.MachineTargetValueChangesAggregator);

            // Construct an integration worker and ask it to perform the integration
            List<AggregatedDataIntegratorTask> ProcessedTasks = new List<AggregatedDataIntegratorTask>();

            AggregatedDataIntegratorWorker worker = new AggregatedDataIntegratorWorker(integrator.TasksToProcess);
            worker.ProcessTask(ProcessedTasks);

            Assert.Equal(1, ProcessedTasks.Count);
        }
    }
}