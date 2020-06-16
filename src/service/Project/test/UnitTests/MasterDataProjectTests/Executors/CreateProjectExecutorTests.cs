﻿using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.Configuration;
using VSS.DataOcean.Client;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Productivity3D.Models.Coord.ResultHandling;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;
using VSS.WebApi.Common;
using Xunit;

namespace VSS.MasterData.ProjectTests.Executors
{
  public class CreateProjectExecutorTests : UnitTestsDIFixture<CreateProjectExecutorTests>
  {
    public CreateProjectExecutorTests()
    {
      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();
    }

    [Fact]
    public async Task CreateProjectV6Executor_HappyPath()
    {
      var coordSystemFileContent = "Some dummy content";
      var request = CreateProjectRequest.CreateACreateProjectRequest
      (Guid.NewGuid().ToString(),
        CwsProjectType.AcceptsTagFiles, "projectName", "NZ whatsup",
        "POLYGON((172.595831670724 -43.5427038560109,172.594630041089 -43.5438859356773,172.59329966542 -43.542486101965, 172.595831670724 -43.5427038560109))",
        "some coord file", System.Text.Encoding.ASCII.GetBytes(coordSystemFileContent));
      var createProjectEvent = AutoMapperUtility.Automapper.Map<CreateProjectEvent>(request);
      createProjectEvent.ActionUTC = DateTime.UtcNow;
      
      var createProjectResponseModel = new CreateProjectResponseModel() { TRN = _projectTrn };
      var project = CreateProjectDetailModel(_customerTrn, _projectTrn, request.ProjectName);
      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(pr => pr.CreateProject(It.IsAny<CreateProjectRequestModel>(), _customHeaders)).ReturnsAsync(createProjectResponseModel);
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);
      cwsProjectClient.Setup(ps => ps.GetMyProject(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(project);

      var createFileResponseModel = new CreateFileResponseModel
      { FileSpaceId = "2c171c20-ca7a-45d9-a6d6-744ac39adf9b", UploadUrl = "an upload url" };
      var cwsDesignClient = new Mock<ICwsDesignClient>();
      cwsDesignClient.Setup(d => d.CreateAndUploadFile(It.IsAny<Guid>(), It.IsAny<CreateFileRequestModel>(), It.IsAny<Stream>(), _customHeaders))
        .ReturnsAsync(createFileResponseModel);

      var projectConfigurationModel = new ProjectConfigurationModel
      {
        FileName = "some coord sys file",
        FileDownloadLink = "some download link"
      };
      var cwsProfileSettingsClient = new Mock<ICwsProfileSettingsClient>();
      cwsProfileSettingsClient.Setup(ps => ps.SaveProjectConfiguration(It.IsAny<Guid>(), ProjectConfigurationFileType.CALIBRATION, It.IsAny<ProjectConfigurationFileRequestModel>(), _customHeaders))
        .ReturnsAsync(projectConfigurationModel);

      var httpContextAccessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
      httpContextAccessor.HttpContext.Request.Path = new PathString("/api/v6/projects");

      var productivity3dV1ProxyCoord = new Mock<IProductivity3dV1ProxyCoord>();
      productivity3dV1ProxyCoord.Setup(p =>
          p.CoordinateSystemValidate(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<HeaderDictionary>()))
        .ReturnsAsync(new CoordinateSystemSettingsResult());
      productivity3dV1ProxyCoord.Setup(p => p.CoordinateSystemPost(It.IsAny<Guid>(), It.IsAny<byte[]>(), It.IsAny<string>(),
          It.IsAny<HeaderDictionary>()))
        .ReturnsAsync(new CoordinateSystemSettingsResult());

      var dataOceanClient = new Mock<IDataOceanClient>();
      dataOceanClient.Setup(f => f.FolderExists(It.IsAny<string>(), It.IsAny<HeaderDictionary>())).ReturnsAsync(true);
      dataOceanClient.Setup(f => f.PutFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(),
        It.IsAny<HeaderDictionary>())).ReturnsAsync(true);

      var authn = new Mock<ITPaaSApplicationAuthentication>();
      authn.Setup(a => a.GetApplicationBearerToken()).Returns("some token");

      var executor = RequestExecutorContainerFactory.Build<CreateProjectExecutor>
      (_loggerFactory, _configStore, ServiceExceptionHandler, 
        _customerUid.ToString(), _userUid.ToString(), null, _customHeaders,
        productivity3dV1ProxyCoord.Object, httpContextAccessor: httpContextAccessor,
        dataOceanClient: dataOceanClient.Object, authn: authn.Object,
        cwsProjectClient: cwsProjectClient.Object, cwsDesignClient: cwsDesignClient.Object,
        cwsProfileSettingsClient: cwsProfileSettingsClient.Object);
      var result = await executor.ProcessAsync(createProjectEvent) as ProjectV6DescriptorsSingleResult;

      Assert.NotNull(result);
      Assert.False(string.IsNullOrEmpty(result.ProjectDescriptor.ProjectUid));
      Assert.Equal(request.ProjectName, result.ProjectDescriptor.Name);
    }
  }
}
