﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using log4net;
using VSS.VisionLink.Raptor.GridFabric.Caches;
using VSS.VisionLink.Raptor.GridFabric.Grids;
using VSS.VisionLink.Raptor.TAGFiles.GridFabric.Arguments;
using VSS.VisionLink.Raptor.TAGFiles.GridFabric.Requests;
using VSS.VisionLink.Raptor.TAGFiles.GridFabric.Responses;

namespace VSS.TRex.TAGFiles.Classes.Queues
{
    public class TAGFileBufferQueueItemHandler : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static TAGFileBufferQueueItemHandler _Instance;

        public static TAGFileBufferQueueItemHandler Instance() => _Instance ?? (_Instance = new TAGFileBufferQueueItemHandler());

        /// <summary>
        /// The interval between epochs where the service checks to see if there is anything to do
        /// </summary>
        private const int kTAGFileBufferQueueServiceCheckIntervalMS = 1000;

        /// <summary>
        /// Flag set then Cancel() is called to instruct the service to finish operations
        /// </summary>
        private bool aborted;

        /// <summary>
        /// The event wait handle used to mediate sleep periods between operation epochs of the service
        /// </summary>
        private EventWaitHandle waitHandle;

        /// <summary>
        /// The grouper responsible for grouping TAG files into Project/Asset groups ready for processing into a
        /// project.
        /// </summary>
        private TAGFileBufferQueueGrouper grouper;

        /// <summary>
        /// The thread providing independent lifecycle activity
        /// </summary>
        private Thread thread;

        private void ProcessTAGFilesFromGrouper()
        {
            Log.Info("ProcessTAGFilesFromGrouper starting executing");

            List<long> ProjectsToAvoid = new List<long>();

            // Get the ignite grid and cache references
            IIgnite ignite = Ignition.GetIgnite(RaptorGrids.RaptorMutableGridName());
            ICache<TAGFileBufferQueueKey, TAGFileBufferQueueItem> queueCache =
                ignite.GetCache<TAGFileBufferQueueKey, TAGFileBufferQueueItem>(
                    RaptorCaches.TAGFileBufferQueueCacheName());

            // Cycle looking for new work to do as TAG files arrive until aborted...
            do
            {
                var hadWorkToDo = false;

                // Check to see if there is a work package to feed to the processing pipline
                // -> Ask the grouper for a package 
                var package = grouper.Extract(ProjectsToAvoid, out long projectID)?.ToList();

                Log.Info($"Extracted package from grouper, ProjectID:{projectID}, with {package?.Count} items");

                if (package?.Count > 0)
                {
                    hadWorkToDo = true;

                    // Add the project to the avoid list
                    ProjectsToAvoid.Add(projectID);

                    try
                    {
                        List<TAGFileBufferQueueItem> TAGQueueItems = null;
                        List<ProcessTAGFileRequestFileItem> fileItems = null;
                        try
                        {
                            TAGQueueItems = package.Select(x =>
                            {
                                try
                                {
                                    return queueCache.Get(x);
                                }
                                catch (KeyNotFoundException e)
                                {
                                    // Odd, but let's be graceful and attempt to process the remainder in the package
                                    Log.Error($"Error, exception {e} occurred while attempting to retrieve TAG file for key {x} from the TAG file buffer queue cache");
                                    return null;
                                }
                                catch (Exception e)
                                {
                                    // More worrying, report and bail on this package
                                    Log.Error($"Error, exception {e} occurred while attempting to retrieve TAG file for key {x} from the TAG file buffer queue cache - aborting processing this package");
                                    throw;
                                }
                            }).ToList();
                            fileItems = TAGQueueItems
                                .Where(x => x != null)
                                .Select(x => new ProcessTAGFileRequestFileItem
                                {
                                    FileName = x.FileName,
                                    TagFileContent = x.Content,
                                }).ToList();
                        }
                        catch (Exception e)
                        {
                            Log.Error($"Error, exception {e} occurred while attempting to retrieve TAG files from the TAG file buffer queue cache");
                        }

                        if (TAGQueueItems?.Count > 0)
                        {
                            // -> Supply the package to the processor
                            ProcessTAGFileRequest request = new ProcessTAGFileRequest();
                            ProcessTAGFileResponse response = request.Execute(new ProcessTAGFileRequestArgument
                            {
                                //AssetUID = TAGQueueItems[0].AssetUID,
                                //ProjectUID = TAGQueueItems[0].ProjectUID,
                                AssetID = TAGQueueItems[0].AssetID,
                                ProjectID = TAGQueueItems[0].ProjectID,
                                TAGFiles = fileItems
                            });

                            // -> Remove the set of processed TAG files from the buffer queue cache (depending on processing status?...)
                            foreach (var tagFileResponse in response.Results)
                            {
                                try
                                {
                                    // TODO: Determine what to do in this failure more:
                                    // TODO: - Leave in place?
                                    // TODO: - Copy to dead letter queue?
                                    // TODO: - Place in S3 bucket pending downstream handling?
                                    if (!tagFileResponse.Success)
                                        Log.Error($"TAG file failed to process, with exception {tagFileResponse.Exception}. WARNING: FILE REMOVED FROM QUEUE");

                                    queueCache.Remove(new TAGFileBufferQueueKey
                                    {
                                        ProjectID = projectID,
                                        FileName = tagFileResponse.FileName
                                    });
                                }
                                catch (Exception e)
                                {
                                    Log.Error(
                                        $"Exception {e} occurred while removing TAG file {tagFileResponse.FileName} in project {projectID} from the TAG file buffer queue");
                                }
                            }
                        }
                    }
                    finally
                    {
                        // Remove the project from the avoid list
                        ProjectsToAvoid.Remove(projectID);
                    }
                }

                // if there was no work to do in the last epoch, sleep for a bit until the next check epoch
                if (!hadWorkToDo)
                {
                    Log.Info($"ProcessTAGFilesFromGrouper sleeping for {kTAGFileBufferQueueServiceCheckIntervalMS}ms");

                    waitHandle.WaitOne(kTAGFileBufferQueueServiceCheckIntervalMS);
                }
            } while (!aborted);

            Log.Info("ProcessTAGFilesFromGrouper completed executing");
        }

        /// <summary>
        /// No-arg constructor that creates the intermal grouper, thread and waithandle for managing incoming TAG files
        /// into the cache and supplied by the continuous query
        /// </summary>
        public TAGFileBufferQueueItemHandler()
        {
            // Create the grouper responsible for grouping TAG files into projecft/asset combinations
            grouper = new TAGFileBufferQueueGrouper();
            waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            thread = new Thread(ProcessTAGFilesFromGrouper);
            thread.Start();
        }

        /// <summary>
        /// Adds a new TAG file item from the buffer queue via the remote filter supplied tot he continous query
        /// </summary>
        /// <param name="key"></param>
        public void Add(TAGFileBufferQueueKey key /*, TAGFileBufferQueueItem value*/)
        {
            grouper.Add(key /*, value*/);
        }

        public void Dispose()
        {
            aborted = true;
            waitHandle?.Set();
            waitHandle?.Dispose();
            waitHandle = null;
            thread.Abort();
        }
    }
}
