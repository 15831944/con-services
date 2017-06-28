﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor
{
    /// <summary>
    /// Raptor server config is intended to collect and represent configuration presented to this server instance
    /// in particular, such as its spatial subdivision role or whether it handles mutable or immutable spatial data 
    /// (ie: read-write or read-only contexts)
    /// </summary>
    public class RaptorServerConfig
    {
        private static RaptorServerConfig instance = null;

        public static RaptorServerConfig Instance()
        {
            if (instance == null)
            {
                string[] args = Environment.CommandLine.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                instance = new RaptorServerConfig();
                instance.SpatialSubdivisionDescriptor = args.Where(x => x.Contains("SpatialDivision=")).Select(x => x.Split(new String[] { "=" }, StringSplitOptions.RemoveEmptyEntries)[1]).Select(x => Convert.ToUInt16(x)).FirstOrDefault();
            }

            return instance;
        }

        public RaptorServerConfig()
        {
            // Pick up the parameters from command line or other sources...
        }

        /// <summary>
        /// SpatialSubdivisionDescriptor records which division of the spatial data in the system this node instance is responsible
        /// for serving requests against.
        /// </summary>
        public uint SpatialSubdivisionDescriptor { get; set; } = 0;

        /// <summary>
        /// UseMutableCellPassSegments controls whether the subgrid segments containing cell passes use a mutable structure 
        /// that permits addition/removal of cell passes (eg: in the context of processing in-bound TAG files and other 
        /// changes), or an immutable structure that favours memory allocation efficiency given read-only operations
        /// </summary>
        public bool UseMutableSpatialData { get; set; } = true;

        /// <summary>
        /// Defines whether spatial data (eg: cell pass sets for subgrid segments) should be compressed
        /// </summary>
        public bool CompressImmutableSpatialData { get; set; } = true;

        /// <summary>
        /// UseMutableSpatialData controls whether the event list and other non-spatial information in a datamodel
        /// use a mutable structure that permits addition/removal of non-spatial information or an immutable structure 
        /// that favours memory allocation efficiency given read-only operations
        /// </summary>
        public bool UseMutableNonSpatialData { get; set; } = true;

        /// <summary>
        /// Defines whether non spatial data should be compressed in it's immutable form
        /// </summary>
        public bool CompressImmutableNonSpatialData { get; set; } = true;
    }
}
