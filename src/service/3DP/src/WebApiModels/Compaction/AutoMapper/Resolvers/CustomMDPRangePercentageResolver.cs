﻿using AutoMapper;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;

namespace VSS.Productivity3D.WebApi.Models.Compaction.AutoMapper
{
  public partial class AutoMapperUtility
  {
    public class CustomMDPRangePercentageResolver : IValueResolver<CompactionProjectSettings, LiftBuildSettings, MDPRangePercentage>
    {
      public MDPRangePercentage Resolve(CompactionProjectSettings src, LiftBuildSettings dst, MDPRangePercentage member, ResolutionContext context)
      {
        return src.OverrideDefaultTargetRangeMdpPercent ? 
          new MDPRangePercentage(src.CustomTargetMdpPercentMinimum, src.CustomTargetMdpPercentMaximum) :
          new MDPRangePercentage(CompactionProjectSettings.DefaultSettings.CustomTargetMdpPercentMinimum, CompactionProjectSettings.DefaultSettings.CustomTargetMdpPercentMaximum);
      }
    }
  }
}
