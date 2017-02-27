﻿using System;
using System.IO;
using TechTalk.SpecFlow;
using System.Reflection;
using System.Net;
using Newtonsoft.Json;
using System.Collections.Generic;
using RestAPICoreTestFramework.Utils.Common;
using RaptorSvcAcceptTestsCommon.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TagFileProcessing.AcceptanceTests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TagFileProcessing.AcceptanceTests.StepDefinitions
{
    [Binding, Scope(Feature = "TagFile")]
    public class TagFileSteps
    {
        private Poster<TagFilePostParameter, TagFilePostResult> tagPoster;

        [Given(@"the Tag file service URI ""(.*)"" and request repo ""(.*)""")]
        public void GivenTheTagFileServiceURIAndRequestRepo(string uri, string requestFile)
        {
            uri = RaptorClientConfig.TagSvcBaseUri + uri;
            tagPoster = new Poster<TagFilePostParameter, TagFilePostResult>(uri, requestFile);
        }

        [When(@"I POST a tag file with code (.*) from the repository")]
        public void WhenIPOSTATagFileWithCodeFromTheRepository(int code)
        {
            tagPoster.DoValidRequest(code.ToString());
        }

        [When(@"I POST a tag file with Code (.*) from the repository expecting bad request return")]
        public void WhenIPOSTATagFileWithCodeFromTheRepositoryExpectingBadRequestReturn(int code)
        {
            tagPoster.DoInvalidRequest(code.ToString());
        }

        [When(@"I POST a Tag file with name ""(.*)"" from the repository expecting bad request return")]
        public void WhenIPOSTATagFileWithNameFromTheRepositoryExpectingBadRequestReturn(string paramName)
        {
            tagPoster.DoInvalidRequest(paramName);
        }

        [Then(@"the Tag Process Service response should contain Code (.*) and Message ""(.*)""")]
        public void ThenTheTagProcessServiceResponseShouldContainCodeAndMessage(int code, string message)
        {
            Assert.IsTrue(tagPoster.CurrentResponse.Code == code && tagPoster.CurrentResponse.Message == message,
                string.Format("Expected Code {0} and Message {1}, but received {2} and {3} instead.",
                code, message, tagPoster.CurrentResponse.Code, tagPoster.CurrentResponse.Message));
        }

        [Then(@"the Tag Process Service response should contain Error Code (.*)")]
        public void ThenTheTagProcessServiceResponseShouldContainErrorCode(int code)
        {
            Assert.AreEqual(code, tagPoster.CurrentResponse.Code,
                string.Format("Expected Code {0}, but received {1} instead.", code, tagPoster.CurrentResponse.Code));
        }
    }
}
