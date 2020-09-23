﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VSS.Productivity3D.Filter.Abstractions.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;

namespace VSS.Productivity3D.WebApiModels.Compaction.Interfaces
{
  public interface IElevationExtentsProxy
  {
    Task<ElevationStatisticsResult> GetElevationRange(long projectId, Guid projectUid, FilterResult filter, CompactionProjectSettings projectSettings, IHeaderDictionary customHeaders, string userId);
  }
}
