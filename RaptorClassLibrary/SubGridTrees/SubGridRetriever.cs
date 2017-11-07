﻿using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Cells;
using VSS.VisionLink.Raptor.Common;
using VSS.VisionLink.Raptor.Events;
using VSS.VisionLink.Raptor.Filters;
using VSS.VisionLink.Raptor.Geometry;
using VSS.VisionLink.Raptor.Interfaces;
using VSS.VisionLink.Raptor.SiteModels;
using VSS.VisionLink.Raptor.Storage;
using VSS.VisionLink.Raptor.SubGridTrees.Client;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;
using VSS.VisionLink.Raptor.SubGridTrees.Server;
using VSS.VisionLink.Raptor.SubGridTrees.Server.Interfaces;
using VSS.VisionLink.Raptor.SubGridTrees.Server.Iterators;
using VSS.VisionLink.Raptor.SubGridTrees.Utilities;
using VSS.VisionLink.Raptor.Types;
using VSS.VisionLink.Raptor.Utilities;
using static VSS.VisionLink.Raptor.Filters.FilteredValueAssignmentContext;

namespace VSS.VisionLink.Raptor.SubGridTrees
{
    public class SubGridRetriever
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        // Local state populated for the purpose of access from variosu local methods
        private CombinedFilter Filter = null;
        private AreaControlSet AreaControlSet;
        private SiteModel SiteModel = null;
        private IClientLeafSubGrid ClientGrid = null;
        private ClientLeafSubGrid ClientGridAsLeaf = null;
        private GridDataType _GridDataType = GridDataType.All;
        private bool SeiveFilterInUse = false;
        private SubGridTreeBitmapSubGridBits SeiveBitmask;
        private SubGridTreeBitmapSubGridBits CellIterationBitmask = SubGridTreeBitmapSubGridBits.FullMask;
        ISubGrid _SubGrid = null;
        ServerSubGridTreeLeaf _SubGridAsLeaf = null;

        FilteredValueAssignmentContext AssignmentContext = null;
        SubGridSegmentIterator SegmentIterator = null;
        SubGridSegmentCellPassIterator_NonStatic CellPassIterator = null;

        double _CellSize = Consts.NullDouble;
        int NumRowsToScan, NumColsToScan;
        double FirstScanPointNorth, FirstScanPointEast;

        double StepNorthX, StepNorthY, StepEastX, StepEastY;
        double StepX, StepY;
        double IntraGridOffsetX, IntraGridOffsetY;

        // TFilteredValuePopulationControl PopulationControl = null;
        // TICServerProfiler Profiler = null;
        object Profiler = null; // Null place holder...
        // TICProfileCell CellProfile = null;

        // TICCellPassFastEventLookerUpper CellPassFastEventLookerUpper = null;

        // bool CompactionSummaryInLiftBuildSettings = false;
        // bool WorkInProgressSummaryInLiftBuildSettings = false;
        // bool ThicknessInProgressInLiftBuildSettings = false;

        // ProductionEventChanges MachineTargetValues = null;

        // long LastGetTargetValues_MachineID = -1;
        // bool MachineTargetValuesEventsLocked = false;
        bool HaveFilteredPass = false;
        FilteredPassData CurrentPass;
        FilteredPassData TempPass;
        bool PrepareGridForCacheStorageIfNoSeiving = false;

        bool CanUseGlobalLatestCells = true;
        ISubGridCellLatestPassDataWrapper _GlobalLatestCells = null;
        bool UseLastPassGrid = false; // Assume we can't use last pass data

        private void AcquirePopulationFilterValuesInterlock()
        {
            /* TODO
        if Debug_ExtremeLogSwitchD then
          SIGLogMessage.PublishNoODS(Nil, Format('In AcquirePopulationFilterValuesInterlock %d at %dx%d', [StripeIndex, CellX, CellY]), slmcDebug);

        MachineTargetValues.TargetValueChanges.AcquireReadAccessInterlock;

        if Debug_ExtremeLogSwitchD then
          SIGLogMessage.PublishNoODS(Nil, Format('In AcquirePopulationFilterValuesInterlock %d at %dx%d (Complete)', [StripeIndex, CellX, CellY]), slmcDebug);
          */
        }

        private void ReleasePopulationFilterValuesInterlock()
        {
            /* TODO
            if Debug_ExtremeLogSwitchD then
            SIGLogMessage.PublishNoODS(Nil, Format('In ReleasePopulationFilterValuesInterlock %d at %dx%d', [StripeIndex, CellX, CellY]), slmcDebug);

            if MachineTargetValuesEventsLocked then
            begin
            MachineTargetValuesEventsLocked = False;
            MachineTargetValues.TargetValueChanges.ReleaseReadAccessInterlock;
            end;

            if Debug_ExtremeLogSwitchD then
            SIGLogMessage.PublishNoODS(Nil, Format('In ReleasePopulationFilterValuesInterlock %d at %dx%d (Complete)', [StripeIndex, CellX, CellY]), slmcDebug);
            */
        }

        /* TODO GetTargetValues only called by non-supported contexts in the code below
        private ProductionEventChanges GetTargetValues(long ForMachineID)
        {
            if (ForMachineID == -1)
            {
                return null;
            }

            if (ForMachineID == LastGetTargetValues_MachineID)
            {
                return MachineTargetValues;
            }

            // Locate the machine target values of the machine we want to lock
            MachineTargetValues = SiteModel.MachinesTargetValues[ForMachineID];

            // If necessary, acquire the interlock on this set of machine target values
            if (PopulationControl.AnySet && MachineTargetValues != null)
            {
                MachineTargetValuesEventsLocked = true;
                // RCE 36155        AcquirePopulationFilterValuesInterlock;
                LastGetTargetValues_MachineID = ForMachineID;
            }
            else
            {
                LastGetTargetValues_MachineID = -1;
            }

            return MachineTargetValues;
        }
        */

