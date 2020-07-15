﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Serilog.Extensions;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.SubGridTrees.Core;
using VSS.TRex.SubGridTrees.Factories;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SubGridTrees.Server.Iterators;
using VSS.TRex.SubGridTrees.Server.Utilities;
using VSS.TRex.Types;

namespace VSS.TRex.SubGridTrees.Server
{
  public class ServerSubGridTree : SubGridTree, IServerSubGridTree
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<ServerSubGridTree>();

    /// <summary>
    /// Controls whether segment and cell pass information held within this server sub grid tree is represented
    /// in the mutable or immutable forms supported by TRex
    /// </summary>
    public bool IsMutable { get; } = false;

    /// <summary>
    /// Controls emission of sub grid reading activities into the log.
    /// </summary>
    public bool RecordSubGridFileReadingToLog { get; set; } = false;

    /// <summary>
    /// The caching strategy to be used for sub grids requested from this sub grid tree
    /// </summary>
    public ServerSubGridTreeCachingStrategy CachingStrategy { get; set; } = ServerSubGridTreeCachingStrategy.CacheSubGridsInTree;

    /// <summary>
    /// Record operations related to segments cloven during TAG file ingest into the log
    /// </summary>
    private static readonly bool _segmentCleavingOperationsToLog = DIContext.Obtain<IConfigurationStore>().GetValueBool("SEGMENTCLEAVINGOOPERATIONS_TOLOG", Consts.SEGMENTCLEAVINGOOPERATIONS_TOLOG);
    
    public ServerSubGridTree(Guid siteModelId, StorageMutability mutability) :
      this(SubGridTreeConsts.SubGridTreeLevels, SubGridTreeConsts.DefaultCellSize,
        new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>(), mutability)
    {
      ID = siteModelId; // Ensure the ID of the sub grid tree matches the datamodel ID
    }

    public ServerSubGridTree(byte numLevels,
      double cellSize,
      ISubGridFactory subGridFactory,
      StorageMutability mutability) : base(numLevels, cellSize, subGridFactory)
    {
      IsMutable = mutability == StorageMutability.Mutable;

      SetDefaultCachingStrategy();
    }

    /// <summary>
    /// Default the caching strategy to be used based on the mutable versus immutable context
    /// Mutable contexts require transient caching of processing sub grid information so default to caching sub grids within the tree
    /// Immutable contexts represent 'PSNode' sub grid request processing contexts which rely on the underlying grid cache for caching support.
    /// </summary>
    private void SetDefaultCachingStrategy()
    {
      CachingStrategy = IsMutable ? ServerSubGridTreeCachingStrategy.CacheSubGridsInTree : ServerSubGridTreeCachingStrategy.CacheSubGridsInIgniteGridCache;
    }

    public override ISubGrid CreateNewSubGrid(byte level)
    {
      var subGrid = base.CreateNewSubGrid(level);

      if (level == numLevels) 
      {
        // It is a leaf sub grid, decorate it with the required mutability. Note, this subGrid is guaranteed to be an instance
        // of leaf generic type supplied to the factory in the constructor for this sub grid tree.
        ((ServerSubGridTreeLeaf)subGrid).SetIsMutable(IsMutable);
      }

      return subGrid;
    }

    /// <summary>
    /// Computes a unique file name for a segment within a particular sub grid
    /// </summary>
    public static string GetLeafSubGridSegmentFullFileName(SubGridCellAddress cellAddress,
      ISubGridCellPassesDataSegmentInfo segmentInfo)
    {
      // Work out the cell address of the origin cell in the appropriate leaf
      // sub grid. We use this cell position to derive the name of the file
      // containing the leaf sub grid data
      return segmentInfo.FileName(cellAddress.X & ~SubGridTreeConsts.SubGridLocalKeyMask,
                                  cellAddress.Y & ~SubGridTreeConsts.SubGridLocalKeyMask);
    }

