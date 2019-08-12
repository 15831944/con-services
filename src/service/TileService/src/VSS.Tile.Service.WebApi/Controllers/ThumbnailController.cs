﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.Http;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Tile.Service.Common.Authentication;
using VSS.Tile.Service.Common.Extensions;
using VSS.Tile.Service.Common.Helpers;
using VSS.Tile.Service.Common.Interfaces;
using VSS.Tile.Service.Common.Models;
using VSS.Tile.Service.Common.ResultHandling;
using VSS.Tile.Service.Common.Services;
using VSS.WebApi.Common;

namespace VSS.Tile.Service.WebApi.Controllers
{
  public class ThumbnailController : BaseController<ThumbnailController>
  {
    private readonly TileOverlayType[] DEFAULT_PROJECT_THUMBNAIL_OVERLAYS =
    {
      TileOverlayType.BaseMap,
      TileOverlayType.ProjectBoundary
    };

    private readonly TileOverlayType[] DEFAULT_GEOFENCE_THUMBNAIL_OVERLAYS =
    {
      TileOverlayType.BaseMap,
      TileOverlayType.GeofenceBoundary
    };

    private const int DEFAULT_THUMBNAIL_WIDTH = 220;
    private const int DEFAULT_THUMBNAIL_HEIGHT = 182;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public ThumbnailController(IProductivity3dProxy productivity3DProxy, IPreferenceProxy prefProxy, IFileImportProxy fileImportProxy, 
      IMapTileGenerator tileGenerator, IGeofenceProxy geofenceProxy, IMemoryCache cache, IConfigurationStore configStore, 
      IBoundingBoxHelper boundingBoxHelper, ITPaaSApplicationAuthentication authn)
      : base(productivity3DProxy, prefProxy, fileImportProxy, tileGenerator, geofenceProxy, cache, configStore, boundingBoxHelper, authn)
    {
    }

    /// <summary>
    /// Gets a project thumbnail image as a raw png.
    /// </summary>
    [ProjectUidVerifier]
    [Route("api/v1/projectthumbnail/png")]
    [HttpGet]
    public async Task<FileResult> GetProjectThumbnailPng(
      [FromQuery] Guid projectUid, 
      [FromQuery] TileOverlayType[] additionalOverlays)
    {
      Log.LogDebug($"{nameof(GetProjectThumbnailPng)}: {Request.QueryString}");

      var project = await ((TilePrincipal) User).GetProject(projectUid);
      var bbox = GetBoundingBoxFromWKT(project.ProjectGeofenceWKT);

      DisplayMode? mode = null;
      var overlays = DEFAULT_PROJECT_THUMBNAIL_OVERLAYS.ToList();
      if (additionalOverlays?.Length > 0)
      {
        overlays.AddRange(additionalOverlays);
        if (additionalOverlays.Contains(TileOverlayType.ProductionData))
          mode = DisplayMode.Height;
      }

      var tileResult = await GetGeneratedTile(projectUid, null, null, null, null,
        null, overlays.ToArray(), DEFAULT_THUMBNAIL_WIDTH, DEFAULT_THUMBNAIL_HEIGHT, bbox, MapType.MAP,
        mode, null, true);

      // TODO (Aaron) refactor this repeated code
      //Short-circuit cache time for Archived projects
      if (project.IsArchived)
        Response.Headers["Cache-Control"] = "public,max-age=31536000";
      Response.Headers.Add("X-Warning", "false");

      return tileResult;
    }

    /// <summary>
    /// Gets a project thumbnail image as a Base64 encoded string.
    /// </summary>
    [ProjectUidVerifier]
    [Route("api/v1/projectthumbnail/base64")]
    [HttpGet]
    public async Task<byte[]> GetProjectThumbnailBase64(
      [FromQuery] Guid projectUid,
      [FromQuery] TileOverlayType[] additionalOverlays)
    {
      Log.LogDebug($"{nameof(GetProjectThumbnailBase64)}: {Request.QueryString}");
      var result = await GetProjectThumbnailPng(projectUid, additionalOverlays);
      return GetStreamContents(result);
    }

    /// <summary>
    /// Gets a 3D project thumbnail image as a raw png.
    /// </summary>
    [ProjectUidVerifier]
    [Route("api/v1/projectthumbnail3d/png")]
    [HttpGet]
    public async Task<FileResult> GetProjectThumbnail3DPng(
      [FromQuery] Guid projectUid)
    {
      Log.LogDebug($"{nameof(GetProjectThumbnail3DPng)}: {Request.QueryString}");

      return await GetProjectThumbnailPng(projectUid, new [] {TileOverlayType.ProductionData});
    }

