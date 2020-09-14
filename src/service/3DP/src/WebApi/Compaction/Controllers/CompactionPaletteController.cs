﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;
using VSS.Productivity3D.WebApi.Models.ProductionData.Executors;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApiModels.Compaction.Interfaces;
using VSS.Productivity3D.WebApiModels.Compaction.Models.Palettes;
using ColorValue = VSS.Productivity3D.WebApiModels.Compaction.Models.Palettes.ColorValue;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Controller for getting color palettes for displaying Raptor production data
  /// </summary>
  [ResponseCache(Duration = 900, VaryByQueryKeys = new[] { "*" })]
  public class CompactionPaletteController : BaseController<CompactionPaletteController>
  {
    /// <summary>
    /// Proxy for getting elevation statistics from Raptor
    /// </summary>
  private readonly IElevationExtentsProxy elevProxy;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public CompactionPaletteController(IConfigurationStore configStore, IElevationExtentsProxy elevProxy, IFileImportProxy fileImportProxy, ICompactionSettingsManager settingsManager) :
      base(configStore, fileImportProxy, settingsManager)
    {
      this.elevProxy = elevProxy;
    }

    /// <summary>
    /// Get color palettes for a project.
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <returns>Color palettes for all display types</returns>
    [ProjectVerifier]
    [Route("api/v2/colorpalettes")]
    [HttpGet]
    public async Task<CompactionColorPalettesResult> GetColorPalettes(
      [FromQuery] Guid projectUid)
    {
      Log.LogInformation("GetColorPalettes: " + Request.QueryString);
      var projectSettings = GetProjectSettingsTargets(projectUid);
      var projectSettingsColors = GetProjectSettingsColors(projectUid);

      await Task.WhenAll(projectSettings, projectSettingsColors);

      //Note: elevation palette is a separate call as it requires a filter
      List<DisplayMode> modes = new List<DisplayMode>
      {
        DisplayMode.CCV,
        DisplayMode.PassCount,
        DisplayMode.PassCountSummary,
        DisplayMode.CutFill,
        DisplayMode.TemperatureSummary,
        DisplayMode.CCVPercentSummary,
        DisplayMode.MDPPercentSummary,
        DisplayMode.TargetSpeedSummary,
        DisplayMode.CMVChange,
        DisplayMode.TemperatureDetail
      };

      DetailPalette cmvDetailPalette = null;
      DetailPalette passCountDetailPalette = null;
      SummaryPalette passCountSummaryPalette = null;
      DetailPalette cutFillPalette = null;
      SummaryPalette temperatureSummaryPalette = null;
      SummaryPalette cmvSummaryPalette = null;
      SummaryPalette mdpSummaryPalette = null;
      DetailPalette cmvPercentChangePalette = null;
      SummaryPalette speedSummaryPalette = null;
      DetailPalette temperatureDetailPalette = null;

      foreach (var mode in modes)
      {
        List<ColorValue> colorValues;
        var compactionPalette = SettingsManager.CompactionPalette(mode, null, projectSettings.Result, projectSettingsColors.Result);
        switch (mode)
        {
          case DisplayMode.CCV:
            colorValues = new List<ColorValue>();
            for (int i = 0; i < compactionPalette.Count; i++)
            {
              colorValues.Add(ColorValue.CreateColorValue(compactionPalette[i].Color,
                compactionPalette[i].Value / 10));//Raptor CMV is 10ths but return actual CMV to UI
            }
            cmvDetailPalette = DetailPalette.CreateDetailPalette(colorValues, null, null);
            break;
          case DisplayMode.PassCount:
            colorValues = new List<ColorValue>();
            for (int i = 0; i < compactionPalette.Count - 1; i++)
            {
              colorValues.Add(ColorValue.CreateColorValue(compactionPalette[i].Color,
                compactionPalette[i].Value));
            }
            passCountDetailPalette = DetailPalette.CreateDetailPalette(colorValues,
              compactionPalette[compactionPalette.Count - 1].Color, null);
            break;
          case DisplayMode.PassCountSummary:
            passCountSummaryPalette = SummaryPalette.CreateSummaryPalette(compactionPalette[2].Color,
              compactionPalette[1].Color, compactionPalette[0].Color);
            break;
          case DisplayMode.CutFill:
            colorValues = new List<ColorValue>();
            for (int i = compactionPalette.Count - 1; i >= 0; i--)
            {
              colorValues.Add(ColorValue.CreateColorValue(compactionPalette[i].Color,
                compactionPalette[i].Value));
            }
            cutFillPalette = DetailPalette.CreateDetailPalette(colorValues, null, null);
            break;
          case DisplayMode.TemperatureSummary:
            temperatureSummaryPalette = SummaryPalette.CreateSummaryPalette(compactionPalette[2].Color,
              compactionPalette[1].Color, compactionPalette[0].Color);
            break;
          case DisplayMode.CCVPercentSummary:
            cmvSummaryPalette = SummaryPalette.CreateSummaryPalette(compactionPalette[3].Color,
              compactionPalette[0].Color, compactionPalette[2].Color);
            break;
          case DisplayMode.MDPPercentSummary:
            mdpSummaryPalette = SummaryPalette.CreateSummaryPalette(compactionPalette[3].Color,
              compactionPalette[0].Color, compactionPalette[2].Color);
            break;
          case DisplayMode.TargetSpeedSummary:
            speedSummaryPalette = SummaryPalette.CreateSummaryPalette(compactionPalette[2].Color,
              compactionPalette[1].Color, compactionPalette[0].Color);
            break;
          case DisplayMode.CMVChange:
            colorValues = new List<ColorValue>();
            for (int i = 0; i < compactionPalette.Count - 1; i++)
            {
              colorValues.Add(ColorValue.CreateColorValue(compactionPalette[i].Color,
                compactionPalette[i].Value));
            }
            cmvPercentChangePalette = DetailPalette.CreateDetailPalette(colorValues,
              compactionPalette[compactionPalette.Count - 1].Color, null);
            break;
          case DisplayMode.TemperatureDetail:
            colorValues = new List<ColorValue>();
            for (int i = 0; i < compactionPalette.Count; i++)
            {
              colorValues.Add(ColorValue.CreateColorValue(compactionPalette[i].Color,
                compactionPalette[i].Value / 10));//Raptor Temperature is 10ths but return actual Temperature to UI
            }
            temperatureDetailPalette = DetailPalette.CreateDetailPalette(colorValues, null, null);
            break;
        }

      }
      return CompactionColorPalettesResult.CreateCompactionColorPalettesResult(
        cmvDetailPalette, passCountDetailPalette, passCountSummaryPalette, cutFillPalette, temperatureSummaryPalette,
        cmvSummaryPalette, mdpSummaryPalette, cmvPercentChangePalette, speedSummaryPalette, temperatureDetailPalette);
    }

    /// <summary>
    /// Get the elevation color palette for a project.
    /// </summary>
    [ProjectVerifier]
    [HttpGet("api/v2/elevationpalette")]
    public async Task<CompactionDetailPaletteResult> GetElevationPalette(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid)
    {
      Log.LogInformation("GetElevationPalette: " + Request.QueryString);

      var projectSettingsTask = GetProjectSettingsTargets(projectUid);
      var projectSettingsColorsTask = GetProjectSettingsColors(projectUid);

      var filterTask = GetCompactionFilter(projectUid, filterUid);
      var projectIdTask = GetLegacyProjectId(projectUid);

      await Task.WhenAll(projectSettingsTask, filterTask, projectIdTask);

      var elevExtents = await elevProxy.GetElevationRange(projectIdTask.Result, projectUid, filterTask.Result, projectSettingsTask.Result, CustomHeaders, GetUserId());

      var compactionPalette = SettingsManager.CompactionPalette(DisplayMode.Height, elevExtents, projectSettingsTask.Result, projectSettingsColorsTask.Result);
      
      DetailPalette elevationPalette = null;

      if (compactionPalette != null)
      {
        var colorValues = compactionPalette.Select(t => ColorValue.CreateColorValue(t.Color, t.Value)).ToList();

        elevationPalette = DetailPalette.CreateDetailPalette(colorValues,
          compactionPalette[compactionPalette.Count - 1].Color, compactionPalette[0].Color);
      }

      return CompactionDetailPaletteResult.CreateCompactionDetailPaletteResult(elevationPalette);
    }

    /// <summary>
    /// Get the CCA color palette for a project.
    /// This assumes the filter contains the machine whose palette is requested
    /// </summary>
    [ProjectVerifier]
    [HttpGet("api/v2/ccapalette")]
    public async Task<CompactionDetailPaletteResult> GetCCAPalette(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid filterUid)
    {
      Log.LogInformation($"{nameof(GetCCAPalette)}: " + Request.QueryString);

      var filterTask = GetCompactionFilter(projectUid, filterUid);
      var projectIdTask = GetLegacyProjectId(projectUid);

      await Task.WhenAll(filterTask, projectIdTask);

      //Quickest for now to use old Landfill executor
      var filter = filterTask.Result;
      if (filter?.ContributingMachines?.Count != 1)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "CCA palette requires one and only one machine filter"));
      }
      var machine = filter.ContributingMachines[0];
      var request = CCAColorPaletteRequest.CreateCCAColorPaletteRequest(projectIdTask.Result, machine.AssetId, filter.StartUtc, filter.EndUtc, filter.LayerNumber, projectUid, machine.AssetUid);
      request.Validate();
      var palette = await RequestExecutorContainerFactory.Build<CCAColorPaletteExecutor>(LoggerFactory,
#if RAPTOR
        RaptorClient,
#endif   
        configStore: ConfigStore,
        trexCompactionDataProxy: TRexCompactionDataProxy).ProcessAsync(request) as CCAColorPaletteResult;

      return CompactionDetailPaletteResult.CreateCompactionDetailPaletteResult(
        DetailPalette.CreateDetailPalette(
          palette.Palettes.Select(x => ColorValue.CreateColorValue(x.Color, x.Value)).ToList(), 
          0, null));//black for too thick
    }
  }
}
