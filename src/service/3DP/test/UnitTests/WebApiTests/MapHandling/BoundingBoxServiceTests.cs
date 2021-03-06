﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DesignProfiler.ComputeDesignBoundary.RPC;
using DesignProfiler.ComputeDesignFilterBoundary.RPC;
using DesignProfilerDecls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Serilog;
using SVOICStatistics;
using VLPDDecls;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Models.Designs;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.WebApi.Models.MapHandling;
using VSS.Serilog.Extensions;
using VSS.TRex.Gateway.Common.Abstractions;

namespace VSS.Productivity3D.WebApiTests.MapHandling
{
  [TestClass]
  public class BoundingBoxServiceTests
  {
    public IServiceProvider serviceProvider;

    [TestInitialize]
    public void InitTest()
    {
      serviceProvider = new ServiceCollection()
        .AddLogging()
        .AddSingleton(new LoggerFactory().AddSerilog(SerilogExtensions.Configure()))
        .BuildServiceProvider();
    }

    [TestMethod]
    public async Task GetBoundingBoxPolygonFilter()
    {
      var polygonPoints = new List<WGSPoint>
      {
        new WGSPoint(10, 20),
        new WGSPoint(15, 20),
        new WGSPoint(13, 15),
        new WGSPoint(25, 30),
        new WGSPoint(27, 27)
      };

      var filterResult = FilterResult.CreateFilterObsolete(polygonLL: polygonPoints);
      var raptorClient = new Mock<IASNodeClient>();

      var service = new BoundingBoxService(serviceProvider.GetRequiredService<ILoggerFactory>()
#if RAPTOR
        , raptorClient.Object
#endif
        , new Mock<IConfigurationStore>().Object
        , new Mock<ITRexCompactionDataProxy>().Object, new Mock<IFileImportProxy>().Object
      );
      var bbox = await service.GetBoundingBox(project, filterResult, new[] { TileOverlayType.BaseMap }, null, null, null, null, null);
      Assert.AreEqual(polygonPoints.Min(p => p.Lat), bbox.minLat);
      Assert.AreEqual(polygonPoints.Min(p => p.Lon), bbox.minLng);
      Assert.AreEqual(polygonPoints.Max(p => p.Lat), bbox.maxLat);
      Assert.AreEqual(polygonPoints.Max(p => p.Lon), bbox.maxLng);
    }

    [TestMethod]
    public async Task GetBoundingBoxPolygonAndDesignBoundaryFilter()
    {
      //design boundary points: -115.018,36.208 -115.025,36.214 -115.123,36.17 -115.018,36.208
      var design = new DesignDescriptor(-1, null, 0);
      var polygonPoints = new List<WGSPoint>
      {
        new WGSPoint(35.98.LatDegreesToRadians(), -115.11.LonDegreesToRadians()),
        new WGSPoint(36.15.LatDegreesToRadians(), -115.74.LonDegreesToRadians()),
        new WGSPoint(36.10.LatDegreesToRadians(), -115.39.LonDegreesToRadians())
      };
      var filterResult = FilterResult.CreateFilterObsolete(polygonLL: polygonPoints, designFile: design);

      var raptorClient = new Mock<IASNodeClient>();

      TDesignProfilerRequestResult designProfilerResult;
      var ms = new MemoryStream();
      using (var writer = new StreamWriter(ms))
      {
        writer.Write(DESIGN_GEO_JSON);
        writer.Flush();

        raptorClient
          .Setup(x => x.GetDesignBoundary(It.IsAny<TDesignProfilerServiceRPCVerb_CalculateDesignBoundary_Args>(),
            out ms, out designProfilerResult))
          .Returns(true);

        var service = new BoundingBoxService(serviceProvider.GetRequiredService<ILoggerFactory>()
#if RAPTOR
          , raptorClient.Object
#endif
          , new Mock<IConfigurationStore>().Object
          , new Mock<ITRexCompactionDataProxy>().Object, new Mock<IFileImportProxy>().Object
        );
        var bbox = await service.GetBoundingBox(project, filterResult, new[] { TileOverlayType.BaseMap }, null, null, null, null, null);
        //bbox is a mixture of polgon and design boundary (see GeoJson)
        Assert.AreEqual(-115.74.LonDegreesToRadians(), bbox.minLng);
        Assert.AreEqual(35.98.LatDegreesToRadians(), bbox.minLat);
        Assert.AreEqual(-115.018.LonDegreesToRadians(), bbox.maxLng);
        Assert.AreEqual(36.214.LatDegreesToRadians(), bbox.maxLat);
      }
    }