    /// <summary>
    /// Gets a 3D project thumbnail image as a Base64 encoded string.
    /// </summary>
    [ProjectUidVerifier]
    [Route("api/v1/projectthumbnail3d/base64")]
    [HttpGet]
    public async Task<byte[]> GetProjectThumbnail3DBase64(
      [FromQuery] Guid projectUid)
    {
      Log.LogDebug($"{nameof(GetProjectThumbnail3DBase64)}: {Request.QueryString}");

      var result = await GetProjectThumbnailPng(projectUid, new[] { TileOverlayType.ProductionData });
      return GetStreamContents(result);
    }

    /// <summary>
    /// Gets a 2D (unified productivity) project thumbnail image as a raw png.
    /// </summary>
    [ProjectUidVerifier]
    [Route("api/v1/projectthumbnail2d/png")]
    [HttpGet]
    public async Task<FileResult> GetProjectThumbnail2DPng(
      [FromQuery] Guid projectUid)
    {
      Log.LogDebug($"{nameof(GetProjectThumbnail2DPng)}: {Request.QueryString}");

      return await GetProjectThumbnailPng(projectUid, new[] { TileOverlayType.LoadDumpData });
    }

    /// <summary>
    /// Gets a 2D (unified productivity) project thumbnail image as a Base64 encoded string.
    /// </summary>
    [ProjectUidVerifier]
    [Route("api/v1/projectthumbnail2d/base64")]
    [HttpGet]
    public async Task<byte[]> GetProjectThumbnail2DBase64(
      [FromQuery] Guid projectUid)
    {
      Log.LogDebug($"{nameof(GetProjectThumbnail2DBase64)}: {Request.QueryString}");

      var result = await GetProjectThumbnailPng(projectUid, new[] { TileOverlayType.LoadDumpData });
      return GetStreamContents(result);
    }

    /// <summary>
    /// Gets a geofence thumbnail image as a raw png.
    /// </summary>
    [Route("api/v1/geofencethumbnail/png")]
    [HttpGet]
    public async Task<FileResult> GetGeofenceThumbnailPng(
      [FromQuery] Guid geofenceUid)
    {
      Log.LogDebug($"{nameof(GetGeofenceThumbnailPng)}: {Request.QueryString}");
      var tileResult = await GeofenceThumbnailPng(
        (await geofenceProxy.GetGeofences(GetCustomerUid, CustomHeaders)).FirstOrDefault(gfc =>
          gfc.GeofenceUID == geofenceUid));

      return tileResult.Item2;
    }

    /// <summary>
    /// Gets a geofence thumbnail image as a Base64 encoded string.
    /// </summary>
    [Route("api/v1/geofencethumbnail/base64")]
    [HttpGet]
    public async Task<byte[]> GetGeofenceThumbnailBase64(
      [FromQuery] Guid geofenceUid)
    {
      Log.LogDebug($"{nameof(GetGeofenceThumbnailBase64)}: {Request.QueryString}");

      var result = await GetGeofenceThumbnailPng(geofenceUid);
      return GetStreamContents(result);
    }

    /// <summary>
    /// Gets a list of geofence thumbnail images as Base64 encoded strings.
    /// </summary>
    [Route("api/v1/geofencethumbnails/base64")]
    [HttpGet]
    public async Task<MultipleThumbnailsResult> GetGeofenceThumbnailsBase64([FromQuery] Guid[] geofenceUids)
    {
      Log.LogDebug($"{nameof(GetGeofenceThumbnailsBase64)}: {Request.QueryString}");
      List<GeofenceData> geofences;

      geofences = await geofenceProxy.GetGeofences(GetCustomerUid, CustomHeaders);
      if (geofenceUids.Any(g => !geofences.Select(k => k.GeofenceUID).Contains(g)))
      {
        geofenceProxy.ClearCacheItem(GetCustomerUid);
        geofences = await geofenceProxy.GetGeofences(GetCustomerUid, CustomHeaders);
      }


      var selectedGeofences = geofenceUids != null && geofenceUids.Length > 0
        ? geofences.Where(g => geofenceUids.Contains(g.GeofenceUID))
        : geofences;

      var thumbnails = await Task.WhenAll(selectedGeofences.Select(GeofenceThumbnailPng));

      return new MultipleThumbnailsResult
      {
        Thumbnails = thumbnails.Select(thumb => new ThumbnailResult
          {Uid = thumb.Item1, Data = GetStreamContents(thumb.Item2)}).ToList()
      };
    }

