﻿using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using VSS.Common.Abstractions.Configuration;
using VSS.ConfigurationStore;
using VSS.Serilog.Extensions;
using Microsoft.AspNetCore.TestHost;

namespace VSS.MasterData.Proxies.UnitTests
{
  public class MemoryCacheTestsFixture : IDisposable
  {
    public IServiceProvider serviceProvider;

    public MemoryCacheTestsFixture()
    {
      serviceProvider = new ServiceCollection()
                        .AddLogging()
                        .AddSingleton(new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.MasterData.Proxies.UnitTests.log")))
                        .AddSingleton<IConfigurationStore, GenericConfiguration>()
                        .AddTransient<IMemoryCache, MemoryCache>()
                        .AddHttpClient()
                        .BuildServiceProvider();
    }

    public void Dispose()
    { }
  }
}
