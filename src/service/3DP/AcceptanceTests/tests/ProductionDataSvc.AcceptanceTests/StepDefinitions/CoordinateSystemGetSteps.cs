﻿using Newtonsoft.Json.Linq;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("CoordinateSystemGet.feature")]
  public class CoordinateSystemGetSteps : FeatureGetRequestBase<JObject>
  { }
}
