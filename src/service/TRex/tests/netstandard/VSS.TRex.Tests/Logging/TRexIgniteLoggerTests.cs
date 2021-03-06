﻿using System;
using FluentAssertions;
using Moq;
using VSS.Common.Abstractions.Configuration;
using VSS.TRex.Logging;
using VSS.TRex.Tests.TestFixtures;
using Xunit;
using LogLevel = Apache.Ignite.Core.Log.LogLevel;

namespace VSS.TRex.Tests.Logging
{
  public class TRexIgniteLoggerTests : IClassFixture<DILoggingFixture>
  {


    [Fact]
    public void Creation()
    {
      var log = new TRexIgniteLogger(new Mock<IConfigurationStore>().Object, Logger.CreateLogger("UnitTests"));

      log.Should().NotBeNull();
    }

    [Fact]
    public void IsEnabled()
    {
      var config = new Mock<IConfigurationStore>();
      config
        .Setup(m => m.GetValueInt(It.Is<string>(s => s == "TREX_IGNITE_MIN_LOG_LEVEL"), It.IsAny<int>()))
        .Returns<string, int>((_, i) => i);

      var log = new TRexIgniteLogger(config.Object, Logger.CreateLogger("UnitTests"));

      log.IsEnabled(LogLevel.Trace).Should().BeFalse();
      log.IsEnabled(LogLevel.Debug).Should().BeFalse();
      log.IsEnabled(LogLevel.Info).Should().BeTrue();
    }

    [Fact]
    public void ConfigIsEnabled()
    {
      var config = new Mock<IConfigurationStore>();
      config
        .Setup(m => m.GetValueInt(It.Is<string>(s => s == "TREX_IGNITE_MIN_LOG_LEVEL"), It.IsAny<int>()))
        .Returns((int) LogLevel.Trace);

      var log = new TRexIgniteLogger(new Mock<IConfigurationStore>().Object, Logger.CreateLogger("UnitTests"));

      log.IsEnabled(LogLevel.Trace).Should().BeTrue();
      log.IsEnabled(LogLevel.Debug).Should().BeTrue();
      log.IsEnabled(LogLevel.Info).Should().BeTrue();
    }

    [Fact]
    public void Log()
    {
      var log = new TRexIgniteLogger(new Mock<IConfigurationStore>().Object, Logger.CreateLogger("UnitTests"));

      log.Log(LogLevel.Info, "Informative Message", null, null, "Category", string.Empty, null);
    }

    [Fact]
    public void ConvertLogLevel2()
    {
      TRexIgniteLogger.ConvertLogLevel2(LogLevel.Trace).Should().Be(Microsoft.Extensions.Logging.LogLevel.Trace);
      TRexIgniteLogger.ConvertLogLevel2(LogLevel.Debug).Should().Be(Microsoft.Extensions.Logging.LogLevel.Debug);
      TRexIgniteLogger.ConvertLogLevel2(LogLevel.Warn).Should().Be(Microsoft.Extensions.Logging.LogLevel.Warning);
      TRexIgniteLogger.ConvertLogLevel2(LogLevel.Info).Should().Be(Microsoft.Extensions.Logging.LogLevel.Information);
      TRexIgniteLogger.ConvertLogLevel2(LogLevel.Error).Should().Be(Microsoft.Extensions.Logging.LogLevel.Error);

      Action act = () => TRexIgniteLogger.ConvertLogLevel2((LogLevel) 100);
      act.Should().Throw<ArgumentOutOfRangeException>();
    }
  }
}
