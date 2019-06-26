﻿using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Common;
using VSS.TRex.Common.Models;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.Geometry;
using VSS.TRex.Profiling;
using VSS.TRex.Profiling.GridFabric.Arguments;
using VSS.TRex.Profiling.GridFabric.Requests;
using VSS.TRex.Profiling.Models;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Webtools.Controllers
{
  [Route("api/profiles")]
  public class ProfilesController : Controller
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<ProfilesController>();

    /// <summary>
    /// Gets a profile between two points across a design in a project
    /// </summary>
    /// <param name="siteModelID">Grid to return status for</param>
    /// <param name="designID"></param>
    /// <param name="startX"></param>
    /// <param name="startY"></param>
    /// <param name="endX"></param>
    /// <param name="endY"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    [HttpGet("design/{siteModelID}/{designID}")]
    public JsonResult ComputeDesignProfile(string siteModelID, string designID,
      [FromQuery] double startX,
      [FromQuery] double startY,
      [FromQuery] double endX,
      [FromQuery] double endY,
      [FromQuery] double? offset)
    {
      var siteModelUid = Guid.Parse(siteModelID);
      var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(siteModelUid);
      var design = siteModel?.Designs?.Locate(Guid.Parse(designID));

      if (design == null)
        return new JsonResult($"Unable to locate design {designID} in project {siteModelID}");

      var result = design.ComputeProfile(siteModelUid, new[] {new XYZ(startX, startY, 0), new XYZ(endX, endY, 0)}, siteModel.CellSize, offset ?? 0, out DesignProfilerRequestResult errCode);

      return new JsonResult(result);
    }

    /// <summary>
    /// Gets a profile between two points across a design in a project
    /// </summary>
    /// <param name="siteModelID">Grid to return status for</param>
    /// <param name="startX"></param>
    /// <param name="startY"></param>
    /// <param name="endX"></param>
    /// <param name="endY"></param>
    /// <returns></returns>
    [HttpGet("compositeelevations/{siteModelID}")]
    public JsonResult ComputeCompositeElevationProfile(string siteModelID,
      [FromQuery] double startX,
      [FromQuery] double startY,
      [FromQuery] double endX,
      [FromQuery] double endY)
    {
      var siteModelUid = Guid.Parse(siteModelID);
   
      var arg = new ProfileRequestArgument_ApplicationService
      {
        ProjectID = siteModelUid,
        ProfileTypeRequired = GridDataType.Height,
        ProfileStyle = ProfileStyle.CellPasses,
        PositionsAreGrid = true,
        Filters = new FilterSet(new[] {new CombinedFilter()}),
        ReferenceDesign = new DesignOffset(),
        StartPoint = new WGS84Point(lon: startX, lat: startY),
        EndPoint = new WGS84Point(lon: endX, lat: endY),
        ReturnAllPassesAndLayers = false,
      };

      // Compute a profile from the bottom left of the screen extents to the top right 
      var request = new ProfileRequest_ApplicationService_ProfileCell();
      var Response = request.Execute(arg);

      if (Response == null)
        return new JsonResult(@"Profile response is null");

      if (Response.ProfileCells == null)
        return new JsonResult(@"Profile response contains no profile cells");

      //var nonNulls = Response.ProfileCells.Where(x => !x.IsNull()).ToArray();
      return new JsonResult(Response.ProfileCells.Select(x => new
      {
        station = x.Station,
        cellLowestElev = x.CellLowestElev,
        cellHighestElev = x.CellHighestElev,
        cellLastElev = x.CellLastElev,
        cellFirstElev = x.CellFirstElev,
        cellLowestCompositeElev = x.CellLowestCompositeElev,
        cellHighestCompositeElev = x.CellHighestCompositeElev,
        cellLastCompositeElev = x.CellLastCompositeElev,
        cellFirstCompositeElev = x.CellFirstCompositeElev
      }));
    }

    /// <summary>
    /// Gets a profile between two points across a design in a project
    /// </summary>
    /// <param name="siteModelID">Grid to return status for</param>
    /// <param name="startX"></param>
    /// <param name="startY"></param>
    /// <param name="endX"></param>
    /// <param name="endY"></param>
    /// <param name="mode"></param>
    /// <param name="cutFillDesignUid"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    [HttpGet("productiondata/{siteModelID}")]
    public JsonResult ComputeProductionDataProfile(string siteModelID,
      [FromQuery] double startX,
      [FromQuery] double startY,
      [FromQuery] double endX,
      [FromQuery] double endY,
      [FromQuery] int displayMode,
      [FromQuery] Guid? cutFillDesignUid,
      [FromQuery] double? offset)
    {
      var siteModelUid = Guid.Parse(siteModelID);

      var arg = new ProfileRequestArgument_ApplicationService
      {
        ProjectID = siteModelUid,
        ProfileTypeRequired = GridDataType.Height,
        ProfileStyle = ProfileStyle.CellPasses,
        PositionsAreGrid = true,
        Filters = new FilterSet(new [] { new CombinedFilter() }),
        ReferenceDesign = new DesignOffset(cutFillDesignUid ?? Guid.Empty, offset ?? 0.0),
        StartPoint = new WGS84Point(lon: startX, lat: startY),
        EndPoint = new WGS84Point(lon: endX, lat: endY),
        ReturnAllPassesAndLayers = false,
      };

      // Compute a profile from the bottom left of the screen extents to the top right 
      var request = new ProfileRequest_ApplicationService_ProfileCell();
      var Response = request.Execute(arg);
      
      if (Response == null)
        return new JsonResult(@"Profile response is null");
      
      if (Response.ProfileCells == null)
        return new JsonResult(@"Profile response contains no profile cells");

      return new JsonResult(Response.ProfileCells.Select(x => new { station = x.Station, z = x.CellLastElev, value = x.CellLastElev })); 
    }


    private double ProfileValue(int mode, ProfileCell cell)
    {
      switch ((DisplayMode) mode)
      {
        case DisplayMode.CCV:
          break;
        case DisplayMode.CCVPercentSummary:
          break;
        case DisplayMode.CMVChange:
          break;
        case DisplayMode.PassCount:
          break;
        case DisplayMode.PassCountSummary:
          break;
        case DisplayMode.CutFill:
          break;
        case DisplayMode.TemperatureSummary:
          break;
        case DisplayMode.TemperatureDetail:
          break;
        case DisplayMode.MDPPercentSummary:
          break;
        case DisplayMode.TargetSpeedSummary:
          break;
        case DisplayMode.Height:
        default:
          break;
      }
    }


    private double ProfileElevation(int mode, ProfileCell cell)
    {
      //TODO: see 3dpm service for this (CompactionProfileExecutor)

      var elevation = 0.0;
      switch ((DisplayMode) mode)
      {
        case DisplayMode.CCV:
        case DisplayMode.CCVPercentSummary:
        case DisplayMode.CMVChange:
          elevation = cell.CellCCVElev;
          break;
        case DisplayMode.PassCount:
        case DisplayMode.PassCountSummary:
          elevation = cell.CellLastElev;
          break;
        case DisplayMode.CutFill:
          break;
        case DisplayMode.TemperatureSummary:
        case DisplayMode.TemperatureDetail:
          //TODO:
          break;
        case DisplayMode.MDPPercentSummary:
          elevation = cell.CellMDPElev;
          break;
        case DisplayMode.TargetSpeedSummary:
          //TODO:

          break;
        case DisplayMode.Height:
        default:
          elevation = cell.CellLastElev;

          break;
      }

      return elevation;
    }



    [HttpGet("volumes/{siteModelID}")]
    public JsonResult ComputeSummaryVolumesProfile(string siteModelID,
      [FromQuery] double startX,
      [FromQuery] double startY,
      [FromQuery] double endX,
      [FromQuery] double endY)
    {
      //TODO: can add design to ground and ground to design by passing the cutFillDesignUid

      var siteModelUid = Guid.Parse(siteModelID);

      var arg = new ProfileRequestArgument_ApplicationService
      {
        ProjectID = siteModelUid,
        ProfileTypeRequired = GridDataType.Height,
        ProfileStyle = ProfileStyle.SummaryVolume,
        PositionsAreGrid = true,
        Filters = new FilterSet(new CombinedFilter(), new CombinedFilter()),
        StartPoint = new WGS84Point(lon: startX, lat: startY),
        EndPoint = new WGS84Point(lon: endX, lat: endY),
        ReturnAllPassesAndLayers = false,
        VolumeType = VolumeComputationType.Between2Filters
      };

      // This is a simple earliest filter to latest filter test
      arg.Filters.Filters[0].AttributeFilter.ReturnEarliestFilteredCellPass = true;
      arg.Filters.Filters[1].AttributeFilter.ReturnEarliestFilteredCellPass = false;

      // Compute a profile from the bottom left of the screen extents to the top right 
      var request = new ProfileRequest_ApplicationService_SummaryVolumeProfileCell();

      var Response = request.Execute(arg);
      if (Response == null)
        return new JsonResult(@"Profile response is null");

      if (Response.ProfileCells == null)
        return new JsonResult(@"Profile response contains no profile cells");

      return new JsonResult(Response.ProfileCells.Select(x => new XYZS(0, 0, x.LastCellPassElevation2 - x.LastCellPassElevation1, x.Station, -1)));
    }
  }
}
