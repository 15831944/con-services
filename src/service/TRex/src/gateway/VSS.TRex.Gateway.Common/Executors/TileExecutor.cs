﻿using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.Productivity3D.Models;
using VSS.TRex.Common;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Common.Models;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Designs.Models;
using VSS.TRex.Filters;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.Geometry;
using VSS.TRex.Rendering.GridFabric.Arguments;
using VSS.TRex.Rendering.GridFabric.Requests;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.Gateway.Common.Executors
{
  public class TileExecutor : BaseExecutor
  {
    public TileExecutor(IConfigurationStore configStore, ILoggerFactory logger, 
      IServiceExceptionHandler exceptionHandler) : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public TileExecutor()
    {
    }

    private Guid[] GetSurveyedSurfaceExclusionList(ISiteModel siteModel, bool includeSurveyedSurfaces)
    {
      return siteModel.SurveyedSurfaces == null || includeSurveyedSurfaces ? new Guid[0] : siteModel.SurveyedSurfaces.Select(x => x.ID).ToArray();
    }

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as TRexTileRequest;

      if (request == null)
        ThrowRequestTypeCastException<TRexTileRequest>();

      BoundingWorldExtent3D extents = null;
      var hasGridCoords = false;
      if (request.BoundBoxLatLon != null)
      {
        extents = AutoMapperUtility.Automapper.Map<BoundingBox2DLatLon, BoundingWorldExtent3D>(request.BoundBoxLatLon);
      }
      else if (request.BoundBoxGrid != null)
      {
        hasGridCoords = true;
        extents = AutoMapperUtility.Automapper.Map<BoundingBox2DGrid, BoundingWorldExtent3D>(request.BoundBoxGrid);
      }

      var siteModel = GetSiteModel(request.ProjectUid);

      var filter1 = ConvertFilter(request.Filter1, siteModel);
      var arg = new TileRenderRequestArgument
      (siteModel.ID,
        request.Mode,
        ConvertColorPalettes(request, siteModel),
        extents,
        hasGridCoords,
        request.Width, // PixelsX
        request.Height, // PixelsY
        //for now always construct 2 filters for cut-fill/volumes until we do #1329
        request.Mode == DisplayMode.CutFill ?
          new FilterSet(filter1, ConvertFilter(request.Filter2, siteModel)) :
          new FilterSet(filter1),
        new DesignOffset(request.DesignDescriptor?.FileUid ?? Guid.Empty, request.DesignDescriptor?.Offset ?? 0),
        (VolumeComputationType)request.VolumeType
      );

      var tileRequest = new TileRenderRequest();
      var response = await tileRequest.ExecuteAsync(arg);

      return new TileResult(response?.TileBitmapData);
    }

    /// <summary>
    /// Processes the tile request synchronously.
    /// </summary>
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }

    private PaletteBase ConvertColorPalettes(TRexTileRequest request, ISiteModel siteModel)
    {
      const double PERCENTAGE_RANGE_MIN = 0.0;
      const double PERCENTAGE_RANGE_MAX = 100.0;
      const ushort PASS_COUNT_TARGET_RANGE_MIN = 1;
      const ushort PASS_COUNT_TARGET_RANGE_MAX = ushort.MaxValue;
      const ushort TEMPERATURE_LEVELS_MIN = 0;
      const ushort TEMPERATURE_LEVELS_MAX = 100;

      var overrides = AutoMapperUtility.Automapper.Map<OverrideParameters>(request.Overrides);

      PaletteBase convertedPalette;

      var availableTransitions = request.Palettes != null ? request.Palettes.Select(p => new Transition(p.Value, ColorUtility.UIntToColor(p.Color))).ToArray() : new Transition[0];

      switch (request.Mode)
      {
        case DisplayMode.CCA:
          convertedPalette = new CCAPalette();
          convertedPalette.PaletteTransitions = availableTransitions;
          break;
        case DisplayMode.CCASummary:
          convertedPalette = new CCASummaryPalette();

          var ccaSummaryPalette = ((CCASummaryPalette)convertedPalette);

          if (request.Palettes != null)
          {
            ccaSummaryPalette.UndercompactedColour = ColorUtility.UIntToColor(request.Palettes[0].Color);
            ccaSummaryPalette.CompactedColour = ColorUtility.UIntToColor(request.Palettes[1].Color);
            ccaSummaryPalette.OvercompactedColour = ColorUtility.UIntToColor(request.Palettes[2].Color);
          }

          break;
        case DisplayMode.CCV:
          convertedPalette = new CMVPalette();

          var cmvPalette = ((CMVPalette)convertedPalette);

          cmvPalette.CMVPercentageRange.Min = overrides?.CMVRange.Min ?? PERCENTAGE_RANGE_MIN;
          cmvPalette.CMVPercentageRange.Max = overrides?.CMVRange.Max ?? PERCENTAGE_RANGE_MAX;

          cmvPalette.UseMachineTargetCMV = !overrides?.OverrideMachineCCV ?? true;
          cmvPalette.AbsoluteTargetCMV = overrides?.OverridingMachineCCV ?? 0;

          cmvPalette.TargetCCVColour = Color.Green;
          cmvPalette.DefaultDecoupledCMVColour = Color.Black;

          cmvPalette.PaletteTransitions = availableTransitions;
          break;
        case DisplayMode.CCVPercent:
        case DisplayMode.CCVPercentSummary:
        case DisplayMode.CCVPercentChange:
          convertedPalette = new CCVPercentPalette();

          var cmvPercentPalette = ((CCVPercentPalette) convertedPalette);

          cmvPercentPalette.CMVPercentageRange.Min = overrides?.CMVRange.Min ?? PERCENTAGE_RANGE_MIN;
          cmvPercentPalette.CMVPercentageRange.Max = overrides?.CMVRange.Max ?? PERCENTAGE_RANGE_MAX;

          cmvPercentPalette.UseMachineTargetCMV = !overrides?.OverrideMachineCCV ?? true;
          cmvPercentPalette.AbsoluteTargetCMV = overrides?.OverridingMachineCCV ?? 0;

          if (request.Palettes != null)
          {
            cmvPercentPalette.WithinCMVTargetRangeColour = ColorUtility.UIntToColor(request.Palettes[0].Color);
            cmvPercentPalette.BelowCMVTargetRangeColour = ColorUtility.UIntToColor(request.Palettes[2].Color);
            cmvPercentPalette.AboveCMVTargetRangeColour = ColorUtility.UIntToColor(request.Palettes[3].Color);
          }

          break;
        case DisplayMode.CMVChange:
          convertedPalette = new CMVChangePalette();

          var cmvPercentChangePalette = ((CMVChangePalette)convertedPalette);
          
          cmvPercentChangePalette.CMVPercentageRange.Min = overrides?.CMVRange.Min ?? PERCENTAGE_RANGE_MIN;
          cmvPercentChangePalette.CMVPercentageRange.Max = overrides?.CMVRange.Max ?? PERCENTAGE_RANGE_MAX;

          cmvPercentChangePalette.UseAbsoluteValues = false;

          cmvPercentChangePalette.UseMachineTargetCMV = !overrides?.OverrideMachineCCV ?? true;
          cmvPercentChangePalette.AbsoluteTargetCMV = overrides?.OverridingMachineCCV ?? 0;

          cmvPercentChangePalette.TargetCCVColour = Color.Green;
          cmvPercentChangePalette.DefaultDecoupledCMVColour = Color.Black;

          cmvPercentChangePalette.PaletteTransitions = availableTransitions;
          break;
        case DisplayMode.CutFill:
          convertedPalette = new CutFillPalette();
          convertedPalette.PaletteTransitions = availableTransitions;
          break;
        case DisplayMode.Height:
          convertedPalette = request.Palettes != null ? new HeightPalette(request.Palettes.First().Value, request.Palettes.Last().Value) : new HeightPalette();

          ((HeightPalette)convertedPalette).ElevationPalette = request.Palettes?.Select(p => ColorUtility.UIntToColor(p.Color)).ToArray();

          break;
        case DisplayMode.MDP:
          convertedPalette = new MDPPalette();

          var mdpPalette = ((MDPPalette)convertedPalette);

          mdpPalette.MDPPercentageRange.Min = overrides?.MDPRange.Min ?? PERCENTAGE_RANGE_MIN;
          mdpPalette.MDPPercentageRange.Max = overrides?.MDPRange.Max ?? PERCENTAGE_RANGE_MAX;

          mdpPalette.UseMachineTargetMDP = !overrides?.OverrideMachineMDP ?? true;
          mdpPalette.AbsoluteTargetMDP = overrides?.OverridingMachineMDP ?? 0;

          mdpPalette.TargetMDPColour = Color.Green;

          mdpPalette.PaletteTransitions = availableTransitions;
          break;
        case DisplayMode.MDPPercentSummary:
          convertedPalette = new MDPSummaryPalette();

          var mdpSummaryPalette = ((MDPSummaryPalette)convertedPalette);

          mdpSummaryPalette.MDPPercentageRange.Min = overrides?.MDPRange.Min ?? PERCENTAGE_RANGE_MIN;
          mdpSummaryPalette.MDPPercentageRange.Max = overrides?.MDPRange.Max ?? PERCENTAGE_RANGE_MAX;

          mdpSummaryPalette.UseMachineTargetMDP = !overrides?.OverrideMachineMDP ?? true;
          mdpSummaryPalette.AbsoluteTargetMDP = overrides?.OverridingMachineMDP ?? 0;

          if (request.Palettes != null)
          {
            mdpSummaryPalette.WithinMDPTargetRangeColour = ColorUtility.UIntToColor(request.Palettes[0].Color);
            mdpSummaryPalette.BelowMDPTargetRangeColour = ColorUtility.UIntToColor(request.Palettes[2].Color);
            mdpSummaryPalette.AboveMDPTargetRangeColour = ColorUtility.UIntToColor(request.Palettes[3].Color);
          }

          break;
        case DisplayMode.PassCount:
          convertedPalette = new PassCountPalette();
          convertedPalette.PaletteTransitions = availableTransitions;
          break;
        case DisplayMode.PassCountSummary:
          convertedPalette = new PassCountSummaryPalette();

          var passCountPalette = ((PassCountSummaryPalette)convertedPalette);

          if (request.Palettes != null)
          {
            passCountPalette.AbovePassTargetRangeColour = ColorUtility.UIntToColor(request.Palettes[2].Color);
            passCountPalette.WithinPassTargetRangeColour = ColorUtility.UIntToColor(request.Palettes[1].Color);
            passCountPalette.BelowPassTargetRangeColour = ColorUtility.UIntToColor(request.Palettes[0].Color);
          }

          passCountPalette.UseMachineTargetPass = !overrides?.OverrideTargetPassCount ?? true;
          passCountPalette.TargetPassCountRange.Min = overrides?.OverridingTargetPassCountRange.Min ?? PASS_COUNT_TARGET_RANGE_MIN;
          passCountPalette.TargetPassCountRange.Max = overrides?.OverridingTargetPassCountRange.Max ?? PASS_COUNT_TARGET_RANGE_MAX;
          break;
        case DisplayMode.MachineSpeed:
          convertedPalette = new SpeedPalette();
          convertedPalette.PaletteTransitions = availableTransitions;
          break;
        case DisplayMode.TargetSpeedSummary:
          convertedPalette = new SpeedSummaryPalette();

          var speedSummaryPalette = ((SpeedSummaryPalette)convertedPalette);

          if (request.Palettes != null)
          {
            speedSummaryPalette.LowerSpeedRangeColour = ColorUtility.UIntToColor(request.Palettes[0].Color);
            speedSummaryPalette.WithinSpeedRangeColour = ColorUtility.UIntToColor(request.Palettes[1].Color);
            speedSummaryPalette.OverSpeedRangeColour = ColorUtility.UIntToColor(request.Palettes[2].Color);
          }

          speedSummaryPalette.MachineSpeedTarget.Min = overrides?.TargetMachineSpeed.Min ?? CellPassConsts.NullMachineSpeed;
          speedSummaryPalette.MachineSpeedTarget.Max = overrides?.TargetMachineSpeed.Max ?? CellPassConsts.NullMachineSpeed;
          break;
        case DisplayMode.TemperatureDetail:
          convertedPalette = new TemperaturePalette();
          convertedPalette.PaletteTransitions = availableTransitions;
          break;
        case DisplayMode.TemperatureSummary:
          convertedPalette = new TemperatureSummaryPalette();

          var temperatureSummaryPalette = ((TemperatureSummaryPalette)convertedPalette);

          if (request.Palettes != null)
          {
            temperatureSummaryPalette.AboveMaxLevelColour = ColorUtility.UIntToColor(request.Palettes[2].Color);
            temperatureSummaryPalette.WithinLevelsColour = ColorUtility.UIntToColor(request.Palettes[1].Color);
            temperatureSummaryPalette.BelowMinLevelColour = ColorUtility.UIntToColor(request.Palettes[0].Color);
          }

          temperatureSummaryPalette.UseMachineTempWarningLevels = !overrides?.OverrideTemperatureWarningLevels ?? true;
          temperatureSummaryPalette.TemperatureLevels.Min = overrides?.OverridingTemperatureWarningLevels.Min ?? TEMPERATURE_LEVELS_MIN;
          temperatureSummaryPalette.TemperatureLevels.Max = overrides?.OverridingTemperatureWarningLevels.Max ?? TEMPERATURE_LEVELS_MAX;
          break;
        case DisplayMode.CompactionCoverage:
          convertedPalette = new CompactionCoveragePalette();

          var compactionCoveragePalette = ((CompactionCoveragePalette)convertedPalette);
          compactionCoveragePalette.HasCMVData = availableTransitions[0].Color;
          compactionCoveragePalette.HasNoCMVData = availableTransitions[1].Color;
          break;
        default:
          // when adding modes, there are various other places in the code which will need work
          throw new TRexException($"No implemented colour palette for this mode ({request.Mode})");
      }

      return convertedPalette;
    }
  }
}
