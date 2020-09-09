﻿using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Filter.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Helpers
{
  /// <summary>
  /// The request representation for getting production data from Raptor for a grid report.
  /// </summary>
  public class CompactionReportGridRequestHelper : DataRequestBase, ICompactionReportGridRequestHelper
  {
    /// <summary>
    /// Parameter-less constructor is required to support factory create function in <see cref="WebApi"/> project.
    /// </summary>
    public CompactionReportGridRequestHelper()
    { }

    public CompactionReportGridRequestHelper(ILoggerFactory logger, IConfigurationStore configurationStore,
      IFileImportProxy fileImportProxy, ICompactionSettingsManager settingsManager)
    {
      Log = logger.CreateLogger<ProductionDataProfileRequestHelper>();
      ConfigurationStore = configurationStore;
      FileImportProxy = fileImportProxy;
      SettingsManager = settingsManager;
    }

    public CompactionReportGridRequest CreateCompactionReportGridRequest(
      bool reportElevation,
      bool reportCmv,
      bool reportMdp,
      bool reportPassCount,
      bool reportTemperature,
      bool reportCutFill,
      DesignDescriptor designFile,
      double? gridInerval,
      GridReportOption gridReportOption,
      double startNorthing,
      double startEasting,
      double endNorthing,
      double endEasting,
      double azimuth)
    {
      var liftBuildSettings = SettingsManager.CompactionLiftBuildSettings(ProjectSettings);

      return CompactionReportGridRequest.CreateCompactionReportGridRequest(
        ProjectId,
        ProjectUid,
        Filter,
        Filter != null ? Filter.Id ?? -1 : -1,
        liftBuildSettings,
        reportElevation,
        reportCmv,
        reportMdp,
        reportPassCount,
        reportTemperature,
        reportCutFill,
        designFile,
        gridInerval,
        gridReportOption,
        startNorthing,
        startEasting,
        endNorthing,
        endEasting,
        azimuth);
    }
  }
}
