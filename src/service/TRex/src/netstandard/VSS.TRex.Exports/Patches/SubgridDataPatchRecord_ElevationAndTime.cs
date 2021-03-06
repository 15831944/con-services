﻿using System;
using VSS.TRex.Common;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.Common.Records;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Exports.Patches
{
  /// <summary>
  /// Represents a sub grid patch result containing elevation data
  /// </summary>
  public class SubgridDataPatchRecord_ElevationAndTime : SubgridDataPatchRecordBase
  {
    private const byte ONE_BYTE = 1;
    private const byte TWO_BYTES = 2;
    private const byte FOUR_BYTES = 4;

    private const double DOUBLE_VALUE = 1E10;
    private const byte TIME_MINIMUM_VALUE = 0;
    private const uint TIME_MAXIMUM_VALUE = 0xffffffff;

    private const short ELEVATION_OFFSET_FACTOR = 1000;
    private const double ELEVATION_OFFSET_TOLERANCE = 0.0005;

    /// <summary>
    /// The elevation offset size in bytes.
    /// </summary>
    public byte ElevationOffsetSize { get; set; }

    /// <summary>
    /// The Time offset size in bytes.
    /// </summary>
    public byte TimeOffsetSize { get; set; }

    // SubGridOriginX, SubGridOriginY are world space coordinates of the bottom left hand corner of the subgrid
    public double SubGridOriginX { get; set; }

    public double SubGridOriginY { get; set; }

    /// <summary>
    /// The elevation of the lowest cell elevation in the elevation sub grid result, expressed in grid coordinates (meters)
    /// </summary>
    public float ElevationOrigin { get; set; }

    /// <summary>
    /// The time, which elevation of the lowest cell elevation in the elevation sub grid result was reported at, expressed in seconds
    /// </summary>
    public uint TimeOrigin { get; set; }
    /// <summary>
    /// Contains the elevation and time values for cells in the grid. This array is the same dimensions as a sub grid
    /// (currently 32x32) and contains positive elevation offsets from the ElevationOrigin member, expressed in integer millimeters as well
    /// as offsets from the TimeOrigin member, expressed in seconds.
    /// </summary>
    public PatchOffsetsRecord[,] Data { get; set; }

    /// <summary>
    /// Populate requested elevation information into the sub grid result
    /// </summary>
    /// <param name="subGrid"></param>
    public override void Populate(IClientLeafSubGrid subGrid)
    {
      //========================================================================================
      byte BytesForRangeExcludingNull(uint range) => range < byte.MaxValue ? ONE_BYTE : range < ushort.MaxValue ? TWO_BYTES : FOUR_BYTES;
      //========================================================================================

      base.Populate(subGrid);

      ClientHeightAndTimeLeafSubGrid elevSubGrid = (ClientHeightAndTimeLeafSubGrid)subGrid;
      var elevations = elevSubGrid.Cells;
      var times = elevSubGrid.Times;
      IsNull = true;

      subGrid.CalculateWorldOrigin(out var worldOriginX, out var worldOriginY);
      SubGridOriginX = worldOriginX;
      SubGridOriginY = worldOriginY;

      if (elevSubGrid.Cells != null)
      {
        // Determine the minimum/maximum non-null elevation/time in the sub grid
        double minElevation = DOUBLE_VALUE;
        double maxElevation = -DOUBLE_VALUE;
        uint minTime = TIME_MAXIMUM_VALUE;
        uint maxTime = TIME_MINIMUM_VALUE;

        SubGridUtilities.SubGridDimensionalIterator((x, y) =>
        {
          var valueHeight = elevations[x, y];

          if (Math.Abs(valueHeight - CellPassConsts.NullHeight) > Consts.TOLERANCE_DIMENSION)
          {
            if (valueHeight < minElevation)
              minElevation = valueHeight;
            if (valueHeight > maxElevation)
              maxElevation = valueHeight;

            long valueTime = times[x, y];

            if (valueTime < minTime)
              minTime = (uint) valueTime;
            if (valueTime > maxTime)
              minTime = (uint) valueTime;
          }
        });

        if (Math.Abs(minElevation - CellPassConsts.NullHeight) >= Consts.TOLERANCE_DIMENSION)
        {
          var minElevationAsMM = (uint) Math.Floor(minElevation * ELEVATION_OFFSET_FACTOR + ELEVATION_OFFSET_TOLERANCE);
          var maxElevationAsMM = (uint) Math.Floor(maxElevation * ELEVATION_OFFSET_FACTOR + ELEVATION_OFFSET_TOLERANCE);

          // Set the appropriate values into the result
          Data = new PatchOffsetsRecord[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension];
          IsNull = false;

          ElevationOrigin = (float) minElevation;
          ElevationOffsetSize = BytesForRangeExcludingNull(maxElevationAsMM - minElevationAsMM);

          TimeOrigin = minTime;
          TimeOffsetSize = BytesForRangeExcludingNull(minTime - maxTime);

          SubGridUtilities.SubGridDimensionalIterator((x, y) =>
          {
            var valueHeight = elevations[x, y];
            var valueTime = (uint) times[x, y];

            if (Math.Abs(valueHeight - CellPassConsts.NullHeight) < Consts.TOLERANCE_DIMENSION)
            {
              Data[x, y] = new PatchOffsetsRecord(uint.MaxValue, uint.MaxValue);
            }
            else
            {
              Data[x, y] = new PatchOffsetsRecord((uint) Math.Floor((valueHeight - minElevation) * ELEVATION_OFFSET_FACTOR + ELEVATION_OFFSET_TOLERANCE),
                                                  valueTime - minTime);
            }
          });
        }
      }
    }
  }
}
