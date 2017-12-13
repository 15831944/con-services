﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VSS.Velociraptor.DesignProfiling;
using VSS.VisionLink.Raptor.Common;
using VSS.VisionLink.Raptor.Designs;
using VSS.VisionLink.Raptor.Designs.Storage;
using VSS.VisionLink.Raptor.Geometry;
using VSS.VisionLink.Raptor.GridFabric.Requests.Interfaces;
using VSS.VisionLink.Raptor.SubGridTrees;
using VSS.VisionLink.Raptor.SubGridTrees.Client;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;
using VSS.VisionLink.Raptor.SubGridTrees.Types;
using VSS.VisionLink.Raptor.SubGridTrees.Utilities;
using VSS.VisionLink.Raptor.Utilities;
using VSS.VisionLink.Raptor.Volumes.Interfaces;

namespace VSS.VisionLink.Raptor.Volumes
{
    /// <summary>
    /// Defines an aggregator that summaries simple volumes information for subgrids
    /// </summary>
    public class SimpleVolumesCalculationsAggregator : ISubGridRequestsAggregator, IResponseAggregateWith<SimpleVolumesCalculationsAggregator>
    {
        /// <summary>
        /// Defines a subgrid full of null values to run through the volumes engine in cases when 
        /// one of the two subgrids is not available to allow for correctly tracking of statistics
        /// </summary>
        private static ClientHeightLeafSubGrid NullHeightSubgrid = new ClientHeightLeafSubGrid(null, null, 0, 0, 0);

        // FCoverageMap maps the area of cells that we have considered and successfully
        // computed volume information from
      //  public SubGridTreeBitMask CoverageMap = new SubGridTreeBitMask();

        // NoChangeMap maps the area of cells that we have considered and found to have
        // had no height change between to two surfaces considered
        // SubGridTreeBitMask FNoChangeMap = new SubGridTreeBitMask();

        /// <summary>
        /// The design being used to compare heights derived from production data against to calculate per-cell volumes
        /// </summary>
        public Design ActiveDesign { get; set; } = null;

        // References necessary for correct summarisation of aggregated state

        // public LiftBuildSettings        : TICLiftBuildSettings; = null;
        // DesignProfilerService    : TDesignProfilerPublicInterface; = null;

        public long SiteModelID { get; set; } = -1;

        public bool RequiresSerialisation { get; set; } = true;

        // The sum of the aggregated summarised information relating to volumes summary based reports

        // CellsUsed records how many cells were used in the volume calculation
        public long CellsUsed { get; set; } = 0;
        public long CellsUsedCut { get; set; } = 0;
        public long CellsUsedFill { get; set; } = 0;

        // FCellsScanned records the total number of cells that were considered by
        // the engine. This includes cells outside of reference design fence boundaries
        // and cells where both base and top values may have been null.
        public long CellsScanned { get; set; } = 0;

        // FCellsDiscarded records how many cells were discarded because filtered value was null
        public long CellsDiscarded { get; set; } = 0;
        public double CellSize { get; set; } = 0;
        public VolumeComputationType VolumeType { get; set; } = VolumeComputationType.None;

        // Volume is the calculated volume deterimined by simple difference between
        // cells. It does not take into account cut/fill differences (see FCut|FillVolume)
        // This volume is the sole output for operations that apply levels to the surfaces
        public double Volume { get; set; } = 0;
        public double CutVolume { get; set; } = 0;
        public double FillVolume { get; set; } = 0;

        // CutFillVolume is the calculated volume of material that has been 'cut' and 'filled' when the
        // base surface is compared to the top surface. ie: If the top surface is below
        // the base surface at a point then that point is in 'cut'.
        public CutFillVolume CutFillVolume = new CutFillVolume(0, 0);

        public DesignDescriptor DesignDescriptor = DesignDescriptor.Null(); // no {get;set;} intentionally

        public double TopLevel { get; set; } = 0;
        public double BaseLevel { get; set; } = 0;
        public double CoverageArea { get; set; } = 0;
        public double CutArea { get; set; } = 0;
        public double FillArea { get; set; } = 0;
        public double TotalArea { get; set; } = 0;
        public BoundingWorldExtent3D BoundingExtents = BoundingWorldExtent3D.Inverted(); // no {get; set;} intentionally 

        // CutTolerance determines the tolerance (in meters) that the 'From' surface
        // needs to be above the 'To' surface before the two surfaces are not
        // considered to be equivalent, or 'on-grade', and hence there is material still remaining to
        // be cut
        public double CutTolerance { get; set; } = VolumesConsts.DEFAULT_CELL_VOLUME_CUT_TOLERANCE;

