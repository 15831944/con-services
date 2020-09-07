﻿using System;
using System.Threading;
using System.Threading.Tasks;
using CoreX.Interfaces;
using CoreX.Wrapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.AWS.TransferProxy;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.ConfigurationStore;
using VSS.TRex.Caching;
using VSS.TRex.Caching.Interfaces;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Common.HeartbeatLoggers;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.Common.Models;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.Designs;
using VSS.TRex.Designs.Factories;
using VSS.TRex.Designs.GridFabric.Events;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.Events;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.GridFabric.Factories;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.GridFabric.Servers.Compute;
using VSS.TRex.Pipelines;
using VSS.TRex.Pipelines.Factories;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.Pipelines.Interfaces.Tasks;
using VSS.TRex.Pipelines.Tasks;
using VSS.TRex.Profiling;
using VSS.TRex.Profiling.Factories;
using VSS.TRex.Profiling.Interfaces;
using VSS.TRex.SiteModelChangeMaps;
using VSS.TRex.SiteModelChangeMaps.Interfaces;
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.GridFabric.Events;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SiteModels.Interfaces.Events;
using VSS.TRex.Storage.Models;
using VSS.TRex.SubGrids;
using VSS.TRex.SubGrids.GridFabric.Arguments;
using VSS.TRex.SubGrids.Interfaces;
using VSS.TRex.SubGrids.Responses;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SurveyedSurfaces;
using VSS.TRex.SurveyedSurfaces.GridFabric.Requests;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Volumes.GridFabric.Arguments;

namespace VSS.TRex.Server.PSNode
{
  class Program
  {
    private static ISubGridPipelineBase SubGridPipelineFactoryMethod(PipelineProcessorPipelineStyle key)
    {
      return key switch
      {
        PipelineProcessorPipelineStyle.DefaultAggregative => new SubGridPipelineAggregative<SubGridsRequestArgument, SubGridRequestsResponse>(),
        PipelineProcessorPipelineStyle.ProgressiveVolumes => new SubGridPipelineAggregative<ProgressiveVolumesSubGridsRequestArgument, SubGridRequestsResponse>(),
        _ => null
      };
    }

    private static ITRexTask SubGridTaskFactoryMethod(PipelineProcessorTaskStyle key)
    {
      return key switch
      {
        PipelineProcessorTaskStyle.AggregatedPipelined => new AggregatedPipelinedSubGridTask(),
        _ => null
      };
    }

