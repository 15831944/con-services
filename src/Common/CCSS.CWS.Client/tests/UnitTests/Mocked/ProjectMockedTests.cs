﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSS.Common.Abstractions.Clients.CWS;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using Xunit;

namespace CCSS.CWS.Client.UnitTests.Mocked
{
  public class ProjectMockedTests : BaseTestClass
  {
    protected override IServiceCollection SetupTestServices(IServiceCollection services)
    {
      services.AddSingleton(mockWebRequest.Object);
      services.AddSingleton(mockServiceResolution.Object);
      services.AddTransient<ICwsProjectClient, CwsProjectClient>();

      return services;
    }

    [Fact]
    public void CreateProjectTest()
    {
      var customerUid = new Guid("560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97");
      var expectedProjectUid = "560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97";
      var expectedProjectTrn = TRNHelper.MakeTRN(expectedProjectUid);

      var createProjectRequestModel = new CreateProjectRequestModel
      {
        AccountId = customerUid.ToString(),
        ProjectName = "my first project",
        Timezone = "Mountain Standard Time",
        Boundary = new ProjectBoundary()
        {
          type = "Polygon",
          coordinates = new List<double[,]>() { { new double[,] { { 150.3, 1.2 }, { 150.4, 1.2 }, { 150.4, 1.3 }, { 150.4, 1.4 }, { 150.3, 1.2 } } } }
        }
      };

      var createProjectResponseModel = new CreateProjectResponseModel
      {
        TRN = expectedProjectTrn
      };
      const string route = "/projects";
      var expectedUrl = $"{baseUrl}{route}";
      mockServiceResolution.Setup(m => m.ResolveRemoteServiceEndpoint(
        It.IsAny<string>(), It.IsAny<ApiType>(), It.IsAny<ApiVersion>(), route, It.IsAny<IList<KeyValuePair<string, string>>>())).Returns(Task.FromResult(expectedUrl));

      MockUtilities.TestRequestSendsCorrectJson("Create a project", mockWebRequest, null, expectedUrl, HttpMethod.Post, createProjectResponseModel, async () =>
      {
        var client = ServiceProvider.GetRequiredService<ICwsProjectClient>();
        var result = await client.CreateProject(createProjectRequestModel);

        Assert.NotNull(result);
        Assert.Equal(expectedProjectUid, result.Id);
        return true;
      });
    }

    [Fact]
    public void GetProjectsForMyAccount()
    {
      var customerUid = Guid.NewGuid();
      var userUid = Guid.NewGuid();
      var projectUid = Guid.NewGuid();

      var projectSummaryListResponseModel = new ProjectSummaryListResponseModel();
      projectSummaryListResponseModel.Projects.Add(new ProjectSummaryResponseModel()
      {
        ProjectTRN = TRNHelper.MakeTRN(projectUid),
        Role = UserProjectRoleEnum.Admin
      }
      );
      var route = $"/accounts/{TRNHelper.MakeTRN(customerUid, TRNHelper.TRN_ACCOUNT)}/projects";
      var expectedUrl = $"{baseUrl}{route}?from=0&limit=20";
      mockServiceResolution.Setup(m => m.ResolveRemoteServiceEndpoint(
        It.IsAny<string>(), It.IsAny<ApiType>(), It.IsAny<ApiVersion>(), route, It.IsAny<IList<KeyValuePair<string, string>>>())).Returns(Task.FromResult(expectedUrl));

      MockUtilities.TestRequestSendsCorrectJson("Get projects for account", mockWebRequest, null, expectedUrl, HttpMethod.Get, projectSummaryListResponseModel, async () =>
      {
        var client = ServiceProvider.GetRequiredService<ICwsProjectClient>();
        var result = await client.GetProjectsForMyCustomer(customerUid, userUid);

        Assert.NotNull(result);
        Assert.Single(result.Projects);
        Assert.Equal(projectUid.ToString(), result.Projects[0].ProjectId);
        return true;
      });
    }

