﻿using ASNodeDecls;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Executors;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Filters.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Common.Utilities;
using VSS.Productivity3D.WebApi.Models.MapHandling;
using VSS.Productivity3D.WebApi.Models.Notification.Helpers;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApiModels.Compaction.Helpers;
using VSS.Productivity3D.WebApiModels.ProductionData.Contracts;
using Filter = VSS.Productivity3D.Common.Models.Filter;
using WGSPoint = VSS.Productivity3D.Common.Models.WGSPoint;

namespace VSS.Productivity3D.WebApi.ProductionData.Controllers
{
  /// <summary>
  /// Controller for supplying CCA data tiles.
  /// </summary>
  /// 
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
  public class CCATileController : Controller, ICCATileContract
  {
    /// <summary>
    /// Raptor client for use by executor
    /// </summary>
    private readonly IASNodeClient raptorClient;
    /// <summary>
    /// Logger for logging
    /// </summary>
    private readonly ILogger log;
    /// <summary>
    /// Logger factory for use by executor
    /// </summary>
    private readonly ILoggerFactory logger;
    /// <summary>
    /// Proxy for getting geofences from master data. Used to get boundary for Raptor using given geofenceUid.
    /// </summary>
    private readonly IGeofenceProxy geofenceProxy;

    /// <summary>
    /// Constructor with dependency injection
    /// </summary>
    /// <param name="geofenceProxy">Proxy client for getting geofences for boundaries</param>
    /// <param name="logger">Logger</param>
    /// <param name="raptorClient">Raptor client</param>
    public CCATileController(IGeofenceProxy geofenceProxy, ILoggerFactory logger, IASNodeClient raptorClient)
    {
      this.geofenceProxy = geofenceProxy;
      this.logger = logger;
      this.log = logger.CreateLogger<CCATileController>();
      this.raptorClient = raptorClient;
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
    /// <param name="geofenceUid">Geofence boundary unique identifier.</param>
    /// <returns>An HTTP response containing an error code is there is a failure, or a PNG image if the request suceeds. If the size of a pixel in the rendered tile coveres more than 10.88 meters in width or height, then the pixel will be rendered in a 'representational style' where black (currently, but there is a work item to allow this to be configurable) is used to indicate the presense of data. Representational style rendering performs no filtering what so ever on the data.10.88 meters is 32 (number of cells across a subgrid) * 0.34 (default width in meters of a single cell)</returns>
    [ProjectIdVerifier]
    [Route("api/v1/ccatiles/png")]
    [HttpGet]
    public async Task<FileResult> Get
    (
      [FromQuery] long projectId,
      [FromQuery] long assetId,
      [FromQuery] string machineName,
      [FromQuery] bool isJohnDoe,
      [FromQuery] DateTime startUtc,
      [FromQuery] DateTime endUtc,
      [FromQuery] string bbox,
      [FromQuery] ushort width,
      [FromQuery] ushort height,
      [FromQuery] int? liftId = null,
      [FromQuery] Guid? geofenceUid = null
    )
    {
      log.LogInformation("Get: " + Request.QueryString);

      var request = CreateAndValidateRequest(projectId, assetId, machineName, isJohnDoe, startUtc, endUtc, bbox, width, height, liftId, geofenceUid);

      return GetCCADataTile(request);
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
    /// <param name="geofenceUid">Geofence boundary unique identifier.</param>
    /// <returns>An HTTP response containing an error code is there is a failure, or a PNG image if the request suceeds. If the size of a pixel in the rendered tile coveres more than 10.88 meters in width or height, then the pixel will be rendered in a 'representational style' where black (currently, but there is a work item to allow this to be configurable) is used to indicate the presense of data. Representational style rendering performs no filtering what so ever on the data.10.88 meters is 32 (number of cells across a subgrid) * 0.34 (default width in meters of a single cell)</returns>
    [ProjectUidVerifier]
    [NotLandFillProjectWithUIDVerifier]
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
      [FromQuery] Guid? geofenceUid = null
    )
    {
      log.LogInformation("Get: " + Request.QueryString);
      long projectId = (User as RaptorPrincipal).GetProjectId(projectUid);
      var request = CreateAndValidateRequest(projectId, assetId, machineName, isJohnDoe, startUtc, endUtc, bbox, width, height, liftId, geofenceUid);

      return GetCCADataTile(request);
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
    private FileResult GetCCADataTile(TileRequest request)
    {
      var tileResult = RequestExecutorContainerFactory.Build<TilesExecutor>(logger, raptorClient).Process(request) as TileResult;

      if (tileResult == null)
      {
        //Return en empty tile
        using (Bitmap bitmap = new Bitmap(WebMercatorProjection.TILE_SIZE, WebMercatorProjection.TILE_SIZE))
        {
          tileResult = TileResult.CreateTileResult(bitmap.BitmapToByteArray(), TASNodeErrorStatus.asneOK);
        }
      }

      Response.Headers.Add("X-Warning", tileResult.TileOutsideProjectExtents.ToString());
      return new FileStreamResult(new MemoryStream(tileResult.TileData), "image/png");
    }
    private TileRequest CreateAndValidateRequest(
      long projectId,
      long assetId,
      string machineName,
      bool isJohnDoe,
      DateTime startUtc,
      DateTime endUtc,
      string bbox,
      ushort width,
      ushort height,
      int? liftId,
      Guid? geofenceUid)
    {
      if (liftId.HasValue)
        if (liftId.Value == 0)
          liftId = null;

      var points = bbox.Split(',');

      if (points.Length != 4)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "BBOX parameter must contain 4 coordinates!"));
      }

      List<WGSPoint> geometry = null;
      if (geofenceUid.HasValue)
      {
        //Todo this ahould be async
        var geometryWKT = geofenceProxy.GetGeofenceBoundary((User as RaptorPrincipal).CustomerUid,  geofenceUid.ToString(), RequestUtils.GetCustomHeaders(Request.Headers)).Result;

        if (string.IsNullOrEmpty(geometryWKT))
          throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
              "No Geofence geometry found."));

        geometry = RaptorConverters.geometryToPoints(geometryWKT).ToList();
      }

      var filter = Filter.CreateFilter
      (
        null,
        null,
        null,
        startUtc,
        endUtc,
        null,
        new List<long> { assetId },
        null,
        null,
        null,
        geometry,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        liftId.HasValue ? FilterLayerMethod.TagfileLayerNumber : FilterLayerMethod.None,
        null,
        null,
        liftId,
        null,
        new List<MachineDetails> { MachineLiftDetails.CreateMachineDetails(assetId, machineName, isJohnDoe) },
        null,
        null,
        null,
        null,
        null,
        null,
        null);

      var request = TileRequest.CreateTileRequest
      (
        projectId,
        null,
        DisplayMode.CCA,
        null,
        null,
        RaptorConverters.VolumesType.None,
        0,
        null,
        filter,
        0,
        null,
        0,
        FilterLayerMethod.TagfileLayerNumber,
        BoundingBox2DLatLon.CreateBoundingBox2DLatLon
          (
            double.Parse(points[1]) * ConversionConstants.DEGREES_TO_RADIANS, // The Bottom Left corner, longitude...
            double.Parse(points[0]) * ConversionConstants.DEGREES_TO_RADIANS, // The Bottom Left corner, latitude...
            double.Parse(points[3]) * ConversionConstants.DEGREES_TO_RADIANS, // The Top Right corner, longitude..
            double.Parse(points[2]) * ConversionConstants.DEGREES_TO_RADIANS  // The Top Right corner, latitude...
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