﻿using Newtonsoft.Json.Linq;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("CompactionSpeed.feature")]
  public class CompactionSpeedSteps : FeatureGetRequestBase<JObject>
  { }
}
