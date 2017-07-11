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
  public class ProjectBoundariesAtDateExecutorTests : ExecutorBaseTests
  {

    [TestMethod]
    public void CanCallProjectBoundariesAtDateExecutorNoValidInput()
    {
      GetProjectBoundariesAtDateRequest ProjectBoundariesAtDateRequest = GetProjectBoundariesAtDateRequest.CreateGetProjectBoundariesAtDateRequest(-1, DateTime.UtcNow);
      GetProjectBoundariesAtDateResult ProjectBoundariesAtDateResult = new GetProjectBoundariesAtDateResult();
      var factory = serviceProvider.GetRequiredService<IRepositoryFactory>();
      ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      var result = RequestExecutorContainer.Build<ProjectBoundariesAtDateExecutor>(factory, loggerFactory.CreateLogger<ProjectBoundariesAtDateExecutorTests>()).Process(ProjectBoundariesAtDateRequest) as GetProjectBoundariesAtDateResult;
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.IsNotNull(result.projectBoundaries, "executor returned incorrect ProjectBoundaries");
      Assert.AreEqual(0, result.projectBoundaries.Length, "executor returned incorrect ProjectBoundaries");
    }
   
  }
}
