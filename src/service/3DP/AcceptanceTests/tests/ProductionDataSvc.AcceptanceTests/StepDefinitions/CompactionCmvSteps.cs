﻿using Newtonsoft.Json.Linq;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("CompactionCmv.feature")]
  public class CompactionCmvSteps : FeatureGetRequestBase<JObject>
  { }
}
