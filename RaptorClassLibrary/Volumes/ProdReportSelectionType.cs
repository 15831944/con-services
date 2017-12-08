﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.Volumes
{
    /// <summary>
    /// ProdReportSelectionType denotes whether the 'from' or 'to' surface used for the
    /// volumes productivity calculations is a surface (a deisgn loaded into SVO),
    /// a filter (production filter),or a time range (antime interval with respect to
    /// a filter date stamp or arbitrary time.
    /// </summary>
    public enum ProdReportSelectionType
    {
        None,
        Surface,
        Filter
    }

    /// <summary>
    /// Indicates a calculation direction between one production data surface and another for volumes
    /// </summary>
    public enum ProdReportFromTo
    { 
       From,
       To
    }
}
