﻿namespace VSS.TRex.Types
{
    /// <summary>
    /// The mode of the automatics blade control system within the machine control software
    /// </summary>
    public enum MachineAutomaticsMode : byte
  {
        /// <summary>
        /// Automatics mode is unavailable or unknown
        /// </summary>
        Unknown,

        /// <summary>
        /// Blade control is manually operated
        /// </summary>
        Manual,

        /// <summary>
        /// Blade control is automatically operated by the machine control system
        /// </summary>
        Automatics
    }
}
