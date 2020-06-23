﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Common.Exceptions;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Enums;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.RadioSerialMap;

namespace WebApiTests.Executors
{
  [TestClass]
  public class ProjectAndAssetUidsExecutorManualTests : ExecutorBaseTests
  {
    private ILoggerFactory _loggerFactory;

    [TestInitialize]
    public override void InitTest()
    {
      base.InitTest();

      _loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();
    }


    [TestMethod]
    public async Task TRexExecutor_Manual_Sad_ProjectNotFound()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectAccountUid = Guid.NewGuid().ToString();
      var projectOfInterest = new ProjectData
      {
        ProjectUID = projectUid,
        ProjectType = CwsProjectType.AcceptsTagFiles,
        CustomerUID = projectAccountUid
      };

      var getProjectAndAssetUidsRequest = new GetProjectAndAssetUidsRequest(projectUid, (int)TagFileDeviceTypeEnum.SNM940, "snm940Serial", string.Empty, 91, 181, DateTime.UtcNow.AddDays(-3));
      var projectForProjectUid = (ProjectData)null;

      var radioSerialDeviceUid = Guid.NewGuid().ToString();
      var radioSerialAccountUid = Guid.NewGuid().ToString();
      var radioSerialDevice = new DeviceData { CustomerUID = radioSerialAccountUid, DeviceUID = radioSerialDeviceUid };
      var projectListForRadioSerial = new ProjectDataResult() { ProjectDescriptors = new List<ProjectData>() { projectOfInterest } };

      var expectedGetProjectAndAssetUidsResult = new GetProjectAndAssetUidsResult(string.Empty, string.Empty);

      await ExecuteManual
        (getProjectAndAssetUidsRequest, projectForProjectUid,
          radioSerialDevice, projectListForRadioSerial,
          null, null,
          ServiceProvider.GetService<ICustomRadioSerialProjectMap>(),
          expectedGetProjectAndAssetUidsResult, expectedCode: 3038, expectedMessage: ContractExecutionStatesEnum.FirstNameWithOffset(38)
        );
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Happy_ProjectArchived()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectAccountUid = Guid.NewGuid().ToString();
      var projectOfInterest = new ProjectData
      {
        ProjectUID = projectUid,
        ProjectType = CwsProjectType.AcceptsTagFiles,
        CustomerUID = projectAccountUid,
        IsArchived = true
      };

      var getProjectAndAssetUidsRequest = new GetProjectAndAssetUidsRequest(projectUid, (int)TagFileDeviceTypeEnum.SNM940, "snm940Serial", string.Empty, 91, 181, DateTime.UtcNow.AddDays(-3));
      var projectForProjectUid = projectOfInterest;

      var radioSerialDeviceUid = Guid.NewGuid().ToString();
      var radioSerialAccountUid = Guid.NewGuid().ToString();
      var radioSerialDevice = new DeviceData { CustomerUID = radioSerialAccountUid, DeviceUID = radioSerialDeviceUid };
      var projectListForRadioSerial = new ProjectDataResult() { ProjectDescriptors = new List<ProjectData>() { projectOfInterest } };

      var expectedGetProjectAndAssetUidsResult = new GetProjectAndAssetUidsResult(string.Empty, string.Empty);

