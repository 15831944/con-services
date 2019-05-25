﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Apache.Ignite.Core;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;

namespace VSS.TRex.Storage
{
  /// <summary>
  /// Implementation of the IStorageProxy interface that provides read through for items covered by the storage proxy
  /// but which buffers all writes (enlists them in a transaction) until commanded to flush the writes to Ignite in a
  /// single transacted PutAll().
  /// Note: All read and write operations are sending and receiving MemoryStream objects.
  /// </summary>
  public class StorageProxy_Ignite_Transactional : StorageProxy_Ignite
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<StorageProxy_Ignite_Transactional>();

    /// <summary>
    /// Constructor that obtains references to the mutable and immutable, spatial and non-spatial caches present in the grid
    /// </summary>
    /// <param name="mutability"></param>
    public StorageProxy_Ignite_Transactional(StorageMutability mutability) : base(mutability)
    {
      EstablishCaches();
    }

    /// <summary>
    /// Creates transactional storage proxies to be used by the consuming client
    /// </summary>
    private void EstablishCaches()
    {
      spatialCache = DIContext.Obtain<Func<IIgnite, StorageMutability, IStorageProxyCacheTransacted<ISubGridSpatialAffinityKey, byte[]>>>()(ignite, Mutability);
      generalNonSpatialCache = DIContext.Obtain<Func<IIgnite, StorageMutability, IStorageProxyCacheTransacted<INonSpatialAffinityKey, byte[]>>>()(ignite, Mutability);
      siteModelCache = DIContext.Obtain<Func<IIgnite, StorageMutability, IStorageProxyCacheTransacted<INonSpatialAffinityKey, byte[]>>>()(ignite, Mutability);
    }

    /// <summary>
    /// Commits all unsaved changes in the spatial and non-spatial stores. Each store is committed asynchronously.
    /// </summary>
    /// <returns></returns>
    public override bool Commit(out int numDeleted, out int numUpdated, out long numBytesWritten)
    {
      numDeleted = 0;
      numUpdated = 0;
      numBytesWritten = 0;

      var commitTasks = new List<Task<(int _numDeleted, int _numUpdated, long _numBytesWritten)>>
      {
        Task.Factory.StartNew(() =>
        {
          try
          {
            spatialCache.Commit(out int _numDeleted, out int _numUpdated, out long _numBytesWritten);
            return (_numDeleted, _numUpdated, _numBytesWritten);
          }
          catch (Exception e)
          {
            Log.LogError(e, "Exception thrown committing changes to Ignite for spatial cache");
            throw;
          }
        }),
        Task.Factory.StartNew(() =>
        {
          try
          {
            generalNonSpatialCache.Commit(out int _numDeleted, out int _numUpdated, out long _numBytesWritten);
            return (_numDeleted, _numUpdated, _numBytesWritten);
          }
          catch (Exception e)
          {
            Log.LogError(e, "Exception thrown committing changes to Ignite for general non spatial cache");
            throw;
          }
        }),
        Task.Factory.StartNew(() =>
        {
          try
          {
            siteModelCache.Commit(out int _numDeleted, out int _numUpdated, out long _numBytesWritten);
            return (_numDeleted, _numUpdated, _numBytesWritten);
          }
          catch (Exception e)
          {
            Log.LogError(e, "Exception thrown committing changes to Ignite for site model cache");
            throw;
          }
        })
      };

      var commitResults = commitTasks.WhenAll();
      commitResults.Wait();

      if (commitResults.IsFaulted)
        return false;

      foreach (var (_numDeleted, _numUpdated, _numBytesWritten) in commitResults.Result)
      {
        numDeleted += _numDeleted;
        numUpdated += _numUpdated;
        numBytesWritten += _numBytesWritten;
      }

      return ImmutableProxy?.Commit() ?? true;
    }

    public override bool Commit() => Commit(out _, out _, out _);

    /// <summary>
    /// Clears all changes in the spatial and non spatial stores
    /// </summary>
    public override void Clear()
    {
      try
      {
        spatialCache.Clear();
      }
      catch (Exception e)
      {
        Log.LogError(e, "Exception thrown clearing changes for spatial cache");
        throw;
      }

      try
      {
        generalNonSpatialCache.Clear();
      }
      catch (Exception e)
      {
        Log.LogError(e, "Exception thrown clearing changes for general non spatial cache");
        throw;
      }

      try
      {
        siteModelCache.Clear();
      }
      catch (Exception e)
      {
        Log.LogError(e, "Exception thrown clearing changes for site model cache");
        throw;
      }

      ImmutableProxy?.Clear();
    }
  }
}
