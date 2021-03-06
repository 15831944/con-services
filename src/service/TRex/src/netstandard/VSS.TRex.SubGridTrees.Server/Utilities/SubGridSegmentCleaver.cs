﻿using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.TRex.Common;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SubGridTrees.Server.Iterators;

namespace VSS.TRex.SubGridTrees.Server.Utilities
{
  /// <summary>
  /// Provides segment cleaving semantics against the set of segments contained within a sub grid
  /// </summary>
  public class SubGridSegmentCleaver
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<SubGridSegmentCleaver>();

    private readonly int _subGridMaxSegmentCellPassesLimit = DIContext.Obtain<IConfigurationStore>().GetValueInt("VLPDSUBGRID_MAXSEGMENTCELLPASSESLIMIT", Consts.VLPDSUBGRID_MAXSEGMENTCELLPASSESLIMIT);

    private readonly bool _segmentCleavingOperationsToLog = DIContext.Obtain<IConfigurationStore>().GetValueBool("SEGMENTCLEAVINGOOPERATIONS_TOLOG", Consts.SEGMENTCLEAVINGOOPERATIONS_TOLOG);

    /// <summary>
    /// PersistedClovenSegments contains a list of all the segments that exists in the
    /// persistent data store that have been cloven since the last time this leaf
    /// was persisted to the data store. This is essentially a list of obsolete
    /// segments whose presence in the persistent data store need to be removed
    /// when the sub grid is next persisted    
    /// /// </summary>
    public List<ISubGridSpatialAffinityKey> PersistedClovenSegments { get; } = new List<ISubGridSpatialAffinityKey>(10);

    public List<ISubGridCellPassesDataSegment> NewSegmentsFromCleaving { get; } = new List<ISubGridCellPassesDataSegment>(100);

    /// <summary>
    /// Cleaves all dirty segments requiring cleaving within the given sub grid
    /// </summary>
    public void PerformSegmentCleaving(IStorageProxy storageProxyForSubGridSegments, IServerLeafSubGrid subGrid, int subGridSegmentPassCountLimit = 0)
    {
      var iterator = new SubGridSegmentIterator(subGrid, storageProxyForSubGridSegments)
      {
        IterationDirection = IterationDirection.Forwards,
        ReturnDirtyOnly = true,
        RetrieveAllPasses = true
      };

      var origin = new SubGridCellAddress(subGrid.OriginX, subGrid.OriginY);

      if (!iterator.MoveToFirstSubGridSegment())
        return;

      do
      {
        var segment = iterator.CurrentSubGridSegment;

        var cleavedTimeRangeStart = segment.SegmentInfo.StartTime;
        var cleavedTimeRangeEnd = segment.SegmentInfo.EndTime;

        if (!segment.RequiresCleaving(out var totalPassCount, out var maximumPassCount))
          continue;

        if (subGrid.Cells.CleaveSegment(segment, NewSegmentsFromCleaving, PersistedClovenSegments, subGridSegmentPassCountLimit))
        {
          iterator.SegmentListExtended();

          if (_segmentCleavingOperationsToLog)
            _log.LogInformation(
              $"Info: Performed cleave on segment ({cleavedTimeRangeStart}-{cleavedTimeRangeEnd}) of sub grid {ServerSubGridTree.GetLeafSubGridFullFileName(origin)}. TotalPassCount = {totalPassCount} MaximumPassCount = {maximumPassCount}");
        }
        else
        {
          // The segment cleave failed. While this is not a serious problem (as the sub grid will be
          // cleaved at some point in the future when it is modified again via tag file processing etc)
          // it will be noted in the log.

          _log.LogWarning(
            $"Cleave on segment ({cleavedTimeRangeStart}-{cleavedTimeRangeEnd}) of sub grid {ServerSubGridTree.GetLeafSubGridFullFileName(origin)} failed. TotalPassCount = {totalPassCount} MaximumPassCount = {maximumPassCount}");
        }

        if (_segmentCleavingOperationsToLog)
        {
          if (segment.RequiresCleaving(out totalPassCount, out maximumPassCount))
            _log.LogWarning(
              $"Cleave on segment ({cleavedTimeRangeStart}-{cleavedTimeRangeEnd}) of sub grid {subGrid.Moniker()} failed to reduce cell pass count below maximums (max passes = {totalPassCount}/{subGridSegmentPassCountLimit}, per cell = {maximumPassCount}/{_subGridMaxSegmentCellPassesLimit})");
        }
      } while (iterator.MoveToNextSubGridSegment());
    }
  }
}
