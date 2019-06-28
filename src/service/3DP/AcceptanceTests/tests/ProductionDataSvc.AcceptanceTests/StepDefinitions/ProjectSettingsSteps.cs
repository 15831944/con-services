﻿using Newtonsoft.Json.Linq;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("ProjectSettings.feature")]
  public class ProjectSettingsSteps : FeatureGetRequestBase<JObject>
  { }
}
