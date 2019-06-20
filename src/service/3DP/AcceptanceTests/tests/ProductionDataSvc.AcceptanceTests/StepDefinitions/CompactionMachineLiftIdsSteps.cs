﻿using Newtonsoft.Json.Linq;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("CompactionMachineLiftIds.feature")]
  public class CompactionMachineLiftIdsSteps : FeatureGetRequestBase<JObject>
  { }
}
