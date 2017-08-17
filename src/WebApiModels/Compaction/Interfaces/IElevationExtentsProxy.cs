﻿using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApiModels.Report.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.Compaction.Interfaces
{
  public interface IElevationExtentsProxy
  {
    ElevationStatisticsResult GetElevationRange(long projectId, Filter filter,
      CompactionProjectSettings projectSettings);
  }
}