    [TestMethod]
    public async Task GetBoundingBoxDesignBoundaryFilter()
    {
      var design = new DesignDescriptor(-1, null, 0);
      var filterResult = FilterResult.CreateFilterObsolete(designFile: design);
      var raptorClient = new Mock<IASNodeClient>();

      TDesignProfilerRequestResult designProfilerResult;
      var ms = new MemoryStream();
      using (var writer = new StreamWriter(ms))
      {
        writer.Write(DESIGN_GEO_JSON);
        writer.Flush();

        raptorClient
          .Setup(x => x.GetDesignBoundary(It.IsAny<TDesignProfilerServiceRPCVerb_CalculateDesignBoundary_Args>(),
            out ms, out designProfilerResult))
          .Returns(true);

        var service = new BoundingBoxService(serviceProvider.GetRequiredService<ILoggerFactory>()
#if RAPTOR
          , raptorClient.Object
#endif
          , new Mock<IConfigurationStore>().Object
          , new Mock<ITRexCompactionDataProxy>().Object, new Mock<IFileImportProxy>().Object
        );
        var bbox = await service.GetBoundingBox(project, filterResult, new[] { TileOverlayType.BaseMap }, null, null, null, null, null);
        //Values are from GeoJson below
        Assert.AreEqual(-115.123.LonDegreesToRadians(), bbox.minLng);
        Assert.AreEqual(36.175.LatDegreesToRadians(), bbox.minLat);
        Assert.AreEqual(-115.018.LonDegreesToRadians(), bbox.maxLng);
        Assert.AreEqual(36.214.LatDegreesToRadians(), bbox.maxLat);
      }
    }

    [TestMethod]
    public async Task GetBoundingBoxCutFillDesign()
    {
      var design = new DesignDescriptor(-1, null, 0);

      var raptorClient = new Mock<IASNodeClient>();

      TDesignProfilerRequestResult designProfilerResult;
      var ms = new MemoryStream();
      using (var writer = new StreamWriter(ms))
      {
        writer.Write(DESIGN_GEO_JSON);
        writer.Flush();

        raptorClient
          .Setup(x => x.GetDesignBoundary(It.IsAny<TDesignProfilerServiceRPCVerb_CalculateDesignBoundary_Args>(),
            out ms, out designProfilerResult))
          .Returns(true);

        var service = new BoundingBoxService(serviceProvider.GetRequiredService<ILoggerFactory>()
#if RAPTOR
          , raptorClient.Object
#endif
          , new Mock<IConfigurationStore>().Object
          , new Mock<ITRexCompactionDataProxy>().Object, new Mock<IFileImportProxy>().Object
        );
        var bbox = await service.GetBoundingBox(project, null, new[] { TileOverlayType.BaseMap }, null, null, design, null, null);
        //Values are from GeoJson below
        Assert.AreEqual(-115.123.LonDegreesToRadians(), bbox.minLng);
        Assert.AreEqual(36.175.LatDegreesToRadians(), bbox.minLat);
        Assert.AreEqual(-115.018.LonDegreesToRadians(), bbox.maxLng);
        Assert.AreEqual(36.214.LatDegreesToRadians(), bbox.maxLat);
      }
    }

    [TestMethod]
    public async Task GetBoundingBoxPolygonFilterAndCutFillDesign()
    {
      var polygonPoints = new List<WGSPoint>
      {
        new WGSPoint(10, 20),
        new WGSPoint(15, 20),
        new WGSPoint(13, 15),
        new WGSPoint(25, 30),
        new WGSPoint(27, 27)
      };

      var filterResult = FilterResult.CreateFilterObsolete(polygonLL: polygonPoints);
      var design = new DesignDescriptor(-1, null, 0);
      var raptorClient = new Mock<IASNodeClient>();

      TDesignProfilerRequestResult designProfilerResult;
      var ms = new MemoryStream();
      using (var writer = new StreamWriter(ms))
      {
        writer.Write(DESIGN_GEO_JSON);
        writer.Flush();

        raptorClient
          .Setup(x => x.GetDesignBoundary(It.IsAny<TDesignProfilerServiceRPCVerb_CalculateDesignBoundary_Args>(),
            out ms, out designProfilerResult))
          .Returns(true);

        var service = new BoundingBoxService(serviceProvider.GetRequiredService<ILoggerFactory>()
#if RAPTOR
          , raptorClient.Object
#endif
          , new Mock<IConfigurationStore>().Object
          , new Mock<ITRexCompactionDataProxy>().Object, new Mock<IFileImportProxy>().Object
        );
        var bbox = await service.GetBoundingBox(project, filterResult, new[] { TileOverlayType.BaseMap }, null, null, design, null, null);
        Assert.AreEqual(polygonPoints.Min(p => p.Lat), bbox.minLat);
        Assert.AreEqual(polygonPoints.Min(p => p.Lon), bbox.minLng);
        Assert.AreEqual(polygonPoints.Max(p => p.Lat), bbox.maxLat);
        Assert.AreEqual(polygonPoints.Max(p => p.Lon), bbox.maxLng);
      }
    }

