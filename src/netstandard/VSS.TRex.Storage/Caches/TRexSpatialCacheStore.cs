﻿using Apache.Ignite.Core.Cache.Store;
using System;
using System.IO;
using VSS.TRex.GridFabric.Interfaces;

namespace VSS.TRex.Storage.Caches
{
    public class TRexSpatialCacheStore : CacheStoreAdapter<ISubGridSpatialAffinityKey, MemoryStream>
    {
        private TRexCacheStoreUtilities Utilities;

        public TRexSpatialCacheStore(string mutabilitySuffix)
        {
            Utilities = new TRexCacheStoreUtilities(mutabilitySuffix);
        }

        public override void Delete(ISubGridSpatialAffinityKey key)
        {
            Utilities.Delete(key.ToString());
        }

        public override MemoryStream Load(ISubGridSpatialAffinityKey key)
        {
            return Utilities.Load(key.ToString());
        }

        public override void LoadCache(Action<ISubGridSpatialAffinityKey, MemoryStream> act, params object[] args)
        {
            // Ignore - not a supported activity
            // throw new NotImplementedException();
        }

        public override void SessionEnd(bool commit)
        {
            // Ignore, nothing to do
            // throw new NotImplementedException();
        }

        public override void Write(ISubGridSpatialAffinityKey key, MemoryStream val)
        {
            Utilities.Write(key.ToString(), val);
        }
    }
}
