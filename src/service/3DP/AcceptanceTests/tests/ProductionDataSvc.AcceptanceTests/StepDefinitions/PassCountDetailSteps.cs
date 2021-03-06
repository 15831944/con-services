﻿using Newtonsoft.Json.Linq;
using ProductionDataSvc.AcceptanceTests.Models;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("PassCountDetail.feature")]
  public class PassCountDetailSteps : FeaturePostRequestBase<JObject, ResponseBase>
  { }
}
