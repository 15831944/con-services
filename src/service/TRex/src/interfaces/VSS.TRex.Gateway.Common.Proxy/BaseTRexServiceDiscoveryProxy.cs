﻿using System;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Constants;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.TRex.Gateway.Common.Proxy
{
  /// <summary>
  /// Service Discovery enabled Proxy
  /// For now, we inherit from the BaseProxy to get code related to caching
  /// But we should create brand new fetch methods than don't accept URL values
  /// As these should be 'resolved' by the Service Resolution class
  /// </summary>
  public abstract class BaseTRexServiceDiscoveryProxy : BaseServiceDiscoveryProxy
  {
    private readonly IServiceResolution _serviceResolution;

    protected BaseTRexServiceDiscoveryProxy(IWebRequest webRequest, IConfigurationStore configurationStore,
      ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(webRequest, configurationStore, logger, dataCache, serviceResolution)
    {
      _serviceResolution = serviceResolution;
    }

    /// <summary>
    /// The Type of gateway this service is for, so the service-name includes it e.g. trex-gateway; trex-mutable-gateway; trex-connectedSite-gateway
    /// </summary>
    protected GatewayType Gateway { get; set; } = GatewayType.None;

    protected override string GetServiceName() => Gateway switch
    {
      GatewayType.Immutable => ServiceNameConstants.TREX_SERVICE_IMMUTABLE,
      GatewayType.Mutable => ServiceNameConstants.TREX_SERVICE_MUTABLE,
      _ => throw new ArgumentOutOfRangeException("Trex", Gateway, null)
    };
  }
}
