﻿using Apache.Ignite.Core;
using VSS.TRex.TAGFiles.GridFabric.Services;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Servers.Client;
using Xunit;

namespace TAGFiles.Tests
{
    /// <summary>
    /// Tests to ensure the grid deployed service that takes TAG files in the buffer queue and sends them to the grouper 
    /// functions as expected.
    /// </summary>
    public class TAGFileBufferQueueServiceTests
    {
        private static MutableClientServer TAGClientServer;
        private static IIgnite ignite;

        private static void EnsureServer()
        {
            try
            {
                ignite = Ignition.GetIgnite(TRexGrids.MutableGridName());
            }
            catch
            {
                TAGClientServer = TAGClientServer ?? new MutableClientServer(ServerRoles.TAG_PROCESSING_NODE_CLIENT);
                ignite = Ignition.GetIgnite(TRexGrids.MutableGridName());
            }
        }

        [Fact(Skip = "Requires live Ignite node")]
        public void Test_TAGFileBufferQueueServiceTests_Creation()
        {
            EnsureServer();

            TAGFileBufferQueueServiceProxy serviceProxy = new TAGFileBufferQueueServiceProxy();

            Assert.True(serviceProxy != null);
        }

        [Fact(Skip = "Requires live Ignite node")]
        public void Test_TAGFileBufferQueueServiceTests_Deploying()
        {
            EnsureServer();

            TAGFileBufferQueueServiceProxy serviceProxy = new TAGFileBufferQueueServiceProxy();
            serviceProxy.Deploy();

            Assert.True(true);
        }
    }
}
