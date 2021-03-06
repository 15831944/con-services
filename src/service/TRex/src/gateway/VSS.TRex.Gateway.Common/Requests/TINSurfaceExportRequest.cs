﻿using System;
using VSS.Productivity3D.Filter.Abstractions.Models;
using VSS.Productivity3D.Models.Models;

namespace VSS.TRex.Gateway.Common.Requests
{
  public class TINSurfaceExportRequest
  {
    public Guid? ProjectUid { get; set; }
    public FilterResult Filter { get; set; }
    public string FileName { get; set; }
    public double? Tolerance { get; set; }

    public void Validate()
    {
    }
  }
}
