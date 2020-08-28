﻿using System;
using Newtonsoft.Json;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Productivity3D.Models;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Models
{
  /// <summary>
  /// Request to get manually edited production data changes.
  /// </summary>
  public class GetEditDataRequest : ProjectID, IValidatable
  {

    /// <summary>
    /// The id of the machine whose data is overridden. If not provided then overridden data for all machines for the specified project is returned.
    /// </summary>
    [JsonProperty(PropertyName = "assetId", Required = Required.Default)]
    public long? assetId { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private GetEditDataRequest()
    {
    }


    /// <summary>
    /// Create instance of GetEditDataRequest
    /// </summary>
    public static GetEditDataRequest CreateGetEditDataRequest(
      long projectId,
      long assetId,
      Guid? projectUid
      )
    {
      return new GetEditDataRequest
      {
        ProjectId = projectId,
        assetId = assetId,
        ProjectUid = projectUid
      };
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public override void Validate()
    {
      base.Validate();
    }
  }
}
