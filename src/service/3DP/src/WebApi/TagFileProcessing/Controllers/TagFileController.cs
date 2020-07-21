﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Models.Utilities;
using VSS.MasterData.Proxies;
using VSS.Productivity3D.Common;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.WebApi.Compaction.Utilities;
using VSS.Productivity3D.WebApi.Models.TagfileProcessing.Executors;
using VSS.Productivity3D.WebApi.Models.TagfileProcessing.Models;
using VSS.Productivity3D.WebApi.Models.TagfileProcessing.ResultHandling;
using VSS.TRex.Gateway.Common.Abstractions;

namespace VSS.Productivity3D.WebApi.TagFileProcessing.Controllers
{
  /// <summary>
  /// For handling Tag file submissions from either TCC or machines equiped with direct submission capable units.
  /// </summary>
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
  public class TagFileController : Controller
  {
#if RAPTOR
    private readonly ITagProcessor _tagProcessor;
    private readonly IASNodeClient _raptorClient;
#endif
    private readonly ILogger _log;
    private readonly ILoggerFactory _logger;
    private readonly IConfigurationStore _configStore;
    private readonly ITRexTagFileProxy _tRexTagFileProxy;
    private IHeaderDictionary CustomHeaders => Request.Headers.GetCustomHeaders();

    /// <summary>
    /// Default constructor.
    /// </summary>
    public TagFileController(
#if RAPTOR
      IASNodeClient raptorClient, 
      ITagProcessor tagProcessor, 
#endif
      ILoggerFactory logger,ITRexTagFileProxy tRexTagFileProxy, IConfigurationStore configStore)
    {
#if RAPTOR
      _raptorClient = raptorClient;
      _tagProcessor = tagProcessor;
#endif
      _logger = logger;
      _log = logger.CreateLogger<TagFileController>();
      _tRexTagFileProxy = tRexTagFileProxy;
      _configStore = configStore;
    }

    /// <summary>
    /// For accepting and loading manually or automatically submitted tag files.
    /// </summary>
    /// <remarks>
    /// Manually submitted tag files include a project Id, the service performs a lookup for the boundary.
    /// </remarks>
    [PostRequestVerifier]
    [Route("api/v2/tagfiles")]
    [HttpPost]
    public async Task<IActionResult> PostTagFileNonDirectSubmission([FromBody]CompactionTagFileRequest request)
    {
      var serializedRequest = JsonUtilities.SerializeObjectIgnoringProperties(request, "Data");
      _log.LogDebug($"{nameof(PostTagFileNonDirectSubmission)}: request {serializedRequest}");

      //Check it's a 3dp project
      ProjectData projectData = null;

      if (request.ProjectId != null)
        projectData = await ((RaptorPrincipal)User).GetProject(request.ProjectId.Value);
      else if (request.ProjectUid != null)
        projectData = await ((RaptorPrincipal)User).GetProject(request.ProjectUid.Value);

      if (projectData != null && projectData.ProjectType.HasFlag(CwsProjectType.AcceptsTagFiles) == false)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "The project is standard and does not accept tag files."));
      }

      if (projectData?.IsArchived == true)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "The project has been archived and this function is not allowed."));
      }

      //Now submit tag file to Raptor and/or TRex
      Task<WGS84Fence> boundary = null;
      if (request.ProjectUid != null)
      {
        var projectTask = GetLegacyProjectId(request.ProjectUid);
        
        // the boundary parameter is only used by Raptor. This is handy validation for manual import anyway.
        boundary = GetProjectBoundary(request.ProjectUid.Value);

        await Task.WhenAll(projectTask, boundary);

        request.ProjectId = projectTask.Result;
      }

      var requestExt = CompactionTagFileRequestExtended.CreateCompactionTagFileRequestExtended(request, boundary?.Result);

      var responseObj = await RequestExecutorContainerFactory
        .Build<TagFileNonDirectSubmissionExecutor>(_logger,
#if RAPTOR
          _raptorClient, 
          _tagProcessor, 
#endif
          _configStore, tRexTagFileProxy:_tRexTagFileProxy, customHeaders: CustomHeaders)
        .ProcessAsync(requestExt);

      // when we disable Raptor, allowing Trex response to return to harvester,
      //  will need to rewrite the Trex result and handle these new codes in the Harvester.
      //  IMHO it would be nice to return the same response as for the DirectSubmission,
      //        which indicates whether a failure is permanent etc

      return responseObj.Code == 0
        ? (IActionResult)Ok(responseObj)
        : BadRequest(responseObj);
    }

    /// <summary>
    /// For the direct submission of tag files from GNSS capable machines.
    /// </summary>
    /// <remarks>
    /// Direct submission tag files don't include a project Id or boundary.
    /// </remarks>
    [PostRequestVerifier]
    [Route("api/v2/tagfiles/direct")]
    [HttpPost]
    public async Task<ObjectResult> PostTagFileDirectSubmission([FromBody] CompactionTagFileRequest request)
    {
      var serializedRequest = JsonUtilities.SerializeObjectIgnoringProperties(request, "Data");
      _log.LogDebug($"{nameof(PostTagFileDirectSubmission)}: request {serializedRequest}");

      // todoJeannie temporary to look into the device info available.
      _log.LogDebug($"{nameof(PostTagFileDirectSubmission)}: customHeaders {CustomHeaders.LogHeaders()}");

      var result = await RequestExecutorContainerFactory
        .Build<TagFileDirectSubmissionExecutor>(_logger,
#if RAPTOR
          _raptorClient, 
          _tagProcessor, 
#endif
          _configStore, tRexTagFileProxy:_tRexTagFileProxy, customHeaders: CustomHeaders)
        .ProcessAsync(request) as TagFileDirectSubmissionResult;

      if (result?.Code == 0)
        return StatusCode((int)HttpStatusCode.OK, result);
      return StatusCode((int)HttpStatusCode.BadRequest, result);
    }

    private async Task<long> GetLegacyProjectId(Guid? projectUid) => projectUid == null
      ? VelociraptorConstants.NO_PROJECT_ID
      : await ((RaptorPrincipal)User).GetLegacyProjectId(projectUid.Value);

    /// <summary>
    /// Gets the WGS84 project boundary geofence for a given project.
    /// </summary>
    private async Task<WGS84Fence> GetProjectBoundary(Guid projectUid)
    {
      var projectData = await ((RaptorPrincipal) User).GetProject(projectUid);
      var result = GeofenceValidation.ValidateWKT(projectData.ProjectGeofenceWKT);
      if (string.CompareOrdinal(result, GeofenceValidation.ValidationOk) != 0)
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            $"{nameof(GetProjectBoundary)}: The project has an invalid boundary ({result})."));

      return new WGS84Fence(CommonConverters.GeometryToPoints(projectData.ProjectGeofenceWKT).ToArray());
    }
  }
}
