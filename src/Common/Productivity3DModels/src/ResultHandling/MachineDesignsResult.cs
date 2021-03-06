﻿using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.Models.ResultHandling
{
  public class MachineDesignsResult : ContractExecutionResult
  {
    /// <summary>
    /// The list of the on-machine designs available for the project.
    /// </summary>
    [JsonProperty(PropertyName = "designs")]
    public List<AssetOnDesignPeriodResult> AssetOnDesignPeriods { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private MachineDesignsResult()
    {
    }

    public MachineDesignsResult(List<AssetOnDesignPeriod> assetOnDesignPeriods)
    {
      AssetOnDesignPeriods = assetOnDesignPeriods.Select(d => new AssetOnDesignPeriodResult(d)).ToList();
    }
  }
}
