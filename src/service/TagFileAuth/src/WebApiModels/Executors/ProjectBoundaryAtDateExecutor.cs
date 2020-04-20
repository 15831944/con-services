﻿using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors
{
  /// <summary>
  /// The executor which gets the project boundary of the shortRaptorProjectId project.
  /// </summary>
  public class ProjectBoundaryAtDateExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Processes the get project boundary request and finds active projects of the device owner at the given date time.
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as GetProjectBoundaryAtDateRequest;
      var result = false;
      var projectBoundary = new TWGS84FenceContainer();

      var project = await dataRepository.GetProject(request.shortRaptorProjectId);
      log.LogDebug($"{nameof(ProjectBoundaryAtDateExecutor)}: Loaded project? {JsonConvert.SerializeObject(project)}");

      if (project != null)
      {
        if (!string.IsNullOrEmpty(project.GeometryWKT))
        {
          projectBoundary.FencePoints = dataRepository.ParseBoundaryData(project.GeometryWKT);
          log.LogDebug(
            $"{nameof(ProjectBoundaryAtDateExecutor)}: Loaded projectBoundary.FencePoints? {JsonConvert.SerializeObject(projectBoundary.FencePoints)}");

          if (projectBoundary.FencePoints.Length > 0)
            result = true;
        }
      }

      return GetProjectBoundaryAtDateResult.CreateGetProjectBoundaryAtDateResult(result, projectBoundary);
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }
  }
}
