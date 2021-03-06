﻿using System.Collections.Generic;
using System.Linq;
using System.Net;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Models.Designs;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Executors
{
  /// <summary>
  /// Executes POST, PUT,GET and DELETE methods on Surveyed Surfaces resource.
  /// </summary>
  public class SurveyedSurfaceExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public SurveyedSurfaceExecutor()
    {
      ProcessErrorCodes();
    }

    /// <summary>
    /// Populates ContractExecutionStates with Production Data Server error messages.
    /// </summary>
    protected sealed override void ProcessErrorCodes()
    {

      RaptorResult.AddErrorMessages(ContractExecutionStates);
    }

    /// <summary>
    /// Sends a request to Production Data Server (PDS) client.
    /// </summary>
    /// <param name="item">Request description.</param>
    /// <param name="surveyedSurfaces">Returned list of Surveyed Surfaces.</param>
    /// <returns>True if the processed request from PDS was successful, false - otherwise.</returns>
    /// 
    protected virtual bool SendRequestToPdsClient(object item, out TSurveyedSurfaceDetails[] surveyedSurfaces)
    {
      surveyedSurfaces = null;

      return true;
    }

    /// <summary>
    /// Returns an instance of the ContractExecutionResult class as an execution result.
    /// </summary>
    /// <returns>An instance of the ContractExecutionResult class.</returns>
    /// 
    protected virtual ContractExecutionResult ExecutionResult(SurveyedSurfaceDetails[] surveyedSurfaces)
    {
      return null;
    }

    /// <summary>
    /// Surveyed Surface data executor (Post/Put/Get/Delete).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item">A Domain object.</param>
    /// <returns></returns>
    /// 
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      if (item != null)
      {
        try
        {
          if (SendRequestToPdsClient(item, out var surveyedSurfaces))
            return ExecutionResult(surveyedSurfaces != null ? ConvertToSurveyedSurfaceDetails(surveyedSurfaces).ToArray() : null);

          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
              "Failed to process Surveyed Surface data request."));
        }
        finally
        {
          ContractExecutionStates.ClearDynamic();
        }
      }

      throw new ServiceException(HttpStatusCode.BadRequest,
        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
          "No Surveyed Surface data request sent."));
    }

    private IEnumerable<SurveyedSurfaceDetails> ConvertToSurveyedSurfaceDetails(IEnumerable<TSurveyedSurfaceDetails> surveyedSurfaces)
    {
      return surveyedSurfaces.Select(surveyedSurface => SurveyedSurfaceDetails.CreateSurveyedSurfaceDetails(
        id: surveyedSurface.ID,
        surveyedSurface: new DesignDescriptor(
          surveyedSurface.DesignDescriptor.DesignID,
          FileDescriptor.CreateFileDescriptor(
            surveyedSurface.DesignDescriptor.FileSpaceID,
            surveyedSurface.DesignDescriptor.Folder,
            surveyedSurface.DesignDescriptor.FileName),
          surveyedSurface.DesignDescriptor.Offset),
        asAtDate: surveyedSurface.AsAtDate,
        extents: new BoundingBox3DGrid(
          surveyedSurface.Extents.MinX,
          surveyedSurface.Extents.MinY,
          surveyedSurface.Extents.MinZ,
          surveyedSurface.Extents.MaxX,
          surveyedSurface.Extents.MaxY,
          surveyedSurface.Extents.MaxZ)));
    }
  }
}
