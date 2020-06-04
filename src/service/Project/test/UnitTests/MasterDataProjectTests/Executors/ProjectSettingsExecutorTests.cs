﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Productivity3D.Models;
using VSS.Productivity3D.Project.Abstractions.Interfaces.Repository;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;
using Xunit;

namespace VSS.MasterData.ProjectTests.Executors
{
  public class ProjectSettingsExecutorTests : UnitTestsDIFixture<ProjectSettingsExecutorTests>
  {
    [Theory]
    [InlineData(ProjectSettingsType.Targets)]
    [InlineData(ProjectSettingsType.Colors)]
    public async Task GetProjectSettingsExecutor_NoDataExists(ProjectSettingsType settingsType)
    {
      var userEmailAddress = "whatever@here.there.com";

      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(ps => ps.GetProjectSettings(It.IsAny<string>(), It.IsAny<string>(), settingsType)).ReturnsAsync((ProjectSettings)null);

      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);

      var configStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();

      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(_projectUid.ToString(), string.Empty, settingsType);

      var executor = RequestExecutorContainerFactory.Build<GetProjectSettingsExecutor>
      (logger, configStore, serviceExceptionHandler,
        _customerUid.ToString(), _userUid.ToString(), userEmailAddress, 
        projectRepo: projectRepo.Object, cwsProjectClient: cwsProjectClient.Object);
      var result = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;

      Assert.NotNull(result);
      Assert.Equal(_projectUid.ToString(), result.projectUid);
      Assert.Null(result.settings);
      Assert.Equal(settingsType, result.projectSettingsType);
    }

    [Theory]
    [InlineData(ProjectSettingsType.Targets)]
    [InlineData(ProjectSettingsType.Colors)]
    public async Task GetProjectSettingsExecutor_DataExists(ProjectSettingsType settingsType)
    {
      var settings = string.Empty;

      var projectRepo = new Mock<IProjectRepository>();
      var projectSettings = new ProjectSettings { ProjectUid = _projectUid.ToString(), Settings = settings, ProjectSettingsType = settingsType, UserID = _userUid.ToString() };
      projectRepo.Setup(ps => ps.GetProjectSettings(It.IsAny<string>(), _userUid.ToString(), settingsType)).ReturnsAsync(projectSettings);

      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);

      var configStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();

      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(_projectUid.ToString(), settings, settingsType);

      var executor = RequestExecutorContainerFactory.Build<GetProjectSettingsExecutor>
      (logger, configStore, serviceExceptionHandler,
        _customerUid.ToString(), _userUid.ToString(), 
        projectRepo: projectRepo.Object, cwsProjectClient: cwsProjectClient.Object);
      var result = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;

