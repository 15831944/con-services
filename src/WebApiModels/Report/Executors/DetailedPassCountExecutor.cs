﻿using ASNodeDecls;
using SVOICFilterSettings;
using System;
using System.Net;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Report.Models;
using VSS.Productivity3D.WebApi.Models.Report.ResultHandling;
using VSS.Productivity3D.WebApiModels.Report.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.Report.Executors
{
  /// <summary>
  /// The executor which passes the detailed pass counts request to Raptor
  /// </summary>
  public class DetailedPassCountExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public DetailedPassCountExecutor()
    {
      ProcessErrorCodes();
    }

    /// <summary>
    /// Processes the detailed pass counts request by passing the request to Raptor and returning the result.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a PassCountDetailedResult if successful</returns>     
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      ContractExecutionResult result = null;

      try
      {
        TPassCountDetails passCountDetails;
        PassCounts request = item as PassCounts;
        TICFilterSettings raptorFilter = RaptorConverters.ConvertFilter(request.filterID, request.filter, request.ProjectId,
          request.overrideStartUTC, request.overrideEndUTC, request.overrideAssetIds,log: log);
        var raptorResult = raptorClient.GetPassCountDetails(request.ProjectId ?? -1,
          ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor((Guid)(request.callId ?? Guid.NewGuid()), 0,
            TASNodeCancellationDescriptorType.cdtPassCountDetailed),
          request.passCountSettings != null ? ConvertSettings(request.passCountSettings) : new TPassCountSettings(),
          raptorFilter,
          RaptorConverters.ConvertLift(request.liftBuildSettings, raptorFilter.LayerMethod),
          out passCountDetails);
        //log.LogDebug($"Result from Raptor {success} with {JsonConvert.SerializeObject(passCountDetails)}");
        if (raptorResult == TASNodeErrorStatus.asneOK)
        {
          result = ConvertResult(passCountDetails, request.liftBuildSettings);
        }
        else
        {
          throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult((int)raptorResult,//ContractExecutionStatesEnum.FailedToGetResults,
            $"Failed to get requested pass count details data with error: {ContractExecutionStates.FirstNameWithOffset((int)raptorResult)}"));
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

    private PassCountDetailedResult ConvertResult(TPassCountDetails details, LiftBuildSettings liftSettings)
    {
      return new PassCountDetailedResult(
          ((liftSettings != null) && (liftSettings.OverridingTargetPassCountRange != null)) 
              ? liftSettings.OverridingTargetPassCountRange 
              : (!details.IsTargetPassCountConstant ? new TargetPassCountRange(0, 0) : new TargetPassCountRange(details.ConstantTargetPassCountRange.Min, details.ConstantTargetPassCountRange.Max)),
          details.IsTargetPassCountConstant,
          details.Percents, details.TotalAreaCoveredSqMeters);
    }

    private TPassCountSettings ConvertSettings(PassCountSettings settings)
    {
      return new TPassCountSettings
      {
        IsSummary = false,
        PassCounts = settings.passCounts
      };
    }
  }
}
