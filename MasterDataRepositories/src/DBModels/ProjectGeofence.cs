﻿using System;

namespace Repositories.DBModels
{
    public class ProjectGeofence
    {
        public string ProjectUID { get; set; }
        public string GeofenceUID { get; set; }
        public DateTime LastActionedUTC { get; set; }
    }
}