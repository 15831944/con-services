﻿using Newtonsoft.Json.Linq;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling
{
  public class DesignResult : ContractExecutionResult
  {
    /// <summary>
    /// Array of design boundaries in GeoJson format.
    /// </summary>
    /// 
    public JObject[] DesignBoundaries { get; private set; }

    /// <summary>
    /// Private constructor.
    /// </summary>
    /// 
    private DesignResult()
    {
      // ...
    }

    /// <summary>
    /// Creates an instance of the DesignResult class.
    /// </summary>
    /// <param name="designBoundaries">Array of design boundaries in GeoJson format.</param>
    /// <returns>A created instance of the AlignmentResult class.</returns>
    /// 
    public DesignResult (JObject[] designBoundaries)
    {
      DesignBoundaries = designBoundaries;
    }
  }
}
