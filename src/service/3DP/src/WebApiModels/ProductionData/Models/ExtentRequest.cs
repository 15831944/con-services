﻿using System;
using Newtonsoft.Json;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Productivity3D.Models;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Models
{
  /// <summary>
  /// A representation of a project extents request
  /// </summary>
  public class ExtentRequest : ProjectID, IValidatable
    {
      /// <summary>
      /// The set of surveyed surface IDs to be excluded from the calculation of the project extents
      /// </summary>
      [JsonProperty(PropertyName = "excludedSurveyedSurfaceIds", Required = Required.Default)]
      public long[] excludedSurveyedSurfaceIds { get; set; }


      public ExtentRequest(long projectId, Guid? projectUid, long[] ExcludedSurveyedSurfaceIds)
      {
        ProjectId = projectId;
        ProjectUid = projectUid;
        excludedSurveyedSurfaceIds = ExcludedSurveyedSurfaceIds;
      }

      public override void Validate()
      {
        base.Validate();
      }
    }
}
