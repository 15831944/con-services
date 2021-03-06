﻿using System;
using AutoMapper;
using CCSS.Productivity3D.Preferences.Abstractions.ResultsHandling;
using CCSS.Productivity3D.Preferences.Common.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.VisionLink.Interfaces.Events.Preference;
using PrefKeyDataModel = CCSS.Productivity3D.Preferences.Abstractions.Models.Database.PreferenceKey;
using UserPrefKeyDataModel = CCSS.Productivity3D.Preferences.Abstractions.Models.Database.UserPreferenceKey;


namespace CSS.Productivity3D.Preferences.Common.Utilities
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
          
          cfg.CreateMap<UserPrefKeyDataModel, UserPreferenceV1Result>()
            .ForMember(dest => dest.Code, opt => opt.MapFrom(x => ContractExecutionStatesEnum.ExecutedSuccessfully))
            .ForMember(dest => dest.Message, opt => opt.MapFrom(x => ContractExecutionResult.DefaultMessage))
            .ForMember(dest => dest.PreferenceKeyName, opt => opt.MapFrom(src => src.KeyName))
            .ForMember(dest => dest.PreferenceKeyUID, opt => opt.MapFrom(src => Guid.Parse(src.PreferenceKeyUID)))
            .ForMember(dest => dest.PreferenceJson, opt => opt.MapFrom(src => src.PreferenceJson))
            .ForMember(dest => dest.SchemaVersion, opt => opt.MapFrom(src => src.SchemaVersion));

          cfg.CreateMap<PrefKeyDataModel, PreferenceKeyV1Result>()
            .ForMember(dest => dest.Code, opt => opt.MapFrom(x => ContractExecutionStatesEnum.ExecutedSuccessfully))
            .ForMember(dest => dest.Message, opt => opt.MapFrom(x => ContractExecutionResult.DefaultMessage))
            .ForMember(dest => dest.PreferenceKeyName, opt => opt.MapFrom(src => src.KeyName))
            .ForMember(dest => dest.PreferenceKeyUID, opt => opt.MapFrom(src => Guid.Parse(src.PreferenceKeyUID)));

          cfg.CreateMap<UpsertUserPreferenceRequest, CreateUserPreferenceEvent>()
            .ForMember(dest => dest.UserUID, opt => opt.MapFrom(src => src.TargetUserUID))
            .ForMember(dest => dest.PreferenceKeyUID, opt => opt.MapFrom(src => src.PreferenceKeyUID))
            .ForMember(dest => dest.PreferenceKeyName, opt => opt.MapFrom(src => src.PreferenceKeyName))
            .ForMember(dest => dest.PreferenceJson, opt => opt.MapFrom(src => src.PreferenceJson))
            .ForMember(dest => dest.SchemaVersion, opt => opt.MapFrom(src => src.SchemaVersion))
            .ForMember(dest => dest.ActionUTC, opt => opt.Ignore());

          cfg.CreateMap<UpsertUserPreferenceRequest, UpdateUserPreferenceEvent>()
            .ForMember(dest => dest.UserUID, opt => opt.MapFrom(src => src.TargetUserUID))
            .ForMember(dest => dest.PreferenceKeyUID, opt => opt.MapFrom(src => src.PreferenceKeyUID))
            .ForMember(dest => dest.PreferenceKeyName, opt => opt.MapFrom(src => src.PreferenceKeyName))
            .ForMember(dest => dest.PreferenceJson, opt => opt.MapFrom(src => src.PreferenceJson))
            .ForMember(dest => dest.SchemaVersion, opt => opt.MapFrom(src => src.SchemaVersion))
            .ForMember(dest => dest.ActionUTC, opt => opt.Ignore());
        }
      );

      _automapper = _automapperConfiguration.CreateMapper();
    }
  }
}
