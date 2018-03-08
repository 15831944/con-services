﻿using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies;
using VSS.Productivity3D.Common.Extensions;
using VSS.Productivity3D.Common.Filters.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;
using VSS.Productivity3D.WebApiModels.Compaction.Interfaces;
using VSS.Productivity3D.WebApiModels.ProductionData.Executors;
using VSS.Productivity3D.WebApiModels.Report.Executors;
using VSS.Productivity3D.WebApiModels.Report.Models;
using Filter = VSS.Productivity3D.Common.Models.Filter;


namespace VSS.Productivity3D.WebApiModels.Compaction.Helpers
{
  /// <summary>
  /// Proxy for getting elevation statistics from Raptor. Used by elevation range, elevation palette and elevation tiles requests.
  /// </summary>
  public class ElevationExtentsProxy : IElevationExtentsProxy
  {

    /// <summary>
    /// Logger factory for use by executor
    /// </summary>
    private readonly ILoggerFactory logger;

    /// <summary>
    /// Cache for elevation extents
    /// </summary>
    private readonly IMemoryCache elevationExtentsCache;

    /// <summary>
    /// Raptor client for use by executor
    /// 
    /// </summary>
    private readonly IASNodeClient raptorClient;

    /// <summary>
    /// For getting compaction settings for a project
    /// </summary>
    private readonly ICompactionSettingsManager settingsManager;

    private readonly string elevationExtentsCacheLifeKey = "ELEVATION_EXTENTS_CACHE_LIFE";

    /// <summary>
    /// Logger for logging
    /// </summary>
    private readonly ILogger log;

    /// <summary>
    /// Where to get environment variables, connection string etc. from
    /// </summary>
    private readonly IConfigurationStore configStore;

    /// <summary>
    /// Constructor with injection
    /// </summary>
    /// <param name="raptorClient">Raptor client</param>
    /// <param name="logger">Logger</param>
    /// <param name="cache">Elevation extents cache</param>
    /// <param name="settingsManager">Compaction settings manager</param>
    /// <param name="configStore">Configuration store</param>
    public ElevationExtentsProxy(IASNodeClient raptorClient, ILoggerFactory logger, IMemoryCache cache, ICompactionSettingsManager settingsManager, IConfigurationStore configStore)
    {
      this.raptorClient = raptorClient;
      this.logger = logger;
      this.log = logger.CreateLogger<ElevationExtentsProxy>();
      elevationExtentsCache = cache;
      this.settingsManager = settingsManager;
      this.configStore = configStore;
    }


    /// <summary>
    /// Gets the elevation statistics for the given filter from Raptor
    /// </summary>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="filter">Compaction filter</param>
    /// <param name="projectSettings">Project settings</param>
    /// <returns>Elevation statistics</returns>
    public ElevationStatisticsResult GetElevationRange(long projectId, Filter filter,
      CompactionProjectSettings projectSettings)
    {
      var cacheKey = ElevationCacheKey(projectId, filter);
      var strFilter = filter != null ? JsonConvert.SerializeObject(filter) : "";
      var opts = (new MemoryCacheEntryOptions()).GetCacheOptions(elevationExtentsCacheLifeKey, configStore, log);

      return elevationExtentsCache.GetOrAdd(cacheKey, opts, () =>
      {
        ElevationStatisticsResult result;
        if (filter == null || (filter.isFilterContainsSSOnly) || (filter.IsFilterEmpty))
        {
          log.LogDebug(
            $"Calling elevation statistics from Project Extents for project {projectId} and filter {strFilter}");

          var projectExtentsRequest = ExtentRequest.CreateExtentRequest(projectId,
            filter != null ? filter.SurveyedSurfaceExclusionList.ToArray() : null);
          var extents =
            RequestExecutorContainerFactory.Build<ProjectExtentsSubmitter>(logger, raptorClient)
              .Process(projectExtentsRequest) as ProjectExtentsResult;
          result = ElevationStatisticsResult.CreateElevationStatisticsResult(
            BoundingBox3DGrid.CreatBoundingBox3DGrid(extents.ProjectExtents.minX, extents.ProjectExtents.minY,
              extents.ProjectExtents.minZ, extents.ProjectExtents.maxX, extents.ProjectExtents.maxY,
              extents.ProjectExtents.maxZ), extents.ProjectExtents.minZ, extents.ProjectExtents.maxZ, 0);
        }
        else
        {
          log.LogDebug(
            $"Calling elevation statistics from Elevation Statistics for project {projectId} and filter {strFilter}");

          LiftBuildSettings liftSettings = settingsManager.CompactionLiftBuildSettings(projectSettings);

          ElevationStatisticsRequest statsRequest =
            ElevationStatisticsRequest.CreateElevationStatisticsRequest(projectId, null, filter, 0,
              liftSettings);
          statsRequest.Validate();

          result =
            RequestExecutorContainerFactory.Build<ElevationStatisticsExecutor>(logger, raptorClient)
              .Process(statsRequest) as ElevationStatisticsResult;

        }
        //Check for 'No elevation range' result
        const double NO_ELEVATION = 10000000000.0;
        if (Math.Abs(result.MinElevation - NO_ELEVATION) < 0.001 &&
            Math.Abs(result.MaxElevation + NO_ELEVATION) < 0.001)
        {
          result = null;
        }
        log.LogDebug($"Done elevation request");
        return result;
      });
    }

    /// <summary>
    /// Gets the key for the elevation extents cache
    /// </summary>
    /// <param name="projectId">project ID</param>
    /// <param name="filter">Compaction filter</param>
    /// <returns>Cache key</returns>
    private string ElevationCacheKey(long projectId, Filter filter)
    {
      var filterHash = filter == null ? 0 : filter.GetHashCode();
      return $"{projectId},{filterHash}";
    }
  }
}