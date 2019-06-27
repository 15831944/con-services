﻿using Newtonsoft.Json.Linq;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("CcaColorPalette.feature")]
  public class CcaColorPaletteSteps : FeatureGetRequestBase<JObject>
  { }
}
