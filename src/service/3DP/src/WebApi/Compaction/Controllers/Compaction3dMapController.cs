﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.Http;
#if RAPTOR
using VLPDDecls;
#endif
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Converters;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Filter.Abstractions.Interfaces;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Models.Coords;
using VSS.Productivity3D.Models.Models.Designs;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.Models.ResultHandling.Coords;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.WebApi.Compaction.ActionServices;
using VSS.Productivity3D.WebApi.Compaction.Controllers.Filters;
using VSS.Productivity3D.WebApi.Models.Compaction.Executors;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Productivity3D.WebApi.Models.Factories.ProductionData;
using VSS.Productivity3D.WebApi.Models.Interfaces;
using VSS.Productivity3D.WebApi.Models.Report.Models;
using VSS.TCCFileAccess;
using VSS.TRex.Designs.TTM.Optimised;
using VSS.TRex.Designs.TTM.Optimised.Exceptions;
using VSS.TRex.Gateway.Common.Abstractions;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// A controller for getting 3d map tiles
  /// </summary>
  [ResponseCache(Duration = 900, VaryByQueryKeys = new[] { "*" })]
  [ProjectVerifier]
  public class Compaction3DMapController : BaseController<Compaction3DMapController>
  {
    /// <summary>
    /// Map Display Type for the 3d Maps control
    /// </summary>
    public enum MapDisplayType
    {
      /// <summary>
      /// Height Map image
      /// </summary>
      HeightMap = 0,
      /// <summary>
      /// Height Map representing the design
      /// </summary>
      DesignMap = 1,
      /// <summary>
      /// The texture to be displayed
      /// </summary>
      Texture = 2
    }

    /// <summary>
    /// Class to hold a face, used for the model generated
    /// TODO: Move this to it's own file, even own library if we need obj files after PoC
    /// </summary>
    private class Face
    {
      public int VertexIdx0 { get; set; }
      public int VertexIdx1 { get; set; }
      public int VertexIdx2 { get; set; }

      public int UvIdx0 { get; set; }
      public int UvIdx1 { get; set; }
      public int UvIdx2 { get; set; }
    }

    /// <summary>
    /// Class to hold a UV, used for the texture mapping on the model
    /// TODO: Move this to it's own file, even own library if we need obj files after PoC
    /// </summary>
    public class Uv
    {
      public double U { get; set; }
      public double V { get; set; }
    }

    private readonly IProductionDataTileService tileService;
    private readonly IBoundingBoxHelper boundingBoxHelper;
    private readonly ITRexCompactionDataProxy trexCompactionDataProxy;

    /// <summary>
    /// Default Constructor
    /// </summary>
    public Compaction3DMapController(ILoggerFactory loggerFactory,
      IServiceExceptionHandler serviceExceptionHandler,
      IConfigurationStore configStore,
      IFileImportProxy fileImportProxy,
      IProjectSettingsProxy projectSettingsProxy,
      IFilterServiceProxy filterServiceProxy,
      ICompactionSettingsManager settingsManager,
      IProductionDataTileService tileService,
#if RAPTOR
      IASNodeClient raptorClient,
#endif
      IBoundingBoxHelper boundingBoxHelper,
      ITRexCompactionDataProxy trexCompactionDataProxy) : base(configStore, fileImportProxy, settingsManager)
    {
      this.tileService = tileService;
      this.boundingBoxHelper = boundingBoxHelper;
      this.trexCompactionDataProxy = trexCompactionDataProxy;
    }

    /// <summary>
    /// Generates a image for use in the 3d map control
    /// These can be heightmaps / textures / designs
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="filterUid">Optional Filter UID</param>
    /// <param name="designUid">The Design File UID if showing the Design (ignored otherwise)</param>
    /// <param name="cutfillDesignUid">Cut fill design UID for the Texture, ignored for other modes</param>
    /// <param name="type">Map Display Type - Heightmap / Texture / Design</param>
    /// <param name="mode">The thematic mode to be rendered; elevation, compaction, temperature etc (Ignored in Height Map type)</param>
    /// <param name="width">The width, in pixels, of the image tile to be rendered</param>
    /// <param name="height">The height, in pixels, of the image tile to be rendered</param>
    /// <param name="bbox">The bounding box of the tile in decimal degrees: bottom left corner lat/lng and top right corner lat/lng</param>
    /// <returns>An image representing the data requested</returns>
    [ResponseCache(Duration = 900, VaryByQueryKeys = new[] { "*" })]
    [ValidateTileParameters]
    [Route("api/v2/map3d")]
    [HttpGet]
    public async Task<TileResult> GetMapTileData(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid,
      [FromQuery] Guid? designUid,
      [FromQuery] Guid? cutfillDesignUid,
      [FromQuery] MapDisplayType type,
      [FromQuery] DisplayMode mode,
      [FromQuery] ushort width,
      [FromQuery] ushort height,
      [FromQuery] string bbox)
    {
      var projectId = ((RaptorPrincipal)User).GetLegacyProjectId(projectUid);
      var projectSettings = GetProjectSettingsTargets(projectUid);
      var filter = GetCompactionFilter(projectUid, filterUid);

      Task<CompactionProjectSettingsColors> projectSettingsColors;
      Task<DesignDescriptor> design = null;
      Task<DesignDescriptor> cutFillDesign = null;
      if (type == MapDisplayType.DesignMap)
      {
        design = GetAndValidateDesignDescriptor(projectUid, designUid);

        projectSettingsColors = GetGreyScaleHeightColors();

        await Task.WhenAll(projectId, projectSettings, filter, design, projectSettingsColors);
        mode = DisplayMode.Design3D;
      }
      else if (type == MapDisplayType.HeightMap)
      {
        projectSettingsColors = GetGreyScaleHeightColors();
        
        await Task.WhenAll(projectId, projectSettings, filter, projectSettingsColors);
        mode = DisplayMode.Height; // The height map must be of type height....
      }
      else if(type == MapDisplayType.Texture)
      {
        // Only used in texture mode
        cutFillDesign = GetAndValidateDesignDescriptor(projectUid, cutfillDesignUid);

        projectSettingsColors = GetProjectSettingsColors(projectUid);

        await Task.WhenAll(projectId, projectSettings, filter, cutFillDesign, projectSettingsColors);
      }
      else
        throw new NotImplementedException();

      var tileResult = await WithServiceExceptionTryExecuteAsync(async () =>
        await tileService.GetProductionDataTile(
          projectSettings.Result,
          projectSettingsColors.Result,
          filter.Result,
          projectId.Result,
          projectUid,
          mode,
          width,
          height,
          boundingBoxHelper.GetBoundingBox(bbox),
          design?.Result ?? cutFillDesign?.Result, // If we have a design, it means we are asking for the design height map - otherwise we may have a cut fill design to determine the texture
          null,
          null,
          null,
          null,
          CustomHeaders, false));

      Response.Headers.Add("X-Warning", tileResult.TileOutsideProjectExtents.ToString());

      return tileResult;
    }

    /// <summary>
    /// Generates a raw image for use in the 3d map control
    /// These can be heightmaps / textures / designs
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="filterUid">Optional Filter UID</param>
    /// <param name="type">Map Display Type - Heightmap / Texture / Design</param>
    /// <param name="designUid">The Design File UID if showing the Design (ignored otherwise)</param>
    /// <param name="cutfillDesignUid">Cut fill design UID for the Texture, ignored for other modes</param>
    /// <param name="mode">The thematic mode to be rendered; elevation, compaction, temperature etc (Ignored in Height Map type)</param>
    /// <param name="width">The width, in pixels, of the image tile to be rendered</param>
    /// <param name="height">The height, in pixels, of the image tile to be rendered</param>
    /// <param name="bbox">The bounding box of the tile in decimal degrees: bottom left corner lat/lng and top right corner lat/lng</param>
    /// <returns>An image representing the data requested</returns>
    [ResponseCache(Duration = 900, VaryByQueryKeys = new[] { "*" })]
    [ValidateTileParameters]
    [Route("api/v2/map3d/png")]
    [HttpGet]
    [Obsolete("Use the TTM endpoint instead, it contains the model and texture in one result")]
    public async Task<FileResult> GetMapTileDataRaw(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid,
      [FromQuery] Guid? designUid,
      [FromQuery] Guid? cutfillDesignUid,
      [FromQuery] MapDisplayType type,
      [FromQuery] DisplayMode mode,
      [FromQuery] ushort width,
      [FromQuery] ushort height,
      [FromQuery] string bbox)
    {
      var result = await GetMapTileData(projectUid, filterUid, designUid, cutfillDesignUid, type, mode, width, height, bbox);
      return new FileStreamResult(new MemoryStream(result.TileData), ContentTypeConstants.ImagePng);
    }

    /// <summary>
    /// Get the ttm files from raptor for production data and design if required
    /// Generated the texture for the production data
    /// Return a zip file containing them both
    /// </summary>
    /// <returns>A Zip file containing a file 'model.obj', containing the 3d model(s). and a Texture.png </returns>
    [ResponseCache(Duration = 900, VaryByQueryKeys = new[] {"*"})]
    [Route("api/v2/map3d/ttm")]
    [HttpGet]
    public async Task<FileResult> GetMapTileDataTtm(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid,
      [FromQuery] Guid? designUid,
      [FromQuery] DisplayMode mode,
      [FromServices] IPreferenceProxy prefProxy,
      [FromServices] ITRexCompactionDataProxy tRexCompactionDataProxy,
#if RAPTOR
      [FromServices] IASNodeClient raptorClient,
#endif
      [FromServices] IProductionDataRequestFactory requestFactory,
      [FromServices] IFileRepository tccFileRepository)
    {
      const double SURFACE_EXPORT_TOLERANCE = 0.05;
      const byte COORDS_ARRAY_LENGTH = 3;

      var tins = new List<TrimbleTINModel>();

      var projectTask = ((RaptorPrincipal) User).GetProject(projectUid);
      var projectSettings = GetProjectSettingsTargets(projectUid);
      var userPreferences = prefProxy.GetUserPreferences(CustomHeaders);
      var filter = GetCompactionFilter(projectUid, filterUid);
      var designTask = GetAndValidateDesignDescriptor(projectUid, designUid);
      if (userPreferences == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
            "Failed to retrieve preferences for current user"));
      }

      await Task.WhenAll(projectTask, projectSettings, userPreferences, designTask);

      var project = projectTask.Result;
      var design = designTask.Result;

      // Get the terrain mesh
      var exportRequest = requestFactory.Create<ExportRequestHelper>(r => r
          .ProjectUid(projectUid)
          .ProjectId(project.LegacyProjectId)
          .Headers(CustomHeaders)
          .ProjectSettings(projectSettings.Result)
          .Filter(filter.Result))
        .SetUserPreferences(userPreferences.Result)
