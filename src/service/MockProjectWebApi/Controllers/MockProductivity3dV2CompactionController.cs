﻿using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MockProjectWebApi.Utils;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Http;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Productivity3D.Models.Compaction.ResultHandling;
using VSS.Productivity3D.Productivity3D.Models.Designs;
using VSS.Productivity3D.Project.Abstractions.Models;

namespace MockProjectWebApi.Controllers
{
  public class MockProductivity3dV2CompactionController : BaseController
  {
    public MockProductivity3dV2CompactionController(ILoggerFactory loggerFactory) : base(loggerFactory)
    { }

    [Route("api/v2/designs/alignment/master/geometry")]
    [HttpGet]
    public AlignmentGeometryResult GetLineworkFromAlignment([FromQuery] Guid projectUid, [FromQuery] Guid alignmentUid)
    {
      return new AlignmentGeometryResult
      (
        0,
        new AlignmentGeometry
        (
          alignmentUid,
          "Test.svl",
          new[] { new[] { new double[] { 1, 2, 3 } } },
          new[] { new AlignmentGeometryResultArc(0, 1, 2, 3, 4, 5, 6, 7, 8, true) },
          new[] { new AlignmentGeometryResultLabel(0, 1, 2, 3), }
        )
      );
    }

    /// <summary>
    /// Returns some project stats.
    /// If Project is Dimensions then the date extents will be static
    /// otherwise they will be from now until 1 year ago
    /// </summary>
    [Route("api/v2/projectstatistics")]
    [HttpGet]
    public ProjectStatisticsResult GetProjectStatistics(
      [FromQuery] Guid projectUid
    )
    {
      bool isDimensions = projectUid.ToString() == ConstantsUtil.DIMENSIONS_PROJECT_UID;
      var extents =
        $"{{\"startTime\":\"{(isDimensions ? "2012-10-30T00:12:09.109" : DateTime.UtcNow.AddYears(-1).ToString("s"))}\"," +
        $"\"endTime\":\"{(isDimensions ? "2012-11-08T01:00:08.756" : DateTime.UtcNow.ToString("s"))}\"," +
        $"\"cellSize\":0.34,\"indexOriginOffset\":536870912,\"extents\":{{\"maxX\":2913.2900000000004,\"maxY\":1250.69,\"maxZ\":624.1365966796875,\"minX\":2306.05,\"minY\":1125.2300000000002,\"minZ\":591.953857421875}},\"Code\":0,\"Message\":\"success\"}}";

      Logger.LogInformation($"GetProjectStatistics: res: {extents} ProjectUID {projectUid}");
      return JsonConvert.DeserializeObject<ProjectStatisticsResult>(extents);
    }

    /// <summary>
    /// Dummies the project projectSettings validation.
    /// </summary>
    [Route("api/v2/validatesettings")]
    [HttpGet]
    public BaseDataResult DummyValidateProjectSettingsGet(
      [FromQuery] Guid projectUid,
      [FromQuery] string projectSettings)
    {
      var res = new BaseDataResult();
      var message = $"DummyValidateProjectSettingsGet: res {res}. projectSettings {projectSettings}";
      Logger.LogInformation(message);
      return res;
    }

    /// <summary>
    /// Dummies the project projectSettings validation.
    /// </summary>
    [Route("api/v2/validatesettings")]
    [HttpPost]
    public BaseDataResult DummyValidateProjectSettingsPost([FromBody] ProjectSettingsRequest request)
    {
      var res = new BaseDataResult();
      var message = $"DummyValidateProjectSettingsGet: res {res}. projectSettings {request.Settings}";
      Logger.LogInformation(message);
      return res;
    }

    [Route("api/v2/export/veta")]
    [HttpGet]
    public async Task<IActionResult> GetMockVetaExportData(
     [FromQuery] Guid projectUid,
     [FromQuery] string fileName,
     [FromQuery] string machineNames,
     [FromQuery] Guid? filterUid)
    {
      if (projectUid.ToString() == ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1)
      {
        if ((fileName == MockSchedulerController.SUCCESS_JOB_ID ||
             fileName == MockSchedulerController.TIMEOUT_JOB_ID) &&
            string.IsNullOrEmpty(machineNames) &&
            filterUid.ToString() == "81422acc-9b0c-401c-9987-0aedbf153f1d")
        {
          var result = @"{
          ""exportData"": ""UEsDBBQAAAgIAEsQe0tesHMI2AEAANUIAAAIAAAAVGVzdC5jc3bNlM1um0AQgO+V+g4op1aabPeH5Sc3Sn4q1SDLRpZ6stawjVcB1gXiqn21HvpIfYUu1CZSAlVRKsVcZhdmhk/fDvz68TNRhYRQ5nm8Lrp4ZeJVLveiUbo067mo6/i+2MgKZqJuFiJTetaU6Te4lLW6LWNhGkQi3apSwnInZba+K95tu+Sb+TLSmQQTgzRNdG76JaK6bXuG+r5sYCVylc11DTP1uelqwmjV5bSx3UeX827fxg7gcP+6kl/WH75366DYrYs/rZOtSu9KWdfmVQeqGykqWKmNXDaikV1BIk1F+voVxYSex3p/jrmF/QtuXzAHMQ8DoZgjwjBQj1LECQbue4i75glgiIK5hQmchUFihUvuvD8DijDMdNpZsxbJRwi1qGppvcGIYq94Cww+ydpUA8cmlTAX+UC6tbnaJAwLuZemBmLdWMFul6tUbHIJg5juA6btPsJ0qDuGySZi2m3BMzD5gE2PH2xye9ymPc0mRu5jzGtdfRVV9k+YQ4feY7Z6T+PQ+cCh95hs9NB9RKbatJ9jc2g2j5iOwRyx+fKzyRDxj7Ppj39C7jSbBNH/ZPMp5knOZo/J2QHTYadss8c8bZuee7T5l9/7y9vsMV06bpNPw/Qn2PwNUEsBAhQAFAAACAgASxB7S16wcwjYAQAA1QgAAAgAAAAAAAAAAAAgAAAAAAAAAFRlc3QuY3N2UEsFBgAAAAABAAEANgAAAP4BAAAAAA=="",
          ""resultCode"": 0,
          ""Code"": 0,
          ""Message"": ""success""
          }";
          if (fileName == MockSchedulerController.TIMEOUT_JOB_ID)
          {
            //Default http request timeout is 100 seconds so make it a bit longer
            await Task.Delay(TimeSpan.FromSeconds(105));
          }

          var exportResult = JsonConvert.DeserializeObject<ExportResult>(result);
          return new FileStreamResult(new MemoryStream(exportResult.ExportData), ContentTypeConstants.ApplicationZip);
        }

        if (fileName == MockSchedulerController.FAILURE_JOB_ID &&
            string.IsNullOrEmpty(machineNames) &&
            filterUid.ToString() == "1cf81668-1739-42d5-b068-ea025588796a")
        {
          return BadRequest(new ContractExecutionResult(2002, "Failed to get requested export data with error: No data for export"));
        }
      }

      return BadRequest(new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Unknown project or file for mock export data"));
    }
  }
}
