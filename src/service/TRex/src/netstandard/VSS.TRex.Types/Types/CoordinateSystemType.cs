﻿namespace VSS.TRex.Types
{
    /// <summary>
    /// The type of coordinate system used to define the location written into TAG positioning values
    /// </summary>
    public enum CoordinateSystemType : byte
  {
        /// <summary>
        /// No coordinate system is defined (should never happen)
        /// </summary>
        NoCoordSystem,

        /// <summary>
        /// Coordinate System Information Block describing a project site calibration (see: DC, CAL & CFG files)
        /// </summary>
        CSIB,

        /// <summary>
        /// Automatic coordinate system. This is a system using the Universal Transverse Mercator projection zone the machine is located in
        /// </summary>
        ACS
    }
}
