﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Filters;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.GridFabric.Arguments
{
    /// <summary>
    /// Contains all the parameters necessary to be sent for a generic subgrids request made to the compute cluster
    /// </summary>
    [Serializable]
    public class SubGridsRequestArgument : BaseApplicationServiceRequestArgument
    {
        /// <summary>
        /// The ID of the SiteModel to execute the request against
        /// </summary>
        public long SiteModelID = -1;

        /// <summary>
        /// The request ID for the subgrid request
        /// </summary>
        public long RequestID = -1;

        /// <summary>
        /// The grid data type to extract from the processed subgrids
        /// </summary>
        public GridDataType GridDataType { get; set; } = GridDataType.All;

        /// <summary>
        /// The serialised contents of the SubGridTreeSubGridExistenceBitMask that notes the address of all subgrids that need to be requested for production data
        /// </summary>
        public Byte[] ProdDataMaskBytes { get; set; } = null;

        /// <summary>
        /// The serialised contents of the SubGridTreeSubGridExistenceBitMask that notes the address of all subgrids that need to be requested for surveyed surface data ONLY
        /// </summary>
        public Byte[] SurveyedSurfaceOnlyMaskBytes { get; set; } = null;

        /// <summary>
        /// The set of filters to be applied to the requested subgrids
        /// </summary>
        /// 
        public FilterSet Filters { get; set; } = null;

        /// <summary>
        /// The name of the message topic that subgrid responses should be sent to
        /// </summary>
        public string MessageTopic { get; set; } = String.Empty;

        /// <summary>
        /// Denotes whether results of these requests should include any surveyed surfaces in the site model
        /// </summary>
        public bool IncludeSurveyedSurfaceInformation { get; set; } = false;

        /// <summary>
        /// The design to be used in cases of cut/fill subgrid requests
        /// </summary>
        public long CutFillDesignID { get; set; } = long.MinValue;

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public SubGridsRequestArgument()
        {
        }
    }
}