    [TestMethod]
    public async Task GetBoundingBoxAlignmentFilter()
    {
      var alignment = new DesignDescriptor(-1, null, 0);
      var filterResult = FilterResult.CreateFilterObsolete(alignmentFile: alignment, startStation: 0, endStation: 3, leftOffset: 0.5, rightOffset: 0.5);

      var raptorClient = new Mock<IASNodeClient>();

      TWGS84Point[] fence =
      {
        new TWGS84Point{Lat = 36.1.LatDegreesToRadians(), Lon = -115.1.LonDegreesToRadians()},
        new TWGS84Point{Lat = 36.2.LatDegreesToRadians(), Lon = -115.1.LonDegreesToRadians()},
        new TWGS84Point{Lat = 36.3.LatDegreesToRadians(), Lon = -115.2.LonDegreesToRadians()},
        new TWGS84Point{Lat = 36.3.LatDegreesToRadians(), Lon = -115.3.LonDegreesToRadians()},
        new TWGS84Point{Lat = 36.2.LatDegreesToRadians(), Lon = -115.3.LonDegreesToRadians()},
        new TWGS84Point{Lat = 36.2.LatDegreesToRadians(), Lon = -115.2.LonDegreesToRadians()},
        new TWGS84Point{Lat = 36.1.LatDegreesToRadians(), Lon = -115.1.LonDegreesToRadians()}
      };

      raptorClient
        .Setup(x => x.GetDesignFilterBoundaryAsPolygon(It.IsAny<TDesignProfilerServiceRPCVerb_ComputeDesignFilterBoundary_Args>(),
          out fence))
        .Returns(true);

      var service = new BoundingBoxService(serviceProvider.GetRequiredService<ILoggerFactory>()
#if RAPTOR
        , raptorClient.Object
#endif
        , new Mock<IConfigurationStore>().Object
        , new Mock<ITRexCompactionDataProxy>().Object, new Mock<IFileImportProxy>().Object
      );
      var bbox = await service.GetBoundingBox(project, filterResult, new[] { TileOverlayType.BaseMap }, null, null, null, null, null);
      Assert.AreEqual(-115.3.LonDegreesToRadians(), bbox.minLng);
      Assert.AreEqual(36.1.LatDegreesToRadians(), bbox.minLat);
      Assert.AreEqual(-115.1.LonDegreesToRadians(), bbox.maxLng);
      Assert.AreEqual(36.3.LatDegreesToRadians(), bbox.maxLat);
    }

    [TestMethod]
    public async Task GetBoundingBoxSummaryVolumesFilter()
    {
      var polygonPoints1 = new List<WGSPoint>
      {
        new WGSPoint(10, 20),
        new WGSPoint(15, 20),
        new WGSPoint(13, 15),
        new WGSPoint(25, 30),
        new WGSPoint(27, 27)
      };

      var baseFilterResult = FilterResult.CreateFilterObsolete(polygonLL: polygonPoints1);

      var polygonPoints2 = new List<WGSPoint>
      {
        new WGSPoint(30, 20),
        new WGSPoint(25, 25),
        new WGSPoint(50, 35),
        new WGSPoint(25, 15),
        new WGSPoint(32, 16)
      };

      var topFilterResult = FilterResult.CreateFilterObsolete(polygonLL: polygonPoints2);

      var raptorClient = new Mock<IASNodeClient>();

      var service = new BoundingBoxService(serviceProvider.GetRequiredService<ILoggerFactory>()
#if RAPTOR
        , raptorClient.Object
#endif
        , new Mock<IConfigurationStore>().Object
        , new Mock<ITRexCompactionDataProxy>().Object, new Mock<IFileImportProxy>().Object
      );
      var bbox = await service.GetBoundingBox(project, null, new[] { TileOverlayType.BaseMap }, baseFilterResult, topFilterResult, null, null, null);

      var expectedMinLat = Math.Min(polygonPoints1.Min(p => p.Lat), polygonPoints2.Min(p => p.Lat));
      var expectedMinLng = Math.Min(polygonPoints1.Min(p => p.Lon), polygonPoints2.Min(p => p.Lon));
      var expectedMaxLat = Math.Max(polygonPoints1.Max(p => p.Lat), polygonPoints2.Max(p => p.Lat));
      var expectedMaxLng = Math.Max(polygonPoints1.Max(p => p.Lon), polygonPoints2.Max(p => p.Lon));
      Assert.AreEqual(expectedMinLat, bbox.minLat);
      Assert.AreEqual(expectedMinLng, bbox.minLng);
      Assert.AreEqual(expectedMaxLat, bbox.maxLat);
      Assert.AreEqual(expectedMaxLng, bbox.maxLng);
    }

