﻿using System;
using System.IO;
using System.Threading.Tasks;
using ASNode.RequestProfile.RPC;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SVOICOptionsDecls;
using VLPDDecls;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Converters;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Productivity3D.WebApi.Models.ProductionData.Executors;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using __Global = ASNode.RequestProfile.RPC.__Global;

namespace VSS.Productivity3D.WebApiTests.ProductionData.Controllers
{
  [TestClass]
  public class ProfileProductionDataControllerTests
  {
    private const long PD_MODEL_ID = 544; // Dimensions 2012 project...

    /// <summary>
    /// Creates an instance of the ProfileProductionDataRequest class.
    /// </summary>
    /// <returns>The created instance.</returns>
    /// 
    private ProfileProductionDataRequest CreateRequest()
    {
      var profileLLPoints = ProfileLLPoints.CreateProfileLLPoints(35.109149 * Coordinates.DEGREES_TO_RADIANS,
        -106.6040765 * Coordinates.DEGREES_TO_RADIANS,
        35.109149 * Coordinates.DEGREES_TO_RADIANS,
        -104.28745 * Coordinates.DEGREES_TO_RADIANS);

      return new ProfileProductionDataRequest(
        PD_MODEL_ID,
        new Guid(),
        ProductionDataType.Height,
        null,
        -1,
        null,
        null,
        profileLLPoints,
        1,
        120,
        null,
        true
        );
    }

    /// <summary>
    /// Uses the mock PDS client to post a request with a successful result...
    /// </summary>
    /// 
    [TestMethod]
    public async Task PD_PostProfileProductionDataSuccessful()
    {
      ProfileProductionDataRequest request = CreateRequest();

      MemoryStream raptorResult = new MemoryStream();

      Assert.IsTrue(RaptorConverters.DesignDescriptor(request.AlignmentDesign).IsNull(), "A linear profile expected.");

      TWGS84Point startPt, endPt;

      bool positionsAreGrid;

      ProfilesHelper.ConvertProfileEndPositions(request.GridPoints, request.WGS84Points, out startPt, out endPt, out positionsAreGrid);

      TASNodeServiceRPCVerb_RequestProfile_Args args
           = __Global.Construct_RequestProfile_Args
           (request.ProjectId.Value,
            -1, // don't care
            positionsAreGrid,
            startPt,
            endPt,
            RaptorConverters.ConvertFilter(request.Filter),
            RaptorConverters.ConvertLift(request.LiftBuildSettings, TFilterLayerMethod.flmAutomatic),
            RaptorConverters.DesignDescriptor(request.AlignmentDesign),
            request.ReturnAllPassesAndLayers);

      // Create the mock PDSClient with successful result...
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockLogger = new Mock<ILoggerFactory>();
      var mockConfigStore = new Mock<IConfigurationStore>();

      mockRaptorClient.Setup(prj => prj.GetProfile(It.IsAny<TASNodeServiceRPCVerb_RequestProfile_Args>()/*args*/)).Returns(raptorResult);

      // Create an executor...
      var executor = RequestExecutorContainerFactory.Build<ProfileProductionDataExecutor>(mockLogger.Object, mockRaptorClient.Object, configStore: mockConfigStore.Object);

      var result = await executor.ProcessAsync(request);

      // Assert
      Assert.IsNotNull(result);
      Assert.IsTrue(result.Message == ContractExecutionResult.DefaultMessage, result.Message);
    }

    /// <summary>
    /// Uses the mock PDS client to post a request with unsuccessful result...
    /// </summary>
    /// 
    [TestMethod]
    public void PD_PostProfileProductionDataFailed()
    {
      ProfileProductionDataRequest request = CreateRequest();
      MemoryStream raptorResult = null;

      Assert.IsTrue(RaptorConverters.DesignDescriptor(request.AlignmentDesign).IsNull(), "A linear profile expected.");

      ProfilesHelper.ConvertProfileEndPositions(request.GridPoints, request.WGS84Points, out TWGS84Point startPt, out var endPt, out bool positionsAreGrid);

      TASNodeServiceRPCVerb_RequestProfile_Args args
           = __Global.Construct_RequestProfile_Args
           (request.ProjectId.Value,
            -1, // don't care
            positionsAreGrid,
            startPt,
            endPt,
            RaptorConverters.ConvertFilter(request.Filter),
            RaptorConverters.ConvertLift(request.LiftBuildSettings, TFilterLayerMethod.flmAutomatic),
            RaptorConverters.DesignDescriptor(request.AlignmentDesign),
            request.ReturnAllPassesAndLayers);

      // Create the mock PDSClient with successful result...
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockLogger = new Mock<ILoggerFactory>();
      var mockConfigStore = new Mock<IConfigurationStore>();

      mockRaptorClient.Setup(prj => prj.GetProfile(It.IsAny<TASNodeServiceRPCVerb_RequestProfile_Args>()/*args*/)).Returns(raptorResult);

      // Create an executor...
      var executor = RequestExecutorContainerFactory.Build<ProfileProductionDataExecutor>(mockLogger.Object, mockRaptorClient.Object, configStore: mockConfigStore.Object);

      Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(request));
    }
  }
}