        // FillTolerance determines the tolerance (in meters) that the 'To' surface
        // needs to be above the 'From' surface before the two surfaces are not
        // considered to be equivalent, or 'on-grade', and hence there is material still remaining to
        // be cut
        public double FillTolerance { get; set; } = VolumesConsts.DEFAULT_CELL_VOLUME_FILL_TOLERANCE;

        //  TICVolumesCalculationsAggregateStateArray = Array of TICVolumesCalculationsAggregateState;

        /*
            procedure TICVolumesCalculationsAggregateState.AggregateFrom(const Source: TICVolumesCalculationsAggregateState);
            begin
          if FRequiresSerialisation then
            TMonitor.Enter(Self);
          try
          //  SIGLogMessage.PublishNoODS(Self, Format('Aggregating From:%s', [Source.ToString]), slmcDebug);
          //  SIGLogMessage.PublishNoODS(Self, Format('Into:%s', [ToString]), slmcDebug);

            Inc(FCellsUsed, Source.CellsUsed);
            Inc(FCellsUsedCut, Source.CellsUsedCut);
            Inc(FCellsUsedFill, Source.CellsUsedFill);
            Inc(FCellsScanned, Source.CellsScanned);
            Inc(FCellsDiscarded, Source.CellsDiscarded);

            FCoverageArea := FCoverageArea + Source.CoverageArea;
            FCutArea := FCutArea + Source.CutArea;
            FFillArea := FFillArea + Source.FillArea;
            FTotalArea := FTotalArea + Source.TotalArea;
            FBoundingExtents.Include(Source.BoundingExtents);

            Volume := Volume + Source.Volume;
            CutFillVolume.Create(CutFillVolume.CutVolume + Source.CutFillVolume.CutVolume,
                                 CutFillVolume.FillVolume + Source.CutFillVolume.FillVolume);
          finally
            if FRequiresSerialisation then
              TMonitor.Exit(Self);
          end;
        end;
        */

        public SimpleVolumesCalculationsAggregator() : base()
        {
            // NOTE: This aggregator state is now single threaded in the context of processing subgrid
            // information into it as the processing threads access independent substate aggregators which
            // are aggregated together to form the final aggregation result. However, in contexts that do support
            // threaded access to this sturcture the FRequiresSerialisation flag should be set

            // if Assigned(Source) then
            //    Initialise(Source);
        }

        public void Finalise()
        {
            CoverageArea = CellsUsed * CellSize * CellSize;
            CutArea = CellsUsedCut * CellSize * CellSize;
            FillArea = CellsUsedFill * CellSize * CellSize;
            TotalArea = CellsScanned * CellSize * CellSize;
            BoundingExtents = CoverageMap.ComputeCellsWorldExtents();
        }

        /*
         * procedure TICVolumesCalculationsAggregateState.Initialise(const Source : TICVolumesCalculationsAggregateState);
            begin
          if Assigned(Source) then
            begin
              FCellSize := Source.CellSize;
              FDesignDescriptor := Source.DesignDescriptor;
              FVolumeType := Source.VolumeType;
              FBaseLevel := Source.BaseLevel;
              FTopLevel := Source.TopLevel;

              LiftBuildSettings := Source.LiftBuildSettings;
              SiteModelID := Source.SiteModelID;
              DesignProfilerService := Source.DesignProfilerService;

              CutTolerance := Source.CutTolerance;
              FillTolerance := Source.FillTolerance;
            end;

        //  FNoChangeMap := TSubGridTreeBitMask.Create(kSubGridTreeLevels, CellSize);

          FNullHeightSubgrid := TICClientSubGridTreeLeaf_Height.Create(Nil, Nil, kSubGridTreeLevels, CellSize, 0); //cell size of datamodel in question
          FNullHeightSubgrid.Clear;

          if not FDesignDescriptor.IsNull then
            begin
              FActiveDesign := TVolumesDesign.Create;
            FActiveDesign.DesignDescriptor := FDesignDescriptor;
            end;
        end;
        */

