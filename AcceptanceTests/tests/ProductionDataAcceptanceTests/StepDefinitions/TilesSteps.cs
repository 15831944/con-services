﻿using System;
using System.IO;
using System.Reflection;
using TechTalk.SpecFlow;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using RestAPICoreTestFramework.Utils.Common;
using RaptorSvcAcceptTestsCommon.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using ProductionDataSvc.AcceptanceTests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XnaFan.ImageComparison;
using System.Drawing;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
    [Binding, Scope(Feature = "Tiles")]
    public class TilesSteps
    {
        private Poster<TileRequest, TileResult> tileRequester;
        private WebHeaderCollection header;
        private byte[] pngTile;

        [Given(@"the Tile service URI ""(.*)"", request repo ""(.*)"" and result repo ""(.*)""")]
        public void GivenTheTileServiceURIRequestRepoAndResultRepo(string uri, string requestFile, string resultFile)
        {
            uri = RaptorClientConfig.ProdSvcBaseUri + uri;
            tileRequester = new Poster<TileRequest, TileResult>(uri, requestFile, resultFile);
        }

        [When(@"I request Tiles supplying ""(.*)"" paramters from the repository")]
        public void WhenIRequestTilesSupplyingParamtersFromTheRepository(string paramName)
        {
            tileRequester.DoValidRequest(paramName);
        }

        [When(@"I request Tiles supplying ""(.*)"" paramters from the repository expecting BadRequest")]
        public void WhenIRequestTilesSupplyingParamtersFromTheRepositoryExpectingBadRequest(string paramName)
        {
            tileRequester.DoInvalidRequest(paramName);
        }

        [Then(@"the Tiles response should match ""(.*)"" result from the repository")]
        public void ThenTheTilesResponseShouldMatchResultFromTheRepository(string resultName)
        {
            Assert.AreEqual(tileRequester.ResponseRepo[resultName], tileRequester.CurrentResponse);
        }

        [Then(@"the response should contain error code (.*)")]
        public void ThenTheResponseShouldContainErrorCode(int expectedCode)
        {
            Assert.AreEqual(expectedCode, tileRequester.CurrentResponse.Code);
        }

        [Given(@"the PNG Tile service URI ""(.*)""")]
        public void GivenThePNGTileServiceURI(string pngTileUri)
        {
            tileRequester.Uri = RaptorClientConfig.ProdSvcBaseUri + pngTileUri;
        }

        [When(@"I request PNG Tiles supplying ""(.*)"" paramters from the repository")]
        public void WhenIRequestPNGTilesSupplyingParamtersFromTheRepository(string paramName)
        {
            string requestBodyString = JsonConvert.SerializeObject(tileRequester.RequestRepo[paramName]);

            HttpWebResponse httpResponse = RaptorServicesClientUtil.DoHttpRequest(tileRequester.Uri,
                 "POST", "application/json", "image/png", requestBodyString);

            if(httpResponse != null)
            {
                Assert.AreEqual(HttpStatusCode.OK, httpResponse.StatusCode,
                    String.Format("Expected {0}, but got {1} instead.", HttpStatusCode.OK, httpResponse.StatusCode));

                header = httpResponse.Headers;

                byte[] buffer = new byte[1024];
                using (Stream responseStream = httpResponse.GetResponseStream())
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        int count = 0;
                        do
                        {
                            count = responseStream.Read(buffer, 0, buffer.Length);
                            memoryStream.Write(buffer, 0, count);

                        } while (count != 0);

                        pngTile = memoryStream.ToArray();
                    }
                }

                httpResponse.Close();
            }
        }

        [Then(@"the PNG Tiles response should match ""(.*)"" result from the repository")]
        public void ThenThePNGTilesResponseShouldMatchResultFromTheRepository(string resultName)
        {
            TileResult result = new TileResult() { 
                TileData = pngTile,
                TileOutsideProjectExtents = tileRequester.ResponseRepo[resultName].TileOutsideProjectExtents
            };
            Assert.AreEqual(tileRequester.ResponseRepo[resultName], result);
        }

        [Then(@"the X-Warning in the response header should be ""(.*)""")]
        public void ThenTheX_WarningInTheResponseHeaderShouldBe(string xWarning)
        {
            Assert.AreEqual(xWarning, header.Get("X-Warning"));
        }
    }
}
