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
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Caches;
using VSS.TRex.Storage.Models;
using VSS.TRex.TAGFiles.Models;

namespace VSS.TRex.GridFabric.Servers.Compute
{
  /// <summary>
  /// Defines a representation of a server responsible for performing TRex related compute operations using
  /// the Ignite In Memory Data Grid
  /// </summary>
  public class MutableCacheComputeServer : IgniteServer
  {
    private static readonly ILogger _log = Logger.CreateLogger<MutableCacheComputeServer>();

    /// <summary>
    /// Constructor for the TRex cache compute server node. Responsible for starting all Ignite services and creating the grid
    /// and cache instance in preparation for client access by business logic running on the node.
    /// </summary>
    public MutableCacheComputeServer()
    {
      _log.LogDebug($"PersistentCacheStoreLocation is: {TRexServerConfig.PersistentCacheStoreLocation}");
      if (mutableTRexGrid == null)
      {
        StartTRexGridCacheNode();
      }
    }

    public override void ConfigureTRexGrid(IgniteConfiguration cfg)
    {
      base.ConfigureTRexGrid(cfg);

      cfg.IgniteInstanceName = TRexGrids.MutableGridName();

      cfg.JvmMaxMemoryMb = DIContext.Obtain<IConfigurationStore>().GetValueInt(IGNITE_JVM_MAX_HEAP_SIZE_MB, DEFAULT_IGNITE_JVM_MAX_HEAP_SIZE_MB);
      cfg.JvmInitialMemoryMb = DIContext.Obtain<IConfigurationStore>().GetValueInt(IGNITE_JVM_INITIAL_HEAP_SIZE_MB, DEFAULT_IGNITE_JVM_INITIAL_HEAP_SIZE_MB);

      cfg.UserAttributes = new Dictionary<string, object>
      {
        { "Owner", TRexGrids.MutableGridName() }
      };

      // Configure the Ignite persistence layer to store our data
      cfg.DataStorageConfiguration = new DataStorageConfiguration
      {
        WalMode = WalMode.Fsync,
        PageSize = DataRegions.DEFAULT_MUTABLE_DATA_REGION_PAGE_SIZE,

        StoragePath = Path.Combine(TRexServerConfig.PersistentCacheStoreLocation, "Mutable", "Persistence"),
        WalPath = Path.Combine(TRexServerConfig.PersistentCacheStoreLocation, "Mutable", "WalStore"),
        WalArchivePath = Path.Combine(TRexServerConfig.PersistentCacheStoreLocation, "Mutable", "WalArchive"),

        WalSegmentSize = 512 * 1024 * 1024, // Set the WalSegmentSize to 512Mb to better support high write loads (can be set to max 2Gb)
        MaxWalArchiveSize = (long)10 * 512 * 1024 * 1024, // Ensure there are 10 segments in the WAL archive at the defined segment size

        DefaultDataRegionConfiguration = new DataRegionConfiguration
        {
          Name = DataRegions.DEFAULT_MUTABLE_DATA_REGION_NAME,
          InitialSize = 128 * 1024 * 1024,  // 128 MB // TODO: This needs to be added to configuration
          MaxSize = 2L * 1024 * 1024 * 1024,  // 2 GB // TODO: This needs to be added to configuration

          PersistenceEnabled = true
        },

        // Establish a separate data region for the TAG file buffer queue
        DataRegionConfigurations = new List<DataRegionConfiguration>
        {
          new DataRegionConfiguration
          {
            Name = DataRegions.TAG_FILE_BUFFER_QUEUE_DATA_REGION,
            InitialSize = 128 * 1024 * 1024,  // 128 MB to start // TODO: This needs to be added to configuration
            MaxSize = 128 * 1024 * 1024, // TODO: This needs to be added to configuration

            PersistenceEnabled = true
           }
         }
      };

      cfg.CacheConfiguration = new List<CacheConfiguration>();

      _log.LogInformation($"cfg.DataStorageConfiguration.StoragePath={cfg.DataStorageConfiguration.StoragePath}");
      _log.LogInformation($"cfg.DataStorageConfiguration.WalArchivePath={cfg.DataStorageConfiguration.WalArchivePath}");
      _log.LogInformation($"cfg.DataStorageConfiguration.WalPath={cfg.DataStorageConfiguration.WalPath}");

      cfg.JvmOptions = new List<string>() {
        "-DIGNITE_QUIET=false",
        "-Djava.net.preferIPv4Stack=true",
        "-XX:+UseG1GC",
        "--add-exports=java.base/jdk.internal.misc=ALL-UNNAMED",
        "--add-exports=java.base/sun.nio.ch=ALL-UNNAMED",
        "--add-exports=java.management/com.sun.jmx.mbeanserver=ALL-UNNAMED",
        "--add-exports=jdk.internal.jvmstat/sun.jvmstat.monitor=ALL-UNNAMED",
        "--add-exports=java.base/sun.reflect.generics.reflectiveObjects=ALL-UNNAMED",
        "--illegal-access=permit"
      };


      cfg.Logger = new TRexIgniteLogger(Logger.CreateLogger("MutableCacheComputeServer"));

      // Set an Ignite metrics heartbeat of 10 seconds
      cfg.MetricsLogFrequency = new TimeSpan(0, 0, 0, 10); // TODO: This needs to be added to configuration

      cfg.PublicThreadPoolSize = DIContext.Obtain<IConfigurationStore>().GetValueInt(IGNITE_PUBLIC_THREAD_POOL_SIZE, DEFAULT_IGNITE_PUBLIC_THREAD_POOL_SIZE);

      cfg.PeerAssemblyLoadingMode = PeerAssemblyLoadingMode.Disabled;

      cfg.BinaryConfiguration = new BinaryConfiguration
      {
        Serializer = new BinarizableSerializer()
      };

      bool.TryParse(Environment.GetEnvironmentVariable("IS_KUBERNETES"), out var isKubernetes);
      cfg = isKubernetes ? setKubernetesIgniteConfiguration(cfg) : setLocalIgniteConfiguration(cfg);
      cfg.WorkDirectory = Path.Combine(TRexServerConfig.PersistentCacheStoreLocation, "Mutable");
    }

