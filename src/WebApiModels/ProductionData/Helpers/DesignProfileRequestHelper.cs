﻿using Microsoft.Extensions.Logging;
using System;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Utilities;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApiModels.Extensions;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Helpers
{
  /// <summary>
  /// The request representation for a linear or alignment based profile request for all thematic types other than summary volumes.
  /// Model represents a production data profile
  /// </summary>
  public class DesignProfileRequestHelper : DataRequestBase, IDesignProfileRequestHandler
  {
    public DesignProfileRequestHelper()
    { }

    public DesignProfileRequestHelper(ILoggerFactory logger, IConfigurationStore configurationStore,
      IFileListProxy fileListProxy, ICompactionSettingsManager settingsManager)
    {
      Log = logger.CreateLogger<ProductionDataProfileRequestHelper>();
      ConfigurationStore = configurationStore;
      FileListProxy = fileListProxy;
      SettingsManager = settingsManager;
    }
    public DesignProfileRequestHelper SetRaptorClient(IASNodeClient raptorClient)
    {
      return this;
    }

    /// <summary>
    /// Creates an instance of the ProfileProductionDataRequest class and populate it with data needed for a design profile.   
    /// </summary>
    /// <returns>An instance of the ProfileProductionDataRequest class.</returns>
    public DesignProfileProductionDataRequest CreateDesignProfileRequest(Guid projectUid, double startLatDegrees, double startLonDegrees, double endLatDegrees, double endLonDegrees, Guid customerUid, Guid importedFileUid)
    {
      var llPoints = ProfileLLPoints.CreateProfileLLPoints(
        startLatDegrees.latDegreesToRadians(), startLonDegrees.lonDegreesToRadians(), endLatDegrees.latDegreesToRadians(), endLonDegrees.lonDegreesToRadians());
      
      var designDescriptor = GetDescriptor(projectUid, importedFileUid);

      return DesignProfileProductionDataRequest.CreateProfileProductionData(
        ProjectId,
        importedFileUid,
        ProductionDataType.Height,
        Filter,
        -1,
        designDescriptor,
        null,
        llPoints,
        ValidationConstants.MIN_STATION,
        ValidationConstants.MIN_STATION);
    }
  }
}