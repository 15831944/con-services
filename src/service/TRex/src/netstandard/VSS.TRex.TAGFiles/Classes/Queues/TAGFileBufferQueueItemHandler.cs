﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Serilog.Extensions;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.SiteModels.Interfaces.Executors;
using VSS.TRex.Storage.Caches;
using VSS.TRex.Storage.Models;
using VSS.TRex.TAGFiles.GridFabric.Arguments;
using VSS.TRex.TAGFiles.GridFabric.Requests;
using VSS.TRex.TAGFiles.Models;

namespace VSS.TRex.TAGFiles.Classes.Queues
{
    public class TAGFileBufferQueueItemHandler : IDisposable
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger<TAGFileBufferQueueItemHandler>();

        /// <summary>
        /// The interval between epochs where the service checks to see if there is anything to do
        /// </summary>
        private const int QueueServiceCheckIntervalMs = 1000;

        private const int DefaultNumConcurrentTagFileProcessingTasks = 4;

        /// <summary>
        /// Flag set then Cancel() is called to instruct the service to finish operations
        /// </summary>
        private bool _aborted;

        /// <summary>
        /// The grouper responsible for grouping TAG files into Project/Asset groups ready for processing into a
        /// project.
        /// </summary>
        private readonly TAGFileBufferQueueGrouper _grouper;

        private readonly IIgnite _ignite;
        private readonly ICache<ITAGFileBufferQueueKey, TAGFileBufferQueueItem> _queueCache;

        private readonly List<Guid> _projectsToAvoid = new List<Guid>();

        private static readonly int NumConcurrentProcessingTasks = DIContext.Obtain<IConfigurationStore>().GetValueInt("NUM_CONCURRENT_TAG_FILE_PROCESSING_TASKS", DefaultNumConcurrentTagFileProcessingTasks);

        private static readonly TAGFileNameComparer TagFileNameComparer = new TAGFileNameComparer();

        private async Task ProcessTAGFilesFromGrouper()
        {
            Log.LogInformation("ProcessTAGFilesFromGrouper starting executing");

            ITAGFileBufferQueueKey removalKey = new TAGFileBufferQueueKey();

            // Cycle looking for new work to do as TAG files arrive until aborted...
            do
            {
                var hadWorkToDo = false;

                // Check to see if there is a work package to feed to the processing pipeline
                // -> Ask the grouper for a package 
                var package = _grouper.Extract(_projectsToAvoid, out var projectId)?.ToList();
                var packageCount = package?.Count ?? 0;

                if (packageCount > 0)
                {
                    Log.LogInformation($"Extracted package from grouper, ProjectUID:{projectId}, with {packageCount} items");

                    hadWorkToDo = true;

                    try
                    {
                        List<TAGFileBufferQueueItem> tagQueueItems = null;
                        List<ProcessTAGFileRequestFileItem> fileItems = null;
                        try
                        {
                            tagQueueItems = package?.Select(x =>
                            {
                                try
                                {
                                    return _queueCache.Get(x);
                                }
                                catch (KeyNotFoundException)
                                {
                                    // Odd, but let's be graceful and attempt to process the remainder in the package
                                    Log.LogError($"Error, KeyNotFoundException exception occurred while attempting to retrieve TAG file for key {x} from the TAG file buffer queue cache");
                                    return null;
                                }
                                catch (Exception e)
                                {
                                    // More worrying, report and bail on this package
                                    Log.LogError(e, $"Error, exception occurred while attempting to retrieve TAG file for key {x} from the TAG file buffer queue cache - aborting processing this package");
                                    throw;
                                }
                            }).ToList();

                            fileItems = tagQueueItems?
                                .Where(x => x != null)
                                .Select(x => new ProcessTAGFileRequestFileItem
                                {
                                    FileName = x.FileName,
                                    TagFileContent = x.Content,
                                    IsJohnDoe = x.IsJohnDoe,
                                    AssetId = x.AssetID,
                                    SubmissionFlags = x.SubmissionFlags
                                }).ToList();
                        }
                        catch (Exception e)
                        {
                            Log.LogError(e, "Error, exception occurred while attempting to retrieve TAG files from the TAG file buffer queue cache");
                        }

                        if (tagQueueItems?.Count > 0)
                        {
                            // -> Supply the package to the processor
                            var request = new ProcessTAGFileRequest();
                            var response = await request.ExecuteAsync(new ProcessTAGFileRequestArgument
                            {
                                ProjectID = projectId,
                                TAGFiles = fileItems
                            });

                            removalKey.ProjectUID = projectId;

                            // -> Remove the set of processed TAG files from the buffer queue cache (depending on processing status?...)
                            foreach (var tagFileResponse in response.Results)
                            {
                                try
                                {
                                    if (tagFileResponse.Success)
                                    {
                                        //Commented out to keep happy path log less noisy
                                        Log.LogInformation($"Grouper1 TAG file {tagFileResponse.FileName} successfully processed");
                                    }
                                    else
                                    {
                                        Log.LogError($"Grouper1 TAG file failed to process, with exception {tagFileResponse.Exception}, read result = {tagFileResponse.ReadResult}. WARNING: FILE REMOVED FROM QUEUE");
                                        // TODO: Determine what to do in this failure mode: Leave in place? Copy to dead letter queue? Place in S3 bucket pending downstream handling?

                                    }

                                    removalKey.FileName = tagFileResponse.FileName;
                                    removalKey.AssetUID = tagFileResponse.AssetUid;

                                    if (!_queueCache.Remove(removalKey))
                                    {
                                        Log.LogError($"Failed to remove TAG file {removalKey}");
                                    }
                                    else
                                    {
                                        Log.LogError($"Successfully removed TAG file {removalKey}");
                                    }
                                }
                                catch (Exception e)
                                {
                                    Log.LogError(e, $"Exception occurred while removing TAG file {tagFileResponse.FileName} in project {projectId} from the TAG file buffer queue");
                                }
                            }
                        }
                    }
                    finally
                    {
                        // Remove the project from the avoid list
                        Log.LogInformation($"Thread {Thread.CurrentThread.ManagedThreadId}: About to remove project {projectId} from [{(!_projectsToAvoid.Any() ? "Empty" : _projectsToAvoid.Select(x => $"{x}").Aggregate((a, b) => $"{a} + {b}"))}]");
                        _grouper.RemoveProjectFromAvoidList(_projectsToAvoid, projectId);
                    }
                }

                // if there was no work to do in the last epoch, sleep for a bit until the next check epoch
                if (!hadWorkToDo)
                {
                    if (Log.IsTraceEnabled())
                      Log.LogInformation($"ProcessTAGFilesFromGrouper sleeping for {QueueServiceCheckIntervalMs}ms");

                    Thread.Sleep(QueueServiceCheckIntervalMs);
                }
            } while (!_aborted);

            Log.LogInformation("ProcessTAGFilesFromGrouper completed executing");
        }

