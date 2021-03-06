﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.IO;
using VSS.TRex.Common;
using VSS.TRex.Filters.Models;
using VSS.TRex.IO.Helpers;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.SurveyedSurfaces.Interfaces;

namespace VSS.TRex.SubGridTrees.Client
{
  /// <summary>
  /// Client leaf sub grid that tracks height and time for each cell
  /// This class is derived from the height leaf sub grid and decorated with times to allow efficient copy
  /// operations for serialization and assignation to the height leaf sub grid where the times are removed.
  /// </summary>
 public class ClientHeightAndTimeLeafSubGrid : ClientHeightLeafSubGrid
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<ClientHeightAndTimeLeafSubGrid>();

    /// <summary>
    /// Time values for the heights stored in the height and time structure. Times are expressed as the DateTime ticks format to promote efficient copying of arrays
    /// </summary>
    public long[,] Times = new long[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension];

    /// <summary>
    /// An array containing the content of null times for all the cells in the sub grid
    /// </summary>
    public static long[,] nullTimes = new long[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension];

    /// <summary>
    /// Initialise the null cell values for the client sub grid
    /// </summary>
    static ClientHeightAndTimeLeafSubGrid()
    {
      const long NULL_VALUE = 0; //DateTime.MinValue.Ticks;

      SubGridUtilities.SubGridDimensionalIterator((x, y) => nullTimes[x, y] = NULL_VALUE);
    }

    private void Initialise()
    {
      _gridDataType = TRex.Types.GridDataType.HeightAndTime;
    }

    /// <summary>
    /// Constructs a default client sub grid with no owner or parent, at the standard leaf bottom sub grid level,
    /// and using the default cell size and index origin offset
    /// </summary>
    public ClientHeightAndTimeLeafSubGrid()
    {
      Initialise();
    }

    /// <summary>
    /// Assign filtered height value from a filtered pass to a cell
    /// </summary>
    public override void AssignFilteredValue(byte cellX, byte cellY, FilteredValueAssignmentContext context)
    {
      base.AssignFilteredValue(cellX, cellY, context);

      Times[cellX, cellY] = context.FilteredValue.FilteredPassData.FilteredPass.Time.Ticks;
    }

    /// <summary>
    /// Sets all cell heights to null and clears the surveyed surface pass map
    /// </summary>
    public override void Clear()
    {
      base.Clear();

      Array.Copy(nullTimes, 0, Times, 0, SubGridTreeConsts.SubGridTreeCellsPerSubGrid);
    }

    /// <summary>
    /// Write the contents of the Items array using the supplied writer
    /// </summary>
    public override void Write(BinaryWriter writer)
    {
      base.Write(writer);

      const int BUFFER_SIZE = SubGridTreeConsts.SubGridTreeCellsPerSubGrid * sizeof(long);

      var buffer = GenericArrayPoolCacheHelper<byte>.Caches().Rent(BUFFER_SIZE);
      try
      {
        Buffer.BlockCopy(Times, 0, buffer, 0, BUFFER_SIZE);
        writer.Write(buffer, 0, BUFFER_SIZE);
      }
      finally
      {
        GenericArrayPoolCacheHelper<byte>.Caches().Return(ref buffer);
      }
    }

    /// <summary>
    /// Fill the items array by reading the binary representation using the provided reader. 
    /// </summary>
    /// <param name="reader"></param>
    public override void Read(BinaryReader reader)
    {
      base.Read(reader);

      const int BUFFER_SIZE = SubGridTreeConsts.SubGridTreeCellsPerSubGrid * sizeof(long);

      var buffer = GenericArrayPoolCacheHelper<byte>.Caches().Rent(BUFFER_SIZE);
      try
      {
        reader.Read(buffer, 0, BUFFER_SIZE);
        Buffer.BlockCopy(buffer, 0, Times, 0, BUFFER_SIZE);
      }
      finally
      {
        GenericArrayPoolCacheHelper<byte>.Caches().Return(ref buffer);
      }
    }

    /// <summary>
    /// Assign cell information from a previously cached result held in the general sub grid result cache
    /// using the supplied map to control which cells from the caches sub grid should be copied into this
    /// client leaf sub grid
    /// </summary>
    public override void AssignFromCachedPreProcessedClientSubGrid(ISubGrid source)
    {
      base.AssignFromCachedPreProcessedClientSubGrid(source);
      Array.Copy(((ClientHeightAndTimeLeafSubGrid)source).Times, Times, SubGridTreeConsts.CellsPerSubGrid);

      //SurveyedSurfaceMap.Assign(((ClientHeightAndTimeLeafSubGrid)source).SurveyedSurfaceMap);
    }
    