        private void ProcessCellPasses(bool FilterOverriden = false)
        {
            while (CellPassIterator.MayHaveMoreFilterableCellPasses() && CellPassIterator.GetNextCellPass(ref CurrentPass.FilteredPass))
            {
                /* TODO: Population of machine events is not yet supported
                PopulateFilteredValues(SiteModel.MachinesTargetValues,
                  GetTargetValues(CurrentPass.FilteredPass.MachineID),
                  PopulationControl,
                  CurrentPass);
                */

                if (Filter.AttributeFilter.FilterPass(ref CurrentPass, FilterOverriden))
                {
                    if (Filter.AttributeFilter.HasElevationTypeFilter)
                    {
                        AssignmentContext.FilteredValue.PassCount = 1;
                    }

                    if (Filter.AttributeFilter.HasMinElevMappingFilter || (Filter.AttributeFilter.HasElevationTypeFilter && Filter.AttributeFilter.ElevationType == ElevationType.Lowest))
                    {
                        if (!HaveFilteredPass || CurrentPass.FilteredPass.Height < TempPass.FilteredPass.Height)
                        {
                            TempPass = CurrentPass;
                        }
                        HaveFilteredPass = true;
                    }
                    else
                    {
                        if (Filter.AttributeFilter.HasElevationTypeFilter && Filter.AttributeFilter.ElevationType == ElevationType.Highest)
                        {
                            if (!HaveFilteredPass || CurrentPass.FilteredPass.Height > TempPass.FilteredPass.Height)
                            {
                                TempPass = CurrentPass;
                            }
                            HaveFilteredPass = true;
                        }
                        else
                        {
                            AssignmentContext.FilteredValue.FilteredPassData = CurrentPass;
                            HaveFilteredPass = true;
                            AssignmentContext.FilteredValue.PassCount = -1;
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Performs extraction of specific attributes from a GlobalLatestCells structure depending on the type of
        /// grid being retrieved
        /// </summary>
        /// <param name="cellPass"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void AssignRequiredFilteredPassAttributesFromGlobalLatestCells(ref CellPass cellPass, int x, int y)
        {
            switch (_GridDataType)
            {
                case GridDataType.Height:
                    cellPass.Height = _GlobalLatestCells.ReadHeight(x, y);
                    break;

                case GridDataType.HeightAndTime:
                    cellPass.Height = _GlobalLatestCells.ReadHeight(x, y);
                    cellPass.Time = _GlobalLatestCells.ReadTime(x, y);
                    break;

                case GridDataType.CCV:
                    cellPass.CCV = _GlobalLatestCells.ReadCCV(x, y);
                    break;

                case GridDataType.RMV:
                    cellPass.RMV = _GlobalLatestCells.ReadRMV(x, y);
                    break;

                case GridDataType.Frequency:
                    cellPass.Frequency = _GlobalLatestCells.ReadFrequency(x, y);
                    break;

                case GridDataType.Amplitude:
                    cellPass.Amplitude = _GlobalLatestCells.ReadAmplitude(x, y);
                    break;

                case GridDataType.GPSMode:
                    cellPass.gpsMode = _GlobalLatestCells.ReadGPSMode(x, y);
                    break;

                case GridDataType.MDP:
                    cellPass.MDP = _GlobalLatestCells.ReadMDP(x, y);
                    break;

                case GridDataType.CCA:
                    cellPass.CCA = _GlobalLatestCells.ReadCCA(x, y);
                    break;

                case GridDataType.Temperature:
                    cellPass.MaterialTemperature = _GlobalLatestCells.ReadTemperature(x, y);
                    break;

                default: Debug.Assert(false, String.Format("Unsupported grid data type in AssignRequiredFilteredPassAttributesFromGlobalLatestCells: {0}", _GridDataType));
                    break;
            }
        }

        /// <summary>
        /// Retrieves the requested information from a specific cell within the subgrid currently being processed
        /// </summary>
        /// <param name="CellX"></param>
        /// <param name="CellY"></param>
        /// <returns></returns>
        public void RetrieveSubGridCell(//const PopulationControl: TFilteredValuePopulationControl;
                               byte CellX,
                               byte CellY
                               //LiftBuildSettings: TICLiftBuildSettings
                               )
        {
            // int TopMostLayerPassCount = 0;
            int TopMostLayerCompactionHalfPassCount = 0;
            bool FilteredValueIsFromLatestCellPass = false;

            IterationDirection PreviousIterationDirection;

            /* TODO...
            bool Debug_ExtremeLogSwitchD = VLPDSvcLocations.Debug_ExtremeLogSwitchD;
            */

            // Iterate over the cells in the subgrid applying the filter and assigning the
            // requested information into the subgrid


            try
            {
                /* TODO Readd when LiftBuildSettings is implemented, AND refactor this calculation so that is is made outside
                 * of the per cell loop
                 &&
                 (!(_GridDataType in [icdtCCV, icdtCCVPercent]) && (LiftBuildSettings.CCVSummaryTypes<>[])) &&
                 (!(_GridDataType in [icdtMDP, icdtMDPPercent]) && (LiftBuildSettings.MDPSummaryTypes<>[])) &&
                 (!(_GridDataType in [icdtCCA, icdtCCAPercent])) &&
                 !(_GridDataType in [icdtCellProfile,
                                            icdtPassCount,
                                            icdtCellPasses,
                                            icdtMachineSpeed,
                                            icdtCCVPercentChange,
                                            icdtMachineSpeedTarget,
                                            icdtCCVPercentChangeIgnoredTopNullValue]); */

                // For pass attributes that are maintained on a historical last pass basis
                // (meaning their values bubble up through cell passes where the values of
                // those attributes are null), check the global latest pass version of
                // those values. If they are null, then no further work needs to be done

                switch (_GridDataType)
                {
                    case GridDataType.CCV:
                        if (_GlobalLatestCells.ReadCCV(CellX, CellY) == CellPass.NullCCV) { return; }
                        break;
                    case GridDataType.RMV:
                        if (_GlobalLatestCells.ReadRMV(CellX, CellY) == CellPass.NullRMV) { return; }
                        break;
                    case GridDataType.Frequency:
                        if (_GlobalLatestCells.ReadFrequency(CellX, CellY) == CellPass.NullFrequency) { return ; }
                        break;
                    case GridDataType.Amplitude:
                        if (_GlobalLatestCells.ReadAmplitude(CellX, CellY) == CellPass.NullAmplitude) { return ; }
                        break;
                    case GridDataType.GPSMode:
                        if (_GlobalLatestCells.ReadGPSMode(CellX, CellY) == GPSMode.NoGPS) { return; }
                        break;
                    case GridDataType.MDP:
                        if (_GlobalLatestCells.ReadMDP(CellX, CellY) == CellPass.NullMDP) { return; }
                        break;
                    case GridDataType.CCA:
                        if (_GlobalLatestCells.ReadCCA(CellX, CellY) == CellPass.NullCCA) { return; }
                        break;
                    case GridDataType.Temperature:
                        if (_GlobalLatestCells.ReadTemperature(CellX, CellY) == CellPass.NullMaterialTemp) { return; }
                        break;
                }

                HaveFilteredPass = false;

                if (UseLastPassGrid)
                {
                    /* TODO - readd when logging available
                   if Debug_ExtremeLogSwitchD then
                   {
                       SIGLogMessage.PublishNoODS(Nil, Format('At %dx%d: Using last pass grid', [CellX, CellY]), slmcDebug);
                   }
                    */

                    AssignRequiredFilteredPassAttributesFromGlobalLatestCells(ref AssignmentContext.FilteredValue.FilteredPassData.FilteredPass, CellX, CellY);

                    // TODO: Review if line below replaced with line above in Ignite POC is good...
                    // AssignmentContext.FilteredValue.FilteredPassData.FilteredPass = _GlobalLatestCells[CellX, CellY];

                    HaveFilteredPass = true;
                    AssignmentContext.FilteredValue.PassCount = -1;
                }
                else
                {
                    /* TODO Readd when logging available
                    if (Debug_ExtremeLogSwitchD)
                    {
                        SIGLogMessage.PublishNoODS(Nil, Format('At %dx%d: Using profiler', [CellX, CellY]), slmcDebug);
                    }
                    */

                    Filter.AttributeFilter.InitaliaseFilteringForCell(CellX, CellY);

                    if (Profiler != null) // we don't need this anymore as the logic is implemented in lift builder
                    {
                        // While we have been given a profiler, we may not need to use it to
                        // analyse layers in the cell pass stack. The layer analysis in this
                        // operation is intended to locate cell passes belonging to superceded
                        // layers, in which case they are not considered for providing the
                        // requested value. However, if there is no filter is in effect, then the
                        // global latest information for the subgrid may be consulted first
                        // to see if the appropriate values came from the last physically collected
                        // cell pass in the cell. Note that the tracking of latest values is
                        // also true for time, so that the time recorded in the latest values structure
                        // also includes that cell pass time.

                        if (CanUseGlobalLatestCells)
                        {
                            // Optimistically assume that the global latest value is acceptable
                            AssignRequiredFilteredPassAttributesFromGlobalLatestCells(ref AssignmentContext.FilteredValue.FilteredPassData.FilteredPass, CellX, CellY);

                            // TODO: Review if line below replaced with line above in Ignite POC is good...
                            // AssignmentContext.FilteredValue.FilteredPassData.FilteredPass = _GlobalLatestCells[CellX, CellY];

                            AssignmentContext.FilteredValue.PassCount = -1;

                            // Check to see if there is a non-null value for the requested field in the latest value.
                            // If there is none, then there is no non-null value in any of the recorded cells passes
                            // so the null value may be returned as the filtered value.

                            if (ClientGrid.AssignableFilteredValueIsNull(ref AssignmentContext.FilteredValue.FilteredPassData))
                            {
                                // There is no value available for the requested data field in any recorded
                                // cell pass. Thus, there is no cell pass value to assign so abort
                                // consideration of this cell

                                return; 
                            };

                            FilteredValueIsFromLatestCellPass = false;

                            if (ClientGrid.WantsLiftProcessingResults())
                            {
                                switch (_GridDataType)
                                {
                                    case GridDataType.CCV:
                                        FilteredValueIsFromLatestCellPass = _GlobalLatestCells.CCVValuesAreFromLastPass.BitSet(CellX, CellY);
                                        break;
                                    case GridDataType.RMV:
                                        FilteredValueIsFromLatestCellPass = _GlobalLatestCells.RMVValuesAreFromLastPass.BitSet(CellX, CellY);
                                        break;
                                    case GridDataType.Frequency:
                                        FilteredValueIsFromLatestCellPass = _GlobalLatestCells.FrequencyValuesAreFromLastPass.BitSet(CellX, CellY);
                                        break;
                                    case GridDataType.Amplitude:
                                        FilteredValueIsFromLatestCellPass = _GlobalLatestCells.AmplitudeValuesAreFromLastPass.BitSet(CellX, CellY);
                                        break;
                                    case GridDataType.Temperature:
                                        FilteredValueIsFromLatestCellPass = _GlobalLatestCells.TemperatureValuesAreFromLastPass.BitSet(CellX, CellY);
                                        break;
                                    case GridDataType.GPSMode:
                                        FilteredValueIsFromLatestCellPass = _GlobalLatestCells.GPSModeValuesAreFromLatestCellPass.BitSet(CellX, CellY);
                                        break;
                                    case GridDataType.MDP:
                                        FilteredValueIsFromLatestCellPass = _GlobalLatestCells.MDPValuesAreFromLastPass.BitSet(CellX, CellY);
                                        break;
                                    case GridDataType.CCA:
                                        FilteredValueIsFromLatestCellPass = _GlobalLatestCells.CCAValuesAreFromLastPass.BitSet(CellX, CellY);
                                        break;
                                    case GridDataType.CCVPercentChange:
                                    case GridDataType.CCVPercentChangeIgnoredTopNullValue:
                                        break;
                                    case GridDataType.MachineSpeedTarget:
                                        break;
                                    case GridDataType.PassCount:
                                        // This cannot be answered here
                                        break;
                                    default:
                                        Debug.Assert(false, "Unimplemented data type for subgrid requiring lift processing results");
                                        break;
                                }
                            }

                            if (FilteredValueIsFromLatestCellPass)
                            {
                                HaveFilteredPass = FilteredValueIsFromLatestCellPass;
                            }

                            if (HaveFilteredPass)
                            {
                                /* TODO machine event poplation is not yet supported
                                PopulateFilteredValues(SiteModel.MachinesTargetValues,
                                                       GetTargetValues(AssignmentContext.FilteredValue.FilteredPassData.FilteredPass.MachineID),
                                                       PopulationControl, AssignmentContext.FilteredValue.FilteredPassData);
                                */
                            }
                        }

                        if (!HaveFilteredPass)
                        {
                            CellPassIterator.SetCellCoordinatesInSubgrid(CellX, CellY);

                            /* TODO ...
                            if (Debug_ExtremeLogSwitchD)
                            {
                                SIGLogMessage.PublishNoODS(Nil, Format('At %dx%d: Calling BuildLiftsForCell', [CellX, CellY]), slmcDebug);
                            }
                            */

                            /* TODO Cell Profiling not yet supported
                            if (Profiler.BuildLiftsForCell(cidRetrieveSubGridStripe, CellProfile,
                                                           false, LiftBuildSettings, ClientGrid,
                                                           // This subgrid wants filtered values form the cell passes being processed
                                                           AssignmentContext, // Place a filtered value into this assignment context
                                                           CellPassIterator,  // Iterate over the cells using this cell pass iterator
                                                           true, // Return an individual filtered value
                                                                 // Selection of a filtered value should occur in forwards time order
                                                           Filter.AttributeFilter, TopMostLayerPassCount,
                                                           TopMostLayerCompactionHalfPassCount))
                            {
                                // Filtered value selection is combined with lift analysis in this context via
                                // the provision of the client grid and the assignment context to the
                                // lift analysis engine
                                HaveFilteredPass = true;
                            }
                            */

                            /* TODO ...
                            if (Debug_ExtremeLogSwitchD)
                            {
                                SIGLogMessage.PublishNoODS(Nil, Format('At %dx%d: Call to BuildLiftsForCell completed', [CellX, CellY]), slmcDebug);
                            }
                            */
                        }
                    }
                    else
                    {
                        CellPassIterator.SetCellCoordinatesInSubgrid(CellX, CellY);

                        if (Filter.AttributeFilter.HasElevationRangeFilter)
                        {
                            CellPassIterator.SetIteratorElevationRange(Filter.AttributeFilter.ElevationRangeBottomElevationForCell,
                                                                       Filter.AttributeFilter.ElevationRangeTopElevationForCell);
                        }
                        CellPassIterator.Initialise();

                        ProcessCellPasses();

                        // If we have no passes in current selection and want to search further up we handle it here.
                        // It is used for the summary volumes request - see bug 31677.
                        if (!HaveFilteredPass && Filter.AttributeFilter.HasTimeFilter && Filter.AttributeFilter.OverrideTimeBoundary &&
                            !Filter.AttributeFilter.ReturnEarliestFilteredCellPass)
                        {
                            PreviousIterationDirection = CellPassIterator.SegmentIterator.IterationDirection;

                            // set date range from begining of time to end of time and find first cell pass by searching forward
                            CellPassIterator.SetTimeRange(Filter.AttributeFilter.HasTimeFilter, Filter.AttributeFilter.StartTime, DateTime.MaxValue); // Note this also sets the attached segment iterator daterange
                            CellPassIterator.SegmentIterator.IterationDirection = IterationDirection.Forwards;
                            CellPassIterator.Initialise();

                            ProcessCellPasses(true); // now search forward for first cell pass given we did not find it in the first pass b4 startdate

                            CellPassIterator.SetTimeRange(Filter.AttributeFilter.HasTimeFilter, Filter.AttributeFilter.StartTime, Filter.AttributeFilter.EndTime);
                            CellPassIterator.SegmentIterator.IterationDirection = PreviousIterationDirection;
                        }

                        if (HaveFilteredPass &&
                           (Filter.AttributeFilter.HasMinElevMappingFilter ||
                            (Filter.AttributeFilter.HasElevationTypeFilter &&
                             (Filter.AttributeFilter.ElevationType == ElevationType.Highest || Filter.AttributeFilter.ElevationType == ElevationType.Lowest))))
                        {
                            AssignmentContext.FilteredValue.FilteredPassData = TempPass;
                            AssignmentContext.FilteredValue.PassCount = -1;
                        }
                    }
                }

                if (HaveFilteredPass)
                {
                    if (_GridDataType == GridDataType.PassCount || _GridDataType == GridDataType.CellProfile)
                    {
                        AssignmentContext.FilteredValue.PassCount = TopMostLayerCompactionHalfPassCount % 2;
                    }

                    // If we are displaying a CCV summary view or are displaying a summary of only
                    // the top layer in the cell pass stack, then we need to make additional checks to
                    // determine if the CCV value filtered from the cell passes is not overridden by
                    // the layer in question being superseded. If that is the case, then the CCV value
                    // is not assigned to the result set to be passed back to the client as it effectively
                    // does not exist given this situation.

                    ClientGrid.AssignFilteredValue(CellX, CellY, AssignmentContext);

                    /* TODO: Replace single line above with implementation below when cell profiling is implemented
                    if (CellProfile == null)
                    {
                        ClientGrid.AssignFilteredValue(CellX, CellY, AssignmentContext);
                    }
                    else
                    {
                        if (((_GridDataType in [icdtCCV, icdtCCVPercent]) && ((LiftBuildSettings.CCVSummaryTypes == []) || !LiftBuildSettings.CCVSummarizeTopLayerOnly)) ||
                            ((_GridDataType in [icdtMDP, icdtMDPPercent]) && ((LiftBuildSettings.MDPSummaryTypes == []) || !LiftBuildSettings.MDPSummarizeTopLayerOnly)) ||
                            (CellProfile.Layers.Count > 0) ||
                            (_GridDataType in [icdtCCA, icdtCCAPercent])) // no CCA settings
                        { 
                            ClientGrid.AssignFilteredValue(CellX, CellY, AssignmentContext);
                        }
                    }
                    */
                }
            }
            finally
            {
                ReleasePopulationFilterValuesInterlock();

                /* TODO...
                if (Debug_ExtremeLogSwitchD)
                {
                SIGLogMessage.PublishNoODS(Nil, Format('Completed cell at %dx%d', [CellX, CellY]), slmcDebug);
                }
                */
            }
        }

        //===================================================================================================


        /// <summary>
        /// Retrieves cell values for a subgrid stripe at a time. Currently deprecated in favour of RetriveSubGridCell()
        /// </summary>
        /// <param name="StripeIndex"></param>
        /// <param name="CellX"></param>
        /// <param name="CellY"></param>
        /// <returns></returns>
        public ServerRequestResult RetrieveSubGridStripe(//const PopulationControl: TFilteredValuePopulationControl;
                               byte StripeIndex,
                               uint CellX,
                               uint CellY
                               //LiftBuildSettings: TICLiftBuildSettings
                               )
        {
            // int TopMostLayerPassCount = 0;
            int TopMostLayerCompactionHalfPassCount = 0;
            bool FilteredValueIsFromLatestCellPass = false;

            IterationDirection PreviousIterationDirection;

            /* TODO...
            bool Debug_ExtremeLogSwitchD = VLPDSvcLocations.Debug_ExtremeLogSwitchD;
            */

            // Iterate over the cells in the subgrid applying the filter and assigning the
            // requested information into the subgrid

            /* TODO...
            if (Debug_ExtremeLogSwitchD)
            {
                SIGLogMessage.PublishNoODS(Nil, Format('Beginning stripe iteration %d at %dx%d', [StripeIndex, CellX, CellY]), slmcDebug);
            }
            */

            try
            {
                /* TODO Readd when LiftBuildSettings is implemented
                 &&
                 (!(_GridDataType in [icdtCCV, icdtCCVPercent]) && (LiftBuildSettings.CCVSummaryTypes<>[])) &&
                 (!(_GridDataType in [icdtMDP, icdtMDPPercent]) && (LiftBuildSettings.MDPSummaryTypes<>[])) &&
                 (!(_GridDataType in [icdtCCA, icdtCCAPercent])) &&
                 !(_GridDataType in [icdtCellProfile,
                                            icdtPassCount,
                                            icdtCellPasses,
                                            icdtMachineSpeed,
                                            icdtCCVPercentChange,
                                            icdtMachineSpeedTarget,
                                            icdtCCVPercentChangeIgnoredTopNullValue]); */

                for (byte J = 0; J < SubGridTree.SubGridTreeDimension; J++)
                {
                    // If there is an overriding seive bitmask (from WMS rendering) then
                    // check if this cell is contained in the seive, otherwise ignore it.
                    if (SeiveFilterInUse && !SeiveBitmask.BitSet(StripeIndex, J))
                    {
                        continue;
                    }

                    if (SeiveFilterInUse || !PrepareGridForCacheStorageIfNoSeiving)
                    {
                        if (!ClientGridAsLeaf.ProdDataMap.BitSet(StripeIndex, J)) // This cell does not match the filter mask and should not be processed
                        {
                            continue;
                        }
                    }

                    // For pass attributes that are maintained on a historical last pass basis
                    // (meaning their values bubble up through cell passes where the values of
                    // those attributes are null), check the global latest pass version of
                    // those values. If they are null, then no further work needs to be done

                    switch (_GridDataType)
                    {
                        case GridDataType.CCV:
                            if (_GlobalLatestCells.ReadCCV(StripeIndex, J) == CellPass.NullCCV)
                            { continue; }
                            break;
                        case GridDataType.RMV:
                            if (_GlobalLatestCells.ReadRMV(StripeIndex, J) == CellPass.NullRMV)
                            { continue; }
                            break;
                        case GridDataType.Frequency:
                            if (_GlobalLatestCells.ReadFrequency(StripeIndex, J) == CellPass.NullFrequency)
                            { continue; }
                            break;
                        case GridDataType.Amplitude:
                            if (_GlobalLatestCells.ReadAmplitude(StripeIndex, J) == CellPass.NullAmplitude)
                            { continue; }
                            break;
                        case GridDataType.GPSMode:
                            if (_GlobalLatestCells.ReadGPSMode(StripeIndex, J) == GPSMode.NoGPS)
                            { continue; }
                            break;
                        case GridDataType.MDP:
                            if (_GlobalLatestCells.ReadMDP(StripeIndex, J) == CellPass.NullMDP)
                            { continue; }
                            break;
                        case GridDataType.CCA:
                            if (_GlobalLatestCells.ReadCCA(StripeIndex, J) == CellPass.NullCCA)
                            { continue; }
                            break;
                        case GridDataType.Temperature:
                            if (_GlobalLatestCells.ReadTemperature(StripeIndex, J) == CellPass.NullMaterialTemp)
                            { continue; }
                            break; 
                    }

                    HaveFilteredPass = false;

                    if (UseLastPassGrid)
                    {
                        /* TODO - readd when logging available
                       if Debug_ExtremeLogSwitchD then
                       {
                           SIGLogMessage.PublishNoODS(Nil, Format('SI@%d/%d at %dx%d: Using last pass grid', [StripeIndex, J, CellX, CellY]), slmcDebug);
                       }
                        */

                        AssignRequiredFilteredPassAttributesFromGlobalLatestCells(ref AssignmentContext.FilteredValue.FilteredPassData.FilteredPass, StripeIndex, J);

                        // TODO: Review if line below replaced with line above in Ignite POC is good...
                        // AssignmentContext.FilteredValue.FilteredPassData.FilteredPass = _GlobalLatestCells[StripeIndex, J];

                        HaveFilteredPass = true;
                        AssignmentContext.FilteredValue.PassCount = -1;
                    }
                    else
                    {
                        // SIGLogMessage.PublishNoODS(Nil, Format('Using profiler, StripeIndex = %d', [StripeIndex]), slmcMessage);

                        /* TODO Readd when logging available
                        if (Debug_ExtremeLogSwitchD)
                        {
                            SIGLogMessage.PublishNoODS(Nil, Format('SI@%d/%d at %dx%d: Using profiler', [StripeIndex, J, CellX, CellY]), slmcDebug);
                        }
                        */

                        Filter.AttributeFilter.InitaliaseFilteringForCell(StripeIndex, J);

                        if (Profiler != null) // we don't need this anymore as the logic is implemented in lift builder
                        {
                            // While we have been given a profiler, we may not need to use it to
                            // analyse layers in the cell pass stack. The layer analysis in this
                            // operation is intended to locate cell passes belonging to superceded
                            // layers, in which case they are not considered for providing the
                            // requested value. However, if there is no filter is in effect, then the
                            // global latest information for the subgrid may be consulted first
                            // to see if the appropriate values came from the last physically collected
                            // cell pass in the cell. Note that the tracking of latest values is
                            // also true for time, so that the time recorded in the latest values structure
                            // also includes that cell pass time.

                            if (CanUseGlobalLatestCells)
                            {
                                // Optimistically assume that the global latest value is acceptable
                                AssignRequiredFilteredPassAttributesFromGlobalLatestCells(ref AssignmentContext.FilteredValue.FilteredPassData.FilteredPass, StripeIndex, J);

                                // TODO: Review if line below replaced with line above in Ignite POC is good...
                                // AssignmentContext.FilteredValue.FilteredPassData.FilteredPass = _GlobalLatestCells[StripeIndex, J];

                                AssignmentContext.FilteredValue.PassCount = -1;

                                // Check to see if there is a non-null value for the requested field in the latest value.
                                // If there is none, then there is no non-null value in any of the recorded cells passes
                                // so the null value may be returned as the filtered value.

                                if (ClientGrid.AssignableFilteredValueIsNull(ref AssignmentContext.FilteredValue.FilteredPassData))
                                {
                                    // There is no value available for the requested data field in any recorded
                                    // cell pass. Thus, there is no cell pass value to assign so abort
                                    // consideration of this cell

                                    continue;
                                };

                                FilteredValueIsFromLatestCellPass = false;

                                if (ClientGrid.WantsLiftProcessingResults())
                                {
                                    switch (_GridDataType)
                                    {
                                        case GridDataType.CCV:
                                            FilteredValueIsFromLatestCellPass = _GlobalLatestCells.CCVValuesAreFromLastPass.BitSet(StripeIndex, J);
                                            break;
                                        case GridDataType.RMV:
                                            FilteredValueIsFromLatestCellPass = _GlobalLatestCells.RMVValuesAreFromLastPass.BitSet(StripeIndex, J);
                                            break;
                                        case GridDataType.Frequency:
                                            FilteredValueIsFromLatestCellPass = _GlobalLatestCells.FrequencyValuesAreFromLastPass.BitSet(StripeIndex, J);
                                            break;
                                        case GridDataType.Amplitude:
                                            FilteredValueIsFromLatestCellPass = _GlobalLatestCells.AmplitudeValuesAreFromLastPass.BitSet(StripeIndex, J);
                                            break;
                                        case GridDataType.Temperature:
                                            FilteredValueIsFromLatestCellPass = _GlobalLatestCells.TemperatureValuesAreFromLastPass.BitSet(StripeIndex, J);
                                            break;
                                        case GridDataType.GPSMode:
                                            FilteredValueIsFromLatestCellPass = _GlobalLatestCells.GPSModeValuesAreFromLatestCellPass.BitSet(StripeIndex, J);
                                            break;
                                        case GridDataType.MDP:
                                            FilteredValueIsFromLatestCellPass = _GlobalLatestCells.MDPValuesAreFromLastPass.BitSet(StripeIndex, J);
                                            break;
                                        case GridDataType.CCA:
                                            FilteredValueIsFromLatestCellPass = _GlobalLatestCells.CCAValuesAreFromLastPass.BitSet(StripeIndex, J);
                                            break;
                                        case GridDataType.CCVPercentChange:
                                        case GridDataType.CCVPercentChangeIgnoredTopNullValue:
                                            break;
                                        case GridDataType.MachineSpeedTarget:
                                            break;
                                        case GridDataType.PassCount:
                                            // This cannot be answered here
                                            break;
                                        default:
                                            Debug.Assert(false, "Unimplemented data type for subgrid requiring lift processing results");
                                            break;
                                    }
                                }

                                if (FilteredValueIsFromLatestCellPass)
                                {
                                    HaveFilteredPass = FilteredValueIsFromLatestCellPass;
                                }

                                if (HaveFilteredPass)
                                {
                                    /* TODO machine event poplation is not yet supported
                                    PopulateFilteredValues(SiteModel.MachinesTargetValues,
                                                           GetTargetValues(AssignmentContext.FilteredValue.FilteredPassData.FilteredPass.MachineID),
                                                           PopulationControl, AssignmentContext.FilteredValue.FilteredPassData);
                                    */
                                }
                            }

                            if (!HaveFilteredPass)
                            {
                                CellPassIterator.SetCellCoordinatesInSubgrid(StripeIndex, J);

                                /* TODO ...
                                if (Debug_ExtremeLogSwitchD)
                                {
                                    SIGLogMessage.PublishNoODS(Nil, Format('SI@%d/%d at %dx%d: Calling BuildLiftsForCell', [StripeIndex, J, CellX, CellY]), slmcDebug);
                                }
                                */

                                /* TODO Cell Profiling not yet supported
                                if (Profiler.BuildLiftsForCell(cidRetrieveSubGridStripe, CellProfile,
                                                               false, LiftBuildSettings, ClientGrid,
                                                               // This subgrid wants filtered values form the cell passes being processed
                                                               AssignmentContext, // Place a filtered value into this assignment context
                                                               CellPassIterator,  // Iterate over the cells using this cell pass iterator
                                                               true, // Return an individual filtered value
                                                                     // Selection of a filtered value should occur in forwards time order
                                                               Filter.AttributeFilter, TopMostLayerPassCount,
                                                               TopMostLayerCompactionHalfPassCount))
                                {
                                    // Filtered value selection is combined with lift analysis in this context via
                                    // the provision of the client grid and the assignment context to the
                                    // lift analysis engine
                                    HaveFilteredPass = true;
                                }
                                */

                                /* TODO ...
                                if (Debug_ExtremeLogSwitchD)
                                {
                                    SIGLogMessage.PublishNoODS(Nil, Format('SI@%d/%d at %dx%d: Call to BuildLiftsForCell completed', [StripeIndex, J, CellX, CellY]), slmcDebug);
                                }
                                */
                            }
                        }
                        else
                        {
                            CellPassIterator.SetCellCoordinatesInSubgrid(StripeIndex, J);

                            if (Filter.AttributeFilter.HasElevationRangeFilter)
                            {
                                CellPassIterator.SetIteratorElevationRange(Filter.AttributeFilter.ElevationRangeBottomElevationForCell,
                                                                           Filter.AttributeFilter.ElevationRangeTopElevationForCell);
                            }
                            CellPassIterator.Initialise();

                            ProcessCellPasses();

                            // If we have no passes in current selection and want to search further up we handle it here.
                            // It is used for the summary volumes request - see bug 31677.
                            if (!HaveFilteredPass && Filter.AttributeFilter.HasTimeFilter && Filter.AttributeFilter.OverrideTimeBoundary &&
                                !Filter.AttributeFilter.ReturnEarliestFilteredCellPass)
                            {
                                PreviousIterationDirection = CellPassIterator.SegmentIterator.IterationDirection;

                                // set date range from begining of time to end of time and find first cell pass by searching forward
                                CellPassIterator.SetTimeRange(Filter.AttributeFilter.HasTimeFilter, Filter.AttributeFilter.StartTime, DateTime.MaxValue); // Note this also sets the attached segment iterator daterange
                                CellPassIterator.SegmentIterator.IterationDirection = IterationDirection.Forwards;
                                CellPassIterator.Initialise();

                                ProcessCellPasses(true); // now search forward for first cell pass given we did not find it in the first pass b4 startdate

                                CellPassIterator.SetTimeRange(Filter.AttributeFilter.HasTimeFilter, Filter.AttributeFilter.StartTime, Filter.AttributeFilter.EndTime);
                                CellPassIterator.SegmentIterator.IterationDirection = PreviousIterationDirection;
                            }

                            if (HaveFilteredPass &&
                               (Filter.AttributeFilter.HasMinElevMappingFilter ||
                                (Filter.AttributeFilter.HasElevationTypeFilter &&
                                 (Filter.AttributeFilter.ElevationType == ElevationType.Highest || Filter.AttributeFilter.ElevationType == ElevationType.Lowest))))
                            {
                                AssignmentContext.FilteredValue.FilteredPassData = TempPass;
                                AssignmentContext.FilteredValue.PassCount = -1;
                            }
                        }
                    }

                    if (HaveFilteredPass)
                    {
                        if (_GridDataType == GridDataType.PassCount || _GridDataType == GridDataType.CellProfile)
                        {
                            AssignmentContext.FilteredValue.PassCount = TopMostLayerCompactionHalfPassCount % 2;
                        }

                        // If we are displaying a CCV summary view or are displaying a summary of only
                        // the top layer in the cell pass stack, then we need to make additional checks to
                        // determine if the CCV value filtered from the cell passes is not overridden by
                        // the layer in question being superseded. If that is the case, then the CCV value
                        // is not assigned to the result set to be passed back to the client as it effectively
                        // does not exist given this situation.

                        ClientGrid.AssignFilteredValue(StripeIndex, J, AssignmentContext);

                        /* TODO: Replace single line above with implementation below when cell profiling is implemented
                        if (CellProfile == null)
                        {
                            ClientGrid.AssignFilteredValue(StripeIndex, J, AssignmentContext);
                        }
                        else
                        {
                            if (((_GridDataType in [icdtCCV, icdtCCVPercent]) && ((LiftBuildSettings.CCVSummaryTypes == []) || !LiftBuildSettings.CCVSummarizeTopLayerOnly)) ||
                                ((_GridDataType in [icdtMDP, icdtMDPPercent]) && ((LiftBuildSettings.MDPSummaryTypes == []) || !LiftBuildSettings.MDPSummarizeTopLayerOnly)) ||
                                (CellProfile.Layers.Count > 0) ||
                                (_GridDataType in [icdtCCA, icdtCCAPercent])) // no CCA settings
                            { 
                                ClientGrid.AssignFilteredValue(StripeIndex, J, AssignmentContext);
                            }
                        }
                        */
                    }
                }

                return ServerRequestResult.NoError;
            }
            finally
            {
                ReleasePopulationFilterValuesInterlock();

                /* TODO...
                if (Debug_ExtremeLogSwitchD)
                {
                SIGLogMessage.PublishNoODS(Nil, Format('Completed stripe iteration %d at %dx%d', [StripeIndex, CellX, CellY]), slmcDebug);
                }
                */
            }
        }

        /// <summary>
        /// PruneSubGridRetrievalHere determines if there is no point continuing the
        /// process of retrieving the subgrid due to the impossibility of returning any
        /// valid values for any cells in the subgrid due to a combination of filter
        /// settings and flags set in the subgrid that denote the types of data that
        /// are, or are not, contained in the subgrid.
        /// </summary>
        /// <returns></returns>
        private bool PruneSubGridRetrievalHere()
        {
/*
            if (_SubGridAsLeaf == null)
            {
                Debug.Assert(false, "No available leaf subgrid");
                return true;
            }
*/

            // Obtain a reference to the base class from the interfaces implementation to access the flags below
            SubGridCellLatestPassDataWrapperBase baseGlobalLatestCells = (SubGridCellLatestPassDataWrapperBase)_GlobalLatestCells;

            // Check the subgrid global attribute presence flags that are tracked for optional
            // attribute values to see if there is anything at all that needs to be done here
            switch (_GridDataType)
            {
                case GridDataType.CCV: return !baseGlobalLatestCells.HasCCVData;
                case GridDataType.RMV: return !baseGlobalLatestCells.HasRMVData;
                case GridDataType.Frequency: return !baseGlobalLatestCells.HasFrequencyData;
                case GridDataType.Amplitude: return !baseGlobalLatestCells.HasAmplitudeData;
                case GridDataType.GPSMode: return !baseGlobalLatestCells.HasGPSModeData;
                case GridDataType.Temperature: return !baseGlobalLatestCells.HasTemperatureData;
                case GridDataType.MDP: return !baseGlobalLatestCells.HasMDPData;
                case GridDataType.CCA: return !baseGlobalLatestCells.HasCCAData;
                default: return false;
            }
        }

        private void SetupForCellPassStackExamination()
        {
            /* TODO readd when LiftBuldSettings is implemented
            TICServerProfiler.CalculateFlags(_GridDataType, LiftBuildSettings,
                                             CompactionSummaryInLiftBuildSettings,
                                             WorkInProgressSummaryInLiftBuildSettings,
                                             ThicknessInProgressInLiftBuildSettings);

            PopulationControl = TICServerProfiler.PreparePopulationControl(_GridDataType, LiftBuildSettings, Filter.AttributeFilter, ClientGrid);
            */

            Filter.AttributeFilter.RequestedGridDataType = _GridDataType;

            // Create and configure the segment iterator to be used

            SegmentIterator = new SubGridSegmentIterator(_SubGridAsLeaf, _SubGridAsLeaf.Directory);

            if (Filter.AttributeFilter.ReturnEarliestFilteredCellPass ||
                (Filter.AttributeFilter.HasElevationTypeFilter && (Filter.AttributeFilter.ElevationType == ElevationType.First)))
            {
                SegmentIterator.IterationDirection = IterationDirection.Forwards;
            }
            else
            {
                SegmentIterator.IterationDirection = IterationDirection.Backwards;
            }

//            SegmentIterator.SiteModelReference = SiteModel;
            SegmentIterator.SubGrid = _SubGridAsLeaf;
            SegmentIterator.Directory = _SubGridAsLeaf.Directory;
            if (Filter.AttributeFilter.HasMachineFilter)
            {
                /* TODO Re-add when machine ID sets are implemented
                SegmentIterator.SetMachineRestriction(PassFilter.FMachineIDSets);
                */
            }

            // Create and configure the cell pass iterator to be used

            CellPassIterator = new SubGridSegmentCellPassIterator_NonStatic(SegmentIterator);
            // TODO ??? CellPassIterator.SiteModelReference = SiteModel;
            CellPassIterator.SetTimeRange(Filter.AttributeFilter.HasTimeFilter,
                                          Filter.AttributeFilter.StartTime,
                                          Filter.AttributeFilter.EndTime);
        }

        private bool ComputeSeiveBitmask(ISubGrid SubGrid)
        {
            const int kMaxStepSize = 10000;

            /* TODO - add configuration item
            if (!VLPDSvcLocations.VLPDPSNode_UseSkipStepComputationForWMSSubgridRequests)
            {
                return false;
            }
            */

            if (AreaControlSet.PixelXWorldSize == 0 || AreaControlSet.PixelYWorldSize == 0)
            {
                return false;
            }

            double SubGridWorldOriginX, SubGridWorldOriginY;
            double Temp;

            int north_row, east_col;

            double CurrentNorth, CurrentEast;

            int StepX, StepY;
            double StepXIncrement, StepYIncrement;
            double StepXIncrementOverTwo, StepYIncrementOverTwo;

            // Progress through the cells in the grid, starting from the southern most
            // row in the grid and progressing from the western end to the eastern end
            // (ie: bottom to top, left to right)

            ///////////////// CalculateParameters;  START

            double StepsPerPixelX, StepsPerPixelY;

            StepsPerPixelX = AreaControlSet.PixelXWorldSize / _CellSize;
            StepsPerPixelY = AreaControlSet.PixelYWorldSize / _CellSize;

            StepX = Math.Min(kMaxStepSize, Math.Max(1, (int)Math.Truncate(StepsPerPixelX)));
            StepY = Math.Min(kMaxStepSize, Math.Max(1, (int)Math.Truncate(StepsPerPixelY)));

            StepXIncrement = StepX * _CellSize;
            StepYIncrement = StepY * _CellSize;

            StepXIncrementOverTwo = StepXIncrement / 2;
            StepYIncrementOverTwo = StepYIncrement / 2;

            ///////////////// CalculateParameters;  END

            if (StepX < 2 && StepY < 2)
            {
                return false;
            }

            if (StepX >= SubGridTree.SubGridTreeDimension && StepY >= SubGridTree.SubGridTreeDimension)
            {
                // TODO Readd when logging available
                //SIGLogMessage.PublishNoODS(Nil, Format('Skip value of %d/%d chosen for %s', [StepX, StepY, subgrid.Moniker]), slmcDebug);
            }

            SeiveBitmask.Clear();

            // Calculate the world coordinate location of the origin (bottom left corner)
            // of this subgrid
            SubGrid.CalculateWorldOrigin(out SubGridWorldOriginX, out SubGridWorldOriginY);

            // Skip-Iterate through the cells marking those cells that require values
            // calculate for them in the bitmask

            Temp = SubGridWorldOriginY / StepYIncrement;
            CurrentNorth = (Math.Truncate(Temp) * StepYIncrement) - StepYIncrementOverTwo;
            north_row = (int)Math.Floor((CurrentNorth - SubGridWorldOriginY) / _CellSize);

            while (north_row < 0)
            {
                north_row += StepY;
            }

            while (north_row < SubGridTree.SubGridTreeDimension)
            {
                Temp = SubGridWorldOriginX / StepXIncrement;

                CurrentEast = (Math.Truncate(Temp) * StepXIncrement) + StepXIncrementOverTwo;
                east_col = (int)Math.Floor((CurrentEast - SubGridWorldOriginX) / _CellSize);

                while (east_col < 0)
                {
                    east_col += StepX;
                }

                while (east_col < SubGridTree.SubGridTreeDimension)
                {
                    SeiveBitmask.SetBit(east_col, north_row);
                    east_col += StepX;
                }

                north_row += StepY;
            }

            return true;
        }

        private void InitialiseRotationAndBounds(double Rotation, // Radians, north azimuth survey angle
                                                 double SubgridMinX, double SubgridMinY, 
                                                 double SubgridMaxX, double SubgridMaxY)
        {
            double _X, _Y;
            double CosOfRotation, SinOfRotation;

            Fence RotatedSubgridBoundary = null;

            if (Rotation != 0)
            {
                RotatedSubgridBoundary = new Fence();

                // Create the rotated boundary by 'unrotating' the subgrid world extents into a context
                // where the grid is itself not rotated
                GeometryHelper.RotatePointAbout(Rotation, SubgridMinX, SubgridMinY, out _X, out _Y, AreaControlSet.UserOriginX, AreaControlSet.UserOriginY);
                RotatedSubgridBoundary.Points.Add(new FencePoint(_X, _Y));
                GeometryHelper.RotatePointAbout(Rotation, SubgridMinX, SubgridMaxY, out _X, out _Y, AreaControlSet.UserOriginX, AreaControlSet.UserOriginY);
                RotatedSubgridBoundary.Points.Add(new FencePoint(_X, _Y));
                GeometryHelper.RotatePointAbout(Rotation, SubgridMaxX, SubgridMaxY, out _X, out _Y, AreaControlSet.UserOriginX, AreaControlSet.UserOriginY);
                RotatedSubgridBoundary.Points.Add(new FencePoint(_X, _Y));
                GeometryHelper.RotatePointAbout(Rotation, SubgridMaxX, SubgridMinY, out _X, out _Y, AreaControlSet.UserOriginX, AreaControlSet.UserOriginY);
                RotatedSubgridBoundary.Points.Add(new FencePoint(_X, _Y));

                FirstScanPointEast = Math.Truncate(RotatedSubgridBoundary.MinX / StepX) * StepX + IntraGridOffsetX;
                FirstScanPointNorth = Math.Truncate(RotatedSubgridBoundary.MinY / StepY) * StepY + IntraGridOffsetY;

                NumRowsToScan = (int)Math.Ceiling((RotatedSubgridBoundary.MaxY - FirstScanPointNorth) / StepY) + 1;
                NumColsToScan = (int)Math.Ceiling((RotatedSubgridBoundary.MaxX - FirstScanPointEast) / StepX) + 1;

                // Rotate the first scan point back to the context of the grid projection north oriented
                // subgrid world extents
                GeometryHelper.RotatePointAbout(-Rotation, FirstScanPointEast, FirstScanPointNorth, out FirstScanPointEast, out FirstScanPointNorth, AreaControlSet.UserOriginX, AreaControlSet.UserOriginY);

                // Perform a 'unit' rotation of the StepX and StepY quantities about the
                // origin to define step quantities that orient the vector of probe position movement
                // to the rotated probe grid
                SinOfRotation = Math.Sin(Rotation);
                CosOfRotation = Math.Cos(Rotation);

                StepNorthY = CosOfRotation * StepY;
                StepNorthX = SinOfRotation * StepX;
                StepEastX = CosOfRotation * StepX;
                StepEastY = -SinOfRotation * StepY;
            }
            else
            {
                FirstScanPointEast = Math.Truncate(SubgridMinX / StepX) * StepX + IntraGridOffsetX;
                FirstScanPointNorth = Math.Truncate(SubgridMinY / StepY) * StepY + IntraGridOffsetY;

                NumRowsToScan = (int)Math.Ceiling((SubgridMaxY - FirstScanPointNorth) / StepY) + 1;
                NumColsToScan = (int)Math.Ceiling((SubgridMaxX - FirstScanPointEast) / StepX) + 1;

                StepNorthX = 0;
                StepNorthY = StepY;
                StepEastX = StepX;
                StepEastY = 0;
            }
        }

        private void PerformScan(double SubgridMinX, double SubgridMinY, double SubgridMaxX, double SubgridMaxY)
        {
            int north_row, east_col;
            double CurrentNorth, CurrentEast;

            // Skip-Iterate through the cells marking those cells that require values
            // calculate for them in the bitmask. Also record the actual probe locations
            // that determined the cells to be processed.

            for (int I = 0; I < NumRowsToScan; I++)
            {
                CurrentNorth = FirstScanPointNorth + (I * StepNorthY);
                CurrentEast = FirstScanPointEast + (I * StepNorthX);

                for (int J = 0; J < NumColsToScan; J++)
                {
                    east_col = (int)Math.Floor((CurrentEast - SubgridMinX) / _CellSize);
                    north_row = (int)Math.Floor((CurrentNorth - SubgridMinY) / _CellSize);

                    if (Range.InRange(east_col, 0, SubGridTree.SubGridTreeDimensionMinus1) &&
                        Range.InRange(north_row, 0, SubGridTree.SubGridTreeDimensionMinus1))
                    {
                        SeiveBitmask.SetBit(east_col, north_row);
                        AssignmentContext.ProbePositions[east_col, north_row].SetOffsets(CurrentEast - SubgridMinX, CurrentNorth - SubgridMinY); // = new ProbePoint(CurrentEast - SubgridMinX, CurrentNorth - SubgridMinY);
                    }

                    CurrentEast = CurrentEast + StepEastX;
                    CurrentNorth = CurrentNorth + StepEastY;
                }
            }
        }

        private bool ComputeSeiveBitmaskFloat(ISubGrid SubGrid)
        {
            double SubGridWorldOriginX, SubGridWorldOriginY;
            double SubGridWorldLimitX, SubGridWorldLimitY;

            if (AreaControlSet.PixelXWorldSize == 0 || AreaControlSet.PixelYWorldSize == 0)
            {
                return false;
            }

            if (AreaControlSet.PixelXWorldSize < _CellSize && AreaControlSet.PixelYWorldSize < _CellSize)
            {
                return false;
            }

            StepX = AreaControlSet.PixelXWorldSize;
            StepY = AreaControlSet.PixelYWorldSize;

            // Progress through the cells in the grid, starting from the southern most
            // row in the grid and progressing from the western end to the eastern end
            // (ie: bottom to top, left to right), taking into account grid offsets and
            // rotations specified in AreaControlSet

            SeiveBitmask.Clear();

            // Calculate the world coordinate location of the origin (bottom left corner)
            // and limits (top right corner) of this subgrid
            SubGrid.CalculateWorldOrigin(out SubGridWorldOriginX, out SubGridWorldOriginY);
            SubGridWorldLimitX = SubGridWorldOriginX + (SubGridTree.SubGridTreeDimension * _CellSize);
            SubGridWorldLimitY = SubGridWorldOriginY + (SubGridTree.SubGridTreeDimension * _CellSize);

            // Take into account the effect of having to have a grid probe position at
            // the 'first point' defined in AreaControlSet
            // Calculate the intra-interval offset that needs to be applied to align the
            // skip-stepping to that modifed gridding
            IntraGridOffsetX = AreaControlSet.UserOriginX - (Math.Floor(AreaControlSet.UserOriginX / StepX) * StepX);
            IntraGridOffsetY = AreaControlSet.UserOriginY - (Math.Floor(AreaControlSet.UserOriginY / StepY) * StepY);

            // Calculate the parameter to control skipping across a rotated grid with respect to
            // a grid projection north oriented subgrid
            InitialiseRotationAndBounds(AreaControlSet.Rotation, SubGridWorldOriginX, SubGridWorldOriginY, SubGridWorldLimitX, SubGridWorldLimitY);

            // Perform the walk across all probed locations determining the cells we want to
            // obtain values for and the probe locations.
            PerformScan(SubGridWorldOriginX, SubGridWorldOriginY, SubGridWorldLimitX, SubGridWorldLimitY);

            return true;
        }

        public ServerRequestResult RetrieveSubGrid(CombinedFilter filter,
                                                   int maxNumberOfPassesToReturn,
                                                   bool hasOverrideSpatialCellRestriction,
                                                   BoundingIntegerExtent2D overrideSpatialCellRestriction,
                                                   SiteModel siteModel,
                                                   // gridDataCache : TICDataStoreCache;
                                                   byte Level,
                                                   uint CellX,
                                                   uint CellY,
                                                   // liftBuildSettings          : TICLiftBuildSettings;
                                                   IClientLeafSubGrid clientGrid,
                                                   bool prepareGridForCacheStorageIfNoSeiving,
                                                   SubGridTreeBitmapSubGridBits cellOverrideMask,
                                                   // subgridLockToken          : Integer;
                                                   ref AreaControlSet areaControlSet,
                                                   ClientHeightLeafSubGrid designElevations)
        {
//            Log.Info("Entering RetrieveSubGrid");

            ServerRequestResult Result = ServerRequestResult.UnknownError;

            //  SIGLogMessage.PublishNoODS(Nil, Format('In RetrieveSubGrid: Active pass filters = %s, Active cell filters = %s', [PassFilter.ActiveFiltersText, CellFilter.ActiveFiltersText]), slmcDebug);

            // Set up class local state for other methods to access
            this._CellSize = siteModel.Grid.CellSize;
            this.Filter = filter == null ? new CombinedFilter(siteModel) : filter;
            this.AreaControlSet = areaControlSet;
            this.ClientGrid = clientGrid;
            this.ClientGridAsLeaf = clientGrid as ClientLeafSubGrid;
            this._GridDataType = clientGrid.GridDataType;

            this.SiteModel = siteModel;
            this.PrepareGridForCacheStorageIfNoSeiving = prepareGridForCacheStorageIfNoSeiving;

            this.CanUseGlobalLatestCells = Filter.AttributeFilter.LastRecordedCellPassSatisfiesFilter;

            try
            {
                try
                {
                    /* TODO Readd when Pass Types are implemented
                    // Ensure passtype filter is set correctly
                    if (Filter.AttributeFilter.HasPassTypeFilter)
                    {
                        if (TICPassType.ptFront in PassFilter.PassTypeSet)
                        {
                            if (!(TICPassType.ptRear in PassFilter.PassTypeSet)) // VL interface doesnt mention Rear
                            {
                                PassFilter.PassTypeSet = PassFilter.PassTypeSet + [TICPassType.ptRear]; // these two types go together as half passes
                            }
                        }
                    }
                    */

                    // ... unless we if we can use the last pass grid to satisfy the query
                    if (CanUseGlobalLatestCells &&
                        !Filter.AttributeFilter.HasElevationRangeFilter &&
                        !ClientGrid.WantsLiftProcessingResults() &&
                        !Filter.AttributeFilter.HasMinElevMappingFilter &&
                        !(Filter.AttributeFilter.HasElevationTypeFilter &&
                          (Filter.AttributeFilter.ElevationType == ElevationType.Highest || Filter.AttributeFilter.ElevationType == ElevationType.Lowest)) &&
                        !(_GridDataType == GridDataType.PassCount || _GridDataType == GridDataType.Temperature ||
                          _GridDataType == GridDataType.CellProfile || _GridDataType == GridDataType.CellPasses ||
                          _GridDataType == GridDataType.MachineSpeed))
                    {
                        UseLastPassGrid = true;
                    }

                    // First get the subgrid we are interested in
                    // SIGLogMessage.PublishNoODS(Nil, Format('Begin LocateSubGridContaining at %dx%d', [CellX, CellY]), slmcDebug); {SKIP}

                    // _SubGrid = SiteModel.Grid.LocateSubGridContaining(CellX, CellY, Level);
                    _SubGrid = SubGridUtilities.LocateSubGridContaining(/*storageProxy, */SiteModel.Grid, CellX, CellY, Level, 0, false, false); 

                    /* TODO ???:
                    if (_SubGrid != null && _SubGrid.LockToken != ASubGridLockToken)
                    {
                        SIGLogMessage.PublishNoODS(Nil, Format('Returned, locked, subgrid has incorrect lock token (%d vs expected %d)', [_SubGrid.LockToken, ASubGridLockToken]), slmcAssert); {SKIP}
                        return Result;
                    }
                    */

                    //      SIGLogMessage.PublishNoODS(Nil, Format('End LocateSubGridContaining at %dx%d', [CellX, CellY]), slmcDebug); {SKIP}

                    if (_SubGrid == null)
                    {
                        // This should never really happen, but we'll be polite about it
                        // TODO Readd when logging available
                        // SIGLogMessage.PublishNoODS(Nil, Format('Subgrid address (CellX=%d, CellY=%d) passed to LocateSubGridContaining() from RetrieveSubgriod() did not match an existing subgrid in the data model.' + 'Returning icsrrSubGridNotFound as response with a nil subgrid reference.', [CellX, CellY]), slmcWarning);
                        return ServerRequestResult.SubGridNotFound;
                    }

                    // Now process the contents of that subgrid into the subgrid to be returned
                    // to the client.

                    if (!_SubGrid.IsLeafSubGrid()) // It's a leaf node
                    {
                        // TODO Readd when logging available
                        //SIGLogMessage.PublishNoODS(Nil, 'Requests of node subgrids in the IC server subgrid are not yet supported', slmcMessage); {SKIP}
                        return Result;
                    }

                    if (!(_SubGrid is ServerSubGridTreeLeaf))
                    {
                        // TODO Readd when logging available
                        // SIGLogMessage.PublishNoODS(Nil, Format('_SubGrid %s is not a server grid leaf node', [_Subgrid.Moniker]), slmcAssert);
                        return Result;
                    }

                    // SIGLogMessage.PublishNoODS(Nil, Format('Getting subgrid leaf at %dx%d', [CellX, CellY]), slmcDebug);

                    _SubGridAsLeaf = (ServerSubGridTreeLeaf)_SubGrid;
                    this._GlobalLatestCells = _SubGridAsLeaf.Directory.GlobalLatestCells;

                    if (PruneSubGridRetrievalHere())
                    {
                        return ServerRequestResult.NoError;
                    }

                    // Determine the bitmask detailing which cells match the cell selection filter
                    if (!SubGridFilterMasks.ConstructSubgridCellFilterMask(_SubGridAsLeaf, SiteModel, Filter,
                                                                           cellOverrideMask, 
                                                                           hasOverrideSpatialCellRestriction, 
                                                                           overrideSpatialCellRestriction,
                                                                           ref ClientGridAsLeaf.ProdDataMap, 
                                                                           ref ClientGridAsLeaf.FilterMap))
                    {
                        return ServerRequestResult.FailedToComputeDesignFilterPatch;
                    }

                    // SIGLogMessage.PublishNoODS(Nil, Format('Setup for stripe iteration at %dx%d', [CellX, CellY]), slmcDebug);

                    try
                    {
                        if (!UseLastPassGrid)
                        {
                            SetupForCellPassStackExamination();
                        }

                        // Some display types require lift processing to be able to select the
                        // appropriate cell pass containing the filtered value required.
                        if (ClientGrid.WantsLiftProcessingResults())
                        {
                            /* TODO Readd when cell profiling is supported
                            Profiler  = TICServerProfiler.Create(SiteModel, SiteModel.Grid, nil, nil, ClientGrid.GridDataType, PopulationControl);

                            CellPassFastEventLookerUpper  = TICCellPassFastEventLookerUpper.Create;
                            CellPassFastEventLookerUpper.SetSiteModel(SiteModel);

                            Profiler.CellPassFastEventLookerUpper  = CellPassFastEventLookerUpper;
                            CellProfile  = TICProfileCell.Create;
                            SegmentIterator.IterationDirection  = ssidForwards;

                            CellPassIterator.MaxNumberOfPassesToReturn  = AMaxNumberOfPassesToReturn; //VLPDSvcLocations.VLPDPSNode_MaxCellPassIterationDepth_PassCountDetailAndSummary;
                            */
                        }

                        // Create and configure teh assignment context which is used to contain
                        // a filtered pass and its attendant machine events and target values
                        // prior to assignment to the client subgrid.
                        AssignmentContext = new FilteredValueAssignmentContext();

                        // TODO Add when cell left build setting sna cell profiles supported
                        // AssignmentContext.CellProfile = CellProfile;
                        // AssignmentContext.LiftBuildSettings = LiftBuildSettings;

                        // Determine if a seive filter is required for the subgrid where the seive matches
                        // the X and Y pixel world size (used for WMS tile computation)
                        SeiveFilterInUse = AreaControlSet.UseIntegerAlgorithm ? ComputeSeiveBitmask(_SubGrid) : ComputeSeiveBitmaskFloat(_SubGrid);

                        if (!SeiveFilterInUse)
                        {
                            // Reset pixel size parameters to indicate no skip stepping is being performed
                            AreaControlSet.PixelXWorldSize = 0;
                            AreaControlSet.PixelYWorldSize = 0;
                        }

                        /* TODO 
                        if (VLPDSvcLocations.Debug_ExtremeLogSwitchC)
                        {
                          // TODO Readd when logging available
                          // SIGLogMessage.PublishNoODS(Nil, Format('Performing stripe iteration at %dx%d', [CellX, CellY]), slmcDebug);
                        }
                        */

/*
                        // Compute a mask of the cells that should be visited for this subgrid retrieval operation
                        if (SeiveFilterInUse)
                        {
                            // Remove cells that do not match the filter mask and should not be processed, and those which are not identified
                            // by any active seive
                            CellIterationBitmask.SetAndOf(SeiveBitmask, ClientGridAsLeaf.ProdDataMap);
                        }
                        else
                        {
                            // No active seive
                            if (!PrepareGridForCacheStorageIfNoSeiving)
                            {
                                // Remove cells that do not match the fiilter mask and should not be processed
                                CellIterationBitmask.Assign(ClientGridAsLeaf.ProdDataMap);
                            }
                        }

                        // Iterate over the cells in the cell iteration mask and retrieve them
                        CellIterationBitmask.ForEachSetBit((x, y) => RetrieveSubGridCell(// TODO PopulationControl,
                                                  (byte)x, (byte)y
                                                  // TODO LiftBuildSettings
                                                  ));
*/
                        // Iterate over the stripes in the subgrid processing each on in turn.
                        /**/
                        for (byte I = 0; I < SubGridTree.SubGridTreeDimension; I++)
                        {
                            RetrieveSubGridStripe(// TODO PopulationControl,
                                                  I, 
                                                  CellX, CellY
                                                  // TODO LiftBuildSettings
                                                  );
                        }
                        /**/

                        /* TODO
                        if VLPDSvcLocations.Debug_ExtremeLogSwitchC then
                          SIGLogMessage.PublishNoODS(Nil, Format('Stripe iteration complete at %dx%d', [CellX, CellY]), slmcDebug);
                          */
                    }
                    finally
                    {
                        /* TODO - readd when looker upper etc implemented
                        if (CellPassFastEventLookerUpper != null)
                        {
                            if (VLPDSvcLocations.Debug_LogCellPassLookerUpperFullLookups)
                            {
                                InterlockedExchangeAdd64(Debug_TotalCellPassLookerUpperFullLookups, CellPassFastEventLookerUpper.NumFullEventLookups);
                                SIGLogMessage.PublishNoODS(Nil, Format('Cell pass looker-upper invoked %d full event lookups, total = %d', [CellPassFastEventLookerUpper.NumFullEventLookups, Debug_TotalCellPassLookerUpperFullLookups]), slmcDebug);
                            }
                        }
                        */
                    }

                    //      SIGLogMessage.PublishNoODS(Nil, Format('Stripe iteration context shutdown at %dx%d', [CellX, CellY]), slmcDebug);

                    Result = ServerRequestResult.NoError;
                }
                finally
                {
                    /* TODO...
                    if (_SubGrid != null)
                    {
                        _SubGrid.ReleaseLock(ASubGridLockToken);
                    }
                    */

                    //      SIGLogMessage.PublishNoODS(Nil, 'Completed RetrieveSubGrid operation', slmcDebug); {SKIP}
                }
            }
            catch
            {
                // TODO readd when logging available
                // SIGLogMessage.PublishNoODS(Nil, Format('Exception ''%s'' occured in RetrieveSubGrid', [E.Message]), E, slmcException);
                throw;
            }

//            Log.Info("Exiting RetrieveSubGrid");

            return Result;
        }
    }
}
