﻿using AutoMapper;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.AutoMapper.Profiles
{
  public class CmvPercentChangeSettingsProfile : Profile
  {
    public CmvPercentChangeSettingsProfile()
    {
      CreateMap<CompactionProjectSettings, CmvPercentChangeSettings>()
        .ForMember(x => x.percents,
          opt => opt.MapFrom(ps => ps.CmvPercentChange));
    }
  }
}
