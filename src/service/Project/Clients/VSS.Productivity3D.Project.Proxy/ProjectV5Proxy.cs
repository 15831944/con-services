﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;

namespace VSS.Productivity3D.Project.Proxy
{
  public class ProjectV5Proxy : BaseServiceDiscoveryProxy, IProjectProxy
  {
    public ProjectV5Proxy(IWebRequest webRequest, IConfigurationStore configurationStore, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution) 
      : base(webRequest, configurationStore, logger, dataCache, serviceResolution)
    {
    }

    public  override bool IsInsideAuthBoundary => true;

    public  override ApiService InternalServiceType => ApiService.Project;

    public override string ExternalServiceName => null;

    public  override ApiVersion Version => ApiVersion.V5;

    public  override ApiType Type => ApiType.Public;

    public  override string CacheLifeKey => "PROJECT_CACHE_LIFE";

    public async Task<List<ProjectData>> GetProjects(string accountUid, IDictionary<string, string> customHeaders = null)
    {
      var result = await GetMasterDataItemServiceDiscovery<ProjectDataResult>("/project", accountUid, null, customHeaders);

      if (result.Code == 0)
        return result.ProjectDescriptors;

      log.LogDebug($"Failed to get list of projects: {result.Code}, {result.Message}");
      return null;
    }

    public Task<ProjectData> GetProject(long shortRaptorProjectId, IDictionary<string, string> customHeaders = null)
    {
      // todoMaverick
      // ProjectSvc.ProjectController should be able to get this from localDB now.
      // response should include accountUid
      throw new System.NotImplementedException();
    }

    public Task<ProjectData> GetProject(string projectUid, IDictionary<string, string> customHeaders = null)
    {
      // todoMaverick
      // ProjectSvc.ProjectController should be able to get this from localDB now.
      // response should include accountUid
      throw new System.NotImplementedException();
    }

    public Task<List<ProjectData>> GetIntersectingProjects(string accountUid, 
      double latitude, double longitude, string projectUid = null, DateTime? timeOfPosition = null, IDictionary<string, string> customHeaders = null)
    {
      // todoMaverick
      // ProjectSvc.ProjectController should:
      // a) if projectUid, get it if it overlaps in localDB
      //    else get overlapping projects in localDB for this AccountUid. 
      //  Note that if timeOfPosition == null, don't check it.
      throw new System.NotImplementedException();
    }

    // Have device
    //    get list of projects the loc is within (note if manualImport then time will be null, dont' check time)
    //    want to get list of projects it is associated with
    //    want to know if loc within any of these todoMaverick may be quicker to do pointinpoly first, then 
    //    if lo , want to know if loc within it
    //     also, if device is !enpty, then see if device is associated with the project (and claimed etc)

    public Task<List<ProjectData>> GetIntersectingProjectForDevice(string deviceAccountUid, string deviceUid,
      double latitude, double longitude, DateTime? timeOfPosition = null, IDictionary<string, string> customHeaders = null)
    {
      // todoMaverick ProjectSvc should:
      // a) get overlapping project in localDB for this AccountUid. Note if timeOfPosition = null then don't check time.
      // b) if >1 project, and if deviceUid not empty,
      //    retrieve list of projects from WM which this device is associated with
      //    pair up active projects, good devices and project-device associations
      throw new System.NotImplementedException();
    }

    public async Task<ProjectData> GetProjectForAccount(string accountUid, string projectUid,
      IDictionary<string, string> customHeaders = null)
    {
      var result = await GetMasterDataItemServiceDiscovery<ProjectDataSingleResult>($"/project/{projectUid}",
        projectUid,
        null,
        customHeaders);

      if (result.Code == 0)
        return result.ProjectDescriptor;

      log.LogDebug($"Failed to get project with Uid {projectUid}: {result.Code}, {result.Message}");
      return null;
    }

    //To support 3dpm v1 end points which use legacy project id
    public async Task<ProjectData> GetProjectForAccount(string accountUid, long shortRaptorProjectId,
      IDictionary<string, string> customHeaders = null)
    {
      return await GetItemWithRetry<ProjectDataResult, ProjectData>(GetProjects, p => p.ShortRaptorProjectId == shortRaptorProjectId, accountUid, customHeaders);
    }

    /// <summary>
    /// Clears an item from the cache
    /// </summary>
    /// <param name="uid">The uid of the item (either accountUid or projectUid) to remove from the cache</param>
    /// <param name="userId">The user ID</param>
    public void ClearCacheItem(string uid, string userId = null)
    {
      ClearCacheByTag(uid);

      if (string.IsNullOrEmpty(userId))
        ClearCacheByTag(userId);
    }

  }
}