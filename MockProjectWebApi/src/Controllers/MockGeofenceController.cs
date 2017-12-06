﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;

namespace MockProjectWebApi.Controllers
{
  public class MockGeofenceController : Controller
  {
    [Route("api/v1/mock/geofences")]
    [HttpGet]
    public GeofenceDataResult GetMockGeofences()
    {
      Console.WriteLine("GetMockGeofences");
      var geofences = new List<GeofenceData>
      {
        //Copied from MockBoundaryController
        new GeofenceData
        {
          GeofenceName = "Dimensions boundary CMV",
          GeofenceUID = Guid.Parse("c910d127-5e3c-453f-82c3-e235848ac20e"),
          GeometryWKT = "POLYGON((-115.020509 36.207183,-115.020187 36.206862,-115.019731 36.207174,-115.020509 36.207183))"         
        },
        new GeofenceData
        {
          GeofenceName = "Inside Dimensions project",
          GeofenceUID = Guid.Parse("d4edddc9-d07f-4d56-ad50-5e9671631f70"),
          GeometryWKT = "POLYGON((-115.020 36.207,-115.021 36.2075,-115.023 36.208,-115.020 36.207))",
          FillColor = 16711680,//red
          IsTransparent = false         
        }
      };
      return new GeofenceDataResult {Geofences = geofences};
    }
  }
}
