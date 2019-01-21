﻿using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SubGridTrees.Server.Utilities;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Common.Utilities;
using VSS.TRex.DI;

namespace VSS.TRex.SubGridTrees.Server
{
  public class SubGridCellSegmentPassesDataWrapper_NonStatic : SubGridCellSegmentPassesDataWrapperBase, ISubGridCellSegmentPassesDataWrapper
    {
        /// <summary>
        /// A hook that may be used to gain notification of the add, replace and remove cell pass mutations in the cell pass stack
        /// </summary>
        private readonly ICell_NonStatic_MutationHook _mutationHook = DIContext.Obtain<ICell_NonStatic_MutationHook>();
     
        public Cell_NonStatic[,] PassData = new Cell_NonStatic[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension];

        public SubGridCellSegmentPassesDataWrapper_NonStatic()
        {
        }

        public uint PassCount(uint X, uint Y)
        {
            return PassData[X, Y].PassCount;
        }

        public void AllocatePasses(uint X, uint Y, uint passCount)
        {
            PassData[X, Y].AllocatePasses(passCount);
        }

        public void AddPass(uint X, uint Y, CellPass pass, int position = -1)
        {
            _mutationHook?.AddPass(X, Y, PassData[X, Y], pass, position);

            PassData[X, Y].AddPass(pass, position);

            SegmentPassCount++;
        }

        public void ReplacePass(uint X, uint Y, int position, CellPass pass)
        {
            _mutationHook?.ReplacePass(X, Y, PassData[X, Y], position, pass);

            PassData[X, Y].ReplacePass(position, pass);
        }

        /// <summary>
        /// Removes a cell pass at a specific position within the cell passes for a cell in this segment. Only valid for mutable representations exposing this interface.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="position"></param>
        public void RemovePass(uint X, uint Y, int position)
        {
           _mutationHook?.RemovePass(X, Y, position);
           throw new NotImplementedException("Removal of cell passes is not yet supported");
        }

        public CellPass ExtractCellPass(uint X, uint Y, int passNumber)
        {
            return PassData[X, Y].Passes[passNumber];
        }

        /// <summary>
        /// Locates a cell pass occurring at or immediately after a given time within the passes for a specific cell within this segment.
        /// If there is not an exact match, the returned index is the location in the cell pass list where a cell pass 
        /// with the given time would be inserted into the list to maintain correct time ordering of the cell passes in that cell.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="time"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool LocateTime(uint X, uint Y, DateTime time, out int index)
        {
            return PassData[X, Y].LocateTime(time, out index);
        }

        public void Read(BinaryReader reader)
        {
            int TotalPasses = reader.ReadInt32();
            int MaxPassCount = reader.ReadInt32();

            int[,] PassCounts = new int[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension];

            int PassCounts_Size = PassCountSize.Calculate(MaxPassCount);

            Core.Utilities.SubGridUtilities.SubGridDimensionalIterator((i, j) =>
            {
                switch (PassCounts_Size)
                {
                    case 1: PassCounts[i, j] = reader.ReadByte(); break;
                    case 2: PassCounts[i, j] = reader.ReadInt16(); break;
                    case 3: PassCounts[i, j] = reader.ReadInt32(); break;
                    default:
                        throw new InvalidDataException($"Unknown PassCounts_Size {PassCounts_Size}");
                }
            });

          // Read all the cells from the stream
          Core.Utilities.SubGridUtilities.SubGridDimensionalIterator((i, j) =>
            {
                int PassCount_ = PassCounts[i, j];

                if (PassCount_ > 0)
                {
                    AllocatePasses(i, j, (uint)PassCount_);
                    Read(i, j, reader);

                    SegmentPassCount += PassCount_;
                }
            });
        }

        private void Read(uint X, uint Y, BinaryReader reader)
        {
            uint passCount = PassCount(X, Y);
            for (uint cellPassIndex = 0; cellPassIndex < passCount; cellPassIndex++)
            {
                PassData[X, Y].Passes[cellPassIndex].Read(reader);
            }
        }

        private void Read(uint X, uint Y, uint passNumber, BinaryReader reader)
        {
            PassData[X, Y].Passes[passNumber].Read(reader);
        }

        /// <summary>
        /// Calculate the total number of passes from all the cells present in this sub grid segment
        /// </summary>
        /// <param name="TotalPasses"></param>
        /// <param name="MaxPassCount"></param>
        public void CalculateTotalPasses(out uint TotalPasses, out uint MaxPassCount)
        {
            SegmentTotalPassesCalculator.CalculateTotalPasses(this, out TotalPasses, out MaxPassCount);
        }

        /// <summary>
        /// Calculates the time range covering all the cell passes within this segment
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        public void CalculateTimeRange(out DateTime startTime, out DateTime endTime)
        {
            SegmentTimeRangeCalculator.CalculateTimeRange(this, out startTime, out endTime);
        }

