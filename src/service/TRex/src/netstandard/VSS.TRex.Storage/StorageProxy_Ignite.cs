﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Transactions;
using VSS.Serilog.Extensions;
using VSS.TRex.Common.Extensions;
using VSS.TRex.DI;
using VSS.TRex.GridFabric;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.Storage.Utilities;
using VSS.TRex.Types;

namespace VSS.TRex.Storage
{
  /// <summary>
  /// Implementation of the IStorageProxy interface that allows to read/write operations against Ignite based IO support.
  /// Note: All read and write operations are sending and receiving MemoryStream objects.
  /// </summary>
  public class StorageProxy_Ignite : StorageProxy_IgniteBase, IStorageProxy
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<StorageProxy_Ignite>();

    /// <summary>
    /// The reference to a storage proxy representing the immutable data store derived from a mutable data store
    /// </summary>w
    public IStorageProxy ImmutableProxy { get; private set; }

    /// <summary>
    /// Constructor that obtains references to the mutable and immutable, spatial and non-spatial caches present in the grid
    /// </summary>
    /// <param name="mutability"></param>
    public StorageProxy_Ignite(StorageMutability mutability) : base(mutability)
    {
      EstablishCaches();
    }

    private void EstablishCaches()
    {
      var spatialCacheFactory = DIContext.Obtain<Func<IIgnite, StorageMutability, FileSystemStreamType, IStorageProxyCacheTransacted<ISubGridSpatialAffinityKey, ISerialisedByteArrayWrapper>>>();
      var nonSpatialCacheFactory = DIContext.Obtain<Func<IIgnite, StorageMutability, FileSystemStreamType, IStorageProxyCacheTransacted<INonSpatialAffinityKey, ISerialisedByteArrayWrapper>>>();
      var machineCacheFactory = DIContext.Obtain<Func<IIgnite, StorageMutability, FileSystemStreamType, IStorageProxyCacheTransacted<ISiteModelMachineAffinityKey, ISerialisedByteArrayWrapper>>>();

      spatialSubGridDirectoryCache = spatialCacheFactory(ignite, Mutability, FileSystemStreamType.SubGridDirectory);
      spatialSubGridSegmentCache = spatialCacheFactory(ignite, Mutability, FileSystemStreamType.SubGridSegment);

      generalNonSpatialCache = nonSpatialCacheFactory(ignite, Mutability, FileSystemStreamType.SubGridDirectory);
      spatialDataExistenceMapCache = nonSpatialCacheFactory(ignite, Mutability, FileSystemStreamType.SubGridExistenceMap);
      siteModelCache = nonSpatialCacheFactory(ignite, Mutability, FileSystemStreamType.ProductionDataXML);
      
      siteModelMachineCache = machineCacheFactory(ignite, Mutability, FileSystemStreamType.SiteModelMachineElevationChangeMap);

      designTopologyExistenceMapsCache = nonSpatialCacheFactory(ignite, Mutability, FileSystemStreamType.DesignTopologyExistenceMap);

      CommittableCaches = new IStorageProxyCacheCommit[]
      {
        spatialSubGridDirectoryCache,
        spatialSubGridSegmentCache,
        generalNonSpatialCache,
        spatialDataExistenceMapCache,
        siteModelCache,
        siteModelMachineCache,
        designTopologyExistenceMapsCache
      }.Where(x => x!= null).ToArray();
    }

    /// <summary>
    /// Supports writing a named data stream to the persistent store via the grid cache.
    /// </summary>
    public FileSystemErrorStatus WriteStreamToPersistentStore(Guid dataModelId,
      string streamName,
      FileSystemStreamType streamType,
      MemoryStream mutableStream,
      object source)
    {
      try
      {
        var cacheKey = ComputeNamedStreamCacheKey(dataModelId, streamName);

        using (var compressedStream = MemoryStreamCompression.Compress(mutableStream))
        {
          if (_log.IsTraceEnabled())
            _log.LogInformation($"Putting key:{cacheKey} in {NonSpatialCache(streamType).Name}, size:{mutableStream.Length} -> {compressedStream.Length}, ratio:{(compressedStream.Length / (1.0 * mutableStream.Length)) * 100}%");
          NonSpatialCache(streamType).Put(cacheKey, new SerialisedByteArrayWrapper(compressedStream.ToArray()));
        }

        try
        {
          // Create the immutable stream from the source data
          if (Mutability == StorageMutability.Mutable && ImmutableProxy != null)
          {
            if (!PerformNonSpatialImmutabilityConversion(mutableStream, ImmutableProxy.NonSpatialCache(streamType), cacheKey, streamType, source))
            {
              _log.LogError("Unable to project an immutable stream");
              return FileSystemErrorStatus.MutableToImmutableConversionError;
            }
          }
        }
        catch (Exception e)
        {
          _log.LogError(e, $"Exception performing mutability conversion in {nameof(WriteStreamToPersistentStore)}");
          return FileSystemErrorStatus.MutableToImmutableConversionError;
        }

        return FileSystemErrorStatus.OK;
      }
      catch (Exception e)
      {
        _log.LogError(e, $"Exception writing stream {streamName} to persistent store");
        return FileSystemErrorStatus.UnknownErrorWritingToFS;
      }
    }

