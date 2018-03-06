﻿using ASNodeDecls;
using SVOICFilterSettings;
using System;
using System.Net;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Filters.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApiModels.Report.Models;
using VSS.Productivity3D.WebApiModels.Report.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.Report.Executors
{
  /// <summary>
  /// The executor which passes the summary CCA request to Raptor
  /// </summary>
  public class SummaryCCAExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public SummaryCCAExecutor()
    {
      ProcessErrorCodes();
    }

    /// <summary>
    /// Processes the summary CCA request by passing the request to Raptor and returning the result.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a CCASummaryResult if successful</returns>      
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      ContractExecutionResult result = null;
      try
      {
        TCCASummary ccaSummary;
        CCARequest request = item as CCARequest;
        TICFilterSettings raptorFilter = RaptorConverters.ConvertFilter(request.filterID, request.filter, request.projectId);

        bool success = raptorClient.GetCCASummary(request.projectId ?? -1,
                            ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor((Guid)(request.callId ?? Guid.NewGuid()), 0, TASNodeCancellationDescriptorType.cdtCCASummary),
                            raptorFilter,
                            RaptorConverters.ConvertLift(request.liftBuildSettings, raptorFilter.LayerMethod),
                            out ccaSummary);
         
        if (success)
        {
          result = ConvertResult(ccaSummary);
        }
        else
        {
          throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
            $"Failed to get requested CCA summary data with error: {ContractExecutionStates.FirstNameWithOffset(ccaSummary.ReturnCode)}"));
        }

      }

      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
      return result;
    }

    protected sealed override void ProcessErrorCodes()
    {
      RaptorResult.AddErrorMessages(ContractExecutionStates);
    }

    
    private CCASummaryResult ConvertResult(TCCASummary summary)
    {
      return CCASummaryResult.CreateCCASummaryResult(
                summary.CompactedPercent,
                summary.OverCompactedPercent,
                summary.ReturnCode,
                summary.TotalAreaCoveredSqMeters,
                summary.UnderCompactedPercent);
    }

  }
}