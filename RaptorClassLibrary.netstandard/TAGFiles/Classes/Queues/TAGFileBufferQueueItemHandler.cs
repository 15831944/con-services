﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using log4net;
using VSS.TRex.GridFabric.Caches;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.TAGFiles.GridFabric.Arguments;
using VSS.TRex.TAGFiles.GridFabric.Requests;
using VSS.TRex.TAGFiles.GridFabric.Responses;

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
//        private EventWaitHandle waitHandle;

        /// <summary>
        /// The grouper responsible for grouping TAG files into Project/Asset groups ready for processing into a
        /// project.
        /// </summary>
        private TAGFileBufferQueueGrouper grouper;

        /// <summary>
        /// The thread providing independent lifecycle activity
        /// </summary>
//        private Thread thread1;
//        private Thread thread2;

        private IIgnite ignite;
        private ICache<TAGFileBufferQueueKey, TAGFileBufferQueueItem> queueCache;

        private List<Guid> ProjectsToAvoid = new List<Guid>();

        private void ProcessTAGFilesFromGrouper()
        {
            Log.Info("ProcessTAGFilesFromGrouper starting executing");

            TAGFileBufferQueueKey removalKey = new TAGFileBufferQueueKey();

            // Cycle looking for new work to do as TAG files arrive until aborted...
            do
            {
                var hadWorkToDo = false;

                // Check to see if there is a work package to feed to the processing pipline
                // -> Ask the grouper for a package 
                var package = grouper.Extract(ProjectsToAvoid, out Guid projectID)?.ToList();
                int packageCount = package?.Count ?? 0;

                if (packageCount > 0)
                {
                    Log.Info($"Extracted package from grouper, ProjectID:{projectID}, with {packageCount} items");

                    hadWorkToDo = true;

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
                                //ProjectUID = TAGQueueItems[0].ProjectUID,
                                ProjectID = projectID,
                                AssetID = TAGQueueItems[0].AssetID,
                                TAGFiles = fileItems
                            });

                            removalKey.ProjectID = projectID;
                            removalKey.AssetID = TAGQueueItems[0].AssetID;

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

                                    removalKey.FileName = tagFileResponse.FileName;

                                    if (!queueCache.Remove(removalKey))
                                    {
                                        Log.Error($"Failed to remove TAG file {removalKey}");
                                    }
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
                        Log.Info($"Thread {Thread.CurrentThread.ManagedThreadId}: About to remove project {projectID} from [{(!ProjectsToAvoid.Any() ? "Empty" : ProjectsToAvoid.Select(x => $"{x}").Aggregate((a, b) => $"{a} + {b}"))}]");
                        grouper.RemoveProjectFromAvoidList(ProjectsToAvoid, projectID);
                    }
                }

                // if there was no work to do in the last epoch, sleep for a bit until the next check epoch
                if (!hadWorkToDo)
                {
                    //Log.Info($"ProcessTAGFilesFromGrouper sleeping for {kTAGFileBufferQueueServiceCheckIntervalMS}ms");

                    Thread.Sleep(kTAGFileBufferQueueServiceCheckIntervalMS);
                    //waitHandle.WaitOne(kTAGFileBufferQueueServiceCheckIntervalMS);
                }
            } while (!aborted);

            Log.Info("ProcessTAGFilesFromGrouper completed executing");
        }

        /// <summary>
        /// Contains the business logic for managing the processing of a package of TAG files into TRex
        /// The package of TAG files contains files for a single project [and currentlyu a single machine]
        /// </summary>
        /// <param name="package"></param>
        private void ProcessTAGFileBucketFromGrouper2(IReadOnlyList<TAGFileBufferQueueKey> package)
        {
            Guid projectID = package[0].ProjectID;

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
                        Log.Error(
                            $"Error, exception {e} occurred while attempting to retrieve TAG file for key {x} from the TAG file buffer queue cache");
                        return null;
                    }
                    catch (Exception e)
                    {
                        // More worrying, report and bail on this package
                        Log.Error(
                            $"Error, exception {e} occurred while attempting to retrieve TAG file for key {x} from the TAG file buffer queue cache - aborting processing this package");
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
                Log.Error(
                    $"Error, exception {e} occurred while attempting to retrieve TAG files from the TAG file buffer queue cache");
            }

            if (TAGQueueItems?.Count > 0)
            {
                // -> Supply the package to the processor
                ProcessTAGFileRequest request = new ProcessTAGFileRequest();
                ProcessTAGFileResponse response = request.Execute(new ProcessTAGFileRequestArgument
                {
                    //ProjectUID = TAGQueueItems[0].ProjectUID,
                    ProjectID = projectID,
                    AssetID = TAGQueueItems[0].AssetID,
                    TAGFiles = fileItems
                });

                TAGFileBufferQueueKey removalKey = new TAGFileBufferQueueKey
                {
                    ProjectID = projectID,
                    AssetID = TAGQueueItems[0].AssetID
                };

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
                            Log.Error(
                                $"TAG file failed to process, with exception {tagFileResponse.Exception}. WARNING: FILE REMOVED FROM QUEUE");

                        removalKey.FileName = tagFileResponse.FileName;

                        if (!queueCache.Remove(removalKey))
                        {
                            Log.Error($"Failed to remove TAG file {removalKey}");
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error(
                            $"Exception {e} occurred while removing TAG file {tagFileResponse.FileName} in project {projectID} from the TAG file buffer queue");
                    }
                }
            }
        }

        /// <summary>
        /// A version of ProcessTAGFilesFromGrouper2 that uses task parallelisation
        /// </summary>
        private void ProcessTAGFilesFromGrouper2()
        {
            try
            {
                Log.Info("ProcessTAGFilesFromGrouper2 starting executing");

                // Cycle looking for new work to do as TAG files arrive until aborted...
                do
                {
                    var hadWorkToDo = false;

                    // Check to see if there is a work package to feed to the processing pipline
                    // -> Ask the grouper for a package 
                    var package = grouper.Extract(ProjectsToAvoid, out Guid projectID)?.ToList();
                    int packageCount = package?.Count ?? 0;

                    if (packageCount > 0)
                    {
                        Log.Info(
                            $"Extracted package from grouper, ProjectID:{projectID}, with {packageCount} items in thread {Thread.CurrentThread.ManagedThreadId}");

                        hadWorkToDo = true;
                        try
                        {
                            //Task task = Task.Factory.StartNew(() => ProcessTAGFileBucketFromGrouper2(package));
                            //task.Wait();

                            Log.Info(
                                $"Start processing {packageCount} TAG files from package in thread {Thread.CurrentThread.ManagedThreadId}");
                            ProcessTAGFileBucketFromGrouper2(package);
                            Log.Info(
                                $"Completed processing {packageCount} TAG files from package in thread {Thread.CurrentThread.ManagedThreadId}");
                        }
                        finally
                        {
                            // Remove the project from the avoid list
                            Log.Info(
                                $"Thread {Thread.CurrentThread.ManagedThreadId}: About to remove project {projectID} from [{(!ProjectsToAvoid.Any() ? "Empty" : ProjectsToAvoid.Select(x => $"{x}").Aggregate((a, b) => $"{a} + {b}"))}]");
                            grouper.RemoveProjectFromAvoidList(ProjectsToAvoid, projectID);
                        }
                    }

                    // if there was no work to do in the last epoch, sleep for a bit until the next check epoch
                    if (!hadWorkToDo)
                    {
                        //Log.Info($"ProcessTAGFilesFromGrouper2 sleeping for {kTAGFileBufferQueueServiceCheckIntervalMS}ms");

                        Thread.Sleep(kTAGFileBufferQueueServiceCheckIntervalMS);
                        //waitHandle.WaitOne(kTAGFileBufferQueueServiceCheckIntervalMS);
                    }
                } while (!aborted);

                Log.Info("ProcessTAGFilesFromGrouper2 completed executing");
            }
            catch (Exception e)
            {
                Log.Error($"Exception {e} thrown in ProcessTAGFilesFromGrouper2");
            }
        }

        /// <summary>
        /// No-arg constructor that creates the intermal grouper, thread and waithandle for managing incoming TAG files
        /// into the cache and supplied by the continuous query
        /// </summary>
        public TAGFileBufferQueueItemHandler()
        {
            ignite = Ignition.GetIgnite(RaptorGrids.RaptorMutableGridName());
            queueCache = ignite.GetCache<TAGFileBufferQueueKey, TAGFileBufferQueueItem>(RaptorCaches.TAGFileBufferQueueCacheName());

            // Create the grouper responsible for grouping TAG files into projecft/asset combinations
            grouper = new TAGFileBufferQueueGrouper();
            // waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

            const int NumTasks = 1;
            Task[] tasks = Enumerable.Range(0, NumTasks).Select(x => Task.Factory.StartNew(ProcessTAGFilesFromGrouper2, TaskCreationOptions.LongRunning)).ToArray();

            //thread1 = new Thread(ProcessTAGFilesFromGrouper2);
            //thread1.Start();
            //thread2 = new Thread(ProcessTAGFilesFromGrouper2);
            //thread2.Start();
        }

        /// <summary>
        /// Adds a new TAG file item from the buffer queue via the remote filter supplied tot he continous query
        /// </summary>
        /// <param name="key"></param>
        public void Add(TAGFileBufferQueueKey key)
        {
            grouper.Add(key);
        }

        public void Dispose()
        {
            aborted = true;
            //waitHandle?.Set();
            //waitHandle?.Dispose();
            //waitHandle = null;
            //thread1?.Abort();
            //thread2?.Abort();
        }
    }
}