    public bool LoadLeafSubGridSegment(IStorageProxy storageProxy,
      SubGridCellAddress cellAddress,
      bool loadLatestData,
      bool loadAllPasses,
      IServerLeafSubGrid subGrid,
      ISubGridCellPassesDataSegment segment)
    {
      //Log.LogInformation($"Segment load on {cellAddress}:{Segment.SegmentInfo.StartTime}-{Segment.SegmentInfo.EndTime} beginning, loadLatestData = {loadLatestData}, loadAllPasses = {loadAllPasses}");

      var needToLoadLatestData = loadLatestData && !segment.HasLatestData;
      var needToLoadAllPasses = loadAllPasses && !segment.HasAllPasses;

      if (!needToLoadLatestData && !needToLoadAllPasses)
      {
        //Log.LogInformation($"Segment load on {cellAddress} exiting as neither latest nor all passes required");
        return true; // Nothing more to do here
      }

      // Lock the segment briefly while its contents is being loaded
      lock (segment)
      {
        if (!(needToLoadLatestData ^ segment.HasLatestData) && !(needToLoadAllPasses ^ segment.HasAllPasses))
        {
          //Log.LogInformation($"Segment load on {cellAddress} leaving quietly as a previous thread has performed the load");
          return true; // The load operation was performed on another thread. Leave quietly
        }

        // Ensure the appropriate storage is allocated
        if (needToLoadLatestData)
          segment.AllocateLatestPassGrid();

        if (needToLoadAllPasses)
          segment.AllocateFullPassStacks();

        if (!segment.SegmentInfo.ExistsInPersistentStore)
        {
          //Log.LogInformation($"Segment load on {cellAddress} exiting as segment does not exist in persistent store");
          return true; // Nothing more to do here
        }

        // Locate the segment file and load the data from it
        var fullFileName = GetLeafSubGridSegmentFullFileName(cellAddress, segment.SegmentInfo);

        // Load the cells into it from its file
        var fileLoaded = subGrid.LoadSegmentFromStorage(storageProxy, fullFileName, segment, needToLoadLatestData, needToLoadAllPasses);

        if (!fileLoaded)
        {
          //Log.LogInformation($"Segment load on {cellAddress} failed, performing allocation cleanup activities");

          // Oops, something bad happened. Remove the segment from the list. Return failure to the caller.
          if (loadAllPasses)
            segment.DeAllocateFullPassStacks();

          if (loadLatestData)
            segment.DeAllocateLatestPassGrid();

          return false;
        }
      }
        
      //Log.LogInformation($"Segment load on {cellAddress} succeeded, AllPasses?={Segment.HasAllPasses}, Segment.PassesData?Null={Segment.PassesData==null} ");

      return true;
    }

    public bool LoadLeafSubGrid(IStorageProxy storageProxy,
      SubGridCellAddress cellAddress,
      bool loadAllPasses, bool loadLatestPasses,
      IServerLeafSubGrid subGrid)
    {
      var fullFileName = string.Empty;
      var result = false;

      try
      {
        // Loading contents into a dirty sub grid (which should happen only on the mutable nodes), or
        // when there is already content in the segment directory are strictly forbidden and break immutability
        // rules for sub grids
        if (subGrid.Dirty)
        {
          throw new TRexSubGridIOException("Leaf sub grid directory loads may not be performed while the sub grid is dirty. The information should be taken from the cache instead.");
        }

        // Load the cells into it from its file
        // Briefly lock this sub grid just for the period required to read its contents
        lock (subGrid)
        {
          // If more than thread wants this sub grid then they may concurrently attempt to load it.
          // Make a check to see if this has happened and another thread has already loaded this sub grid directory.
          // If so, just return success. Previously the commented out assert was enforced causing exceptions
          // Debug.Assert(SubGrid.Directory?.SegmentDirectory?.Count == 0, "Loading a leaf sub grid directory when there are already segments present within it.");

          // Check this thread is the winner of the lock to be able to load the contents
          if (subGrid.Directory?.SegmentDirectory?.Count == 0)
          {
            // Ensure the appropriate storage is allocated
            if (loadAllPasses)
              subGrid.AllocateLeafFullPassStacks();

            if (loadLatestPasses)
              subGrid.AllocateLeafLatestPassGrid();

            fullFileName = GetLeafSubGridFullFileName(cellAddress);

            // Briefly lock this sub grid just for the period required to read its contents
            result = subGrid.LoadDirectoryFromFile(storageProxy, fullFileName);
          }
          else
          {
            // A previous thread has already read the directory so there is nothing more to do, return a true result.
            return true;
          }
        }
      }
      finally
      {
        if (result && RecordSubGridFileReadingToLog)
          _log.LogDebug($"Sub grid file {fullFileName} read from persistent store containing {subGrid.Directory.SegmentDirectory.Count} segments (Moniker: {subGrid.Moniker()}");
      }

      return result;
    }

