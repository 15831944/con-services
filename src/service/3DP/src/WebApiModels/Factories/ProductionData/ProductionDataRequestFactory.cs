﻿using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Filter.Abstractions.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Models.Designs;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;

namespace VSS.Productivity3D.WebApi.Models.Factories.ProductionData
{
  /// <summary>
  /// 
  /// </summary>
  public class ProductionDataRequestFactory : IProductionDataRequestFactory
  {
    private readonly ILogger log;
    private readonly IConfigurationStore configStore;
    private readonly IFileImportProxy fileImportProxy;
    private readonly ICompactionSettingsManager settingsManager;
    private Guid projectUid;
    private long projectId;
    private IHeaderDictionary headers;
    private CompactionProjectSettings projectSettings;
    private CompactionProjectSettingsColors projectSettingsColors;
    private FilterResult filter;
    private DesignDescriptor designDescriptor;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="logger">ILoggerFactory service implementation</param>
    /// <param name="configStore">IConfigurationStore service implementation</param>
    /// <param name="settingsManager">ICompactionSettingsManager service implementation</param>
    public ProductionDataRequestFactory(ILoggerFactory logger, IConfigurationStore configStore,
      IFileImportProxy fileImportProxy, ICompactionSettingsManager settingsManager)
    {
      log = logger.CreateLogger<ProductionDataRequestFactory>();
      this.configStore = configStore;
      this.fileImportProxy = fileImportProxy;
      this.settingsManager = settingsManager;
    }

    /// <summary>
    /// Create instance of T.
    /// </summary>
    /// <typeparam name="T">Derived implementation of DataRequestBase</typeparam>
    /// <returns>Returns instance of T with required attributes set.</returns>
    public T Create<T>(Action<ProductionDataRequestFactory> action) where T : DataRequestBase, new()
    {
      action(this);

      var obj = new T();
      obj.Initialize(log, configStore, fileImportProxy, settingsManager, projectUid, projectId, projectSettings, projectSettingsColors, headers, filter, designDescriptor);

      return obj;
    }

    /// <summary>
    /// Sets the ProjectUID
    /// </summary>
    /// <param name="projectUid"></param>
    public ProductionDataRequestFactory ProjectUid(Guid projectUid)
    {
      this.projectUid = projectUid;
      return this;
    }
    
    /// <summary>
    /// Sets the ProjectID
    /// </summary>
    /// <param name="projectId"></param>
    public ProductionDataRequestFactory ProjectId(long projectId)
    {
      this.projectId = projectId;
      return this;
    }

    /// <summary>
    /// Sets the collection of custom headers used on the service request.
    /// </summary>
    /// <param name="headers"></param>
    public ProductionDataRequestFactory Headers(IHeaderDictionary headers)
    {
      this.headers = headers;
      return this;
    }

    /// <summary>
    /// Sets the compaction settings targets used for the project.
    /// </summary>
    /// <param name="projectSettings"></param>
    public ProductionDataRequestFactory ProjectSettings(CompactionProjectSettings projectSettings)
    {
      this.projectSettings = projectSettings;
      return this;
    }

    /// <summary>
    /// Sets the compaction settings colors used for the project.
    /// </summary>
    /// <param name="projectSettingsColors"></param>
    public ProductionDataRequestFactory ProjectSettingsColors(CompactionProjectSettingsColors projectSettingsColors)
    {
      this.projectSettingsColors = projectSettingsColors;
      return this;
    }

    /// <summary>
    /// Sets the filter.
    /// </summary>
    /// <param name="filter">Filter model for the raptor query.</param>
    public ProductionDataRequestFactory Filter(FilterResult filter)
    {
      this.filter = filter;
      return this;
    }

    /// <summary>
    /// Sets the design descriptor.
    /// </summary>
    /// <param name="designDescriptor">Design for the raptor query.</param>
    public ProductionDataRequestFactory DesignDescriptor(DesignDescriptor designDescriptor)
    {
      this.designDescriptor = designDescriptor;
      return this;
    }
  }
}
