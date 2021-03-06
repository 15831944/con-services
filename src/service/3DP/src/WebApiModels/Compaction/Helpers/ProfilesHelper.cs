﻿using System;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.ResultHandling.Profiling;
#if RAPTOR
using VLPDDecls;
#endif
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Helpers
{
  public class ProfilesHelper
  {
    public const int PROFILE_TYPE_NOT_REQUIRED = -1;
    public const int PROFILE_TYPE_HEIGHT = 2;
    public const double ONE_MM = 0.001;

    public static bool CellGapExists(ProfileCellData prevCell, ProfileCellData currCell, out double prevStationIntercept)
    {
      return CellGapExists(prevCell?.Station, prevCell?.InterceptLength, currCell.Station, out prevStationIntercept);
    }

    public static bool CellGapExists(SummaryVolumesProfileCell prevCell, SummaryVolumesProfileCell currCell, out double prevStationIntercept)
    {
      return CellGapExists(prevCell?.Station, prevCell?.InterceptLength, currCell.Station, out prevStationIntercept);
    }

    private static bool CellGapExists(double? prevStation, double? prevInterceptLength, double currStation, out double prevStationIntercept)
    {
      bool hasPrev = prevStation.HasValue && prevInterceptLength.HasValue;
      prevStationIntercept = hasPrev ? prevStation.Value + prevInterceptLength.Value : 0.0;
     
      return hasPrev && Math.Abs(currStation - prevStationIntercept) > ONE_MM;
    }

    private static ServiceException ThrowNoProfileLineDefinedException()
    {
      return new ServiceException(HttpStatusCode.BadRequest,
        new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
          "The profile line requires series either grid or WGS84 points."));
    }

#if RAPTOR
    public static void ConvertProfileEndPositions(ProfileGridPoints gridPoints, ProfileLLPoints lLPoints,
                                                 out TWGS84Point startPt, out TWGS84Point endPt,
                                                 out bool positionsAreGrid)
    {
      if (gridPoints != null)
      {
        positionsAreGrid = true;
        startPt = new TWGS84Point { Lat = gridPoints.y1, Lon = gridPoints.x1 };
        endPt = new TWGS84Point { Lat = gridPoints.y2, Lon = gridPoints.x2 };
      }
      else if (lLPoints != null)
      {
        positionsAreGrid = false;
        startPt = new TWGS84Point
        {
          Lat = lLPoints.lat1,
          Lon = lLPoints.lon1
        };
        endPt = new TWGS84Point
        {
          Lat = lLPoints.lat2,
          Lon = lLPoints.lon2
        };
      }
      else
        throw ThrowNoProfileLineDefinedException();
    }
#endif

      public static void ConvertProfileEndPositions(ProfileGridPoints gridPoints, ProfileLLPoints lLPoints,
      out WGSPoint startPt, out WGSPoint endPt, out bool positionsAreGrid)
    {
      if (gridPoints != null)
      {
        positionsAreGrid = true;
        startPt = new WGSPoint(gridPoints.y1, gridPoints.x1);
        endPt = new WGSPoint(gridPoints.y2, gridPoints.x2);
      }
      else
      if (lLPoints != null)
      {
        positionsAreGrid = false;
        startPt = new WGSPoint(lLPoints.lat1, lLPoints.lon1);
        endPt = new WGSPoint(lLPoints.lat2, lLPoints.lon2);
      }
      else
        throw ThrowNoProfileLineDefinedException();
    }
  }
}
