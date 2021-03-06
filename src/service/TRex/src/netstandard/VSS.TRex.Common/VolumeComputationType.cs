﻿namespace VSS.TRex.Common
{
    /// <summary>
    /// Denotes the different types of volume computations that may be performed
    /// </summary>
    public enum VolumeComputationType : byte
  {
        /// <summary>
        /// No volume computation type (null)
        /// </summary>
        None,

        /// <summary>
        /// Calculate volumes above a base datum level
        /// </summary>
        AboveLevel,

        /// <summary>
        /// Calculate volumes between two datum levels
        /// </summary>
        Between2Levels,

        /// <summary>
        /// Calculate the volume above a defined filter
        /// </summary>
        AboveFilter,

        /// <summary>
        /// Calculate the volume between two filters
        /// </summary>
        Between2Filters,

        /// <summary>
        /// Calculate the volume from a production data filter to a design surface
        /// </summary>
        BetweenFilterAndDesign,

        /// <summary>
        /// Calculate the volume from a design surface to a production data filter
        /// </summary>
        BetweenDesignAndFilter
    }
}
