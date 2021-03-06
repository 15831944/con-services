﻿using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VSS.TRex.Caching.Interfaces;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.DI;
using VSS.TRex.SiteModelChangeMaps.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SiteModels.Interfaces.Events;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.SiteModels
{
  /// <summary>
  /// SiteModels contains a map of site model/data model identifiers (long) and SiteModel instances. 
  /// It may receive messages from the Ignite layer regarding invalidation of cache items...
  /// </summary>
  public class SiteModels : ISiteModels
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<SiteModels>();

    /// <summary>
    /// The cached set of site models that are currently 'open' in TRex
    /// </summary>
    private readonly Dictionary<Guid, ISiteModel> _cachedModels = new Dictionary<Guid, ISiteModel>();

    private IStorageProxy _primaryMutableStorageProxy;
    private IStorageProxy _primaryImmutableStorageProxy;
    private IStorageProxyFactory _storageProxyFactory;

    private IStorageProxyFactory StorageProxyFactory => _storageProxyFactory ??= DIContext.Obtain<IStorageProxyFactory>();

    public IStorageProxy PrimaryMutableStorageProxy => _primaryMutableStorageProxy ??= StorageProxyFactory.MutableGridStorage();
    public IStorageProxy PrimaryImmutableStorageProxy => _primaryImmutableStorageProxy ??= StorageProxyFactory.ImmutableGridStorage();

    public IStorageProxy PrimaryStorageProxy(StorageMutability mutability)
    {
      return mutability == StorageMutability.Immutable ? PrimaryImmutableStorageProxy : PrimaryMutableStorageProxy;
    }

    /// <summary>
    /// The primary grid this site models instance is targeting based on the desired mutability
    /// </summary>
    public StorageMutability PrimaryMutability { get; }

    /// <summary>
    /// Default no-arg constructor. Made private to enforce provision of storage proxy
    /// </summary>
    public SiteModels(StorageMutability primaryMutability)
    {
      PrimaryMutability = primaryMutability;
    }

    public ISiteModel GetSiteModel(Guid id) => GetSiteModel(id, false);

    /// <summary>
    /// Retrieves a site model from the persistent store ready for use. If the site model does not
    /// exist it will be created if CreateIfNotExist is true.
    /// </summary>
    public ISiteModel GetSiteModel(Guid id, bool createIfNotExist)
    {
      ISiteModel result;

      lock (_cachedModels)
      {
        if (_cachedModels.TryGetValue(id, out result))
        {
          if (result.IsMarkedForDeletion)
          {
            // Ignore this site model as it is in the process of being deleted, also
            // remove it from the cache...
            _cachedModels.Remove(id);
            return null;
          }

          return result;
        }
      }

      // Note: If this context is clients to both grids, then prioritise accessing the site model on the mutable context.
      result = DIContext.Obtain<ISiteModelFactory>().NewSiteModel_NonTransient(id, PrimaryMutability);

      _log.LogInformation($"Loading site model {id} from persistent store");

      if (result.LoadFromPersistentStore() == FileSystemErrorStatus.OK)
      {
        lock (_cachedModels)
        {
          if (result.IsMarkedForDeletion)
          {
            // Ignore this site model as it is in the process of being deleted, also
            // remove it from the cache...
            _cachedModels.Remove(id);
            return null;
          }

          // Check if another thread managed to get in before this thread. If so discard
          // the one just created in favor of the one in the dictionary
          if (_cachedModels.TryGetValue(id, out var result2))
            return result2;

          _cachedModels.Add(id, result);
          return result;
        }
      }

      _log.LogInformation($"Site model {id} is not present in the persistent store, createIfNotExist = {createIfNotExist}");

      if (createIfNotExist)
      {
        lock (_cachedModels)
        {
          // Check if another thread managed to get in before this thread. If so discard
          // the one just created in favor of the one in the dictionary
          if (_cachedModels.TryGetValue(id, out var result2))
            return result2;

          _log.LogInformation($"Creating new site model {id} and adding to internal cache");

          _cachedModels.Add(id, result);

          var storageProxy = DIContext.Obtain<IStorageProxyFactory>().MutableGridStorage();

          if (storageProxy.Mutability != StorageMutability.Mutable)
          {
            throw new TRexPersistencyException($"Site model {id} may only be created within the mutable context");
          }
          
          result.SaveMetadataToPersistentStore(storageProxy, true);

          // Establish the metadata entry for this new site model
          DIContext.Obtain<ISiteModelMetadataManager>().Add(id, result.MetaData);

          return result;
        }
      }

      return null;
    }

    /// <summary>
    /// Retrieves a site model from persistent storage with no reference to cached
    /// site models, or attributes of the site model that might otherwise affect its
    /// retrieval, such as being marked for deletion
    /// </summary>
    public ISiteModel GetSiteModelRaw(Guid id)
    {
      _log.LogInformation($"Loading site model {id} from persistent store as raw read");

      // Note: If this context is clients to both grids, then prioritise accessing the site model on the mutable context.
      var result = DIContext.Obtain<ISiteModelFactory>().NewSiteModel_NonTransient(id, PrimaryMutability);

      return result.LoadFromPersistentStore() == FileSystemErrorStatus.OK ? result : null;
    }

    /// <summary>
    /// Drops a site model from the site models cache.
    /// Note: This may be performed safely at any time irrespective of the concurrently executing requests
    /// referencing that site model
    /// </summary>
    /// <param name="id">The UID identifying the site model to be dropped from the cache</param>
    public void DropSiteModel(Guid id)
    {
      ISiteModel siteModel;

      lock (_cachedModels)
      {
        if (_cachedModels.TryGetValue(id, out siteModel))
        {
          _cachedModels.Remove(id);
        }
      }

      siteModel?.Dispose();
    }

    /// <summary>
    /// Handles the situation when TAG file processing or some other activity has modified the attributes of a site model
    /// requiring the site model to be reloaded
    /// </summary>
    public void SiteModelAttributesHaveChanged(ISiteModelAttributesChangedEvent message)
    {
      var messageAge = DateTime.UtcNow - message.TimeSentUtc;
      _log.LogInformation($"Entering attribute change notification processor for project {message.SiteModelID}, change event ID {message.ChangeEventUid}, event message age {messageAge}");

      if (messageAge.TotalSeconds > 1.0)
      {
        _log.LogWarning($"Message age more than 1 second [{messageAge}]");
      }

      // Site models have immutable characteristics in TRex. Multiple requests may reference the same site model
      // concurrently, with no interlocks enforcing access serialization. Any attempt to replace or modify an already loaded
      // site model may cause issue with concurrent request accessing that site model.
      // THe strategy here is to preserve continued access by concurrent requests to the site model retrieved
      // at the time the request was initiated by removing it from the SiteModels cache but not destroying it.
      // Once all request based references to the site model have completed the now orphaned site model will be cleaned
      // up by the garbage collector. Removal of the site model is interlocked with getting a site model reference
      // to ensure no concurrency issues within the underlying cache implementation
      // Note: The site model references some elements that may be preserved via the site model factory method that
      // accepts an origin site model.
      // These elements are:
      // 1. ExistenceMap
      // 2. Sub grid tree containing cached sub grid data
      // 3. Coordinate system
      // 4. Designs
      // 5. Surveyed Surfaces
      // 6. Machines
      // 7. Machines target values
      // 8. Machines design names
      // 9. Proofing runs
      // 10. Alignments
      // 11. Site model marked for deletion

      ISiteModel siteModel;

      // Construct a new site model that preserves elements not affected by the notification and replace the existing 
      // site model reference with it.
      lock (_cachedModels)
      {
        _cachedModels.TryGetValue(message.SiteModelID, out siteModel);

        if (siteModel != null && message.SiteModelMarkedForDeletion)
        {
          // Remove the site model from the cache and exit.
          _cachedModels.Remove(message.SiteModelID);
          return;
        }

        // Note: The spatial data grid is highly conserved and never killed in a site model change notification.
        var originFlags =
            SiteModelOriginConstructionFlags.PreserveGrid
            | (!message.ExistenceMapModified ? SiteModelOriginConstructionFlags.PreserveExistenceMap : 0)
            | (!message.CsibModified ? SiteModelOriginConstructionFlags.PreserveCsib : 0)
            | (!message.DesignsModified ? SiteModelOriginConstructionFlags.PreserveDesigns : 0)
            | (!message.SurveyedSurfacesModified ? SiteModelOriginConstructionFlags.PreserveSurveyedSurfaces : 0)
            | (!message.MachinesModified ? SiteModelOriginConstructionFlags.PreserveMachines : 0)
            | (!message.MachineTargetValuesModified ? SiteModelOriginConstructionFlags.PreserveMachineTargetValues : 0)
            | (!message.MachineDesignsModified ? SiteModelOriginConstructionFlags.PreserveMachineDesigns | SiteModelOriginConstructionFlags.PreserveSiteModelDesigns : 0)
            | (!message.ProofingRunsModified ? SiteModelOriginConstructionFlags.PreserveProofingRuns : 0)
            | (!message.AlignmentsModified ? SiteModelOriginConstructionFlags.PreserveAlignments : 0)
          ;

        _log.LogInformation($"Processing attribute change notification for site model {message.SiteModelID}. Preserved elements are {originFlags}");

        if (siteModel != null)
        {
          // First create a new site model to replace the site model with, requesting certain elements of the existing site model
          // to be preserved in the new site model instance.
          siteModel = DIContext.Obtain<ISiteModelFactory>().NewSiteModel(siteModel, originFlags);

          // Replace the site model reference in the cache with the new site model
          _cachedModels[message.SiteModelID] = siteModel;
        }
      }

      // If the notification contains an existence map change mask then all cached sub grid based elements that match the masked sub grids
      // need to be evicted from all cached contexts related to this site model. Note: This operation is not performed under a lock as the 
      // removal operations on the cache are lock free
      if (message.ExistenceMapChangeMask != null)
      {
        // Create and deserialize the sub grid but mask from the message
        var mask = new SubGridTreeSubGridExistenceBitMask();
        mask.FromBytes(message.ExistenceMapChangeMask);

        if (siteModel != null)
        {
          // Iterate over all leaf sub grids in the mask. For each get the matching node sub grid in siteModel.Grid, 
          // and remove all sub grid references from that node sub grid matching the bits in the bit mask sub grid
          mask.ScanAllSubGrids(leaf =>
          {
            // Obtain the matching node sub grid in Grid
            var node = siteModel.Grid.LocateClosestSubGridContaining
             (leaf.OriginX << SubGridTreeConsts.SubGridIndexBitsPerLevel,
              leaf.OriginY << SubGridTreeConsts.SubGridIndexBitsPerLevel,
              leaf.Level);

            // If there are sub grids present in Grid that match the sub grids identified by leaf
            // remove the elements identified in leaf from the node sub grid.
            if (node != null)
            {
              ((ISubGridTreeLeafBitmapSubGrid) leaf).ForEachSetBit((x, y) => node.SetSubGrid(x, y, null));
            }

            return true; // Keep processing leaf sub grids
          });
        }

        // Advise the spatial memory general sub grid result cache of the change so it can invalidate cached derivatives
        DIContext.ObtainOptional<ITRexSpatialMemoryCache>()?.InvalidateDueToProductionDataIngest(message.SiteModelID, message.ChangeEventUid, mask);

        // Advise any registered site model change map notifier of the changes
        DIContext.ObtainOptional<ISiteModelChangeMapDeltaNotifier>()?.Notify(message.SiteModelID, DateTime.UtcNow, mask, SiteModelChangeMapOrigin.Ingest, SiteModelChangeMapOperation.AddSpatialChanges);
      }
    }

    /// <summary>
    /// Returns a cloned list of references to the set of site models currently present in the site models cache
    /// </summary>
    public List<ISiteModel> GetSiteModels()
    {
      var models = new List<ISiteModel>();

      lock (_cachedModels)
      {
        models.AddRange(_cachedModels.Values);
      }

      return models;
    }
  }
}
