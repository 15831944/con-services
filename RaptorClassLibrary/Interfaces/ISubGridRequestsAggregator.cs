﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;

namespace VSS.VisionLink.Raptor.Interfaces
{
    /// <summary>
    /// Interface supporting SubGridRequestors performing aggregative processing of a set of subgrids in a request
    /// </summary>
    public interface ISubGridRequestsAggregator
    {
        /// <summary>
        /// Process the result of querying a subgrid against one or more filters. The argument is a generic lsit of client subgrids
        /// </summary>
        /// <param name="subGrids"></param>
        void ProcessSubgridResult(IClientLeafSubGrid[][] subGrids);

        /// <summary>
        /// Perform any finalisation logic required once all subgrids have been processed into the aggregator
        /// </summary>
        void Finalise();
    }
}
