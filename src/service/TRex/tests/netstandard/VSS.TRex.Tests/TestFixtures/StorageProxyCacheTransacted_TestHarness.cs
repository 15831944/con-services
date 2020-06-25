﻿using System.Collections.Generic;
using Apache.Ignite.Core.Cache;
using Moq;
using VSS.TRex.Storage;

namespace VSS.TRex.Tests.TestFixtures
{
  public class StorageProxyCacheTransacted_TestHarness<TK, TV> : StorageProxyCacheTransacted<TK, TV>, IStorageProxyCacheTransacted_TestHarness<TK, TV>
  {
    public StorageProxyCacheTransacted_TestHarness(ICache<TK, TV> cache, IEqualityComparer<TK> comparer) : base(cache, comparer)
    {
    }

    /// <summary>
    /// Expose pending transacted writes in test context to provide simplified access to content in unit tests
    /// </summary>
    public Dictionary<TK, TV> GetPendingTransactedWrites() => PendingTransactedWrites;

    /// <summary>
    /// Override the name property which extracts the cache name from the storage proxy. Test harnesses don;t have a valid cache
    /// reference so just return a dummy name for the cache.
    /// </summary>
    public override string Name => HasCache ? base.Name : "StorageProxyCacheTransacted_TestHarness";

    /// <summary>
    /// Override the commit behaviour to make it a null operation for unit test activities
    /// </summary>
    public override void Commit(out int numDeleted, out int numUpdated, out long numBytesWritten)
    {
      if (HasCache)
      {
        base.Commit(out numDeleted, out numUpdated, out numBytesWritten);
        return;
      }

      // Do nothing on purpose
      numDeleted = 0;
      numUpdated = 0;
      numBytesWritten = 0;
    }

    /// <summary>
    /// Override the look-aside get semantics into the transaction writes so that gets don't read through into the
    /// null cache reference in the underlying base class.
    /// </summary>
    public override TV Get(TK key)
    {
      if (HasCache)
      {
        return base.Get(key);
      }

      lock (PendingTransactedWrites)
      {
        if (PendingTransactedWrites.TryGetValue(key, out var value))
          return value;
      }

      throw new KeyNotFoundException($"Key {key} not found");
    }

    /// <summary>
    /// Suppress clearing content from the test harne4ss storage proxy cache. This is due to the need to mock the
    /// persistence layer in Ignite via the proxied storage cache for the lifecycle of individual tests.
    /// </summary>
    public override void Clear()
    {
      if (HasCache)
      {
        base.Clear();
      }
      else
      {
        // Do nothing - retain the information within the storage proxy for test purposes
      }
    }

    /// <summary>
    /// Returns a mocked ICacheLock for the key
    /// </summary>
    /// <param name="key"></param>
    public override ICacheLock Lock(TK key) => new Mock<ICacheLock>().Object;
  }
}