    /// <summary>
    /// Gets a list of geofence thumbnail images as a Base64 encoded string using the provided geometry
    /// </summary>
    [Route("api/v1/geofencethumbnailsraw/base64")]
    [HttpPost]
    public async Task<MultipleThumbnailsResult> GetGeofenceThumbnailsRawBase64(
      [FromBody] RootObject geoJson)
    {
      Log.LogDebug($"{nameof(GetGeofenceThumbnailsRawBase64)}: {Request.QueryString}");

      if (geoJson == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Missing geoJson for geofence thumbnail"));
      }

      //GeoJson can contain multiple boundaries. Limit to 10.
      const int MAX_GEOFENCES = 10;
      var polygons = geoJson.GeoJsonToPoints().ToList();
      if (polygons.Count == 0)
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "No boundaries found for geofence thumbnails"));
      if (polygons.Count > MAX_GEOFENCES)
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            $"A maximum of {MAX_GEOFENCES} boundaries allowed for geofence thumbnails"));

      var thumbnails = await Task.WhenAll(polygons.Select(GeofenceThumbnailPng));

      return new MultipleThumbnailsResult
      {
        Thumbnails = thumbnails.Select(thumb => new ThumbnailResult
          { Uid = Guid.Empty, Data = GetStreamContents(thumb) }).ToList()
      };
    }

    #region privates
    /// <summary>
    /// Multithreaded method to retrieve Geofence PNG
    /// </summary>
    /// <param name="geofence"></param>
    /// <returns></returns>
    private async Task<(Guid, FileResult)> GeofenceThumbnailPng(GeofenceData geofence)
    {
      //This appear to be a non-caching request
      //We don't need to do it twice as the very first caching request should be enough
      //var geofence = await geofenceProxy.GetGeofenceForCustomer(GetCustomerUid, geofenceUid.ToString(), CustomHeaders);

      if (geofence == null)
      {
        return (Guid.Empty, new FileStreamResult(
          new MemoryStream(TileServiceUtils.EmptyTile(DEFAULT_THUMBNAIL_WIDTH, DEFAULT_THUMBNAIL_HEIGHT)), ContentTypeConstants.ImagePng));
      }

      Log.LogDebug($"Generating geofence tile for {geofence.GeofenceUID}");

      var bbox = GetBoundingBoxFromWKT(geofence.GeometryWKT);

      var tileResult = await GetGeneratedTile(geofence, DEFAULT_GEOFENCE_THUMBNAIL_OVERLAYS, DEFAULT_THUMBNAIL_WIDTH,
        DEFAULT_THUMBNAIL_HEIGHT, bbox, MapType.MAP, null, true);
      return (geofence.GeofenceUID, tileResult);
    }

    /// <summary>
    /// Multithreaded method to retrieve Geofence PNG
    /// </summary>
    /// <param name="boundary"></param>
    /// <returns></returns>
    private Task<FileResult> GeofenceThumbnailPng(IEnumerable<WGSPoint> boundary)
    {
      var points = boundary.ToList();
      var bbox = GetBoundingBox(points);

      var tileResult = GetGeneratedTile(points, DEFAULT_GEOFENCE_THUMBNAIL_OVERLAYS, DEFAULT_THUMBNAIL_WIDTH,
        DEFAULT_THUMBNAIL_HEIGHT, bbox, MapType.MAP, null, true);

      return tileResult;
    }

    /// <summary>
    /// Gets the bounding box of the WKT
    /// </summary>
    private string GetBoundingBoxFromWKT(string wkt)
    {
      var points = wkt.GeometryToPoints();
      return GetBoundingBox(points.ToList());
    }

    /// <summary>
    /// Gets the bounding box of the points
    /// </summary>
    private string GetBoundingBox(List<WGSPoint> points)
    {
      var minLat = points.Min(p => p.Lat).LatRadiansToDegrees();
      var minLng = points.Min(p => p.Lon).LonRadiansToDegrees();
      var maxLat = points.Max(p => p.Lat).LatRadiansToDegrees();
      var maxLng = points.Max(p => p.Lon).LonRadiansToDegrees();
      return $"{minLat},{minLng},{maxLat},{maxLng}";
    }

    /// <summary>
    /// Convert the raw PNG into an array of bytes
    /// </summary>
    private byte[] GetStreamContents(FileResult result)
    {
      using (MemoryStream ms = new MemoryStream())
      {
        (result as FileStreamResult).FileStream.CopyTo(ms);
        return ms.ToArray();
      }
    }
    #endregion
  }
}
