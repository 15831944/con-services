﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.DI;
using VSS.TRex.Events;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SiteModels.Interfaces.Events;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.TAGFiles.Classes.Queues;
using VSS.TRex.TAGFiles.Models;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.Integrator
{
  public class AggregatedDataIntegratorWorker
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<AggregatedDataIntegratorWorker>();

    /// <summary>
    /// A queue of the tasks this worker will process into the TRex data stores
    /// </summary>
    private readonly ConcurrentQueue<AggregatedDataIntegratorTask> _tasksToProcess;

    /// <summary>
    /// A bitmask sub grid tree that tracks all sub grids modified by the tasks this worker has processed
    /// </summary>
    private ISubGridTreeBitMask _workingModelUpdateMap;

    /// <summary>
    /// The mutable grid storage proxy
    /// </summary>
    private readonly IStorageProxy _storageProxyMutable = DIContext.Obtain<IStorageProxyFactory>().MutableGridStorage();
    private readonly IStorageProxy _storageProxyMutableForSubGrids = DIContext.Obtain<IStorageProxyFactory>().MutableGridStorage();
    private readonly IStorageProxy _storageProxyMutableForSubGridSegments = DIContext.Obtain<IStorageProxyFactory>().MutableGridStorage();

    public bool AdviseOtherServicesOfDataModelChanges = DIContext.Obtain<IConfigurationStore>().GetValueBool("ADVISE_OTHER_SERVICES_OF_MODEL_CHANGES", Consts.ADVISE_OTHER_SERVICES_OF_MODEL_CHANGES);

    public int MaxMappedTagFilesToProcessPerAggregationEpoch = DIContext.Obtain<IConfigurationStore>().GetValueInt("MAX_MAPPED_TAG_FILES_TO_PROCESS_PER_AGGREGATION_EPOCH", Consts.MAX_MAPPED_TAG_FILES_TO_PROCESS_PER_AGGREGATION_EPOCH);

    private Guid SiteModelID { get; }

    private AggregatedDataIntegratorWorker(Guid siteModelId)
    {
      SiteModelID = siteModelId;
    }

    /// <summary>
    /// Worker constructor accepting the list of tasks for it to process.
    /// The tasks in the tasksToProcess list contain TAG files relating to a single machine's activities
    /// within a single project
    /// </summary>
    /// <param name="tasksToProcess"></param>
    /// <param name="siteModelId"></param>
    public AggregatedDataIntegratorWorker(ConcurrentQueue<AggregatedDataIntegratorTask> tasksToProcess,
      Guid siteModelId) : this(siteModelId)
    {
      _tasksToProcess = tasksToProcess;
    }

    /// <summary>
    /// Event that records a particular sub grid has been modified, identified by the address of a 
    /// cell within that sub grid
    /// </summary>
    /// <param name="cellX"></param>
    /// <param name="cellY"></param>
    private void SubGridHasChanged(int cellX, int cellY)
    {
      _workingModelUpdateMap.SetCell(cellX >> SubGridTreeConsts.SubGridIndexBitsPerLevel,
        cellY >> SubGridTreeConsts.SubGridIndexBitsPerLevel, true);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="task">The 'seed' task used as a template for locating matching tasks</param>
    private ISiteModel ObtainSiteModel(AggregatedDataIntegratorTask task)
    {
      // Note: This request for the SiteModel specifically asks for the mutable grid SiteModel,
      // and also explicitly provides the transactional storage proxy being used for processing the
      // data from TAG files into the model
      var siteModelFromDatamodel = DIContext.Obtain<ISiteModels>().GetSiteModel(task.PersistedTargetSiteModelID, true);

      siteModelFromDatamodel?.SetStorageRepresentationToSupply(StorageMutability.Mutable);

      return siteModelFromDatamodel;
    }

    /// <summary>
    /// Assembles a group of TAG files to be processed in a single operation
    /// </summary>
    /// <param name="processedTasks">The collection of tasks being collated ready for aggregation</param>
    /// <param name="task">The 'seed' task used as a template for locating matching tasks</param>
    private void AssembleGroupedTagFiles(List<AggregatedDataIntegratorTask> processedTasks,
      AggregatedDataIntegratorTask task)
    {
      _log.LogInformation("Aggregation Task Process --> Filter tasks to aggregate");

      // Populate the tasks to process list with the aggregations that will be
      // processed at this time. These tasks are also removed from the main task
      // list to allow the TAG file processors to prepare additional TAG files
      // while this set is being integrated into the model.                    

      while (processedTasks.Count < MaxMappedTagFilesToProcessPerAggregationEpoch &&
             _tasksToProcess.TryDequeue(out var taskToProcess))
      {
        processedTasks.Add(taskToProcess);
      }

      _log.LogInformation($"Aggregation Task Process --> Integrating {processedTasks.Count} TAG file processing tasks for project {task.PersistedTargetSiteModelID}");
    }

    /// <summary>
    /// Group all provided sub grid trees containing TAG file swathing results into a single aggregate sub grid tree
    /// </summary>
    /// <param name="processedTasks">The collated set tasks ready for aggregation</param>
    /// <param name="task">The 'seed' task used as a template for locating matching tasks</param>
    /// <returns>The sub grid tree containing all processed TAG file cell passes ready for aggregation</returns>
    private IServerSubGridTree GroupSwathedSubGridTrees(List<AggregatedDataIntegratorTask> processedTasks,
      AggregatedDataIntegratorTask task)
    {
      // Use the grouped sub grid tree integrator to assemble a single aggregate tree from the set of trees in the processed tasks

      _log.LogDebug($"Aggregation Task Process --> Integrate {processedTasks.Count} cell pass trees");

      IServerSubGridTree groupedAggregatedCellPasses;
      if (processedTasks.Count > 1)
      {
        var subGridTreeIntegrator = new GroupedSubGridTreeIntegrator
        {
          Trees = processedTasks
            .Where(t => t.AggregatedCellPassCount > 0)
            .Select(t => (t.AggregatedCellPasses, DateTime.MinValue, DateTime.MaxValue))
            .ToList()
        };

        // Assign the new grid into Task to represent the spatial aggregation of all of the tasks aggregated cell passes
        groupedAggregatedCellPasses = subGridTreeIntegrator.IntegrateSubGridTreeGroup();
      }
      else
      {
        groupedAggregatedCellPasses = task.AggregatedCellPasses;
      }

#if CELLDEBUG
        groupedAggregatedCellPasses?.ScanAllSubGrids(leaf =>
        {
          foreach (var segment in ((ServerSubGridTreeLeaf)leaf).Cells.PassesData.Items)
          {
            foreach (var cell in segment.PassesData.GetState())
              cell.CheckPassesAreInCorrectTimeOrder("Cell passes not in correct order at point groupedAggregatedCellPasses is determined"); 
          }

          return true;
        });
#endif

      // Discard all the aggregated cell pass models for the tasks being processed as they have now been aggregated into
      // the model represented by groupedAggregatedCellPasses

      _log.LogDebug("Aggregation Task Process --> Clean up cell pass trees");

      processedTasks.ForEach(x =>
      {
        if (x.AggregatedCellPasses != task.AggregatedCellPasses)
        {
          x.AggregatedCellPasses.Dispose();
          x.AggregatedCellPasses = null;
        }
      });

      return groupedAggregatedCellPasses;
    }


    /// <summary>
    /// Aggregates all the machine events from the processed TAG files into a single collection, and
    /// maintains last known values for location, hardware ID and machine type
    /// </summary>
    /// <param name="processedTasks">The collated set tasks ready for aggregation</param>
    /// <param name="task">The 'seed' task used as a hold all for aggregated machines</param>
    private EventIntegrator AggregateAllMachineEvents(List<AggregatedDataIntegratorTask> processedTasks,
      AggregatedDataIntegratorTask task)
    {
      _log.LogDebug("Aggregation Task Process --> Aggregate machine events");

      var eventIntegrator = new EventIntegrator();

      // Iterate through the tasks to integrate the machine events
      for (var i = 1; i < processedTasks.Count; i++) // Zeroth item in the list is Task
      {
        var processedTask = processedTasks[i];

        // 'Include' the extents etc of each site model being merged into 'task' into its extents and design change events
        task.IntermediaryTargetSiteModel.Include(processedTask.IntermediaryTargetSiteModel);

        // Iterate over all the machine events collected in the task
        foreach (var machine in processedTask.IntermediaryTargetMachines)
        {
          // Integrate the machine events
          eventIntegrator.IntegrateMachineEvents
          (processedTask.AggregatedMachineEvents[machine.InternalSiteModelMachineIndex],
            task.AggregatedMachineEvents[machine.InternalSiteModelMachineIndex], false,
            processedTask.IntermediaryTargetSiteModel, task.IntermediaryTargetSiteModel);

          //Update current DateTime with the latest one
          if (machine.LastKnownPositionTimeStamp.CompareTo(machine.LastKnownPositionTimeStamp) == -1)
          {
            machine.LastKnownPositionTimeStamp = machine.LastKnownPositionTimeStamp;
            machine.LastKnownX = machine.LastKnownX;
            machine.LastKnownY = machine.LastKnownY;
          }

          if (string.IsNullOrEmpty(machine.MachineHardwareID))
            machine.MachineHardwareID = machine.MachineHardwareID;

          if (machine.MachineType == 0)
            machine.MachineType = machine.MachineType;
        }
      }

      return eventIntegrator;
    }

    /// <summary>
    /// Creates new or updates existing machines in the site model ready to receive the aggregated machine events
    /// </summary>
    /// <param name="siteModelFromDatamodel">The persistent site model the events will be integrated into</param>
    /// <param name="task">The 'seed' task used as a hold all for aggregated machines</param>
    private void CreateOrUpdateSiteModelMachines(ISiteModel siteModelFromDatamodel, AggregatedDataIntegratorTask task)
    {
      // Integrate the items present in the 'IntermediaryTargetSiteModel' into the real site model
      // read from the datamodel file itself, then synchronously write it to the DataModel

      _log.LogDebug("Aggregation Task Process --> Creating and updating machines in the live site model");

      lock (siteModelFromDatamodel)
      {
        // 'Include' the extents etc of the 'task' each site model being merged into the persistent database
        siteModelFromDatamodel.Include(task.IntermediaryTargetSiteModel);

        // Iterate over all the machine events collected in the task
        foreach (var machineFromTask in task.IntermediaryTargetMachines)
        {
          // Need to locate or create a matching machine in the site model.
          var machineFromDatamodel = siteModelFromDatamodel.Machines.Locate(machineFromTask.ID, machineFromTask.Name, machineFromTask.IsJohnDoeMachine);

          // Log.LogInformation($"Selecting machine: PersistedTargetMachineID={task.PersistedTargetMachineID}, IsJohnDoe?:{task.IntermediaryTargetMachine.IsJohnDoeMachine}, Result: {machineFromDatamodel}");

          if (machineFromDatamodel == null)
          {
            machineFromDatamodel = siteModelFromDatamodel.Machines.CreateNew(machineFromTask.Name,
              machineFromTask.MachineHardwareID,
              machineFromTask.MachineType,
              machineFromTask.DeviceType,
              machineFromTask.IsJohnDoeMachine,
              machineFromTask.ID);
            machineFromDatamodel.Assign(machineFromTask);
          }

          // Update the internal name of the machine with the machine name from the TAG file
          if (machineFromTask.Name != "" && machineFromDatamodel.Name != machineFromTask.Name)
            machineFromDatamodel.Name = machineFromTask.Name;

          // Update the internal type of the machine with the machine type from the TAG file
          // if the existing internal machine type is zero then
          if (machineFromTask.MachineType != 0 && machineFromDatamodel.MachineType == 0)
            machineFromDatamodel.MachineType = machineFromTask.MachineType;

          // If the machine target values can't be found then create them
          var siteModelMachineTargetValues = siteModelFromDatamodel.MachinesTargetValues[machineFromDatamodel.InternalSiteModelMachineIndex];

          if (siteModelMachineTargetValues == null)
            siteModelFromDatamodel.MachinesTargetValues.Add(new ProductionEventLists(siteModelFromDatamodel, machineFromDatamodel.InternalSiteModelMachineIndex));
        }
      }
    }

    /// <summary>
    /// Integrates all the machine events from the processed TAG files into the matching machines in the live site model
    /// </summary>
    /// <param name="siteModelFromDatamodel">The persistent site model the events will be integrated into</param>
    /// <param name="task">The 'seed' task used as a hold all for aggregated machines</param>
    /// <param name="eventIntegrator">The event integrator to perform the mechanical insertion of the new events</param>
    private bool IntegrateMachineEventsIntoLiveSiteModel(ISiteModel siteModelFromDatamodel, AggregatedDataIntegratorTask task,
      EventIntegrator eventIntegrator)
    {
      // Perform machine event integration outside of the SiteModel write access interlock as the
      // individual event lists have independent exclusive locks event integration uses.

      _log.LogDebug("Aggregation Task Process --> Integrating machine events into the live site model");

      // Iterate over all the machine events collected in the task
      foreach (var machineFromTask in task.IntermediaryTargetMachines)
      {
        var machineFromDatamodel = siteModelFromDatamodel.Machines.Locate(machineFromTask.ID, machineFromTask.Name, machineFromTask.IsJohnDoeMachine);
        var siteModelMachineTargetValues = siteModelFromDatamodel.MachinesTargetValues[machineFromDatamodel.InternalSiteModelMachineIndex];

        eventIntegrator.IntegrateMachineEvents(task.AggregatedMachineEvents[machineFromTask.InternalSiteModelMachineIndex],
          siteModelMachineTargetValues, true, task.IntermediaryTargetSiteModel, siteModelFromDatamodel);

        // Integrate the machine events into the main site model. This requires the
        // site model interlock as aspects of the site model state (machine) are being changed.
        lock (siteModelFromDatamodel)
        {
          if (siteModelMachineTargetValues != null)
          {
            //Update machine last known value (events) from integrated model before saving
            var comparison = machineFromDatamodel.LastKnownPositionTimeStamp.CompareTo(machineFromDatamodel.LastKnownPositionTimeStamp);
            if (comparison == -1)
            {
              machineFromDatamodel.LastKnownDesignName = siteModelFromDatamodel.SiteModelMachineDesigns[siteModelMachineTargetValues.MachineDesignNameIDStateEvents.LastStateValue()].Name;
              machineFromDatamodel.LastKnownLayerId = siteModelMachineTargetValues.LayerIDStateEvents.Count() > 0 ? siteModelMachineTargetValues.LayerIDStateEvents.LastStateValue() : (ushort)0;
              machineFromDatamodel.LastKnownPositionTimeStamp = machineFromDatamodel.LastKnownPositionTimeStamp;
              machineFromDatamodel.LastKnownX = machineFromDatamodel.LastKnownX;
              machineFromDatamodel.LastKnownY = machineFromDatamodel.LastKnownY;
            }
          }
          else
          {
            _log.LogError("SiteModelMachineTargetValues not located in aggregate machine events integrator");
            return false;
          }
        }

        // Use the synchronous command to save the machine events to the persistent store into the deferred (asynchronous model)
        siteModelMachineTargetValues.SaveMachineEventsToPersistentStore(_storageProxyMutable);
      }

      return true;
    }

    /// <summary>
    /// Sends notifications to all parties interested in changes to site model state
    /// </summary>
    /// <param name="siteModelFromDatamodel">The site model to perform the change notifications for</param>
    private void PerformSiteModelChangeNotifications(ISiteModel siteModelFromDatamodel)
    {
      if (!AdviseOtherServicesOfDataModelChanges)
        return;

      // Notify the site model in all contents in the grid that it's attributes have changed
      _log.LogInformation($"Aggregation Task Process --> Notifying site model attributes changed for {siteModelFromDatamodel.ID}");

      // Notify the immutable grid listeners that attributes of this site model have changed.
      var sender = DIContext.Obtain<ISiteModelAttributesChangedEventSender>();
      sender.ModelAttributesChanged
      (targetGrid: SiteModelNotificationEventGridMutability.NotifyImmutable,
        siteModelID: siteModelFromDatamodel.ID,
        existenceMapChanged: true,
        existenceMapChangeMask: _workingModelUpdateMap,
        machinesChanged: true,
        machineTargetValuesChanged: true,
        machineDesignsModified: true,
        proofingRunsModified: true);
    }

    /// <summary>
    /// Sends notifications to all parties interested in changes to site model state
    /// </summary>
    /// <param name="siteModelFromDatamodel">The site model to perform the change notifications for</param>
    private void UpdateSiteModelMetaData(ISiteModel siteModelFromDatamodel)
    {
      // Update the metadata for the site model
      _log.LogInformation($"Aggregation Task Process --> Updating site model metadata for {siteModelFromDatamodel.ID}");
      DIContext.Obtain<ISiteModelMetadataManager>().Update
      (siteModelID: siteModelFromDatamodel.ID, lastModifiedDate: DateTime.UtcNow, siteModelExtent: siteModelFromDatamodel.SiteModelExtent,
        machineCount: siteModelFromDatamodel.Machines.Count);
    }

    /// <summary>
    /// Integrates the cell passes processed from TAG files into sub grids within the live site model
    /// </summary>
    /// <param name="siteModelFromDatamodel">The site model to perform the change notifications for</param>
    /// <param name="task">The 'seed' task used as a hold all for aggregated machines</param>
    /// <param name="subGridIntegrator">The integrator to use to insert the new cell passes into the live site model</param>
    /// <param name="groupedAggregatedCellPasses">The set of all cell passes from all TAG files, grouped in to a single intermediary site model</param>
    /// <param name="numTagFilesRepresented">The number of TAG files represented in the data set being integrated</param>
    /// <param name="totalPassCountInAggregation">The sum total number of cell passes integrated in the live site model</param>
    private bool IntegrateCellPassesIntoLiveSiteModel(ISiteModel siteModelFromDatamodel,
      AggregatedDataIntegratorTask task,
      SubGridIntegrator subGridIntegrator,
      ISubGridTree groupedAggregatedCellPasses,
      int numTagFilesRepresented,
      out long totalPassCountInAggregation)
    {
      _log.LogInformation($"Aggregation Task Process --> Labeling aggregated cell pass with correct machine ID for {siteModelFromDatamodel.ID}");

      totalPassCountInAggregation = 0;

      // This is a dirty map for the leaf sub grids and is stored as a bitmap grid
      // with one level fewer that the sub grid tree it is representing, and
      // with cells the size of the leaf sub grids themselves. As the cell coordinates
      // we have been given are with respect to the sub grid, we must transform them
      // into coordinates relevant to the dirty bitmap sub grid tree.

      _workingModelUpdateMap = new SubGridTreeSubGridExistenceBitMask
      {
        CellSize = SubGridTreeConsts.SubGridTreeDimension * siteModelFromDatamodel.CellSize,
        ID = siteModelFromDatamodel.ID
      };

      // Integrate the cell pass data into the main site model and commit each sub grid as it is updated
      // ... first relabel the passes with the machine IDs from the persistent datamodel

      // Compute the vector of internal site model machine indexes between the intermediary site model constructed from the TAG files,
      // and the persistent site model the data us being processed into
      (short taskInternalMachineIndex, short datamodelInternalMachineIndex)[] internalMachineIndexMap = task.IntermediaryTargetMachines
        .Select(taskMachine => (taskMachine.InternalSiteModelMachineIndex,
                                siteModelFromDatamodel.Machines.Locate(taskMachine.ID, taskMachine.Name, taskMachine.IsJohnDoeMachine).InternalSiteModelMachineIndex))
        .OrderBy(x => x.Item1)
        .ToArray();

      // Make sure the internal indexes are all there
      for (var i = 0; i < internalMachineIndexMap.Length; i++)
      {
        if (internalMachineIndexMap[i].taskInternalMachineIndex != i)
        {
          throw new TRexException("Internal index map not in expected order, or elements are missing");
        }
      }

      // Iterate across all passes generated from the processed TAG files and modify th
      long totalPassCountInAggregationLocal = 0;
      groupedAggregatedCellPasses?.ScanAllSubGrids(leaf =>
      {
        var serverLeaf = (ServerSubGridTreeLeaf)leaf;

        foreach (var segment in serverLeaf.Cells.PassesData.Items)
        {
          segment.PassesData.SetAllInternalMachineIDs(internalMachineIndexMap, out var modifiedPassCount);
          totalPassCountInAggregationLocal += modifiedPassCount;
        }

        return true;
      });
      totalPassCountInAggregation = totalPassCountInAggregationLocal;

      // ... then integrate them
      var sw2 = Stopwatch.StartNew();
      _log.LogInformation($"Aggregation Task Process --> Integrating aggregated results for {totalPassCountInAggregation} cell passes from {numTagFilesRepresented} TAG files (spanning {groupedAggregatedCellPasses?.CountLeafSubGridsInMemory()} sub grids) into primary data model for {siteModelFromDatamodel.ID} spanning {siteModelFromDatamodel.ExistenceMap.CountBits()} sub grids");

      if (!subGridIntegrator.IntegrateSubGridTree(SubGridTreeIntegrationMode.SaveToPersistentStore, SubGridHasChanged))
      {
        _log.LogError("Aggregation Task Process --> Aborting due to failure in integration process");
        return false;
      }

      _log.LogInformation($"Aggregation Task Process --> Completed integrating aggregated results into primary data model for {siteModelFromDatamodel.ID}, in elapsed time of {sw2.Elapsed}");

      return true;
    }

    /// <summary>
    /// Advise the segment retirement manager of any segments/sub grids that need to be retired as as result of this integration
    /// </summary>
    /// <param name="siteModelFromDatamodel">The site model the changes are being committed to</param>
    /// <param name="invalidatedSpatialStreams">The streams of data in the persistent store that will be invalidated by updates to sub grids performed by the sub grid integrator</param>
    private void AddInvalidatedStreamsToRetirementQueue(ISiteModel siteModelFromDatamodel, List<ISubGridSpatialAffinityKey> invalidatedSpatialStreams)
    {
      _log.LogInformation($"Aggregation Task Process --> Updating segment retirement queue for {siteModelFromDatamodel.ID}");

      if (invalidatedSpatialStreams.Count == 0)
        return;

      // Stamp all the invalidated spatial streams with the project ID
      invalidatedSpatialStreams.ForEach(x => x.ProjectUID = siteModelFromDatamodel.ID);

      try
      {
        var retirementQueue = DIContext.Obtain<ISegmentRetirementQueue>();

        if (retirementQueue == null)
        {
          throw new TRexTAGFileProcessingException("No registered segment retirement queue in DI context");
        }

        var insertUtc = DateTime.UtcNow;

        retirementQueue.Add(
          new SegmentRetirementQueueKey {ProjectUID = siteModelFromDatamodel.ID, InsertUTCAsLong = insertUtc.Ticks},
          new SegmentRetirementQueueItem {InsertUTCAsLong = insertUtc.Ticks, ProjectUID = siteModelFromDatamodel.ID, SegmentKeys = invalidatedSpatialStreams.ToArray()});
      }
      catch (Exception e)
      {
        _log.LogCritical(e, "Unable to add segment invalidation list to segment retirement queue due to exception:");
        _log.LogCritical("The following segments will NOT be retired as a result:");
        foreach (var invalidatedItem in invalidatedSpatialStreams)
        {
          _log.LogCritical($"{invalidatedItem}");
        }
      }
    }

    /// <summary>
    /// Commits all data prepared via aggregation and integration of a set of processed TAG files to the site model
    /// persistent store via the transactional data proxy
    /// </summary>
    private void CommitPendingTransactedChangesToPersistentStore(Guid siteModelUid, IStorageProxy storageProxy) //ISiteModel siteModelFromDatamodel)
    {
      // All operations within the transaction to integrate the changes into the live model have completed successfully.
      // Now commit those changes as a block.

      var startTime = DateTime.UtcNow;

      _log.LogInformation($"Starting storage proxy Commit(). Committing {storageProxy.PotentialCommitWrittenBytes()} bytes from transacted Byte[] elements");

      if (!storageProxy.Commit(out var numDeleted, out var numUpdated, out var numBytesWritten))
      {
        _log.LogCritical($"Failed to commit site model existence map and related information for site model {siteModelUid} during aggregation epoch");
      }

      _log.LogInformation($"Completed storage proxy Commit(), duration = {DateTime.UtcNow - startTime}, requiring {numDeleted} deletions, {numUpdated} updates with {numBytesWritten} bytes written");
    }

    /// <summary>
    /// Advise the TAG file processing statistics of the number of TAG files, processed cell passes etc  during this aggregation epoch
    /// </summary>
    /// <param name="numTagFilesRepresented">The number of source TAG files represented by the tasks this epoch processed</param>
    /// <param name="totalPassCountInAggregation">The number of cell passes committed in this epoch (includes new and updated cell passes)</param>
    private void UpdateTAGTFileProcessingTrackingStatistics(int numTagFilesRepresented, long totalPassCountInAggregation)
    {
      TAGProcessingStatistics.IncrementTotalTAGFilesProcessedIntoModels(numTagFilesRepresented);
      TAGProcessingStatistics.IncrementTotalCellPassesAggregatedIntoModels(totalPassCountInAggregation);
    }

    /// <summary>
    /// Persists the modified existence map, machines and other related site model meta data for the processing epoch
    /// </summary>
    /// <param name="siteModelFromDatamodel">The site model containing the modified data to be written</param>
    private void CommitSiteModelExistenceMapToPersistentStore(ISiteModel siteModelFromDatamodel)
    {
      // Use the synchronous command to save the site model information to the persistent store into the deferred (asynchronous model)
      siteModelFromDatamodel.SaveToPersistentStoreForTAGFileIngest(_storageProxyMutable);

      if (!_storageProxyMutable.Commit())
      {
        _log.LogCritical($"Failed to commit site model existence map and related information for site model {siteModelFromDatamodel.ID} during aggregation epoch");
      }
    }

    /// <summary>
    /// Processes all available tasks in the TasksToProcess list up to the maximum number the worker will accept 
    /// for any single epoch of processing TAG files.
    /// </summary>
    /// <param name="processedTasks"></param>
    /// <param name="numTagFilesRepresented"></param>
    /// <returns></returns>
    public bool ProcessTask(List<AggregatedDataIntegratorTask> processedTasks, int numTagFilesRepresented)
    {
      long totalPassCountInAggregation = 0;

      /* The task contains a set of machine events and cell passes that need to be integrated into the
        machine and site model references in the task respectively. Machine events need to be integrated
        before the cell passes that reference them are integrated.

        All other tasks in the task list that contain aggregated machine events and cell passes
        are integrated together into the machine events and site model in one operation prior to
        the modified information being committed to disk.

        A task is only said to be completed when all integrations and resulting updates are
        persisted to disk.*/

      processedTasks.Clear();

      // Set capacity to maximum expected size to prevent List resizing while assembling tasks
      processedTasks.Capacity = MaxMappedTagFilesToProcessPerAggregationEpoch;

      AggregatedDataIntegratorTask task = null;
      var sw = Stopwatch.StartNew();
      try
      {
        if (!_tasksToProcess.TryDequeue(out task))
        {
          return true; // There is nothing in the queue to work on so just return true
        }

        _log.LogInformation("Aggregation Task Process: Clearing mutable storage proxy");

        _storageProxyMutable.Clear();

        var siteModelFromDatamodel = ObtainSiteModel(task);
        if (siteModelFromDatamodel == null)
        {
          _log.LogError($"Unable to lock SiteModel {task.PersistedTargetSiteModelID} from the data model file");
          return false;
        }

        task.StartProcessingTime = Consts.MIN_DATETIME_AS_UTC;
        processedTasks.Add(task); // Seed task is always a part of the processed tasks

        // First check to see if this task has been catered to by previous task processing
        var anyMachineEvents = task.AggregatedMachineEvents != null;
        var anyCellPasses = task.AggregatedCellPasses != null;

        if (!(anyMachineEvents || anyCellPasses))
        {
          _log.LogWarning($"Suspicious task with no cell passes or machine events in site model {task.PersistedTargetSiteModelID}");
          return true;
        }

        AssembleGroupedTagFiles(processedTasks, task);
        var groupedAggregatedCellPasses = GroupSwathedSubGridTrees(processedTasks, task);
        var eventIntegrator = AggregateAllMachineEvents(processedTasks, task);
        CreateOrUpdateSiteModelMachines(siteModelFromDatamodel, task);

        if (!IntegrateMachineEventsIntoLiveSiteModel(siteModelFromDatamodel, task, eventIntegrator))
        {
          return false;
        }

        // Commit the modified events to the persistent store. This precedes the spatial data changes as the spatial data are dependent on the events
        CommitPendingTransactedChangesToPersistentStore(siteModelFromDatamodel.ID, _storageProxyMutable);

        try
        {
          var subGridIntegrator = new SubGridIntegrator(groupedAggregatedCellPasses, siteModelFromDatamodel, siteModelFromDatamodel.Grid, 
                                                        _storageProxyMutableForSubGrids, _storageProxyMutableForSubGridSegments);

          if (!IntegrateCellPassesIntoLiveSiteModel(siteModelFromDatamodel, task, subGridIntegrator, groupedAggregatedCellPasses, numTagFilesRepresented, out totalPassCountInAggregation))
          {
            return false;
          }

          // Commit spatial data changes to the store.
          // This is done in two phases:
          // 1. Sub grid segments. These are references by the sub grids themselves and must be committed to persistent
          //    store prior to the sub grids so that concurrent query operations do not access inconsistent states of persisted data
          // 2. Sub grids. These contain the segment directory and other information related to the sub grid as a whole
          // Note: The site model existence map is not persisted at this point
          CommitPendingTransactedChangesToPersistentStore(siteModelFromDatamodel.ID, _storageProxyMutableForSubGridSegments);
          CommitPendingTransactedChangesToPersistentStore(siteModelFromDatamodel.ID, _storageProxyMutableForSubGrids);

          // Once all event and spatial data has been committed the independently write and commit the modified information 
          // for existence map etc contained in the site model. This permits new information to be added to the site model while 
          // queries are being executed against it. The new information will no be used until the update existence map is saved,
          // though previously existing sub grids may be visible to queries in an updated state for short periods until 
          // Raptor style sub grid version management is implemented (currently not supported in TRex)
          CommitSiteModelExistenceMapToPersistentStore(siteModelFromDatamodel);

          UpdateSiteModelMetaData(siteModelFromDatamodel);

          AddInvalidatedStreamsToRetirementQueue(siteModelFromDatamodel, subGridIntegrator.InvalidatedSpatialStreams);
          PerformSiteModelChangeNotifications(siteModelFromDatamodel);
          UpdateTAGTFileProcessingTrackingStatistics(numTagFilesRepresented, totalPassCountInAggregation);
        }
        finally
        {
          if (groupedAggregatedCellPasses != task.AggregatedCellPasses)
          {
            groupedAggregatedCellPasses?.Dispose();
          }

          task.AggregatedCellPasses?.Dispose();
          task.AggregatedCellPasses = null;
          _workingModelUpdateMap = null;
        }
      }
      finally
      {
        _log.LogInformation($"Aggregation Task Process --> Completed integrating {processedTasks.Count} TAG files and {totalPassCountInAggregation} cell passes in project {task?.PersistedTargetSiteModelID} in elapsed time of {sw.Elapsed}");
      }

      return true;
    }

    /// <summary>
    /// Performs clean up operations required at the end of processing a collection of TAG files.
    /// Clean up operations include:
    /// - Removal of all cached site model content created during processing of TAG files
    /// </summary>
    public void CompleteTaskProcessing()
    {
      _log.LogInformation($"Aggregation Task Process --> Dropping cached content for site model {SiteModelID}");

      // Finally, drop the site model context being used to perform the aggregation/integration to free up the cached
      // sub grid and segment information used during this processing epoch.
      DIContext.Obtain<ISiteModels>().DropSiteModel(SiteModelID);
    }
  }
}
