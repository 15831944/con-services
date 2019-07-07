﻿using System.Collections.Generic;
using Apache.Ignite.Core.Transactions;

namespace VSS.TRex.Storage.Interfaces
{
    /// <summary>
    /// Defines the subset of Ignite ICache APIs required to support storage proxy semantics in TRex
    /// </summary>
    /// <typeparam name="TK"></typeparam>
    /// <typeparam name="TV"></typeparam>
    public interface IStorageProxyCache<TK, TV>
    {        
        TV Get(TK key);

        bool Remove(TK key);

        void RemoveAll(IEnumerable<TK> key);

        void Put(TK key, TV value);

        void PutAll(IEnumerable<KeyValuePair<TK, TV>> values);

        string Name { get; }

        void Commit();

        void Commit(ITransaction tx);

        void Commit(out int numDeleted, out int numUpdated, out long numBytesWritten);

        void Commit(ITransaction tx, out int numDeleted, out int numUpdated, out long numBytesWritten);

        void Clear();

        void IncrementBytesWritten(long bytesWritten);
    }
}
