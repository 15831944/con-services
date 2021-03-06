﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CCSS.Productivity3D.Service.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Productivity3D.Models.Compaction.ResultHandling;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.WebApi.Models.Report.Executors;
using VSS.TRex.Gateway.Common.Abstractions;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Helpers
{
  /// <summary>
  /// To support getting ProjectStatistics from either Raptor or TRex
  ///    we need to muck around converting between Ids and Guids
  /// </summary>
  public class ProjectStatisticsHelper
  {
    private readonly ILoggerFactory _loggerFactory;
    private readonly IConfigurationStore _configStore;
    private readonly IFileImportProxy _fileImportProxy;
    private readonly ITRexCompactionDataProxy _tRexCompactionDataProxy;
    private readonly DesignUtilities _designUtilities;

#if RAPTOR
    private readonly IASNodeClient _raptorClient;
#endif

    public ProjectStatisticsHelper(ILoggerFactory loggerFactory, IConfigurationStore configStore,
      IFileImportProxy fileImportProxy, ITRexCompactionDataProxy tRexCompactionDataProxy, ILogger log
#if RAPTOR
        , IASNodeClient raptorClient
#endif
      )
    {
      _loggerFactory = loggerFactory;
      _configStore = configStore;
      _fileImportProxy = fileImportProxy;
      _tRexCompactionDataProxy = tRexCompactionDataProxy;
      _designUtilities = new DesignUtilities(log, configStore, fileImportProxy);
#if RAPTOR
      _raptorClient = raptorClient;
#endif
    }

    /// <summary>
    /// Gets the ids and uids of the surveyed surfaces to exclude from Raptor/TRex calculations. 
    /// This is the deactivated ones.
    /// </summary>
    public Task<List<(long, Guid)>> GetExcludedSurveyedSurfaceIds(Guid projectUid, string userId, IHeaderDictionary customHeaders)
    {
      return _designUtilities.GetExcludedSurveyedSurfaceIds(projectUid, userId, customHeaders);
    }

    /// <summary>
    /// Get project statistics using all excluded surveyed surfaces.
    /// </summary>
    public async Task<ProjectStatisticsResult> GetProjectStatisticsWithProjectSsExclusions(Guid projectUid, long projectId, string userId, IHeaderDictionary customHeaders)
    {
      var excludedIds = await GetExcludedSurveyedSurfaceIds(projectUid, userId, customHeaders);

      return await GetProjectStatisticsWithSsExclusions(projectUid, projectId, excludedIds?.Select(e => e.Item1), excludedIds?.Select(e => e.Item2), userId);
    }

    /// <summary>
    /// Get project statistics using excluded surveyed surfaces provided in the request.
    /// This is used for v1 requests where the excluded surveyed surfaces are provided in the request
    /// </summary>
    public async Task<ProjectStatisticsResult> GetProjectStatisticsWithRequestSsExclusions(Guid projectUid, long projectId, string userId, long[] excludedIds, IHeaderDictionary customHeaders)
    {
      Guid[] excludedUids = null;
      if (excludedIds != null && excludedIds.Length > 0)
      {
        var excludedSs = await GetExcludedSurveyedSurfaceIds(projectUid, userId, customHeaders);
        excludedUids = excludedSs == null || excludedSs.Count == 0 ? null :
          excludedSs.Where(e => excludedIds.Contains(e.Item1)).Select(e => e.Item2).ToArray();
      }
      return await GetProjectStatisticsWithSsExclusions(projectUid, projectId, excludedIds, excludedUids, userId);
    }

    /// <summary>
    /// Get project statistics using excluded surveyed surfaces provided in the filter.
    /// </summary>
    public Task<ProjectStatisticsResult> GetProjectStatisticsWithFilterSsExclusions(Guid projectUid, long projectId, IEnumerable<long> excludedIds, IEnumerable<Guid> excludedUids, string userId)
    {
      return GetProjectStatisticsWithSsExclusions(projectUid, projectId, excludedIds, excludedUids, userId);
    }

    /// <summary>
    /// Get project statistics using excluded surveyed surfaces.
    /// </summary>
    private async Task<ProjectStatisticsResult> GetProjectStatisticsWithSsExclusions(Guid projectUid, long projectId, IEnumerable<long> excludedIds, IEnumerable<Guid> excludedUids, string userId)
    {
      var request = new ProjectStatisticsMultiRequest(projectUid, projectId, excludedUids?.ToArray(), excludedIds?.ToArray());

      return await
        RequestExecutorContainerFactory.Build<ProjectStatisticsExecutor>(_loggerFactory,
#if RAPTOR
            _raptorClient,
#endif
            configStore: _configStore, trexCompactionDataProxy: _tRexCompactionDataProxy,
            userId: userId, fileImportProxy: _fileImportProxy)
          .ProcessAsync(request) as ProjectStatisticsResult;
    }
  }
}
