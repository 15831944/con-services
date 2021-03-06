﻿using System;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.TRex.Alignments.Interfaces;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.Gateway.Common.Helpers;

namespace VSS.TRex.Gateway.WebApi.ActionServices
{
  /// <summary>
  /// Check that the site model exists, and appropriate designs exist.
  /// </summary>
  public class ReportDataValidationUtility : IReportDataValidationUtility
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="requestObj"></param>
    /// <returns></returns>
    /// <exception cref="ServiceException"></exception>
    public bool ValidateData(string method, Guid? projectUid, object requestObj)
    {
      if (projectUid == null || projectUid == Guid.Empty)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Invalid project UID."));
      }
      GatewayHelper.ValidateAndGetSiteModel(method, projectUid.Value);

      var request = requestObj as CompactionReportTRexRequest;

      if (request.CutFillDesignUid.HasValue && 
          !(request.CutFillDesignUid == Guid.Empty) &&
          DIContext.Obtain<IDesignManager>().List(request.ProjectUid).Locate(request.CutFillDesignUid.Value) == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            $"CutFill design {request.CutFillDesignUid.Value} is not found."));
      }

      if (requestObj != null &&
          requestObj.GetType() == typeof(CompactionReportStationOffsetTRexRequest))
      {
        var alignmentUid = ((CompactionReportStationOffsetTRexRequest) requestObj).AlignmentDesignUid;
        if (!(alignmentUid == Guid.Empty) &&
            DIContext.Obtain<IAlignmentManager>().List(request.ProjectUid).Locate(alignmentUid) == null)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              $"Alignment design {alignmentUid} is not found."));
        }
      }

      return true;
    }
  }
}
