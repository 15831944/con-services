﻿using Newtonsoft.Json.Linq;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("NotificationDelete.feature")]
  public class NotificationDeleteSteps : FeatureGetRequestBase<JObject>
  { }
}
