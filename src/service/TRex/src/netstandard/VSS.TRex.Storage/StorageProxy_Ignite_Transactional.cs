﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using Nito.AsyncEx.Synchronous;
using VSS.Common.Abstractions.Configuration;
using VSS.TRex.Common.Extensions;
using VSS.TRex.DI;
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
    private static readonly ILogger _log = Logging.Logger.CreateLogger<StorageProxy_Ignite_Transactional>();

    private static readonly bool _useAsyncTasksForStorageProxyIgniteTransactionalCommits = DIContext.Obtain<IConfigurationStore>()
      .GetValueBool("USE_SYNC_TASKS_FOR_STORAGE_PROXY_IGNITE_TRANSACTIONAL_COMMITS", true);

    /// <summary>
    /// Constructor that obtains references to the mutable and immutable, spatial and non-spatial caches present in the grid
    /// </summary>
    /// <param name="mutability"></param>
    public StorageProxy_Ignite_Transactional(StorageMutability mutability) : base(mutability)
    {
    }

    private bool CommitAsync(out int numDeleted, out int numUpdated, out long numBytesWritten)
    {
      numDeleted = 0;
      numUpdated = 0;
      numBytesWritten = 0;

      (int, int, long) LocalCommit(IStorageProxyCacheCommit committer)
      {
        try
        {
          var numDeletedLocal = 0;
          var numUpdatedLocal = 0;
          long numBytesWrittenLocal = 0;

          committer?.Commit(out numDeletedLocal, out numUpdatedLocal, out numBytesWrittenLocal);

          return (numDeletedLocal, numUpdatedLocal, numBytesWrittenLocal);
        }
        catch (Exception e)
        {
          _log.LogError(e, $"Exception thrown committing changes to Ignite for {committer?.Name}");
          throw;
        }
      }

      var commitTasks = CommittableCaches.Select(x => Task.Run(() => LocalCommit(x))).ToArray();
      var commitResults = commitTasks.WhenAll();
      commitResults.WaitAndUnwrapException();

      if (commitResults.IsFaulted || commitTasks.Any(x => x.IsFaulted))
      {
        _log.LogError(commitResults.Exception, $"Asynchronous Commit() faulted");
        return false;
      }
      
      foreach (var (numDeletedLocal, numUpdatedLocal, numBytesWrittenLocal) in commitResults.Result)
      {
        numDeleted += numDeletedLocal;
        numUpdated += numUpdatedLocal;
        numBytesWritten += numBytesWrittenLocal;
      }

      return true;
    }

    private bool CommitSync(out int numDeleted, out int numUpdated, out long numBytesWritten)
    {
      var numDeletedLocal = 0;
      var numUpdatedLocal = 0;
      long numBytesWrittenLocal = 0;

      void LocalCommit(IStorageProxyCacheCommit committer)
      {
        try
        {
          var numDeletedInternal = 0;
          var numUpdatedInternal = 0;
          long numBytesWrittenInternal = 0;

          committer?.Commit(out numDeletedInternal, out numUpdatedInternal, out numBytesWrittenInternal);

          numDeletedLocal += numDeletedInternal;
          numUpdatedLocal += numUpdatedInternal;
          numBytesWrittenLocal += numBytesWrittenInternal;
        }
        catch (Exception e)
        {
          _log.LogError(e, $"Exception thrown committing changes to Ignite for {committer?.Name}");
          throw;
        }
      }

      CommittableCaches.ForEach(LocalCommit);

      numDeleted = numDeletedLocal;
      numUpdated = numUpdatedLocal;
      numBytesWritten = numBytesWrittenLocal;
      return true;
    }


    /// <summary>
    /// Commits all unsaved changes in the spatial and non-spatial stores. Each store is committed asynchronously.
    /// </summary>
    /// <returns></returns>
    public override bool Commit(out int numDeleted, out int numUpdated, out long numBytesWritten)
    {
      var commitOk = _useAsyncTasksForStorageProxyIgniteTransactionalCommits
        ? CommitAsync(out numDeleted, out numUpdated, out numBytesWritten)
        : CommitSync(out numDeleted, out numUpdated, out numBytesWritten);

      return commitOk && (ImmutableProxy?.Commit() ?? true);
    }

    public override bool Commit() => Commit(out _, out _, out _);

    /// <summary>
    /// Clears all changes in the spatial and non spatial stores
    /// </summary>
    public override void Clear()
    {
      void LocalClear(IStorageProxyCacheCommit committer)
      {
        try
        {
          committer?.Clear();
        }
        catch
        {
          _log.LogError($"Exception thrown clearing changes for cache {committer?.Name}");
          throw;
        }
      }

      CommittableCaches.ForEach(LocalClear);
      ImmutableProxy?.Clear();
    }
  }
}
