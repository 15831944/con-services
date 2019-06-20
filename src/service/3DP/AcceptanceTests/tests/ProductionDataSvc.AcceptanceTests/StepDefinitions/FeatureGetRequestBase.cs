﻿using System;
using System.Globalization;
using System.Net;
using Gherkin.Ast;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProductionDataSvc.AcceptanceTests.Utils;
using Xunit;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  public abstract class FeatureGetRequestBase<TResponse> : Xunit.Gherkin.Quick.Feature where TResponse : JContainer
  {
    protected Getter<TResponse> GetResponseHandler;
    protected string Url;

    [Given(@"only the service route ""(.*)""")]
    public void GivenOnlyTheServiceRoute(string url)
    {
      Url = RestClient.Productivity3DServiceBaseUrl + url;
      GetResponseHandler = new Getter<TResponse>(url);
    }

    [Given(@"the service route ""(.*)"" and result repo ""(.*)""")]
    public void GivenTheServiceRouteAndResultRepo(string url, string resultFileName)
    {
      Url = RestClient.Productivity3DServiceBaseUrl + url;
      GetResponseHandler = new Getter<TResponse>(url, resultFileName);
    }

    [And(@"the result file ""(.*)""")]
    public void GivenTheResultFile(string resultFileName)
    {
      GetResponseHandler = new Getter<TResponse>(Url, resultFileName);
    }

    /// <summary>
    /// Generic method for setting any key/value query parameter using the mulitline input.
    /// </summary>
    [And(@"with parameter ""(.*)"" and multiline value:")]
    public void AndWithParameterAndMultilineValue(string parameterName, DocString parameterValue)
    {
      if (!string.IsNullOrEmpty(parameterName) && !string.IsNullOrEmpty(parameterValue.Content))
      {
        GetResponseHandler.QueryString.Add(parameterName, WebUtility.UrlEncode(parameterValue.Content));
      }
    }

    /// <summary>
    /// Generic method for setting any key/value query parameter.
    /// </summary>
    [And(@"with parameter ""(.*)"" with value ""(.*)""")]
    public void AndWithParameterWithValue(string parameterName, string parameterValue)
    {
      if (!string.IsNullOrEmpty(parameterName) && !string.IsNullOrEmpty(parameterValue))
      {
        GetResponseHandler.QueryString.Add(parameterName, parameterValue);
      }
    }

    [And(@"with array parameter ""(.*)"" with values ""(.*)""")]
    public void GivenISelectStationOffsetReportParameters(string parameterName, string arrayValues)
    {
      if (string.IsNullOrEmpty(arrayValues))
      {
        return;
      }

      var queryParams = Array.ConvertAll(arrayValues.Split(','), double.Parse);

      for (var i = 0; i < queryParams.Length; i++)
      {
        GetResponseHandler.QueryString.Add($"{parameterName}[{i}]", queryParams[i].ToString(CultureInfo.InvariantCulture));
      }
    }

    [When(@"I send the GET request I expect response code (\d+)")]
    public void WhenISendTheGetRequestIExpectResponseCode(int httpCode)
    {
      GetResponseHandler.SendRequest(Url, httpCode);
    }

    [When(@"I send a GET request with Accept header ""(.*)"" I expect response code (\d+)")]
    public void WhenISendTheGetRequestWithAcceptHeaderIExpectResponseCode(string acceptHeader, int httpCode)
    {
      GetResponseHandler.SendRequest(Url, expectedHttpCode: httpCode, acceptHeader: acceptHeader);
    }

    [Then(@"the response should be:")]
    public void ThenTheResponseShouldBe(DocString json)
    {
      var expectedJObject = JsonConvert.DeserializeObject<JObject>(json.Content);

      ObjectComparer.RoundAllDoubleProperties(GetResponseHandler.CurrentResponse as JObject, roundingPrecision: 8);
      ObjectComparer.RoundAllDoubleProperties(expectedJObject, roundingPrecision: 8);

      ObjectComparer.AssertAreEqual(actualResultObj: GetResponseHandler.CurrentResponse, expectedResultObj: expectedJObject, ignoreCase: true);
    }

    [Then(@"the response should exactly match ""(.*)"" from the repository")]
    public void ThenTheResponseShouldExactlyMatchFromTheRepository(string resultName)
    {
      var actualJObject = JObject.FromObject(GetResponseHandler.CurrentResponse);
      var expectedJObject = JsonConvert.DeserializeObject<TResponse>(GetResponseHandler.ResponseRepo[resultName].ToString());

      ObjectComparer.AssertAreEqual(actualResultObj: actualJObject, expectedResultObj: expectedJObject);
    }

    [Then(@"the response should match to (.*) decimal places ""(.*)"" from the repository")]
    public void ThenTheResponseShouldMatchToDecimalPlacesFromTheRepository(int precision, string resultName)
    {
      var actualJObject = JObject.FromObject(GetResponseHandler.CurrentResponse);
      var expectedJObject = JsonConvert.DeserializeObject<JObject>(GetResponseHandler.ResponseRepo[resultName].ToString());

      ObjectComparer.RoundAllDoubleProperties(actualJObject, roundingPrecision: precision);
      ObjectComparer.RoundAllDoubleProperties(expectedJObject, roundingPrecision: precision);

      ObjectComparer.AssertAreEqual(actualResultObj: actualJObject, expectedResultObj: expectedJObject, resultName: resultName);
    }

    [Then(@"the array response should match ""(.*)"" from the repository")]
    public void ThenTheArrayResponseShouldMatchFromTheRepository(string resultName)
    {
      var actualJObject = JArray.FromObject(GetResponseHandler.CurrentResponse);
      var expectedJObject = JsonConvert.DeserializeObject<JArray>(GetResponseHandler.ResponseRepo[resultName].ToString());

      ObjectComparer.AssertAreEqual(actualResultObj: actualJObject, expectedResultObj: expectedJObject, resultName: resultName);
    }

    [Then(@"the response should match ""(.*)"" from the repository")]
    public void ThenTheResultShouldMatchFromTheRepository(string resultName)
    {
      ThenTheResponseShouldMatchToDecimalPlacesFromTheRepository(8, resultName);
    }

    [Then(@"the response should contain code (.*)")]
    public void ThenTheResponseShouldContainErrorCode(int expectedCode)
    {
      var resultCode = (int)GetResponseHandler.CurrentResponse["Code"];
      Assert.Equal(expectedCode, resultCode);
    }

    [Then(@"the response should contain message ""(.*)"" and code ""(.*)""")]
    public void ThenTheResultShouldContainMessageAndCode(string expectedMessage, int expectedCode)
    {
      var resultCode = (int)GetResponseHandler.CurrentResponse["Code"];
      Assert.Equal(expectedCode, resultCode);

      var resultMessage = (string)GetResponseHandler.CurrentResponse["Message"];
      Assert.Equal(expectedMessage, resultMessage);
    }

    [Then(@"the resulting image should match ""(.*)"" from the response repository within (\d+) percent")]
    public void ThenTheResultingImageShouldMatchFromTheResponseRepositoryWithinPercent(string responseName, int tolerance)
    {
      var imageDifference = Convert.ToDouble(tolerance) / 100;
      var expectedTileData = (byte[])GetResponseHandler.ResponseRepo[responseName]["tileData"];
      var actualTileData = (byte[])GetResponseHandler.CurrentResponse["tileData"];
      var expFileName = "Expected_" + responseName + ".jpg";
      var actFileName = "Actual_" + responseName + ".jpg";

      var differencePercent = ImageUtils.CompareImagesAndGetDifferencePercent(expectedTileData, actualTileData, expFileName, actFileName);

      Assert.True(Math.Abs(differencePercent) < imageDifference, "Actual Difference:" + differencePercent * 100 + "% Expected tiles (" + expFileName + ") doesn't match actual tiles (" + actFileName + ")");
    }

    protected void AssertObjectsAreEqual<T>(T expectedResult)
    {
      var actualResult = JsonConvert.DeserializeObject<T>(GetResponseHandler.CurrentResponse.ToString());

      var actualResultJson = JsonConvert.SerializeObject(actualResult);
      var expectedResultJson = JsonConvert.SerializeObject(expectedResult);

      Assert.True(JToken.DeepEquals(expectedResultJson, actualResultJson));
    }
  }
}
