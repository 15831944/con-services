﻿using System;
using System.Collections.Generic;
using System.IO;
using Apache.Ignite.Core;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSS.TRex.Alignments;
using VSS.TRex.Alignments.Interfaces;
using VSS.TRex.Designs;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.Events;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.GridFabric.Factories;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SiteModels.Interfaces.Events;
using VSS.TRex.Storage;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SubGridTrees.Server.Utilities;
using VSS.TRex.SurveyedSurfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.TAGFiles.Classes.Queues;
using VSS.TRex.TAGFiles.Executors;
using VSS.TRex.TAGFiles.Models;
using VSS.TRex.Types;

namespace VSS.TRex.Tests.TestFixtures
{
  public class DITagFileFixture : DILoggingFixture, IDisposable
  {
    private TAGFileBufferQueue _tagFileBufferQueue;

    public override void ClearDynamicFixtureContent()
    {
      base.ClearDynamicFixtureContent();

      _tagFileBufferQueue = null;
    }

    public static Guid NewSiteModelGuid => Guid.NewGuid();

    public static TAGFileConverter ReadTAGFile(string fileName, Guid assetUid, bool isJohnDoe)
    {
      var converter = new TAGFileConverter();

      using var fs = new FileStream(Path.Combine("TestData", "TAGFiles", fileName), FileMode.Open, FileAccess.Read);
      converter.ExecuteLegacyTAGFile(fileName, fs, assetUid, isJohnDoe);

      return converter;
    }

    public static TAGFileConverter ReadTAGFile(string fileName, Guid assetUid, bool isJohnDoe , ref ISiteModel siteModel)
    {
      var converter = new TAGFileConverter(siteModel.ID);
      converter.SiteModel = siteModel;
      using var fs = new FileStream(Path.Combine("TestData", "TAGFiles", fileName), FileMode.Open, FileAccess.Read);
      converter.ExecuteLegacyTAGFile(fileName, fs, assetUid, isJohnDoe);

      return converter;
    }

    public static TAGFileConverter ReadTAGFile(string subFolder, string fileName, bool treatAsJohnDoeMachine = false)
    {
      var converter = new TAGFileConverter();
      var fn = Path.Combine("TestData", "TAGFiles", subFolder, fileName);

      using var fs = new FileStream(fn, FileMode.Open, FileAccess.Read);
      converter.ExecuteLegacyTAGFile(fileName, fs, Guid.NewGuid(), treatAsJohnDoeMachine);

      return converter;
    }

    public static TAGFileConverter ReadTAGFileFullPath(string fileName, bool treatAsJohnDoeMachine = false)
    {
      var converter = new TAGFileConverter();

      using var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
      converter.ExecuteLegacyTAGFile(fileName, fs, Guid.NewGuid(), treatAsJohnDoeMachine);

      return converter;
    }

    public void AddProxyCacheFactoriesToDI()
    {
      _tagFileBufferQueue = null;

      DIBuilder
        .Continue()

        // Add the factories for the storage proxy caches, both standard and transacted, for spatial and non spatial caches in TRex

        /////////////////////////////////////////////////////
        // Injected standard storage proxy cache factories
        /////////////////////////////////////////////////////
        
        .Add(x => x.AddSingleton<Func<IIgnite, StorageMutability, FileSystemStreamType, IStorageProxyCache<ISubGridSpatialAffinityKey, ISerialisedByteArrayWrapper>>>
          (factory => (ignite, mutability, streamType) => new StorageProxyCacheTransacted_TestHarness<ISubGridSpatialAffinityKey, ISerialisedByteArrayWrapper>(ignite?.GetCache<ISubGridSpatialAffinityKey, ISerialisedByteArrayWrapper>(TRexCaches.SpatialCacheName(mutability, streamType)), new SubGridSpatialAffinityKeyEqualityComparer())))

        .Add(x => x.AddSingleton<Func<IIgnite, StorageMutability, FileSystemStreamType, IStorageProxyCache<INonSpatialAffinityKey, ISerialisedByteArrayWrapper>>>
          (factory => (ignite, mutability, streamType) => new StorageProxyCacheTransacted_TestHarness<INonSpatialAffinityKey, ISerialisedByteArrayWrapper>(ignite?.GetCache<INonSpatialAffinityKey, ISerialisedByteArrayWrapper>(TRexCaches.NonSpatialCacheName(mutability, streamType)), new NonSpatialAffinityKeyEqualityComparer())))

        .Add(x => x.AddSingleton<Func<IIgnite, StorageMutability, FileSystemStreamType, IStorageProxyCache<ISiteModelMachineAffinityKey, ISerialisedByteArrayWrapper>>>
          (factory => (ignite, mutability, streamType) => new StorageProxyCacheTransacted_TestHarness<ISiteModelMachineAffinityKey, ISerialisedByteArrayWrapper>(ignite?.GetCache<ISiteModelMachineAffinityKey, ISerialisedByteArrayWrapper>(TRexCaches.NonSpatialCacheName(mutability, streamType)), new SiteModelMachineAffinityKeyEqualityComparer())))

        /////////////////////////////////////////////////////
        // Injected transacted storage proxy cache factories
        /////////////////////////////////////////////////////

        .Add(x => x.AddSingleton<Func<IIgnite, StorageMutability, FileSystemStreamType, IStorageProxyCacheTransacted<ISubGridSpatialAffinityKey, ISerialisedByteArrayWrapper>>>
          (factory => (ignite, mutability, streamType) => new StorageProxyCacheTransacted_TestHarness<ISubGridSpatialAffinityKey, ISerialisedByteArrayWrapper>(ignite?.GetCache<ISubGridSpatialAffinityKey, ISerialisedByteArrayWrapper>(TRexCaches.SpatialCacheName(mutability, streamType)), new SubGridSpatialAffinityKeyEqualityComparer())))

        .Add(x => x.AddSingleton<Func<IIgnite, StorageMutability, FileSystemStreamType, IStorageProxyCacheTransacted<INonSpatialAffinityKey, ISerialisedByteArrayWrapper>>>
          (factory => (ignite, mutability, streamType) => new StorageProxyCacheTransacted_TestHarness<INonSpatialAffinityKey, ISerialisedByteArrayWrapper>(ignite?.GetCache<INonSpatialAffinityKey, ISerialisedByteArrayWrapper>(TRexCaches.NonSpatialCacheName(mutability, streamType)), new NonSpatialAffinityKeyEqualityComparer())))

        .Add(x => x.AddSingleton<Func<IIgnite, StorageMutability, FileSystemStreamType, IStorageProxyCacheTransacted<ISiteModelMachineAffinityKey, ISerialisedByteArrayWrapper>>>
          (factory => (ignite, mutability, streamType) => new StorageProxyCacheTransacted_TestHarness<ISiteModelMachineAffinityKey, ISerialisedByteArrayWrapper>(ignite?.GetCache<ISiteModelMachineAffinityKey, ISerialisedByteArrayWrapper>(TRexCaches.NonSpatialCacheName(mutability, streamType)), new SiteModelMachineAffinityKeyEqualityComparer())))

        .Add(x => x.AddSingleton<Func<ITAGFileBufferQueue>>(factory => () =>
        {
          _tagFileBufferQueue ??= new TAGFileBufferQueue();
          return _tagFileBufferQueue;
        }))

        .Build();

      // Set up a singleton storage proxy for mutable and immutable contexts for tests when there is no Ignite mock available
      var mutableStorageProxy = new StorageProxy_Ignite_Transactional(StorageMutability.Mutable);
      var immutableStorageProxy = new StorageProxy_Ignite_Transactional(StorageMutability.Immutable);

      DIBuilder
        .Continue()

        // Add the factory to create a single storage proxy instance.
        .Add(x => x.AddSingleton<Func<StorageMutability, IStorageProxy>>(factory => mutability => mutability == StorageMutability.Mutable ? mutableStorageProxy : immutableStorageProxy))
        .Add(x => x.AddSingleton<IStorageProxyFactory>(new StorageProxyFactory()))
        .Build();
    }

