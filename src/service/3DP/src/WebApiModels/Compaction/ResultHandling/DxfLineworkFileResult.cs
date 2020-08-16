﻿using System.Collections.Generic;
using System.Linq;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Algorithms;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Models.MapHandling;
using VSS.TRex.Gateway.Common.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling
{
  /// <summary>
  /// Represents the response to a request to get boundaries from a DXF linework file.
  /// </summary>
  /// <remarks>
  /// We are not altering the coordinates in any way; they are in the order received from Raptor/Trex. Please note, this likely means
  /// they do not conform with the 2016 IETF specification on 'right hand winding'; see section 3.1.6 of the spec; https://tools.ietf.org/html/rfc7946#section-3.1.6.
  ///
  /// If you need a linter for the 3DP response that supports the 2008 informal spec see http://geojson.io/#map=2/20.0/0.0.
  /// </remarks>
  public class DxfLineworkFileResult : ContractExecutionResult
  {
    public WGS84LineworkBoundary[] LineworkBoundaries { get; }

    public DxfLineworkFileResult(int code, string message, WGS84LineworkBoundary[] lineworkBoundaries) : base (code, message)
    {
      LineworkBoundaries = lineworkBoundaries;
    }

    public DxfLineworkFileResult(List<DXFBoundaryResultItem> boundaries, int code, string message) : base(code, message)
    {
      LineworkBoundaries = boundaries.Select(b => new WGS84LineworkBoundary { Boundary = b.Fence.ToArray(), BoundaryName = b.Name, BoundaryType = b.Type }).ToArray();
    }

    public GeoJson ConvertToGeoJson(bool convertLineStringCoordsToPolygon, int maxVerticesToApproximateTo)
    {
      if (LineworkBoundaries == null) return null;

      var geoJson = new GeoJson
      {
        Type = GeoJson.FeatureType.FEATURE_COLLECTION,
        Features = new List<Feature>()
      };

      foreach (var boundary in LineworkBoundaries)
      {
        var fencePoints = DouglasPeucker.DouglasPeuckerByCount(
          boundary.Boundary.Select(p => new WGSPoint(latitude: p.Lat, longtitude: p.Lon)).ToArray(),
          maxVerticesToApproximateTo);

        geoJson.Features.Add(new Feature
        {
          Type = GeoJson.FeatureType.FEATURE,
          Properties = new Properties { Name = boundary.BoundaryName },
          Geometry = GetCoordinatesFromFencePoints(fencePoints, convertLineStringCoordsToPolygon)
        });
      }

      return geoJson;
    }

    private static Geometry GetCoordinatesFromFencePoints(List<double[]> fencePoints, bool convertLineStringCoordsToPolygon)
    {
      var boundaryType = Geometry.Types.POLYGON;

      if (fencePoints.First()[0] != fencePoints.Last()[0] && fencePoints.First()[1] != fencePoints.Last()[1])
      {
        if (convertLineStringCoordsToPolygon)
        {
          fencePoints.Add(fencePoints.First());
        }
        else
        {
          boundaryType = Geometry.Types.LINESTRING;
        }
      }

      return new Geometry
      {
        Type = boundaryType,
        Coordinates = new List<List<double[]>> { fencePoints }
      };
    }
  }
}
