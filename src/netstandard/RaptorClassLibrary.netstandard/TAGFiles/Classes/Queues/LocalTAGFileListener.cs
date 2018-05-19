﻿using System;
using Apache.Ignite.Core.Cache.Event;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace VSS.TRex.TAGFiles.Classes.Queues
{
    public class LocalTAGFileListener : ICacheEntryEventListener<TAGFileBufferQueueKey, TAGFileBufferQueueItem>
    {
        [NonSerialized]
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

        /// <summary>
        /// Event called whenever there are new items in the TAG file buffer queue discovered by the continuous query
        /// Events include creation, modification and deletion of cache entries
        /// </summary>
        /// <param name="evts"></param>
        public void OnEvent(IEnumerable<ICacheEntryEvent<TAGFileBufferQueueKey, TAGFileBufferQueueItem>> evts)
        {
            // Add the keys for the given events into the Project/Asset mapping buckets ready for a processing context
            // to acquire them. 

            // Log.LogInformation("About to add TAG file items to the grouper");
            int countOfCreatedEvents = 0;

            foreach (var evt in evts)
            {
                // Only interested in newly added items to the cache. Updates and deletes are ignored.
                if (evt.EventType != CacheEntryEventType.Created)
                    continue;

                countOfCreatedEvents++;
                try
                {
                    TAGFileBufferQueueItemHandler.Instance().Add(evt.Key /*, evt.Value*/);
                    Log.LogInformation($"Added TAG file item [{evt.Key}] to the grouper");
                }
                catch (Exception e)
                {
                    Log.LogError(
                        $"Exception {e} occurred addign TAG file item {evt.Key} to the grouper");
                }
            }

            if (countOfCreatedEvents > 0)
            {
                Log.LogInformation($"Added {countOfCreatedEvents} TAG file items to the grouper");
            }
        }
    }
}
