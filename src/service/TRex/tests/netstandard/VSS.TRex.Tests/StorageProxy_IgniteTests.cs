﻿using VSS.TRex.Storage;
using VSS.TRex.Storage.Models;
using Xunit;

namespace VSS.TRex.Tests
{
    public class StorageProxy_IgniteTests
    {
        [Fact(Skip = "Requires live Ignite node")]
        public void Test_StorageProxy_Ignite_Creation()
        {
            var proxy = new StorageProxy_Ignite(StorageMutability.Immutable);

            Assert.NotNull(proxy);

            proxy = new StorageProxy_Ignite(StorageMutability.Mutable);

            Assert.NotNull(proxy);
        }

        [Fact(Skip = "Not Implemented")]
        public void Test_StorageProxy_Ignite_ReadSpatialStreamFromPersistentStore()
        {
            Assert.True(false);
        }

        [Fact(Skip = "Not Implemented")]
        public void Test_StorageProxy_Ignite_ReadStreamFromPersistentStore()
        {
            Assert.True(false);
        }

        [Fact(Skip = "Not Implemented")]
        public void Test_StorageProxy_Ignite_ReadStreamFromPersistentStoreTest1()
        {
            Assert.True(false);
        }

        [Fact(Skip = "Not Implemented")]
        public void Test_StorageProxy_Ignite_RemoveStreamFromPersistentStore()
        {
            Assert.True(false);
        }

        [Fact(Skip = "Not Implemented")]
        public void Test_StorageProxy_Ignite_WriteSpatialStreamToPersistentStore()
        {
            Assert.True(false);
        }

        [Fact(Skip = "Not Implemented")]
        public void Test_StorageProxy_Ignite_WriteStreamToPersistentStore()
        {
            Assert.True(false);
        }
    }
}
