﻿using Newtonsoft.Json.Linq;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("CompactionPassCount.feature")]
  public class CompactionPassCountSteps : FeatureGetRequestBase<JObject>
  { }
}
