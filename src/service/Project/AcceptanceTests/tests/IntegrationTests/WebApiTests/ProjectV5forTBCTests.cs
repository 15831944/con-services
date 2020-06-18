﻿using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TestUtility;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using Xunit;

namespace IntegrationTests.WebApiTests
{
  public class ProjectV5ForTBCTests : WebApiTestsBase
  {
    private static List<TBCPoint> _boundaryLL;

    public ProjectV5ForTBCTests()
    {
      _boundaryLL = new List<TBCPoint>
      {
        new TBCPoint(-43.5, 172.6),
        new TBCPoint(-43.5003, 172.6),
        new TBCPoint(-43.5003, 172.603),
        new TBCPoint(-43.5, 172.603)
      };
    }

    [Fact]
    public async Task Create_ProjectV5_All_Ok()
    {
      Msg.Title("Project V5TBC", "Create a project");
      var ts = new TestSupport();

      var serialized = JsonConvert.SerializeObject(_boundaryLL);
      Assert.Equal(@"[{""Latitude"":-43.5,""Longitude"":172.6},{""Latitude"":-43.5003,""Longitude"":172.6},{""Latitude"":-43.5003,""Longitude"":172.603},{""Latitude"":-43.5,""Longitude"":172.603}]", serialized);

      var response = await CreateProjectV5TBC(ts, "project 1", CwsProjectType.AcceptsTagFiles);
      var createProjectV5Result = JsonConvert.DeserializeObject<ReturnLongV5Result>(response);

      Assert.Equal(HttpStatusCode.Created, createProjectV5Result.Code);
      Assert.NotEqual(-1, createProjectV5Result.Id);
    }


    [Fact]
    public async Task ValidateTBCOrg_All_Ok()
    {
      Msg.Title("Project V5TBC", "Validate TBCOrg endpoint");
      var ts = new TestSupport();

      var response = await ValidateTbcOrgId(ts, "the sn");

      Assert.Equal(HttpStatusCode.OK, response.Code);
      Assert.True(response.Success, "Validation not flagged as successful.");
    }

    private static async Task<ReturnSuccessV5Result> ValidateTbcOrgId(TestSupport ts, string orgShortName)
    {
      var response = await ts.ValidateTbcOrgIdApiV5(orgShortName);

      return JsonConvert.DeserializeObject<ReturnSuccessV5Result>(response);
    }

    private static Task<string> CreateProjectV5TBC(TestSupport ts, string projectName, CwsProjectType projectType)
    {
      return ts.CreateProjectViaWebApiV5TBC(projectName, ts.FirstEventDate, ts.LastEventDate, "New Zealand Standard Time", projectType, _boundaryLL);
    }
  }
}