    [TestMethod]
    public async Task GetBoundingBoxValidProductionDataExtents()
    {
      //Production data inside project boundary is valid
      var prodDataMinLat = projMinLatRadians + 0.01;
      var prodDataMinLng = projMinLngRadians + 0.01;
      var prodDataMaxLat = projMaxLatRadians - 0.01;
      var prodDataMaxLng = projMaxLngRadians - 0.01;

      var raptorClient = new Mock<IASNodeClient>();

      var statistics = new TICDataModelStatistics();

      raptorClient
        .Setup(x => x.GetDataModelStatistics(project.LegacyProjectId, It.IsAny<TSurveyedSurfaceID[]>(), out statistics))
        .Returns(true);

      var pointList = new TCoordPointList
      {
        ReturnCode = TCoordReturnCode.nercNoError,
        Points = new TCoordContainer
        {
          Coords = new[]
          {
            new TCoordPoint {X = prodDataMinLng, Y = prodDataMinLat},
            new TCoordPoint {X = prodDataMaxLng, Y = prodDataMaxLat}
          }
        }
      };

      raptorClient
        .Setup(x => x.GetGridCoordinates(project.LegacyProjectId, It.IsAny<TWGS84FenceContainer>(),
          TCoordConversionType.ctNEEtoLLH, out pointList))
        .Returns(TCoordReturnCode.nercNoError);

      var service = new BoundingBoxService(serviceProvider.GetRequiredService<ILoggerFactory>()
#if RAPTOR
        , raptorClient.Object
#endif
        , new Mock<IConfigurationStore>().Object
        , new Mock<ITRexCompactionDataProxy>().Object, new Mock<IFileImportProxy>().Object
      );
      var bbox = await service.GetBoundingBox(project, null, new[] { TileOverlayType.ProductionData }, null, null, null, null, null);
      Assert.AreEqual(prodDataMinLat, bbox.minLat);
      Assert.AreEqual(prodDataMaxLat, bbox.maxLat);
      Assert.AreEqual(prodDataMinLng, bbox.minLng);
      Assert.AreEqual(prodDataMaxLng, bbox.maxLng);
    }

    [TestMethod]
    public async Task GetBoundingBoxInvalidProductionDataExtents()
    {
      //Production data outside project boundary is invalid
      var prodDataMinLat = projMinLatRadians - 0.2;
      var prodDataMinLng = projMinLngRadians - 0.2;
      var prodDataMaxLat = projMaxLatRadians + 0.2;
      var prodDataMaxLng = projMaxLngRadians + 0.2;

      var raptorClient = new Mock<IASNodeClient>();

      var statistics = new TICDataModelStatistics();

      raptorClient
        .Setup(x => x.GetDataModelStatistics(project.LegacyProjectId, It.IsAny<TSurveyedSurfaceID[]>(), out statistics))
        .Returns(true);

      var pointList = new TCoordPointList
      {
        ReturnCode = TCoordReturnCode.nercNoError,
        Points = new TCoordContainer
        {
          Coords = new[]
          {
            new TCoordPoint {X = prodDataMinLng, Y = prodDataMinLat},
            new TCoordPoint {X = prodDataMaxLng, Y = prodDataMaxLat}
          }
        }
      };

      raptorClient
        .Setup(x => x.GetGridCoordinates(project.LegacyProjectId, It.IsAny<TWGS84FenceContainer>(),
          TCoordConversionType.ctNEEtoLLH, out pointList))
        .Returns(TCoordReturnCode.nercNoError);

      var service = new BoundingBoxService(serviceProvider.GetRequiredService<ILoggerFactory>()
#if RAPTOR
        , raptorClient.Object
#endif
        , new Mock<IConfigurationStore>().Object
        , new Mock<ITRexCompactionDataProxy>().Object, new Mock<IFileImportProxy>().Object
      );
      var bbox = await service.GetBoundingBox(project, null, new[] { TileOverlayType.ProductionData }, null, null, null, null, null);
      Assert.AreEqual(projMinLatRadians, bbox.minLat);
      Assert.AreEqual(projMaxLatRadians, bbox.maxLat);
      Assert.AreEqual(projMinLngRadians, bbox.minLng);
      Assert.AreEqual(projMaxLngRadians, bbox.maxLng);
    }