    private static void DependencyInjection()
    {
      DIBuilder
        .New()
        .AddLogging()
        .Add(x => x.AddSingleton<IConfigurationStore, GenericConfiguration>())
        .Add(x => x.AddSingleton<ITransferProxyFactory>(factory => new TransferProxyFactory(factory.GetRequiredService<IConfigurationStore>(), factory.GetRequiredService<ILoggerFactory>())))
        .Build()
        .Add(x => x.AddSingleton<ICoreXWrapper, CoreXWrapper>())
        .Add(x => x.AddSingleton<ITRexConvertCoordinates>(new TRexConvertCoordinates()))
        .Add(VSS.TRex.IO.DIUtilities.AddPoolCachesToDI)
        .Add(VSS.TRex.Cells.DIUtilities.AddPoolCachesToDI)
        .Add(TRexGridFactory.AddGridFactoriesToDI)
        .Add(VSS.TRex.Storage.Utilities.DIUtilities.AddProxyCacheFactoriesToDI)
        .Build()
        .Add(VSS.TRex.SiteModelChangeMaps.Utilities.DIUtilities.AddProxyCacheFactoriesToDI)
        .Add(x => x.AddSingleton<ISubGridCellSegmentPassesDataWrapperFactory>(new SubGridCellSegmentPassesDataWrapperFactory()))
        .Add(x => x.AddSingleton<ISubGridCellLatestPassesDataWrapperFactory>(new SubGridCellLatestPassesDataWrapperFactory()))      
        .Add(x => x.AddTransient<ISurveyedSurfaces>(factory => new SurveyedSurfaces.SurveyedSurfaces()))
        .Add(x => x.AddSingleton<ISurveyedSurfaceFactory>(new SurveyedSurfaceFactory()))
        .Add(x => x.AddSingleton<ISubGridSpatialAffinityKeyFactory>(new SubGridSpatialAffinityKeyFactory()))
        .Add(x => x.AddSingleton<ISiteModels>(new SiteModels.SiteModels(StorageMutability.Immutable)))
        .Add(x => x.AddSingleton<ISiteModelFactory>(new SiteModelFactory()))
        .Add(x => x.AddSingleton<IProfilerBuilderFactory<ProfileCell>>(new ProfilerBuilderFactory<ProfileCell>()))
        .Add(x => x.AddSingleton<IProfilerBuilderFactory<SummaryVolumeProfileCell>>(new ProfilerBuilderFactory<SummaryVolumeProfileCell>()))
        .Add(x => x.AddTransient<IProfilerBuilder<ProfileCell>>(factory => new ProfilerBuilder<ProfileCell>()))
        .Add(x => x.AddTransient<IProfilerBuilder<SummaryVolumeProfileCell>>(factory => new ProfilerBuilder<SummaryVolumeProfileCell>()))
        .Add(ExistenceMaps.ExistenceMaps.AddExistenceMapFactoriesToDI)
        .Add(x => x.AddSingleton<IPipelineProcessorFactory>(new PipelineProcessorFactory()))
        .Add(x => x.AddSingleton<Func<PipelineProcessorPipelineStyle, ISubGridPipelineBase>>(provider => SubGridPipelineFactoryMethod))
        .Add(x => x.AddTransient<IRequestAnalyser>(factory => new RequestAnalyser()))
        .Add(x => x.AddSingleton<Func<ISubGridRequestor>>(factory => () => new SubGridRequestor()))
        .Add(x => x.AddSingleton<Func<PipelineProcessorTaskStyle, ITRexTask>>(provider => SubGridTaskFactoryMethod))
        .Add(x => x.AddSingleton<IProductionEventsFactory>(new ProductionEventsFactory()))
        .Add(x => x.AddSingleton<IClientLeafSubGridFactory>(ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory()))
        .Build()
        .Add(x => x.AddSingleton(new SubGridProcessingServer()))
        .Add(x => x.AddSingleton<IDesignClassFactory>(new DesignClassFactory()))
        .Add(x => x.AddTransient<IDesigns>(factory => new Designs.Storage.Designs()))
        .Add(x => x.AddSingleton<IDesignFiles>(new DesignFiles()))
        .Add(x => x.AddSingleton<IDesignManager>(factory => new DesignManager(StorageMutability.Immutable)))
        .Add(x => x.AddSingleton<IDesignChangedEventListener>(new DesignChangedEventListener(TRexGrids.ImmutableGridName())))
        .Add(x => x.AddSingleton<ISurveyedSurfaceManager>(factory => new SurveyedSurfaceManager(StorageMutability.Immutable)))

        // Create the cache to store the general sub grid results. Up to one million items, 1Gb RAM, MRU dead band fraction of one third
        .Add(x => x.AddSingleton<ITRexSpatialMemoryCache>(
          new TRexSpatialMemoryCache(
            DIContext.Obtain<IConfigurationStore>().GetValueInt("GENERAL_SUBGRID_RESULT_CACHE_MAXIMUM_ELEMENT_COUNT", Consts.GENERAL_SUBGRID_RESULT_CACHE_MAXIMUM_ELEMENT_COUNT),
            DIContext.Obtain<IConfigurationStore>().GetValueLong("GENERAL_SUBGRID_RESULT_CACHE_MAXIMUM_SIZE", Consts.GENERAL_SUBGRID_RESULT_CACHE_MAXIMUM_SIZE),
            DIContext.Obtain<IConfigurationStore>().GetValueDouble("GENERAL_SUBGRID_RESULT_CACHE_DEAD_BAND_FRACTION", Consts.GENERAL_SUBGRID_RESULT_CACHE_DEAD_BAND_FRACTION))
        ))

        // Register the listener for site model attribute change notifications
        .Add(x => x.AddSingleton<ISiteModelAttributesChangedEventListener>(new SiteModelAttributesChangedEventListener(TRexGrids.ImmutableGridName())))
        .Add(x => x.AddTransient<IFilterSet>(factory => new FilterSet()))

        .Add(x => x.AddSingleton<ITRexHeartBeatLogger>(new TRexHeartBeatLogger()))

        //.Add(x => x.AddSingleton<Func<IProfileCell>>(() => new ProfileCell()))

        // Register the factory for the CellProfileAnalyzer for detailed cell pass/lift cell profiles
        .Add(x => x.AddTransient<Func<ISiteModel, ISubGridTreeBitMask, IFilterSet, ICellLiftBuilder, IOverrideParameters, ILiftParameters, ICellProfileAnalyzer<ProfileCell>>>(
          factory => (siteModel, pDExistenceMap, filterSet, cellLiftBuilder, overrides, liftParams) 
            => new CellProfileAnalyzer(siteModel, pDExistenceMap, filterSet, cellLiftBuilder, overrides, liftParams)))

        // Register the factory for the CellProfileAnalyzer for summary volume cell profiles
        .Add(x => x.AddTransient<Func<ISiteModel, ISubGridTreeBitMask, IFilterSet, IDesignWrapper, ICellLiftBuilder, VolumeComputationType, IOverrideParameters, ILiftParameters, ICellProfileAnalyzer<SummaryVolumeProfileCell>>>(
          factory => (siteModel, pDExistenceMap, filterSet, referenceDesignWrapper, cellLiftBuilder, volumeComputationType, overrides, liftParams) 
            =>  new SummaryVolumesCellProfileAnalyzer(siteModel, pDExistenceMap, filterSet, referenceDesignWrapper, cellLiftBuilder, volumeComputationType, overrides, liftParams)))

        // Register the factory for surface elevation requests
        .Build()
        .Add(x => x.AddSingleton<Func<ITRexSpatialMemoryCache, ITRexSpatialMemoryCacheContext, ISurfaceElevationPatchRequest>>((cache, context) => new SurfaceElevationPatchRequestViaLocalCompute(cache, context)))

        .Build()
        .Add(x => x.AddSingleton<IRequestorUtilities>(new RequestorUtilities()))
        .Add(x => x.AddSingleton<ISubGridRetrieverFactory>(new SubGridRetrieverFactory()))
        .Add(x => x.AddSingleton<ISiteModelChangeMapDeltaNotifier>(new SiteModelChangeMapDeltaNotifier()))

        .Add(x => x.AddSingleton<ISubGridQOSTaskScheduler, SubGridQOSTaskScheduler>())
        .Complete();
    }

