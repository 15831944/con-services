﻿using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Communication.Tcp;
using Apache.Ignite.Core.Configuration;
using Apache.Ignite.Core.Discovery.Tcp;
using Apache.Ignite.Core.Discovery.Tcp.Static;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Deployment;
using VSS.Common.Abstractions.Configuration;
using VSS.TRex.Common;
using VSS.TRex.Common.Serialisation;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Logging;
using VSS.TRex.SiteModelChangeMaps.Interfaces.GridFabric.Queues;
using VSS.TRex.Storage.Models;

namespace VSS.TRex.GridFabric.Servers.Compute
{
  /// <summary>
  /// Defines a representation of a server responsible for performing TRex related compute operations using
  /// the Ignite In Memory Data Grid
  /// </summary>
  public class ImmutableCacheComputeServer : IgniteServer
  {
    private static readonly ILogger Log = Logger.CreateLogger<ImmutableCacheComputeServer>();
    public const string IMMUTABLE_DATA_REGION_INITIAL_SIZE_MB = "IMMUTABLE_DATA_REGION_INITIAL_SIZE_MB";
    public const long DEFAULT_IMMUTABLE_DATA_REGION_INITIAL_SIZE_MB = 128;
    public const string IMMUTABLE_DATA_REGION_MAX_SIZE_MB = "IMMUTABLE_DATA_REGION_MAX_SIZE_MB";
    public const long DEFAULT_IMMUTABLE_DATA_REGION_MAX_SIZE_MB = 1000;

    /// <summary>
    /// Constructor for the TRex cache compute server node. Responsible for starting all Ignite services and creating the grid
    /// and cache instance in preparation for client access by business logic running on the node.
    /// </summary>
    public ImmutableCacheComputeServer()
    {
      Console.WriteLine("PersistentCacheLocation:" + TRexServerConfig.PersistentCacheStoreLocation);
      Console.WriteLine($"Log is: {Log}");
      Log.LogDebug($"PersistentCacheStoreLocation: {TRexServerConfig.PersistentCacheStoreLocation}");
      if (immutableTRexGrid == null)
      {
        StartTRexGridCacheNode();
      }
    }

