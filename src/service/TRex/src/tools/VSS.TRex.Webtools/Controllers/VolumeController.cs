﻿using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.TRex.Filters;
using VSS.TRex.Common;
using VSS.TRex.Volumes.GridFabric.Arguments;
using VSS.TRex.Volumes.GridFabric.Requests;

namespace VSS.TRex.Webtools.Controllers
{
  [Route("api/volumes")]
  public class VolumeController : Controller
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

    /// <summary>
    /// Generates a tile...
    /// </summary>
    /// <param name="siteModelID">Grid to return status for</param>
    /// <param name="filter"></param>
    /// <returns></returns>
    [HttpGet("{siteModelID}")]
    public async Task<JsonResult> GetVolume(string siteModelID,
    [FromQuery] string filter)
    {
      var filterJson = Encoding.ASCII.GetString(Convert.FromBase64String(filter));

      // Create the two filters necessary to capture the earliest and latest surfaces for the volume
      var baseFilter = JsonConvert.DeserializeObject<CombinedFilter>(filterJson);
      baseFilter.AttributeFilter.ReturnEarliestFilteredCellPass = true;
      baseFilter.SpatialFilter.Fence?.UpdateExtents();
      var topFilter = JsonConvert.DeserializeObject<CombinedFilter>(filterJson);
      topFilter.SpatialFilter.Fence?.UpdateExtents();

      var request = new SimpleVolumesRequest_ApplicationService();
       var response = await request.ExecuteAsync(new SimpleVolumesRequestArgument
       {
         ProjectID = Guid.Parse(siteModelID),
         VolumeType = VolumeComputationType.Between2Filters,
         BaseFilter = baseFilter,
         TopFilter = topFilter
      });

      return new JsonResult(response);
    }
  }
}