    // This static array ensures that all required assemblies are included into the artifacts by the linker
    private static void EnsureAssemblyDependenciesAreLoaded()
    {
      // This static array ensures that all required assemblies are included into the artifacts by the linker
      Type[] AssemblyDependencies =
      {
        typeof(VSS.TRex.Analytics.MDPStatistics.MDPStatisticsAggregator),
        typeof(VSS.TRex.Geometry.BoundingIntegerExtent2D),
        typeof(VSS.TRex.Exports.Patches.PatchResult),
        typeof(VSS.TRex.GridFabric.BaseIgniteClass),
        typeof(VSS.TRex.Common.SubGridsPipelinedResponseBase),
        typeof(VSS.TRex.Logging.Logger),
        typeof(VSS.TRex.DI.DIContext),
        typeof(VSS.TRex.Storage.StorageProxy),
        typeof(VSS.TRex.SiteModels.SiteModel),
        typeof(VSS.TRex.Cells.CellEvents),
        typeof(VSS.TRex.Compression.AttributeValueModifiers),
        typeof(CoreX.Models.LLH),
        typeof(VSS.TRex.Designs.DesignBase),
        typeof(VSS.TRex.Designs.TTM.HashOrdinate),
        typeof(VSS.TRex.Designs.TTM.Optimised.HeaderConsts),
        typeof(VSS.TRex.Events.CellPassFastEventLookerUpper),
        typeof(VSS.TRex.ExistenceMaps.ExistenceMaps),
        typeof(VSS.TRex.Filters.CellPassAttributeFilter),
        typeof(VSS.TRex.GridFabric.BaseIgniteClass),
        typeof(VSS.TRex.Machines.Machine),
        typeof(VSS.TRex.Pipelines.PipelineProcessor<SubGridsRequestArgument>),
        typeof(VSS.TRex.Profiling.CellLiftBuilder),
        typeof(VSS.TRex.Rendering.PlanViewTileRenderer),
        typeof(VSS.TRex.SubGrids.CutFillUtilities),
        typeof(VSS.TRex.SubGridTrees.Client.ClientCMVLeafSubGrid),
        typeof(VSS.TRex.SubGridTrees.Core.Utilities.SubGridUtilities),
        typeof(VSS.TRex.SubGridTrees.Server.MutabilityConverter),
        typeof(VSS.TRex.SurveyedSurfaces.SurveyedSurface),
        typeof(VSS.TRex.Volumes.CutFillVolume),
        typeof(VSS.TRex.Reports.StationOffset.Executors.ComputeStationOffsetReportExecutor_ClusterCompute),
        typeof(VSS.TRex.CellDatum.GridFabric.Responses.CellDatumResponse_ClusterCompute),
        typeof(VSS.TRex.SiteModelChangeMaps.GridFabric.Services.SiteModelChangeProcessorService)
      };

      foreach (var asmType in AssemblyDependencies)
      {
        if (asmType.FullName == "DummyTypeName")
          Console.WriteLine($"Assembly for type {asmType} has not been loaded.");
      }
    }

