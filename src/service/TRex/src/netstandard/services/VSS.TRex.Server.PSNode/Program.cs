﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using VSS.TRex.Caching;
using VSS.ConfigurationStore;
using VSS.TRex.Caching.Interfaces;
using VSS.TRex.Common;
using VSS.TRex.Designs;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.Events;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.ExistenceMaps.Interfaces;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.Factories;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.GridFabric.Responses;
using VSS.TRex.GridFabric.Servers.Compute;
using VSS.TRex.Pipelines;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.Pipelines.Interfaces.Tasks;
using VSS.TRex.Pipelines.Tasks;
using VSS.TRex.Profiling;
using VSS.TRex.Profiling.Factories;
using VSS.TRex.Profiling.Interfaces;
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.GridFabric.Events;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SiteModels.Interfaces.Events;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SubGrids;
using VSS.TRex.SubGrids.Interfaces;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SurveyedSurfaces;
using VSS.TRex.SurveyedSurfaces.GridFabric.Requests;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using Consts = VSS.TRex.Common.Consts;

namespace VSS.TRex.Server.PSNode
{
  class Program
  {
    private static ISubGridPipelineBase SubGridPipelineFactoryMethod(PipelineProcessorPipelineStyle key)
    {
      switch (key)
      {
        case PipelineProcessorPipelineStyle.DefaultAggregative:
          return new SubGridPipelineAggregative<SubGridsRequestArgument, SubGridRequestsResponse>();
        default:
          return null;
      }
    }

    private static ITRexTask SubGridTaskFactoryMethod(PipelineProcessorTaskStyle key)
    {
      switch (key)
      {
        case PipelineProcessorTaskStyle.AggregatedPipelined:
          return new AggregatedPipelinedSubGridTask();
        default:
          return null;
      }
    }

