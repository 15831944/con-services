﻿using AutoMapper;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling.Designs;
using VSS.TRex.Alignments;
using VSS.TRex.Designs.Storage;
using VSS.TRex.Geometry;
using VSS.TRex.SurveyedSurfaces;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.TRex.Gateway.Common.Converters.Profiles
{
  public class DesignResultProfile : Profile
  {
    public DesignResultProfile()
    {
      CreateMap<BoundingWorldExtent3D, BoundingExtents3D>()
        .ForMember(x => x.MinX,
          opt => opt.MapFrom(f => f.MinX))
        .ForMember(x => x.MinY,
          opt => opt.MapFrom(f => f.MinY))
        .ForMember(x => x.MinZ,
          opt => opt.MapFrom(f => f.MinZ))
        .ForMember(x => x.MaxX,
          opt => opt.MapFrom(f => f.MaxX))
        .ForMember(x => x.MaxY,
          opt => opt.MapFrom(f => f.MaxY))
        .ForMember(x => x.MaxZ,
          opt => opt.MapFrom(f => f.MaxZ));

      CreateMap<Design, DesignFileDescriptor>()
        .ForMember(x => x.FileType,
          opt => opt.MapFrom(src => ImportedFileType.DesignSurface))
        .ForMember(x => x.Name,
          opt => opt.MapFrom(f => f.DesignDescriptor.FileName))
        .ForMember(x => x.DesignUid,
          opt => opt.MapFrom(f => f.ID))
        .ForMember(x => x.Extents,
          opt => opt.MapFrom(f => f.Extents))
        .ForMember(x => x.SurveyedUtc,
          opt => opt.Ignore());

      CreateMap<SurveyedSurfaces.SurveyedSurface, DesignFileDescriptor>()
        .ForMember(x => x.FileType,
          opt => opt.MapFrom(src => ImportedFileType.SurveyedSurface))
        .ForMember(x => x.Name,
          opt => opt.MapFrom(f => f.DesignDescriptor.FileName))
        .ForMember(x => x.DesignUid,
          opt => opt.MapFrom(f => f.ID))
        .ForMember(x => x.Extents,
          opt => opt.MapFrom(f => f.Extents))
        .ForMember(x => x.SurveyedUtc,
          opt => opt.MapFrom(f => f.AsAtDate));

      CreateMap<Alignment, DesignFileDescriptor>()
        .ForMember(x => x.FileType,
          opt => opt.MapFrom(src => ImportedFileType.Alignment))
        .ForMember(x => x.Name,
          opt => opt.MapFrom(f => f.DesignDescriptor.FileName))
        .ForMember(x => x.DesignUid,
          opt => opt.MapFrom(f => f.ID))
        .ForMember(x => x.Extents,
          opt => opt.MapFrom(f => f.Extents))
        .ForMember(x => x.SurveyedUtc,
          opt => opt.Ignore());
    }
  }
}