    /// <summary>
    /// Supports writing a spatial data stream to the persistent store via the grid cache.
    /// </summary>
    public FileSystemErrorStatus WriteSpatialStreamToPersistentStore(Guid dataModelId,
      string streamName,
      int subGridX, int subGridY,
      long segmentStartDateTicks, long segmentEndDateTicks,
      long version,
      FileSystemStreamType streamType,
      MemoryStream mutableStream,
      object source)
    {
      try
      {
        var cacheKey = new SubGridSpatialAffinityKey(version, dataModelId, subGridX, subGridY, segmentStartDateTicks, segmentEndDateTicks);

        using (var compressedStream = MemoryStreamCompression.Compress(mutableStream))
        {
          var spatialCache = SpatialCache(streamType);
          if (_log.IsTraceEnabled())
            _log.LogInformation($"Putting key:{cacheKey} in {spatialCache.Name}, size:{mutableStream.Length} -> {compressedStream.Length}, ratio:{(compressedStream.Length / (1.0 * mutableStream.Length)) * 100}%");
          spatialCache.Put(cacheKey, new SerialisedByteArrayWrapper(compressedStream.ToArray()));
        }

        // Convert the stream to the immutable form and write it to the immutable storage proxy
        try
        {
          if (Mutability == StorageMutability.Mutable && ImmutableProxy != null)
          {
            PerformSpatialImmutabilityConversion(mutableStream, ImmutableProxy.SpatialCache(streamType), cacheKey, streamType, source);
          }
        }
        catch (Exception e)
        {
          _log.LogError(e, $"Exception performing mutability conversion in {nameof(WriteSpatialStreamToPersistentStore)}");
          return FileSystemErrorStatus.MutableToImmutableConversionError;
        }

        return FileSystemErrorStatus.OK;
      }
      catch (Exception e)
      {
        _log.LogError(e, $"Exception writing spatial stream {streamName} to persistent store");
        return FileSystemErrorStatus.UnknownErrorWritingToFS;
      }
    }

    /// <summary>
    /// Supports reading a named stream from the persistent store via the grid cache
    /// </summary>
    public FileSystemErrorStatus ReadStreamFromPersistentStore(Guid dataModelId, string streamName, FileSystemStreamType streamType, out MemoryStream stream)
    {
      stream = null;

      try
      {
        var cacheKey = ComputeNamedStreamCacheKey(dataModelId, streamName);

        //Log.LogInformation($"Getting key:{cacheKey}");

        try
        {
          using var ms = new MemoryStream(NonSpatialCache(streamType).Get(cacheKey).Bytes);
          stream = MemoryStreamCompression.Decompress(ms);
          stream.Position = 0;
        }
        catch (KeyNotFoundException)
        {
          return FileSystemErrorStatus.GranuleDoesNotExist;
        }

        return FileSystemErrorStatus.OK;
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception occurred:");

        stream = null;
        return FileSystemErrorStatus.UnknownErrorReadingFromFS;
      }
    }

    /// <summary>
    /// Supports reading a stream of spatial data from the persistent store via the grid cache
    /// </summary>
    public FileSystemErrorStatus ReadSpatialStreamFromPersistentStore(Guid dataModelId,
      string streamName,
      int subGridX, int subGridY,
      long segmentStartDateTicks,
      long segmentEndDateTicks,
      long version,
      FileSystemStreamType streamType,
      out MemoryStream stream)
    {
      stream = null;

      try
      {
        var cacheKey = new SubGridSpatialAffinityKey(version, dataModelId, subGridX, subGridY, segmentStartDateTicks, segmentEndDateTicks);

        //Log.LogInformation($"Getting key:{streamName}");

        try
        {
          using var ms = new MemoryStream(SpatialCache(streamType).Get(cacheKey).Bytes);
          stream = MemoryStreamCompression.Decompress(ms);
          stream.Position = 0;
        }
        catch (KeyNotFoundException)
        {
          return FileSystemErrorStatus.GranuleDoesNotExist;
        }

        return FileSystemErrorStatus.OK;
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception occurred:");

        stream = null;
        return FileSystemErrorStatus.UnknownErrorReadingFromFS;
      }
    }