    public override void ConfigureTRexGrid(IgniteConfiguration cfg)
    {
      base.ConfigureTRexGrid(cfg);

      cfg.IgniteInstanceName = TRexGrids.ImmutableGridName();

      cfg.JvmOptions = new List<string>() {
        "-DIGNITE_QUIET=false",
        "-Djava.net.preferIPv4Stack=true",
        "-XX:+UseG1GC",
        "--add-exports=java.base/jdk.internal.misc=ALL-UNNAMED",
        "--add-exports=java.base/sun.nio.ch=ALL-UNNAMED",
        "--add-exports=java.management/com.sun.jmx.mbeanserver=ALL-UNNAMED",
        "--add-exports=jdk.internal.jvmstat/sun.jvmstat.monitor=ALL-UNNAMED",
        "--add-exports=java.base/sun.reflect.generics.reflectiveObjects=ALL-UNNAMED",
        "--illegal-access=permit",
        "--add-opens jdk.management/com.sun.management.internal=ALL-UNNAMED"
      };

      cfg.JvmMaxMemoryMb = DIContext.Obtain<IConfigurationStore>().GetValueInt(IGNITE_JVM_MAX_HEAP_SIZE_MB, DEFAULT_IGNITE_JVM_MAX_HEAP_SIZE_MB);
      cfg.JvmInitialMemoryMb = DIContext.Obtain<IConfigurationStore>().GetValueInt(IGNITE_JVM_INITIAL_HEAP_SIZE_MB, DEFAULT_IGNITE_JVM_INITIAL_HEAP_SIZE_MB);

      cfg.UserAttributes = new Dictionary<string, object>
            {
                { "Owner", TRexGrids.ImmutableGridName() }
            };

      // Configure the Ignite persistence layer to store our data
      cfg.DataStorageConfiguration = new DataStorageConfiguration
      {
        WalMode = WalMode.Fsync,
        PageSize = DataRegions.DEFAULT_IMMUTABLE_DATA_REGION_PAGE_SIZE,

        StoragePath = Path.Combine(TRexServerConfig.PersistentCacheStoreLocation, "Immutable", "Persistence"),
        WalPath = Path.Combine(TRexServerConfig.PersistentCacheStoreLocation, "Immutable", "WalStore"),
        WalArchivePath = Path.Combine(TRexServerConfig.PersistentCacheStoreLocation, "Immutable", "WalArchive"),

        WalSegmentSize = 512 * 1024 * 1024, // Set the WalSegmentSize to 512Mb to better support high write loads (can be set to max 2Gb)
        MaxWalArchiveSize = (long)10 * 512 * 1024 * 1024, // Ensure there are 10 segments in the WAL archive at the defined segment size

        DefaultDataRegionConfiguration = new DataRegionConfiguration
        {
          Name = DataRegions.DEFAULT_IMMUTABLE_DATA_REGION_NAME,
          InitialSize = DIContext.Obtain<IConfigurationStore>().GetValueLong(
              IMMUTABLE_DATA_REGION_INITIAL_SIZE_MB, 
              DEFAULT_IMMUTABLE_DATA_REGION_INITIAL_SIZE_MB) * 1024 * 1024,  
          MaxSize = DIContext.Obtain<IConfigurationStore>().GetValueLong(
              IMMUTABLE_DATA_REGION_MAX_SIZE_MB, 
              DEFAULT_IMMUTABLE_DATA_REGION_MAX_SIZE_MB) * 1024 * 1024,  

          PersistenceEnabled = true
        }
      };

      Log.LogInformation($"cfg.DataStorageConfiguration.StoragePath={cfg.DataStorageConfiguration.StoragePath}");
      Log.LogInformation($"cfg.DataStorageConfiguration.WalArchivePath={cfg.DataStorageConfiguration.WalArchivePath}");
      Log.LogInformation($"cfg.DataStorageConfiguration.WalPath={cfg.DataStorageConfiguration.WalPath}");

      bool.TryParse(Environment.GetEnvironmentVariable("IS_KUBERNETES"), out var isKubernetes);
      cfg = isKubernetes ? setKubernetesIgniteConfiguration(cfg) : setLocalIgniteConfiguration(cfg);
      cfg.WorkDirectory = Path.Combine(TRexServerConfig.PersistentCacheStoreLocation, "Immutable");

      cfg.Logger = new TRexIgniteLogger(Logger.CreateLogger("ImmutableCacheComputeServer"));

      // Set an Ignite metrics heartbeat of 10 seconds
      cfg.MetricsLogFrequency = new TimeSpan(0, 0, 0, 10); // TODO: This needs to be added to configuration

      cfg.PublicThreadPoolSize = DIContext.Obtain<IConfigurationStore>().GetValueInt(IGNITE_PUBLIC_THREAD_POOL_SIZE, DEFAULT_IGNITE_PUBLIC_THREAD_POOL_SIZE);

      cfg.PeerAssemblyLoadingMode = PeerAssemblyLoadingMode.CurrentAppDomain;

      cfg.BinaryConfiguration = new BinaryConfiguration
      {
        Serializer = new BinarizableSerializer()
      };
    }


    private IgniteConfiguration setKubernetesIgniteConfiguration(IgniteConfiguration cfg)
    {
      cfg.SpringConfigUrl = @".\ignitePersistentImmutableKubeConfig.xml";

      cfg.CommunicationSpi = new TcpCommunicationSpi()
      {
        LocalPort = 47100
      };

      cfg.JvmOptions.Add("-javaagent:./libs/jmx_prometheus_javaagent-0.12.0.jar=8088:prometheusConfig.yaml");

      return cfg;
    }

    /// <summary>
    /// Configures ignite for use locally i.e on developers pc
    /// </summary>
    /// <param name="cfg">Ignite configuration that is being built</param>
    /// <returns></returns>
    private IgniteConfiguration setLocalIgniteConfiguration(IgniteConfiguration cfg)
    {
      cfg.SpringConfigUrl = @".\immutablePersistence.xml";

      // Enforce using only the LocalHost interface
      cfg.DiscoverySpi = new TcpDiscoverySpi
      {
        LocalAddress = "127.0.0.1",
        LocalPort = 47500,

        IpFinder = new TcpDiscoveryStaticIpFinder
        {
          Endpoints = new[] { "127.0.0.1:47500..47502" }
        }
      };

      cfg.CommunicationSpi = new TcpCommunicationSpi
      {
        LocalAddress = "127.0.0.1",
        LocalPort = 47100
      };
      return cfg;
    }

