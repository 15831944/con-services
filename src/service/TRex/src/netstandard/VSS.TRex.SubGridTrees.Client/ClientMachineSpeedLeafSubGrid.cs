﻿using System;
using System.IO;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Filters.Models;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.Types;

namespace VSS.TRex.SubGridTrees.Client
{
  /// <summary>
  /// The content of each cell in a height client leaf sub grid. Each cell stores an elevation only.
  /// </summary>
  public class ClientMachineSpeedLeafSubGrid : GenericClientLeafSubGrid<ushort>
  {
    /// <summary>
    /// Initialise the null cell values for the client subgrid
    /// </summary>
    static ClientMachineSpeedLeafSubGrid()
    {
      SubGridUtilities.SubGridDimensionalIterator((x, y) => NullCells[x, y] = CellPassConsts.NullMachineSpeed);
    }

    private void Initialise()
    {
      _gridDataType = GridDataType.MachineSpeed;
    }

    /// <summary>
    /// Constructs a default client subgrid with no owner or parent, at the standard leaf bottom subgrid level,
    /// and using the default cell size and index origin offset
    /// </summary>
    public ClientMachineSpeedLeafSubGrid()
    {
      Initialise();
    }

    /*
    /// <summary>
    /// Constructor. Set the grid to MachineSpeed.
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="parent"></param>
    /// <param name="level"></param>
    /// <param name="cellSize"></param>
    /// <param name="indexOriginOffset"></param>
    public ClientMachineSpeedLeafSubGrid(ISubGridTree owner, ISubGrid parent, byte level, double cellSize, uint indexOriginOffset) : base(owner, parent, level, cellSize, indexOriginOffset)
    {
      Initialise();
    }
    */

    /// <summary>
    /// Determine if a filtered machine speed is valid (not null)
    /// </summary>
    /// <param name="filteredValue"></param>
    /// <returns></returns>
    public override bool AssignableFilteredValueIsNull(ref FilteredPassData filteredValue) => filteredValue.FilteredPass.MachineSpeed == CellPassConsts.NullMachineSpeed;

    /// <summary>
    /// Assign filtered height value from a filtered pass to a cell
    /// </summary>
    /// <param name="cellX"></param>
    /// <param name="cellY"></param>
    /// <param name="Context"></param>
    public override void AssignFilteredValue(byte cellX, byte cellY, FilteredValueAssignmentContext Context)
    {
      Cells[cellX, cellY] = Context.FilteredValue.FilteredPassData.FilteredPass.MachineSpeed;
    }

    /// <summary>
    /// Fills the contents of the client leaf subgrid with a known, non-null test pattern of values
    /// </summary>

    public override void FillWithTestPattern() => ForEach((x, y) => Cells[x, y] = (ushort)(x + y));

    /// <summary>
    /// Determines if the height at the cell location is null or not.
    /// </summary>
    /// <param name="cellX"></param>
    /// <param name="cellY"></param>
    /// <returns></returns>
    public override bool CellHasValue(byte cellX, byte cellY) => Cells[cellX, cellY] != CellPassConsts.NullMachineSpeed;

    /// <summary>
    /// Provides a copy of the null value defined for cells in this client leaf subgrid
    /// </summary>
    /// <returns></returns>
    public override ushort NullCell() => CellPassConsts.NullMachineSpeed;

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

      IGenericClientLeafSubGrid<ushort> _other = (IGenericClientLeafSubGrid<ushort>)other;
      ForEach((x, y) => result &= Cells[x, y] == _other.Cells[x, y]);

      return result;
    }

    /// <summary>
    /// Write the contents of the Items array using the supplied writer
    /// This is an unimplemented override; a generic BinaryReader based implementation is not provided. 
    /// Override to implement if needed.
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="buffer"></param>
    public override void Write(BinaryWriter writer, byte[] buffer)
    {
      base.Write(writer, buffer);

      Buffer.BlockCopy(Cells, 0, buffer, 0, SubGridTreeConsts.SubGridTreeCellsPerSubGrid * sizeof(ushort));
      writer.Write(buffer, 0, SubGridTreeConsts.SubGridTreeCellsPerSubGrid * sizeof(ushort));

      //SubGridUtilities.SubGridDimensionalIterator((x, y) => writer.Write(Cells[x, y]));
    }

    /// <summary>
    /// Fill the items array by reading the binary representation using the provided reader. 
    /// This is an unimplemented override; a generic BinaryReader based implementation is not provided. 
    /// Override to implement if needed.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="buffer"></param>
    public override void Read(BinaryReader reader, byte[] buffer)
    {
      base.Read(reader, buffer);

      reader.Read(buffer, 0, SubGridTreeConsts.SubGridTreeCellsPerSubGrid * sizeof(ushort));
      Buffer.BlockCopy(buffer, 0, Cells, 0, SubGridTreeConsts.SubGridTreeCellsPerSubGrid * sizeof(ushort));

      //SubGridUtilities.SubGridDimensionalIterator((x, y) => Cells[x, y] = reader.ReadUInt16());
    }

    /// <summary>
    /// Return an indicative size for memory consumption of this class to be used in cache tracking
    /// </summary>
    /// <returns></returns>
    public override int IndicativeSizeInBytes()
    {
      return base.IndicativeSizeInBytes();
    }
  }
}
