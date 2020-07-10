﻿using System.IO;
using Microsoft.Extensions.Logging;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.Events.Models;
using VSS.TRex.Filters.Models;
using VSS.TRex.Profiling.Interfaces;
using VSS.TRex.Profiling.Models;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Client.Types;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Core.Utilities;

namespace VSS.TRex.SubGridTrees.Client
{
  /// <summary>
  /// The content of each cell in a MDP client leaf sub grid. Each cell stores an MDP only.
  /// </summary>
  public class ClientMDPLeafSubGrid : GenericClientLeafSubGrid<SubGridCellPassDataMDPEntryRecord>
  {
    /// <summary>
    /// Initialise the null cell values for the client subgrid
    /// </summary>
    static ClientMDPLeafSubGrid()
    {
      SubGridUtilities.SubGridDimensionalIterator((x, y) => NullCells[x, y] = SubGridCellPassDataMDPEntryRecord.NullValue);
    }
    
    /// <summary>
    /// MDP subgrids require lift processing...
    /// </summary>
    /// <returns></returns>
    public override bool WantsLiftProcessingResults() => true;

    private void Initialise()
    {
      EventPopulationFlags |=
        PopulationControlFlags.WantsTargetMDPValues |
        PopulationControlFlags.WantsEventElevationMappingModeValues;

      _gridDataType = TRex.Types.GridDataType.MDP;
    }

    /// <summary>
    /// Constructs a default client subgrid with no owner or parent, at the standard leaf bottom subgrid level,
    /// and using the default cell size and index origin offset
    /// </summary>
    public ClientMDPLeafSubGrid()
    {
      Initialise();
    }

    /*
    /// <summary>
    /// Constructor. Set the grid to MDP.
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="parent"></param>
    /// <param name="level"></param>
    /// <param name="cellSize"></param>
    /// <param name="indexOriginOffset"></param>
    public ClientMDPLeafSubGrid(ISubGridTree owner, ISubGrid parent, byte level, double cellSize, uint indexOriginOffset) : base(owner, parent, level, cellSize, indexOriginOffset)
    {
      Initialise();
    }
    */

    /// <summary>
    /// Determine if a filtered MDP is valid (not null)
    /// </summary>
    /// <param name="filteredValue"></param>
    /// <returns></returns>
    public override bool AssignableFilteredValueIsNull(ref FilteredPassData filteredValue) => filteredValue.FilteredPass.MDP == CellPassConsts.NullMDP;

    /// <summary>
    /// Assign filtered MDP value from a filtered pass to a cell
    /// </summary>
    /// <param name="cellX"></param>
    /// <param name="cellY"></param>
    /// <param name="context"></param>
    public override void AssignFilteredValue(byte cellX, byte cellY, FilteredValueAssignmentContext context)
    {
      Cells[cellX, cellY].MeasuredMDP = context.FilteredValue.FilteredPassData.FilteredPass.MDP;
      Cells[cellX, cellY].TargetMDP = context.FilteredValue.FilteredPassData.TargetValues.TargetMDP;
      Cells[cellX, cellY].IsUndercompacted = false;
      Cells[cellX, cellY].IsTooThick = false;
      Cells[cellX, cellY].IsTopLayerTooThick = false;
      Cells[cellX, cellY].IsTopLayerUndercompacted = false;
      Cells[cellX, cellY].IsOvercompacted = false;

      int lowLayerIndex = -1;
      int highLayerIndex = -1;

      IProfileLayers layers = ((IProfileCell) context.CellProfile).Layers;

      if (context.LiftParams.MDPSummarizeTopLayerOnly)
      {
        for (var i = layers.Count() - 1; i >= 0; i--)
        {
          if ((layers[i].Status & LayerStatus.Superseded) == 0)
          {
            lowLayerIndex = highLayerIndex = i;
            break;
          }
        }
      }
      else
      {
        for (var i = layers.Count() - 1; i >= 0; i--)
        {
          if ((layers[i].Status & LayerStatus.Superseded) == 0 && layers[i].MDP != CellPassConsts.NullMDP)
          {
            highLayerIndex = i;
            break;
          }
        }

        lowLayerIndex = highLayerIndex > -1 ? 0 : -1;
      }

      if (highLayerIndex > -1 && lowLayerIndex > -1)
      {
        for (var i = lowLayerIndex; i <= highLayerIndex; i++)
        {
          var layer = layers[i];

          if (layer.FilteredPassCount == 0)
            continue;

          if ((layer.Status & LayerStatus.Undercompacted) != 0)
          {
            if (i == highLayerIndex)
              Cells[cellX, cellY].IsTopLayerUndercompacted = true;
            else
              Cells[cellX, cellY].IsUndercompacted = true;
          }

          if ((layer.Status & LayerStatus.Overcompacted) != 0)
            Cells[cellX, cellY].IsOvercompacted = true;

          if ((layer.Status & LayerStatus.TooThick) != 0)
          {
            if (i == highLayerIndex)
              Cells[cellX, cellY].IsTopLayerTooThick = true;
            else
              Cells[cellX, cellY].IsTooThick = true;
          }
        }
      }
    }

    /// <summary>
    /// Fills the contents of the client leaf subgrid with a known, non-null test pattern of values
    /// </summary>
    public override void FillWithTestPattern()
    {
      ForEach((x, y) => Cells[x, y] = new SubGridCellPassDataMDPEntryRecord { MeasuredMDP = x, TargetMDP = y });
    }

    /// <summary>
    /// Determines if the leaf content of this subgrid is equal to 'other'
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public override bool LeafContentEquals(IClientLeafSubGrid other)
    {
      bool result = true;

      IGenericClientLeafSubGrid<SubGridCellPassDataMDPEntryRecord> _other = (IGenericClientLeafSubGrid<SubGridCellPassDataMDPEntryRecord>)other;
      ForEach((x, y) => result &= Cells[x, y].Equals(_other.Cells[x, y]));

      return result;
    }

    /// <summary>
    /// Determines if the MDP at the cell location is null or not.
    /// </summary>
    /// <param name="cellX"></param>
    /// <param name="cellY"></param>
    /// <returns></returns>
    public override bool CellHasValue(byte cellX, byte cellY) => Cells[cellX, cellY].MeasuredMDP != CellPassConsts.NullMDP;

    /// <summary>
    /// Provides a copy of the null value defined for cells in this client leaf subgrid
    /// </summary>
    /// <returns></returns>
    public override SubGridCellPassDataMDPEntryRecord NullCell() => SubGridCellPassDataMDPEntryRecord.NullValue;

    /// <summary>
    /// Sets all cell MDP values to null and clears the first pass and surveyed surface pass maps
    /// </summary>
    public override void Clear()
    {
      base.Clear();
    }

    /// <summary>
    /// Dumps elevations from subgrid to the log
    /// </summary>
    public override void DumpToLog(ILogger log, string title)
    {
      base.DumpToLog(log, title);
      /*
       * var
        I, J : Integer;
        S : String;
      begin
        SIGLogMessage.PublishNoODS(Nil, Format('Dump of MDP map for sub grid %s', [Moniker]) , ...);

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
             SubGridTreeConsts.SubGridTreeCellsPerSubGrid * SubGridCellPassDataMDPEntryRecord.IndicativeSizeInBytes();
    }
  }
}
