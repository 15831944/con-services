﻿using Newtonsoft.Json.Linq;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("CompactionDesignProfile.feature")]
  public class CompactionDesignProfileSteps : FeatureGetRequestBase<JObject>
  { }
}
