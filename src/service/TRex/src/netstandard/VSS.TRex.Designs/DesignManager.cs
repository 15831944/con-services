﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SiteModels.Interfaces.Events;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Types;
using VSS.TRex.Common.Utilities.ExtensionMethods;
using VSS.TRex.Storage.Models;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.ExistenceMaps.GridFabric.Requests;
using VSS.TRex.Common.Extensions;

namespace VSS.TRex.Designs
{
  /// <summary>
  /// Service metaphor providing access and management control over designs stored for site models
  /// </summary>
  public class DesignManager : IDesignManager
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<DesignManager>();

    private readonly IStorageProxy _writeStorageProxy;
    private readonly IStorageProxy _readStorageProxy;

    public const string DESIGNS_STREAM_NAME = "Designs";

    /// <summary>
    /// Default no-arg constructor that sets the grid and cache name to default values
    /// </summary>
    public DesignManager(StorageMutability mutability)
    {
       _writeStorageProxy = DIContext.Obtain<ISiteModels>().PrimaryMutableStorageProxy;
       _readStorageProxy = DIContext.Obtain<ISiteModels>().PrimaryStorageProxy(mutability);
    }

    /// <summary>
    /// Loads the set of designs for a site model. If none exist an empty list is returned.
    /// </summary>
    private IDesigns Load(Guid siteModelId)
    {
      _log.LogInformation($"Loading designs for project {siteModelId}");

      var designs = DIContext.ObtainRequired<IDesigns>();
      try
      {
        _readStorageProxy.ReadStreamFromPersistentStore(siteModelId, DESIGNS_STREAM_NAME, FileSystemStreamType.Designs, out var ms);

        if (ms != null)
        {
          using (ms)
          {
            designs.FromStream(ms);
          }
        }
      }
      catch (KeyNotFoundException)
      {
        // This is OK, the element is not present in the cache yet - return an empty design list
      }
      catch (Exception e)
      {
        throw new TRexException("Exception reading designs cache element from Ignite", e);
      }

      _log.LogInformation($"Loaded {designs.Count} designs for project {siteModelId}");

      return designs;
    }

    /// <summary>
    /// Stores the list of designs for a site model
    /// </summary>
    private void Store(Guid siteModelId, IDesigns designs)
    {
      try
      {
        using var stream = designs.ToStream();
        if (_writeStorageProxy.WriteStreamToPersistentStore(siteModelId, DESIGNS_STREAM_NAME, FileSystemStreamType.Designs, stream, designs) == FileSystemErrorStatus.OK)
        {
          _writeStorageProxy.Commit();
        }

        // Notify the mutable and immutable grid listeners that attributes of this site model have changed
        var sender = DIContext.Obtain<ISiteModelAttributesChangedEventSender>();
        sender.ModelAttributesChanged(SiteModelNotificationEventGridMutability.NotifyAll, siteModelId, designsChanged: true);
      }
      catch (Exception e)
      {
        throw new TRexException("Exception writing updated designs cache element to Ignite", e);
      }
    }

    /// <summary>
    /// Add a new design to a site model
    /// </summary>
    public IDesign Add(Guid siteModelId, DesignDescriptor designDescriptor, BoundingWorldExtent3D extents, ISubGridTreeBitMask existenceMap)
    {
      if (extents == null)
        throw new ArgumentNullException(nameof(extents));

      if (existenceMap == null)
        throw new ArgumentNullException(nameof(existenceMap));

      // Add the design to the designs list
      var designs = Load(siteModelId);
      var result = designs.AddDesignDetails(designDescriptor.DesignID, designDescriptor, extents);

      // Store the existence map into the cache
      using var stream = existenceMap.ToStream();
      var fileName = BaseExistenceMapRequest.CacheKeyString(ExistenceMaps.Interfaces.Consts.EXISTENCE_MAP_DESIGN_DESCRIPTOR, designDescriptor.DesignID);
      if (_writeStorageProxy.WriteStreamToPersistentStore(siteModelId, fileName,
                                                          FileSystemStreamType.DesignTopologyExistenceMap, stream, existenceMap) != FileSystemErrorStatus.OK)
      {
        _log.LogError("Failed to write existence map to persistent store for key {fileName}");
        return null;
      }

      // Store performs Commit() operation
      Store(siteModelId, designs);

      return result;
    }

    /// <summary>
    /// Returns the list of all designs known for the site model
    /// </summary>
    public IDesigns List(Guid siteModelId)
    {
      _log.LogInformation($"Listing designs from {siteModelId}");

      return Load(siteModelId);
    }

    /// <summary>
    /// Remove a given design from a site model
    /// </summary>
    public bool Remove(Guid siteModelId, Guid designId)
    {
      var designs = Load(siteModelId);
      var result = designs.RemoveDesign(designId);

      if (result)
      {
        var removeMapResult = _writeStorageProxy.RemoveStreamFromPersistentStore(siteModelId, FileSystemStreamType.DesignTopologyExistenceMap,
          BaseExistenceMapRequest.CacheKeyString(ExistenceMaps.Interfaces.Consts.EXISTENCE_MAP_DESIGN_DESCRIPTOR, designId));
        
        if (removeMapResult != FileSystemErrorStatus.OK)
        {
          _log.LogInformation($"Removing existence map for design {designId} in project {siteModelId} failed with error {removeMapResult}");
        }

        Store(siteModelId, designs);
      }

      return result;
    }

    /// <summary>
    /// Remove the design list for a site model from the persistent store
    /// </summary>
    public bool Remove(Guid siteModelId, IStorageProxy storageProxy)
    {
      // First remove all the existence maps associated with the designs
      var designs = Load(siteModelId);
      designs.ForEach(x =>
      {
        FileSystemErrorStatus result;
        var filename = BaseExistenceMapRequest.CacheKeyString(ExistenceMaps.Interfaces.Consts.EXISTENCE_MAP_DESIGN_DESCRIPTOR, x.ID);
        if ((result = storageProxy.RemoveStreamFromPersistentStore(siteModelId, FileSystemStreamType.DesignTopologyExistenceMap, filename)) != FileSystemErrorStatus.OK)
        {
          _log.LogWarning($"Unable to remove existence map for design {x.ID}, filename = {filename}, with result: {result}");
        }
      });

      // Then remove the designs list stream itself
      var result = storageProxy.RemoveStreamFromPersistentStore(siteModelId, FileSystemStreamType.Designs, DESIGNS_STREAM_NAME);

      if (result != FileSystemErrorStatus.OK)
      {
        _log.LogInformation($"Removing designs list from project {siteModelId} failed with error {result}");
      }

      return result == FileSystemErrorStatus.OK;
    }
  }
}

