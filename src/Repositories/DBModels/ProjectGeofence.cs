﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.TagFileAuthentication.Data.Models
{
  public class ProjectGeofence
  {
    public string ProjectUID { get; set; }
    public string GeofenceUID { get; set; }
    public DateTime LastActionedUTC { get; set; }
  }
}
