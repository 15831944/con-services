﻿using Newtonsoft.Json.Linq;
using ProductionDataSvc.AcceptanceTests.Models;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("SummaryVolumes.feature")]
  public class SummaryVolumesSteps : FeaturePostRequestBase<JObject, ResponseBase>
  { }
}
