﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Common;
using VSS.VisionLink.Raptor.Designs;
using VSS.VisionLink.Raptor.Filters;
using VSS.VisionLink.Raptor.GridFabric.Arguments;

namespace VSS.VisionLink.Raptor.Volumes.GridFabric.Arguments
{
    public class SimpleVolumesRequestArgument : BaseApplicationServiceRequestArgument
    {
        public long SiteModelID = -1;

        //ExternalDescriptor : TASNodeRequestDescriptor;

        /// <summary>
        /// The volume computation method to use when calculating volume information
        /// </summary>
        public VolumeComputationType VolumeType = VolumeComputationType.None;

        // FLiftBuildSettings : TICLiftBuildSettings;

        /// <summary>
        /// BaseFilter and TopFilter reference two sets of filter settings
        /// between which we may calculate volumes. At the current time, it is
        /// meaingful for a filter to have a spatial extent, and to denote aa
        /// 'as-at' time only.
        /// </summary>
        public CombinedFilter BaseFilter = null;
        public CombinedFilter TopFilter = null;

        // public DesignDescriptor BaseDesign = DesignDescriptor.Null();
        // public DesignDescriptor TopDesign = DesignDescriptor.Null();

        public long BaseDesignID = long.MinValue;
        public long TopDesignID = long.MinValue;
        
        /// <summary>
        /// BaseSpatialFilter, TopSpatialFilter and AdditionalSpatialFilter are
        /// three filters used to implement the spatial restrictions represented by
        /// spatial restrictions in the base and top filters and the additional
        /// boundary specified by the user.
        /// </summary>
        public CombinedFilter AdditionalSpatialFilter = null;

        /// <summary>
        /// CutTolerance determines the tolerance (in meters) that the 'From' surface
        /// needs to be above the 'To' surface before the two surfaces are not
        /// considered to be equivalent, or 'on-grade', and hence there is material still remaining to
        /// be cut
        /// </summary>
        public double CutTolerance = VolumesConsts.DEFAULT_CELL_VOLUME_CUT_TOLERANCE;

        /// <summary>
        /// FillTolerance determines the tolerance (in meters) that the 'To' surface
        /// needs to be above the 'From' surface before the two surfaces are not
        /// considered to be equivalent, or 'on-grade', and hence there is material still remaining to
        /// be filled
        /// </summary>
        public double FillTolerance = VolumesConsts.DEFAULT_CELL_VOLUME_FILL_TOLERANCE;

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public SimpleVolumesRequestArgument()
        {
        }
    }
}