        protected void ProcessVolumeInformationForSubgrid(ClientHeightLeafSubGrid BaseScanSubGrid,
                                                          ClientHeightLeafSubGrid TopScanSubGrid)
        {
            float BaseZ, TopZ;

            double VolumeDifference;
            bool CellUsedInVolumeCalc;

            // DesignHeights represents all the valid spot elevations for the cells in the
            // subgrid being processed
            ClientHeightLeafSubGrid DesignHeights = null;
            DesignProfilerRequestResult ProfilerRequestResult = DesignProfilerRequestResult.UnknownError;

            ISubGrid CoverageMapSubgrid;

            double BelowToleranceToCheck, AboveToleranceToCheck;
            double ElevationDiff;

            DesignHeights = null;

            // FCellArea is a handy place to store the cell area, rather than calculate
            // it all the time (value wont change);
            double CellArea = CellSize * CellSize;

            // Query the patch of elevations from the surface model for this subgrid
            if (ActiveDesign?.GetDesignHeights(SiteModelID, new SubGridCellAddress(), CellSize, out DesignHeights, out ProfilerRequestResult) == false)
            {
                if (ProfilerRequestResult != DesignProfilerRequestResult.NoElevationsInRequestedPatch)
                {
                    // TODO readd when logging available
                    //SIGLogMessage.PublishNoODS(Self, Format('Design profiler subgrid elevation request for %s failed with error %d', [BaseScanSubGrid.OriginAsCellAddress.AsText, Ord(ProfilerRequestResult)]), slmcError);
                    return;
                }
            }

            SubGridTreeBitmapSubGridBits Bits = new SubGridTreeBitmapSubGridBits(SubGridTreeBitmapSubGridBits.SubGridBitsCreationOptions.Unfilled);

            // TODO: Liftbuildsettings not available in Ignite
            bool StandardVolumeProcessing = true; // TODO: Should be -> (LiftBuildSettings.TargetLiftThickness == Consts.NullHeight || LiftBuildSettings.TargetLiftThickness <= 0)

            // If we are interested in standard volume processing use this cycle
            if (StandardVolumeProcessing)
            {
                CellsScanned += SubGridTree.SubGridTreeCellsPerSubgrid;

                for (int I = 0; I < SubGridTree.SubGridTreeDimension; I++)
                {
                    for (int J = 0; J < SubGridTree.SubGridTreeDimension; J++)
                    {
                        BaseZ = BaseScanSubGrid.Cells[I, J];

                        /* TODO - removed for Ignite POC until LiftBuildSettings is available
                        // If the user has configured a first pass thickness, then we need to subtract this height
                        // difference from the BaseZ retrieved from the current cell if this measured height was
                        // the first pass made in the cell.
                        if (LiftBuildSettings.FirstPassThickness > 0)
                        {
                            BaseZ -= LiftBuildSettings.FirstPassThickness;
                        }
                        */

                        if (VolumeType == VolumeComputationType.BetweenFilterAndDesign || VolumeType == VolumeComputationType.BetweenDesignAndFilter)
                        {
                            TopZ = DesignHeights == null ? Consts.NullHeight : DesignHeights.Cells[I, J];

                            if (VolumeType == VolumeComputationType.BetweenDesignAndFilter)
                            {
                                MinMax.Swap(ref BaseZ, ref TopZ);
                            }
                        }
                        else
                        {
                            TopZ = TopScanSubGrid.Cells[I, J];
                        }

                        switch (VolumeType)
                        {
                            case VolumeComputationType.None:
                                break;

                            case VolumeComputationType.AboveLevel:
                                {
                                    if (BaseZ != Consts.NullHeight)
                                    {
                                        CellsUsed++;
                                        if (BaseZ > BaseLevel)
                                            Volume += CellArea * (BaseZ - BaseLevel);
                                    }
                                    else
                                    {
                                        CellsDiscarded++;
                                    }
                                    break;
                                }

                            case VolumeComputationType.Between2Levels:
                                {
                                    if (BaseZ != Consts.NullHeight)
                                    {
                                        CellsUsed++;

                                        if (BaseZ > BaseLevel)
                                        {
                                            Volume += CellArea * (BaseZ < TopLevel ? (BaseZ - BaseLevel) : (TopLevel - BaseLevel));
                                        }
                                    }
                                    else
                                    {
                                        CellsDiscarded++;
                                    }
                                    break;
                                }

                            case VolumeComputationType.AboveFilter:
                            case VolumeComputationType.Between2Filters:
                            case VolumeComputationType.BetweenFilterAndDesign:
                            case VolumeComputationType.BetweenDesignAndFilter:
                                {
                                    if (BaseZ != Consts.NullHeight && TopZ != Consts.NullHeight)
                                    {
                                        CellsUsed++;

                                        //  Note the fact we have processed this cell in the coverage map
                                        Bits.SetBit(I, J);

                                        // FCoverageMap.Cells[BaseScanSubGrid.OriginX + I,
                                        //                    BaseScanSubGrid.OriginY + J] := True;

                                        CellUsedInVolumeCalc = (TopZ - BaseZ >= FillTolerance) || (BaseZ - TopZ >= CutTolerance);

                                        // Accumulate volumes
                                        if (CellUsedInVolumeCalc)
                                        {
                                            VolumeDifference = CellArea * (TopZ - BaseZ);

                                            // Accumulate the 'surplus' volume. Ie: the simple summation of
                                            // all cuts and fills.
                                            Volume += VolumeDifference;

                                            // Accumulate the cuts and fills into discrete cut and fill quantities
                                            if (TopZ < BaseZ)
                                            {
                                                CellsUsedCut++;
                                                CutFillVolume.AddCutVolume(Math.Abs(VolumeDifference));
                                            }
                                            else
                                            {
                                                CellsUsedFill++;
                                                CutFillVolume.AddFillVolume(Math.Abs(VolumeDifference));
                                            }
                                        }
                                        else
                                        {
                                            // Note the fact there was no volume change in this cell
                                            // NoChangeMap.Cells[BaseScanSubGrid.OriginX + I, BaseScanSubGrid.OriginY + J] := True;
                                        }
                                    }
                                    else
                                    {
                                        CellsDiscarded++;
                                    }
                                }
                                break;

                            default:
                                //SIGLogMessage.Publish(Self, Format('Unknown volume type %d in ProcessVolumeInformationForSubgrid()', [Ord(FVolumeType)]), slmcError);
                                break;
                        }
                    }
                }
            }

            // TODO: Liftbuildsettings not available in Ignite
            bool TargetLiftThicknessCalculationsRequired = false; // TODO: Should be -> (LiftBuildSettings.TargetLiftThickness != Consts.NullHeight && LiftBuildSettings.TargetLiftThickness > 0)

            //If we are interested in thickness calculations do them
            if (TargetLiftThicknessCalculationsRequired)
            {
                /* TODO: Commented out as lift build settings not in Ignite POC
                BelowToleranceToCheck = LiftBuildSettings.TargetLiftThickness - LiftBuildSettings.BelowToleranceLiftThickness;
                AboveToleranceToCheck = LiftBuildSettings.TargetLiftThickness + LiftBuildSettings.AboveToleranceLiftThickness;
                */
                BelowToleranceToCheck = 0; // Assign value for PCO to keep compiler happy
                AboveToleranceToCheck = 0; // Assign value for PCO to keep compiler happy

                SubGridUtilities.SubGridDimensionalIterator((I, J) =>
                {
                    BaseZ = BaseScanSubGrid.Cells[I, J];
                    TopZ = TopScanSubGrid.Cells[I, J];

                    if (BaseZ != Consts.NullHeight || TopZ != Consts.NullHeight)
                    {
                        CellsScanned++;
                    }

                        //Test if we don't have NULL values and carry on
                        if (BaseZ != Consts.NullHeight && TopZ != Consts.NullHeight)
                    {
                        Bits.SetBit(I, J);
                        ElevationDiff = TopZ - BaseZ;

                        if (ElevationDiff <= AboveToleranceToCheck && ElevationDiff >= BelowToleranceToCheck)
                        {
                            CellsUsed++;
                        }
                        else
                        {
                            if (ElevationDiff > AboveToleranceToCheck)
                            {
                                CellsUsedFill++;
                            }
                            else
                            {
                                if (ElevationDiff < BelowToleranceToCheck)
                                {
                                    CellsUsedCut++;
                                }
                            }
                        }
                    }
                    else
                    {
                        CellsDiscarded++;
                    }
                });
            }

            // Record the bits for this subgrid in the coverage map by requesting the whole subgrid
            // of bits from the leaf level and setting it in one operation under an exclusive lock
            if (!Bits.IsEmpty())
            {
                if (RequiresSerialisation)
                {
                    Monitor.Enter(CoverageMap);
                }
                try
                {
                    CoverageMapSubgrid = CoverageMap.ConstructPathToCell(BaseScanSubGrid.OriginX, BaseScanSubGrid.OriginY, SubGridPathConstructionType.CreateLeaf);

                    if (CoverageMapSubgrid != null)
                    {
                        Debug.Assert(CoverageMapSubgrid is SubGridTreeLeafBitmapSubGrid, "CoverageMapSubgrid in TICVolumesCalculationsAggregateState.ProcessVolumeInformationForSubgrid is not a TSubGridTreeLeafBitmapSubGrid");
                        (CoverageMapSubgrid as SubGridTreeLeafBitmapSubGrid).Bits = Bits;
                    }
                    else
                    {
                        Debug.Assert(false, "Failed to request CoverageMapSubgrid from FCoverageMap in TICVolumesCalculationsAggregateState.ProcessVolumeInformationForSubgrid");
                    }
                }
                finally
                {
                    if (RequiresSerialisation)
                    {
                        Monitor.Exit(CoverageMap);
                    }
                }
            }
        }

