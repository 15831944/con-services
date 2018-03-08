﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Common.Filters.Authentication.Models
{
  /// <summary>
  ///   Custom principal for Raptor with list of projects.
  /// </summary>
  public class RaptorPrincipal : ClaimsPrincipal
  {
    private readonly IProjectListProxy projectProxy;
    private readonly IDictionary<string, string> authNContext;

    //We need to delefgate Project retrieval downstream as project may not accessible to a user once it has been created
    public RaptorPrincipal(ClaimsIdentity identity, string customerUid, 
      string username, string customername, IProjectListProxy projectProxy, IDictionary<string, string> contextHeaders, bool isApplication = false) : base(identity)
    {
      CustomerUid = customerUid;
      IsApplication = isApplication;
      UserEmail = username;
      CustomerName = customername;
      this.projectProxy = projectProxy;
      this.authNContext = contextHeaders;
    }

    public string CustomerUid { get; }

    public IEnumerable<ProjectDescriptor> Projects => RetreieveProjects();

    public string UserEmail { get; }

    public string CustomerName { get; }

    public bool IsApplication { get; }

    private void InvalidateProjectList()
    {
      projectProxy.ClearCacheItem(CustomerUid);
    }

    private IEnumerable<ProjectDescriptor> RetreieveProjects()
    {
      var customerProjects = projectProxy.GetProjectsV4(CustomerUid, authNContext).Result;
      if (customerProjects != null)
      {
        foreach (var project in customerProjects)
        {
          var projectDesc = new ProjectDescriptor
          {
            isLandFill = project.ProjectType == ProjectType.LandFill,
            isArchived = project.IsArchived,
            projectUid = project.ProjectUid,
            projectId = project.LegacyProjectId,
            coordinateSystemFileName = project.CoordinateSystemFileName,
            projectGeofenceWKT = project.ProjectGeofenceWKT,
            projectTimeZone = project.ProjectTimeZone,
            ianaTimeZone = project.IanaTimeZone
          };
          yield return projectDesc;
        }
      }
    }

    /// <summary>
    ///   Get the project descriptor for the specified project id.
    /// </summary>
    /// <param name="projectId">The project ID</param>
    /// <returns>Project descriptor</returns>
    public ProjectDescriptor GetProject(long projectId)
    {
      var projectDescr = Projects.FirstOrDefault(p => p.projectId == projectId);

      if (projectDescr != null) return projectDescr;
      InvalidateProjectList();
      projectDescr = Projects.FirstOrDefault(p => p.projectId == projectId);
      if (projectDescr != null) return projectDescr;

      throw new ServiceException(HttpStatusCode.Unauthorized,
          new ContractExecutionResult(ContractExecutionStatesEnum.AuthError,
            $"Missing Project or project does not belong to specified customer or don't have access to the project {projectId}"));
    }

    /// <summary>
    ///   Get the project descriptor for the specified project uid.
    /// </summary>
    /// <param name="projectUid">THe project UID</param>
    /// <returns>Project descriptor</returns>
    public ProjectDescriptor GetProject(Guid? projectUid)
    {
      if (!projectUid.HasValue)
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Missing project UID"));
      return GetProject(projectUid.ToString());
    }

    /// <summary>
    ///   Get the project descriptor for the specified project uid.
    /// </summary>
    /// <param name="projectUid">THe project UID</param>
    /// <returns>Project descriptor</returns>
    public ProjectDescriptor GetProject(string projectUid)
    {
      if (string.IsNullOrEmpty(projectUid))
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Missing project UID"));

      var projectDescr =
        Projects.FirstOrDefault(p => string.Equals(p.projectUid, projectUid, StringComparison.OrdinalIgnoreCase));
      if (projectDescr != null) return projectDescr;

      InvalidateProjectList();
      projectDescr =
        Projects.FirstOrDefault(p => string.Equals(p.projectUid, projectUid, StringComparison.OrdinalIgnoreCase));

      if (projectDescr != null) return projectDescr;

      throw new ServiceException(HttpStatusCode.Unauthorized,
        new ContractExecutionResult(ContractExecutionStatesEnum.AuthError,
          $"Missing Project or project does not belong to specified customer or don't have access to the project {projectUid}"));
    }

    /// <summary>
    ///   Gets the legacy project id for the specified project uid
    /// </summary>
    /// <param name="projectUid">THe project UID</param>
    /// <returns>Legacy project ID</returns>
    public long GetProjectId(Guid? projectUid)
    {
      var projectId = GetProject(projectUid).projectId;

      if (projectId <= 0)
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.AuthError, "Missing project ID"));

      return projectId;
    }
  }
}