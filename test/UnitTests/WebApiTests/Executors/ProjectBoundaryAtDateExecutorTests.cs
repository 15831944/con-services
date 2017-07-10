﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.Extensions.Logging;
using Repositories;
using VSS.Productivity3D.WebApiModels.Executors;
using VSS.Productivity3D.WebApiModels.Models;
using VSS.Productivity3D.WebApiModels.ResultHandling;

namespace WebApiTests.Executors
{
  [TestClass]
  public class ProjectBoundaryAtDateExecutorTests : ExecutorBaseTests
  {

    [TestMethod]
    public void CanCallProjectBoundaryAtDateExecutorNoValidInput()
    {
      GetProjectBoundaryAtDateRequest ProjectBoundaryAtDateRequest = GetProjectBoundaryAtDateRequest.CreateGetProjectBoundaryAtDateRequest(-1, DateTime.UtcNow);
      var factory = serviceProvider.GetRequiredService<IRepositoryFactory>();
      ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      var result = RequestExecutorContainer.Build<ProjectBoundaryAtDateExecutor>(factory, loggerFactory.CreateLogger<ProjectBoundaryAtDateExecutorTests>()).Process(ProjectBoundaryAtDateRequest) as GetProjectBoundaryAtDateResult;
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(ContractExecutionStatesEnum.ExecutedSuccessfully, result.Code, "executor "); // todo does executed successfully mean it executed but may return nothing?

      Assert.IsNotNull(result.projectBoundary, "executor returned incorrect projectBoundary");
      Assert.IsNull(result.projectBoundary.FencePoints, "executor returned incorrect projectBoundary count");
    }

  }
}