        /// <summary>
        /// Summarises the client height grids derived from subgrid processing into the running volumes aggregation state
        /// </summary>
        /// <param name="subGrids"></param>
        public void SummariseSubgridResult(IClientLeafSubGrid[][] subGrids)
        {
            if (RequiresSerialisation)
            {
                Monitor.Enter(this);
            }

            try
            {
                foreach (IClientLeafSubGrid[] subGridResult in subGrids)
                {
                    // We have a subgrid from the Production Database. If we are processing volumes
                    // between two filters, then there will be a second subgrid in the sungrids array.
                    // By convention BaseSubgrid is always the first subgrid in the array,
                    // regardless of whether it really forms the 'top' or 'bottom' of the interval.

                    IClientLeafSubGrid TopSubGrid = null;
                    IClientLeafSubGrid BaseSubGrid = subGridResult[0]; //.Subgrid as TICClientSubGridTreeLeaf_Height;

                    if (BaseSubGrid == null)
                    {
                        // TODO readd when logging available
                        //SIGLogMessage.PublishNoODS(Self, Format('#W# TICVolumesCalculationsAggregateState.SummariseSubgridResult BaseSubGrid is nil', []), slmcWarning);
                        return;
                    }

                    if (subGrids.Length > 1)
                    {
                        TopSubGrid = subGridResult[1]; //.Subgrid as TICClientSubGridTreeLeaf_Height;
                        if (BaseSubGrid == null)
                        {
                            // TODO readd when logging available
                            // SIGLogMessage.PublishNoODS(Self, Format('#W# TICVolumesCalculationsAggregateState.SummariseSubgridResult TopSubGrid is nil', []), slmcWarning);
                            return;
                        };
                    }
                    else
                    {
                        TopSubGrid = NullHeightSubgrid;
                    }

                    ProcessVolumeInformationForSubgrid(BaseSubGrid as ClientHeightLeafSubGrid, TopSubGrid as ClientHeightLeafSubGrid);
                }
            }
            finally
            {
                if (RequiresSerialisation)
                {
                    Monitor.Exit(this);
                }
            }
        }

