﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MockProjectWebApi.Utils;
using Newtonsoft.Json;
using VSS.Productivity3D.TagFileAuth.Models;

namespace MockProjectWebApi.Controllers
{
  public class MockTagFileAuthV2ProjectController : BaseController
  {
    public MockTagFileAuthV2ProjectController(ILoggerFactory loggerFactory) : base(loggerFactory)
    { }

    [Route("api/v2/project/getUidsCTCT")]
    [HttpPost]
    public GetProjectAndAssetUidsCTCTResult GetProjectAndAssetUidsCTCT([FromBody]GetProjectAndAssetUidsCTCTRequest request)
    {
      Logger.LogInformation($"{nameof(GetProjectAndAssetUidsCTCT)} request {JsonConvert.SerializeObject(request)}");

      // this SNM940 exists on `VSS-TagFileAuth-Alpha` with a valid 3d sub (it's not on Dev)
      // Dims project and customer are on alpha tfa
      if (request.RadioSerial == "5051593854")
        return new GetProjectAndAssetUidsCTCTResult(ConstantsUtil.DIMENSIONS_PROJECT_UID, "039c1ee8-1f21-e311-9ee2-00505688274d", ConstantsUtil.DIMENSIONS_CUSTOMER_UID, true);

      return new GetProjectAndAssetUidsCTCTResult(string.Empty, string.Empty, string.Empty, false, 41, "Manual Import: no intersecting projects found");
    }

    [Route("api/v2/project/getUids")]
    [HttpPost]
    public GetProjectAndAssetUidsResult GetProjectAndAssetUids([FromBody]GetProjectAndAssetUidsRequest request)
    {
      Logger.LogInformation($"{nameof(GetProjectAndAssetUids)} request {JsonConvert.SerializeObject(request)}");

      // this SNM940 exists on `VSS-TagFileAuth-Alpha` with a valid 3d sub (it's not on Dev)
      if (request.RadioSerial == "5051593854")
       return new GetProjectAndAssetUidsResult(ConstantsUtil.DIMENSIONS_PROJECT_UID, "039c1ee8-1f21-e311-9ee2-00505688274d");

      return new GetProjectAndAssetUidsResult(string.Empty, string.Empty, 41, "Manual Import: no intersecting projects found");
    }
  }
}