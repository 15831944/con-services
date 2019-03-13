﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;

namespace VSS.MasterData.Project.WebAPI.Common.Models
{
  /// <summary>
  /// The request representation used to Upsert a project. 
  /// If CustomerUI, ProjectUID are null, then they will be populated via other means.
  /// </summary>
  public class UpdateProjectGeofenceRequest
  {
    /// <summary>
    /// The unique ID of the project. if null, then one will be generated.
    /// </summary>
    [JsonProperty(PropertyName = "ProjectUID", Required = Required.Always)]
    public Guid ProjectUid { get; set; }

    /// <summary>
    /// The geofence type/s which this request applies to
    /// </summary>
    [JsonProperty(PropertyName = "GeofenceTypes", Required = Required.Always)]
    public List<GeofenceType> GeofenceTypes;

    /// <summary>
    /// The complete list of Geofences of type associated with this project
    /// </summary>
    [JsonProperty(PropertyName = "GeofenceGuids", Required = Required.Always)]
    public List<Guid> GeofenceGuids;

    /// <summary>
    /// Private constructor
    /// </summary>
    private UpdateProjectGeofenceRequest()
    {
    }

    /// <summary>
    /// Create instance of CreateProjectGeofenceRequest
    /// </summary>
    public static UpdateProjectGeofenceRequest CreateUpdateProjectGeofenceRequest(Guid projectUid,
      List<GeofenceType> geofenceTypes, List<Guid> geofenceUids)
    {
      return new UpdateProjectGeofenceRequest
      {
        ProjectUid = projectUid,
        GeofenceTypes = geofenceTypes,
        GeofenceGuids = geofenceUids
      };
    }

    public void Validate()
    {
      if (ProjectUid == null || Guid.TryParse(ProjectUid.ToString(), out var x) == false || ProjectUid == Guid.Empty)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(2005, "Missing ProjectUID."));
      }

      ProjectDataValidator.ValidateGeofenceTypes(GeofenceTypes);

      if (GeofenceGuids == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(2103, "ProjectGeofenceAssociation: Invalid GeofenceUid list."));
      }

      if (GeofenceGuids == null || GeofenceGuids.Count == 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(2103, "ProjectGeofenceAssociation: Invalid GeofenceUid list."));
      }

      foreach (var g in GeofenceGuids)
      {
        if (Guid.TryParse(g.ToString(), out var y) == false || g == Guid.Empty)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(2103, "ProjectGeofenceAssociation: Invalid GeofenceUid list."));
        }
      }
      if (GeofenceGuids.Count != GeofenceGuids.Distinct().Count())
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(2110, "ProjectGeofenceAssociation: Duplicate GeofenceUids appear in the request body."));

      }
    }
  }
}
