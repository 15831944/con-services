﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.Http;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Filter.Abstractions.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.WebApi.Compaction.Controllers;
using VSS.Productivity3D.WebApi.Models.ProductionData.Contracts;
using VSS.Productivity3D.WebApi.Models.ProductionData.Executors;

namespace VSS.Productivity3D.WebApi.ProductionData.Controllers
{
  /// <summary>
  /// TerrainController responsible for all quantized mesh tile requests
  /// </summary>
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
  public class TerrainController : BaseController<TerrainController>, ITerrainContract
  {
    private const string layer = "{  \"tilejson\": \"2.1.0\",  \"name\": \"VSS\",  \"description\": \"\",  \"version\": \"1.1.0\",  \"format\": \"quantized-mesh-1.0\",  \"attribution\": \"\",  \"schema\": \"tms\",  \"tiles\": [ \"{z}/{x}/{y}.terrain?v={version}\" ],  \"projection\": \"EPSG:4326\",  \"bounds\": [ 0.00, -90.00, 180.00, 90.00 ],  \"extensions\": [\"octvertexnormals\"], \"available\": [    [ { \"startX\": 0, \"startY\": 0, \"endX\": 1, \"endY\": 0 } ]   ,[ { \"startX\": 0, \"startY\": 0, \"endX\": 3, \"endY\": 1 } ]   ,[ { \"startX\": 0, \"startY\": 0, \"endX\": 7, \"endY\": 3 } ]   ,[ { \"startX\": 0, \"startY\": 0, \"endX\": 15, \"endY\": 7 } ]   ,[ { \"startX\": 0, \"startY\": 0, \"endX\": 31, \"endY\": 15 } ]   ,[ { \"startX\": 0, \"startY\": 0, \"endX\": 63, \"endY\": 31 } ]   ,[ { \"startX\": 0, \"startY\": 0, \"endX\": 127, \"endY\": 63 } ]   ,[ { \"startX\": 0, \"startY\": 0, \"endX\": 255, \"endY\": 127 } ]   ,[ { \"startX\": 0, \"startY\": 0, \"endX\": 511, \"endY\": 255 } ]   ,[ { \"startX\": 0, \"startY\": 0, \"endX\": 1023, \"endY\": 511 } ]   ,[ { \"startX\": 0, \"startY\": 0, \"endX\": 2047, \"endY\": 1023 } ]   ,[ { \"startX\": 0, \"startY\": 0, \"endX\": 4095, \"endY\": 2047 } ]   ,[ { \"startX\": 0, \"startY\": 0, \"endX\": 8191, \"endY\": 4095 } ]   ,[ { \"startX\": 0, \"startY\": 0, \"endX\": 16383, \"endY\": 8191 } ]   ,[ { \"startX\": 0, \"startY\": 0, \"endX\": 32767, \"endY\": 16383 } ]   ,[ { \"startX\": 0, \"startY\": 0, \"endX\": 65535, \"endY\": 32767 } ]   ,[ { \"startX\": 0, \"startY\": 0, \"endX\": 131071, \"endY\": 65535 } ]   ,[ { \"startX\": 0, \"startY\": 0, \"endX\": 262143, \"endY\": 131071 } ]   ,[ { \"startX\": 0, \"startY\": 0, \"endX\": 524287, \"endY\": 262143 } ]   ,[ { \"startX\": 0, \"startY\": 0, \"endX\": 1048575, \"endY\": 54287 } ]   ,[ { \"startX\": 0, \"startY\": 0, \"endX\": 2097151, \"endY\": 1048575 } ]   ,[ { \"startX\": 0, \"startY\": 0, \"endX\": 4194303, \"endY\": 2097151 } ]   ,[ { \"startX\": 0, \"startY\": 0, \"endX\": 8388607, \"endY\": 4194303 } ]   ,[ { \"startX\": 0, \"startY\": 0, \"endX\": 16777215, \"endY\": 8388607 } ]  ]}";

    /// <summary>
    /// Constructor with injection
    /// </summary>
    public TerrainController(ILoggerFactory logger, IConfigurationStore configStore, ICompactionSettingsManager settingsManager, IFileImportProxy fileImportProxy) : base (configStore,fileImportProxy,settingsManager)
    { }