    private static void DependencyInjection()
    {
      DIBuilder
        .New()
        .AddLogging()
        .Add(x => x.AddSingleton<IConfigurationStore, GenericConfiguration>())
        .Add(x => x.AddSingleton<ITRexGridFactory>(new TRexGridFactory()))
        .Build()
        .Add(VSS.TRex.Storage.Utilities.DIUtilities.AddProxyCacheFactoriesToDI)
        .Add(x => x.AddTransient<ISurveyedSurfaces>(factory => new SurveyedSurfaces.SurveyedSurfaces()))
        .Add(x => x.AddSingleton<ISurveyedSurfaceFactory>(new SurveyedSurfaceFactory()))
        .Add(x => x.AddSingleton<ISubGridSpatialAffinityKeyFactory>(new SubGridSpatialAffinityKeyFactory()))
        .Build()
        .Add(x => x.AddSingleton<ISiteModels>(new SiteModels.SiteModels(() => DIContext.Obtain<IStorageProxyFactory>().ImmutableGridStorage())))
        .Add(x => x.AddSingleton<ISiteModelFactory>(new SiteModelFactory()))
        .Add(x => x.AddSingleton<IProfilerBuilderFactory<ProfileCell>>(new ProfilerBuilderFactory<ProfileCell>()))
        .Add(x => x.AddSingleton<IProfilerBuilderFactory<SummaryVolumeProfileCell>>(new ProfilerBuilderFactory<SummaryVolumeProfileCell>()))
        .Add(x => x.AddTransient<IProfilerBuilder<ProfileCell>>(factory => new ProfilerBuilder<ProfileCell>()))
        .Add(x => x.AddTransient<IProfilerBuilder<SummaryVolumeProfileCell>>(factory => new ProfilerBuilder<SummaryVolumeProfileCell>()))
        .Add(x => x.AddSingleton<IExistenceMaps>(new ExistenceMaps.ExistenceMaps()))
        .Add(x => x.AddSingleton<IPipelineProcessorFactory>(new PipelineProcessorFactory()))
        .Add(x => x.AddSingleton<Func<PipelineProcessorPipelineStyle, ISubGridPipelineBase>>(provider => SubGridPipelineFactoryMethod))
        .Add(x => x.AddTransient<IRequestAnalyser>(factory => new RequestAnalyser()))
        .Add(x => x.AddSingleton<Func<ISubGridRequestor>>(factory => () => new SubGridRequestor()))
        .Add(x => x.AddSingleton<Func<PipelineProcessorTaskStyle, ITRexTask>>(provider => SubGridTaskFactoryMethod))
        .Add(x => x.AddSingleton<IProductionEventsFactory>(new ProductionEventsFactory()))
        .Add(x => x.AddSingleton<IClientLeafSubGridFactory>(ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory()))
        .Build()
        .Add(x => x.AddSingleton(new SubGridProcessingServer()))
        .Add(x => x.AddSingleton<IDesignManager>(factory => new DesignManager()))
        .Add(x => x.AddSingleton<ISurveyedSurfaceManager>(factory => new SurveyedSurfaceManager()))

        .Add(x => x.AddSingleton<IRequestorUtilities>(new RequestorUtilities()))

        // Create the cache to store the general subgrid results. Up to one million items, 1Gb RAM, MRU dead band fraction of one third
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
        .Add(x => x.AddTransient<Func<ISiteModel, ISubGridTreeBitMask, IFilterSet, IDesign, ICellLiftBuilder, ICellProfileAnalyzer<ProfileCell>>>(
          factory => (siteModel, pDExistenceMap, filterSet, cellPassFilter_ElevationRangeDesign, cellLiftBuilder) => new CellProfileAnalyzer(siteModel, pDExistenceMap, filterSet, cellPassFilter_ElevationRangeDesign, cellLiftBuilder)))

        // Register the factory for the CellProfileAnalyzer for summary volume cell profiles
        .Add(x => x.AddTransient<Func<ISiteModel, ISubGridTreeBitMask, IFilterSet, IDesign, IDesign, ICellLiftBuilder, ICellProfileAnalyzer<SummaryVolumeProfileCell>>>(
          factory => (siteModel, pDExistenceMap, filterSet, cellPassFilter_ElevationRangeDesign, referenceDesign, cellLiftBuilder) =>  new SummaryVolumesCellProfileAnalyzer(siteModel, pDExistenceMap, filterSet, cellPassFilter_ElevationRangeDesign, referenceDesign, cellLiftBuilder)))

        // Register the factory for surface elevation requests
        .Add(x => x.AddSingleton<Func<ITRexSpatialMemoryCache, ITRexSpatialMemoryCacheContext, ISurfaceElevationPatchRequest>>((cache, context) => new SurfaceElevationPatchRequest(cache, context)))

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
        typeof(VSS.TRex.CoordinateSystems.Models.LLH),
        typeof(VSS.TRex.Designs.DesignBase),
        typeof(VSS.TRex.Designs.TTM.HashOrdinate),
        typeof(VSS.TRex.Designs.TTM.Optimised.HeaderConsts),
        typeof(VSS.TRex.Events.CellPassFastEventLookerUpper),
        typeof(VSS.TRex.ExistenceMaps.ExistenceMaps),
        typeof(VSS.TRex.Filters.CellPassAttributeFilter),
        typeof(VSS.TRex.GridFabric.BaseIgniteClass),
        typeof(VSS.TRex.Machines.Machine),
        typeof(VSS.TRex.Pipelines.PipelineProcessor),
        typeof(VSS.TRex.Profiling.CellLiftBuilder),
        typeof(VSS.TRex.Rendering.PlanViewTileRenderer),
        typeof(VSS.TRex.Services.Designs.DesignsService),
        typeof(VSS.TRex.Services.SurveyedSurfaces.SurveyedSurfaceService),
        typeof(VSS.TRex.SubGrids.CutFillUtilities),
        typeof(VSS.TRex.SubGridTrees.Client.ClientCMVLeafSubGrid),
        typeof(VSS.TRex.SubGridTrees.Core.Utilities.SubGridUtilities),
        typeof(VSS.TRex.SubGridTrees.Server.MutabilityConverter),
        typeof(VSS.TRex.SurveyedSurfaces.SurveyedSurface),
        typeof(VSS.TRex.Volumes.CutFillVolume)
      };

      foreach (var asmType in AssemblyDependencies)
        if (asmType.Assembly == null)
          Console.WriteLine($"Assembly for type {asmType} has not been loaded.");
    }

    private static void DoServiceInitialisation()
    {
      // Start listening to site model change notifications
      DIContext.Obtain<ISiteModelAttributesChangedEventListener>().StartListening();

      // Register the heartbeat loggers
      DIContext.Obtain<ITRexHeartBeatLogger>().AddContext(new MemoryHeartBeatLogger());
      DIContext.Obtain<ITRexHeartBeatLogger>().AddContext(new SpatialMemoryCacheHeartBeatLogger());      
    }

    static async Task<int> Main(string[] args)
    {
      EnsureAssemblyDependenciesAreLoaded();
      DependencyInjection();

      var cancelTokenSource = new CancellationTokenSource();
      AppDomain.CurrentDomain.ProcessExit += (s, e) =>
      {
        Console.WriteLine("Exiting");
        cancelTokenSource.Cancel();
      };

      DoServiceInitialisation();

      await Task.Delay(-1, cancelTokenSource.Token);
      return 0;
    }
  }
}
