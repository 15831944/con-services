﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.Http;
using VSS.Common.Exceptions;
using VSS.MasterData.Models;
using VSS.MasterData.Models.Converters;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Filter.Abstractions.Models;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.WebApi.Models.ProductionData.Contracts;
using VSS.Productivity3D.WebApi.Models.ProductionData.Executors;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.TRex.Gateway.Common.Abstractions;

namespace VSS.Productivity3D.WebApi.ProductionData.Controllers
{
  /// <summary>
  /// Controller for supplying CCA data tiles.
  /// </summary>
  /// 
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
  public class CCATileController : Controller, ICCATileContract
  {
#if RAPTOR
    /// <summary>
    /// Raptor client for use by executor
    /// </summary>
    private readonly IASNodeClient raptorClient;
#endif
    /// <summary>
    /// LoggerFactory for logging
    /// </summary>
    private readonly ILogger log;
    /// <summary>
    /// LoggerFactory factory for use by executor
    /// </summary>
    private readonly ILoggerFactory logger;

    /// <summary>
    /// Where to get environment variables, connection string etc. from
    /// </summary>
    private readonly IConfigurationStore ConfigStore;

    /// <summary>
    /// The TRex Gateway proxy for use by executor.
    /// </summary>
    private readonly ITRexCompactionDataProxy TRexCompactionDataProxy;

    /// <summary>
    /// Gets the custom headers for the request.
    /// </summary>
    protected IHeaderDictionary CustomHeaders => Request.Headers.GetCustomHeaders();

    private readonly IFileImportProxy FileImportProxy;

    private string UserId => User.Identity.Name;

    /// <summary>
    /// Constructor with dependency injection
    /// </summary>
    /// <param name="logger">LoggerFactory</param>
    /// <param name="configStore">Configuration Store</param>
    /// <param name="trexCompactionDataProxy">Trex Gateway production data proxy</param>
    public CCATileController(ILoggerFactory logger,
#if RAPTOR
      IASNodeClient raptorClient, 
#endif
      IConfigurationStore configStore, ITRexCompactionDataProxy trexCompactionDataProxy, IFileImportProxy fileImportProxy)
    {
      this.logger = logger;
      log = logger.CreateLogger<CCATileController>();
#if RAPTOR
      this.raptorClient = raptorClient;
#endif
      ConfigStore = configStore;
      TRexCompactionDataProxy = trexCompactionDataProxy;
      FileImportProxy = fileImportProxy;
    }

    /// <summary>
    /// Supplies tiles of rendered CCA data overlays.
    /// </summary>
    /// <param name="projectId">Raptor's data model/project identifier.</param>
    /// <param name="assetId">Raptor's machine identifier.</param>
    /// <param name="machineName">Raptor's machine name.</param>
    /// <param name="isJohnDoe">IsJohnDoe flag.</param>
    /// <param name="startUtc">Start date of the requeted CCA data in UTC.</param>
    /// <param name="endUtc">End date of the requested CCA data in UTC.</param>
    /// <param name="bbox">Bounding box, as a comma separated string, that represents a WGS84 latitude/longitude coordinate area.</param>
    /// <param name="width">Width of the requested CCA data tile.</param>
    /// <param name="height">Height of the requested CCA data tile.</param>
    /// <param name="liftId">Lift identifier of the requested CCA data.</param>
    /// <param name="assetUid">TRex's machine identifier.</param>
    /// <returns>An HTTP response containing an error code is there is a failure, or a PNG image if the request suceeds. If the size of a pixel in the rendered tile coveres more than 10.88 meters in width or height, then the pixel will be rendered in a 'representational style' where black (currently, but there is a work item to allow this to be configurable) is used to indicate the presense of data. Representational style rendering performs no filtering what so ever on the data.10.88 meters is 32 (number of cells across a subgrid) * 0.34 (default width in meters of a single cell)</returns>
    [ProjectVerifier]
    [Route("api/v1/ccatiles/png")]
    [HttpGet]
    public async Task<FileResult> Get
    ([FromQuery] long projectId,
      [FromQuery] long assetId,
      [FromQuery] string machineName,
      [FromQuery] bool isJohnDoe,
      [FromQuery] DateTime startUtc,
      [FromQuery] DateTime endUtc,
      [FromQuery] string bbox,
      [FromQuery] ushort width,
      [FromQuery] ushort height,
      [FromQuery] int? liftId = null,
      // no longer supported in WorksOS as no geofenceService [FromQuery] Guid? geofenceUid = null,
      [FromQuery] Guid? assetUid = null)
    {
      log.LogInformation("Get: " + Request.QueryString);

      var request = await CreateAndValidateRequest(projectId, null, assetId, machineName, isJohnDoe, startUtc, endUtc, bbox, width, height, liftId, assetUid);

      return await GetCCADataTile(request);
    }