    /// <summary>
    /// Supports removing a named stream from the persistent store via the grid cache
    /// </summary>
    public FileSystemErrorStatus RemoveStreamFromPersistentStore(Guid dataModelId,
      FileSystemStreamType streamType,
      string streamName)
    {
      try
      {
        var cacheKey = ComputeNamedStreamCacheKey(dataModelId, streamName);

        if (_log.IsTraceEnabled())
          _log.LogInformation($"Removing key:{cacheKey}");

        // Remove item from both immutable and mutable caches
        try
        {
          NonSpatialCache(streamType).Remove(cacheKey);
        }
        catch (KeyNotFoundException e)
        {
          _log.LogError(e, "Exception occurred:");
        }

        ImmutableProxy?.RemoveStreamFromPersistentStore(dataModelId, streamType, streamName);

        return FileSystemErrorStatus.OK;
      }
      catch (Exception e)
      {
        _log.LogError(e, $"Exception removing stream {streamName} from persistent store");
        return FileSystemErrorStatus.UnknownErrorWritingToFS;
      }
    }

    /// <summary>
    /// Removes a spatial stream from the persistent store identified by its spatial descriptor attributes
    /// </summary>
    public FileSystemErrorStatus RemoveSpatialStreamFromPersistentStore(Guid dataModelId,
      string streamName,
      int subGridX, int subGridY,
      long segmentStartDateTicks,
      long segmentEndDateTicks,
      long version,
      FileSystemStreamType streamType)
    {
      try
      {
        var cacheKey = new SubGridSpatialAffinityKey(version, dataModelId, subGridX, subGridY, segmentStartDateTicks, segmentEndDateTicks);

        try
        {
          SpatialCache(streamType).Remove(cacheKey);
        }
        catch (KeyNotFoundException)
        {
          return FileSystemErrorStatus.GranuleDoesNotExist;
        }

        ImmutableProxy?.RemoveSpatialStreamFromPersistentStore(dataModelId, streamName, subGridX, subGridY, segmentStartDateTicks, segmentEndDateTicks, version, streamType);

        return FileSystemErrorStatus.OK;
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception occurred:");

        return FileSystemErrorStatus.UnknownFailureRemovingFileFromFS;
      }
    }

    /// <summary>
    /// Sets a reference to a storage proxy that proxies the immutable data store for this mutable data store
    /// </summary>
    /// <param name="immutableProxy"></param>
    public void SetImmutableStorageProxy(IStorageProxy immutableProxy)
    {
      if (Mutability != StorageMutability.Mutable)
        throw new ArgumentException("Non-mutable storage proxy may not accept an immutable storage proxy reference");

      if (immutableProxy == null)
        throw new ArgumentException("Null immutable storage proxy reference supplied to SetImmutableStorageProxy()");

      if (immutableProxy.Mutability != StorageMutability.Immutable)
        throw new ArgumentException("Immutable storage proxy reference is not marked with Immutable mutability");

      ImmutableProxy = immutableProxy;
    }

    /// <summary>
    /// Commits unsaved changes in the storage proxy.
    /// No implementation for non-transactional storage proxy
    /// </summary>
    public virtual bool Commit()
    {
      return true;
    }

    /// <summary>
    /// Commits unsaved changes in the storage proxy.
    /// No implementation for non-transactional storage proxy
    /// </summary>
    public virtual bool Commit(out int numDeleted, out int numUpdated, out long numBytesWritten)
    {
      numDeleted = -1;
      numUpdated = -1;
      numBytesWritten = -1;

      return true;
    }


    /// <summary>
    /// Commits unsaved changes in the storage proxy in conjunction with an Ignite transaction mediating the activity
    /// No implementation for non-transactional storage proxy
    /// </summary>
    public virtual bool Commit(ITransaction tx, out int numDeleted, out int numUpdated, out long numBytesWritten)
    {
      numDeleted = -1;
      numUpdated = -1;
      numBytesWritten = -1;

      return true;
    }

    /// <summary>
    /// Clears changes in the storage proxy.
    /// No implementation for non-transactional storage proxy
    /// </summary>
    public virtual void Clear()
    {
    }

    public bool Commit(ITransaction tx)
    {
      Commit();
      tx.Commit();

      return true;
    }

    public ITransaction StartTransaction(TransactionConcurrency concurrency, TransactionIsolation isolation)
    { 
      return (ignite ?? DIContext.Obtain<ITRexGridFactory>().Grid(Mutability))
        .GetTransactions()
        .TxStart(concurrency, isolation);
    }

    /// <summary>
    /// Determine the amount of bytes that will be written as a result of Commit()
    /// </summary>
    public long PotentialCommitWrittenBytes()
    {
      long numBytes = 0;

      CommittableCaches.ForEach(x => numBytes += x.PotentialCommitWrittenBytes());

      return numBytes;
    }
  }
}