    public ICache<INonSpatialAffinityKey, ISerialisedByteArrayWrapper> InstantiateNonSpatialCacheReference()
    {
      var cfg = new CacheConfiguration();
      base.ConfigureNonSpatialImmutableCache(cfg);

      cfg.Name = TRexCaches.ImmutableNonSpatialCacheName();
      cfg.KeepBinaryInStore = true;

      // Non-spatial (event) data is replicated to all nodes for local access
      cfg.CacheMode = CacheMode.Replicated;

      cfg.Backups = 0;  // No backups need as it is a replicated cache

      Console.WriteLine($"CacheConfig is: {cfg}");
      Console.WriteLine($"immutableTRexGrid is : {immutableTRexGrid}");

      return immutableTRexGrid.GetOrCreateCache<INonSpatialAffinityKey, ISerialisedByteArrayWrapper>(cfg);
    }

    public ICache<ISubGridSpatialAffinityKey, ISerialisedByteArrayWrapper> InstantiateSpatialSubGridDirectoryCacheReference()
    {
      var cfg = new CacheConfiguration();
      base.ConfigureImmutableSpatialCache(cfg);

      cfg.Name = TRexCaches.SpatialSubGridDirectoryCacheName(StorageMutability.Immutable);
      cfg.KeepBinaryInStore = true;

      // TODO: No backups for now
      cfg.Backups = 0;

      // Spatial data is partitioned among the server grid nodes according to spatial affinity mapping
      cfg.CacheMode = CacheMode.Partitioned;

      // Configure the function that maps sub grid data into the affinity map for the nodes in the grid
      cfg.AffinityFunction = new SubGridBasedSpatialAffinityFunction();

      return immutableTRexGrid.GetOrCreateCache<ISubGridSpatialAffinityKey, ISerialisedByteArrayWrapper>(cfg);
    }

    public ICache<ISubGridSpatialAffinityKey, ISerialisedByteArrayWrapper> InstantiateSpatialSubGridSegmentCacheReference()
    {
      var cfg = new CacheConfiguration();
      base.ConfigureImmutableSpatialCache(cfg);

      cfg.Name = TRexCaches.SpatialSubGridSegmentCacheName(StorageMutability.Immutable);
      cfg.KeepBinaryInStore = true;

      // TODO: No backups for now
      cfg.Backups = 0;

      // Spatial data is partitioned among the server grid nodes according to spatial affinity mapping
      cfg.CacheMode = CacheMode.Partitioned;

      // Configure the function that maps sub grid data into the affinity map for the nodes in the grid
      cfg.AffinityFunction = new SubGridBasedSpatialAffinityFunction();

      return immutableTRexGrid.GetOrCreateCache<ISubGridSpatialAffinityKey, ISerialisedByteArrayWrapper>(cfg);
    }

    private void InstantiateSiteModelExistenceMapsCacheReference()
    {
      immutableTRexGrid.GetOrCreateCache<INonSpatialAffinityKey, ISerialisedByteArrayWrapper>(new CacheConfiguration
      {
        Name = TRexCaches.ProductionDataExistenceMapCacheName(StorageMutability.Immutable),
        KeepBinaryInStore = true,
        CacheMode = CacheMode.Replicated,

        Backups = 0,  // No backups need as it is a replicated cache

        DataRegionName = DataRegions.IMMUTABLE_SPATIAL_DATA_REGION
      });
    }

    private void InstantiateSiteModelsCacheReference()
    {
      immutableTRexGrid.GetOrCreateCache<INonSpatialAffinityKey, ISerialisedByteArrayWrapper>(new CacheConfiguration
      {
        Name = TRexCaches.SiteModelsCacheName(StorageMutability.Immutable),
        KeepBinaryInStore = true,
        CacheMode = CacheMode.Replicated,

        Backups = 0, // No backups need as it is a replicated cache

        DataRegionName = DataRegions.IMMUTABLE_NONSPATIAL_DATA_REGION
      });
    }