#if RAPTOR
        .SetRaptorClient(raptorClient)
#endif
        .SetProjectDescriptor(project)
        .CreateExportRequest(
          null, //startUtc,
          null, //endUtc,
          CoordType.LatLon,
          ExportTypes.SurfaceExport,
          "test.zip",
          true,
          false,
          OutputTypes.VedaAllPasses,
          string.Empty,
          SURFACE_EXPORT_TOLERANCE);

      exportRequest.Validate();

      // First get the export of production data from Raptor
      // comes in a zip file
      var result = await WithServiceExceptionTryExecuteAsync(async () => await RequestExecutorContainerFactory.Build<CompactionExportExecutor>(LoggerFactory,
#if RAPTOR
            raptorClient, 
#endif
            configStore: ConfigStore,
            trexCompactionDataProxy: tRexCompactionDataProxy,
            customHeaders: CustomHeaders)
          .ProcessAsync(exportRequest) as CompactionExportResult);

      var zipStream = new FileStream(result.FullFileName, FileMode.Open);

      using (var archive = new ZipArchive(zipStream))
      {
        // The zip file will have exactly one file in it
        if (archive.Entries.Count == 1)
        {
          try
          {
            var tin = new TrimbleTINModel();
            using (var stream = archive.Entries[0].Open() as DeflateStream)
            using (var ms = new MemoryStream())
            {
              // Unzip the file, copy to memory as the TIN file needs the byte array, and stream
              stream.CopyTo(ms);
              ms.Seek(0, SeekOrigin.Begin);

              tin.LoadFromStream(ms, ms.GetBuffer());

              tins.Add(tin);
            }
          }
          catch (TTMFileReadException e)
          {
            // Not valid, continue
            Log.LogWarning(e, "Failed to parse ttm in zip file");
          }
        }

      }

      // If we didn't get a valid file, then we failed to read the ttm from raptor
      if (tins.Count == 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
            "Failed to retrieve raptor data"));
      }

      // If we have a design request, get the ttm and add it for parsing
      if (design != null)
      {
        var path = design.File.Path + "/" + design.File.FileName;
        var file = await tccFileRepository.GetFile(design.File.FilespaceId, path);
        using (var ms = new MemoryStream())
        {
          if (file != null)
          {
            file.CopyTo(ms);
            ms.Seek(0, SeekOrigin.Begin);
            var tin = new TrimbleTINModel();
            tin.LoadFromStream(ms, ms.GetBuffer());
            tins.Add(tin);
          }
        }
      }

      // Calculating the bounding box for the model (including design if supplied)
      var minEasting = tins.Select(t => t.Header.MinimumEasting).Min();
      var maxEasting = tins.Select(t => t.Header.MaximumEasting).Max();
      var minNorthing = tins.Select(t => t.Header.MinimumNorthing).Min();
      var maxNorthing = tins.Select(t => t.Header.MaximumNorthing).Max();
      var centerEasting = (maxEasting + minEasting) / 2.0;
      var centerNorthing = (maxNorthing + minNorthing) / 2.0;
     
      TwoDConversionCoordinate[] convertedCoordinates;

