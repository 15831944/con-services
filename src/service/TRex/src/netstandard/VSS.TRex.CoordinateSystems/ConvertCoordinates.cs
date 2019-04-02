﻿using System;
using System.Linq;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using VSS.ConfigurationStore;
using VSS.Tpaas.Client.Abstractions;
using VSS.Tpaas.Client.Clients;
using VSS.Tpaas.Client.RequestHandlers;
using VSS.TRex.Common;
using VSS.TRex.Common.Models;
using VSS.TRex.Common.Utilities;
using VSS.TRex.CoordinateSystems.Models;
using VSS.TRex.DI;
using VSS.TRex.Geometry;
using VSS.TRex.HttpClients;
using VSS.TRex.Types;

namespace VSS.TRex.CoordinateSystems
{
  /// <summary>
  /// Implements a set of capabilities for coordinate conversion between WGS and grid contexts, and
  /// conversion of coordinate system files into CSIB (Coordinate System Information Block) strings.
  /// </summary>
  /// <remarks>
  /// While these methods can be called directly, it's recommended to utilize the static ConvertCoordinates helper.
  /// </remarks>
  public static class ConvertCoordinates
  {
    private static readonly object lockObject = new object();

    static ConvertCoordinates()
    {
      var configurationStore = DIContext.Obtain<IConfigurationStore>();

      lock (lockObject)
      {
        DIBuilder
          .Continue()
          .Add(x => x.AddTransient<TRexTPaaSAuthenticatedRequestHandler>()
            .AddHttpClient<ITPaaSClient, TPaaSClient>(client => client.BaseAddress = new Uri(configurationStore.GetValueString(TPaaSClient.TPAAS_AUTH_URL_ENV_KEY)))
            .ConfigurePrimaryHttpMessageHandler(() => new TPaaSApplicationCredentialsRequestHandler
            {
              TPaaSToken = configurationStore.GetValueString(TPaaSApplicationCredentialsRequestHandler.TPAAS_APP_TOKEN_ENV_KEY),
              InnerHandler = new HttpClientHandler()
            })
            .Services.AddHttpClient<CoordinatesServiceClient>(client => client.BaseAddress = new Uri(configurationStore.GetValueString(CoordinatesServiceClient.COORDINATE_SERVICE_URL_ENV_KEY)))
            .AddHttpMessageHandler<TRexTPaaSAuthenticatedRequestHandler>())
          .Complete();
      }

      serviceClient = DIContext.Obtain<CoordinatesServiceClient>();
    }

    private static readonly CoordinatesServiceClient serviceClient;

    /// <summary>
    /// Provides a null conversion between the 2D coordinates in a WGS84 LL point and a XYZ NEE point.
    /// Only the 2D coordinates are used, and directly copied from the LL point to the XY point, maintaining the
    /// X == Longitude and Y == Latitude sense of the relative coordinates
    /// </summary>
    public static XYZ NullWGSLLToXY(WGS84Point WGSLL) => new XYZ(WGSLL.Lon, WGSLL.Lat, Consts.NullDouble);

    /// <summary>
    /// Takes an array of <see cref="WGS84Point"/> and uses the Coordinate Service to convert it into <see cref="XYZ"/> data.
    /// </summary>
    public static XYZ[] NullWGSLLToXY(WGS84Point[] WGSLLs) => WGSLLs.Select(x => new XYZ(x.Lon, x.Lat)).ToArray();

    /// <summary>
    /// Takes a <see cref="LLH"/> and uses the Coordinate Service to convert it into <see cref="NEE"/> data.
    /// </summary>
    public static NEE LLHToNEE(string id, LLH LLH, bool convertToRadians = true)
    {
      if (convertToRadians)
      {
        LLH.Longitude = MathUtilities.RadiansToDegrees(LLH.Longitude);
        LLH.Latitude = MathUtilities.RadiansToDegrees(LLH.Latitude);
      }

      return serviceClient.GetNEEFromLLHAsync(id, LLH).Result;
    }

    /// <summary>
    /// Takes an array of <see cref="LLH"/> and uses the Coordinate Service to convert it into <see cref="NEE"/> data.
    /// </summary>
    public static (RequestErrorStatus ErrorCode, NEE[] NEECoordinates) LLHToNEE(string id, LLH[] LLH, bool convertToRadians = true)
    {
      return serviceClient.GetNEEFromLLHAsync(id, LLH.ToRequestArray(convertToRadians)).Result;
    }

    /// <summary>
    /// Converts <see cref="XYZ"/> coordinates holding <see cref="LLH"/> data into <see cref="NEE"/> data.
    /// </summary>
    public static XYZ LLHToNEE(string id, XYZ coordinates, bool convertToRadians = true)
    {
      var result = serviceClient.GetNEEFromLLHAsync(id, coordinates.ToLLH(convertToRadians)).Result;

      return new XYZ
      {
        X = result.East,
        Y = result.North,
        Z = result.Elevation
      };
    }

    /// <summary>
    /// Converts <see cref="XYZ"/> coordinates holding <see cref="NEE"/> data into <see cref="LLH"/> data.
    /// </summary>
    public static XYZ NEEToLLH(string id, XYZ coordinates)
    {
      var result = serviceClient.GetLLHFromNEEAsync(id, coordinates.ToNEE()).Result;

      return new XYZ
      {
        X = result.Longitude,
        Y = result.Latitude,
        Z = result.Height
      };
    }