    /// <summary>
    /// Supplies tiles of rendered CCA data overlays.
    /// </summary>
    /// <param name="projectUid">Raptor's data model/project unique identifier.</param>
    /// <param name="assetId">Raptor's machine identifier.</param>
    /// <param name="machineName">Raptor's machine name.</param>
    /// <param name="isJohnDoe">IsJohnDoe flag.</param>
    /// <param name="startUtc">Start date of the requeted CCA data in UTC.</param>
    /// <param name="endUtc">End date of the requested CCA data in UTC.</param>
    /// <param name="bbox">Bounding box, as a comma separated string, that represents a WGS84 latitude/longitude coordinate area.</param>    
    /// <param name="width">Width of the requested CCA data tile.</param>
    /// <param name="height">Height of the requested CCA data tile.</param>
    /// <param name="liftId">Lift identifier of the requested CCA data.</param>
    /// <param name="assetUid">TRex's machine identifier.</param>
    /// <returns>An HTTP response containing an error code is there is a failure, or a PNG image if the request suceeds. If the size of a pixel in the rendered tile coveres more than 10.88 meters in width or height, then the pixel will be rendered in a 'representational style' where black (currently, but there is a work item to allow this to be configurable) is used to indicate the presense of data. Representational style rendering performs no filtering what so ever on the data.10.88 meters is 32 (number of cells across a subgrid) * 0.34 (default width in meters of a single cell)</returns>
    [ProjectVerifier]
    [Route("api/v2/ccatiles/png")]
    [HttpGet]
    public async Task<FileResult> Get
    (
      [FromQuery] Guid projectUid,
      [FromQuery] long assetId,
      [FromQuery] string machineName,
      [FromQuery] bool isJohnDoe,
      [FromQuery] DateTime startUtc,
      [FromQuery] DateTime endUtc,
      [FromQuery] string bbox,
      [FromQuery] ushort width,
      [FromQuery] ushort height,
      [FromQuery] int? liftId = null,
      // no longer supported in WorksOS as no geofenceService [FromQuery] Guid? geofenceUid = null,
      [FromQuery] Guid? assetUid = null
    )
    {
      log.LogInformation("Get: " + Request.QueryString);
      long projectId = await ((RaptorPrincipal)User).GetLegacyProjectId(projectUid);
      var request = await CreateAndValidateRequest(projectId, projectUid, assetId, machineName, isJohnDoe, startUtc, endUtc, bbox, width, height, liftId, assetUid);

      return await GetCCADataTile(request);
    }

    /// <summary>
    /// Gets the requested CA data tile from Raptor.
    /// </summary>
    /// <param name="request">HTTP request.</param>
    /// <returns>An HTTP response containing an error code is there is a failure, or a PNG image if the request suceeds. 
    /// If the size of a pixel in the rendered tile coveres more than 10.88 meters in width or height, then the pixel will be rendered 
    /// in a 'representational style' where black (currently, but there is a work item to allow this to be configurable) is used 
    /// to indicate the presense of data. Representational style rendering performs no filtering what so ever on the data.10.88 meters is 32 
    /// (number of cells across a subgrid) * 0.34 (default width in meters of a single cell)</returns>
    private async Task<FileResult> GetCCADataTile(TileRequest request)
    {
      var tileResult = await RequestExecutorContainerFactory.Build<TilesExecutor>(logger,
#if RAPTOR
          raptorClient, 
#endif
          configStore: ConfigStore, trexCompactionDataProxy: TRexCompactionDataProxy, customHeaders: CustomHeaders,
          userId: UserId, fileImportProxy: FileImportProxy)
                         .ProcessAsync(request) as TileResult;

      if (tileResult?.TileData == null)
        tileResult = TileResult.EmptyTile(WebMercatorProjection.TILE_SIZE, WebMercatorProjection.TILE_SIZE);

      Response.Headers.Add("X-Warning", tileResult.TileOutsideProjectExtents.ToString());

      return new FileStreamResult(new MemoryStream(tileResult.TileData), ContentTypeConstants.ImagePng);
    }

    private async Task<TileRequest> CreateAndValidateRequest(
      long projectId,
      Guid? projectUid,
      long assetId,
      string machineName,
      bool isJohnDoe,
      DateTime startUtc,
      DateTime endUtc,
      string bbox,
      ushort width,
      ushort height,
      int? liftId,
      Guid? assetUid)
    {
      if (liftId == 0)
      {
        liftId = null;
      }

      var points = bbox.Split(',');

      if (points.Length != 4)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "BBOX parameter must contain 4 coordinates!"));
      }

      /*** no longer supported in WorksOS as no geofenceService
      List<WGSPoint> geometry = null;
      if (geofenceUid.HasValue)
      {
        var geometryWKT = await geofenceProxy.GetGeofenceBoundary(((RaptorPrincipal) User).CustomerUid, geofenceUid.ToString(), Request.Headers.GetCustomHeaders());

        if (string.IsNullOrEmpty(geometryWKT))
          throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
              "No Geofence geometry found."));

        geometry = CommonConverters.GeometryToPoints(geometryWKT).ToList();
      }
      ***/

      var filter = FilterResult.CreateFilterForCCATileRequest
      (
        startUtc,
        endUtc,
        new List<long> { assetId },
        null,
        liftId.HasValue ? FilterLayerMethod.TagfileLayerNumber : FilterLayerMethod.None,
        liftId,
        new List<MachineDetails> { new MachineDetails(assetId, machineName, isJohnDoe, assetUid) }
       );

      var request = new TileRequest
      (
        projectId,
        projectUid,
        null,
        DisplayMode.CCA,
        null,
        null,
        VolumesType.None,
        0,
        null,
        filter,
        0,
        null,
        0,
        FilterLayerMethod.TagfileLayerNumber,
        new BoundingBox2DLatLon
          (
            double.Parse(points[1]) * Coordinates.DEGREES_TO_RADIANS, // The Bottom Left corner, longitude...
            double.Parse(points[0]) * Coordinates.DEGREES_TO_RADIANS, // The Bottom Left corner, latitude...
            double.Parse(points[3]) * Coordinates.DEGREES_TO_RADIANS, // The Top Right corner, longitude..
            double.Parse(points[2]) * Coordinates.DEGREES_TO_RADIANS  // The Top Right corner, latitude...
          ),
        null,
        width,
        height
      );

      request.Validate();

      return request;
    }
  }
}