#if RAPTOR
      if (UseTRexGateway("ENABLE_TREX_GATEWAY_TILES"))
      {
#endif
        var conversionCoordinates = new []
        {
          new TwoDConversionCoordinate(minEasting, minNorthing),
          new TwoDConversionCoordinate(maxEasting, maxNorthing),
          new TwoDConversionCoordinate(centerEasting, centerNorthing)
        };

        var conversionRequest = new CoordinateConversionRequest(projectUid, TwoDCoordinateConversionType.NorthEastToLatLon, conversionCoordinates);
        var conversionResult = await trexCompactionDataProxy.SendDataPostRequest<CoordinateConversionResult, CoordinateConversionRequest>(conversionRequest, "/coordinateconversion", CustomHeaders);

        if (conversionResult.Code != 0 || conversionResult.ConversionCoordinates.Length != COORDS_ARRAY_LENGTH)
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
              "Failed to retrieve long lat for boundary"));

        convertedCoordinates = conversionResult.ConversionCoordinates;
#if RAPTOR
      }
      else
      {
        var points = new TWGS84FenceContainer
        {
          FencePoints = new[]
          {
            TWGS84Point.Point(minEasting, minNorthing),
            TWGS84Point.Point(maxEasting, maxNorthing),
            TWGS84Point.Point(centerEasting, centerNorthing),
          }
        };

        // Convert the northing easting values to long lat values
        var res = raptorClient.GetGridCoordinates(project.LegacyProjectId, points, TCoordConversionType.ctNEEtoLLH, out var coordPointList);
        if (res != TCoordReturnCode.nercNoError)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
              "Failed to retrieve long lat for boundary"));
        }

        convertedCoordinates = coordPointList.Points.Coords.Select(c => new TwoDConversionCoordinate(c.X, c.Y)).ToArray();
      }