    /// <summary>
    /// Async call to make quantized mesh tile
    /// </summary>
    /// <param name="projectUid"> project id</param>
    /// <param name="filterUid">filter id</param>
    /// <param name="displayMode">DisplayMode</param>
    /// <param name="x">tile x coordinate</param>
    /// <param name="y">tile y coordinate</param>
    /// <param name="z">tile z coordinate</param>
    /// <param name="hasLighting">Does tile have lighting</param>
    /// <returns></returns>
    private async Task<byte[]> FetchTile(Guid projectUid, Guid filterUid, int displayMode, int x, int y, int z, bool hasLighting)
    {
      FilterResult filter;
      try
      {
        filter = await GetCompactionFilter(projectUid, filterUid);
        // Note! When debugging locally with your own data you may want to skip the above line and make a empty filter to avoid lookup validation failures 
        // filter = new FilterResult();
      }
      catch (Exception e)
      { var msg = $"TerrainController.FetchTile. Call to GetCompactionFilter has failed. ProjectUid:{projectUid}, FilterUid{filterUid}. Error:{e.Message}";
        Log.LogError(e, msg);
        throw;
      }

      var request = new QMTileRequest()
      {
        ProjectUid = projectUid,
        Filter = filter,
        DisplayMode = displayMode,
        X = x,
        Y = y,
        Z = z,
        HasLighting = hasLighting
      };

      request.Validate();

      try
      {
        var qmTileResult = await RequestExecutorContainerFactory.Build<QMTilesExecutor>(LoggerFactory,
          configStore: ConfigStore, trexCompactionDataProxy: TRexCompactionDataProxy, customHeaders: CustomHeaders,
          userId: GetUserId(), fileImportProxy: FileImportProxy).ProcessAsync(request) as QMTileResult;
        return (qmTileResult == null) ? null : qmTileResult.TileData;
      }
      catch (Exception e)
      {
        var msg = $"TerrainController.FetchTile. Call to TRex gateway for QMTile has failed. ProjectUid:{projectUid}, FilterUid{filterUid}. Error:{e.Message}";
        Log.LogError(e, msg);
        throw;
      }

    }

    /// <summary>
    /// Request for a quantized mesh tile
    /// </summary>
    /// <param name="x">x tile coordinate</param>
    /// <param name="y">y tile coordinate</param>
    /// <param name="z">z tile coordinate</param>
    /// <param name="formatExtension">terrain ext</param>
    /// <param name="projectUid">Project UId</param>
    /// <param name="filterUId">Filter Id</param>
    /// <param name="displayMode">DisplayMode</param>/// 
    /// <param name="hasLighting">Does tile have lighting</param>
    /// <returns></returns>
    [HttpGet("api/v2/qmesh/{z}/{x}/{y}.{formatExtension}")]
    public async Task<IActionResult> Get(int x, int y, int z, string formatExtension, [FromQuery] Guid projectUid, [FromQuery] Guid filterUId, [FromQuery] int displayMode, [FromQuery] bool hasLighting = false)
    {

      Log.LogInformation("Get: " + Request.QueryString);
      var qmTile = await FetchTile(projectUid, filterUId, displayMode, x, y, z, hasLighting);

      if (qmTile != null)
      {
        HttpContext.Response.Headers.Add(ContentTypeConstants.ContentEncoding, ContentTypeConstants.ContentEncodingGzip); 
        HttpContext.Response.Headers.Add(ContentTypeConstants.ContentLength, qmTile.Length.ToString());
        HttpContext.Response.Headers.Add(ContentTypeConstants.ContentType, ContentTypeConstants.ApplicationOctetStream);
        HttpContext.Response.Headers.Add(ContentTypeConstants.ContentDisposition, $"attachment;filename={y}.terrain");
        return File(qmTile, ContentTypeConstants.ApplicationOctetStream);
      }

      Log.LogDebug($"Requested tile x:{x},y: {y},z:{z} for Project:{projectUid} was not found");
      return NotFound();
    }

    /// <summary>
    /// Returns layer.json that controls the layout of all future tile requets
    /// </summary>
    /// <returns></returns>
    [HttpGet("api/v2/qmesh/layer.json")]
    public string GetTRexLayerFile()
    {
      return layer; 
    }
  }
}
