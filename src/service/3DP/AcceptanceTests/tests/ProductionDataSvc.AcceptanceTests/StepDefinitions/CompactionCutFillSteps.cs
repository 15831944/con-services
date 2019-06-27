﻿using Newtonsoft.Json.Linq;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("CompactionCutFill.feature")]
  public class CompactionCutFillSteps : FeatureGetRequestBase<JObject>
  { }
}
