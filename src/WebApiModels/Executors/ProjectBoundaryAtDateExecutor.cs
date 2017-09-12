﻿using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;
using ContractExecutionStatesEnum = VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling.ContractExecutionStatesEnum;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors
{
  /// <summary>
  /// The executor which gets the project boundary of the project for the requested project id.
  /// </summary>
  public class ProjectBoundaryAtDateExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Processes the get project boundary request and finds active projects of the asset owner at the given date time.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a GetProjectBoundaryAtDateResult if successful</returns>      
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      GetProjectBoundaryAtDateRequest request = item as GetProjectBoundaryAtDateRequest;
      log.LogDebug("ProjectBoundaryAtDateExecutor: Going to process request {0}", JsonConvert.SerializeObject(request));

      bool result = false;
      TWGS84FenceContainer projectBoundary = new TWGS84FenceContainer();
      Project project = null;

      project = await dataRepository.LoadProject(request.projectId);
      log.LogDebug("ProjectBoundaryAtDateExecutor: Loaded project? {0}", JsonConvert.SerializeObject(project));

      if (project != null)
      {
        if (project.StartDate <= request.tagFileUTC.Date && request.tagFileUTC.Date <= project.EndDate &&
            !string.IsNullOrEmpty(project.GeometryWKT)
        )
        {
          projectBoundary.FencePoints = dataRepository.ParseBoundaryData(project.GeometryWKT);
          log.LogDebug("ProjectBoundaryAtDateExecutor: Loaded projectBoundary.FencePoints? {0}",
            JsonConvert.SerializeObject(projectBoundary.FencePoints));

          if (projectBoundary.FencePoints.Length > 0)
            result = true;
        }
      }

      try
      {
        return GetProjectBoundaryAtDateResult.CreateGetProjectBoundaryAtDateResult(result, projectBoundary);
      }
      catch
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          GetProjectBoundaryAtDateResult.CreateGetProjectBoundaryAtDateResult(false, new TWGS84FenceContainer(),
            ContractExecutionStatesEnum.InternalProcessingError, 27));
      }
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }
  }
}
  