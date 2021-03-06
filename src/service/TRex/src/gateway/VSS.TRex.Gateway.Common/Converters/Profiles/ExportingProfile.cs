﻿using AutoMapper;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Exports.CSV.GridFabric;
using VSS.TRex.Gateway.Common.Requests;

namespace VSS.TRex.Gateway.Common.Converters.Profiles
{
  public class ExportingProfile : Profile
  {
    public ExportingProfile()
    {
      CreateMap<UserPreferences, CSVExportUserPreferences>()
        .ForMember(x => x.ProjectTimeZoneOffset,
          opt => opt.MapFrom(f => f.TimeZoneOffset));

      CreateMap<CompactionVetaExportRequest, CompactionCSVExportRequest>()
        .ForMember(x => x.RestrictOutputSize,
          opt => opt.MapFrom(src => false))
        .ForMember(x => x.RawDataAsDBase,
          opt => opt.MapFrom(src => false))
        .ForMember(x => x.Overrides,
          opt => opt.MapFrom(o => o.Overrides))
        .ForMember(x => x.LiftSettings,
          opt => opt.MapFrom(o => o.LiftSettings));

      CreateMap<CompactionPassCountExportRequest, CompactionCSVExportRequest>()
        .ForMember(x => x.MachineNames,
          opt => opt.MapFrom(src => new string[0]))
        .ForMember(x => x.Overrides,
          opt => opt.MapFrom(o => o.Overrides))
        .ForMember(x => x.LiftSettings,
          opt => opt.MapFrom(o => o.LiftSettings));

      CreateMap<CompactionCSVExportRequest, CSVExportRequestArgument>()
        .ForMember(x => x.ProjectID,
          opt => opt.MapFrom(f => f.ProjectUid))
        .ForMember(x => x.TRexNodeID,
          opt => opt.Ignore())
        .ForMember(x => x.OriginatingIgniteNodeId,
          opt => opt.Ignore())
        .ForMember(x => x.ExternalDescriptor,
          opt => opt.Ignore())
        .ForMember(x => x.Filters,
          opt => opt.Ignore())
        .ForMember(x => x.ReferenceDesign,
          opt => opt.Ignore())
        // MappedMachines are mapped separately using CSVExportHelper.MapRequestedMachines()
        .ForMember(x => x.MappedMachines,
          opt => opt.Ignore())
        .ForMember(x => x.Overrides,
          opt => opt.MapFrom(o => o.Overrides))
        .ForMember(x => x.LiftParams,
          opt => opt.MapFrom(o => o.LiftSettings))
        .ForMember(x => x.IsOutsideTPaaSTimeout, opt => opt.Ignore())
        .ForMember(x => x.RequestEmissionDateUtc, opt => opt.Ignore());
    }
  }
}
