﻿using System;
using System.Linq;
using System.Net;
#if RAPTOR
using ASNode.ExportProductionDataCSV.RPC;
using ASNode.UserPreferences;
using BoundingExtents;
using VLPDDecls;
#endif
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Models.Report.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Helpers
{
  /// <summary>
  /// The request representation for a linear or alignment based profile request for all thematic types other than summary volumes.
  /// Model represents a production data profile
  /// </summary>
  public class ExportRequestHelper : DataRequestBase, IExportRequestHandler
  {
#if RAPTOR
    private IASNodeClient raptorClient;
#endif
    private UserPreferenceData userPreferences;
    private ProjectData projectDescriptor;

    /// <summary>
    /// Parameter-less constructor is required to support factory create function in <see cref="WebApi"/> project.
    /// </summary>
    public ExportRequestHelper()
    { }

    public ExportRequestHelper(ILoggerFactory logger, IConfigurationStore configurationStore, IFileListProxy fileListProxy, ICompactionSettingsManager settingsManager)
    {
      Log = logger.CreateLogger<ProductionDataProfileRequestHelper>();
      ConfigurationStore = configurationStore;
      FileListProxy = fileListProxy;
      SettingsManager = settingsManager;
    }
#if RAPTOR
    public ExportRequestHelper SetRaptorClient(IASNodeClient raptorClient)
    {
      this.raptorClient = raptorClient;
      return this;
    }
#endif
    public ExportRequestHelper SetUserPreferences(UserPreferenceData userPrefs)
    {
      userPreferences = userPrefs;
      return this;
    }

    public UserPreferenceData GetUserPreferences()
    {
      return userPreferences;
    }

    public ExportRequestHelper SetProjectDescriptor(ProjectData projectDescriptor)
    {
      this.projectDescriptor = projectDescriptor;
      return this;
    }

    /// <summary>
    /// Creates an instance of the ProfileProductionDataRequest class and populate it with data needed for a design profile.   
    /// </summary>
    public ExportReport CreateExportRequest(
      DateTime? startUtc,
      DateTime? endUtc,
      CoordType coordType,
      ExportTypes exportType,
      string fileName,
      bool restrictSize,
      bool rawData,
      OutputTypes outputType,
      string machineNameString,
      double tolerance = 0.0)
    {
#if !RAPTOR
      throw new ServiceException(HttpStatusCode.BadRequest,
        new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "TRex unsupported request"));
#else
      var liftSettings = SettingsManager.CompactionLiftBuildSettings(ProjectSettings);

      T3DBoundingWorldExtent projectExtents = new T3DBoundingWorldExtent();
      TMachine[] machineList = null;
      string[] machineNames = null;

      if (exportType == ExportTypes.SurfaceExport)
      {
        raptorClient.GetDataModelExtents(ProjectId,
          RaptorConverters.convertSurveyedSurfaceExlusionList(Filter?.SurveyedSurfaceExclusionList), out projectExtents);
      }
      else if (exportType == ExportTypes.VedaExport)
      {
        if (!string.IsNullOrEmpty(machineNameString) && machineNameString != "All")
        {
          machineNames = machineNameString.Split(',');
        }
#if RAPTOR
          TMachineDetail[] machineDetails = raptorClient.GetMachineIDs(ProjectId);

        if (machineDetails != null)
        {
          if (!string.IsNullOrEmpty(machineNameString) && machineNameString != "All")
          {
            machineDetails = machineDetails.Where(machineDetail => machineNames.Contains(machineDetail.Name)).ToArray();
          }

          machineList = machineDetails.Select(m => new TMachine { AssetID = m.ID, MachineName = m.Name, SerialNo = "" }).ToArray();
        }
#endif
      }

      if (!string.IsNullOrEmpty(fileName))
      {
        fileName = StripInvalidCharacters(fileName);
      }

      return new ExportReport(
        ProjectId,
        ProjectUid,
        liftSettings,
        Filter,
        -1,
        null,
        false,
        null,
        coordType,
        startUtc ?? DateTime.MinValue,
        endUtc ?? DateTime.MinValue,
        tolerance,
        false,
        restrictSize,
        rawData,
        RaptorConverters.convertProjectExtents(projectExtents),
        false,
        outputType,
        RaptorConverters.convertMachines(machineList),
        exportType == ExportTypes.SurfaceExport,
        fileName,
        exportType,
        ConvertUserPreferences(userPreferences, projectDescriptor.ProjectTimeZone),
        machineNames);
#endif
    }

#if RAPTOR
    private static string StripInvalidCharacters(string str)
    {
      // Remove all invalid characters except of the underscore...
      str = System.Text.RegularExpressions.Regex.Replace(str, @"[^A-Za-z0-9\s-\w\/_]", "");

      // Convert multiple spaces into one space...
      str = System.Text.RegularExpressions.Regex.Replace(str, @"\s+", " ").Trim();

      // Replace spaces with undescore characters...
      str = System.Text.RegularExpressions.Regex.Replace(str, @"\s", "_");

      return str;
    }

    // TODO (Aaron) move to RaptopHelper
    /// <summary>
    /// Converts a set user preferences in the format understood by Raptor.
    /// It is solely used by production data export WebAPIs.
    /// </summary>
    /// <param name="userPref">The set of user preferences.</param>
    /// <param name="projectTimezone">The project time zone.</param>
    /// <returns>The set of user preferences in Raptor's format</returns>
    public static TASNodeUserPreferences ConvertToRaptorUserPreferences(UserPreferenceData userPref, string projectTimezone)
    {
      var timezone = projectTimezone ?? userPref.Timezone;
      TimeZoneInfo projectTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timezone);
      double projectTimeZoneOffset = projectTimeZone?.GetUtcOffset(DateTime.Now).TotalHours ?? 0;

      var languageIndex = Array.FindIndex(LanguageLocales.LanguageLocaleStrings, s => s.Equals(userPref.Language, StringComparison.OrdinalIgnoreCase));
      
      if (languageIndex == -1)
      {
        languageIndex = (int)LanguageEnum.enUS;
      }

      return ASNode.UserPreferences.__Global.Construct_TASNodeUserPreferences(
        timezone,
        Preferences.DefaultDateSeparator,
        Preferences.DefaultTimeSeparator,
        //Hardwire number format as "xxx,xxx.xx" or it causes problems with the CSV file as comma is the column separator.
        //To respect user preferences requires Raptor to enclose formatted numbers in quotes.
        //This bug is present in CG since it uses user preferences separators.
        Preferences.DefaultThousandsSeparator,
        Preferences.DefaultDecimalSeparator,
        projectTimeZoneOffset,
        languageIndex,
        (int)userPref.Units.UnitsType(),
        Preferences.DefaultDateTimeFormat,
        Preferences.DefaultNumberFormat,
        (int)userPref.TemperatureUnit.TemperatureUnitType(),
        Preferences.DefaultAssetLabelTypeId);
    }

    /// <summary>
    /// Converts a set user preferences in the common format.
    /// It is solely used by production data export WebAPIs.
    /// </summary>
    /// <param name="userPref">The set of user preferences.</param>
    /// <param name="projectTimezone">The project time zone.</param>
    /// <returns>The set of user preferences in Raptor's format</returns>
    public static UserPreferences ConvertUserPreferences(UserPreferenceData userPref, string projectTimezone)
    {
      var timezone = projectTimezone ?? userPref.Timezone;
      TimeZoneInfo projectTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timezone);
      double projectTimeZoneOffset = projectTimeZone?.GetUtcOffset(DateTime.Now).TotalHours ?? 0;

      var languageIndex = Array.FindIndex(LanguageLocales.LanguageLocaleStrings, s => s.Equals(userPref.Language, StringComparison.OrdinalIgnoreCase));

      if (languageIndex == -1)
      {
        languageIndex = (int)LanguageEnum.enUS;
      }

      return new UserPreferences(
        timezone,
        Preferences.DefaultDateSeparator,
        Preferences.DefaultTimeSeparator,
        //Hardwire number format as "xxx,xxx.xx" or it causes problems with the CSV file as comma is the column separator.
        //To respect user preferences requires Raptor to enclose formatted numbers in quotes.
        //This bug is present in CG since it uses user preferences separators.
        Preferences.DefaultThousandsSeparator,
        Preferences.DefaultDecimalSeparator,
        projectTimeZoneOffset,
        languageIndex,
        (int)userPref.Units.UnitsType(),
        Preferences.DefaultDateTimeFormat,
        Preferences.DefaultNumberFormat,
        (int)userPref.TemperatureUnit.TemperatureUnitType(),
        Preferences.DefaultAssetLabelTypeId);
    }

#endif
  }
}
