﻿using Newtonsoft.Json.Linq;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("CompactionPalette.feature")]
  public class CompactionPaletteSteps : FeatureGetRequestBase<JObject>
  { }
}
