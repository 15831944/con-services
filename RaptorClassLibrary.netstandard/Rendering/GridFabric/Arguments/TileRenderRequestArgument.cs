﻿using System;
using VSS.VisionLink.Raptor.Filters;
using VSS.VisionLink.Raptor.Geometry;
using VSS.VisionLink.Raptor.GridFabric.Arguments;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.Rendering.GridFabric.Arguments
{
    [Serializable]
    public class TileRenderRequestArgument : BaseApplicationServiceRequestArgument
    {
        /// <summary>
        /// The ID of the SiteModel to execute the request against
        /// </summary>
        public long SiteModelID { get; set; } = -1;

        public DisplayMode Mode { get; set; } = DisplayMode.Height;

        public BoundingWorldExtent3D Extents = BoundingWorldExtent3D.Inverted();

        public bool CoordsAreGrid { get; set; } = false;

        public ushort PixelsX { get; set; } = 256;
        public ushort PixelsY { get; set; } = 256;

        public CombinedFilter Filter1 { get; set; } = null;
        public CombinedFilter Filter2 { get; set; } = null;

        /// <summary>
        /// The identifier for the design held in the designs list ofr the project to be used to calculate cut/fill values
        /// </summary>
        public long CutFillDesignID { get; set; } = long.MinValue;
        //public DesignDescriptor CutFillDesign { get; set; }

        public TileRenderRequestArgument(long siteModelID,
                                         DisplayMode mode,
                                         BoundingWorldExtent3D extents,
                                         bool coordsAreGrid,
                                         ushort pixelsX,
                                         ushort pixelsY,
                                         CombinedFilter filter1,
                                         CombinedFilter filter2,
                                         long cutFillDesignID /*DesignDescriptor cutFillDesign*/)
        {
            SiteModelID = siteModelID;
            Mode = mode;
            Extents = extents;
            CoordsAreGrid = coordsAreGrid;
            PixelsX = pixelsX;
            PixelsY = pixelsY;
            Filter1 = filter1;
            Filter2 = filter2;
            CutFillDesignID = cutFillDesignID; // CutFillDesign = cutFillDesign;
        }
    }
}
