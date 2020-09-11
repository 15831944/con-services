﻿using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Serilog;
using VSS.Common.Abstractions.Configuration;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Filter.Abstractions.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.ProductionData.Executors;
using VSS.Serilog.Extensions;
using VSS.TRex.Gateway.Common.Abstractions;

namespace VSS.Productivity3D.WebApiTests.ProductionData.Controllers
{
  [TestClass]
  public class TerrainControllerTests
  {
    private static IServiceProvider serviceProvider;
    private static ILoggerFactory logger;
    private static HeaderDictionary _customHeaders;

    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
      serviceProvider = new ServiceCollection()
        .AddLogging()
        .AddSingleton(new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.Productivity3D.WebApi.Tests.log")))
        .BuildServiceProvider();

      logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      _customHeaders = new HeaderDictionary();
    }

    [TestMethod]
    public void PD_GetTerrainTile_TRex_Fail()
    {
      var resultStream = new MemoryStream(new byte[]
        {
          0x41, 0x42, 0x42, 0x41
        });

      var tRexProxy = new Mock<ITRexCompactionDataProxy>();

      var request = new QMTileRequest
      {
        X = 0,
        Y = 0,
        Z = 0,
        Filter = new FilterResult(),
        ProjectUid = Guid.Empty
      };

      // make bad call
      tRexProxy.Setup(x => x.SendDataPostRequestWithStreamResponse(
        It.Is<QMTileRequest>(r => r.ProjectUid == request.ProjectUid),
          It.Is<string>(s => s == "/terrainNoValid"),
          It.IsAny<IHeaderDictionary>()))
       .Returns(Task.FromResult<Stream>(resultStream));

      var configStore = new Mock<IConfigurationStore>();

      var executor = RequestExecutorContainerFactory
        .Build<QMTilesExecutor>(logger, configStore: configStore.Object,
          trexCompactionDataProxy: tRexProxy.Object, customHeaders: _customHeaders);

      var result = executor.ProcessAsync(request).Result as QMTileResult;

      Assert.IsNull(result, "Result should be null");

      tRexProxy.Verify(m => m.SendDataPostRequestWithStreamResponse(
        It.Is<QMTileRequest>(r => r.ProjectUid == request.ProjectUid),
          It.Is<string>(s => s == "/terrain"),
          It.IsAny<IHeaderDictionary>())
      , Times.Once);

    }

    [TestMethod]
    public void PD_GetTerrainTile_TRex_Success()
    {
      var expectedResult = new QMTileResult
      (
        new byte[] { 0x41, 0x42, 0x42, 0x41 } // may get compressed(gzip) later in part two
      );

      var resultStream = new MemoryStream(new byte[]
        {
          0x41, 0x42, 0x42, 0x41
        });

      var tRexProxy = new Mock<ITRexCompactionDataProxy>();

      var request = new QMTileRequest
      {
        X = 0,
        Y = 0,
        Z = 0,
        Filter = new FilterResult(),
        ProjectUid = Guid.NewGuid()
      };


      tRexProxy.Setup(x => x.SendDataPostRequestWithStreamResponse(
        It.Is<QMTileRequest>(r => r.ProjectUid == request.ProjectUid),
          It.Is<string>(s => s == "/terrain"),
          It.IsAny<IHeaderDictionary>()))
       .Returns(Task.FromResult<Stream>(resultStream));


      var configStore = new Mock<IConfigurationStore>();

      var executor = RequestExecutorContainerFactory
        .Build<QMTilesExecutor>(logger, configStore: configStore.Object,
          trexCompactionDataProxy: tRexProxy.Object, customHeaders: _customHeaders);


      var result = executor.ProcessAsync(request).Result as QMTileResult;

      Assert.IsNotNull(result, "Result should not be null");


      Assert.AreEqual(expectedResult.TileData.Length, result.TileData.Length, "QM Tile does not match");

      for (int i = 0; i < expectedResult.TileData.Length; i++)
      {
        Assert.IsTrue(expectedResult.TileData[i].Equals(result.TileData[i]), "QM Tile does not match");

      }

      tRexProxy.Verify(m => m.SendDataPostRequestWithStreamResponse(
        It.Is<QMTileRequest>(r => r.ProjectUid == request.ProjectUid),
          It.Is<string>(s => s == "/terrain"),
          It.IsAny<IHeaderDictionary>())
      , Times.Once);
    }
  }
}