    public override void SetupFixture()
    {
      base.SetupFixture();

      var mockSiteModelMetadataManager = new Mock<ISiteModelMetadataManager>();
      var mockSiteModelAttributesChangedEventSender = new Mock<ISiteModelAttributesChangedEventSender>();

      DIBuilder
        .Continue()

        .Add(x => AddProxyCacheFactoriesToDI())

        .Add(x => x.AddSingleton<ISubGridSpatialAffinityKeyFactory>(new SubGridSpatialAffinityKeyFactory()))

        .Add(x => x.AddSingleton<ISiteModels>(new TRex.SiteModels.SiteModels(StorageMutability.Immutable)))
        .Add(x => x.AddSingleton<ISiteModelFactory>(new SiteModelFactory()))
        .Add(x => x.AddSingleton<ISiteModelMetadataManager>(factory => new SiteModelMetadataManager(StorageMutability.Mutable)))

        .Add(x => x.AddTransient<ISurveyedSurfaces>(factory => new TRex.SurveyedSurfaces.SurveyedSurfaces()))

        .Add(x => x.AddSingleton<IProductionEventsFactory>(new ProductionEventsFactory()))
        .Add(x => x.AddSingleton<IMutabilityConverter>(new MutabilityConverter()))
        .Add(x => x.AddSingleton<ISiteModelMetadataManager>(mockSiteModelMetadataManager.Object))

        .Add(x => x.AddTransient<IDesigns>(factory => new TRex.Designs.Storage.Designs()))
        .Add(x => x.AddSingleton<IDesignManager>(factory => new DesignManager(StorageMutability.Mutable)))
        .Add(x => x.AddSingleton<ISurveyedSurfaceManager>(factory => new SurveyedSurfaceManager(StorageMutability.Mutable)))
        .Add(x => x.AddTransient<IAlignments>(factory => new TRex.Alignments.Alignments()))
        .Add(x => x.AddSingleton<IAlignmentManager>(factory => new AlignmentManager(StorageMutability.Mutable)))

        .Add(x => x.AddSingleton<ISiteModelAttributesChangedEventSender>(mockSiteModelAttributesChangedEventSender.Object))

        // Register the hook used to capture cell pass mutation events while processing TAG files.
        .Add(x => x.AddSingleton<ICell_NonStatic_MutationHook>(new Cell_NonStatic_MutationHook()))

        .Complete();

        MockACSDependencies(); // default mocking behaviour for ACS tagfiles
    }

    private void MockACSDependencies()
    {
      var mockACSTranslator = new Mock<IACSTranslator>();
      mockACSTranslator
        .Setup(x => x.TranslatePositions(It.IsAny<Guid?>(), It.IsAny<List<UTMCoordPointPair>>()))
        .Returns((Guid? x, List<UTMCoordPointPair> y) => y);

      DIBuilder
        .Continue()
        .Add(x => x.AddSingleton<IACSTranslator>(mockACSTranslator.Object))
        .Complete();
    }

    public DITagFileFixture()
    {
      SetupFixture();
    }

    public override void Dispose()
    {
      base.Dispose();
    }
  }
}
