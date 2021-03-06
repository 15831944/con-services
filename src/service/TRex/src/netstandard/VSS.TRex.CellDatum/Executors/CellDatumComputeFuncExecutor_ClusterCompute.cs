﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.CellDatum.GridFabric.Arguments;
using VSS.TRex.CellDatum.GridFabric.Responses;
using VSS.TRex.Common;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.Common.Models;
using VSS.TRex.Designs;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGrids.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Types;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.CellDatum.Executors
{
  public class CellDatumComputeFuncExecutor_ClusterCompute
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<CellDatumComputeFuncExecutor_ClusterCompute>();

    /// <summary>
    /// Constructor
    /// </summary>
    public CellDatumComputeFuncExecutor_ClusterCompute() {}

    /// <summary>
    /// Executor that implements requesting and rendering sub grid information to create the cell datum
    /// </summary>
    public async Task<CellDatumResponse_ClusterCompute> ExecuteAsync(CellDatumRequestArgument_ClusterCompute arg, SubGridSpatialAffinityKey key)
    {
      Log.LogInformation($"Performing Execute for DataModel:{arg.ProjectID}, Mode={arg.Mode}");

      var result = new CellDatumResponse_ClusterCompute { ReturnCode = CellDatumReturnCode.UnexpectedError };

      var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(arg.ProjectID);
      if (siteModel == null)
      {
        Log.LogError($"Failed to locate site model {arg.ProjectID}");
        return result;
      }

      IDesignWrapper cutFillDesign = null;
      if (arg.ReferenceDesign != null && arg.ReferenceDesign.DesignID != Guid.Empty)
      {
        var design = siteModel.Designs.Locate(arg.ReferenceDesign.DesignID);
        if (design == null)
        {
          throw new ArgumentException($"Design {arg.ReferenceDesign.DesignID} not a recognized design in project {arg.ProjectID}");
        }
        cutFillDesign = new DesignWrapper(arg.ReferenceDesign, design);
      }

      await GetProductionData(siteModel, cutFillDesign, result, arg);
      return result;
    }

    /// <summary>
    /// Gets the production data values for the requested cell
    /// </summary>
    private async Task GetProductionData(ISiteModel siteModel, IDesignWrapper cutFillDesign, CellDatumResponse_ClusterCompute result, CellDatumRequestArgument_ClusterCompute arg)
    {
      var existenceMap = siteModel.ExistenceMap;

      var utilities = DIContext.Obtain<IRequestorUtilities>();
      var requestors = utilities.ConstructRequestors(null, siteModel, arg.Overrides, arg.LiftParams,
        utilities.ConstructRequestorIntermediaries(siteModel, arg.Filters, true, GridDataType.CellProfile),
        AreaControlSet.CreateAreaControlSet(), existenceMap);

      // Get the sub grid relative cell location
      int cellX = arg.OTGCellX & SubGridTreeConsts.SubGridLocalKeyMask;
      int cellY = arg.OTGCellY & SubGridTreeConsts.SubGridLocalKeyMask;

      // Reach into the sub-grid request layer and retrieve an appropriate sub-grid
      var cellOverrideMask = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);
      cellOverrideMask.SetBit(cellX, cellY);
      requestors[0].CellOverrideMask = cellOverrideMask;

      // using the cell address get the index of cell in clientGrid
      var thisSubGridOrigin = new SubGridCellAddress(arg.OTGCellX, arg.OTGCellY);
      var requestSubGridInternalResult = requestors[0].RequestSubGridInternal(thisSubGridOrigin, true, true);
      if (requestSubGridInternalResult.requestResult != ServerRequestResult.NoError)
      {
        if (requestSubGridInternalResult.requestResult == ServerRequestResult.SubGridNotFound)
          result.ReturnCode = CellDatumReturnCode.NoValueFound;
        else
          Log.LogError($"Request for sub grid {thisSubGridOrigin} request failed with code {requestSubGridInternalResult.requestResult}");
        return;
      }

      var cell = ((ClientCellProfileLeafSubgrid) requestSubGridInternalResult.clientGrid).Cells[cellX, cellY];
      if (cell.PassCount > 0) // Cell is not in our areaControlSet...
      {
        await ExtractRequiredValue(cutFillDesign, cell, result, arg);
        result.TimeStampUTC = cell.LastPassTime;
      }
    }

    /// <summary>
    /// Gets the required datum from the cell according to the requested display mode
    /// </summary>
    private async Task ExtractRequiredValue(IDesignWrapper cutFillDesign, ClientCellProfileLeafSubgridRecord cell, CellDatumResponse_ClusterCompute result, CellDatumRequestArgument_ClusterCompute arg)
    {
      var success = false;
      int intValue;

      switch (arg.Mode)
      {
        case DisplayMode.Height:
          result.Value = cell.Height;
          success = result.Value != CellPassConsts.NullHeight;
          break;
        case DisplayMode.CCV:
        case DisplayMode.CompactionCoverage:
          result.Value = cell.LastPassValidCCV;
          success = result.Value != CellPassConsts.NullCCV;
          break;
        case DisplayMode.CCVPercent:
        case DisplayMode.CCVSummary:
        case DisplayMode.CCVPercentSummary:
          result.Value = 0; // default - no value...
          intValue = arg.Overrides.OverrideMachineCCV ? arg.Overrides.OverridingMachineCCV : cell.TargetCCV;
          if (intValue != 0)
          {
            success = cell.LastPassValidCCV != CellPassConsts.NullCCV && intValue != CellPassConsts.NullCCV;
            if (success)
              result.Value = ((double)cell.LastPassValidCCV / intValue) * 100;
          }
          break;
        case DisplayMode.PassCount:
          result.Value = cell.PassCount;
          success = result.Value != CellPassConsts.NullPassCountValue;
          break;
        case DisplayMode.PassCountSummary:
          result.Value = 0; // default - no value...
          if (arg.Overrides.OverrideTargetPassCount)
          {
            if (cell.PassCount > arg.Overrides.OverridingTargetPassCountRange.Max)
              intValue = arg.Overrides.OverridingTargetPassCountRange.Max;
            else if (cell.PassCount < arg.Overrides.OverridingTargetPassCountRange.Min)
              intValue = arg.Overrides.OverridingTargetPassCountRange.Min;
            else
              intValue = cell.PassCount;
          }
          else
            intValue = cell.TargetPassCount;
          if (intValue != 0)
          {
            success = cell.PassCount != CellPassConsts.NullPassCountValue;
            if (success)
              result.Value = ((double)cell.PassCount / intValue) * 100;
          }
          break;
        case DisplayMode.CutFill:
          result.Value = cell.Height;
          if (cutFillDesign != null)
          {
            var designSpotHeightResult = await cutFillDesign.Design.GetDesignSpotHeight(arg.ProjectID, cutFillDesign.Offset, arg.NEECoords.X, arg.NEECoords.Y);

            if (designSpotHeightResult.errorCode == DesignProfilerRequestResult.OK && designSpotHeightResult.spotHeight != CellPassConsts.NullHeight)
            {
              result.Value = result.Value - designSpotHeightResult.spotHeight;
              success = true;
            }
          }
          break;
        case DisplayMode.TemperatureSummary:
        case DisplayMode.TemperatureDetail:
          result.Value = cell.LastPassValidTemperature;
          success = cell.LastPassValidTemperature != CellPassConsts.NullMaterialTemperatureValue;
          if (success)
            result.Value = cell.LastPassValidTemperature / 10.0; // temp is stored a int a 1 point precision
          break;
        case DisplayMode.MDP:
          result.Value = cell.LastPassValidMDP;
          success = result.Value != CellPassConsts.NullMDP;
          break;
        case DisplayMode.MDPSummary:
        case DisplayMode.MDPPercent:
        case DisplayMode.MDPPercentSummary:
          result.Value = 0; // default - no value...
          intValue = arg.Overrides.OverrideMachineMDP ? arg.Overrides.OverridingMachineMDP : cell.TargetMDP;
          if (intValue != 0)
          {
            success = cell.LastPassValidMDP != CellPassConsts.NullMDP && intValue != CellPassConsts.NullMDP;
            if (success)
              result.Value = ((double)cell.LastPassValidMDP / intValue) * 100;
          }
          break;
        case DisplayMode.MachineSpeed:
          result.Value = cell.MachineSpeed;
          success = cell.MachineSpeed != Consts.NullMachineSpeed;
          break;
        case DisplayMode.CCVPercentChange:
        case DisplayMode.CMVChange:
          result.Value = cell.CCVChange;
          success = cell.CCVChange != CellPassConsts.NullCCV;
          break;
        case DisplayMode.Latency:
        case DisplayMode.RMV:
        case DisplayMode.Frequency:
        case DisplayMode.Amplitude:
        case DisplayMode.Moisture:
        case DisplayMode.GPSMode:
        case DisplayMode.VolumeCoverage:
          break;
      }

      result.ReturnCode = success ? CellDatumReturnCode.ValueFound : CellDatumReturnCode.NoValueFound;
    }
  }
}

