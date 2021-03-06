﻿using VSS.TRex.Common.Interfaces;

namespace VSS.TRex.Filters.Interfaces
{
  public interface ICombinedFilter : IFromToBinary
  {
    /// <summary>
    /// The filter responsible for selection of cell passes based on attribute filtering criteria related to cell passes
    /// </summary>
    ICellPassAttributeFilter AttributeFilter { get; set; }

    /// <summary>
    /// The filter responsible for selection of cells based on spatial filtering criteria related to cell location
    /// </summary>
    ICellSpatialFilter SpatialFilter { get; set; }
  }
}
