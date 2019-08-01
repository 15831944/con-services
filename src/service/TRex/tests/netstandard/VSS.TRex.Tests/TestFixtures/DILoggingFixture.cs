﻿using System;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSS.Common.Abstractions.Configuration;
using VSS.TRex.Common;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Factories;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.IO;
using VSS.TRex.IO.Helpers;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.SubGridTrees.Server.Interfaces;

namespace VSS.TRex.Tests.TestFixtures
{
  public class DILoggingFixture : IDisposable
  {
    public void SetupFixture()
    {
      DIBuilder
        .New()
        .AddLogging()
        .Add(VSS.TRex.IO.DIUtilities.AddPoolCachesToDI)
        .Add(VSS.TRex.Cells.DIUtilities.AddPoolCachesToDI)

        .Add(x => x.AddSingleton<Mock<IConfigurationStore>>(mock =>
        {
          var config = new Mock<IConfigurationStore>();
          config.Setup(c => c.GetValueInt("NUMPARTITIONS_PERDATACACHE", It.IsAny<int>())).Returns(Consts.NUMPARTITIONS_PERDATACACHE);

          config.Setup(c => c.GetValueInt("VLPDSUBGRID_SEGMENTPASSCOUNTLIMIT", It.IsAny<int>())).Returns(Consts.VLPDSUBGRID_SEGMENTPASSCOUNTLIMIT);
          config.Setup(c => c.GetValueInt("VLPDSUBGRID_MAXSEGMENTCELLPASSESLIMIT", It.IsAny<int>())).Returns(Consts.VLPDSUBGRID_MAXSEGMENTCELLPASSESLIMIT);
          config.Setup(c => c.GetValueBool("SEGMENTCLEAVINGOOPERATIONS_TOLOG", It.IsAny<bool>())).Returns(true /*Consts.SEGMENTCLEAVINGOOPERATIONS_TOLOG*/);
          config.Setup(c => c.GetValueBool("ITEMSPERSISTEDVIADATAPERSISTOR_TOLOG", It.IsAny<bool>())).Returns(Consts.ITEMSPERSISTEDVIADATAPERSISTOR_TOLOG);
          config.Setup(c => c.GetValueBool("DEBUG_PERFORMSEGMENT_ADDITIONALINTEGRITYCHECKS", It.IsAny<bool>())).Returns(true /*Consts.DEBUG_PERFORMSEGMENT_ADDITIONALINTEGRITYCHECKS*/);

          config.Setup(c => c.GetValueInt("VLPDPSNODE_CELL_PASS_AGGREGATOR_LIST_SIZE_INCREMENT_DEFAULT", It.IsAny<int>())).Returns(Consts.VLPDPSNODE_CELL_PASS_AGGREGATOR_LIST_SIZE_INCREMENT_DEFAULT);

          config.Setup(c => c.GetValueBool("ADVISEOTHERSERVICES_OFMODELCHANGES", It.IsAny<bool>())).Returns(true /*Consts.ADVISEOTHERSERVICES_OFMODELCHANGES*/);

          config.Setup(c => c.GetValueInt("MAX_MAPPED_TAG_FILES_TO_PROCESS_PER_AGGREGATION_EPOCH", It.IsAny<int>())).Returns(Consts.MAX_MAPPED_TAG_FILES_TO_PROCESS_PER_AGGREGATION_EPOCH);
          config.Setup(c => c.GetValueInt("MAX_GROUPED_TAG_FILES_TO_PROCESS_PER_PROCESSING_EPOCH", It.IsAny<int>())).Returns(Consts.MAX_GROUPED_TAG_FILES_TO_PROCESS_PER_PROCESSING_EPOCH);
          
          config.Setup(c => c.GetValueInt("HEARTBEAT_LOGGER_INTERVAL")).Returns(Consts.HEARTBEAT_LOGGER_INTERVAL);

          config.Setup(c => c.GetValueInt("GENERAL_SUBGRID_RESULT_CACHE_MAXIMUM_ELEMENT_COUNT", It.IsAny<int>())).Returns(Consts.GENERAL_SUBGRID_RESULT_CACHE_MAXIMUM_ELEMENT_COUNT);
          config.Setup(c => c.GetValueLong("GENERAL_SUBGRID_RESULT_CACHE_MAXIMUM_SIZE", It.IsAny<long>())).Returns(Consts.GENERAL_SUBGRID_RESULT_CACHE_MAXIMUM_SIZE);
          config.Setup(c => c.GetValueDouble("GENERAL_SUBGRID_RESULT_CACHE_DEAD_BAND_FRACTION", It.IsAny<double>())).Returns(Consts.GENERAL_SUBGRID_RESULT_CACHE_DEAD_BAND_FRACTION);

          config.Setup(c => c.GetValueInt("SUBGRIDTREENODE_CELLSPARCITYLIMIT", It.IsAny<int>())).Returns(Consts.SUBGRIDTREENODE_CELLSPARCITYLIMIT);

          config.Setup(c => c.GetValueBool("ENABLE_TAGFILE_ARCHIVING_METADATA", It.IsAny<bool>())).Returns(Consts.ENABLE_TAGFILE_ARCHIVING_METADATA);
          config.Setup(c => c.GetValueBool("ENABLE_TAGFILE_ARCHIVING", It.IsAny<bool>())).Returns(Consts.ENABLE_TAGFILE_ARCHIVING);

          config.Setup(c => c.GetValueBool("ENABLE_GENERAL_SUBGRID_RESULT_CACHING", It.IsAny<bool>())).Returns(true /*Consts.ENABLE_GENERAL_SUBGRID_RESULT_CACHING*/);
          config.Setup(c => c.GetValueBool("DEBUG_DRAWDIAGONALCROSS_ONRENDEREDTILES", It.IsAny<bool>())).Returns(true /*Consts.DEBUG_DRAWDIAGONALCROSS_ONRENDEREDTILES*/);

          config.Setup(c => c.GetValueInt("MAX_EXPORT_ROWS")).Returns(Consts.DEFAULT_MAX_EXPORT_ROWS);
          config.Setup(c => c.GetValueInt("MAX_EXPORT_ROWS", It.IsAny<int>())).Returns(Consts.DEFAULT_MAX_EXPORT_ROWS);

          config.Setup(c => c.GetValueInt("SPATIAL_MEMORY_CACHE_INTER_EPOCH_SLEEP_TIME_SECONDS", It.IsAny<int>())).Returns(Consts.SPATIAL_MEMORY_CACHE_INTER_EPOCH_SLEEP_TIME_SECONDS);
          config.Setup(c => c.GetValueInt("SPATIAL_MEMORY_CACHE_INVALIDATED_CACHE_CONTEXT_REMOVAL_WAIT_TIME_SECONDS", It.IsAny<int>())).Returns(Consts.SPATIAL_MEMORY_CACHE_INVALIDATED_CACHE_CONTEXT_REMOVAL_WAIT_TIME_SECONDS);

          config.Setup(c => c.GetValueInt("NUM_CONCURRENT_TAG_FILE_PROCESSING_TASKS", It.IsAny<int>())).Returns(Consts.NUM_CONCURRENT_TAG_FILE_PROCESSING_TASKS);

          config.Setup(c => c.GetValueBool("TREX_USE_SYNC_TASKS_FOR_STORAGE_PROXY_IGNITE_TRANSACTIONAL_COMMITS", It.IsAny<bool>())).Returns(true);

          config.Setup(c => c.GetValueInt("MIN_TAGFILE_LENGTH", It.IsAny<int>())).Returns(Consts.kMinTagFileLengthDefault);
          config.Setup(c => c.GetValueBool("ENABLE_TFA_SERVICE", It.IsAny<bool>())).Returns(Consts.ENABLE_TFA_SERVICE);
          config.Setup(c => c.GetValueString("TAGFILE_ARCHIVE_FOLDER", It.IsAny<string>())).Returns("");

          config.Setup(c => c.GetValueString("AWS_BUCKET_NAME")).Returns("UnitTestAWSBucketKey");
          config.Setup(c => c.GetValueString("AWS_BUCKET_NAME", It.IsAny<string>())).Returns("UnitTestAWSBucketKey");

          config.Setup(c => c.GetValueString(CoordinatesServiceClient.COORDINATE_SERVICE_URL_ENV_KEY, It.IsAny<string>())).Returns("https://api-stg.trimble.com/t/trimble.com/coordinates/1.0");
          config.Setup(c => c.GetValueString(CoordinatesServiceClient.COORDINATE_SERVICE_URL_ENV_KEY)).Returns("https://api-stg.trimble.com/t/trimble.com/coordinates/1.0");

          return config;
        }))
        .Build()
        .Add(x => x.AddSingleton<IConfigurationStore>(DIContext.Obtain<Mock<IConfigurationStore>>().Object))
        .Add(x => x.AddSingleton(ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory()))
        .Add(x => x.AddSingleton<ISubGridCellSegmentPassesDataWrapperFactory>(new SubGridCellSegmentPassesDataWrapperFactory()))
        .Add(x => x.AddSingleton<ISubGridCellLatestPassesDataWrapperFactory>(new SubGridCellLatestPassesDataWrapperFactory()))
        .Add(x => x.AddSingleton<ISubGridSpatialAffinityKeyFactory>(new SubGridSpatialAffinityKeyFactory()))
        .Complete();
    }