    public static string GetLeafSubGridFullFileName(SubGridCellAddress cellAddress)
    {
      // Work out the cell address of the origin cell in the appropriate leaf
      // sub grid. We use this cell position to derive the name of the file
      // containing the leaf sub grid data
      return ServerSubGridTreeLeaf.FileNameFromOriginPosition(new SubGridCellAddress(cellAddress.X & ~SubGridTreeConsts.SubGridLocalKeyMask, cellAddress.Y & ~SubGridTreeConsts.SubGridLocalKeyMask));
    }

    /// <summary>
    /// Orchestrates all the activities relating to saving the state of a created or modified sub grid, including
    /// cleaving, saving updated elements, creating new elements and arranging for the retirement of
    /// elements that have been replaced in the persistent store as a result of this activity.
    /// </summary>
    public bool SaveLeafSubGrid(IServerLeafSubGrid subGrid,
                                IStorageProxy storageProxyForSubGrids,
                                IStorageProxy storageProxyForSubGridSegments,
                                List<ISubGridSpatialAffinityKey> invalidatedSpatialStreams)
    {
      //Log.LogInformation($"Saving {subGrid.Moniker()} to persistent store");

      try
      {
        // Perform segment cleaving as the first activity in the action of saving a leaf sub grid
        // to disk. This reduces the number of segment cleaving actions that would otherwise
        // be performed in the context of the aggregated integrator.

        if (_segmentCleavingOperationsToLog)
          _log.LogDebug($"About to perform segment cleaving on {subGrid.Moniker()}");

        var cleaver = new SubGridSegmentCleaver();

        cleaver.PerformSegmentCleaving(storageProxyForSubGridSegments, subGrid);

        // Calculate the cell last pass information here, immediately before it is
        // committed to the persistent store. The reason for this is to remove this
        // compute intensive operation from the critical path in TAG file processing
        // (which is the only writer of this information in the Raptor system).
        // The computer is instructed to do a partial recompute, which will recompute
        // all segments from the first segment marked as dirty.
        subGrid.ComputeLatestPassInformation(false, storageProxyForSubGridSegments);

        if (_segmentCleavingOperationsToLog)
          _log.LogInformation($"SaveLeafSubGrid: {subGrid.Moniker()} ({subGrid.Cells.PassesData.Count} segments)");

        var modifiedOriginalSegments = new List<ISubGridCellPassesDataSegment>(100);
        var newSegmentsFromCleaving = new List<ISubGridCellPassesDataSegment>(100);

        var originAddress = new SubGridCellAddress(subGrid.OriginX, subGrid.OriginY);

        // The following used to be an assert/exception. However, this is may readily
        // happen if there are no modified segments resulting from processing a
        // process TAG file where the Dirty flag for the sub grid is set but no cell
        // passes are added to segments in that sub grid. As this is not an integrity
        // issue the persistence of the modified sub grid is allowed, but noted in
        // the log for posterity.
        if (subGrid.Cells.PassesData.Count == 0)
          _log.LogInformation(
            $"Note: Saving a sub grid, {subGrid.Moniker()}, (Segments = {subGrid.Cells.PassesData.Count}, Dirty = {subGrid.Dirty}) with no cached sub grid segments to the persistent store in SaveLeafSubGrid (possible reprocessing of TAG file with no cell pass changes). " +
            $"SubGrid.Directory.PersistedClovenSegments.Count={cleaver.PersistedClovenSegments?.Count}, ModifiedOriginalFiles.Count={modifiedOriginalSegments.Count}, NewSegmentsFromCleaving.Count={newSegmentsFromCleaving.Count}");

        var iterator = new SubGridSegmentIterator(subGrid, storageProxyForSubGridSegments)
        {
          IterationDirection = IterationDirection.Forwards,
          ReturnDirtyOnly = true,

          // We don't consider saving of segments to disk to be equivalent to
          // 'use' of the segments so they are not touched with respect to the cache
          // when saved. This aids segments that might not be updated again in TAG
          // processing to exit the cache sooner and also removes clashes with the
          // cache with it performs cache resize operations.
          MarkReturnedSegmentsAsTouched = false
        };

        //**********************************************************************
        //***Construct list of original segment files that have been modified***
        //*** These files may be updated in-situ with no sub grid/segment    ***
        //*** integrity issues wrt the segment directory in the sub grid     ***
        //**********************************************************************

        iterator.MoveToFirstSubGridSegment();
        while (iterator.CurrentSubGridSegment != null)
        {
          if (iterator.CurrentSubGridSegment.SegmentInfo.ExistsInPersistentStore && iterator.CurrentSubGridSegment.Dirty)
            modifiedOriginalSegments.Add(iterator.CurrentSubGridSegment);
          iterator.MoveToNextSubGridSegment();
        }

        //**********************************************************************
        //*** Construct list of spatial streams that will be deleted or replaced
        //*** in the FS file. These will be passed to the call that saves the
        //*** sub grid directory file as an instruction to place them into the
        //*** deferred deletion list
        //**********************************************************************

        lock (invalidatedSpatialStreams)
        {
          if (cleaver.PersistedClovenSegments != null)
            invalidatedSpatialStreams.AddRange(cleaver.PersistedClovenSegments);

          invalidatedSpatialStreams.AddRange(modifiedOriginalSegments.Select(x => x.SegmentInfo.AffinityKey(ID)));
        }

        //**********************************************************************
        //*** Construct list of new segment files that have been created     ***
        //*** by the cleaving of other segments in the sub grid              ***
        //**********************************************************************

        iterator.MoveToFirstSubGridSegment();
        while (iterator.CurrentSubGridSegment != null)
        {
          if (!iterator.CurrentSubGridSegment.SegmentInfo.ExistsInPersistentStore)
            newSegmentsFromCleaving.Add(iterator.CurrentSubGridSegment);

          iterator.MoveToNextSubGridSegment();
        }

        if (newSegmentsFromCleaving.Count > 0)
        {
          //**********************************************************************
          //***    Write new segment files generated by cleaving               ***
          //*** File system integrity failures here will have no effect on the ***
          //*** sub grid/segment directory as they are not referenced by it.   ***
          //*** At worst they become orphans they may be cleaned by in the FS  ***
          //*** recovery phase                                                 ***
          //**********************************************************************

          if (_segmentCleavingOperationsToLog)
            _log.LogInformation($"Sub grid has {newSegmentsFromCleaving.Count} new segments from cleaving");

          foreach (var segment in newSegmentsFromCleaving)
          {
            // Update the version of the segment as it is about to be written
            segment.SegmentInfo.Touch();

            segment.SaveToFile(storageProxyForSubGridSegments, GetLeafSubGridSegmentFullFileName(originAddress, segment.SegmentInfo), out var fsError);

            if (fsError == FileSystemErrorStatus.OK)
            {
              if (_segmentCleavingOperationsToLog)
                _log.LogInformation($"Saved new cloven grid segment file: {GetLeafSubGridSegmentFullFileName(originAddress, segment.SegmentInfo)}");
            }
            else
            {
              _log.LogWarning($"Failed to save cloven grid segment {GetLeafSubGridSegmentFullFileName(originAddress, segment.SegmentInfo)}: Error:{fsError}");
              return false;
            }
          }
        }

        if (modifiedOriginalSegments.Count > 0)
        {
          //**********************************************************************
          //***    Write modified segment files                                ***
          //*** File system integrity failures here will have no effect on the ***
          //*** sub grid/segment directory as the previous version of the      ***
          //*** modified file being written will be recovered.                 ***
          //**********************************************************************

          if (_log.IsTraceEnabled())
            _log.LogTrace($"Sub grid has {modifiedOriginalSegments.Count} modified segments");

          foreach (var segment in modifiedOriginalSegments)
          {
            // Update the version of the segment as it is about to be written
            segment.SegmentInfo.Touch();

            if (segment.SaveToFile(storageProxyForSubGridSegments, GetLeafSubGridSegmentFullFileName(originAddress, segment.SegmentInfo), out var fsError))
            {
              if (_log.IsTraceEnabled())
                _log.LogTrace($"Saved modified grid segment file: {segment}");
            }
            else
            {
              _log.LogError($"Failed to save modified original grid segment {GetLeafSubGridSegmentFullFileName(originAddress, segment.SegmentInfo)}: Error:{fsError}");
              return false;
            }
          }
        }

        //**********************************************************************
        //***                 Write the sub grid directory file              ***
        //**********************************************************************

        /*
        There is no need to add the sub grid directory stream to the segment retirement
        queue as this will be automatically replaced when the new version of the 
        sub grid directory is written to persistent store.

        // Add the stream representing the sub grid directory file to the list of
        // invalidated streams as this stream will be replaced with the stream
        // containing the updated directory information. Note: This only needs to
        // be done if the sub grid has previously been read from the FS file (if not
        // it has been created and not yet persisted to the store.

        if (subGrid.Directory.ExistsInPersistentStore)
        {
          // Include an additional invalidated spatial stream for the sub grid directory stream
          lock (invalidatedSpatialStreams)
          {
            invalidatedSpatialStreams.Add(subGrid.AffinityKey());
          }
        }
        */

        if (subGrid.SaveDirectoryToFile(storageProxyForSubGrids, GetLeafSubGridFullFileName(originAddress)))
        {
          if (_log.IsTraceEnabled())
            _log.LogTrace($"Saved grid directory file: {GetLeafSubGridFullFileName(originAddress)}");
        }
        else
        {
          if (_log.IsTraceEnabled())
            _log.LogTrace($"Failed to save grid: {GetLeafSubGridFullFileName(originAddress)}");
          return false;
        }

        //**********************************************************************
        //***                   Reset segment dirty flags                    ***
        //**********************************************************************

        iterator.MoveToFirstSubGridSegment();
        while (iterator.CurrentSubGridSegment != null)
        {
          iterator.CurrentSubGridSegment.Dirty = false;
          iterator.CurrentSubGridSegment.SegmentInfo.ExistsInPersistentStore = true;

          iterator.MoveToNextSubGridSegment();
        }

        //Log.LogInformation($"Completed saving {subGrid.Moniker()} to persistent store");

        return true;
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception raised in SaveLeafSubGrid");
      }

      return false;
    }

    #region IDisposable Support
    private bool _disposedValue; // To detect redundant calls

    protected override void Dispose(bool disposing)
    {
      if (_disposedValue)
        return;

      if (disposing)
      {
        // Treat disposal and finalization as the same, dependent on the primary disposedValue flag
        // Traverse all loaded sub grids and advise each to dispose themselves
        ScanAllSubGrids(leaf =>
        {
          (leaf as IServerLeafSubGrid)?.Dispose();
          return true;
        });
      }

      base.Dispose(disposing);

      _disposedValue = true;
    }

    public override void Dispose()
    {
      Dispose(true);
    }
    #endregion
  }
}
