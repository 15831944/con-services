﻿using System.Net;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApiModels.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.ProductionData.Executors
{
  public class GetLayerIdsExecutor : RequestExecutorContainer
    {
        protected override ContractExecutionResult ProcessEx<T>(T item)
        {
          ContractExecutionResult result;
          ProjectID request = item as ProjectID;
          TDesignLayer[] layerlist;
          if (raptorClient.GetOnMachineLayers(request.projectId ?? -1, out layerlist) >= 0 && layerlist != null)
            result = LayerIdsExecutionResult.CreateLayerIdsExecutionResult(layerlist);
          else
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
                "Failed to get requested layer ids"));

          return result;
        }
    }
}