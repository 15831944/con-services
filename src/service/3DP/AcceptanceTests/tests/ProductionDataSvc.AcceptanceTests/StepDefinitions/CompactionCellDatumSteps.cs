﻿using Newtonsoft.Json.Linq;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("CompactionCellDatum.feature")]
  public class CompactionCellDatumSteps : FeatureGetRequestBase<JObject>
  { }
}
