﻿using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.MapHandling;

namespace VSS.Productivity3D.WebApiTests.Compaction
{
  [TestClass]
  public class MapOverlayTests
  {
    private static IServiceProvider serviceProvider;
    private static ILoggerFactory logger;

    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();

      var serviceCollection = new ServiceCollection();
      serviceCollection.AddLogging();
      serviceCollection.AddSingleton(loggerFactory);
      serviceCollection
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()
        .AddTransient<IErrorCodesProvider, ErrorCodesProvider>()
        .AddTransient<IMemoryCache,MemoryCache>()
        .AddTransient<IMapTileService, MapTileService>();

      serviceProvider = serviceCollection.BuildServiceProvider();

      logger = serviceProvider.GetRequiredService<ILoggerFactory>();
    }

    [TestMethod]
    public void CanResolveRegion()
    {
      var region = serviceProvider.GetRequiredService<IMapTileService>().GetRegion(-43, 172);
      Assert.AreEqual("OC",region);
    }
  }
}
