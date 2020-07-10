﻿using System;
using System.Net;
using System.Threading.Tasks;
#if Raptor
using ASNode.DXF.RequestBoundaries.RPC;
using ASNodeDecls;
using VLPDDecls;
#endif
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models.Files;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;
using VSS.TRex.Gateway.Common.ResultHandling;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Executors
{
  /// <summary>
  /// Executor for processing DXF linework files.
  /// </summary>
  public class LineworkFileExecutor : RequestExecutorContainer
  {
    public LineworkFileExecutor()
    {
      ProcessErrorCodes();
    }

    protected sealed override void ProcessErrorCodes()
    {
#if RAPTOR
      RaptorResult.AddErrorMessages(ContractExecutionStates);
#endif
    }

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      if (!(item is DxfFileRequest dxfRequest))
      {
        throw new ServiceException(HttpStatusCode.InternalServerError, new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Request is not a DxfFileRequest"));
      }

      var request = new LineworkRequest(dxfRequest).Validate();

      //return UseTRexGateway("ENABLE_TREX_GATEWAY_LINEWORKFILE")
      //  ? await ProcessForTRex(request)
      //  : ProcessForRaptor(request);

      return await ProcessForTRex(request);
    }


    private async Task<DxfLineworkFileResult> ProcessForTRex(LineworkRequest request)
    {
#if !RAPTOR
      try
      {
        log.LogDebug($"{nameof(LineworkFileExecutor)}::{nameof(ProcessForTRex)}()");

        var req = new DXFBoundariesRequest(request.CoordinateSystemFileData, ImportedFileType.SiteBoundary, 
          request.DxfFileData, (DxfUnitsType)request.LineworkUnits, (uint)request.NumberOfBoundariesToProcess,
          request.ConvertLineStringCoordsToPolygon);
        var returnResult = await trexCompactionDataProxy.SendDataPostRequest<DXFBoundaryResult, DXFBoundariesRequest>(req, "files/dxf/boundaries");

        log.LogInformation($"RequestBoundariesFromLineWork: result: {JsonConvert.SerializeObject(returnResult)}");

        if (returnResult.Code != ContractExecutionStatesEnum.ExecutedSuccessfully)
        {
          throw CreateServiceException<LineworkFileExecutor>(returnResult.Code);
        }

        return new DxfLineworkFileResult(returnResult.Boundaries, returnResult.Code, returnResult.Message);
      }
      catch (ServiceException ex)
      {
        var errorMessage = ex.GetResult.Message;

        log.LogError($"RequestBoundariesFromLinework: exception {errorMessage}");

        return new DxfLineworkFileResult(ContractExecutionStatesEnum.InternalProcessingError, errorMessage, null);
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
#else
      throw new Exception("ProcessForTRex called with RAPTOR defined");
#endif
    }

    private DxfLineworkFileResult ProcessForRaptor(LineworkRequest request)
    {
#if Raptor
      var returnResult = TASNodeErrorStatus.asneUnknown;

      try
      {
        var customDescriptor = new TVLPDDesignDescriptor();
        customDescriptor.Init(0, string.Empty, string.Empty, request.DxfFileDescriptor.Path, request.DxfFileDescriptor.FileName, 0);

        log.LogDebug($"{nameof(LineworkFileExecutor)}::{nameof(ProcessForRaptor)}() : {nameof(TVLPDDesignDescriptor)} = {JsonConvert.SerializeObject(customDescriptor)}");

        var args = new TASNodeServiceRPCVerb_RequestBoundariesFromLinework_Args
        {
          DataModelID = request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID,
          LineworkDescriptor = customDescriptor,
          MaxVerticesPerBoundary = request.NumberOfVerticesPerBoundary,
          MaxBoundariesToProcess = request.NumberOfBoundariesToProcess,
          CoordSystemFileName = request.CoordinateSystemFileDescriptor.FileName,
          LineworkUnits = (TVLPDDistanceUnits)request.LineworkUnits
        };

        returnResult = raptorClient.GetBoundariesFromLinework(args, out var lineworksBoundary);

        log.LogInformation($"RequestBoundariesFromLinework: result: {JsonConvert.SerializeObject(returnResult)}");

        if (returnResult != TASNodeErrorStatus.asneOK)
        {
          throw CreateServiceException<LineworkFileExecutor>((int)returnResult);
        }

        return new DxfLineworkFileResult(returnResult, "", lineworksBoundary);
      }
      catch (ServiceException ex)
      {
        var errorMessage = ex.GetResult.Message;

        log.LogError($"RequestBoundariesFromLinework: exception {errorMessage}");

        return new DxfLineworkFileResult(returnResult, errorMessage, null);
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
#else
      throw new Exception("ProcessForRaptor called with RAPTOR not defined");
#endif
    }
  }
}
