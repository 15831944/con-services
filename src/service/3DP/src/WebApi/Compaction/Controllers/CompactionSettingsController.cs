﻿using System;
using System.Net;
using System.Security.Principal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Controller for validating 3D project settings
  /// </summary>
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
  public class CompactionSettingsController : Controller
  {
    /// <summary>
    /// LoggerFactory for logging
    /// </summary>
    private readonly ILogger log;

    /// <summary>
    /// For getting project settings for a project
    /// </summary>
    private readonly IProjectSettingsProxy projectSettingsProxy;

    /// <summary>
    /// Constructor with dependency injection
    /// </summary>
    /// <param name="logger">LoggerFactory</param>
    /// <param name="projectSettingsProxy">Project settings proxy</param>
    public CompactionSettingsController(ILoggerFactory logger, IProjectSettingsProxy projectSettingsProxy)
    {
      this.log = logger.CreateLogger<CompactionSettingsController>();
      this.projectSettingsProxy = projectSettingsProxy;
    }

    /// <summary>
    /// Validates 3D project settings.
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="projectSettings">Project settings to validate as a JSON object</param>
    /// <param name="settingsType">The project settings' type</param>
    /// <returns>ContractExecutionResult</returns>
    [ProjectVerifier]
    [Route("api/v2/validatesettings")]
    [HttpGet]
    public ContractExecutionResult ValidateProjectSettings(
      [FromQuery] Guid projectUid,
      [FromQuery] string projectSettings,
      [FromQuery] ProjectSettingsType? settingsType,
      [FromServices] IServiceExceptionHandler serviceExceptionHandler)
    {
      log.LogInformation("ValidateProjectSettings: " + Request.QueryString);

      return ValidateProjectSettingsEx(projectUid.ToString(), projectSettings, settingsType, serviceExceptionHandler);
    }

    /// <summary>
    /// Validates 3D project settings.
    /// </summary>
    /// <param name="request">Description of the Project Settings request.</param>
    /// <returns>ContractExecutionResult</returns>
    [Route("api/v2/validatesettings")]
    [HttpPost]
    public ContractExecutionResult ValidateProjectSettings([FromBody] ProjectSettingsRequest request,
      [FromServices] IServiceExceptionHandler serviceExceptionHandler)
    {
      log.LogDebug($"UpsertProjectSettings: {JsonConvert.SerializeObject(request)}");

      request.Validate();

      return ValidateProjectSettingsEx(request.projectUid, request.Settings, request.ProjectSettingsType, serviceExceptionHandler);
    }

    private ContractExecutionResult ValidateProjectSettingsEx(string projectUid, string projectSettings, ProjectSettingsType? settingsType, IServiceExceptionHandler serviceExceptionHandler)
    {
      if (!string.IsNullOrEmpty(projectSettings))
      {
        if (settingsType == null)
          settingsType = ProjectSettingsType.Targets;

        switch (settingsType)
        {
          case ProjectSettingsType.Targets:
            var compactionSettings = GetProjectSettingsTargets(projectSettings);
            compactionSettings?.Validate(serviceExceptionHandler);
            break;
          case ProjectSettingsType.Colors:
            var colorSettings = GetProjectSettingsColors(projectSettings);
            colorSettings?.Validate(serviceExceptionHandler);
            break;
          default:
            throw new ServiceException(HttpStatusCode.InternalServerError,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, $"Unsupported project settings type {settingsType} to validate."));
        }
        //It is assumed that the settings are about to be saved.
        //Clear the cache for these updated settings so we get the updated settings for compaction requests.
        log.LogDebug($"About to clear settings for project {projectUid + settingsType}");
        ClearProjectSettingsCaches(projectUid + settingsType, Request.Headers.GetCustomHeaders());
      }
      log.LogInformation("ValidateProjectSettings returned: " + Response.StatusCode);
      return new ContractExecutionResult(ContractExecutionStatesEnum.ExecutedSuccessfully, $"Project settings {settingsType} are valid");
    }

    /// <summary>
    /// Deserializes the project settings targets
    /// </summary>
    /// <param name="projectSettings">JSON representation of the project settings</param>
    /// <returns>The project settings targets instance</returns>
    private CompactionProjectSettings GetProjectSettingsTargets(string projectSettings)
    {
      CompactionProjectSettings ps = null;

      if (!string.IsNullOrEmpty(projectSettings))
      {
        try
        {
          ps = JsonConvert.DeserializeObject<CompactionProjectSettings>(projectSettings);
        }
        catch (Exception ex)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              ex.Message));
        }
      }
      return ps;
    }

    /// <summary>
    /// Deserializes the project settings colors
    /// </summary>
    /// <param name="projectSettings">JSON representation of the project settings</param>
    /// <returns>The project settings colors instance</returns>
    private CompactionProjectSettingsColors GetProjectSettingsColors(string projectSettings)
    {
      CompactionProjectSettingsColors ps = null;

      if (!string.IsNullOrEmpty(projectSettings))
      {
        try
        {
          ps = JsonConvert.DeserializeObject<CompactionProjectSettingsColors>(projectSettings);
        }
        catch (Exception ex)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              ex.Message));
        }
      }
      return ps;
    }

    /// <summary>
    /// Gets the User uid/applicationID from the context.
    /// </summary>
    /// <exception cref="ArgumentException">Incorrect user Id value.</exception>
    private string GetUserId()
    {
      if (User is RaptorPrincipal principal && (principal.Identity is GenericIdentity identity))
      {
        return identity.Name;
      }

      throw new ArgumentException("Incorrect UserId in request context principal.");
    }

    /// <summary>
    /// Clears the project settings cache in the proxy.
    /// </summary>
    /// <param name="projectUid">The project UID that the cached items belong to</param>
    /// <param name="customHeaders">The custom headers of the notification request</param>
    private void ClearProjectSettingsCaches(string projectUid, IHeaderDictionary customHeaders)
    {
      log.LogInformation("Clearing project settingss cache for project {0}", projectUid);

      //Clear file list cache and reload
      if (!customHeaders.ContainsKey("X-VisionLink-ClearCache"))
      {
        customHeaders.Add("X-VisionLink-ClearCache", "true");
      }

      projectSettingsProxy.ClearCacheItem(projectUid, GetUserId());
    }
  }
}
