﻿using VLPDDecls;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApiModels.ProductionData.Models;
using VSS.Productivity3D.WebApiModels.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.ProductionData.Executors
{
  /// <summary>
  /// Executes GET method on Surveyed Surfaces resource.
  /// </summary>
  /// 
  public class SurveyedSurfaceExecutorGet : SurveyedSurfaceExecutor
  {
    /// <summary>
    /// Sends a GET request to Production Data Server (PDS) client.
    /// </summary>
    /// <param name="item">GET request description.</param>
    /// <param name="surveyedSurfaces">Returned list of Surveyed Surfaces.</param>
    /// <returns>True if the processed request from PDS was successful, false - otherwise.</returns>
    /// 
    protected override bool SendRequestToPdsClient(object item, out TSurveyedSurfaceDetails[] surveyedSurfaces)
    {
      ProjectID request = item as ProjectID;

      return raptorClient.GetKnownGroundSurfaceFileDetails(request.projectId ?? -1, out surveyedSurfaces);
    }

    /// <summary>
    /// Returns an instance of the ContractExecutionResult class as GET method execution result.
    /// </summary>
    /// <returns>An instance of the ContractExecutionResult class.</returns>
    /// 
    protected override ContractExecutionResult ExecutionResult(SurveyedSurfaceDetails[] surveyedSurfaces)
    {
      //string[] surveyedSurfaceNames = (from ssd in surveyedSurfaces select ssd.DesignDescriptor.FileName).ToArray();

      return SurveyedSurfaceResult.CreateSurveyedSurfaceResult(surveyedSurfaces);
    }
  }
} 