#endif

      // The values returned from Raptor/TRex are in rads, where we need degrees for the bbox
      var minLat = convertedCoordinates[0].Y * Coordinates.RADIANS_TO_DEGREES;
      var minLng = convertedCoordinates[0].X * Coordinates.RADIANS_TO_DEGREES;
      var maxLat = convertedCoordinates[1].Y * Coordinates.RADIANS_TO_DEGREES;
      var maxLng = convertedCoordinates[1].X * Coordinates.RADIANS_TO_DEGREES;
      var centerLat = convertedCoordinates[2].Y * Coordinates.RADIANS_TO_DEGREES;
      var centerLng = convertedCoordinates[2].X * Coordinates.RADIANS_TO_DEGREES;
      var bbox = $"{minLat},{minLng},{maxLat},{maxLng}";

      var outputStream = new MemoryStream();
      using (var zipArchive = new ZipArchive(outputStream, ZipArchiveMode.Create, true))
      {
        var textureZipEntry = zipArchive.CreateEntry("texture.png");
        using (var stream = textureZipEntry.Open())
        {
          // Write the texture to the zip
          var textureFileStream = await GetTexture(projectUid, designUid, projectSettings.Result, filter.Result, mode, bbox);
          textureFileStream.FileStream.CopyTo(stream);
        }

        // Write the model to the zip
        var modelZipEntry = zipArchive.CreateEntry("model.obj");
        using (var stream = modelZipEntry.Open())
        {
          var modelFileStream = ConvertMultipleToObj(tins, centerEasting, centerNorthing);
          modelFileStream.FileStream.CopyTo(stream);
        }

        // Add some metadata to help with positioning of the model
        var metaDataEntry = zipArchive.CreateEntry("metadata.json");
        using (var stream = metaDataEntry.Open())
        {
          var metaData = new
          {
            Minimum = new
            {
              Lat = minLat,
              Lng = minLng
            },
            Maximum = new
            {
              Lat = maxLat,
              Lng = maxLng
            },
            Center = new
            {
              Lat = centerLat,
              Lng = centerLng
            },
            HasDesign = design != null
          };
          var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(metaData));
          stream.Write(bytes,0, bytes.Length);
        }
      }

      // Don't forget to seek back, or else the content length will be 0
      outputStream.Seek(0, SeekOrigin.Begin);
      return new FileStreamResult(outputStream, ContentTypeConstants.ApplicationZip);
    }

    /// <summary>
    /// Get the texture for the model being created
    /// Generates the required boundign box, using the same information as used to generated the 3d model
    /// </summary>
    private async Task<FileStreamResult> GetTexture(Guid projectUid, Guid? cutfillDesignUid,
      CompactionProjectSettings projectSettings, FilterResult filter, DisplayMode mode, string bbox)
    {
      
      var project = ((RaptorPrincipal) User).GetProject(projectUid);
      var cutFillDesign = GetAndValidateDesignDescriptor(projectUid, cutfillDesignUid);

      var projectSettingsColors = GetProjectSettingsColors(projectUid);

      await Task.WhenAll(project, cutFillDesign, projectSettingsColors);

      var tileResult = await WithServiceExceptionTryExecuteAsync(() =>
        tileService.GetProductionDataTile(projectSettings,
          projectSettingsColors.Result,
          filter,
          project.Result.LegacyProjectId,
          projectUid,
          mode,
          4096,
          4096,
          boundingBoxHelper.GetBoundingBox(bbox),
          cutFillDesign.Result, // If we have a design, it means we are asking for the design height map - otherwise we may have a cut fill design to determine the texture
          null,
          null,
          null,
          null,
          CustomHeaders, false));

      Response.Headers.Add("X-Warning", tileResult.TileOutsideProjectExtents.ToString());

      return new FileStreamResult(new MemoryStream(tileResult.TileData), ContentTypeConstants.ImagePng);
    }

    /// <summary>
    /// Converts a collection of TTMs to a single obj model, including UV mapping
    /// </summary>
    private FileStreamResult ConvertMultipleToObj(IList<TrimbleTINModel> tins, double eastingOffset, double northingOffset)
    {
      // FileStreamResult will dispose of this once the response has been completed
      // See here: https://github.com/aspnet/Mvc/blob/25eb50120eceb62fd24ab5404210428fcdf0c400/src/Microsoft.AspNetCore.Mvc.Core/FileStreamResult.cs#L82
      var outputStream = new MemoryStream();
      using (var writer = new StreamWriter(outputStream, Encoding.UTF8, 32, true))
      {
        var
          vertexOffset =
            1; // With multiple objects in a file, the vertex indices used by faces does NOT reset between objects, therefor we have to keep a count
        var currentUvIndex = 1;
        var objIdx = 1;

        var zModifier = tins.SelectMany(t => t.Vertices.Items).Min(v => v.Z);

        var minX = tins.SelectMany(t => t.Vertices.Items).Min(v => v.X);
        var maxX = tins.SelectMany(t => t.Vertices.Items).Max(v => v.X);
        var width = maxX - minX;

        var minY = tins.SelectMany(t => t.Vertices.Items).Min(v => v.Y);
        var maxY = tins.SelectMany(t => t.Vertices.Items).Max(v => v.Y);
        var height = maxY - minY;

        foreach (var tin in tins)
        {
          var faces = new List<Face>();
          var uvs = new List<Uv>();
          writer.WriteLine($"o {tin.ModelName.Replace(" ", "")}.{objIdx++}");

          foreach (var vertex in tin.Vertices.Items)
          {
            writer.WriteLine($"v {(float) (vertex.X - eastingOffset)} " +
                             $"{(float) (vertex.Y - northingOffset)} " +
                             $"{(float) (vertex.Z - zModifier)}");
          }

          writer.WriteLine("");

          foreach (var face in tin.Triangles.Items)
          {
            var f = new Face
            {
              VertexIdx0 = face.Vertex0 + vertexOffset,
              VertexIdx1 = face.Vertex1 + vertexOffset,
              VertexIdx2 = face.Vertex2 + vertexOffset
            };

            foreach (var vertexIdx in new List<int> {face.Vertex0, face.Vertex1, face.Vertex2})
            {
              var vertex = tin.Vertices.Items[vertexIdx];
              var u = (vertex.X - minX) / width;
              var v = (vertex.Y - minY) / height;
              var uv = new Uv()
              {
                U = u,
                V = v
              };
              uvs.Add(uv);
              if (f.UvIdx0 == 0)
                f.UvIdx0 = currentUvIndex++;
              else if (f.UvIdx1 == 0)
                f.UvIdx1 = currentUvIndex++;
              else if (f.UvIdx2 == 0)
                f.UvIdx2 = currentUvIndex++;

            }

            faces.Add(f);
          }

          foreach (var uv in uvs)
          {
            writer.WriteLine($"vt {uv.U} {uv.V}");
          }

          writer.WriteLine("");
          foreach (var face in faces)
          {
            writer.WriteLine($"f {face.VertexIdx0}/{face.UvIdx0} " +
                             $"{face.VertexIdx1}/{face.UvIdx1} " +
                             $"{face.VertexIdx2}/{face.UvIdx2}");
          }

          writer.WriteLine("");

          // Update the vertex index for the next object, as the vertex index is global for the file (not per object)
          vertexOffset += tin.Vertices.Items.Length;
          writer.Flush();
        }

        outputStream.Seek(0, SeekOrigin.Begin);
        Log.LogInformation($"GetExportReportSurface completed: ExportData size={outputStream.Length}");
        return new FileStreamResult(outputStream, ContentTypeConstants.TextPlain);
      }
    }

    private Task<CompactionProjectSettingsColors> GetGreyScaleHeightColors()
    {
      var colors = new List<uint>();
      for (var i = 0; i <= 255; i++)
      {
        colors.Add((uint)i << 16 | (uint)i << 8 | (uint)i << 0);
      }

      return Task.FromResult(CompactionProjectSettingsColors.Create(false, colors));
    }
  }
}

