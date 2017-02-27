﻿using System;
using System.IO;
using System.Reflection;
using TechTalk.SpecFlow;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using RestAPICoreTestFramework.Utils.Common;
using RaptorSvcAcceptTestsCommon.Models;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TagFileProcessing.AcceptanceTests.Models;
using System.Threading;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
    [Binding, Scope(Feature = "DotOnTheMap")]
    public class DotOnTheMapSteps
    {
        private Poster<TagFilePostParameter, TagFilePostResult> tagFilePoster;
        private Getter<GetMachinesResult> machineStatusGetter;
        private long machineId;
        private GetMachinesResult firstDotMachineStatus, secondDotMachineStatus;

        [Given(@"the Tag service URI ""(.*)"", Tag request repo file ""(.*)""")]
        public void GivenTheTagServiceURITagRequestRepoFile(string uri, string requestFile)
        {
            uri = RaptorClientConfig.TagSvcBaseUri + uri;
            tagFilePoster = new Poster<TagFilePostParameter, TagFilePostResult>(uri, requestFile, null);
        }

        [Given(@"the Machine service URI ""(.*)"", Machine result repo file ""(.*)""")]
        public void GivenTheMachineServiceURIMachineResultRepoFile(string uri, string resultFile)
        {
            Random rnd = new Random();
            machineId = rnd.Next(int.MaxValue);

            uri = RaptorClientConfig.ProdSvcBaseUri + uri + machineId;
            machineStatusGetter = new Getter<GetMachinesResult>(uri, resultFile);
        }

        [When(@"I post Tag file ""(.*)"" from the Tag request repo")]
        public void WhenIPostTagFileFromTheTagRequestRepo(string paramName)
        {
            //tagFilePoster.CurrentRequest = tagFilePoster.GetRequest(paramName);
            tagFilePoster.CurrentRequest = tagFilePoster.RequestRepo[paramName];
            tagFilePoster.CurrentRequest.machineId = machineId;

            tagFilePoster.DoValidRequest();
            Thread.Sleep(8000);
        }

        [When(@"I get and save the machine detail in one place")]
        public void WhenIGetAndSaveTheMachineDetailInOnePlace()
        {
            firstDotMachineStatus = machineStatusGetter.DoValidRequest();
        }

        [When(@"I get and save the machine detail in another place")]
        public void WhenIGetAndSaveTheMachineDetailInAnotherPlace()
        {
            secondDotMachineStatus = machineStatusGetter.DoValidRequest();
        }

        [Then(@"the first saved machine detail should match ""(.*)"" result from the Machine result repo")]
        public void ThenTheFirstSavedMachineDetailShouldMatchResultFromTheMachineResultRepo(string resultName)
        {
            if(firstDotMachineStatus.MachineStatuses.Length < 1)
                Assert.Fail(string.Format("Unable to get machine status {0}", firstDotMachineStatus));

            // Need to ignore assetID in validation
            GetMachinesResult expectedFirstDotMachineStatus = machineStatusGetter.ResponseRepo[resultName];
            expectedFirstDotMachineStatus.MachineStatuses[0].assetID = firstDotMachineStatus.MachineStatuses[0].assetID;

            Assert.AreEqual(expectedFirstDotMachineStatus, firstDotMachineStatus);
        }

        [Then(@"the second saved machine detail should match ""(.*)"" result from the Machine result repo")]
        public void ThenTheSecondSavedMachineDetailShouldMatchResultFromTheMachineResultRepo(string resultName)
        {
            if (secondDotMachineStatus.MachineStatuses.Length < 1)
                Assert.Fail(string.Format("Unable to get machine status {0}", secondDotMachineStatus));

            // Need to ignore assetID in validation
            GetMachinesResult expectedSecondDotMachineStatus = machineStatusGetter.ResponseRepo[resultName];
            expectedSecondDotMachineStatus.MachineStatuses[0].assetID = secondDotMachineStatus.MachineStatuses[0].assetID;

            Assert.AreEqual(machineStatusGetter.ResponseRepo[resultName], secondDotMachineStatus);
        }
    }
}
