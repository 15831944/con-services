﻿using System;
using AutoMapper;
using CCSS.Geometry;
using VSS.Common.Abstractions.Clients.CWS;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.Cws;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;
using ProjectDatabaseModel = VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels.Project;

namespace VSS.MasterData.Project.WebAPI.Common.Utilities
{
  public class AutoMapperUtility
  {
    private static MapperConfiguration _automapperConfiguration;

    public static MapperConfiguration AutomapperConfiguration
    {
      get
      {
        if (_automapperConfiguration == null)
        {
          ConfigureAutomapper();
        }

        return _automapperConfiguration;
      }
    }

    private static IMapper _automapper;

    public static IMapper Automapper
    {
      get
      {
        if (_automapperConfiguration == null)
        {
          ConfigureAutomapper();
        }

        return _automapper;
      }
    }


    public static void ConfigureAutomapper()
    {
      _automapperConfiguration = new MapperConfiguration(
        //define mappings <source type, destination type>
        cfg =>
        {
          cfg.AllowNullCollections = true; // so that byte[] can be null
          cfg.CreateMap<CreateProjectRequest, CreateProjectEvent>()
            .ForMember(dest => dest.ProjectUID, opt => opt.Ignore()) // will come from cws
            .ForMember(dest => dest.CustomerUID, opt => opt.MapFrom(src => src.CustomerUID))
            .ForMember(dest => dest.ActionUTC, opt => opt.Ignore())
            .ForMember(dest => dest.ShortRaptorProjectId, opt => opt.Ignore());
          cfg.CreateMap<CreateProjectEvent, ProjectDatabaseModel>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.ProjectName))
            .ForMember(dest => dest.Boundary, opt => opt.MapFrom(src => src.ProjectBoundary))
            .ForMember(dest => dest.ShortRaptorProjectId, opt => opt.MapFrom(src => src.ShortRaptorProjectId))
            .ForMember(dest => dest.CustomerUID, opt => opt.MapFrom(src => src.CustomerUID))
            .ForMember(dest => dest.UserProjectRole, opt => opt.MapFrom(x => UserProjectRoleEnum.Unknown))
            .ForMember(dest => dest.ProjectTimeZoneIana, opt => opt.Ignore())
            .ForMember(dest => dest.CoordinateSystemLastActionedUTC, opt => opt.Ignore())
            .ForMember(dest => dest.IsArchived, opt => opt.Ignore())
            .ForMember(dest => dest.LastActionedUTC, opt => opt.Ignore());
          cfg.CreateMap<UpdateProjectRequest, UpdateProjectEvent>()
            .ForMember(dest => dest.ActionUTC, opt => opt.Ignore())
            .ForMember(dest => dest.ProjectTimezone, opt => opt.Ignore());
          cfg.CreateMap<ProjectDatabaseModel, ProjectV6Descriptor>()
            .ForMember(dest => dest.ProjectGeofenceWKT, opt => opt.MapFrom(src => src.Boundary))
            .ForMember(dest => dest.IanaTimeZone, opt => opt.MapFrom(src => src.ProjectTimeZoneIana))
            .ForMember(dest => dest.ShortRaptorProjectId, opt => opt.MapFrom(src => src.ShortRaptorProjectId))
            .ForMember(dest => dest.IsArchived, opt => opt.MapFrom(src => src.IsArchived));
          cfg.CreateMap<ImportedFile, ImportedFileDescriptor>()
            .ForMember(dest => dest.ImportedUtc, opt => opt.MapFrom(src => src.LastActionedUtc))
            .ForMember(dest => dest.LegacyFileId, opt => opt.MapFrom(src => src.ImportedFileId))
            .ForMember(dest => dest.ImportedFileHistory, opt => opt.MapFrom(src => src.ImportedFileHistory.ImportedFileHistoryItems))
            .ForMember(dest => dest.IsActivated, opt => opt.MapFrom(x => true));
          cfg.CreateMap<Productivity3D.Project.Abstractions.Models.DatabaseModels.ImportedFileHistoryItem, MasterData.Project.WebAPI.Common.Models.ImportedFileHistoryItem>()
            .ForMember(dest => dest.FileCreatedUtc, opt => opt.MapFrom(src => src.FileCreatedUtc))
            .ForMember(dest => dest.FileUpdatedUtc, opt => opt.MapFrom(src => src.FileUpdatedUtc));
          cfg.CreateMap<ImportedFile, UpdateImportedFileEvent>()
            .ForMember(dest => dest.ImportedFileUID, opt => opt.MapFrom(src => Guid.Parse(src.ImportedFileUid)))
            .ForMember(dest => dest.ProjectUID, opt => opt.MapFrom(src => Guid.Parse(src.ProjectUid)))
            .ForMember(dest => dest.ActionUTC, opt => opt.MapFrom(src => src.LastActionedUtc));

          // for v5 BC apis
          cfg.CreateMap<CreateProjectV5Request, CreateProjectEvent>()
            .ForMember(dest => dest.CustomerUID, opt => opt.Ignore()) // done externally
            .ForMember(dest => dest.ProjectBoundary, opt => opt.Ignore()) // done externally
            .ForMember(dest => dest.CoordinateSystemFileName, opt => opt.MapFrom((src => src.CoordinateSystem.Name)))
            .ForMember(dest => dest.CoordinateSystemFileContent, opt => opt.Ignore()) // done externally
            .ForMember(dest => dest.ActionUTC, opt => opt.MapFrom(x => DateTime.UtcNow))
            .ForMember(dest => dest.ShortRaptorProjectId, opt => opt.MapFrom(x => 0))
            .ForMember(dest => dest.ProjectUID, opt => opt.Ignore());
          cfg.CreateMap<TBCPoint, VSS.MasterData.Models.Models.Point>()
            .ForMember(dest => dest.y, opt => opt.MapFrom((src => src.Latitude)))
            .ForMember(dest => dest.x, opt => opt.MapFrom((src => src.Longitude)));
          // ProjectGeofenceAssociations
          cfg.CreateMap<GeofenceWithAssociation, GeofenceV4Descriptor>();

          // cws clients
          cfg.CreateMap<CreateProjectEvent, CreateProjectRequestModel>()
            .ForMember(dest => dest.AccountId, opt => opt.MapFrom(src => src.CustomerUID))
            .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.ProjectName))
            .ForMember(dest => dest.Timezone, opt => opt.MapFrom(src => src.ProjectTimezone))
            .ForMember(dest => dest.Boundary, opt => opt.Ignore()) // done externally
            .ForMember(dest => dest.TRN, opt => opt.Ignore())
            ;
          cfg.CreateMap<UpdateProjectEvent, CreateProjectRequestModel>()
            .ForMember(dest => dest.AccountId, opt => opt.Ignore())
            .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.ProjectName))
            .ForMember(dest => dest.Timezone, opt => opt.MapFrom(src => src.ProjectTimezone)) // not sure if we can update timezone
            .ForMember(dest => dest.Boundary, opt => opt.Ignore()) // done externally
            .ForMember(dest => dest.TRN, opt => opt.Ignore())
            ;

          cfg.CreateMap<AccountResponseModel, CustomerData>()
            .ForMember(dest => dest.uid, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.type, opt => opt.MapFrom(c => CustomerType.Customer.ToString()))
            ;
          cfg.CreateMap<AccountResponseModel, AccountHierarchyCustomer>()
            .ForMember(dest => dest.CustomerUid, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.CustomerCode, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.CustomerType, opt => opt.MapFrom(src => "Customer"))
            .ForMember(dest => dest.Children, opt => opt.Ignore());

          cfg.CreateMap<ProjectResponseModel, ProjectData>()
            .ForMember(dest => dest.ProjectUID, opt => opt.MapFrom(src => src.ProjectId))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.ProjectName))
            .ForMember(dest => dest.CustomerUID, opt => opt.MapFrom(src => src.AccountId))
            .ForMember(dest => dest.ProjectGeofenceWKT, opt => opt.MapFrom(src => GeometryConversion.ProjectBoundaryToWKT(src.Boundary)))
            .ForMember(dest => dest.IanaTimeZone, opt => opt.MapFrom(src => src.Timezone))
            .ForMember(dest => dest.CoordinateSystemFileName, opt => opt.Ignore())
            .ForMember(dest => dest.CoordinateSystemLastActionedUTC, opt => opt.Ignore())
            .ForMember(dest => dest.ProjectTimeZone, opt => opt.Ignore())
            .ForMember(dest => dest.ShortRaptorProjectId, opt => opt.Ignore())
            .ForMember(dest => dest.ProjectType, opt => opt.Ignore())
            .ForMember(dest => dest.IsArchived, opt => opt.Ignore())
            ;

          cfg.CreateMap<ProjectValidateDto, ProjectValidation>()
            .ForMember(dest => dest.CustomerUid, opt => opt.MapFrom(src => TRNHelper.ExtractGuid(src.AccountTrn)))
            .ForMember(dest => dest.ProjectUid, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.ProjectTrn) ? null : TRNHelper.ExtractGuid(src.ProjectTrn)))
            .ForMember(dest => dest.ProjectType, opt => opt.MapFrom(src => src.ProjectType))
            .ForMember(dest => dest.UpdateType, opt => opt.MapFrom(src => src.UpdateType))
            .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.ProjectName))
            .ForMember(dest => dest.ProjectBoundaryWKT, opt => opt.MapFrom(src => GeometryConversion.ProjectBoundaryToWKT(src.Boundary)))
            ;

          cfg.CreateMap<CreateProjectRequest, ProjectValidation>()
            .ForMember(dest => dest.CustomerUid, opt => opt.MapFrom(src => src.CustomerUID))
            .ForMember(dest => dest.ProjectUid, opt => opt.MapFrom(src => Guid.Empty))
            .ForMember(dest => dest.ProjectType, opt => opt.MapFrom(src => src.ProjectType == ProjectType.Standard ? CwsProjectType.AcceptsTagFiles : (CwsProjectType?)null))
            .ForMember(dest => dest.UpdateType, opt => opt.MapFrom(src => ProjectUpdateType.Created))
            .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.ProjectName))
            .ForMember(dest => dest.ProjectBoundaryWKT, opt => opt.MapFrom(src => src.ProjectBoundary))
            ;

          cfg.CreateMap<CreateProjectEvent, ProjectValidation>()
            .ForMember(dest => dest.CustomerUid, opt => opt.MapFrom(src => src.CustomerUID))
            .ForMember(dest => dest.ProjectUid, opt => opt.MapFrom(src => Guid.Empty))
            .ForMember(dest => dest.ProjectType, opt => opt.MapFrom(src => src.ProjectType == ProjectType.Standard ? CwsProjectType.AcceptsTagFiles : (CwsProjectType?)null))
            .ForMember(dest => dest.UpdateType, opt => opt.MapFrom(src => ProjectUpdateType.Created))
            .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.ProjectName))
            .ForMember(dest => dest.ProjectBoundaryWKT, opt => opt.MapFrom(src => src.ProjectBoundary))
            ;

          cfg.CreateMap<UpdateProjectRequest, ProjectValidation>()
            .ForMember(dest => dest.CustomerUid, opt => opt.MapFrom(src => Guid.Empty))
            .ForMember(dest => dest.ProjectUid, opt => opt.MapFrom(src => src.ProjectUid))
            .ForMember(dest => dest.ProjectType, opt => opt.MapFrom(src => src.ProjectType == ProjectType.Standard ? CwsProjectType.AcceptsTagFiles : (CwsProjectType?)null))
            .ForMember(dest => dest.UpdateType, opt => opt.MapFrom(src => ProjectUpdateType.Updated))
            .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.ProjectName))
            .ForMember(dest => dest.ProjectBoundaryWKT, opt => opt.MapFrom(src => src.ProjectBoundary))
            ;

          cfg.CreateMap<DeleteProjectEvent, ProjectValidation>()
            .ForMember(dest => dest.CustomerUid, opt => opt.MapFrom(src => Guid.Empty))
            .ForMember(dest => dest.ProjectUid, opt => opt.MapFrom(src => src.ProjectUID))
            .ForMember(dest => dest.ProjectType, opt => opt.Ignore())
            .ForMember(dest => dest.UpdateType, opt => opt.MapFrom(src => ProjectUpdateType.Deleted))
            .ForMember(dest => dest.ProjectName, opt => opt.Ignore())
            .ForMember(dest => dest.ProjectBoundaryWKT, opt => opt.Ignore())
            ;

        }
      );

      _automapper = _automapperConfiguration.CreateMapper();
    }
  }
}