        /// <summary>
        /// Provides a human readable form of the aggregator state
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"VolumeType:{VolumeType}, Cellsize:{CellSize}, CoverageArea:{CoverageArea}, Bounding:{BoundingExtents}, " +
                $"Volume:{Volume}, Cut:{CutFillVolume.CutVolume}, Fill:{CutFillVolume.FillVolume}, " +
                $"Cells Used/Discarded/Scanned:{CellsUsed}/{CellsDiscarded}/{CellsScanned}, ReferenceDesign:{DesignDescriptor}";
        }

        /// <summary>
        /// Combine this aggregator with another simple volumes aggregator and store the result in this aggregator
        /// </summary>
        /// <param name="other"></param>
        public SimpleVolumesCalculationsAggregator AggregateWith(SimpleVolumesCalculationsAggregator other)
        {
            if (RequiresSerialisation)
            {
                //  TMonitor.Enter(Self);
            }
            try
            {
                //  SIGLogMessage.PublishNoODS(Self, Format('Aggregating From:%s', [Source.ToString]), slmcDebug);
                //  SIGLogMessage.PublishNoODS(Self, Format('Into:%s', [ToString]), slmcDebug);

                CellsUsed += other.CellsUsed;
                CellsUsedCut += other.CellsUsedCut;
                CellsUsedFill += other.CellsUsedFill;
                CellsScanned += other.CellsScanned;
                CellsDiscarded += other.CellsDiscarded;

                CoverageArea += other.CoverageArea;
                CutArea += other.CutArea;
                FillArea += other.FillArea;
                TotalArea += other.TotalArea;
                BoundingExtents.Include(other.BoundingExtents);

                Volume += other.Volume;
                CutFillVolume.AddCutFillVolume(other.CutFillVolume.CutVolume, other.CutFillVolume.FillVolume);

                return this;
            }
            finally
            {
                if (RequiresSerialisation)
                {
                 //   TMonitor.Exit(Self);
                }
            }        
        }

        /// <summary>
        /// Implement the subgrids request aggregator method ro process subgrid results...
        /// </summary>
        /// <param name="subGrids"></param>
        public void ProcessSubgridResult(IClientLeafSubGrid[][] subGrids)
        {
            SummariseSubgridResult(subGrids as ClientHeightLeafSubGrid[][]);
        }
    }
}
