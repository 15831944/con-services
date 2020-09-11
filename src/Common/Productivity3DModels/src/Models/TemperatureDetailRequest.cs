﻿using System;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Filter.Abstractions.Models;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// Request for a temperature detail report
  /// </summary>
  public class TemperatureDetailRequest : TRexBaseRequest
  {
    /// <summary>
    /// The array of temperature limits to be accounted for in the temperature count analysis.
    /// Order is from low to high.
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public int[] TemperatureList { get; private set; }


    /// <summary>
    /// Default private constructor.
    /// </summary>
    private TemperatureDetailRequest()
    {
    }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    public TemperatureDetailRequest(
      Guid projectUid,
      FilterResult filter,
      int[] temperatureList,
      OverridingTargets overrides,
      LiftSettings liftSettings
    )
    {
      ProjectUid = projectUid;
      Filter = filter;
      TemperatureList = temperatureList;
      Overrides = overrides;
      LiftSettings = liftSettings;
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public override void Validate()
    {
      base.Validate();

      if (TemperatureList == null || TemperatureList.Length == 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Temperature list required"));
      }
      for (int i = 1; i < TemperatureList.Length; i++)
      {
        if (TemperatureList[i] <=TemperatureList[i - 1])
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Temperature list must be ordered from lowest to the highest"));
        }
      }
    }


  }
}
