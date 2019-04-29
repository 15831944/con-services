﻿using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.SurveyedSurfaces.Executors
{
  /// <summary>
  /// Calculate a surface patch for a sub grid by querying a set of supplied surveyed surfaces and extracting
  /// earliest, latest or composite elevation information from those surveyed surfaces
  /// </summary>
  public class CalculateSurfaceElevationPatch
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<CalculateSurfaceElevationPatch>();

    /// <summary>
    /// Local reference to the client sub grid factory
    /// </summary>
    private IClientLeafSubGridFactory ClientLeafSubGridFactory = DIContext.Obtain<IClientLeafSubGridFactory>();

    /// <summary>
    /// Private reference to the arguments provided to the executor
    /// </summary>
    private ISurfaceElevationPatchArgument Args { get; }

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public CalculateSurfaceElevationPatch()
    {
    }

    /// <summary>
    /// Constructor for the executor accepting the arguments for its operation
    /// </summary>
    /// <param name="args"></param>
    public CalculateSurfaceElevationPatch(ISurfaceElevationPatchArgument args) : this()
    {
      Args = args;
    }

    /// <summary>
    /// Performs the donkey work of the elevation patch calculation
    /// </summary>
    /// <param name="CalcResult"></param>
    /// <returns></returns>
    private IClientLeafSubGrid Calc(out DesignProfilerRequestResult CalcResult)
    {
      CalcResult = DesignProfilerRequestResult.UnknownError;

      IDesignBase Design;
      int Hint = -1;

      // if <config>.Debug_PerformDPServiceRequestHighRateLogging then
      //   SIGLogMessage.PublishNoODS(Self, Format('In %s.Execute for DataModel:%d  OTGCellBottomLeftX:%d  OTGCellBottomLeftY:%d', [Self.ClassName, Args.DataModelID, Args.OTGCellBottomLeftX, Args.OTGCellBottomLeftY]), slmcDebug);
      // InterlockedIncrement64(DesignProfilerRequestStats.NumSurfacePatchesComputed);

      //try
      //{

      if (!Enum.IsDefined(typeof(SurveyedSurfacePatchType), Args.SurveyedSurfacePatchType))
        throw new TRexException($"Unknown SurveyedSurfacePatchType: {Args.SurveyedSurfacePatchType}");

      var Patch = ClientLeafSubGridFactory.GetSubGridEx(
        Args.SurveyedSurfacePatchType == SurveyedSurfacePatchType.CompositeElevations
          ? GridDataType.CompositeHeights
          : GridDataType.HeightAndTime, 
          Args.CellSize, SubGridTreeConsts.SubGridTreeLevels, 
          Args.OTGCellBottomLeftX, Args.OTGCellBottomLeftY);

      // Assign 
      var PatchSingle = Args.SurveyedSurfacePatchType != SurveyedSurfacePatchType.CompositeElevations
        ? Patch as ClientHeightAndTimeLeafSubGrid : null;

      var PatchComposite = Args.SurveyedSurfacePatchType == SurveyedSurfacePatchType.CompositeElevations
        ? Patch as ClientCompositeHeightsLeafSubgrid : null;

      Patch.CalculateWorldOrigin(out double OriginX, out double OriginY);

      double CellSize = Args.CellSize;
      double HalfCellSize = CellSize / 2;
      double OriginXPlusHalfCellSize = OriginX + HalfCellSize;
      double OriginYPlusHalfCellSize = OriginY + HalfCellSize;

      var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(Args.SiteModelID);
      var Designs = DIContext.Obtain<IDesignFiles>();

      // Work down through the list of surfaces in the time ordering provided by the caller
      for (int i = 0; i < Args.IncludedSurveyedSurfaces.Length; i++)
      {
        if (Args.ProcessingMap.IsEmpty())
          break;

        var ThisSurveyedSurface = siteModel.SurveyedSurfaces.Locate(Args.IncludedSurveyedSurfaces[i]);

        // Lock & load the design
        Design = Designs.Lock(ThisSurveyedSurface.DesignDescriptor.DesignID, Args.SiteModelID, Args.CellSize, out _);

        if (Design == null)
        {
          Log.LogError($"Failed to read design file {ThisSurveyedSurface.DesignDescriptor} in {nameof(CalculateSurfaceElevationPatch)}");
          CalcResult = DesignProfilerRequestResult.FailedToLoadDesignFile;
          return null;
        }

        try
        {
          // Todo: Determine if this exclusive lock acquisition is really necessary
          Design.AcquireExclusiveInterlock();
          try
          {
            if (!Design.HasElevationDataForSubGridPatch(
              Args.OTGCellBottomLeftX >> SubGridTreeConsts.SubGridIndexBitsPerLevel,
              Args.OTGCellBottomLeftY >> SubGridTreeConsts.SubGridIndexBitsPerLevel))
              continue;

            long AsAtDate = ThisSurveyedSurface.AsAtDate.Ticks;
            double Offset = 0;//ThisSurveyedSurface.DesignDescriptor.Offset;  //TODO: #81789

            // Walk across the sub grid checking for a design elevation for each appropriate cell
            // based on the processing bit mask passed in
            Args.ProcessingMap.ForEachSetBit((x, y) =>
            {
              // If we can interpolate a height for the requested cell, then update the cell height
              // and decrement the bit count so that we know when we've handled all the requested cells

              if (Design.InterpolateHeight(ref Hint,
                OriginXPlusHalfCellSize + CellSize * x, OriginYPlusHalfCellSize + CellSize * y,
                Offset, out double z))
              {
                // Check for composite elevation processing
                if (Args.SurveyedSurfacePatchType == SurveyedSurfacePatchType.CompositeElevations)
                {
                  // Set the first elevation if not already set
                  if (PatchComposite.Cells[x, y].FirstHeightTime == 0)
                  {
                    PatchComposite.Cells[x, y].FirstHeightTime = AsAtDate;
                    PatchComposite.Cells[x, y].FirstHeight = (float) z;
                  }

                  // Always set the latest elevation (surfaces ordered by increasing date)
                  PatchComposite.Cells[x, y].LastHeightTime = AsAtDate;
                  PatchComposite.Cells[x, y].LastHeight = (float) z;

                  // Update the lowest height
                  if (PatchComposite.Cells[x, y].LowestHeightTime == 0 ||
                      PatchComposite.Cells[x, y].LowestHeight > z)
                  {
                    PatchComposite.Cells[x, y].LowestHeightTime = AsAtDate;
                    PatchComposite.Cells[x, y].LowestHeight = (float) z;
                  }

                  // Update the highest height
                  if (PatchComposite.Cells[x, y].HighestHeightTime == 0 ||
                      PatchComposite.Cells[x, y].HighestHeight > z)
                  {
                    PatchComposite.Cells[x, y].HighestHeightTime = AsAtDate;
                    PatchComposite.Cells[x, y].HighestHeight = (float) z;
                  }
                }
                else // earliest/latest singular value processing
                {
                  PatchSingle.Times[x, y] = AsAtDate;
                  PatchSingle.Cells[x, y] = (float) z;
                }
              }

              // Only clear the processing bit if earliest or latest information is wanted from the surveyed surfaces
              if (Args.SurveyedSurfacePatchType != SurveyedSurfacePatchType.CompositeElevations)
                Args.ProcessingMap.ClearBit(x, y);

              return true;
            });
          }
          finally
          {
            Design.ReleaseExclusiveInterlock();
          }
        }
        finally
        {
          Designs.UnLock(ThisSurveyedSurface.DesignDescriptor.DesignID, Design);
        }
      }

      CalcResult = DesignProfilerRequestResult.OK;

      return Patch;
      // }
      // finally
      // {
      //if <config>.Debug_PerformDPServiceRequestHighRateLogging then
      //Log.LogInformation($"Out {nameof(CalculateSurfaceElevationPatch)}.Execute");
      // }
    }

    /// <summary>
    /// Performs execution business logic for this executor
    /// </summary>
    /// <returns></returns>
    public IClientLeafSubGrid Execute()
    {
      // Perform the design profile calculation
      //try
      //{
      // Calculate the patch of elevations and return it
      IClientLeafSubGrid result = Calc(out DesignProfilerRequestResult CalcResult);

      // TODO: Handle case of failure to request patch of elevations from design

      return result;
      //}
      //finally
      //{
      //if <config>.Debug_PerformDPServiceRequestHighRateLogging then
      // Log.LogInformation($"#Out# {nameof(CalculateSurfaceElevationPatch)}.Execute #Result# {CalcResult}");
      //}
    }
  }
}
