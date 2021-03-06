﻿using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Common.Models
{
  /// <summary>
  /// Contains a percentage range of observed CCV values with respect to the target MDP value configured on a machine
  /// </summary>
  public class CCVRangePercentage 
  {
    private const double MIN_PERCENT = 0.0;
    private const double MAX_PERCENT = 250.0;

    /// <summary>
    /// The minimum percentage range. Must be between 0 and 250.
    /// </summary>
    [Range(MIN_PERCENT, MAX_PERCENT)]
    [JsonProperty(PropertyName = "min", Required = Required.Always)]
    [Required]
    public double Min { get; private set; }

    /// <summary>
    /// The maximum percentage range. Must be between 0 and 250.
    /// </summary>
    [Range(MIN_PERCENT, MAX_PERCENT)]
    [JsonProperty(PropertyName = "max", Required = Required.Always)]
    [Required]
    public double Max { get; private set; }


    /// <summary>
    /// Default private constructor
    /// </summary>
    private CCVRangePercentage()
    { }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    public CCVRangePercentage(double min, double max)
    {
      Min = min;
      Max = max;
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {

      if (Min > Max)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "CCV percentage minimum must be less than CCV percentage maximum"));
      }
    }
  }
}
