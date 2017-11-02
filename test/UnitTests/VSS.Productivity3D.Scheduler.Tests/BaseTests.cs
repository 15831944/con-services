﻿using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.ConfigurationStore;
using VSS.Log4Net.Extensions;

namespace VSS.Productivity3D.Scheduler.Tests
{
  [TestClass]
  public class BaseTests
  {
    protected IServiceProvider serviceProvider;
    protected IConfigurationStore _configStore;
    protected ILoggerFactory _logger;

    [TestInitialize]
    public virtual void InitTest()
    {
      var serviceCollection = new ServiceCollection();

      const string loggerRepoName = "UnitTestLogTest";
      var logPath = Directory.GetCurrentDirectory();
      Log4NetAspExtensions.ConfigureLog4Net(logPath, "log4nettest.xml", loggerRepoName);

      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(loggerRepoName);

      serviceCollection.AddLogging();
      serviceCollection.AddSingleton(loggerFactory);
      serviceCollection.AddSingleton<IConfigurationStore, GenericConfiguration>();
      serviceProvider = serviceCollection.BuildServiceProvider();

      _logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      _configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      //_configStore = new GenericConfiguration(_logger);
    }
  }
}
