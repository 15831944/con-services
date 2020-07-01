﻿using System;
using VSS.TRex.DI;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;

namespace VSS.TRex.Storage
{
  /// <summary>
  ///  StorageProxyFactory is a factory for the storage proxy used in TRex
  /// </summary>
  public class StorageProxyFactory : IStorageProxyFactory
  {
    /// <summary>
    /// Creates the storage proxy to be used. 
    /// This factory method provides access to the mutable grid storage
    /// </summary>
    public IStorageProxy MutableGridStorage()
    {
      var injectedProxyFactory = DIContext.Obtain<Func<StorageMutability, IStorageProxy>>();
      var proxy = injectedProxyFactory?.Invoke(StorageMutability.Mutable) ?? new StorageProxy_Ignite_Transactional(StorageMutability.Mutable);

      // Establish any available immutable storage proxy into the mutable storage proxy to allow transparent
      // promotion of data changes in the mutable data store to the immutable data store.
      proxy.SetImmutableStorageProxy(ImmutableGridStorage());

      return proxy;
    }

    /// <summary>
    /// Creates the storage proxy to be used. 
    /// This factory method provides access to the immutable grid storage
    /// </summary>
    public IStorageProxy ImmutableGridStorage()
    {
      var injectedProxyFactory = DIContext.Obtain<Func<StorageMutability, IStorageProxy>>();
      return injectedProxyFactory?.Invoke(StorageMutability.Immutable) ?? new StorageProxy_Ignite_Transactional(StorageMutability.Immutable);
    }

    /// <summary>
    /// Creates the storage proxy to be used. 
    /// This factory method provides access to the immutable grid storage
    /// </summary>
    public IStorageProxy Storage(StorageMutability mutability) => mutability == StorageMutability.Immutable ? ImmutableGridStorage() : MutableGridStorage();
  }
}