        /// <summary>
        /// Calculates the number of passes in the segment that occur before searchTime
        /// </summary>
        /// <param name="searchTime"></param>
        /// <param name="totalPasses"></param>
        /// <param name="maxPassCount"></param>
        public void CalculatePassesBeforeTime(DateTime searchTime, out uint totalPasses, out uint maxPassCount)
        {
            SegmentTimeRangeCalculator.CalculatePassesBeforeTime(this, searchTime, out totalPasses, out maxPassCount);
        }

        /// <summary>
        /// Causes this segment to adopt all cell passes from sourceSegment where those cell passes were 
        /// recorded at or later than a specific date
        /// </summary>
        /// <param name="sourceSegment"></param>
        /// <param name="atAndAfterTime"></param>
        public void AdoptCellPassesFrom(ISubGridCellSegmentPassesDataWrapper sourceSegment, DateTime atAndAfterTime)
        {
            SegmentCellPassAdopter.AdoptCellPassesFrom(this, sourceSegment, atAndAfterTime);
        }

        /// <summary>
        /// Returns a null machine ID set for nonstatic cell pass wrappers. MachineIDSets are an 
        /// optimization for read requests on compressed static cell pass representations
        /// </summary>
        /// <returns></returns>
        public BitArray GetMachineIDSet() => null;

      /// <summary>
      /// Sets the internal machine ID for the cell pass identified by x & y spatial location and passNumber.
      /// </summary>
      /// <param name="X"></param>
      /// <param name="Y"></param>
      /// <param name="passNumber"></param>
      /// <param name="internalMachineID"></param>
      public void SetInternalMachineID(uint X, uint Y, int passNumber, short internalMachineID)
      {
        PassData[X, Y].Passes[passNumber].InternalSiteModelMachineIndex = internalMachineID;
      }

      public void GetSegmentElevationRange(out double MinElev, out double MaxElev)
      {
        MinElev = Consts.NullDouble;
        MaxElev = Consts.NullDouble;

        Debug.Assert(false, "Elevation range determination for segments limited to STATIC_CELL_PASSES");
      }

      public void Write(BinaryWriter writer)
        {
            CalculateTotalPasses(out uint TotalPasses, out uint MaxPassCount);

            writer.Write(TotalPasses);
            writer.Write(MaxPassCount);

            int PassCounts_Size = PassCountSize.Calculate((int)MaxPassCount);

      // Read all the cells from the stream
          Core.Utilities.SubGridUtilities.SubGridDimensionalIterator((i, j) =>
            {
                switch (PassCounts_Size)
                {
                    case 1: writer.Write((byte)PassCount(i, j)); break;
                    case 2: writer.Write((ushort)PassCount(i, j)); break;
                    case 3: writer.Write((int)PassCount(i, j)); break;
                    default:
                        throw new InvalidDataException($"Unknown PassCounts_Size: {PassCounts_Size}");
                }
            });

      // write all the cell passes to the stream, avoiding those cells that do not have any passes
          Core.Utilities.SubGridUtilities.SubGridDimensionalIterator((i, j) => 
            {
                if (PassCount(i, j) > 0)
                {
                    Write(i, j, writer);
                }
            });
        }

        private void Write(uint X, uint Y, uint passNumber, BinaryWriter writer)
        {
            PassData[X, Y].Passes[passNumber].Write(writer);
        }

        private void Write(uint X, uint Y, BinaryWriter writer)
        {
            foreach (CellPass cellPass in PassData[X, Y].Passes)
            {
                cellPass.Write(writer);
            }
        }

        public float PassHeight(uint X, uint Y, uint passNumber)
        {
            return PassData[X, Y].Passes[passNumber].Height;
        }

        public DateTime PassTime(uint X, uint Y, uint passNumber)
        {
            return PassData[X, Y].Passes[passNumber].Time;
        }

        public void Integrate(uint X, uint Y, CellPass[] sourcePasses, uint StartIndex, uint EndIndex, out int AddedCount, out int ModifiedCount)
        {
            PassData[X, Y].Integrate(sourcePasses, StartIndex, EndIndex, out AddedCount, out ModifiedCount);
        }

        public CellPass[] ExtractCellPasses(uint X, uint Y)
        {
            return PassData[X, Y].Passes;
        }

        public CellPass Pass(uint X, uint Y, uint passIndex)
        {
            return PassData[X, Y].Passes[passIndex];
        }

        public void SetState(CellPass[,][] cellPasses)
        {
           PassData = new Cell_NonStatic[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension];

          Core.Utilities.SubGridUtilities.SubGridDimensionalIterator((x, y) => PassData[x, y].Passes = cellPasses[x, y]);
        }

        public CellPass[,][] GetState()
        {
            CellPass[,][] result = new CellPass[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension][];

          Core.Utilities.SubGridUtilities.SubGridDimensionalIterator((x, y) => result[x, y] = PassData[x, y].Passes);

            return result;
        }
    }
}
