﻿using Newtonsoft.Json.Linq;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("NotificationAdd.feature")]
  public class NotificationAddSteps : FeatureGetRequestBase<JObject>
  { }
}
