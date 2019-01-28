﻿using System;
using Microsoft.Extensions.DependencyInjection;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.TRex.DI;

namespace VSS.TRex.Tests.CoordinateSystem
{
  public class CoordinatesAPIClientTestDIFixture : IDisposable
  {
    public CoordinatesAPIClientTestDIFixture()
    {
      DIBuilder
        .New()
        .AddLogging()
        .Add(x => x.AddSingleton<IConfigurationStore, GenericConfiguration>())
        .Add(x => x.AddSingleton<ITPaasProxy, TPaasProxy>())
        .Complete();
    }

    public void Dispose()
    {
      DIBuilder.Eject();
    }
  }
}
