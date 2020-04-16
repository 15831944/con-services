﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Utilities;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Controllers
{
  /// <summary>
  /// Project controller.
  /// </summary>
  public class ProjectV4Controller : BaseController
  {
    private readonly ILogger _log;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public ProjectV4Controller(ILoggerFactory logger, IConfigurationStore configStore,
      ICwsAccountClient cwsAccountClient, IProjectProxy projectProxy, IDeviceProxy deviceProxy)
      : base(logger, configStore, cwsAccountClient, projectProxy, deviceProxy)
    {
      _log = logger.CreateLogger<ProjectV4Controller>();
    }

    /// <summary>
    /// This endpoint is used by CTCTs Earthworks product.
    ///   It allows an operator, once or twice a day
    ///      to obtain data to enable it to generate a Cut/fill or other map from 3dpService. 
    ///   This step tries to identify a unique projectUid.
    /// 
    /// EC and/or radio, location and possibly TCCOrgID are provided.
    /// 
    /// Get the ProjectUid 
    ///     which belongs to the devices Customer and 
    ///     whose boundary the location is inside at the given date time. 
    ///     NOTE as of Sept 2019, VSS commercial model has not been determined,
    ///        current thinking is that:
    ///          1) if there is no traditional sub, they may get cutfill for surveyed surfaces only
    ///          2) if there is a traditional sub they get production data as well
    ///          3) there may be a completely new type of subscription, specific to EarthWorks cutfill ...
    /// </summary>
    /// <returns>
    /// The project Uid which satisfies spatial and time requirements
    ///      and possibly device
    ///      and an indicator of subscription availability
    ///      otherwise a returnCode.
    /// </returns>
    [Route("api/v4/project/getUidsEarthWorks")]
    [HttpPost]
    public async Task<GetProjectAndAssetUidsEarthWorksResult> GetProjectAndDeviceUidsEarthWorks([FromBody]GetProjectAndAssetUidsEarthWorksRequest request)
    {
      _log.LogDebug($"{nameof(GetProjectAndDeviceUidsEarthWorks)}: request: {JsonConvert.SerializeObject(request)}");
      request.Validate();
  
      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsEarthWorksExecutor>(_log, configStore, cwsAccountClient, projectProxy, deviceProxy);
      var result = await executor.ProcessAsync(request) as GetProjectAndAssetUidsEarthWorksResult;

      _log.LogResult(nameof(GetProjectAndDeviceUidsEarthWorks), request, result);
      return result;
    }

    /// <summary>
    /// This endpoint is used by TRex to identify a project to assign a tag file to.
    ///      It is called for each tag file, with as much information as is available e.g. device; location; projectUid
    ///      Attempts to identify a unique project which the tag file could be applied to, also to identify the device
    ///     On error returns:
    ///         If validation of request fails, returns BadRequest plus a unique error code and message
    ///         If it fails to identify/verify a project, returns BadRequest plus a unique error code and message
    ///         Note that it is not an error if it fails to identify the device as the caller will determine from context if this is an issue
    ///         If something internal has gone wrong, which may be retryable e.g. database unavailable
    ///            it returns InternalError plus a unique error code and message
    ///
    /// Workflows:  #1 Manual import (has a projectUid)
    ///             #2 TFHarvester Auto import             
    ///             #3 DirectSubmission from CTCT device (appears the same as #2)
    /// Note that for this endpoint we use Guids to identify projects etc
    /// </summary>
    /// <param name="request">Details of the project, device, also location and its date time</param>
    /// <returns>
    /// The project Uid which satisfies spatial, time requirements
    ///      and possibly deviceUid
    ///      otherwise errorCode etc:
    ///         Validation errors  HttpStatusCode.BadRequest:
    ///                     SerializationError (-2)
    ///                     "Latitude should be between -90 degrees and 90 degrees", 21);
    ///                     "Longitude should be between -180 degrees and 180 degrees", 22);
    ///                     "TimeOfPosition must have occurred between 50 years ago and the next 2 days", 23);
    ///                     "DeviceType is invalid", 30)
    ///                     "Manual Import: The Projects account cannot have not have a free device entitlement.", 31
    ///                     "ProjectUid is present, but invalid", 36);
    ///                     "Auto Import: Either Radio Serial or ec520 Serial must be provided", 37);
    ///                     "Unable to find the Project requested", 38);
    ///         Errors: HttpStatusCode.InternalServerError (retryable?)
    ///            "A problem occurred accessing database. Exception: {0}"             28 
    ///            "A problem occurred accessing a service. Service: {0} Exception: {1}", 17
    ///            
    ///         HttpStatusCode.OK   Response Code:
    ///            1 "Manual Import: The Projects account cannot have not have a free device entitlement."
    ///            41 "Manual Import: no intersecting projects found"
    ///            43 "Manual Import: cannot import to an archived project"
    ///            48 "Auto Import: for this radioSerial, no intersecting projects found"
    ///            49 "More than 1 project meets the location requirements"
    ///     
    /// </returns>
    [Route("api/v4/project/getUids")]
    [HttpPost]
    public async Task<GetProjectAndAssetUidsResult> GetProjectAndDeviceUids([FromBody]GetProjectAndAssetUidsRequest request)
    {
      _log.LogDebug($"{nameof(GetProjectAndDeviceUids)}: request:{JsonConvert.SerializeObject(request)}");
      request.Validate();

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsExecutor>(_log, configStore, cwsAccountClient, projectProxy, deviceProxy);
      var result = await executor.ProcessAsync(request) as GetProjectAndAssetUidsResult;

      _log.LogResult(nameof(GetProjectAndDeviceUids), request, result);
      return result;
    }

  }
}