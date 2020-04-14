﻿using System;
using System.Collections.Generic;
using System.Net;
using VSS.MasterData.Models.Handlers;
using VSS.Productivity3D.Filter.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models;

namespace VSS.Productivity3D.Filter.Common.Models
{
  public class FilterRequestFull : FilterRequest
  {
    public ProjectData ProjectData { get; set; }

    public string CustomerUid { get; set; }

    public bool IsApplicationContext { get; set; }

    public string UserId { get; set; }

    public string ProjectUid { get; set; }

    public IDictionary<string, string> CustomHeaders { get; set; }
    
    public static FilterRequestFull Create(IDictionary<string, string> customHeaders, string customerUid, bool isApplicationContext, string userId, ProjectData projectData, FilterRequest request = null)
    {
      return new FilterRequestFull
      {
        FilterUid = request?.FilterUid ?? string.Empty,
        HierarchicFilterUids = request?.HierarchicFilterUids,
        Name = request?.Name ?? string.Empty,
        FilterJson = request?.FilterJson ?? string.Empty,
        FilterType = request?.FilterType ?? VSS.Visionlink.Interfaces.Events.MasterData.Models.FilterType.Transient,
        CustomerUid = customerUid,
        IsApplicationContext = isApplicationContext,
        UserId = userId,
        ProjectData = projectData,
        ProjectUid = projectData?.ProjectUID,
        CustomHeaders = customHeaders
      };
    }

    public override void Validate(IServiceExceptionHandler serviceExceptionHandler, bool onlyFilterUid = false)
    {
      if (string.IsNullOrEmpty(CustomerUid) || Guid.TryParse(CustomerUid, out _) == false)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 27);
      }

      if (string.IsNullOrEmpty(UserId) || (IsApplicationContext == false && Guid.TryParse(UserId, out _) == false))
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 28);
      }

      if (ProjectData == null || string.IsNullOrEmpty(ProjectUid) || Guid.TryParse(ProjectUid, out _) == false)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 1);
      }

      base.Validate(serviceExceptionHandler, onlyFilterUid);
    }
  }
}