    [TestMethod]
    public async Task GetBoundingBoxProjectExtentsNoMode()
    {
      var raptorClient = new Mock<IASNodeClient>();

      TICDataModelStatistics statistics;
      raptorClient
        .Setup(x => x.GetDataModelStatistics(project.LegacyProjectId, It.IsAny<TSurveyedSurfaceID[]>(), out statistics))
        .Returns(false);

      var service = new BoundingBoxService(serviceProvider.GetRequiredService<ILoggerFactory>()
#if RAPTOR
        , raptorClient.Object
#endif
        , new Mock<IConfigurationStore>().Object
        , new Mock<ITRexCompactionDataProxy>().Object, new Mock<IFileImportProxy>().Object
      );
      var bbox = await service.GetBoundingBox(project, null, new[] { TileOverlayType.ProjectBoundary }, null, null, null, null, null);
      Assert.AreEqual(projMinLatRadians, bbox.minLat);
      Assert.AreEqual(projMaxLatRadians, bbox.maxLat);
      Assert.AreEqual(projMinLngRadians, bbox.minLng);
      Assert.AreEqual(projMaxLngRadians, bbox.maxLng);
    }

    [TestMethod]
    public async Task GetBoundingBoxProjectExtentsWithMode()
    {
      var raptorClient = new Mock<IASNodeClient>();

      var statistics = new TICDataModelStatistics();
      raptorClient
        .Setup(x => x.GetDataModelStatistics(project.LegacyProjectId, It.IsAny<TSurveyedSurfaceID[]>(), out statistics))
        .Returns(false);

      var service = new BoundingBoxService(serviceProvider.GetRequiredService<ILoggerFactory>()
#if RAPTOR
        , raptorClient.Object
#endif
        , new Mock<IConfigurationStore>().Object
        , new Mock<ITRexCompactionDataProxy>().Object, new Mock<IFileImportProxy>().Object
      );
      var bbox = await service.GetBoundingBox(project, null, new[] { TileOverlayType.ProductionData, TileOverlayType.ProjectBoundary }, null, null, null, null, null);
      Assert.AreEqual(projMinLatRadians, bbox.minLat);
      Assert.AreEqual(projMaxLatRadians, bbox.maxLat);
      Assert.AreEqual(projMinLngRadians, bbox.minLng);
      Assert.AreEqual(projMaxLngRadians, bbox.maxLng);
    }


    private static readonly List<Point> projectPoints = new List<Point>
    {
      new Point {y = 36.208, x = -115.018},
      new Point {y = 36.145, x = -115.665},
      new Point {y = 36.877, x = -115.109},
      new Point {y = 36.103, x = -115.687}
    };

    private static readonly ProjectData project = new ProjectData
    {
      ProjectUid = Guid.NewGuid().ToString(),
      LegacyProjectId = 1234,
      ProjectGeofenceWKT = TestUtils.GetWicketFromPoints(projectPoints)
    };

    private static readonly double projMinLatRadians = projectPoints.Min(p => p.Latitude).LatDegreesToRadians();
    private static readonly double projMinLngRadians = projectPoints.Min(p => p.Longitude).LonDegreesToRadians();
    private static readonly double projMaxLatRadians = projectPoints.Max(p => p.Latitude).LatDegreesToRadians();
    private static readonly double projMaxLngRadians = projectPoints.Max(p => p.Longitude).LonDegreesToRadians();

    private const string DESIGN_GEO_JSON = @"
      {
        ""type"": ""FeatureCollection"",
        ""features"": [
          {
            ""type"": ""Feature"",
            ""geometry"": {
              ""type"": ""Polygon"",
              ""coordinates"": [
                [
                  [
                    -115.018,
                    36.208
                  ],
                  [
                    -115.025,
                    36.214
                  ],
                  [
                    -115.123,
                    36.175
                  ],
				          [
                    -115.018,
                    36.208
                  ]
                ]
              ]
            },
            ""properties"": {
              ""name"": ""Acme Design.TTM""
            }
          }
        ]
		  }
    ";
  }
}
