﻿using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.Utilities;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApiModels.Compaction.Helpers;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Helpers
{
  /// <summary>
  /// The request representation for a linear or alignment based profile request for a design surface.
  /// Model represents a design profile
  /// </summary>
  public class DesignProfileRequestHelper : DataRequestBase, IDesignProfileRequestHandler
  {
    public DesignProfileRequestHelper()
    { }

    public DesignProfileRequestHelper(ILoggerFactory logger, IConfigurationStore configurationStore,
      IFileImportProxy fileImportProxy, ICompactionSettingsManager settingsManager)
    {
      Log = logger.CreateLogger<ProductionDataProfileRequestHelper>();
      ConfigurationStore = configurationStore;
      FileImportProxy = fileImportProxy;
      SettingsManager = settingsManager;
    }

    /// <summary>
    /// Creates an instance of the CompactionProfileDesignRequest class and populate it with data needed for a design profile.   
    /// </summary>
    /// <returns>An instance of the CompactionProfileDesignRequest class.</returns>
    public CompactionProfileDesignRequest CreateDesignProfileRequest(double startLatDegrees, double startLonDegrees, double endLatDegrees, double endLonDegrees)
    {
      var llPoints = ProfileLLPoints.CreateProfileLLPoints(
        startLatDegrees.LatDegreesToRadians(), startLonDegrees.LonDegreesToRadians(), endLatDegrees.LatDegreesToRadians(), endLonDegrees.LonDegreesToRadians());
      
      return new CompactionProfileDesignRequest(
        ProjectId,
        ProjectUid,
        DesignDescriptor,
        Filter,
        null,
        null,
        null,
        llPoints,
        ValidationConstants3D.MIN_STATION,
        ValidationConstants3D.MIN_STATION);
    }
  }
}
