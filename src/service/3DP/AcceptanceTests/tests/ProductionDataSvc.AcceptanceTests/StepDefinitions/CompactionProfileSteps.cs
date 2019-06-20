﻿using Newtonsoft.Json.Linq;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("CompactionProfile.feature")]
  public class CompactionProfileSteps : FeatureGetRequestBase<JObject>
  { }
}
