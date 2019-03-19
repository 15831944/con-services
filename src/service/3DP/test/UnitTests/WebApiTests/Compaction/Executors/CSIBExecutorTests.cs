﻿using System;
using System.IO;
#if RAPTOR
using ASNodeDecls;
#endif
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.Executors;

namespace VSS.Productivity3D.WebApiTests.Compaction.Executors
{
  [TestClass]
  public class CSIBExecutorTests
  {
    private static IServiceProvider _serviceProvider;
    private static ILoggerFactory _logger;

    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();

      var serviceCollection = new ServiceCollection();
      serviceCollection.AddLogging();
      serviceCollection.AddSingleton(loggerFactory);
      serviceCollection.AddSingleton<IConfigurationStore, GenericConfiguration>();

      serviceCollection.AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()
#if RAPTOR
        .AddTransient<IErrorCodesProvider, RaptorResult>()
#endif
  ;
      _serviceProvider = serviceCollection.BuildServiceProvider();

      _logger = _serviceProvider.GetRequiredService<ILoggerFactory>();
    }

#if RAPTOR
    [TestMethod]
    public void RequestCSIBForProject_no_result()
    {
      var request = new ProjectID
        {
          ProjectUid = Guid.NewGuid(),
          ProjectId =  987654321
      };
      
      MemoryStream responseData;

      var raptorClient = new Mock<IASNodeClient>();

      raptorClient.Setup(x => x.GetCSIBFile(request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID, out responseData))
                  .Returns(TASNodeErrorStatus.asneUnknown);

      var executor = RequestExecutorContainerFactory.Build<CSIBExecutor>(_logger, raptorClient.Object);

      var result = executor.Process(request);

      Assert.AreEqual(result.Code, (int)TASNodeErrorStatus.asneUnknown);
      Assert.AreEqual(result.Message, $"RequestCSIBForProject: result: {TASNodeErrorStatus.asneUnknown}");
    }
#endif
  }
}