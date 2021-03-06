﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.TRex.Caching.Interfaces;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.DI;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Caching
{
  /// <summary>
  /// The top level class that implements spatial data caching in TRex where that spatial data is represented by SubGrids and SubGridTrees
  /// </summary>
  public class TRexSpatialMemoryCache : ITRexSpatialMemoryCache, IDisposable
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<TRexSpatialMemoryCache>();

    private const int MAX_NUM_ELEMENTS = 1000000000;

    private readonly int _spatialMemoryCacheInterEpochSleepTimeSeconds 
      = DIContext.Obtain<IConfigurationStore>().GetValueInt("SPATIAL_MEMORY_CACHE_INTER_EPOCH_SLEEP_TIME_SECONDS", Consts.SPATIAL_MEMORY_CACHE_INTER_EPOCH_SLEEP_TIME_SECONDS);
    private readonly int _spatialMemoryCacheInvalidatedCacheContextRemovalWaitTimeSeconds 
      = DIContext.Obtain<IConfigurationStore>().GetValueInt("SPATIAL_MEMORY_CACHE_INVALIDATED_CACHE_CONTEXT_REMOVAL_WAIT_TIME_SECONDS", Consts.SPATIAL_MEMORY_CACHE_INVALIDATED_CACHE_CONTEXT_REMOVAL_WAIT_TIME_SECONDS);

    /// <summary>
    /// The MRU list that threads through all the elements in the overall cache
    /// </summary>
    public ITRexSpatialMemoryCacheStorage<ITRexMemoryCacheItem> MRUList { get; }

    /// <summary>
    /// The collection of contexts managed within the overall memory cache.
    /// Each context is identified by a string based fingerprint that encodes information such
    /// as project, grid data type requests & filter parameters etc
    /// </summary>
    private readonly Dictionary<string, ITRexSpatialMemoryCacheContext> _contexts;

    /// <summary>
    /// A collection of cache context lists, one per project active in the system.
    /// This provides a single clear boundary for locating all cache contexts related
    /// to a project for operations such as invalidation due to TAG file ingest and
    /// eviction of contexts that are empty or are otherwise subject to removal.
    /// </summary>
    private readonly Dictionary<Guid, List<ITRexSpatialMemoryCacheContext>> _projectContexts;

    // ReSharper disable once InconsistentlySynchronizedField
    public int ContextCount => _contexts.Count;

    // ReSharper disable once InconsistentlySynchronizedField
    public int ProjectCount => _projectContexts.Count;

    /// <summary>
    /// The internal modifiable count of contexts that have been removed
    /// </summary>
    private long _contextRemovalCount;

    /// <summary>
    /// THe exposed readonly property for the number of removed contexts
    /// </summary>
    public long ContextRemovalCount => _contextRemovalCount;

    /// <summary>
    /// The maximum number of elements that may be contained in this cache
    /// </summary>
    public int MaxNumElements { get; }

    private int _currentNumElements;
    public int CurrentNumElements => _currentNumElements;

    private long _currentSizeInBytes;
    public long CurrentSizeInBytes => _currentSizeInBytes;

    public int MruNonUpdateableSlotCount { get; }

    public long MaxSizeInBytes { get; }

    private readonly TRexSpatialMemoryCacheContextRemover _contextRemover;

    /// <summary>
    /// Creates a new spatial data cache containing at most maxNumElements items. Elements are stored in
    /// an MRU list and are moved to the top of the MRU list of their distance from the top of the list at the time they
    /// are touched is outside the MRU dead band (expressed as a fraction of the overall maximum number of elements in the cache.
    /// </summary>
    public TRexSpatialMemoryCache(int maxNumElements, long maxSizeInBytes, double mruDeadBandFraction)
    {
      if (maxNumElements < 1 || maxNumElements > MAX_NUM_ELEMENTS)
        throw new ArgumentException($"maxNumElements ({maxNumElements}) not in range 1..{MAX_NUM_ELEMENTS}");

      // Set cache size range between 1kb and 100Gb
      if (maxSizeInBytes < 1000 || maxSizeInBytes > 100000000000)
        throw new ArgumentException($"maxSizeInBytes ({maxSizeInBytes}) not in range 1000..100000000000 (1e3..1e11)");

      if (mruDeadBandFraction < 0.0 || mruDeadBandFraction > 1.0)
        throw new ArgumentException($"mruDeadBandFraction ({mruDeadBandFraction}) not in range 0.0..1.0");

      MaxNumElements = maxNumElements;
      MaxSizeInBytes = maxSizeInBytes;
      MruNonUpdateableSlotCount = (int)Math.Truncate(maxNumElements * mruDeadBandFraction);

      MRUList = new TRexSpatialMemoryCacheStorage<ITRexMemoryCacheItem>(maxNumElements, MruNonUpdateableSlotCount);
      _contexts = new Dictionary<string, ITRexSpatialMemoryCacheContext>();
      _projectContexts = new Dictionary<Guid, List<ITRexSpatialMemoryCacheContext>>();

      _contextRemover = new TRexSpatialMemoryCacheContextRemover(this, _spatialMemoryCacheInterEpochSleepTimeSeconds);
    }

    /// <summary>
    /// Locates a cache context responsible for storing elements that share the same context fingerprint. If there is no matching context
    /// available then a new one is created and returned. This operation is performed under a lock covering the pool of available contexts
    /// </summary>
    public ITRexSpatialMemoryCacheContext LocateOrCreateContext(Guid projectUid, GridDataType gridDataType, string contextFingerPrint, TimeSpan cacheDuration)
    {
      lock (_contexts)
      {
        if (_contexts.TryGetValue(contextFingerPrint, out var context))
        {
          if (context.MarkedForRemoval)
            context.Reanimate();

          return context; // It exists, return it
        }

        // Create the new context
        var newContext = new TRexSpatialMemoryCacheContext(this, MRUList, cacheDuration, contextFingerPrint, gridDataType, projectUid);
        _contexts.Add(contextFingerPrint, newContext);

        lock (_projectContexts)
        {
          // Add a reference to this context into the project specific list of contexts
          if (_projectContexts.TryGetValue(projectUid, out var projectList))
            projectList.Add(newContext);
          else
            _projectContexts.Add(projectUid, new List<ITRexSpatialMemoryCacheContext> {newContext});
        }

        // Mark the newly created context for removal. This may seem counter intuitive, but covers the case
        // where a context is created but has no elements added to it, or subsequently removed
        newContext.MarkForRemoval(DateTime.UtcNow.AddSeconds(_spatialMemoryCacheInvalidatedCacheContextRemovalWaitTimeSeconds));

        return newContext;
      }
    }

    /// <summary>
    /// Locates a cache context responsible for storing elements that share the same context fingerprint. If there is no matching context
    /// available then a new one is created and returned. This operation is performed under a lock covering the pool of available contexts
    /// </summary>
    public ITRexSpatialMemoryCacheContext LocateOrCreateContext(Guid projectUid, GridDataType gridDataType, string contextFingerPrint)
    {
      return LocateOrCreateContext(projectUid, gridDataType, contextFingerPrint, TRexSpatialMemoryCacheContext.NullCacheTimeSpan);
    }

    /// <summary>
    /// Adds an item into a context within the overall memory cache of these items. The context must be obtained from the
    /// memory cache instance prior to use. This operation is thread safe - all operations are concurrency locked within the
    /// confines of the context.
    /// </summary>
    public CacheContextAdditionResult Add(ITRexSpatialMemoryCacheContext context, ITRexMemoryCacheItem element, long invalidationVersion)
    {
      lock (_contexts)
      {
        // If the invalidation version of a sub grid derivative being added to the cache does not match the current invalidation version
        // then prevent it being added to the cache, and return a negative result to indicate the refusal
        if (invalidationVersion != context.InvalidationVersion)
        {
          _log.LogInformation($"Sub grid derivative at {element.CacheOriginX}:{element.CacheOriginY} not added to cache context due to invalidation version mismatch ({invalidationVersion} vs {context.InvalidationVersion})");
          return CacheContextAdditionResult.RejectedDueToInvlidationVersionMismatch;
        }

        if (context.MarkedForRemoval)
          context.Reanimate();

        var result = context.Add(element);

        if (result == CacheContextAdditionResult.Added)
        {
          // Perform some house keeping to keep the cache size in bounds
          ItemAddedToContext(element.IndicativeSizeInBytes());
          while (_currentSizeInBytes > MaxSizeInBytes && !MRUList.IsEmpty())
          {
            MRUList.EvictOneLRUItem();
          }
        }

        return result;
      }
    }

    /// <summary>
    /// Removes an item from a context within the overall memory cache of these items. The context must be obtained from the
    /// memory cache instance prior to use. This operation is thread safe - all operations are concurrency locked within the
    /// confines of the context.
    /// </summary>
    public void Remove(ITRexSpatialMemoryCacheContext context, ITRexMemoryCacheItem element)
    {
      lock (_contexts)
      {
        context.Remove(element);

        // Perform some house keeping to keep the cache size in bounds
        ItemRemovedFromContext(element.IndicativeSizeInBytes());
      }
    }

    private void ItemAddedToContext(int sizeInBytes)
    {
      // Increment the number of elements in the cache
      Interlocked.Increment(ref _currentNumElements);

      // Increment the memory usage in the cache
      Interlocked.Add(ref _currentSizeInBytes, sizeInBytes);
    }

    public void ItemRemovedFromContext(int sizeInBytes)
    {
      // Decrement the memory usage in the cache
      var number = Interlocked.Add(ref _currentSizeInBytes, -sizeInBytes);

      if (number < 0)
        throw new TRexException("CurrentSizeInBytes < 0! Consider using Cache.Add(context, item).");

      // Decrement the number of elements in the cache
      Interlocked.Decrement(ref _currentNumElements);
    }

    /// <summary>
    /// Attempts to read an element from a cache context given the spatial location of the element
    /// </summary>
    /// <param name="context">The request, filter and other data specific context for spatial data</param>
    /// <param name="originX">The origin (bottom left) cell of the spatial data sub grid</param>
    /// <param name="originY">The origin (bottom left) cell of the spatial data sub grid</param>
    public ITRexMemoryCacheItem Get(ITRexSpatialMemoryCacheContext context, int originX, int originY)
    {
      lock (_contexts)
      {
        return context.Get(originX, originY);
      }
    }

    /// <summary>
    /// Invalidates sub grids held within all cache contexts for a project that are sensitive to
    /// ingest of production data (eg: from TAG files)
    /// </summary>
    public void InvalidateDueToProductionDataIngest(Guid projectUid, Guid changeEventUid, ISubGridTreeBitMask mask)
    {
      var numInvalidatedSubGrids = 0;
      var numScannedSubGrids = 0;
      var startTime = DateTime.UtcNow;
      var contextCount = 0;

      lock (_contexts)
      {
        if (_projectContexts.TryGetValue(projectUid, out var projectContexts))
        {

          if (projectContexts == null || projectContexts.Count == 0)
            return;

          contextCount = projectContexts.Count;

          // Walk through the cloned list of contexts evicting all relevant element per the supplied mask
          // Only hold a Contexts lock for the duration of a single context. 'Eviction' is really marking the 
          // element as dirty to amortize the effort in executing the invalidation across cache accessor contexts.
          foreach (var context in projectContexts)
          {
            // Empty contexts are ignored
            if (context.TokenCount == 0)
              return;

            // If the context in question is not sensitive to production data ingest then ignore it
            if (!context.Sensitivity.HasFlag(TRexSpatialMemoryCacheInvalidationSensitivity.ProductionDataIngest))
              return;

            // Iterate across all elements in the mask:
            // 1. Locate the cache entry
            // 2. Mark it as dirty
            mask.ScanAllSetBitsAsSubGridAddresses(origin =>
            {
              context.InvalidateSubGrid(origin.X, origin.Y, out var subGridPresentForInvalidation);

              numScannedSubGrids++;
              if (subGridPresentForInvalidation)
                numInvalidatedSubGrids++;
            });
          }
        }
      }

      _log.LogInformation($"Invalidated {numInvalidatedSubGrids} out of {numScannedSubGrids} scanned sub grid from {contextCount} contexts in {DateTime.UtcNow - startTime} [project {projectUid}, change event ID {changeEventUid}]");
    }

    /// <summary>
    /// Removes all contexts in the cache that are marked for removal more than 'age' ago
    /// </summary>
    public void RemoveContextsMarkedForRemoval()
    {
      var numRemoved = 0;
      var removalDateUtc = DateTime.UtcNow;
      var startTime = DateTime.UtcNow;
      
      lock (_contexts)
      {
        // Construct a list of candidates to work through so there are not issues with modifying a collection being enumerated
        var candidates = _contexts.Values.Where(x => x.MarkedForRemoval && x.MarkedForRemovalAtUtc <= removalDateUtc).ToList();
        foreach (var context in candidates)
        {
          if (context.TokenCount != 0)
          {
            _log.LogError($"Context in project {context.ProjectUID} with fingerprint {context.FingerPrint} has tokens in it {context.TokenCount} and is set for removal. Resetting context state to normal.");
            context.Reanimate();

            continue;
          }

          // Remove the context:
          // 1. From the primary contexts dictionary
          _contexts.Remove(context.FingerPrint);

          // 2. From the project list of contexts
          _projectContexts[context.ProjectUID].Remove(context);

          // 3. Dispose the context
          context.Dispose();

          numRemoved++;
          Interlocked.Increment(ref _contextRemovalCount);
        }
      }

      _log.LogInformation($"{numRemoved} contexts removed in {DateTime.UtcNow - startTime}");
    }

    public void Dispose()
    {
      _contextRemover.Dispose();
    }

    /// <summary>
    /// Invalidate sub grids associated with the design changed.
    /// </summary>
    public void InvalidateDueToDesignChange(Guid projectUid, Guid designUid)
    {
      var numInvalidatedContexts = 0;

      lock (_contexts)
      {
        if (_projectContexts.TryGetValue(projectUid, out var projectContexts))
        {
          if (projectContexts == null || projectContexts.Count == 0)
            return;

          // Walk through contexts for the project evicting all relevant element per the supplied mask
          // Only hold a Contexts lock for the duration of a single context. 'Eviction' is really marking the 
          // element as dirty to amortize the effort in executing the invalidation across cache accessor contexts.
          foreach (var context in projectContexts)
          {
            if (context.FingerPrint.Contains(designUid.ToString()))
            {
              context.InvalidateAllSubGrids();
              numInvalidatedContexts++;
            }
          }
        }
      }

      _log.LogInformation($"Invalidating sub grids due to design change for Project:{projectUid},  Design{designUid}, #ContextsInvalidated:{numInvalidatedContexts}");
    }
  }
}