    /// <summary>
    /// Create the cache that holds the per project, per machine, change maps driven by TAG file ingest
    /// Note: This machine based information is distinguished from that in the non-spatial cache in that
    /// it is partitioned, rather than replicated.
    /// </summary>
    private void InstantiateSiteModelMachinesChangeMapsCacheReference()
    {
      immutableTRexGrid.GetOrCreateCache<ISiteModelMachineAffinityKey, ISerialisedByteArrayWrapper>(new CacheConfiguration
      {
        Name = TRexCaches.SiteModelChangeMapsCacheName(),
        KeepBinaryInStore = true,
        CacheMode = CacheMode.Partitioned,

        // TODO: No backups for now
        Backups = 0,
        DataRegionName = DataRegions.IMMUTABLE_NONSPATIAL_DATA_REGION,

        // Configure the function that maps the change maps to nodes in the grid
        // Note: This cache uses an affinity function that assigns data for a site model onto a single node.
        // For the purposes of the immutable grid, it is helpful for a node to contain all change maps for a single
        // site model as this simplifies the process of updating those change maps in response to messages from production data ingest 
        AffinityFunction = new ProjectBasedSpatialAffinityFunction(),

        AtomicityMode = CacheAtomicityMode.Transactional
      });
    }

    /// <summary>
    /// Create the cache that holds the per project, per machine, change maps driven by TAG file ingest
    /// Note: This machine based information is distinguished from that in the non-spatial cache in that
    /// it is partitioned, rather than replicated.
    /// </summary>
    private void InstantiateSiteModelChangeBufferQueueCacheReference()
    {
      immutableTRexGrid.GetOrCreateCache<ISiteModelChangeBufferQueueKey, ISiteModelChangeBufferQueueItem>(new CacheConfiguration
      {
        Name = TRexCaches.SiteModelChangeBufferQueueCacheName(),
        KeepBinaryInStore = true,
        CacheMode = CacheMode.Partitioned,

        // TODO: No backups for now
        Backups = 0,
        DataRegionName = DataRegions.IMMUTABLE_NONSPATIAL_DATA_REGION,

        // Configure the function that maps the change maps to nodes in the grid
        // Note: This cache uses an affinity function that assigns data for a site model onto a single node.
        // For the purposes of the immutable grid, it is helpful for a node to contain all change maps for a single
        // site model as this simplifies the process of updating those change maps in response to messages from production data ingest 
        AffinityFunction = new ProjectBasedSpatialAffinityFunction(),
      });
    }

    /// <summary>
    /// Configure the parameters of the existence map cache
    /// </summary>
    private CacheConfiguration ConfigureDesignTopologyExistenceMapsCache()
    {
      return new CacheConfiguration
      {
        Name = TRexCaches.DesignTopologyExistenceMapsCacheName(),

        // cfg.CopyOnRead = false;   Leave as default as should have no effect with 2.1+ without on heap caching enabled
        KeepBinaryInStore = true,

        // Replicate the maps across nodes
        CacheMode = CacheMode.Replicated,

        Backups = 0,  // No backups need as it is a replicated cache

        DataRegionName = DataRegions.IMMUTABLE_NONSPATIAL_DATA_REGION
      };
    }
    
    private void InstantiateDesignTopologyExistenceMapsCache()
    {
      immutableTRexGrid.GetOrCreateCache<INonSpatialAffinityKey, ISerialisedByteArrayWrapper>(ConfigureDesignTopologyExistenceMapsCache());
    }

    public void StartTRexGridCacheNode()
    {
      Log.LogInformation("Creating new Ignite node");

      var cfg = new IgniteConfiguration();
      ConfigureTRexGrid(cfg);

      Log.LogInformation($"Creating new Ignite node for {cfg.IgniteInstanceName}");

      try
      {
        Console.WriteLine($"Creating new Ignite node for {cfg.IgniteInstanceName}");
        immutableTRexGrid = DIContext.Obtain<ITRexGridFactory>()?.Grid(TRexGrids.ImmutableGridName(), cfg); 
      }
      finally
      {
        Log.LogInformation($"Completed creation of new Ignite node: Exists = {immutableTRexGrid != null}, Factory available = {DIContext.Obtain<ITRexGridFactory>() != null}");
      }

      // Wait until the grid is active
      DIContext.Obtain<IActivatePersistentGridServer>().WaitUntilGridActive(TRexGrids.ImmutableGridName());

      // Add the immutable Spatial & NonSpatial caches

      InstantiateNonSpatialCacheReference();

      InstantiateSpatialSubGridDirectoryCacheReference();
      InstantiateSpatialSubGridSegmentCacheReference();

      InstantiateSiteModelExistenceMapsCacheReference();

      InstantiateSiteModelsCacheReference();

      InstantiateSiteModelChangeBufferQueueCacheReference();
      InstantiateSiteModelMachinesChangeMapsCacheReference();

      InstantiateDesignTopologyExistenceMapsCache();
    }
  }
}
