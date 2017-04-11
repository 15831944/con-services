﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.Types
{
    /// <summary>
    /// Global Positioning Accuracy metric emitted from the GCS900 machine control system at the time cell passes
    /// are being measured
    /// </summary>
    public enum GPSAccuracy
    {
        Fine,
        Medium,
        Coarse,
        Unknown
    }
}
