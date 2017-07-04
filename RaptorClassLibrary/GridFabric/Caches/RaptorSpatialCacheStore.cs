﻿using Apache.Ignite.Core.Cache.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.GridFabric.Affinity;

namespace VSS.VisionLink.Raptor.GridFabric.Caches
{
    public class RaptorSpatialCacheStore : CacheStoreAdapter<SubGridSpatialAffinityKey, MemoryStream>
    {
        private RaptorCacheStoreUtilities Utilities = null;

        public RaptorSpatialCacheStore(string mutabilitySuffix) : base()
        {
            Utilities = new RaptorCacheStoreUtilities(mutabilitySuffix);
        }

        public override void Delete(SubGridSpatialAffinityKey key)
        {
            Utilities.Delete(key.ToString());
        }

        public override MemoryStream Load(SubGridSpatialAffinityKey key)
        {
            return Utilities.Load(key.ToString());
        }

        public override void LoadCache(Action<SubGridSpatialAffinityKey, MemoryStream> act, params object[] args)
        {
            // Ignore - not a supported activity
            // throw new NotImplementedException();
        }

        public override void SessionEnd(bool commit)
        {
            // Ignore, nothign to do
            // throw new NotImplementedException();
        }

        public override void Write(SubGridSpatialAffinityKey key, MemoryStream val)
        {
            Utilities.Write(key.ToString(), val);
        }
    }
}