    private static void DoServiceInitialisation()
    {
      // Start listening to site model change notifications
      DIContext.Obtain<ISiteModelAttributesChangedEventListener>().StartListening();
      // Start listening to design state change notifications
      DIContext.Obtain<IDesignChangedEventListener>().StartListening();

      // Register the heartbeat loggers
      DIContext.Obtain<ITRexHeartBeatLogger>().AddContext(new MemoryHeartBeatLogger());
      DIContext.Obtain<ITRexHeartBeatLogger>().AddContext(new SpatialMemoryCacheHeartBeatLogger());
      DIContext.Obtain<ITRexHeartBeatLogger>().AddContext(new DotnetThreadHeartBeatLogger());
      DIContext.Obtain<ITRexHeartBeatLogger>().AddContext(new IgniteNodeMetricsHeartBeatLogger(DIContext.Obtain<ITRexGridFactory>().Grid(StorageMutability.Immutable)));
    }

    static async Task<int> Main(string[] args)
    {
      try
      {
        Console.WriteLine($"TRex service starting at {DateTime.Now}");

        EnsureAssemblyDependenciesAreLoaded();
        DependencyInjection();

        ThreadPool.GetMinThreads(out var minWorkerThreads, out var minCompletionPortThreads);
        ThreadPool.GetMaxThreads(out var maxWorkerThreads, out var maxCompletionPortThreads);

        // Create a much larger pool of system threads to allow QOS channels with groups of sub-tasks room to take advantage of all system resources while also allowing
        // other requests to run concurrently
        ThreadPool.SetMinThreads(minWorkerThreads * DIContext.ObtainRequired<ISubGridQOSTaskScheduler>().DefaultThreadPoolFractionDivisor, minCompletionPortThreads);

        Console.WriteLine($"Operating thread pool: min threads {minWorkerThreads}/{minCompletionPortThreads}, max threads {maxWorkerThreads}/{maxCompletionPortThreads}");

        var cancelTokenSource = new CancellationTokenSource();
        AppDomain.CurrentDomain.ProcessExit += (s, e) =>
        {
          Console.WriteLine("Exiting");
          DIContext.Obtain<ITRexGridFactory>().StopGrids();
          cancelTokenSource.Cancel();
        };

        AppDomain.CurrentDomain.UnhandledException += TRexAppDomainUnhandledExceptionHandler.Handler;

        DoServiceInitialisation();

        await Task.Delay(-1, cancelTokenSource.Token);
        return 0;
      }
      catch (TaskCanceledException)
      {
        // Don't care as this is thrown by Task.Delay()
        Console.WriteLine("Process exit via TaskCanceledException (SIGTERM)");
        return 0;
      }
      catch (Exception e)
      {
        Console.WriteLine($"Unhandled exception: {e}");
        Console.WriteLine($"Stack trace: {e.StackTrace}");
        return -1;
      }
    }
  }
}
