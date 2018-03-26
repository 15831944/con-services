﻿using SVOICDecls;
using SVOICFilterSettings;
using System.Net;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Common.Models;

namespace VSS.Productivity3D.Common.Executors
{
  public class CellDatumExecutor : RequestExecutorContainer
    {
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      ContractExecutionResult result;
      CellDatumRequest request = item as CellDatumRequest;
      TCellProductionData data;

      TICFilterSettings raptorFilter = RaptorConverters.ConvertFilter(request.filterId, request.filter, request.projectId);
      if (raptorClient.GetCellProductionData
      (request.projectId ?? -1,
        (int) RaptorConverters.convertDisplayMode(request.displayMode),
        request.gridPoint?.x ?? 0,
        request.gridPoint?.y ?? 0,
        request.llPoint != null ? RaptorConverters.convertWGSPoint(request.llPoint) : new TWGS84Point(),                    
        request.llPoint == null,
        raptorFilter,
        RaptorConverters.ConvertLift(request.liftBuildSettings, raptorFilter.LayerMethod),
        RaptorConverters.DesignDescriptor(request.design),
        out data))
      {
        result = ConvertCellDatumResult(data, request);
      }
      else
      {
        throw new ServiceException(HttpStatusCode.BadRequest,  new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
          "No cell datum returned"));
      }

      return result;
    }
    
    protected virtual CellDatumResponse ConvertCellDatumResult(TCellProductionData result, CellDatumRequest request)
    {
      return CellDatumResponse.CreateCellDatumResponse(
          RaptorConverters.convertDisplayMode((TICDisplayMode) result.DisplayMode),
              result.ReturnCode,
              result.Value,
              result.TimeStampUTC);
            
    }
  }
}