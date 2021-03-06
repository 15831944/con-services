﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.Common.Exceptions;
using VSS.Pegasus.Client;
using VSS.Pegasus.Client.Models;
using VSS.Productivity.Push.Models.Notifications;
using VSS.Productivity3D.Push.Abstractions.Notifications;
using VSS.Productivity3D.Push.Clients.Notifications;
using VSS.Productivity3D.Scheduler.Jobs.DxfTileJob;
using VSS.Productivity3D.Scheduler.Jobs.DxfTileJob.Models;
using VSS.Productivity3D.Scheduler.Models;
using VSS.Serilog.Extensions;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.Scheduler.Jobs.Tests
{
  [TestClass]
  public class DxfTileGenerationJobTests
  {
    private ILoggerFactory loggerFactory;

    [TestInitialize]
    public void TestInitialize()
    {
      loggerFactory = new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.Scheduler.Jobs.UnitTests"));
      var serviceCollection = new ServiceCollection();

      serviceCollection.AddLogging()
                       .AddSingleton(loggerFactory);
    }

    [TestMethod]
    public void CanSetupDxfJob() => CreateDxfJobWithMocks().Setup(null,null);

    [TestMethod]
    public void CanTearDownDxfJob() => CreateDxfJobWithMocks().TearDown(null, null);

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task CanRunDxfJobSuccess(bool enableDxfTileGeneration)
    {
      var request = new DxfTileGenerationRequest
      {
        CustomerUid = Guid.NewGuid(),
        ProjectUid = Guid.NewGuid(),
        ImportedFileUid = Guid.NewGuid(),
        DataOceanRootFolder = "some folder",
        FileName = "a dxf file",
        DcFileName = "a dc file",
        DxfUnitsType = DxfUnitsType.Meters
      };

      var obj = JObject.Parse(JsonConvert.SerializeObject(request));
      var configStore = new Mock<IConfigurationStore>();

      configStore.Setup(x => x.GetValueBool("SCHEDULER_ENABLE_DXF_TILE_GENERATION"))
                 .Returns(enableDxfTileGeneration);

      var mockPegasus = new Mock<IPegasusClient>();

      mockPegasus.Setup(x => x.GenerateDxfTiles(
                           It.IsAny<string>(),
                           It.IsAny<string>(),
                           DxfUnitsType.Meters,
                           It.IsAny<IHeaderDictionary>(), It.IsAny<Action<IHeaderDictionary>>() ))
                 .ReturnsAsync(new TileMetadata());

      var mockNotification = new Mock<INotificationHubClient>();

      mockNotification.Setup(n => n.Notify(It.IsAny<ProjectFileRasterTilesGeneratedNotification>()))
                      .Returns(Task.FromResult(default(object)));

      var mockTPaaSAuth = new Mock<ITPaaSApplicationAuthentication>();

      mockTPaaSAuth.Setup(t => t.GetApplicationBearerToken())
                   .Returns("this is a dummy bearer token");

      var job = new DxfTileGenerationJob(configStore.Object, mockPegasus.Object, mockTPaaSAuth.Object, mockNotification.Object, loggerFactory);

      await job.Run(obj, null);

      var runTimes = enableDxfTileGeneration ? Times.Once() : Times.Never();

      // Verify based on the value of SCHEDULER_ENABLE_DXF_TILE_GENERATION the execution of GenerateDxfTiles().
      mockPegasus.Verify(x => x.GenerateDxfTiles(
                           It.IsAny<string>(),
                           It.IsAny<string>(),
                           DxfUnitsType.Meters,
                           It.IsAny<IHeaderDictionary>(), It.IsAny<Action<IHeaderDictionary>>()), runTimes);
    }

    [TestMethod]
    public async Task CanRunDxfJobFailureMissingRequest() => await Assert.ThrowsExceptionAsync<ServiceException>(() => CreateDxfJobWithMocks().Run(null, null));

    [TestMethod]
    public async Task CanRunDxfJobFailureWrongRequest()
    {
      var obj = JObject.Parse(JsonConvert.SerializeObject(new JobRequest())); //any model which is not DxfTileGenerationRequest

      await Assert.ThrowsExceptionAsync<ServiceException>(() => CreateDxfJobWithMocks().Run(obj, null));
    }

    private DxfTileGenerationJob CreateDxfJobWithMocks()
    {
      var configStore = new Mock<IConfigurationStore>();
      var mockPegasus = new Mock<IPegasusClient>();
      var mockTPaaSAuth = new Mock<ITPaaSApplicationAuthentication>();
      var mockProvider = new Mock<IServiceProvider>();
      var mockConfig = new Mock<IConfigurationStore>();
      var mockPushProxy = new Mock<IServiceResolution>();
      var mockDataCache = new Mock<IDataCache>();
      var mockNotification = new Mock<NotificationHubClient>(mockProvider.Object, mockConfig.Object, mockPushProxy.Object, mockDataCache.Object, loggerFactory);

      return new DxfTileGenerationJob(configStore.Object, mockPegasus.Object, mockTPaaSAuth.Object, mockNotification.Object, loggerFactory);
    }
  }
}