        /// <summary>
        /// Contains the business logic for managing the processing of a package of TAG files into TRex
        /// The package of TAG files contains files for a single project
        /// </summary>
        private async Task ProcessTAGFileBucketFromGrouper2(IReadOnlyList<ITAGFileBufferQueueKey> package)
        {
            var projectId = package[0].ProjectUID;

            List<TAGFileBufferQueueItem> tagQueueItems = null;
            List<ProcessTAGFileRequestFileItem> fileItems = null;
            try
            {
                tagQueueItems = package.Select(x =>
                {
                    try
                    {
                        return _queueCache.Get(x);
                    }
                    catch (KeyNotFoundException)
                    {
                        // Odd, but let's be graceful and attempt to process the remainder in the package
                        Log.LogError($"Error, KeyNotFoundException exception occurred while attempting to retrieve TAG file for key {x} from the TAG file buffer queue cache");
                        return null;
                    }
                    catch (Exception e)
                    {
                        // More worrying, report and bail on this package
                        Log.LogError(e, $"Error, exception occurred while attempting to retrieve TAG file for key {x} from the TAG file buffer queue cache - aborting processing this package");
                        throw;
                    }
                }).ToList();

                fileItems = tagQueueItems
                    .Where(x => x != null)
                    .Select(x => new ProcessTAGFileRequestFileItem
                    {
                        FileName = x.FileName,
                        TagFileContent = x.Content,
                        IsJohnDoe = x.IsJohnDoe,
                        AssetId = x.AssetID,
                        SubmissionFlags = x.SubmissionFlags
                    })
                    .OrderBy(x => x.FileName, TagFileNameComparer)
                    .ToList();
            }
            catch (Exception e)
            {
                Log.LogError(e, "Error, exception occurred while attempting to retrieve TAG files from the TAG file buffer queue cache");
            }

            try
            {
                if (tagQueueItems?.Count > 0)
                {
                    // Log.LogInformation($"Submitting group of {tagQueueItems.Count} tag files for machine {tagQueueItems[0].AssetID} in project {projectId}");

                    // -> Supply the package to the processor
                    var request = new ProcessTAGFileRequest();
                    var response = await request.ExecuteAsync(new ProcessTAGFileRequestArgument
                    {
                        ProjectID = projectId,
                        TAGFiles = fileItems
                    });
           
                    ITAGFileBufferQueueKey removalKey = new TAGFileBufferQueueKey
                    {
                        ProjectUID = projectId
                    };

                    // -> Perform any required notifications requested in the TAGFileSubmissionRequests
                    var notifier = DIContext.Obtain<IRebuildSiteModelTAGNotifier>();
                    if (response.Results.Any(x => x.SubmissionFlags.HasFlag(TAGFileSubmissionFlags.NotifyRebuilderOnProceesing)))
                    {
                       notifier.TAGFileProcessed(projectId, response.Results.ToArray());
                    }

                    // -> Remove the set of processed TAG files from the buffer queue cache (depending on processing status?...)
                    foreach (var tagFileResponse in response.Results)
                    {
                        try
                        {
                            if (tagFileResponse.Success)
                            {
                              if (Log.IsTraceEnabled())
                                Log.LogInformation($"Grouper2 TAG file {tagFileResponse.FileName} successfully processed");
                            }
                            else
                            {
                              // TODO: Determine what to do in this failure mode: Leave in place? Copy to dead letter queue? Place in S3 bucket pending downstream handling?
                              Log.LogError($"Grouper2 TAG file {tagFileResponse.FileName} failed to process, with exception '{tagFileResponse.Exception}', read result = {tagFileResponse.ReadResult}. WARNING: FILE REMOVED FROM QUEUE");
                            }
           
                            removalKey.FileName = tagFileResponse.FileName;
                            removalKey.AssetUID = tagFileResponse.AssetUid;

                            if (!_queueCache.Remove(removalKey))
                              Log.LogError($"Failed to remove TAG file {removalKey}");
                            else
                              if (Log.IsTraceEnabled())
                                 Log.LogInformation($"Successfully removed TAG file {removalKey}");
                        }
                        catch (Exception e)
                        {
                            Log.LogError(e, $"Exception occurred while removing TAG file {tagFileResponse.FileName} in project {projectId} from the TAG file buffer queue");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.LogError(e, $"Exception occurred while submitting TAG file processing requests for project {projectId} from the TAG file buffer queue");
            }
        }
        
        /// <summary>
        /// A version of ProcessTAGFilesFromGrouper2 that uses task parallelism
        /// </summary>
        private async Task ProcessTAGFilesFromGrouper2()
        {
            try
            {
                Log.LogInformation("#In# ProcessTAGFilesFromGrouper2 starting executing");

                // Cycle looking for new work to do as TAG files arrive until aborted...
                do
                {
                    var hadWorkToDo = false;

                    // Check to see if there is a work package to feed to the processing pipeline
                    // -> Ask the grouper for a package 
                    var package = _grouper.Extract(_projectsToAvoid, out var projectId)?.ToList();
                    var packageCount = package?.Count ?? 0;

                    if (packageCount > 0)
                    {
                        Log.LogInformation(
                            $"Extracted package from grouper, ProjectUID:{projectId}, with {packageCount} items in thread {Thread.CurrentThread.ManagedThreadId}");

                        hadWorkToDo = true;
                        try
                        {
                            Log.LogInformation(
                                $"#Progress# Start processing {packageCount} TAG files from package in thread {Thread.CurrentThread.ManagedThreadId}");
                            await ProcessTAGFileBucketFromGrouper2(package);
                            Log.LogInformation(
                                $"#Progress# Completed processing {packageCount} TAG files from package in thread {Thread.CurrentThread.ManagedThreadId}");
                        }
                        finally
                        {
                            // Remove the project from the avoid list
                            Log.LogInformation(
                                $"#Progress# Thread {Thread.CurrentThread.ManagedThreadId}: About to remove project {projectId} from [{(!_projectsToAvoid.Any() ? "Empty" : _projectsToAvoid.Select(x => $"{x}").Aggregate((a, b) => $"{a} + {b}"))}]");
                            _grouper.RemoveProjectFromAvoidList(_projectsToAvoid, projectId);
                        }
                    }

                    // if there was no work to do in the last epoch, sleep for a bit until the next check epoch
                    if (!hadWorkToDo)
                    {
                        Thread.Sleep(QueueServiceCheckIntervalMs);
                    }
                } while (!_aborted);

                Log.LogInformation("#Out# ProcessTAGFilesFromGrouper2 completed executing");
            }
            catch (Exception e)
            {
                Log.LogError(e, "Exception thrown in ProcessTAGFilesFromGrouper2");
            }
        }

        /// <summary>
        /// No-arg constructor that creates the internal grouper, thread and wait handle for managing incoming TAG files
        /// into the cache and supplied by the continuous query
        /// </summary>
        public TAGFileBufferQueueItemHandler()
        {
            _ignite = DIContext.Obtain<ITRexGridFactory>()?.Grid(StorageMutability.Mutable) ?? Ignition.GetIgnite(TRexGrids.MutableGridName());
            _queueCache = _ignite.GetCache<ITAGFileBufferQueueKey, TAGFileBufferQueueItem>(TRexCaches.TAGFileBufferQueueCacheName());

            // Create the grouper responsible for grouping TAG files into project/asset combinations
            _grouper = new TAGFileBufferQueueGrouper();

            // Note ToArray at end is important to activate tasks (ie: lazy loading)
            var _ = Enumerable.Range(0, NumConcurrentProcessingTasks).Select(x => Task.Factory.StartNew(ProcessTAGFilesFromGrouper2, TaskCreationOptions.LongRunning)).ToArray();
        }

        /// <summary>
        /// Adds a new TAG file item from the buffer queue via the remote filter supplied tot he continuous query
        /// </summary>
        /// <param name="key"></param>
        public void Add(ITAGFileBufferQueueKey key)
        {
            _grouper.Add(key);
        }

        public void Dispose()
        {
            _aborted = true;
        }
    }
}
