﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.GridFabric.Caches
{
    /// <summary>
    /// Sptial grid cache provides logic to determine which of the spatial data grid caches an application should read data from
    /// depending on it settings in RaptorServerConfig
    /// </summary>
    public static class RaptorCaches
    {
        private const string kSpatialMutable = "Spatial-Mutable";
        private const string kSpatialImmutable = "Spatial-Immutable";
        private const string kSpatialImmutableCompressed = "Spatial-Immutable-Compressed";

        private const string kNonSpatialMutable = "NonSpatial-Mutable";
        private const string kNonSpatialImmutable = "NonSpatial-Immutable";
        private const string kNonSpatialImmutableCompressed = "NonSpatial-Immutable"; // Same as compressed as there is currently no distinction

        /// <summary>
        /// Returns the name of the spatial grid cache to use to locate cell and cell pass information
        /// </summary>
        public static String MutableSpatialCacheName() => kSpatialMutable;
/*
         {
            if (RaptorServerConfig.Instance().UseMutableSpatialData)
            {
                return kSpatialMutable;
            }

            if (RaptorServerConfig.Instance().CompressImmutableSpatialData)
            {
                return kSpatialImmutableCompressed;
            }

            return kSpatialImmutable;
        }
*/
        /// <summary>
        /// Returns the name of the spatial grid cache to use to store mutable cell and cell pass information
        /// </summary>
        /// <returns></returns>
        public static string ImmutableSpatialCacheName()
        {
            if (RaptorServerConfig.Instance().CompressImmutableSpatialData)
            {
                return kSpatialImmutableCompressed;
            }

            return kSpatialImmutable;
        }

        /// <summary>
        /// Returns the name of the event grid cache to use to locate machine event and other non spatial information
        /// </summary>
        public static String MutableNonSpatialCacheName() => kNonSpatialMutable;
/*
         {
            if (RaptorServerConfig.Instance().UseMutableNonSpatialData)
            {
                return kNonSpatialMutable;
            }

            if (RaptorServerConfig.Instance().CompressImmutableNonSpatialData)
            {
                return kNonSpatialImmutableCompressed;
            }

            return kNonSpatialImmutable;
        }
*/

        /// <summary>
        /// Returns the name of the spatial grid cache to use to store immutable cell and cell pass information
        /// </summary>
        /// <returns></returns>
        public static string ImmutableNonSpatialCacheName()
        {
            if (RaptorServerConfig.Instance().CompressImmutableNonSpatialData)
            {
                return kNonSpatialImmutableCompressed;
            }

            return kNonSpatialImmutable;
        }
    }
}
