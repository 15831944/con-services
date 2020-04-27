﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Proxies.Interfaces;

namespace CCSS.CWS.Client
{
  /// <summary>
  /// These use the cws cws-profilemanager controller
  ///   See comments in CwsAccountClient re TRN/Guid conversions
  /// </summary>
  public class CwsProjectClient : CwsProfileManagerClient, ICwsProjectClient
  {
    public CwsProjectClient(IWebRequest gracefulClient, IConfigurationStore configuration, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(gracefulClient, configuration, logger, dataCache, serviceResolution)
    {
    }

    /// <summary>
    /// POST https://api.trimble.com/t/trimble.com/cws-profilemanager/1.0/projects
    ///   user token
    ///   todoMaaverick ProjectSvc v6 and v5TBC
    ///                 ProjectTRN
    ///   CCSSCON-141 available but needs updating to allow description. Note (26 March) that start/end dates will no longer be entered by UI.             
    /// </summary>
    public async Task<CreateProjectResponseModel> CreateProject(CreateProjectRequestModel createProjectRequest, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(CreateProject)}: createProjectRequest {JsonConvert.SerializeObject(createProjectRequest)}");

      createProjectRequest.AccountId = TRNHelper.MakeTRN(createProjectRequest.AccountId, TRNHelper.TRN_ACCOUNT);
      var createProjectResponseModel = await PostData<CreateProjectRequestModel, CreateProjectResponseModel>($"/projects", createProjectRequest, null, customHeaders);
      createProjectResponseModel.Id = TRNHelper.ExtractGuidAsString(createProjectResponseModel.Id);

      log.LogDebug($"{nameof(CreateProject)}: createProjectResponseModel {JsonConvert.SerializeObject(createProjectResponseModel)}");
      return createProjectResponseModel;
    }

    /// <summary>
    /// PUT https://api.trimble.com/t/trimble.com/cws-profilemanager/1.0/projects/{projectUid}
    ///   user token
    ///   todoMaaverick ProjectSvc v6 and v5TBC
    ///                 response code
    ///   CCSSCON-14 available but needs updating to allow description. Note (26 March) that start/end dates will no longer be entered by UI.             
    /// </summary>
    public async Task UpdateProjectDetails(Guid projectUid, UpdateProjectDetailsRequestModel updateProjectDetailsRequest, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(UpdateProjectDetails)}: projectUid {projectUid} updateProjectDetailsRequest {JsonConvert.SerializeObject(updateProjectDetailsRequest)}");

      var projectTrn = TRNHelper.MakeTRN(projectUid, TRNHelper.TRN_PROJECT);
      await UpdateData($"/projects/{projectTrn}", updateProjectDetailsRequest, null, customHeaders);
    }

    /// <summary>
    /// PUT https://api.trimble.com/t/trimble.com/cws-profilemanager/1.0/projects/{projectUid}/boundary
    ///   user token
    ///   todoMaaverick ProjectSvc v6 and v5TBC
    ///                 response code
    ///   CCSSCON-142 available but needs updating to include timeZone?                
    /// </summary>
    public async Task UpdateProjectBoundary(Guid projectUid, ProjectBoundary projectBoundary, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(UpdateProjectBoundary)}: projectUid {projectUid} projectBoundary {JsonConvert.SerializeObject(projectBoundary)}");

      var projectTrn = TRNHelper.MakeTRN(projectUid, TRNHelper.TRN_PROJECT);
      await UpdateData($"/projects/{projectTrn}/boundary", projectBoundary, null, customHeaders);
    }
  }
}
