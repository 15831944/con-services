﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Exceptions;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Enums;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.RadioSerialMap;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

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
    public async Task TRexExecutor_Manual_Happy_ProjectAccountLicense_CBDeviceAndNoLicense()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectAccountUid = Guid.NewGuid().ToString();
      var projectOfInterest = new ProjectData
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = projectAccountUid
      };

      var getProjectAndAssetUidsRequest = new GetProjectAndAssetUidsRequest(projectUid, (int)TagFileDeviceTypeEnum.SNM940, "snm940Serial", string.Empty, 91, 181, DateTime.UtcNow.AddDays(-3));
      var projectForProjectUid = projectOfInterest;
      var projectListForProjectAccountUid = new ProjectDataResult() { ProjectDescriptors = new List<ProjectData>() { projectOfInterest } };
      var projectDeviceLicenseResponseModel = new DeviceLicenseResponseModel() { Total = 1 };

      var radioSerialDeviceUid = Guid.NewGuid().ToString();
      var radioSerialAccountUid = Guid.NewGuid().ToString();
      var radioSerialDevice = new DeviceData { CustomerUID = radioSerialAccountUid, DeviceUID = radioSerialDeviceUid };
      var projectListForRadioSerial = new ProjectDataResult() { ProjectDescriptors = new List<ProjectData>() { projectOfInterest } };
      //var radioSerialDeviceLicenseResponseModel = (DeviceLicenseResponseModel) null;

      var ec520Uid = Guid.NewGuid().ToString();
      //var ec520AccountUid = Guid.NewGuid().ToString();
      var ec520Device = (DeviceData)null;
      var projectListForEC520 = (ProjectDataResult)null;
      //var ec520DeviceLicenseResponseModel = (DeviceLicenseResponseModel)null;

      var expectedGetProjectAndAssetUidsResult = new GetProjectAndAssetUidsResult(projectUid, radioSerialDeviceUid);


      await ExecuteManual
        (getProjectAndAssetUidsRequest,
          projectAccountUid, projectForProjectUid, projectListForProjectAccountUid, projectDeviceLicenseResponseModel,
          radioSerialDevice, projectListForRadioSerial,
          ec520Device, projectListForEC520,
          ServiceProvider.GetService<ICustomRadioSerialProjectMap>(),
          expectedGetProjectAndAssetUidsResult, expectedCode: 0, expectedMessage: "success"
        );
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Happy_ProjectAccountLicense_ECMDeviceAndNoLicense()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectAccountUid = Guid.NewGuid().ToString();
      var projectOfInterest = new ProjectData
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = projectAccountUid
      };

      var getProjectAndAssetUidsRequest = new GetProjectAndAssetUidsRequest(projectUid, (int)TagFileDeviceTypeEnum.EC520, string.Empty, "ec520Serial", 91, 181, DateTime.UtcNow.AddDays(-3));
      var projectForProjectUid = projectOfInterest;
      var projectListForProjectAccountUid = new ProjectDataResult() { ProjectDescriptors = new List<ProjectData>() { projectOfInterest } };
      var projectDeviceLicenseResponseModel = new DeviceLicenseResponseModel() { Total = 1 };

      var radioSerialDeviceUid = Guid.NewGuid().ToString();
      var radioSerialAccountUid = string.Empty;
      var radioSerialDevice = (DeviceData)null;
      var projectListForRadioSerial = (ProjectDataResult)null;
      //var radioSerialDeviceLicenseResponseModel = (DeviceLicenseResponseModel) null;

      var ec520Uid = Guid.NewGuid().ToString();
      var ec520AccountUid = Guid.NewGuid().ToString();
      var ec520Device = new DeviceData { CustomerUID = ec520AccountUid, DeviceUID = ec520Uid };
      var projectListForEC520 = new ProjectDataResult() { ProjectDescriptors = new List<ProjectData>() { projectOfInterest } };
      //var ec520DeviceLicenseResponseModel = (DeviceLicenseResponseModel)null;

      var expectedGetProjectAndAssetUidsResult = new GetProjectAndAssetUidsResult(projectUid, ec520Uid);

      await ExecuteManual
        (getProjectAndAssetUidsRequest,
          projectAccountUid, projectForProjectUid, projectListForProjectAccountUid, projectDeviceLicenseResponseModel,
          radioSerialDevice, projectListForRadioSerial, // radioSerialDeviceLicenseResponseModel,
          ec520Device, projectListForEC520, // ec520DeviceLicenseResponseModel,
          ServiceProvider.GetService<ICustomRadioSerialProjectMap>(),
          expectedGetProjectAndAssetUidsResult, expectedCode: 0, expectedMessage: "success"
          );
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Happy_ProjectAccountLicense_NoDevice()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectAccountUid = Guid.NewGuid().ToString();
      var projectOfInterest = new ProjectData
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = projectAccountUid
      };

      var getProjectAndAssetUidsRequest = new GetProjectAndAssetUidsRequest(projectUid, (int)TagFileDeviceTypeEnum.EC520, string.Empty, "ec520Serial", 91, 181, DateTime.UtcNow.AddDays(-3));
      var projectForProjectUid = projectOfInterest;
      var projectListForProjectAccountUid = new ProjectDataResult() { ProjectDescriptors = new List<ProjectData>() { projectOfInterest } };
      var projectDeviceLicenseResponseModel = new DeviceLicenseResponseModel() { Total = 1 };

      var radioSerialDeviceUid = Guid.NewGuid().ToString();
      var radioSerialAccountUid = string.Empty;
      var radioSerialDevice = (DeviceData)null;
      var projectListForRadioSerial = (ProjectDataResult)null;
      //var radioSerialDeviceLicenseResponseModel = (DeviceLicenseResponseModel) null;

      var ec520Uid = Guid.NewGuid().ToString();
      var ec520AccountUid = Guid.NewGuid().ToString();
      var ec520Device = (DeviceData)null;
      var projectListForEC520 = (ProjectDataResult)null;
      //var ec520DeviceLicenseResponseModel = (DeviceLicenseResponseModel)null;

      var expectedGetProjectAndAssetUidsResult = new GetProjectAndAssetUidsResult(projectUid, string.Empty);


      await ExecuteManual
        (getProjectAndAssetUidsRequest,
          projectAccountUid, projectForProjectUid, projectListForProjectAccountUid, projectDeviceLicenseResponseModel,
          radioSerialDevice, projectListForRadioSerial, // radioSerialDeviceLicenseResponseModel,
          ec520Device, projectListForEC520, // ec520DeviceLicenseResponseModel,
          ServiceProvider.GetService<ICustomRadioSerialProjectMap>(),
          expectedGetProjectAndAssetUidsResult, expectedCode: 0, expectedMessage: "success"
          );
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Sad_ProjectNotFound()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectAccountUid = Guid.NewGuid().ToString();
      var projectOfInterest = new ProjectData
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = projectAccountUid
      };

      var getProjectAndAssetUidsRequest = new GetProjectAndAssetUidsRequest(projectUid, (int)TagFileDeviceTypeEnum.SNM940, "snm940Serial", string.Empty, 91, 181, DateTime.UtcNow.AddDays(-3));
      var projectForProjectUid = (ProjectData)null;
      var projectListForProjectAccountUid = new ProjectDataResult();
      var projectDeviceLicenseResponseModel = new DeviceLicenseResponseModel() { Total = 1 };

      var radioSerialDeviceUid = Guid.NewGuid().ToString();
      var radioSerialAccountUid = Guid.NewGuid().ToString();
      var radioSerialDevice = new DeviceData { CustomerUID = radioSerialAccountUid, DeviceUID = radioSerialDeviceUid };
      var projectListForRadioSerial = new ProjectDataResult() { ProjectDescriptors = new List<ProjectData>() { projectOfInterest } };
      //var radioSerialDeviceLicenseResponseModel = (DeviceLicenseResponseModel) null;

      var ec520Uid = Guid.NewGuid().ToString();
      //var ec520AccountUid = Guid.NewGuid().ToString();
      var ec520Device = (DeviceData)null;
      var projectListForEC520 = (ProjectDataResult)null;
      //var ec520DeviceLicenseResponseModel = (DeviceLicenseResponseModel)null;

      var expectedGetProjectAndAssetUidsResult = new GetProjectAndAssetUidsResult(string.Empty, string.Empty);


      await ExecuteManual
        (getProjectAndAssetUidsRequest,
          projectAccountUid, projectForProjectUid, projectListForProjectAccountUid, projectDeviceLicenseResponseModel,
          radioSerialDevice, projectListForRadioSerial, // radioSerialDeviceLicenseResponseModel,
          ec520Device, projectListForEC520, // ec520DeviceLicenseResponseModel,
          ServiceProvider.GetService<ICustomRadioSerialProjectMap>(),
          expectedGetProjectAndAssetUidsResult, expectedCode: 3038, expectedMessage: ContractExecutionStatesEnum.FirstNameWithOffset(38)
        );
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Sad_ProjectArchived()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectAccountUid = Guid.NewGuid().ToString();
      var projectOfInterest = new ProjectData
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = projectAccountUid,
        IsArchived = true
      };

      var getProjectAndAssetUidsRequest = new GetProjectAndAssetUidsRequest(projectUid, (int)TagFileDeviceTypeEnum.SNM940, "snm940Serial", string.Empty, 91, 181, DateTime.UtcNow.AddDays(-3));
      var projectForProjectUid = projectOfInterest;
      var projectListForProjectAccountUid = new ProjectDataResult();
      var projectDeviceLicenseResponseModel = new DeviceLicenseResponseModel() { Total = 1 };

      var radioSerialDeviceUid = Guid.NewGuid().ToString();
      var radioSerialAccountUid = Guid.NewGuid().ToString();
      var radioSerialDevice = new DeviceData { CustomerUID = radioSerialAccountUid, DeviceUID = radioSerialDeviceUid };
      var projectListForRadioSerial = new ProjectDataResult() { ProjectDescriptors = new List<ProjectData>() { projectOfInterest } };
      //var radioSerialDeviceLicenseResponseModel = (DeviceLicenseResponseModel) null;

      var ec520Uid = Guid.NewGuid().ToString();
      //var ec520AccountUid = Guid.NewGuid().ToString();
      var ec520Device = (DeviceData)null;
      var projectListForEC520 = (ProjectDataResult)null;
      //var ec520DeviceLicenseResponseModel = (DeviceLicenseResponseModel)null;

      var expectedGetProjectAndAssetUidsResult = new GetProjectAndAssetUidsResult(string.Empty, string.Empty);


      await ExecuteManual
        (getProjectAndAssetUidsRequest,
          projectAccountUid, projectForProjectUid, projectListForProjectAccountUid, projectDeviceLicenseResponseModel,
          radioSerialDevice, projectListForRadioSerial, // radioSerialDeviceLicenseResponseModel,
          ec520Device, projectListForEC520, // ec520DeviceLicenseResponseModel,
          ServiceProvider.GetService<ICustomRadioSerialProjectMap>(),
          expectedGetProjectAndAssetUidsResult, expectedCode: 3043, expectedMessage: ContractExecutionStatesEnum.FirstNameWithOffset(43)
        );
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Sad_ProjectAccountHasNoDeviceLicenses()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectAccountUid = Guid.NewGuid().ToString();
      var projectOfInterest = new ProjectData
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = projectAccountUid
      };

      var getProjectAndAssetUidsRequest = new GetProjectAndAssetUidsRequest(projectUid, (int)TagFileDeviceTypeEnum.SNM940, "snm940Serial", string.Empty, 91, 181, DateTime.UtcNow.AddDays(-3));
      var projectForProjectUid = projectOfInterest;
      var projectListForProjectAccountUid = new ProjectDataResult();
      var projectDeviceLicenseResponseModel = new DeviceLicenseResponseModel() { Total = 0 };

      var radioSerialDeviceUid = Guid.NewGuid().ToString();
      var radioSerialAccountUid = Guid.NewGuid().ToString();
      var radioSerialDevice = new DeviceData { CustomerUID = radioSerialAccountUid, DeviceUID = radioSerialDeviceUid };
      var projectListForRadioSerial = new ProjectDataResult() { ProjectDescriptors = new List<ProjectData>() { projectOfInterest } };
      //var radioSerialDeviceLicenseResponseModel = (DeviceLicenseResponseModel) null;

      var ec520Uid = Guid.NewGuid().ToString();
      //var ec520AccountUid = Guid.NewGuid().ToString();
      var ec520Device = (DeviceData)null;
      var projectListForEC520 = (ProjectDataResult)null;
      //var ec520DeviceLicenseResponseModel = (DeviceLicenseResponseModel)null;

      var expectedGetProjectAndAssetUidsResult = new GetProjectAndAssetUidsResult(string.Empty, string.Empty);


      await ExecuteManual
        (getProjectAndAssetUidsRequest,
          projectAccountUid, projectForProjectUid, projectListForProjectAccountUid, projectDeviceLicenseResponseModel,
          radioSerialDevice, projectListForRadioSerial, // radioSerialDeviceLicenseResponseModel,
          ec520Device, projectListForEC520, // ec520DeviceLicenseResponseModel,
          ServiceProvider.GetService<ICustomRadioSerialProjectMap>(),
          expectedGetProjectAndAssetUidsResult, expectedCode: 3031, expectedMessage: ContractExecutionStatesEnum.FirstNameWithOffset(31)
        );
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Sad_Project_NoIntersectingProjectBoundaries()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectAccountUid = Guid.NewGuid().ToString();
      var projectOfInterest = new ProjectData
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = projectAccountUid
      };

      var getProjectAndAssetUidsRequest = new GetProjectAndAssetUidsRequest(projectUid, (int)TagFileDeviceTypeEnum.SNM940, "snm940Serial", string.Empty, 91, 181, DateTime.UtcNow.AddDays(-3));
      var projectForProjectUid = projectOfInterest;
      var projectListForProjectAccountUid = new ProjectDataResult();
      var projectDeviceLicenseResponseModel = new DeviceLicenseResponseModel() { Total = 1 };

      var radioSerialDeviceUid = Guid.NewGuid().ToString();
      var radioSerialAccountUid = Guid.NewGuid().ToString();
      var radioSerialDevice = new DeviceData { CustomerUID = radioSerialAccountUid, DeviceUID = radioSerialDeviceUid };
      var projectListForRadioSerial = new ProjectDataResult() { ProjectDescriptors = new List<ProjectData>() { projectOfInterest } };
      //var radioSerialDeviceLicenseResponseModel = (DeviceLicenseResponseModel) null;

      var ec520Uid = Guid.NewGuid().ToString();
      //var ec520AccountUid = Guid.NewGuid().ToString();
      var ec520Device = (DeviceData)null;
      var projectListForEC520 = (ProjectDataResult)null;
      //var ec520DeviceLicenseResponseModel = (DeviceLicenseResponseModel)null;

      var expectedGetProjectAndAssetUidsResult = new GetProjectAndAssetUidsResult(string.Empty, radioSerialDeviceUid);


      await ExecuteManual
        (getProjectAndAssetUidsRequest,
          projectAccountUid, projectForProjectUid, projectListForProjectAccountUid, projectDeviceLicenseResponseModel,
          radioSerialDevice, projectListForRadioSerial, // radioSerialDeviceLicenseResponseModel,
          ec520Device, projectListForEC520, // ec520DeviceLicenseResponseModel,
          ServiceProvider.GetService<ICustomRadioSerialProjectMap>(),
          expectedGetProjectAndAssetUidsResult, expectedCode: 3041, expectedMessage: ContractExecutionStatesEnum.FirstNameWithOffset(41)
        );
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Sad_Project_TooManyIntersectingProjectBoundaries()
    {
      // this scenario should not be possible, this should be an internal error
      var projectUid = Guid.NewGuid().ToString();
      var projectAccountUid = Guid.NewGuid().ToString();
      var projectOfInterest = new ProjectData
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = projectAccountUid
      };
      var projectOfInterest2 = new ProjectData
      {
        ProjectUID = Guid.NewGuid().ToString(),
        ProjectType = ProjectType.Standard,
        CustomerUID = projectAccountUid
      };

      var getProjectAndAssetUidsRequest = new GetProjectAndAssetUidsRequest(projectUid, (int)TagFileDeviceTypeEnum.SNM940, "snm940Serial", string.Empty, 91, 181, DateTime.UtcNow.AddDays(-3));
      var projectForProjectUid = projectOfInterest;
      var projectListForProjectAccountUid = new ProjectDataResult() { ProjectDescriptors = new List<ProjectData>() { projectOfInterest, projectOfInterest2 } };
      var projectDeviceLicenseResponseModel = new DeviceLicenseResponseModel() { Total = 1 };

      var radioSerialDeviceUid = Guid.NewGuid().ToString();
      var radioSerialAccountUid = Guid.NewGuid().ToString();
      var radioSerialDevice = new DeviceData() { CustomerUID = radioSerialAccountUid, DeviceUID = radioSerialDeviceUid };
      var projectListForRadioSerial = new ProjectDataResult() { ProjectDescriptors = new List<ProjectData>() { projectOfInterest } };
      //var radioSerialDeviceLicenseResponseModel = (DeviceLicenseResponseModel) null;

      var ec520Uid = Guid.NewGuid().ToString();
      //var ec520AccountUid = Guid.NewGuid().ToString();
      var ec520Device = (DeviceData)null;
      var projectListForEC520 = (ProjectDataResult)null;
      //var ec520DeviceLicenseResponseModel = (DeviceLicenseResponseModel)null;

      var expectedGetProjectAndAssetUidsResult = new GetProjectAndAssetUidsResult(string.Empty, radioSerialDeviceUid);


      await ExecuteManual
        (getProjectAndAssetUidsRequest,
          projectAccountUid, projectForProjectUid, projectListForProjectAccountUid, projectDeviceLicenseResponseModel,
          radioSerialDevice, projectListForRadioSerial, // radioSerialDeviceLicenseResponseModel,
          ec520Device, projectListForEC520, // ec520DeviceLicenseResponseModel,
          ServiceProvider.GetService<ICustomRadioSerialProjectMap>(),
          expectedGetProjectAndAssetUidsResult, expectedCode: 3049, expectedMessage: ContractExecutionStatesEnum.FirstNameWithOffset(49)
        );
    }

    [TestMethod]
    public async Task TRexExecutor_Sad_InvalidParameters()
    {
      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsExecutor>(
        _loggerFactory.CreateLogger<ProjectAndAssetUidsExecutorManualTests>(), ConfigStore, authorization.Object,
         cwsAccountClient.Object, projectProxy.Object, deviceProxy.Object, requestCustomHeaders);

      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(() =>
        executor.ProcessAsync((GetProjectAndAssetUidsRequest)null));

      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);
      Assert.AreEqual(-3, ex.GetResult.Code);
      Assert.AreEqual("Serialization error", ex.GetResult.Message);
    }

    private async Task ExecuteManual(GetProjectAndAssetUidsRequest request,
      string projectAccountUid, ProjectData projectForProjectUid, ProjectDataResult projectListForProjectAccountUid, DeviceLicenseResponseModel projectDeviceLicenseResponseModel,
      DeviceData radioSerialDevice, ProjectDataResult projectListForRadioSerial, // DeviceLicenseResponseModel radioSerialDeviceLicenseResponseModel,
      DeviceData ec520Device, ProjectDataResult projectListForEC520, // DeviceLicenseResponseModel ec520DeviceLicenseResponseModel,
      ICustomRadioSerialProjectMap customRadioSerialProjectMap,
      GetProjectAndAssetUidsResult expectedGetProjectAndAssetUidsResult, int expectedCode, string expectedMessage
  )
    {
      projectProxy.Setup(p => p.GetProject(request.ProjectUid, It.IsAny<HeaderDictionary>())).ReturnsAsync(projectForProjectUid);
      projectProxy.Setup(p => p.GetIntersectingProjects(projectAccountUid, It.IsAny<double>(), It.IsAny<double>(), request.ProjectUid, It.IsAny<HeaderDictionary>()))
            .ReturnsAsync(projectListForProjectAccountUid);
      cwsAccountClient.Setup(p => p.GetDeviceLicenses(new Guid(projectAccountUid), It.IsAny<HeaderDictionary>())).ReturnsAsync(projectDeviceLicenseResponseModel);


      deviceProxy.Setup(d => d.GetDevice(request.RadioSerial, It.IsAny<HeaderDictionary>())).ReturnsAsync(radioSerialDevice);
      if (radioSerialDevice != null)
      {
        deviceProxy.Setup(d => d.GetProjectsForDevice(radioSerialDevice.DeviceUID, It.IsAny<HeaderDictionary>())).ReturnsAsync(projectListForRadioSerial);
        // cwsAccountClient.Setup(p => p.GetDeviceLicenses(new Guid(radioSerialDevice.CustomerUID), null)).ReturnsAsync(radioSerialDeviceLicenseResponseModel);  
      }

      deviceProxy.Setup(d => d.GetDevice(request.Ec520Serial, It.IsAny<HeaderDictionary>())).ReturnsAsync(ec520Device);
      if (ec520Device != null)
      {
        deviceProxy.Setup(d => d.GetProjectsForDevice(ec520Device.DeviceUID, It.IsAny<HeaderDictionary>())).ReturnsAsync(projectListForEC520);
        // cwsAccountClient.Setup(p => p.GetDeviceLicenses(new Guid(ec520Device.CustomerUID), null)).ReturnsAsync(ec520DeviceLicenseResponseModel);
      }


      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsExecutor>(
        _loggerFactory.CreateLogger<ProjectAndAssetUidsExecutorManualTests>(), ConfigStore, authorization.Object,
         cwsAccountClient.Object, projectProxy.Object, deviceProxy.Object, requestCustomHeaders);
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
