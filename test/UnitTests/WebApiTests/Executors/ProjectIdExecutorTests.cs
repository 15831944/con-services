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
  public class ProjectIdExecutorTests : ExecutorBaseTests
  {

    [TestMethod]
    public void CanCallProjectIDExecutorNoValidInput()
    {
      GetProjectIdRequest ProjectIdRequest = GetProjectIdRequest.CreateGetProjectIdRequest(-1, 91, 181, 0, DateTime.MinValue, "");
      var factory = serviceProvider.GetRequiredService<IRepositoryFactory>();
      ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      var result = RequestExecutorContainer.Build<ProjectIdExecutor>(factory, loggerFactory.CreateLogger<ProjectIdExecutorTests>()).Process(ProjectIdRequest) as GetProjectIdResult;
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(-1, result.projectId, "executor returned incorrect legacy ProjectId");
    }

  }
}