    /// <summary>
    /// Assign cell information from a previously cached result held in the general sub grid result cache
    /// using the supplied map to control which cells from the cached sub grid should be copied into this
    /// client leaf sub grid
    /// </summary>
    public override void AssignFromCachedPreProcessedClientSubGrid(ISubGrid source, SubGridTreeBitmapSubGridBits map)
    {
      base.AssignFromCachedPreProcessedClientSubGrid(source, map);

      // Copy all of the times as the nullity (or not) of the elevation is the determiner of a value being present
      Array.Copy(((ClientHeightAndTimeLeafSubGrid)source).Times, Times, SubGridTreeConsts.CellsPerSubGrid);

      //SurveyedSurfaceMap.Assign(((ClientHeightLeafSubGrid)source).SurveyedSurfaceMap);
      //SurveyedSurfaceMap.AndWith(map);
    }

    public override bool UpdateProcessingMapForSurveyedSurfaces(SubGridTreeBitmapSubGridBits processingMap, IList filteredSurveyedSurfaces, bool returnEarliestFilteredCellPass)
    {
      if (!(filteredSurveyedSurfaces is ISurveyedSurfaces surveyedSurfaces))
      {
        return false;
      }

      processingMap.Assign(FilterMap);

      // If we're interested in a particular cell, but we don't have any surveyed surfaces later (or earlier)
      // than the cell production data pass time (depending on PassFilter.ReturnEarliestFilteredCellPass)
      // then there's no point in asking the Design Profiler service for an elevation

      processingMap.ForEachSetBit((x, y) =>
      {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (Cells[x, y] != Consts.NullHeight &&
            !(returnEarliestFilteredCellPass ? surveyedSurfaces.HasSurfaceEarlierThan(Times[x, y]) : surveyedSurfaces.HasSurfaceLaterThan(Times[x, y])))
          processingMap.ClearBit(x, y);
      });

      return true;
    }

    public override bool PerformHeightAnnotation(SubGridTreeBitmapSubGridBits processingMap, IList filteredSurveyedSurfaces, bool returnEarliestFilteredCellPass,
      IClientLeafSubGrid surfaceElevationsSource, Func<int, int, float, bool> elevationRangeFilterLambda)
    {
      if (!(surfaceElevationsSource is ClientHeightAndTimeLeafSubGrid surfaceElevations))
      {
        _log.LogError($"{nameof(ClientHeightAndTimeLeafSubGrid)}.{nameof(PerformHeightAnnotation)} not supplied a ClientHeightAndTimeLeafSubGrid instance, but an instance of {surfaceElevationsSource?.GetType().FullName}");
        return false;
      }

      // For all cells we wanted to request a surveyed surface elevation for,
      // update the cell elevation if a non null surveyed surface of appropriate time was computed
      // Note: The surveyed surface will return all cells in the requested sub grid, not just the ones indicated in the processing map
      // IE: It is unsafe to test for null top indicate not-filtered, use the processing map iterators to cover only those cells required
      processingMap.ForEachSetBit((x, y) =>
      {
        var surveyedSurfaceCellHeight = surfaceElevations.Cells[x, y];

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (surveyedSurfaceCellHeight == Consts.NullHeight)
        {
          return;
        }

        // If we got back a surveyed surface elevation...
        var surveyedSurfaceCellTime = surfaceElevations.Times[x, y];
        var prodHeight = Cells[x, y];
        var prodTime = Times[x, y];

        // Determine if the elevation from the surveyed surface data is required based on the production data elevation being null, and
        // the relative age of the measured surveyed surface elevation compared with a non-null production data height
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (!(prodHeight == Consts.NullHeight || (returnEarliestFilteredCellPass ? surveyedSurfaceCellTime < prodTime : surveyedSurfaceCellTime > prodTime)))
        {
          // We didn't get a surveyed surface elevation, so clear the bit in the processing map to indicate there is no surveyed surface information present for it
          processingMap.ClearBit(x, y);
          return;
        }

        // Check if there is an elevation range filter in effect and whether the surveyed surface elevation data matches it
        if (elevationRangeFilterLambda != null)
        {
          if (!elevationRangeFilterLambda(x, y, surveyedSurfaceCellHeight))
          {
            // We didn't get a surveyed surface elevation, so clear the bit in the processing map to indicate there is no surveyed surface information present for it
            processingMap.ClearBit(x, y);
            return;
          }
        }

        Cells[x, y] = surveyedSurfaceCellHeight;
        Times[x, y] = surveyedSurfaceCellTime;
      });

      //        if (ClientGrid_is_TICClientSubGridTreeLeaf_HeightAndTime)
      //          ClientGridAsHeightAndTime.SurveyedSurfaceMap.Assign(ProcessingMap);
      return true;
    }
  }
}