    /// <summary>
    /// Takes an array of <see cref="XYZ"/> and uses the Coordinate Service to convert it into <see cref="NEE"/> data.
    /// </summary>
    public static (RequestErrorStatus ErrorCode, XYZ[] NEECoordinates) LLHToNEE(string id, XYZ[] coordinates, bool convertToRadians = true)
    {
      var result = serviceClient.GetNEEFromLLHAsync(id, coordinates.ToLLHRequestArray(convertToRadians)).Result;
      if (result.ErrorCode != RequestErrorStatus.OK)
      {
        return (result.ErrorCode, null);
      }

      var NEECoords = new XYZ[result.NEECoordinates.Length];

      for (var i = 0; i < result.NEECoordinates.Length; i++)
      {
        NEECoords[i].X = result.NEECoordinates[i].East;
        NEECoords[i].Y = result.NEECoordinates[i].North;
        NEECoords[i].Z = result.NEECoordinates[i].Elevation;
      }

      return (RequestErrorStatus.OK, NEECoords);
    }

    /// <summary>
    /// Takes an array of <see cref="XYZ"/> and uses the Coordinate Service to convert it into <see cref="LLH"/> data.
    /// </summary>
    public static (RequestErrorStatus ErrorCode, XYZ[] LLHCoordinates) NEEToLLH(string id, XYZ[] coordinates)
    {
      var result = serviceClient.GetLLHFromNEEAsync(id, coordinates.ToNEERequestArray()).Result;
      if (result.ErrorCode != RequestErrorStatus.OK)
      {
        return (result.ErrorCode, null);
      }

      var LLHCoords = new XYZ[result.LLHCoordinates.Length];

      for (var i = 0; i < result.LLHCoordinates.Length; i++)
      {
        LLHCoords[i].X = result.LLHCoordinates[i].Longitude;
        LLHCoords[i].Y = result.LLHCoordinates[i].Latitude;
        LLHCoords[i].Z = result.LLHCoordinates[i].Height;
      }

      return (RequestErrorStatus.OK, LLHCoords);
    }

    /// <summary>
    /// Takes a <see cref="NEE"/> and uses the Coordinate Service to convert it into <see cref="LLH"/> data.
    /// </summary>
    public static LLH NEEToLLH(string id, NEE NEE) => serviceClient.GetLLHFromNEEAsync(id, NEE).Result;

    /// <summary>
    /// Takes an array of <see cref="NEE"/> and uses the Coordinate Service to convert it into <see cref="LLH"/> data.
    /// </summary>
    public static (RequestErrorStatus ErrorCode, LLH[] LLHCoordinates) NEEToLLH(string id, NEE[] NEE) => serviceClient.GetLLHFromNEEAsync(id, NEE.ToRequestArray()).Result;

    /// <summary>
    /// Uses the Coordinate Service to convert WGS84 coordinates into the site calibration used by the project.
    /// </summary>
    public static XYZ WGS84ToCalibration(string id, WGS84Point wgs84Point, bool convertToRadians = true)
    {
      var nee = serviceClient.GetNEEFromLLHAsync(id, new LLH
      {
        Latitude = convertToRadians ? MathUtilities.RadiansToDegrees(wgs84Point.Lat) : wgs84Point.Lat,
        Longitude = convertToRadians ? MathUtilities.RadiansToDegrees(wgs84Point.Lon) : wgs84Point.Lon,
        Height = wgs84Point.Height
      }).Result;

      return new XYZ
      {
        X = nee.North,
        Y = nee.East,
        Z = nee.Elevation
      };
    }

    /// <summary>
    /// Uses the Coordinate Service to convert an array of WGS84 coordinates into the site calibration used by the project.
    /// </summary>
    public static XYZ[] WGS84ToCalibration(string id, WGS84Point[] wgs84Points, bool convertToRadians = true)
    {
      (var errorCode, NEE[] neeCoordinates) = serviceClient.GetNEEFromLLHAsync(id, wgs84Points.ToRequestArray(convertToRadians)).Result;

      if (errorCode != RequestErrorStatus.OK)
      {
        return null;
      }

      var xyzArray = new XYZ[neeCoordinates.Length];

      for (var i = 0; i < wgs84Points.Length; i++)
      {
        xyzArray[i] = new XYZ
        {
          X = neeCoordinates[i].East,
          Y = neeCoordinates[i].North,
          Z = neeCoordinates[i].Elevation
        };
      }

      return xyzArray;
    }

    /// <summary>
    /// Takes the full path and name of a DC file, reads it and uses the Trimble Coordinate service to convert it into a
    /// csib string
    /// </summary>
    public static string DCFileToCSIB(string filePath) => serviceClient.ImportFromDCAsync(filePath).Result;

    /// <summary>
    /// Takes the content of a DC file as a byte array and uses the Trimble Coordinates Service to convert
    /// it into a CSIB string
    /// </summary>
    public static string DCFileContentToCSIB(string filePath, byte[] fileContent) => serviceClient.ImportFromDCContentAsync(filePath, fileContent).Result;

    /// <summary>
    /// Takes the content of a DC file as a byte array and uses the Trimble Coordinates Service to convert
    /// it into a coordinate system definition response object.
    /// </summary>
    public static CoordinateSystemResponse DCFileContentToCSD(string filePath, byte[] fileContent) => serviceClient.ImportCSDFromDCContentAsync(filePath, fileContent).Result;

    /// <summary>
    /// Takes the CSIB string and uses the Trimble Coordinates Service to convert
    /// it into a coordinate system definition response object.
    /// </summary>
    public static CoordinateSystemResponse CSIBContentToCSD(string csib) => serviceClient.ImportCSDFromCSIBAsync(csib).Result;

  }
}