    [Fact]
    public void GetMyProject()
    {
      var customerUid = Guid.NewGuid();
      var userUid = Guid.NewGuid();
      var projectUid = Guid.NewGuid();

      var projectDetailResponseModel = new ProjectDetailResponseModel()
      {
        AccountTRN = TRNHelper.MakeTRN(customerUid, TRNHelper.TRN_ACCOUNT),
        ProjectTRN = TRNHelper.MakeTRN(projectUid),
        ProjectSettings = new ProjectSettingsModel()
        {
          ProjectTRN = TRNHelper.MakeTRN(projectUid),
          TimeZone = "Pacific/Auckland",
          Boundary = new ProjectBoundary()
          {
            type = "Polygon",
            coordinates = new List<double[,]>() { { new double[,] { { 150.3, 1.2 }, { 150.4, 1.2 }, { 150.4, 1.3 }, { 150.4, 1.4 }, { 150.3, 1.2 } } } }
          },
          ConfigEntries = new List<ProjectConfiguration>() { new ProjectConfiguration() { FileType = ProjectConfigurationFileType.CALIBRATION.ToString() } } 
        }
      };


      var route = $"/projects/{TRNHelper.MakeTRN(projectUid)}";
      var expectedUrl = $"{baseUrl}{route}";
      mockServiceResolution.Setup(m => m.ResolveRemoteServiceEndpoint(
        It.IsAny<string>(), It.IsAny<ApiType>(), It.IsAny<ApiVersion>(), route, It.IsAny<IList<KeyValuePair<string, string>>>())).Returns(Task.FromResult(expectedUrl));

      MockUtilities.TestRequestSendsCorrectJson("Get my project", mockWebRequest, null, expectedUrl, HttpMethod.Get, projectDetailResponseModel, async () =>
      {
        var client = ServiceProvider.GetRequiredService<ICwsProjectClient>();
        var result = await client.GetMyProject(projectUid, userUid);

        Assert.NotNull(result);
        Assert.Equal(customerUid.ToString(), result.AccountId);
        Assert.Equal(projectUid.ToString(), result.ProjectId);
        Assert.Equal(projectUid.ToString(), result.ProjectSettings.ProjectId);
        Assert.Equal("Polygon", result.ProjectSettings.Boundary.type);
        Assert.Equal(ProjectConfigurationFileType.CALIBRATION.ToString(), result.ProjectSettings.Config.ProjectConfigurations[0].FileType);
        return true;
      });
    }


    [Fact]
    public void UpdateProjectDetailsTest()
    {
      var customerUid = new Guid("560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97");
      //var accountTrn = "trn::profilex:us-west-2:account:{customerUid}";
      string projectUid = "560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97";
      string projectTrn = TRNHelper.MakeTRN(projectUid, TRNHelper.TRN_PROJECT);

      var updateProjectDetailsRequestModel = new UpdateProjectDetailsRequestModel
      {
        projectName = "my updated project"
      };

      string route = $"/projects/{projectTrn}";
      var expectedUrl = $"{baseUrl}{route}";
      mockServiceResolution.Setup(m => m.ResolveRemoteServiceEndpoint(
        It.IsAny<string>(), It.IsAny<ApiType>(), It.IsAny<ApiVersion>(), route, It.IsAny<IList<KeyValuePair<string, string>>>())).Returns(Task.FromResult(expectedUrl));

      MockUtilities.TestRequestSendsCorrectJson("Update a projects details", mockWebRequest, null, expectedUrl, HttpMethod.Post, updateProjectDetailsRequestModel, async () =>
      {
        var client = ServiceProvider.GetRequiredService<ICwsProjectClient>();
        await client.UpdateProjectDetails(new Guid(projectUid), updateProjectDetailsRequestModel);

        return true;
      });
    }

    [Fact]
    public void UpdateProjectBoundaryTest()
    {
      var customerUid = new Guid("560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97");
      //var accountTrn = "trn::profilex:us-west-2:account:{customerUid}";
      string projectUid = "560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97";
      string projectTrn = TRNHelper.MakeTRN(projectUid, TRNHelper.TRN_PROJECT);

      var projectBoundary = new ProjectBoundary()
      {
        type = "Polygon",
        coordinates = new List<double[,]>() { { new double[,] { { 151.3, 1.2 }, { 151.4, 1.2 }, { 151.4, 1.3 }, { 151.4, 1.4 }, { 151.3, 1.2 } } } }
      };

      string route = $"/projects/{projectTrn}/boundary";
      var expectedUrl = $"{baseUrl}{route}";
      mockServiceResolution.Setup(m => m.ResolveRemoteServiceEndpoint(
        It.IsAny<string>(), It.IsAny<ApiType>(), It.IsAny<ApiVersion>(), route, It.IsAny<IList<KeyValuePair<string, string>>>())).Returns(Task.FromResult(expectedUrl));

      MockUtilities.TestRequestSendsCorrectJson("Update a projects boundary", mockWebRequest, null, expectedUrl, HttpMethod.Post, projectBoundary, async () =>
      {
        var client = ServiceProvider.GetRequiredService<ICwsProjectClient>();
        await client.UpdateProjectBoundary(new Guid(projectUid), projectBoundary);

        return true;
      });
    }

  }
}
