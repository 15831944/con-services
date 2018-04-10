﻿using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using System.Reflection;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Common.Filters.Authentication
{
  /// <summary>
  /// Validation filter attribute for the ProjectUid.
  /// </summary>
  public class ProjectUidVerifier : ActionFilterAttribute
  {
    private static string Name => "projectUid";

    /// <summary>
    /// Gets or sets whether the Filter will check for and reject Landfill Projects.
    /// </summary>
    public bool AllowLandfillProjects { get; set; }

    /// <summary>
    /// Gets or sets whether the Filter will check for and reject archived Projects.
    /// </summary>
    public bool AllowArchivedState { get; set; }

    public override void OnActionExecuting(ActionExecutingContext actionContext)
    {
      object projectUidValue = null;

      if (actionContext.ActionArguments.ContainsKey("request"))
      {
        var request = actionContext.ActionArguments["request"];

        // Ignore any query parameter called 'request'.
        if (request.GetType() != typeof(string))
        {
          projectUidValue = request.GetType()
                                   .GetProperty(Name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
                                   ?.GetValue(request);
        }
      }

      if (actionContext.ActionArguments.ContainsKey(Name))
      {
        projectUidValue = actionContext.ActionArguments[Name];
      }

      if (!(projectUidValue is string))
      {
        return;
      }

      // RaptorPrincipal will handle the failure case where project isn't found.
      var projectDescriptor = (actionContext.HttpContext.User as RaptorPrincipal).GetProject((string)projectUidValue).Result;

      if (this.AllowLandfillProjects && projectDescriptor.ProjectType == ProjectType.LandFill)
      {
        throw new ServiceException(HttpStatusCode.Unauthorized,
          new ContractExecutionResult(ContractExecutionStatesEnum.AuthError,
            "Don't have access to the selected landfill project."));
      }

      if (this.AllowArchivedState && projectDescriptor.IsArchived)
      {
        throw new ServiceException(HttpStatusCode.Unauthorized,
          new ContractExecutionResult(ContractExecutionStatesEnum.AuthError,
            "Don't have write access to the selected project."));
      }
    }
  }
}