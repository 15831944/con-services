﻿using Newtonsoft.Json.Linq;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("DxfTile.feature")]
  public class DxfTileSteps : FeatureGetRequestBase<JObject>
  { }
}