      Assert.NotNull(result);
      Assert.Equal(_projectUid.ToString(), result.projectUid);
      Assert.Null(result.settings);
      Assert.Equal(settingsType, result.projectSettingsType);
    }

    [Fact]
    public async Task GetProjectSettingsExecutor_MultipleSettings()
    {
      var settings1 = string.Empty;
      var settings2 = @"{firstValue: 10, lastValue: 20}";
      var settingsType1 = ProjectSettingsType.ImportedFiles;
      var settingsType2 = ProjectSettingsType.Targets;

      var projectRepo = new Mock<IProjectRepository>();
      var projectSettings1 = new ProjectSettings { ProjectUid = _projectUid.ToString(), Settings = settings1, ProjectSettingsType = settingsType1, UserID = _userUid.ToString() };
      projectRepo.Setup(ps => ps.GetProjectSettings(It.IsAny<string>(), _userUid.ToString(), settingsType1)).ReturnsAsync(projectSettings1);
      var projectSettings2 = new ProjectSettings { ProjectUid = _projectUid.ToString(), Settings = settings2, ProjectSettingsType = settingsType2, UserID = _userUid.ToString() };
      projectRepo.Setup(ps => ps.GetProjectSettings(It.IsAny<string>(), _userUid.ToString(), settingsType2)).ReturnsAsync(projectSettings2);

      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);

      var configStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();

      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(_projectUid.ToString(), settings2, settingsType2);

      var executor = RequestExecutorContainerFactory.Build<GetProjectSettingsExecutor>
      (logger, configStore, serviceExceptionHandler,
        _customerUid.ToString(), _userUid.ToString(), 
        projectRepo: projectRepo.Object, cwsProjectClient: cwsProjectClient.Object);
      var result = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;

      var tempSettings = JsonConvert.DeserializeObject<JObject>(settings2);

      Assert.NotNull(result);
      Assert.Equal(_projectUid.ToString(), result.projectUid);
      Assert.NotNull(result.settings);
      Assert.Equal(tempSettings["firstValue"], result.settings["firstValue"]);
      Assert.Equal(tempSettings["lastValue"], result.settings["lastValue"]);
      Assert.Equal(settingsType2, result.projectSettingsType);
    }

    [Fact]
    public async Task GetProjectSettingsExecutor_ProjectCustomerValidationFails()
    {
      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(ps => ps.GetProjectSettings(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ProjectSettingsType>())).ReturnsAsync((ProjectSettings)null);

      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(new ProjectDetailListResponseModel());

      var configStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      var projectErrorCodesProvider = ServiceProvider.GetRequiredService<IErrorCodesProvider>();

      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(_projectUid.ToString(), string.Empty, ProjectSettingsType.Targets);

      var executor = RequestExecutorContainerFactory.Build<GetProjectSettingsExecutor>
      (logger, configStore, serviceExceptionHandler,
        _customerUid.ToString(), _userUid.ToString(), 
        projectRepo: projectRepo.Object, cwsProjectClient: cwsProjectClient.Object);
      var ex = await Assert.ThrowsAsync<ServiceException>(async () =>
        await executor.ProcessAsync(projectSettingsRequest));

      Assert.NotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(1)));
    }


    [Theory]
    [InlineData(ProjectSettingsType.Targets)]
    [InlineData(ProjectSettingsType.ImportedFiles)]
    [InlineData(ProjectSettingsType.Colors)]
    public async Task UpsertProjectSettingsExecutor(ProjectSettingsType settingsType)
    {
      var settings = settingsType != ProjectSettingsType.ImportedFiles ? @"{firstValue: 10, lastValue: 20}" : @"[{firstValue: 10, lastValue: 20}, {firstValue: 20, lastValue: 40}]";

      var projectRepo = new Mock<IProjectRepository>();
      var projectSettings = new ProjectSettings { ProjectUid = _projectUid.ToString(), Settings = settings, ProjectSettingsType = settingsType, UserID = _userUid.ToString() };
      projectRepo.Setup(ps => ps.GetProjectSettings(It.IsAny<string>(), _userUid.ToString(), settingsType)).ReturnsAsync(projectSettings);
      projectRepo.Setup(ps => ps.StoreEvent(It.IsAny<UpdateProjectSettingsEvent>())).ReturnsAsync(1);

      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);

      var configStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      
      var productivity3dV2ProxyCompaction = new Mock<IProductivity3dV2ProxyCompaction>();
      productivity3dV2ProxyCompaction.Setup(r => r.ValidateProjectSettings(It.IsAny<ProjectSettingsRequest>(),
        It.IsAny<HeaderDictionary>())).ReturnsAsync(new BaseMasterDataResult());

      var executor = RequestExecutorContainerFactory.Build<UpsertProjectSettingsExecutor>
        (logger, configStore, serviceExceptionHandler,
        _customerUid.ToString(), _userUid.ToString(), null, null,
        productivity3dV2ProxyCompaction: productivity3dV2ProxyCompaction.Object, 
        projectRepo: projectRepo.Object, cwsProjectClient: cwsProjectClient.Object);
      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(_projectUid.ToString(), settings, settingsType);
      var result = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;

      Assert.NotNull(result);
      Assert.Equal(_projectUid.ToString(), result.projectUid);
      Assert.NotNull(result.settings);
      Assert.Equal(settingsType, result.projectSettingsType);

      if (settingsType == ProjectSettingsType.Targets || settingsType == ProjectSettingsType.Colors)
      {
        var tempSettings = JsonConvert.DeserializeObject<JObject>(settings);

        Assert.Equal(tempSettings["firstValue"], result.settings["firstValue"]);
        Assert.Equal(tempSettings["lastValue"], result.settings["lastValue"]);
      }
      else
      {
        var tempObj = JsonConvert.DeserializeObject<JArray>(settings);
        var tempJObject = new JObject { ["importedFiles"] = tempObj };

        Assert.Equal(tempJObject["importedFiles"][0]["firstValue"], result.settings["importedFiles"][0]["firstValue"]);
        Assert.Equal(tempJObject["importedFiles"][0]["lastValue"], result.settings["importedFiles"][0]["lastValue"]);
        Assert.Equal(tempJObject["importedFiles"][1]["firstValue"], result.settings["importedFiles"][1]["firstValue"]);
        Assert.Equal(tempJObject["importedFiles"][1]["lastValue"], result.settings["importedFiles"][1]["lastValue"]);
      }
    }

    [Fact]
    public async Task UpsertProjectSettingsExecutor_MultipleSettings()
    {
      var settings1 = @"{firstValue: 10, lastValue: 20}";
      var settings2 = @"{firstValue: 30, lastValue: 40}";
      var settingsType1 = ProjectSettingsType.Targets;
      var settingsType2 = ProjectSettingsType.ImportedFiles;

      var projectRepo = new Mock<IProjectRepository>();
      var projectSettings1 = new ProjectSettings { ProjectUid = _projectUid.ToString(), Settings = settings1, ProjectSettingsType = settingsType1, UserID = _userUid.ToString() };
      projectRepo.Setup(ps => ps.GetProjectSettings(It.IsAny<string>(), _userUid.ToString(), settingsType1)).ReturnsAsync(projectSettings1);
      var projectSettings2 = new ProjectSettings { ProjectUid = _projectUid.ToString(), Settings = settings2, ProjectSettingsType = settingsType2, UserID = _userUid.ToString() };
      projectRepo.Setup(ps => ps.GetProjectSettings(It.IsAny<string>(), _userUid.ToString(), settingsType2)).ReturnsAsync(projectSettings2);
      projectRepo.Setup(ps => ps.StoreEvent(It.IsAny<UpdateProjectSettingsEvent>())).ReturnsAsync(1);

      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);

      var configStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();

      var productivity3dV2ProxyCompaction = new Mock<IProductivity3dV2ProxyCompaction>();
      productivity3dV2ProxyCompaction.Setup(r => r.ValidateProjectSettings(It.IsAny<ProjectSettingsRequest>(),
        It.IsAny<IHeaderDictionary>())).ReturnsAsync(new BaseMasterDataResult());

      var executor = RequestExecutorContainerFactory.Build<UpsertProjectSettingsExecutor>
      (logger, configStore, serviceExceptionHandler,
        _customerUid.ToString(), _userUid.ToString(), null, null,
        productivity3dV2ProxyCompaction: productivity3dV2ProxyCompaction.Object, 
        projectRepo: projectRepo.Object, cwsProjectClient: cwsProjectClient.Object);
      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(_projectUid.ToString(), settings1, settingsType1);
      var result = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;

      var tempSettings = JsonConvert.DeserializeObject<JObject>(settings1);

      Assert.NotNull(result);
      Assert.Equal(_projectUid.ToString(), result.projectUid);
      Assert.NotNull(result.settings);
      Assert.Equal(tempSettings["firstValue"], result.settings["firstValue"]);
      Assert.Equal(tempSettings["lastValue"], result.settings["lastValue"]);
      Assert.Equal(settingsType1, result.projectSettingsType);
    }

    [Fact]
    public void ProjectSettingsRequestShouldNotSerializeType()
    {
      var settings = "blah";

      var request = ProjectSettingsRequest.CreateProjectSettingsRequest(_projectUid.ToString(), settings, ProjectSettingsType.Targets);
      var json = JsonConvert.SerializeObject(request);
      Assert.DoesNotContain("ProjectSettingsType", json);
    }

    [Fact]
    public void ProjectSettingsResultShouldNotSerializeType()
    {
      var settings = @"{firstValue: 10, lastValue: 20}";

      var result = ProjectSettingsResult.CreateProjectSettingsResult(_projectUid.ToString(), JsonConvert.DeserializeObject<JObject>(settings), ProjectSettingsType.Targets);
      var json = JsonConvert.SerializeObject(result);
      Assert.DoesNotContain("ProjectSettingsType", json);
    }
  }
}
