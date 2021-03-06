﻿using Apache.Ignite.Core;
using Apache.Ignite.Core.Communication.Tcp;
using Apache.Ignite.Core.Discovery.Tcp;
using Apache.Ignite.Core.Discovery.Tcp.Static;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Deployment;
using VSS.Common.Abstractions.Configuration;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.Logging;
using VSS.TRex.Storage.Models;
using VSS.TRex.Common.Serialisation;
using VSS.TRex.DI;

namespace VSS.TRex.GridFabric.Servers.Client
{
  /// <summary>
  /// Defines a representation of a client able to request TRex related compute operations using
  /// the Ignite In Memory Data Grid. All client type server classes should descend from this class.
  /// </summary>
  public class MutableClientServer : IgniteServer, IMutableClientServer
  {
    private static readonly ILogger _log = Logger.CreateLogger<MutableClientServer>();

    /// <summary>
    /// Constructor that creates a new server instance with a single role
    /// </summary>
    /// <param name="role"></param>
    public MutableClientServer(string role) : this(new[] { role })
    {
    }

    /// <summary>
    /// Constructor that creates a new server instance with a set of roles
    /// </summary>
    /// <param name="roles"></param>
    public MutableClientServer(string[] roles)
    {
      if (mutableTRexGrid == null)
      {
        // Attempt to attach to an already existing Ignite instance
        _log.LogInformation("Getting mutable grid");
        mutableTRexGrid = DIContext.Obtain<ITRexGridFactory>()?.Grid(StorageMutability.Mutable);
        _log.LogInformation($"Got {mutableTRexGrid?.Name}");

        // If there was no connection obtained, attempt to create a new instance
        if (mutableTRexGrid == null)
        {
          var roleNames = roles.Aggregate("|", (s1, s2) => s1 + s2 + "|");

          TRexNodeID = Guid.NewGuid();

          _log.LogInformation($"Creating new Ignite node with Roles = {roleNames} & TRexNodeId = {TRexNodeID}");

          var cfg = new IgniteConfiguration()
          {
            IgniteInstanceName = TRexGrids.MutableGridName(),
            ClientMode = true,

            JvmOptions = new List<string>() {
              "-DIGNITE_QUIET=false",
              "-Djava.net.preferIPv4Stack=true",
              "-XX:+UseG1GC",
              "--add-exports=java.base/jdk.internal.misc=ALL-UNNAMED",
              "--add-exports=java.base/sun.nio.ch=ALL-UNNAMED",
              "--add-exports=java.management/com.sun.jmx.mbeanserver=ALL-UNNAMED",
              "--add-exports=jdk.internal.jvmstat/sun.jvmstat.monitor=ALL-UNNAMED",
              "--add-exports=java.base/sun.reflect.generics.reflectiveObjects=ALL-UNNAMED",
              "--illegal-access=permit"
            },

            JvmMaxMemoryMb = DIContext.Obtain<IConfigurationStore>().GetValueInt(IGNITE_JVM_MAX_HEAP_SIZE_MB, DEFAULT_IGNITE_JVM_MAX_HEAP_SIZE_MB),
            JvmInitialMemoryMb = DIContext.Obtain<IConfigurationStore>().GetValueInt(IGNITE_JVM_INITIAL_HEAP_SIZE_MB, DEFAULT_IGNITE_JVM_INITIAL_HEAP_SIZE_MB),

              UserAttributes = new Dictionary<string, object>()
                        {
                            { "TRexNodeId", TRexNodeID.ToString() }
                        },

            Logger = new TRexIgniteLogger(DIContext.Obtain<IConfigurationStore>(), Logger.CreateLogger("MutableClientServer")),

            // Set an Ignite metrics heartbeat of 10 seconds
            MetricsLogFrequency = new TimeSpan(0, 0, 0, 10),

            PublicThreadPoolSize = DIContext.Obtain<IConfigurationStore>().GetValueInt(IGNITE_PUBLIC_THREAD_POOL_SIZE, DEFAULT_IGNITE_PUBLIC_THREAD_POOL_SIZE),
            SystemThreadPoolSize = DIContext.Obtain<IConfigurationStore>().GetValueInt(IGNITE_SYSTEM_THREAD_POOL_SIZE, DEFAULT_IGNITE_SYSTEM_THREAD_POOL_SIZE),

            PeerAssemblyLoadingMode = PeerAssemblyLoadingMode.CurrentAppDomain,

            BinaryConfiguration = new BinaryConfiguration
            {
              Serializer = new BinarizableSerializer()
            }
          };

          foreach (var roleName in roles)
          {
            cfg.UserAttributes.Add($"{ServerRoles.ROLE_ATTRIBUTE_NAME}-{roleName}", "True");
          }

          bool.TryParse(Environment.GetEnvironmentVariable("IS_KUBERNETES"), out var isKubernetes);
          cfg = isKubernetes ? setKubernetesIgniteConfiguration(cfg) : setLocalIgniteConfiguration(cfg);

          try
          {
            base.ConfigureTRexGrid(cfg);
            mutableTRexGrid = DIContext.Obtain<ITRexGridFactory>()?.Grid(TRexGrids.MutableGridName(), cfg);
          }
          catch (Exception e)
          {
            _log.LogError(e, $"Creation of new Ignite node with Role = {roleNames} & TRexNodeId = {TRexNodeID} failed with Exception:");
          }
          finally
          {
            _log.LogInformation($"Completed creation of new Ignite node with Role = {roleNames} & TRexNodeId = {TRexNodeID}");
          }
        }
      }
    }

    private IgniteConfiguration setKubernetesIgniteConfiguration(IgniteConfiguration cfg)
    {
      cfg.SpringConfigUrl = @".\igniteMutableKubeConfig.xml";
      cfg.JvmOptions.Add("-javaagent:./libs/jmx_prometheus_javaagent-0.12.0.jar=8088:prometheusConfig.yaml");

      cfg.CommunicationSpi = new TcpCommunicationSpi()
      {
        LocalPort = 48100,
      };
      return cfg;
    }

    private IgniteConfiguration setLocalIgniteConfiguration(IgniteConfiguration cfg)
    {
      // Enforce using only the LocalHost interface
      cfg.DiscoverySpi = new TcpDiscoverySpi
      {
        LocalAddress = "127.0.0.1",
        LocalPort = 48500,

        IpFinder = new TcpDiscoveryStaticIpFinder
        {
          Endpoints = new[] { "127.0.0.1:48500..48502" }
        }
      };

      cfg.CommunicationSpi = new TcpCommunicationSpi
      {
        LocalAddress = "127.0.0.1",
        LocalPort = 48100,
      };
      return cfg;
    }
  }
}
