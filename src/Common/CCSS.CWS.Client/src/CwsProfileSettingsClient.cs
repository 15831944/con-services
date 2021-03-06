﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Clients.CWS;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Proxies.Interfaces;

namespace CCSS.CWS.Client
{
  /// <summary>
  /// These use the cws cws-profilesettingsmanager controller
  ///   See comments in CwsAccountClient re TRN/Guid conversions
  ///   
  /// How to create project configuration files:
  ///   1) Call POST CwsDeesignClient.CreateFile() with the filename you want to use. This returns the assigned fileSpaceID and Url.
  ///   2) Use the Url from #1 to PUT the binary file to DataOcean.
  ///   3) Call POST CwsProfileSettingsClient.SaveCalibrationFile() using the fileSpaceId from #1 (or PUT Update)
  ///   
  /// Files per ProjectConfigurationFileType.
  ///    Normally there is only 1 file per type
  ///    However for  control point and avoidance zones (at least), the user can select 2 files.
  ///      One for MachineControl, and the other for SiteCollectors. 
  ///      This is because each machine type supports different formats and content etc. 
  ///      Indicate in ProjectConfigurationModel machineControlFilespaceId and siteCollectorFilespaceId
  /// </summary>
  [Obsolete("UI to use cws directly now")]
  public class CwsProfileSettingsClient : CwsProfileSettingsManagerClient, ICwsProfileSettingsClient
  {
    public CwsProfileSettingsClient(IWebRequest gracefulClient, IConfigurationStore configuration, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(gracefulClient, configuration, logger, dataCache, serviceResolution)
    {
    }

    ///// <summary>
    ///// GET https://trimble.com/connectedsiteprofilesettings/1.0/projects/{projectId}/configuration/{fileType}
    ////// Only 1 of each project calibrationtype is allowed
    /////   user token
    /////   used by ProjectSvc v6 and v5TBC
    ///// </summary>
    public async Task<ProjectConfigurationModel> GetProjectConfiguration(Guid projectUid, ProjectConfigurationFileType projectConfigurationFileType, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(GetProjectConfiguration)}: projectUid {projectUid} projectConfigurationFileType {projectConfigurationFileType}");

      var projectTrn = TRNHelper.MakeTRN(projectUid);
      var projectConfigurationModel =
        await GetData<ProjectConfigurationModel>($"/projects/{projectTrn}/configuration/{projectConfigurationFileType.ToString().ToUpper()}", null, null, null, customHeaders);

      log.LogDebug($"{nameof(GetProjectConfiguration)}: projectConfigurationModel {JsonConvert.SerializeObject(projectConfigurationModel)}");
      return projectConfigurationModel;
    }

    ///// <summary>
    ///// GET https://trimble.com/connectedsiteprofilesettings/1.0/projects/{projectId}/configuration
    ////// Only 1 of each project calibrationtype is allowed
    /////   user token
    /////   used by ProjectSvc v6 and v5TBC
    ///// </summary>
    public async Task<ProjectConfigurationFileListResponseModel> GetProjectConfigurations(Guid projectUid, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(GetProjectConfigurations)}: projectUid {projectUid}");

      var projectTrn = TRNHelper.MakeTRN(projectUid);
      var projectConfigurationFileListResponse =
        await GetData<ProjectConfigurationFileListResponseModel>($"/projects/{projectTrn}/configuration", null, null, null, customHeaders);

      log.LogDebug($"{nameof(GetProjectConfigurations)}: projectConfigurationFileListResponse {JsonConvert.SerializeObject(projectConfigurationFileListResponse)}");
      return projectConfigurationFileListResponse;
    }

    ///// <summary>
    ///// POST https://trimble.com/connectedsiteprofilesettings/1.0/projects/{projectId}/configuration/{fileType}
    /////   user token
    /////   used by ProjectSvc v6 and v5TBC
    ///// </summary>
    public async Task<ProjectConfigurationModel> SaveProjectConfiguration(Guid projectUid, ProjectConfigurationFileType projectConfigurationFileType, ProjectConfigurationFileRequestModel projectConfigurationFileRequest, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(SaveProjectConfiguration)}: projectUid {projectUid} projectConfigurationFileType {projectConfigurationFileType} projectConfigurationFileRequest {JsonConvert.SerializeObject(projectConfigurationFileRequest)}");

      var projectTrn = TRNHelper.MakeTRN(projectUid);
      var projectConfigurationResponse = await PostData<ProjectConfigurationFileRequestModel, ProjectConfigurationModel>($"/projects/{projectTrn}/configuration/{projectConfigurationFileType.ToString().ToUpper()}", projectConfigurationFileRequest, null, customHeaders);

      log.LogDebug($"{nameof(SaveProjectConfiguration)}: projectConfigurationResponse {JsonConvert.SerializeObject(projectConfigurationResponse)}");
      return projectConfigurationResponse;
    }

    ///// <summary>
    ///// PUT https://trimble.com/connectedsiteprofilesettings/1.0/projects/{projectId}/configuration/{fileType}
    /////   user token
    /////   used by ProjectSvc v6 and v5TBC
    ///// </summary>
    public async Task<ProjectConfigurationModel> UpdateProjectConfiguration(Guid projectUid, ProjectConfigurationFileType projectConfigurationFileType, ProjectConfigurationFileRequestModel projectConfigurationFileRequest, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(UpdateProjectConfiguration)}: projectUid {projectUid} projectConfigurationFileType {projectConfigurationFileType} projectConfigurationFileRequest {JsonConvert.SerializeObject(projectConfigurationFileRequest)}");

      var projectTrn = TRNHelper.MakeTRN(projectUid);
      var projectConfigurationResponse =
        await UpdateData<ProjectConfigurationFileRequestModel, ProjectConfigurationModel>($"/projects/{projectTrn}/configuration/{projectConfigurationFileType.ToString().ToUpper()}", projectConfigurationFileRequest, null, customHeaders);

      log.LogDebug($"{nameof(UpdateProjectConfiguration)}: projectConfigurationResponse {JsonConvert.SerializeObject(projectConfigurationResponse)}");
      return projectConfigurationResponse;
    }


    /// <summary>
    /// Only 1 of each project calibrationtype is allowed, so this will delete that type
    /// </summary>
    public Task DeleteProjectConfiguration(Guid projectUid, ProjectConfigurationFileType projectConfigurationFileType, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(DeleteProjectConfiguration)}: projectUid {projectUid} projectConfigurationFileType {projectConfigurationFileType}");

      var projectTrn = TRNHelper.MakeTRN(projectUid);
      return DeleteData($"/projects/{projectTrn}/configuration/{projectConfigurationFileType.ToString().ToUpper()}", null, customHeaders);
    }
  }
}
