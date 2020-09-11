﻿using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Internal;
using VSS.MasterData.Project.WebAPI.Factories;
using VSS.Productivity.Push.Models.Notifications.Changes;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Controllers
{
  /// <summary>
  /// Project Settings controller, version 4.
  /// </summary>
  public class ProjectSettingsV4Controller : BaseController<ProjectSettingsV4Controller>
  {
    private readonly IRequestFactory _requestFactory;


    /// <summary>
    /// Default constructor
    /// </summary>
    public ProjectSettingsV4Controller(IRequestFactory requestFactory)
    {
      this._requestFactory = requestFactory;
    }

    /// <summary>
    /// Gets the target project settings for a project and user. Returns the raw json for the settings.
    /// </summary>
    [Route("api/v4/projectsettings/{projectUid}")]
    [Route("api/v6/projectsettings/{projectUid}")]
    [HttpGet]
    public async Task<ProjectSettingsResult> GetProjectSettingsTargets(string projectUid)
    {
      return await GetProjectSettingsForType(projectUid, ProjectSettingsType.Targets);
    }

    /// <summary>
    /// Gets the project color settings for a project and user.  Returns the raw json for the settings.
    /// </summary>
    [Route("api/v4/projectcolors/{projectUid}")]
    [Route("api/v6/projectcolors/{projectUid}")]
    [HttpGet]
    public async Task<ProjectSettingsResult> GetProjectSettingsColors(string projectUid)
    {
      return await GetProjectSettingsForType(projectUid, ProjectSettingsType.Colors);
    }

    /// <summary>
    /// Upserts the target project settings for a project and user.
    /// </summary>
    [Route("api/v4/projectcolors")]
    [Route("api/v6/projectcolors")]
    [HttpPut]
    public async Task<ProjectSettingsResult> UpsertProjectColors([FromBody]ProjectSettingsRequest request)
    {
      if (string.IsNullOrEmpty(request?.projectUid))
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 68);
      LogCustomerDetails("UpsertProjectSettings", request?.projectUid);
      Logger.LogDebug($"UpsertProjectSettings: {JsonConvert.SerializeObject(request)}");

      request.ProjectSettingsType = ProjectSettingsType.Colors;

      var projectSettingsRequest = _requestFactory.Create<ProjectSettingsRequestHelper>(r => r
          .CustomerUid(CustomerUid))
        .CreateProjectSettingsRequest(request.projectUid, request.Settings, request.ProjectSettingsType);
      projectSettingsRequest.Validate();

      var result = (await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainerFactory
          .Build<UpsertProjectSettingsExecutor>(LoggerFactory, ConfigStore, ServiceExceptionHandler,
            CustomerUid, UserId, headers: customHeaders,
            productivity3dV2ProxyCompaction: Productivity3dV2ProxyCompaction, 
            projectRepo: ProjectRepo, cwsProjectClient: CwsProjectClient)
          .ProcessAsync(projectSettingsRequest)
      )) as ProjectSettingsResult;

      await NotifyChanges(UserId, request.projectUid);

      Logger.LogResult(this.ToString(), JsonConvert.SerializeObject(request), result);
      return result;
    }

    /// <summary>
    /// Upserts the target project settings for a project and user.
    /// </summary>
    [Route("api/v4/projectsettings")]
    [Route("api/v6/projectsettings")]
    [HttpPut]
    public async Task<ProjectSettingsResult> UpsertProjectSettings([FromBody]ProjectSettingsRequest request)
    {
      if (string.IsNullOrEmpty(request?.projectUid))
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 68);
      LogCustomerDetails("UpsertProjectSettings", request?.projectUid);
      Logger.LogDebug($"UpsertProjectSettings: {JsonConvert.SerializeObject(request)}");

      request.ProjectSettingsType = ProjectSettingsType.Targets;

      var projectSettingsRequest = _requestFactory.Create<ProjectSettingsRequestHelper>(r => r
            .CustomerUid(CustomerUid))
          .CreateProjectSettingsRequest(request.projectUid, request.Settings, request.ProjectSettingsType);
      projectSettingsRequest.Validate();

      var result = (await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainerFactory
          .Build<UpsertProjectSettingsExecutor>(LoggerFactory, ConfigStore, ServiceExceptionHandler,
            CustomerUid, UserId, headers: customHeaders,
            productivity3dV2ProxyCompaction: Productivity3dV2ProxyCompaction, 
            projectRepo: ProjectRepo, cwsProjectClient: CwsProjectClient)
          .ProcessAsync(projectSettingsRequest)
      )) as ProjectSettingsResult;

      await NotifyChanges(UserId, request.projectUid);

      Logger.LogResult(this.ToString(), JsonConvert.SerializeObject(request), result);
      return result;
    }

    private async Task<ProjectSettingsResult> GetProjectSettingsForType(string projectUid, ProjectSettingsType settingsType)
    {
      if (string.IsNullOrEmpty(projectUid))
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 68);
      LogCustomerDetails("GetProjectSettings", projectUid);

      var projectSettingsRequest = _requestFactory.Create<ProjectSettingsRequestHelper>(r => r
          .CustomerUid(CustomerUid))
        .CreateProjectSettingsRequest(projectUid, string.Empty, settingsType);
      projectSettingsRequest.Validate();

      var result = (await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainerFactory
          .Build<GetProjectSettingsExecutor>(LoggerFactory, ConfigStore, ServiceExceptionHandler,
            CustomerUid, UserId, headers: customHeaders, 
            projectRepo: ProjectRepo, cwsProjectClient: CwsProjectClient)
          .ProcessAsync(projectSettingsRequest)
      )) as ProjectSettingsResult;

      Logger.LogResult(this.ToString(), projectUid, result);
      return result;
    }

    private Task NotifyChanges(string userUid, string projectUid)
    {
      // You'd like to think these are in the format of a guid, but a simple check will stop any cast exceptions

      var userTask = Guid.TryParse(userUid, out var u)
        ? NotificationHubClient.Notify(new UserChangedNotification(u))
        : Task.CompletedTask;

      var projectTask = Guid.TryParse(projectUid, out var p)
        ? NotificationHubClient.Notify(new ProjectChangedNotification(p))
        : Task.CompletedTask;

      return Task.WhenAll(userTask, projectTask);
    }
  }
}
