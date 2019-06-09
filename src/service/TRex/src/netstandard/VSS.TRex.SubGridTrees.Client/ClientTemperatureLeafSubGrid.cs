﻿using System.IO;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Common.Records;
using VSS.TRex.Events.Models;
using VSS.TRex.Filters.Models;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Client.Types;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Types;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.Types;

namespace VSS.TRex.SubGridTrees.Client
{   
    /// <summary>
    /// The content of each cell in a temperature client leaf sub grid. Each cell stores a temperature only.
    /// </summary>
    public class ClientTemperatureLeafSubGrid : GenericClientLeafSubGrid<SubGridCellPassDataTemperatureEntryRecord>
    {
        private void Initialise()
        {
          EventPopulationFlags |= 
            PopulationControlFlags.WantsTempWarningLevelMinValues | 
            PopulationControlFlags.WantsTempWarningLevelMaxValues;

          _gridDataType = GridDataType.Temperature;
        }

        /// <summary>
        /// Constructs a default client subgrid with no owner or parent, at the standard leaf bottom subgrid level,
        /// and using the default cell size and index origin offset
        /// </summary>
        public ClientTemperatureLeafSubGrid()
        {
          Initialise();
        }
    
        /// <summary>
        /// Initialise the null cell values for the client subgrid
        /// </summary>
        static ClientTemperatureLeafSubGrid()
        {
          SubGridUtilities.SubGridDimensionalIterator((x, y) => NullCells[x, y] = SubGridCellPassDataTemperatureEntryRecord.NullValue);
        } 

        /*
        /// <summary>
        /// Constructor. Set the grid to Temperature.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="parent"></param>
        /// <param name="level"></param>
        /// <param name="cellSize"></param>
        /// <param name="indexOriginOffset"></param>
        public ClientTemperatureLeafSubGrid(ISubGridTree owner, ISubGrid parent, byte level, double cellSize, uint indexOriginOffset) : base(owner, parent, level, cellSize, indexOriginOffset)
        {
           Initialise();
        }
        */

        /// <summary>
        /// Determine if a filtered machine speed is valid (not null)
        /// </summary>
        /// <param name="filteredValue"></param>
        /// <returns></returns>
        public override bool AssignableFilteredValueIsNull(ref FilteredPassData filteredValue) => filteredValue.FilteredPass.MaterialTemperature == CellPassConsts.NullMaterialTemperatureValue;

        /// <summary>
        /// Assign filtered height value from a filtered pass to a cell
        /// </summary>
        /// <param name="cellX"></param>
        /// <param name="cellY"></param>
        /// <param name="Context"></param>
        public override void AssignFilteredValue(byte cellX, byte cellY, FilteredValueAssignmentContext Context)
        {
          Cells[cellX, cellY].MeasuredTemperature = Context.FilteredValue.FilteredPassData.FilteredPass.MaterialTemperature;
	        Cells[cellX, cellY].TemperatureLevels.Min = Context.FilteredValue.FilteredPassData.TargetValues.TempWarningLevelMin;
	        Cells[cellX, cellY].TemperatureLevels.Max = Context.FilteredValue.FilteredPassData.TargetValues.TempWarningLevelMax;
				}

      /// <summary>
      /// Fills the contents of the client leaf subgrid with a known, non-null test pattern of values
      /// </summary>
      public override void FillWithTestPattern()
      {
        ForEach((x, y) =>
        {
          Cells[x, y] = new SubGridCellPassDataTemperatureEntryRecord
          {
            MeasuredTemperature = x,
            TemperatureLevels = new TemperatureWarningLevelsRecord
            {
              Min = x, Max = (ushort)(x + y) 
            }
          };
        });
      }

      /// <summary>
        /// Determines if the height at the cell location is null or not.
        /// </summary>
        /// <param name="cellX"></param>
        /// <param name="cellY"></param>
        /// <returns></returns>
        public override bool CellHasValue(byte cellX, byte cellY) => Cells[cellX, cellY].MeasuredTemperature != CellPassConsts.NullMaterialTemperatureValue;

        /// <summary>
        /// Provides a copy of the null value defined for cells in this client leaf sub grid
        /// </summary>
        /// <returns></returns>
        public override SubGridCellPassDataTemperatureEntryRecord NullCell() => SubGridCellPassDataTemperatureEntryRecord.NullValue;

        /// <summary>
        /// Sets all cell heights to null and clears the first pass and surveyed surface pass maps
        /// </summary>
        public override void Clear()
        {
					base.Clear();
        }

        /// <summary>
        /// Dumps elevations from subgrid to the log
        /// </summary>
        /// <param name="title"></param>
        public override void DumpToLog(string title)
        {
            base.DumpToLog(title);
            /*
             * var
              I, J : Integer;
              S : String;
            begin
              SIGLogMessage.PublishNoODS(Nil, Format('Dump of machine speed map for subgrid %s', [Moniker]) , ...);

              for I := 0 to kSubGridTreeDimension - 1 do
                begin
                  S := Format('%2d:', [I]);

                  for J := 0 to kSubGridTreeDimension - 1 do
                    if CellHasValue(I, J) then
                      S := S + Format('%9.3f', [Cells[I, J]])
                    else
                      S := S + '     Null';

                  SIGLogMessage.PublishNoODS(Nil, S, ...);
                end;
            end;
            */
        }

      /// <summary>
      /// Determines if the leaf content of this subgrid is equal to 'other'
      /// </summary>
      /// <param name="other"></param>
      /// <returns></returns>
      public override bool LeafContentEquals(IClientLeafSubGrid other)
      {
        bool result = true;

        IGenericClientLeafSubGrid<SubGridCellPassDataTemperatureEntryRecord> _other = (IGenericClientLeafSubGrid<SubGridCellPassDataTemperatureEntryRecord>)other;
        ForEach((x, y) => result &= Cells[x, y] .Equals(_other.Cells[x, y]));

        return result;
      }

        /// <summary>
        /// Write the contents of the Items array using the supplied writer
        /// This is an unimplemented override; a generic BinaryReader based implementation is not provided. 
        /// Override to implement if needed.
        /// </summary>
        /// <param name="writer"></param>
        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);

            SubGridUtilities.SubGridDimensionalIterator((x, y) => Cells[x, y].Write(writer));
        }

        /// <summary>
        /// Fill the items array by reading the binary representation using the provided reader. 
        /// This is an unimplemented override; a generic BinaryReader based implementation is not provided. 
        /// Override to implement if needed.
        /// </summary>
        /// <param name="reader"></param>
        public override void Read(BinaryReader reader)
        {
            base.Read(reader);

            SubGridUtilities.SubGridDimensionalIterator((x, y) => Cells[x, y].Read(reader));
        }

      /// <summary>
      /// Return an indicative size for memory consumption of this class to be used in cache tracking
      /// </summary>
      /// <returns></returns>
      public override int IndicativeSizeInBytes()
      {
        return base.IndicativeSizeInBytes() +
               SubGridTreeConsts.SubGridTreeCellsPerSubGrid * SubGridCellPassDataTemperatureEntryRecord.IndicativeSizeInBytes();
      }
  }
}