    private IgniteConfiguration setKubernetesIgniteConfiguration(IgniteConfiguration cfg)
    {
      cfg.SpringConfigUrl = @".\ignitePersistantMutableKubeConfig.xml";
      cfg.JvmOptions.Add("-javaagent:./libs/jmx_prometheus_javaagent-0.12.0.jar=8088:prometheusConfig.yaml");

      cfg.CommunicationSpi = new TcpCommunicationSpi()
      {
        LocalPort = 48100
      };
      return cfg;
    }

    private IgniteConfiguration setLocalIgniteConfiguration(IgniteConfiguration cfg)
    {
      //temp
      cfg.SpringConfigUrl = @".\mutablePersistence.xml";

      // Enforce using only the LocalHost interface
      cfg.DiscoverySpi = new TcpDiscoverySpi()
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

    public override void ConfigureNonSpatialMutableCache(CacheConfiguration cfg)
    {
      base.ConfigureNonSpatialMutableCache(cfg);

      cfg.Name = TRexCaches.MutableNonSpatialCacheName();
      cfg.KeepBinaryInStore = true;
      cfg.CacheMode = CacheMode.Partitioned;
      cfg.AffinityFunction = new MutableNonSpatialAffinityFunction();
      cfg.Backups = 0;

      // cfg.CopyOnRead = false;   Leave as default as should have no effect with 2.1+ without on heap caching enabled
    }

    public ICache<INonSpatialAffinityKey, ISerialisedByteArrayWrapper> InstantiateNonSpatialCacheReference()
    {
      var cfg = new CacheConfiguration();
      base.ConfigureNonSpatialMutableCache(cfg);

      cfg.Name = TRexCaches.MutableNonSpatialCacheName();
      cfg.KeepBinaryInStore = true;
      cfg.CacheMode = CacheMode.Partitioned;
      cfg.AffinityFunction = new MutableNonSpatialAffinityFunction();
      cfg.Backups = 0;

      return mutableTRexGrid.GetOrCreateCache<INonSpatialAffinityKey, ISerialisedByteArrayWrapper>(cfg);
    }

    public ICache<ISubGridSpatialAffinityKey, ISerialisedByteArrayWrapper> InstantiateSpatialSubGridDirectoryCacheReference()
    {
      var cfg = new CacheConfiguration();
      base.ConfigureMutableSpatialCache(cfg);

      cfg.Name = TRexCaches.SpatialSubGridDirectoryCacheName(StorageMutability.Mutable);
      cfg.KeepBinaryInStore = true;

      // TODO: No backups for now
      cfg.Backups = 0;

      // Spatial data is partitioned among the server grid nodes according to spatial affinity mapping
      cfg.CacheMode = CacheMode.Partitioned;

      // Configure the function that maps sub grid data into the affinity map for the nodes in the grid
      cfg.AffinityFunction = new ProjectBasedSpatialAffinityFunction();

      return mutableTRexGrid.GetOrCreateCache<ISubGridSpatialAffinityKey, ISerialisedByteArrayWrapper>(cfg);
    }

    public ICache<ISubGridSpatialAffinityKey, ISerialisedByteArrayWrapper> InstantiateSpatialSubGridSegmentCacheReference()
    {
      var cfg = new CacheConfiguration();
      base.ConfigureMutableSpatialCache(cfg);

      cfg.Name = TRexCaches.SpatialSubGridSegmentCacheName(StorageMutability.Mutable);
      cfg.KeepBinaryInStore = true;

      // TODO: No backups for now
      cfg.Backups = 0;

      // Spatial data is partitioned among the server grid nodes according to spatial affinity mapping
      cfg.CacheMode = CacheMode.Partitioned;

      // Configure the function that maps sub grid data into the affinity map for the nodes in the grid
      cfg.AffinityFunction = new ProjectBasedSpatialAffinityFunction();

      return mutableTRexGrid.GetOrCreateCache<ISubGridSpatialAffinityKey, ISerialisedByteArrayWrapper>(cfg);
    }

    public void InstantiateTAGFileBufferQueueCacheReference()
    {
      var cfg = new CacheConfiguration
      {
        Name = TRexCaches.TAGFileBufferQueueCacheName(),
        KeepBinaryInStore = true,
        CacheMode = CacheMode.Partitioned,
        AffinityFunction = new MutableNonSpatialAffinityFunction(),
        DataRegionName = DataRegions.TAG_FILE_BUFFER_QUEUE_DATA_REGION,

        // TODO: No backups for now
        Backups = 0
      };

      mutableTRexGrid.GetOrCreateCache<ITAGFileBufferQueueKey, TAGFileBufferQueueItem>(cfg);
    }

    private void InstantiateSiteModelExistenceMapsCacheReference()
    {
      mutableTRexGrid.GetOrCreateCache<INonSpatialAffinityKey, ISerialisedByteArrayWrapper>(new CacheConfiguration
      {
        Name = TRexCaches.ProductionDataExistenceMapCacheName(StorageMutability.Mutable),
        KeepBinaryInStore = true,
        CacheMode = CacheMode.Replicated,

        Backups = 0,  // No backups need as it is a replicated cache

        DataRegionName = DataRegions.MUTABLE_SPATIAL_DATA_REGION
      });
    }

    private void InstantiateSiteModelsCacheReference()
    {
      mutableTRexGrid.GetOrCreateCache<INonSpatialAffinityKey, ISerialisedByteArrayWrapper>(new CacheConfiguration
      {
        Name = TRexCaches.SiteModelsCacheName(StorageMutability.Mutable),
        KeepBinaryInStore = true,
        CacheMode = CacheMode.Partitioned,
        AffinityFunction = new MutableNonSpatialAffinityFunction(),

        // TODO: No backups for now
        Backups = 0,

        DataRegionName = DataRegions.MUTABLE_NONSPATIAL_DATA_REGION
      });
    }

    /// <summary>
    /// Create the caches that 
    /// </summary>
    private void InstantiateRebuildSiteModelCacheReferences()
    {
      mutableTRexGrid.GetOrCreateCache<INonSpatialAffinityKey, IRebuildSiteModelMetaData>(new CacheConfiguration
      {
        Name = TRexCaches.SiteModelRebuilderMetaDataCacheName(),
        KeepBinaryInStore = true,
        CacheMode = CacheMode.Partitioned,
        AffinityFunction = new MutableNonSpatialAffinityFunction(),
        Backups = 0,
        DataRegionName = DataRegions.MUTABLE_NONSPATIAL_DATA_REGION
      });

      mutableTRexGrid.GetOrCreateCache<INonSpatialAffinityKey, ISerialisedByteArrayWrapper>(new CacheConfiguration
      {
        Name = TRexCaches.SiteModelRebuilderFileKeyCollectionsCacheName(),
        KeepBinaryInStore = true,
        CacheMode = CacheMode.Partitioned,
        AffinityFunction = new MutableNonSpatialAffinityFunction(),
        Backups = 0,
        DataRegionName = DataRegions.MUTABLE_NONSPATIAL_DATA_REGION
      });
    }

    public void StartTRexGridCacheNode()
    {
      var cfg = new IgniteConfiguration();
      ConfigureTRexGrid(cfg);

      _log.LogInformation($"Creating new Ignite node for {cfg.IgniteInstanceName}");

      try
      {
        mutableTRexGrid = DIContext.Obtain<ITRexGridFactory>()?.Grid(TRexGrids.MutableGridName(), cfg); 
      }
      finally
      {
        _log.LogInformation($"Completed creation of new Ignite node: Exists = {mutableTRexGrid != null}, Factory available = {DIContext.Obtain<ITRexGridFactory>() != null}");
      }

      // Wait until the grid is active
      DIContext.Obtain<IActivatePersistentGridServer>().WaitUntilGridActive(TRexGrids.MutableGridName());

      // Add the mutable Spatial & NonSpatial caches
      InstantiateNonSpatialCacheReference();

      InstantiateSpatialSubGridDirectoryCacheReference();
      InstantiateSpatialSubGridSegmentCacheReference();

      InstantiateTAGFileBufferQueueCacheReference();

      InstantiateSiteModelExistenceMapsCacheReference();
      InstantiateSiteModelsCacheReference();

      InstantiateRebuildSiteModelCacheReferences();

      // Create the SiteModel MetaData Manager so later DI context references wont need to create the cache etc for it at an inappropriate time
      var _ = DIContext.Obtain<ISiteModelMetadataManager>();
    }

  }
}
