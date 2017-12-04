﻿using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;

namespace VSS.Productivity3D.WebApi.Compaction.ActionServices
{
  /// <summary>
  /// Service helper for Volume Summary APIs.
  /// </summary>
  public interface IVolumeSummaryHelper
  {
    /// <summary>
    /// Get the Volumes Type for the given base and top surfaces.
    /// </summary>
    /// <param name="filter1">Filter to compare against filter2</param>
    /// <param name="filter2">Filter to compare against filter1</param>
    /// <returns>Returns the <see cref="RaptorConverters.VolumesType"/> type for the two input surfaces.</returns>
    RaptorConverters.VolumesType GetVolumesType(Filter filter1, Filter filter2);
  }
}