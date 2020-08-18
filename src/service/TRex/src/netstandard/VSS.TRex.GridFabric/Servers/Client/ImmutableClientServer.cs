﻿using Apache.Ignite.Core;
using Apache.Ignite.Core.Communication.Tcp;
using Apache.Ignite.Core.Discovery.Tcp;
using Apache.Ignite.Core.Discovery.Tcp.Static;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Configuration;
using Apache.Ignite.Core.Deployment;
using VSS.Common.Abstractions.Configuration;
using VSS.TRex.Logging;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.Storage.Models;
using VSS.TRex.Common.Serialisation;
using VSS.TRex.DI;

namespace VSS.TRex.GridFabric.Servers.Client
{
  /// <summary>
  /// Defines a representation of a client able to request TRex related compute operations using
  /// the Ignite In Memory Data Grid. All client type server classes should descend from this class.
  /// </summary>
  public class ImmutableClientServer : IgniteServer, IImmutableClientServer
  {
    private static readonly ILogger Log = Logger.CreateLogger<ImmutableClientServer>();

    /// <summary>
    /// Constructor that creates a new server instance with a single role
    /// </summary>
    /// <param name="role"></param>
    public ImmutableClientServer(string role) : this(new[] { role })
    {
    }

    /// <summary>
    /// Constructor that creates a new server instance with a set of roles
    /// </summary>
    /// <param name="roles"></param>
    public ImmutableClientServer(string[] roles)
    {
      if (immutableTRexGrid == null)
      {
        // Attempt to attach to an already existing Ignite instance
        Log.LogInformation("Getting Immutable grid");
        immutableTRexGrid = DIContext.Obtain<ITRexGridFactory>()?.Grid(StorageMutability.Immutable);
        Log.LogInformation($"Got {immutableTRexGrid?.Name}");

        // If there was no connection obtained, attempt to create a new instance
        if (immutableTRexGrid == null)
        {
          var roleNames = roles.Aggregate("|", (s1, s2) => s1 + s2 + "|");

          TRexNodeID = Guid.NewGuid();

          //Log.LogInformation($"Creating new Ignite node with Roles = {roleNames} & TRexNodeId = {TRexNodeID}");

          var cfg = new IgniteConfiguration
          {
            IgniteInstanceName = TRexGrids.ImmutableGridName(),
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

            Logger = new TRexIgniteLogger(DIContext.Obtain<IConfigurationStore>(), Logger.CreateLogger("ImmutableClientServer")),

            // Set an Ignite metrics heartbeat of 10 seconds
            MetricsLogFrequency = new TimeSpan(0, 0, 0, 10),

            PublicThreadPoolSize = DIContext.Obtain<IConfigurationStore>().GetValueInt(IGNITE_PUBLIC_THREAD_POOL_SIZE, DEFAULT_IGNITE_PUBLIC_THREAD_POOL_SIZE),
            SystemThreadPoolSize = DIContext.Obtain<IConfigurationStore>().GetValueInt(IGNITE_SYSTEM_THREAD_POOL_SIZE, DEFAULT_IGNITE_SYSTEM_THREAD_POOL_SIZE),

            PeerAssemblyLoadingMode = PeerAssemblyLoadingMode.CurrentAppDomain,

            BinaryConfiguration = new BinaryConfiguration
            {
              Serializer = new BinarizableSerializer()
            },

            // Add the TRex progressive request custom thread pool
            ExecutorConfiguration = new List<ExecutorConfiguration>
            {
              new ExecutorConfiguration
              {
                Name = BaseIgniteClass.TREX_PROGRESSIVE_QUERY_CUSTOM_THREAD_POOL_NAME,
                Size = DIContext.Obtain<IConfigurationStore>().GetValueInt(PROGRESSIVE_REQUEST_CUSTOM_POOL_SIZE, DEFAULT_PROGRESSIVE_REQUEST_CUSTOM_POOL_SIZE)
              }
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
            immutableTRexGrid = DIContext.Obtain<ITRexGridFactory>()?.Grid(TRexGrids.ImmutableGridName(), cfg);
          }
          catch (Exception e)
          {
            Log.LogError(e, $"Creation of new Ignite node with Role = {roleNames} & TRexNodeId = {TRexNodeID} failed with Exception:");
            throw;
          }
          finally
          {
            Log.LogInformation($"Completed creation of new Ignite node with Role = {roleNames} & TRexNodeId = {TRexNodeID}");
          }
        }
      }
    }

    private IgniteConfiguration setKubernetesIgniteConfiguration(IgniteConfiguration cfg)
    {
      cfg.SpringConfigUrl = @".\igniteKubeConfig.xml";
      cfg.JvmOptions.Add("-javaagent:./libs/jmx_prometheus_javaagent-0.12.0.jar=8088:prometheusConfig.yaml");

      cfg.CommunicationSpi = new TcpCommunicationSpi()
      {
        LocalPort = 47100,
      };
      return cfg;
    }

    /// <summary>
    /// Configures ignite for use locally i.e on developers pc
    /// </summary>
    /// <param name="cfg">Ignite configuration that is being built</param>
    /// <returns></returns>
    private IgniteConfiguration setLocalIgniteConfiguration(IgniteConfiguration cfg)
    {
      // Enforce using only the LocalHost interface
      cfg.DiscoverySpi = new TcpDiscoverySpi()
      {
        LocalAddress = "127.0.0.1",
        LocalPort = 47500,

        IpFinder = new TcpDiscoveryStaticIpFinder()
        {
          Endpoints = new[] { "127.0.0.1:47500..47502" }
        }
      };

      cfg.CommunicationSpi = new TcpCommunicationSpi()
      {
        LocalAddress = "127.0.0.1",
        LocalPort = 47100,
      };
      return cfg;
    }
  }
}
