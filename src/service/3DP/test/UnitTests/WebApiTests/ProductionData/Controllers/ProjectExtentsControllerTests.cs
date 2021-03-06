﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
#if RAPTOR
using BoundingExtents;
using VLPDDecls;
#endif
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.WebApi.Models.ProductionData.Executors;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.TRex.Gateway.Common.Abstractions;

namespace VSS.Productivity3D.WebApiTests.ProductionData.Controllers
{
  [TestClass]
  public class ProjectExtentsControllerTests
  {
#if RAPTOR
    [TestMethod]
    public async Task PD_PostProjectExtentsSuccessful()
    {

      // create the mock RaptorClient with successful result
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockLogger = new Mock<ILoggerFactory>();
      var mockConfigStore = new Mock<IConfigurationStore>();
      var trexCompactionDataProxy = new Mock<ITRexCompactionDataProxy>();
      var getExtentsResults = true;
      var excludedSsIds = new long[1]; // excluded surveyed surfaces
      excludedSsIds[0] = 1;

      // return results
      var extents = new T3DBoundingWorldExtent();
      // mock return result
      // this wont work as we need to mock higher level
      mockRaptorClient.Setup(prj => prj.GetDataModelExtents(544, It.IsAny<TSurveyedSurfaceID[]>(), out extents)).Returns(getExtentsResults);

      // create submitter
      var submitter = RequestExecutorContainerFactory.Build<ProjectExtentsSubmitter>(mockLogger.Object, mockRaptorClient.Object, configStore: mockConfigStore.Object, trexCompactionDataProxy: trexCompactionDataProxy.Object);
      // make request parameters
      var request = new ExtentRequest(544, null, excludedSsIds);

      // Act
      // Call controller
      var result = await submitter.ProcessAsync(request);

      // Assert
      Assert.IsNotNull(result);
      Assert.IsTrue(result.Message == ContractExecutionResult.DefaultMessage, result.Message);
    }

    [TestMethod]
    public void PD_PostProjectExtents_Raptor_Fail()
    {

      // create the mock RaptorClient with successful result
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockLogger = new Mock<ILoggerFactory>();

      var mockConfigStore = new Mock<IConfigurationStore>();
      mockConfigStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_TILES")).Returns(false);


      var trexCompactionDataProxy = new Mock<ITRexCompactionDataProxy>();
      var getExtentsResults = false;
      var excludedSsIds = new long[1]; // excluded surveyed surfaces
      excludedSsIds[0] = 1;

      // return results
      var extents = new T3DBoundingWorldExtent();
      // mock return result
      // this wont work as we need to mock higher level
      mockRaptorClient.Setup(prj => prj.GetDataModelExtents(544, It.IsAny<TSurveyedSurfaceID[]>(), out extents)).Returns(getExtentsResults);

      // create submitter
      var submitter = RequestExecutorContainerFactory.Build<ProjectExtentsSubmitter>(mockLogger.Object, mockRaptorClient.Object, configStore: mockConfigStore.Object, trexCompactionDataProxy: trexCompactionDataProxy.Object);
      // make request parameters
      var request = new ExtentRequest(544, null, excludedSsIds);

      // Act
      // Call controller
      Assert.ThrowsExceptionAsync<ServiceException>(async () => await submitter.ProcessAsync(request));
    }
#endif
    [TestMethod]
    public async Task PD_PostProjectExtents_TRex_Fail()
    {
      const short projectId = 544;

      var projectUid = Guid.NewGuid();

      var request = new ExtentRequest(projectId, projectUid, null);

      var mockLogger = new Mock<ILoggerFactory>();

      var mockConfigStore = new Mock<IConfigurationStore>();
#if RAPTOR
      mockConfigStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_TILES")).Returns(true);
#endif

      var exception = new ServiceException(HttpStatusCode.InternalServerError,
        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
          $"Get project extents has not been implemented in TRex yet. ProjectUid: {projectUid}"));

      var trexCompactionDataProxy = new Mock<ITRexCompactionDataProxy>();
      trexCompactionDataProxy.Setup(x => x.SendDataGetRequest<BoundingBox3DGrid>(projectUid.ToString(),
          It.IsAny<string>(),
          It.IsAny<IHeaderDictionary>(),
          It.IsAny<List<KeyValuePair<string, string>>>()))
        .Throws(exception);

      var executor = RequestExecutorContainerFactory
        .Build<ProjectExtentsSubmitter>(mockLogger.Object, configStore: mockConfigStore.Object, trexCompactionDataProxy: trexCompactionDataProxy.Object);

      var result = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(request));

      Assert.AreEqual(HttpStatusCode.InternalServerError, result.Code);
      Assert.AreEqual(ContractExecutionStatesEnum.InternalProcessingError, result.GetResult.Code);
      Assert.AreEqual(exception.GetResult.Message, result.GetResult.Message);
    }
  }
}
