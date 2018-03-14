﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.Types
{
    /// <summary>
    /// Vibration states reported by a machine for a vibratory compaction drum
    /// </summary>
    public enum VibrationState
    {
        /// <summary>
        /// Vibration is off
        /// </summary>
        Off = 0,

        /// <summary>
        /// Vibration is on
        /// </summary>
        On = 1,

        /// <summary>
        /// Unknown/inmvalid vibration state
        /// </summary>
        Invalid = 2
    }
}
