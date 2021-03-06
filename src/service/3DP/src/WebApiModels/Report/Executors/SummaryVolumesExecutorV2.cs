﻿using System.Threading.Tasks;
using CCSS.Productivity3D.Service.Common;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Productivity3D.Models.Compaction.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Report.Models;

namespace VSS.Productivity3D.WebApi.Models.Report.Executors
{
  /// <summary>
  /// Summary volumes executor for use with API v2.
  /// </summary>
  public class SummaryVolumesExecutorV2 : RequestExecutorContainer
  {
    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public SummaryVolumesExecutorV2()
    {
      ProcessErrorCodes();
    }

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      try
      {
        var request = CastRequestObjectTo<SummaryVolumesRequest>(item);

        var baseFilter = request.BaseFilter;
        var topFilter = request.TopFilter;
        if (request.VolumeCalcType == VolumesType.Between2Filters)
        {
          if (!request.ExplicitFilters)
          {
            (baseFilter, topFilter) = FilterUtilities.AdjustFilterToFilter(request.BaseFilter, request.TopFilter);
          }
        }
        else
        {
          // Note: The use of the ReconcileTopFilterAndVolumeComputationMode() here breaks with the pattern of all the other V2
          // end points which explicitly do not perform this step. It has been copied from the Raptor implementation of this end point
          (baseFilter, topFilter) = FilterUtilities.ReconcileTopFilterAndVolumeComputationMode(baseFilter, topFilter, request.VolumeCalcType);
        }

        var summaryVolumesRequest = new SummaryVolumesDataRequest(
            request.ProjectUid,
            baseFilter,
            topFilter,
            request.BaseDesignDescriptor?.FileUid ,
            request.BaseDesignDescriptor?.Offset,
            request.TopDesignDescriptor?.FileUid,
            request.TopDesignDescriptor?.Offset,
            request.VolumeCalcType);

          return await trexCompactionDataProxy.SendDataPostRequest<SummaryVolumesResult, SummaryVolumesDataRequest>(summaryVolumesRequest, "/volumes/summary", customHeaders);
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
    }

    protected sealed override void ProcessErrorCodes()
    {
    }
  }
}