      await ExecuteManual
        (getProjectAndAssetUidsRequest, projectForProjectUid,
          radioSerialDevice, projectListForRadioSerial,
          null, null,
          ServiceProvider.GetService<ICustomRadioSerialProjectMap>(),
          expectedGetProjectAndAssetUidsResult, expectedCode: 3043, expectedMessage: ContractExecutionStatesEnum.FirstNameWithOffset(43)
        );
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Sad_ProjectDoesntIntersect()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectAccountUid = Guid.NewGuid().ToString();
      var projectOfInterest = new ProjectData
      {
        ProjectUID = projectUid,
        ProjectType = CwsProjectType.AcceptsTagFiles,
        CustomerUID = projectAccountUid,
        IsArchived = false,
        ProjectGeofenceWKT = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))",

      };

      var getProjectAndAssetUidsRequest = new GetProjectAndAssetUidsRequest(projectUid, (int)TagFileDeviceTypeEnum.SNM940, "snm940Serial", string.Empty, 50, 180, DateTime.UtcNow.AddDays(-3));
      var projectForProjectUid = projectOfInterest;

      var radioSerialDeviceUid = Guid.NewGuid().ToString();
      var radioSerialAccountUid = Guid.NewGuid().ToString();
      var radioSerialDevice = new DeviceData { CustomerUID = radioSerialAccountUid, DeviceUID = radioSerialDeviceUid };
      var projectListForRadioSerial = new ProjectDataResult() { ProjectDescriptors = new List<ProjectData>() { projectOfInterest } };
      var expectedGetProjectAndAssetUidsResult = new GetProjectAndAssetUidsResult(string.Empty, radioSerialDeviceUid);

      await ExecuteManual
        (getProjectAndAssetUidsRequest, projectForProjectUid,
          radioSerialDevice, projectListForRadioSerial,
          null, null,
          ServiceProvider.GetService<ICustomRadioSerialProjectMap>(),
          expectedGetProjectAndAssetUidsResult, expectedCode: 3041, expectedMessage: ContractExecutionStatesEnum.FirstNameWithOffset(41)
        );
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Happy_ProjectAndDevice()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectAccountUid = Guid.NewGuid().ToString();
      var projectOfInterest = new ProjectData
      {
        ProjectUID = projectUid,
        ProjectType = CwsProjectType.AcceptsTagFiles,
        CustomerUID = projectAccountUid,
        IsArchived = false,
        ProjectGeofenceWKT = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))",

      };

      var getProjectAndAssetUidsRequest = new GetProjectAndAssetUidsRequest(projectUid, (int)TagFileDeviceTypeEnum.SNM940, "snm940Serial", string.Empty, 15, 180, DateTime.UtcNow.AddDays(-3));
      var projectForProjectUid = projectOfInterest;

      var radioSerialDeviceUid = Guid.NewGuid().ToString();
      var radioSerialAccountUid = Guid.NewGuid().ToString();
      var radioSerialDevice = new DeviceData { CustomerUID = radioSerialAccountUid, DeviceUID = radioSerialDeviceUid };
      var projectListForRadioSerial = new ProjectDataResult() { ProjectDescriptors = new List<ProjectData>() { projectOfInterest } };

      var expectedGetProjectAndAssetUidsResult = new GetProjectAndAssetUidsResult(projectUid, radioSerialDeviceUid);

      await ExecuteManual
      (getProjectAndAssetUidsRequest, projectForProjectUid,
        radioSerialDevice, projectListForRadioSerial,
        null, null,
        ServiceProvider.GetService<ICustomRadioSerialProjectMap>(),
        expectedGetProjectAndAssetUidsResult, expectedCode: 0, expectedMessage: "success"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Sad_InvalidParameters()
    {
      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsExecutor>(
        _loggerFactory.CreateLogger<ProjectAndAssetUidsExecutorManualTests>(), ConfigStore, authorization.Object,
         projectProxy.Object, deviceProxy.Object, tRexCompactionDataProxy.Object, requestCustomHeaders);

      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(() =>
        executor.ProcessAsync((GetProjectAndAssetUidsRequest)null));

      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);
      Assert.AreEqual(-3, ex.GetResult.Code);
      Assert.AreEqual("Serialization error", ex.GetResult.Message);
    }

    private async Task ExecuteManual(GetProjectAndAssetUidsRequest request, ProjectData projectForProjectUid,
      DeviceData radioSerialDevice, ProjectDataResult projectListForRadioSerial,
      DeviceData ec520Device, ProjectDataResult projectListForEC520,
      ICustomRadioSerialProjectMap customRadioSerialProjectMap,
      GetProjectAndAssetUidsResult expectedGetProjectAndAssetUidsResult, int expectedCode, string expectedMessage
  )
    {
      projectProxy.Setup(p => p.GetProject(request.ProjectUid, It.IsAny<HeaderDictionary>())).ReturnsAsync(projectForProjectUid);

      deviceProxy.Setup(d => d.GetDevice(request.RadioSerial, It.IsAny<HeaderDictionary>())).ReturnsAsync(radioSerialDevice);
      if (radioSerialDevice != null)
        deviceProxy.Setup(d => d.GetProjectsForDevice(radioSerialDevice.DeviceUID, It.IsAny<HeaderDictionary>())).ReturnsAsync(projectListForRadioSerial);

      deviceProxy.Setup(d => d.GetDevice(request.Ec520Serial, It.IsAny<HeaderDictionary>())).ReturnsAsync(ec520Device);
      if (ec520Device != null)
        deviceProxy.Setup(d => d.GetProjectsForDevice(ec520Device.DeviceUID, It.IsAny<HeaderDictionary>())).ReturnsAsync(projectListForEC520);

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsExecutor>(
        _loggerFactory.CreateLogger<ProjectAndAssetUidsExecutorManualTests>(), ConfigStore, authorization.Object,
         projectProxy.Object, deviceProxy.Object, tRexCompactionDataProxy.Object, requestCustomHeaders);
      executor.CustomRadioSerialMapper = customRadioSerialProjectMap;
      var result = await executor.ProcessAsync(request) as GetProjectAndAssetUidsResult;

      ValidateResult(result, expectedGetProjectAndAssetUidsResult, expectedCode, expectedMessage);
    }

    private void ValidateResult(GetProjectAndAssetUidsResult actualResult, GetProjectAndAssetUidsResult expectedGetProjectAndAssetUidsResult,
      int resultCode, string resultMessage)
    {
      Assert.IsNotNull(actualResult, "executor returned nothing");
      Assert.AreEqual(expectedGetProjectAndAssetUidsResult.ProjectUid, actualResult.ProjectUid, "executor returned incorrect ProjectUid");
      Assert.AreEqual(expectedGetProjectAndAssetUidsResult.DeviceUid, actualResult.DeviceUid, "executor returned incorrect DeviceUid");
      Assert.AreEqual(resultCode, actualResult.Code, "executor returned incorrect result code");
      Assert.AreEqual(resultMessage, actualResult.Message, "executor returned incorrect result message");
    }
  }
}
