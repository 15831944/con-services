﻿using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;

namespace VSS.MasterData.Project.WebAPI.Controllers
{
  /// <summary>
  /// Project controller v5TBC
  /// This is used by BusinessCenter. 
  ///     The signature must be retained.
  ///     BC is now compatible with jwt/TID etc.   
  /// </summary>
  public class ProjectV5TBCController : ProjectBaseController
  {

    /// <summary>
    /// Gets or sets the Customer Repository.
    /// </summary>
    protected readonly ICustomerRepository customerRepo;

    /// <summary>
    /// Gets or sets the httpContextAccessor.
    /// </summary>
    protected readonly IHttpContextAccessor httpContextAccessor;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public ProjectV5TBCController(IConfigurationStore configStore, ICustomerRepository customerRepo, IHttpContextAccessor httpContextAccessor)
      : base(configStore)
    {
      this.customerRepo = customerRepo;
      this.httpContextAccessor = httpContextAccessor;
    }

    #region projects

    // POST: api/v5/projects
    /// <summary>
    /// TBC CreateProject. Footprint must remain the same as CGen: 
    ///     POST /t/trimble.com/vss-projectmonitoring/1.0/api/v5/projects HTTP/1.1
    ///     Body: {"CoordinateSystem":{"FileSpaceID":"u927f3be6-7987-4944-898f-42a088da94f2","Path":"/BC Data/Sites/Svevia Vargarda","Name":"Svevia Vargarda.dc","CreatedUTC":"0001-01-01T00:00:00Z"},"ProjectType":2,"StartDate":"2018-04-11T00:00:00Z","EndDate":"2018-05-11T00:00:00Z","ProjectName":"Svevia Vargarda","TimeZoneName":"Romance Standard Time","BoundaryLL":[{"Latitude":58.021890362243404,"Longitude":12.778613775843427},{"Latitude":58.033751276149488,"Longitude":12.783760539866186},{"Latitude":58.035972399195963,"Longitude":12.812762795456051},{"Latitude":58.032604039701752,"Longitude":12.841590546413993},{"Latitude":58.024515931878035,"Longitude":12.842137844178708},{"Latitude":58.016620613589389,"Longitude":12.831491715508857},{"Latitude":58.0128142214101,"Longitude":12.793567555971942},{"Latitude":58.021890362243404,"Longitude":12.778613775843427}],"CustomerUID":"323e4a34-56aa-11e5-a400-0050569757e0","CustomerName":"MERINO CONSTRUCTION"}
    ///     Result: HttpStatusCode.Created
    ///            {"id":6964} 
    /// 
    ///   This US only handles happy path. ServiceExceptions will be mapped in a future US.
    /// 
    /// </summary>
    /// <param name="projectRequest">CreateProjectV2Request model</param>
    /// <remarks>Updates existing project</remarks>
    [Route("api/v5/projects")]
    [HttpPost]
    public async Task<ReturnLongV5Result> CreateProjectV2([FromBody] CreateProjectV5Request projectRequest)
    {
      if (projectRequest == null)
      {
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 81);
      }

      Logger.LogInformation($"CreateProjectV2. projectRequest: {JsonConvert.SerializeObject(projectRequest)}");

      var createProjectEvent = MapV2Models.MapCreateProjectV2RequestToEvent(projectRequest, customerUid);

      ProjectDataValidator.Validate(createProjectEvent, ProjectRepo, ServiceExceptionHandler);

      projectRequest.CoordinateSystem =
        ProjectDataValidator.ValidateBusinessCentreFile(projectRequest.CoordinateSystem);

      // Read CoordSystem file from TCC as byte[]. 
      //    Filename and content are used: 
      //      validated via productivity3dProxy
      //      created in Raptor via productivity3dProxy
      //      stored in CreateKafkaEvent
      //    Only Filename is stored in the VL database 
      createProjectEvent.CoordinateSystemFileContent =
        await TccHelper
        .GetFileContentFromTcc(projectRequest.CoordinateSystem,
          Logger, ServiceExceptionHandler, FileRepo).ConfigureAwait(false);

      await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainerFactory
          .Build<CreateProjectExecutor>(LoggerFactory, ConfigStore, ServiceExceptionHandler,
            customerUid, userId, null, customHeaders, 
            Productivity3dV1ProxyCoord, projectRepo: ProjectRepo,
            fileRepo: FileRepo, dataOceanClient: DataOceanClient, authn: Authorization,
            projectCwsClient: CwsProjectClient)
          .ProcessAsync(createProjectEvent)
      );

      Logger.LogDebug("CreateProjectV2. completed successfully");
      return ReturnLongV5Result.CreateLongV5Result(HttpStatusCode.Created, createProjectEvent.ShortRaptorProjectId);
    }

    #endregion projects


    #region TCCAuthorization

    // POST: api/v5/preferences/tcc
    /// <summary>
    /// TBC ValidateTCCAuthorization. This validates that 
    ///      a) the customer has access to the TCC organization and 
    ///      b) that the Folder structure exists in TCC.
    /// Footprint must remain the same as CGen: 
    ///     POST /t/trimble.com/vss-projectmonitoring/1.0/api/v5/preferences/tcc HTTP/1.1
    ///     Body: {"organization":"vssnz19"}     
    ///     Response: {"success":true}
    /// 
    /// Happy path only to be handled in this US. ServiceExceptions will be mapped in a future US.
    /// However this is a faillure Response:
    ///     {"status":500,"message":"invalidUser001\r\n\r\n","errorcode":1000,"link":null}
    /// 
    /// </summary>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad request</response>
    [Route("api/v5/preferences/tcc")]
    [HttpPost]
    public async Task<ReturnSuccessV5Result> ValidateTccAuthorization(
      [FromBody] ValidateTccAuthorizationRequest tccAuthorizationRequest)
    {
      if (tccAuthorizationRequest == null)
      {
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 86);
      }
      // todoMaverick do we not need ValidateTccOrgExecutor and Customer/CustomerTCCOrg tables anymore?

      //Note: This is a very old legacy code that validates subs against TCC. This is not needed anymore as we allow project creation regardless of TCC subscription to support Earthworks machines.

      /* Logger.LogInformation(
       $"ValidateTCCAuthorization. tccAuthorizationRequest: {JsonConvert.SerializeObject(tccAuthorizationRequest)}");

     tccAuthorizationRequest.Validate();

     await WithServiceExceptionTryExecuteAsync(() =>
       RequestExecutorContainerFactory
         .Build<ValidateTccOrgExecutor>(Logger, ConfigStore, ServiceExceptionHandler,
           customerUid, null, null, customHeaders,
           null, null,
           null, null, null,
           null, FileRepo, customerRepo)
         .ProcessAsync(tccAuthorizationRequest)
     );*/

      Logger.LogInformation("ValidateTccAuthorization. completed succesfully");
      return ReturnSuccessV5Result.CreateReturnSuccessV5Result(HttpStatusCode.OK, true);
    }

    #endregion TCCAuthorization
  }
}
