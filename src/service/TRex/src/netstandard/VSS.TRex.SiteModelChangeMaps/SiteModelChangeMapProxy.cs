﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Apache.Ignite.Core.Cache;
using VSS.TRex.DI;
using VSS.TRex.GridFabric;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.Types;

namespace VSS.TRex.SiteModelChangeMaps
{
  /// <summary>
  /// Provides a proxy for accessing site model/asset change map information
  /// </summary>
  public class SiteModelChangeMapProxy
  {
    private readonly IStorageProxyCache<ISiteModelMachineAffinityKey, ISerialisedByteArrayWrapper> _proxyStorageCache;

    public SiteModelChangeMapProxy()
    {
      _proxyStorageCache = DIContext.Obtain<ISiteModels>().PrimaryImmutableStorageProxy.ProjectMachineCache(FileSystemStreamType.SiteModelMachineElevationChangeMap);
    }

    public async Task<SubGridTreeSubGridExistenceBitMask> Get(Guid siteModelUid, Guid assetUid)
    {
      try
      {
        var cacheItem = await _proxyStorageCache.GetAsync(new SiteModelMachineAffinityKey(siteModelUid, assetUid, FileSystemStreamType.SiteModelMachineElevationChangeMap));
        var result = new SubGridTreeSubGridExistenceBitMask();

        if (cacheItem != null)
        {
          result.FromBytes(cacheItem.Bytes);
        }

        return result;
      }
      catch (KeyNotFoundException)
      {
        return null;
      }
    }

    public Task Put(Guid siteModelUid, Guid assetUid, SubGridTreeSubGridExistenceBitMask changeMap)
    {
      if (changeMap == null)
      {
        throw new ArgumentException("Change map cannot be null");
      }

      return _proxyStorageCache.PutAsync(new SiteModelMachineAffinityKey(siteModelUid, assetUid, FileSystemStreamType.SiteModelMachineElevationChangeMap), new SerialisedByteArrayWrapper(changeMap.ToBytes()));
    }

    public ICacheLock Lock(Guid siteModelUid, Guid assetUid)
    {
      return _proxyStorageCache.Lock(new SiteModelMachineAffinityKey(siteModelUid, assetUid, FileSystemStreamType.SiteModelMachineElevationChangeMap));
    }

    public ICacheLock Lock(ISiteModelMachineAffinityKey key)
    {
      return _proxyStorageCache.Lock(key);
    }

  }
}