    public static void SetMaxExportRowsConfig(int rowCount)
    {
      // this Fixture sets to Consts.DEFAULT_MAX_EXPORT_ROWS. Some tests need it to be something different.
      var moqConfiguration = DIContext.Obtain<Mock<IConfigurationStore>>();
      moqConfiguration.Setup(x => x.GetValueInt("MAX_EXPORT_ROWS")).Returns(rowCount);
      moqConfiguration.Setup(x => x.GetValueInt("MAX_EXPORT_ROWS", It.IsAny<int>())).Returns(rowCount);

      DIBuilder.Continue().Add(x => x.AddSingleton(moqConfiguration.Object)).Complete();

      var configuration = DIContext.Obtain<IConfigurationStore>();
      configuration.GetValueInt("MAX_EXPORT_ROWS").Should().Be(rowCount);
      configuration.GetValueInt("MAX_EXPORT_ROWS", 1).Should().Be(rowCount);
    }

    public void ClearHelpers()
    {
      RecyclableMemoryStreamManagerHelper.Clear();
      GenericArrayPoolCachesRegister.ClearAll();
      GenericTwoDArrayCacheRegister.ClearAll();
      GenericSlabAllocatedArrayPoolRegister.ClearAll();
    }

    public DILoggingFixture()
    {
      ClearHelpers();
      SetupFixture();
    }

    public void Dispose()
    {
      ClearHelpers();
      DIBuilder.Eject();
    }
  }
}
