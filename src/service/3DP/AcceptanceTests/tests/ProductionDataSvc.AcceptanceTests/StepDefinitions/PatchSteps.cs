﻿using Newtonsoft.Json.Linq;
using ProductionDataSvc.AcceptanceTests.Models;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("Patch.feature")]
  public class PatchSteps : FeaturePostRequestBase<JObject, ResponseBase>
  { }
}
