﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor;
using VSS.VisionLink.Raptor.Cells;
using VSS.VisionLink.Raptor.Common;
using VSS.VisionLink.Raptor.Interfaces;
using VSS.VisionLink.Raptor.SiteModels;
using VSS.VisionLink.Raptor.Storage;
using VSS.VisionLink.Raptor.SubGridTrees;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;
using VSS.VisionLink.Raptor.SubGridTrees.Server.Iterators;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.SubGridTrees.Server
{
    /// <summary>
    /// The core class containing a description of all cell passes recorded within the spatial confines
    /// of a subgrid on the ground.
    /// </summary>
    public class ServerSubGridTreeLeaf : ServerLeafSubGridBase, IServerLeafSubGrid, ILeafSubGrid, ISubGrid
    {
        /// <summary>
        /// Does this subgrid contain directory information for all the subgrids that exist within it?
        /// </summary>
        bool haveSubgridDirectoryDetails = false;
        public bool HaveSubgridDirectoryDetails { get { return haveSubgridDirectoryDetails; } }

        /// <summary>
        /// The date time of the first observed cell pass within this subgrid
        /// </summary>
        DateTime leafStartTime;

        /// <summary>
        /// The date time of the last observed cell pass within this subgrid
        /// </summary>
        DateTime leafEndTime;

        /// <summary>
        /// The date time of the last observed cell pass within this subgrid
        /// </summary>
        public DateTime LeafEndTime { get { return leafEndTime; } }

        /// <summary>
        /// The date time of the first observed cell pass within this subgrid
        /// </summary>
        public DateTime LeafStartTime { get { return leafStartTime; } }

        /// <summary>
        /// A directory containing metadata regarding the segments present within this subgrid
        /// </summary>
        public SubGridDirectory Directory { get; set; } = new SubGridDirectory();

        /// <summary>
        /// The primary wrapper containing all segments that have been loaded
        /// </summary>
        public SubGridCellPassesDataWrapper Cells { get; set; } = null; // Use AllocateLeafFullPassStacks() to create new SubGridCellPassesDataWrapper();

        /// <summary>
        /// 
        /// </summary>
        private void InitialiseStartEndTime()
        {
            leafStartTime = DateTime.MaxValue;
            leafEndTime = DateTime.MinValue;
        }

        public override void Clear()
        {
            InitialiseStartEndTime();

            if (Cells != null)
            {
                Cells.Clear();
            }

            if (Directory != null)
            {
                Directory.Clear();
            }
        }

        private void CellPassAdded(SubGridCellPassesDataSegment segment, CellPass pass)
        {
            segment.PassesData.SegmentPassCount++;

            UpdateStartEndTimeRange(pass.Time);

            Dirty = true;
        }

        /// <summary>
        /// Takes a date/time and expands the subgrid leaf time range to include it if necessary
        /// </summary>
        /// <param name="time"></param>
        public void UpdateStartEndTimeRange(DateTime time)
        {
            if (time < leafStartTime)
            {
                leafStartTime = time;
            }

            if (time > leafEndTime)
            {
                leafEndTime = time;
            }
        }

        public ServerSubGridTreeLeaf(ISubGridTree owner,
                                     ISubGrid parent,
                                     byte level) : base(owner, parent, level)
        {
            Clear();
        }

        public void AddPass(uint cellX, uint cellY, CellPass Pass)
        {
            int PassIndex;
            SubGridCellPassesDataSegment Segment;

            Segment = Cells.SelectSegment(Pass.Time);

            if (Segment == null)
            {
                Debug.Assert(false, "Cells.SelectSegment failed to return a segment");
                return;
            }

            if (!Segment.HasAllPasses)
            {
                Segment.AllocateFullPassStacks();
            }

            // Add the processed pass to the cell

            if (Segment.PassesData.PassData[cellX, cellY].PassCount == 0)
            {
                Segment.PassesData.PassData[cellX, cellY].AddPass(Pass, 0);
                CellPassAdded(Segment, Pass);
            }
            else
            {
                if (Segment.PassesData.PassData[cellX, cellY].LocateTime(Pass.Time, out PassIndex))
                {
                    // Replace the existing cell pass with the new one. The assumption
                    // here is that more than one machine will never cross a cell center position
                    // within the same second (the resolution of the cell pass time stamps)
                    Segment.PassesData.PassData[cellX, cellY].ReplacePass(PassIndex, Pass);

                    Dirty = true;
                }
                else
                {
                    Segment.PassesData.PassData[cellX, cellY].AddPass(Pass, PassIndex);
                    CellPassAdded(Segment, Pass);
                }
            }
        }

        /// <summary>
        /// Creates the default segment metadata within the segment directory. This is only called to create the first 
        /// segment metadta spanning the entire time range.
        /// </summary>
        public void CreateDefaultSegment()
        {
            Directory.CreateDefaultSegment();
        }

        public void AllocateFullPassStacks(SubGridCellPassesDataSegmentInfo SegmentInfo)
        {
            if (SegmentInfo.Segment == null)
            {
                AllocateSegment(SegmentInfo);
            }

            if (SegmentInfo.Segment != null)
            {
                SegmentInfo.Segment.AllocateFullPassStacks();
                //                FCachedMemorySizeOutOfDate:= True;
            }
        }

        public void AllocateLatestPassGrid(SubGridCellPassesDataSegmentInfo SegmentInfo)
        {
            if (SegmentInfo.Segment == null)
            {
                AllocateSegment(SegmentInfo);
            }

            if (SegmentInfo.Segment != null)
            {
                SegmentInfo.Segment.AllocateLatestPassGrid();
                //                FCachedMemorySizeOutOfDate:= True;
            }
        }

        public bool HasAllCellPasses() => Cells != null;

        public void AllocateLeafFullPassStacks()
        {
            if (Cells == null)
            {
                //                Include(FLeafStorageClasses, icsscAllPasses);
                Cells = new SubGridCellPassesDataWrapper();
                Cells.Owner = this;

                //       FCachedMemorySizeOutOfDate:= True;
            }
        }

        public void DeAllocateLeafFullPassStacks() => Cells = null;

        public bool HasLatestData() => Directory.GlobalLatestCells != null;

        public void AllocateLeafLatestPassGrid()
        {
            if (Directory.GlobalLatestCells == null)
            {
                // Include(FLeafStorageClasses, icsscLatestData);
                Directory.AllocateGlobalLatestCells();

                // FCachedMemorySizeOutOfDate:= True;
            }
        }

        public void DeAllocateLeafLatestPassGrid()
        {
            if (Directory != null)
            {
                Directory.GlobalLatestCells = null;
            }
        }

        /// <summary>
        /// Certain types of grid attribute data requests may need us to select
        /// a pass that is not the latest pass in the pass list. Such an instance is
        /// when request CCV value where null CCV values are passed over in favour of
        /// non-null CCV values in passes that are older in the pass list for the cell.
        /// Important: Also see the PassIsAcceptable() function in
        /// TICDataPassFilter.FilterSinglePass() to ensure that the logic
        /// here is consistent (or at least not contradictory) with the logic here.
        /// The checks are duplicated as there may be different logic applied to the
        /// selection of the 'latest' pass from a cell pass state versus selection of
        /// an appropriate filtered pass given other filtering criteria in play.
        /// </summary>
        /// <param name="TypeToCheck"></param>
        /// <param name="ValueFromLatestCellPass"></param>
        private void GetAppropriateLatestValueFor(Cell CellPasses,
                                                  ref CellPass LatestData,
                                                  int LastPassIndex,
                                                  GridDataType TypeToCheck,
                                                  out bool ValueFromLatestCellPass)
        {

            ValueFromLatestCellPass = false;

            for (int I = LastPassIndex; I >= 0; I--)
            {
                switch (TypeToCheck)
                {
                    case GridDataType.CCV:
                        if (CellPasses.Passes[I].CCV != CellPass.NullCCV)
                        {
                            LatestData.CCV = CellPasses.Passes[I].CCV;
                            ValueFromLatestCellPass = I == LastPassIndex;

                            return;
                        }
                        break;

                    case GridDataType.RMV:
                        if (CellPasses.Passes[I].RMV != CellPass.NullRMV)
                        {
                            LatestData.RMV = CellPasses.Passes[I].RMV;
                            ValueFromLatestCellPass = I == LastPassIndex;

                            return;
                        }
                        break;

                    case GridDataType.Frequency:
                        if (CellPasses.Passes[I].Frequency != CellPass.NullFrequency)
                        {
                            LatestData.Frequency = CellPasses.Passes[I].Frequency;
                            ValueFromLatestCellPass = I == LastPassIndex;

                            return;
                        }
                        break;

                    case GridDataType.Amplitude:
                        if (CellPasses.Passes[I].Amplitude != CellPass.NullAmplitude)
                        {
                            LatestData.Amplitude = CellPasses.Passes[I].Amplitude;
                            ValueFromLatestCellPass = I == LastPassIndex;

                            return;
                        }
                        break;

                    case GridDataType.GPSMode:
                        {
                            // Also grab flags for halfpass and rearaxle
                            LatestData.halfPass = CellPasses.Passes[I].halfPass;
                            LatestData.passType = CellPasses.Passes[I].passType;

                            if (CellPasses.Passes[I].gpsMode != CellPass.NullGPSMode)
                            {
                                LatestData.gpsMode = CellPasses.Passes[I].gpsMode;
                                ValueFromLatestCellPass = I == LastPassIndex;
                                return;
                            }
                        }
                        break;

                    case GridDataType.Temperature:
                        if (CellPasses.Passes[I].MaterialTemperature != CellPass.NullMaterialTemp)
                        {
                            LatestData.MaterialTemperature = CellPasses.Passes[I].MaterialTemperature;
                            ValueFromLatestCellPass = I == LastPassIndex;

                            return;
                        }
                        break;

                    case GridDataType.MDP:
                        if (CellPasses.Passes[I].MDP != CellPass.NullMDP)
                        {
                            LatestData.MDP = CellPasses.Passes[I].MDP;
                            ValueFromLatestCellPass = I == LastPassIndex;

                            return;
                        }
                        break;

                    case GridDataType.CCA:
                        if (CellPasses.Passes[I].CCA != CellPass.NullCCA)
                        {
                            LatestData.CCA = CellPasses.Passes[I].CCA;
                            ValueFromLatestCellPass = I == LastPassIndex;
                            return;
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        public void CalculateLatestPassDataForPassStack(Cell CellPasses,
                                                        ref CellPass LatestData,
                                                        out bool CCVFromLatestCellPass,
                                                        out bool RMVFromLatestCellPass,
                                                        out bool FrequencyFromLatestCellPass,
                                                        out bool AmplitudeFromLatestCellPass,
                                                        out bool TemperatureFromLatestCellPass,
                                                        out bool GPSModeFromLatestCellPass,
                                                        out bool MDPFromLatestCellPass,
                                                        out bool CCAFromLatestCellPass)
        {
            int LastPassIndex;

            Debug.Assert(CellPasses.PassCount > 0, "CalculateLatestPassDataForPassStack called with a cell pass stack containing no passes");

            LastPassIndex = CellPasses.PassCount - 1;

            LatestData.Time = CellPasses.Passes[LastPassIndex].Time;
            LatestData.MachineID = CellPasses.Passes[LastPassIndex].MachineID;

            if (CellPasses.Passes[LastPassIndex].Height != Consts.NullHeight)
            {
                LatestData.Height = CellPasses.Passes[LastPassIndex].Height;
            }

            if (CellPasses.Passes[LastPassIndex].RadioLatency != CellPass.NullRadioLatency)
            {
                LatestData.RadioLatency = CellPasses.Passes[LastPassIndex].RadioLatency;
            }

            LatestData.MachineSpeed = CellPasses.Passes[LastPassIndex].MachineSpeed;

            GetAppropriateLatestValueFor(CellPasses, ref LatestData, LastPassIndex, GridDataType.GPSMode, out GPSModeFromLatestCellPass);
            GetAppropriateLatestValueFor(CellPasses, ref LatestData, LastPassIndex, GridDataType.CCV, out CCVFromLatestCellPass);
            GetAppropriateLatestValueFor(CellPasses, ref LatestData, LastPassIndex, GridDataType.RMV, out RMVFromLatestCellPass);
            GetAppropriateLatestValueFor(CellPasses, ref LatestData, LastPassIndex, GridDataType.Frequency, out FrequencyFromLatestCellPass);
            GetAppropriateLatestValueFor(CellPasses, ref LatestData, LastPassIndex, GridDataType.Amplitude, out AmplitudeFromLatestCellPass);
            GetAppropriateLatestValueFor(CellPasses, ref LatestData, LastPassIndex, GridDataType.Temperature, out TemperatureFromLatestCellPass);
            GetAppropriateLatestValueFor(CellPasses, ref LatestData, LastPassIndex, GridDataType.MDP, out MDPFromLatestCellPass);
            GetAppropriateLatestValueFor(CellPasses, ref LatestData, LastPassIndex, GridDataType.CCA, out CCAFromLatestCellPass);
        }

        public void AllocateSegment(SubGridCellPassesDataSegmentInfo segmentInfo)
        {
            if (segmentInfo.Segment != null)
            {
                // TODO add when loggin available
                // SIGLogMessage.PublishNoODS(Self, 'Cannot allocate a segment that is already allocated', slmcAssert);
                return;
            }

            Cells.PassesData.AddNewSegment(this, segmentInfo);

            //        CachedMemorySizeOutOfDate:= True;
        }

        public override bool CellHasValue(byte CellX, byte CellY)
        {
            return Directory.GlobalLatestCells.PassDataExistanceMap.BitSet(CellX, CellY);
        }

        public void CalculateLatestPassGridForSegment(SubGridCellPassesDataSegment Segment,
                                                      SubGridCellPassesDataSegment TemporallyPrecedingSegment)
        {
            bool UpdatedCell = false;
            bool CCVFromLatestCellPass;
            bool MDPFromLatestCellPass;
            bool RMVFromLatestCellPass;
            bool FrequencyFromLatestCellPass;
            bool AmplitudeFromLatestCellPass;
            bool GPSModeFromLatestCellPass;
            bool TemperatureFromLatestCellPass;
            bool CCAFromLatestCellPass;

            if (Segment.PassesData == null)
            {
                // TODO add when logging available
                // SIGLogMessage.PublishNoODS(Self, Format('TICServerSubGridTreeLeaf.CalculateLatestPassGridForSegment passed a segment in %s with no cell passes allocated', [Moniker]), slmcAssert);
                return;
            }

            Segment.AllocateLatestPassGrid();
            Segment.LatestPasses.Clear();
            Segment.Dirty = true;

            if (Segment.LatestPasses == null)
            {
                // TODO add when logging available
                // SIGLogMessage.PublishNoODS(Self, Format('Cell latest pass store for %s not instantiated', [Moniker]), slmcAssert);
                return;
            }

            if (Cells == null)
            {
                // TODO add when logging available
                //SIGLogMessage.PublishNoODS(Self, Format('Cell passes store for %s not instantiated', [Moniker]), slmcAssert);
                return;
            }

            // Seed the latest value tags for this segment with the latest data from the previous segment
            if (TemporallyPrecedingSegment != null)
            {
                // TODO: Include with other last pass attributes
                // Segment.LatestPasses.AssignValuesFromLastPassFlags(TemporallyPrecedingSegment.LatestPasses);
            }

            // Iterate over the values in the child leaf subgrid looking for
            // the first cell with passes in it
            for (byte I = 0; I < SubGridTree.SubGridTreeDimension; I++)
            {
                for (byte J = 0; J < SubGridTree.SubGridTreeDimension; J++)
                {
                    UpdatedCell = false;

                    if (TemporallyPrecedingSegment != null &&
                       TemporallyPrecedingSegment.LatestPasses.PassDataExistanceMap.BitSet(I, J))
                    {
                        // Seed the latest data for this segment with the latest data from the previous segment
                        // TODO: Include with other last pass attributes
                        // Segment.LatestPasses.PassData[I, J] = TemporallyPrecedingSegment.LatestPasses.PassData[I, J];

                        UpdatedCell = true;
                    }

                    // Update the latest data from any previous segment with the information contained in this segment
                    if (Segment.PassesData.PassData[I, J].PassCount > 0)
                    {
                        CalculateLatestPassDataForPassStack(Segment.PassesData.PassData[I, J],
                                                            ref Segment.LatestPasses.PassData[I, J],
                                                            out CCVFromLatestCellPass,
                                                            out RMVFromLatestCellPass,
                                                            out FrequencyFromLatestCellPass,
                                                            out AmplitudeFromLatestCellPass,
                                                            out TemperatureFromLatestCellPass,
                                                            out GPSModeFromLatestCellPass,
                                                            out MDPFromLatestCellPass,
                                                            out CCAFromLatestCellPass);

                        Segment.LatestPasses.CCVValuesAreFromLastPass.SetBitValue(I, J, CCVFromLatestCellPass);
                        Segment.LatestPasses.RMVValuesAreFromLastPass.SetBitValue(I, J, RMVFromLatestCellPass);
                        Segment.LatestPasses.FrequencyValuesAreFromLastPass.SetBitValue(I, J, FrequencyFromLatestCellPass);
                        Segment.LatestPasses.AmplitudeValuesAreFromLastPass.SetBitValue(I, J, AmplitudeFromLatestCellPass);
                        Segment.LatestPasses.GPSModeValuesAreFromLatestCellPass.SetBitValue(I, J, GPSModeFromLatestCellPass);
                        Segment.LatestPasses.TemperatureValuesAreFromLastPass.SetBitValue(I, J, TemperatureFromLatestCellPass);
                        Segment.LatestPasses.MDPValuesAreFromLastPass.SetBitValue(I, J, MDPFromLatestCellPass);
                        Segment.LatestPasses.CCAValuesAreFromLastPass.SetBitValue(I, J, CCAFromLatestCellPass);

                        UpdatedCell = true;
                    }
                    else
                    {
                        if (TemporallyPrecedingSegment != null)
                        {
                            Segment.LatestPasses.CCVValuesAreFromLastPass.SetBitValue(I, J, TemporallyPrecedingSegment.LatestPasses.CCVValuesAreFromLastPass.BitSet(I, J));
                            Segment.LatestPasses.RMVValuesAreFromLastPass.SetBitValue(I, J, TemporallyPrecedingSegment.LatestPasses.RMVValuesAreFromLastPass.BitSet(I, J));
                            Segment.LatestPasses.FrequencyValuesAreFromLastPass.SetBitValue(I, J, TemporallyPrecedingSegment.LatestPasses.FrequencyValuesAreFromLastPass.BitSet(I, J));
                            Segment.LatestPasses.AmplitudeValuesAreFromLastPass.SetBitValue(I, J, TemporallyPrecedingSegment.LatestPasses.AmplitudeValuesAreFromLastPass.BitSet(I, J));
                            Segment.LatestPasses.GPSModeValuesAreFromLatestCellPass.SetBitValue(I, J, TemporallyPrecedingSegment.LatestPasses.GPSModeValuesAreFromLatestCellPass.BitSet(I, J));
                            Segment.LatestPasses.TemperatureValuesAreFromLastPass.SetBitValue(I, J, TemporallyPrecedingSegment.LatestPasses.TemperatureValuesAreFromLastPass.BitSet(I, J));
                            Segment.LatestPasses.MDPValuesAreFromLastPass.SetBitValue(I, J, TemporallyPrecedingSegment.LatestPasses.MDPValuesAreFromLastPass.BitSet(I, J));
                            Segment.LatestPasses.CCAValuesAreFromLastPass.SetBitValue(I, J, TemporallyPrecedingSegment.LatestPasses.CCAValuesAreFromLastPass.BitSet(I, J));
                        }
                    }

                    if (UpdatedCell)
                    {
                        Segment.LatestPasses.PassDataExistanceMap.SetBit(I, J);
                    }
                }
            }
        }

        public void CalculateLatestPassGridForAllSegments()
        {
            AllocateLeafLatestPassGrid();

            // This statement does assume that the last segment has at least it's latest
            // passes in the cache. This is, currently, a safe assumption as the directoy
            // is only written in response to changes in the cell passes in the segments,
            // which in turn will cause the latest cells in the affected segments to be
            // modified which will always cause the latest cells in the latest segment to be
            // modified.
            SubGridCellPassesDataSegment Segment = Directory.SegmentDirectory.Last().Segment;

            SubGridCellLatestPassDataWrapper_NonStatic _GlobalLatestCells = Directory.GlobalLatestCells;
            SubGridCellLatestPassDataWrapper_NonStatic _LatestPasses = Segment.LatestPasses;

            if (_LatestPasses == null)
            {
                Debug.Assert(false, "Cell latest pass store not instantiated");
            }

            _GlobalLatestCells.Clear();
            _GlobalLatestCells.AssignValuesFromLastPassFlags(_LatestPasses);
            _GlobalLatestCells.PassDataExistanceMap.Assign(_LatestPasses.PassDataExistanceMap);

            Segment.LatestPasses.PassDataExistanceMap.ForEachSetBit((x, y) => _GlobalLatestCells.PassData[x, y] = _LatestPasses.PassData[x, y]);
        }

        public void ComputeLatestPassInformation(/*IStorageProxy storageProxy, */bool fullRecompute)
        {
            //            SubGridCellPassesDataSegment Segment;
            SubGridCellPassesDataSegment LastSegment;
            SubGridCellPassesDataSegmentInfo SeedSegmentInfo;
            SubGridSegmentIterator Iterator;
            int NumProcessedSegments;

            /* TODO Review when locking model established
             *  if not Flocked then
                begin
                  SIGLogMessage.PublishNoODS(Self, Format('May not calculate latest pass information if the subgrid (%s) is not locked', [Moniker]), slmcAssert);
                  Exit;
                end;
            */

            if (!Dirty)
            {
                // TODO readd when logging available
                //SIGLogMessage.PublishNoODS(Self, Format('Subgrid (%s) not marked as dirty when computing lastest pass information', [Moniker]), slmcAssert);
                return;
            }

            Iterator = new SubGridSegmentIterator(this, Directory);
            Iterator.IterationDirection = IterationDirection.Forwards;
            Iterator.ReturnDirtyOnly = !fullRecompute;

            NumProcessedSegments = 0;

            // We are in the process of recalculating latest data, so don't ask the iterator to
            // read the latest data information as it will be reconstructed here. The full cell pass
            // stacks are required though...
            Iterator.RetrieveAllPasses = true;

            SeedSegmentInfo = null;
            LastSegment = null;

            // Locate the segment immediately previous to the first dirty segment in the
            // list of segments

            for (int I = 0; I < Directory.SegmentDirectory.Count; I++)
            {
                if (Directory.SegmentDirectory[I].Segment != null && Directory.SegmentDirectory[I].Segment.Dirty)
                {
                    if (I > 0)
                    {
                        SeedSegmentInfo = Directory.SegmentDirectory[I - 1];
                    }
                    break;
                }
            }

            // If we chose the first segment and it was dirty, then clear it
            if (SeedSegmentInfo != null && SeedSegmentInfo.Segment != null)
            {
                LastSegment = SeedSegmentInfo.Segment;
            }

            // If there was such a last segment, then make sure its latest pass information
            // has been read from the store

            if (SeedSegmentInfo != null && SeedSegmentInfo.ExistsInPersistentStore &&
               ((SeedSegmentInfo.Segment == null) || !SeedSegmentInfo.Segment.HasLatestData))
            {
                if (SeedSegmentInfo.Segment == null)
                {
                    AllocateSegment(SeedSegmentInfo);
                }

                if (SeedSegmentInfo.Segment != null)
                {
                    if (((ServerSubGridTree)Owner).LoadLeafSubGridSegment(new SubGridCellAddress(OriginX, OriginY), true, false,
                                                                          this, SeedSegmentInfo.Segment, null))
                    {
                        LastSegment = SeedSegmentInfo.Segment;
                    }
                    else
                    {
                        // TODO add when logging available
                        //   SIGLogMessage.Publish(Self, Format('Failed to load segment from subgrid where segment was marked as present in persistant store for %s', [TSubGridCellAddress.CreateSimple(OriginX, OriginY).AsText]), slmcAssert);
                    }
                }
            }

            while (Iterator.MoveNext())
            {
                NumProcessedSegments++;

                CalculateLatestPassGridForSegment(Iterator.CurrentSubGridSegment, LastSegment);

                LastSegment = Iterator.CurrentSubGridSegment;

                // We have processed a segment. By definition, all segments after the
                // first segment must have the latest values processed, so instruct
                // the iterator to return all segments from now on
                Iterator.ReturnDirtyOnly = false;
            }

            /* Delphi style of iterator iteration - coudl look at morphing this into C# style iterator
            // Iterate through all segments including and after the first dirty segment in the segment list for the subgrid.
            for Segment In Iterator do
                {
                    NumProcessedSegments++;

                    CalculateLatestPassGridForSegment(Segment, LastSegment);

                    LastSegment = Segment;

                    // We have processed a dirty segment. By definition, all segments after the
                    // first dirty segment must have the latest values processed, so instruct
                    // the iterator to return all segments from now on
                    Iterator.ReturnDirtyOnly = false;
                }
                */

            // Note: It is possible that there were no processed segments (NumProcessedSegments = 0) as a result of processing
            // a TAG file that caused no changes to the database (e.g. it had been processed earlier)
            if (NumProcessedSegments > 0)
            {
                // Now compute the final global latest pass data for the directory (though this will be the
                // same as the last segment)
                CalculateLatestPassGridForAllSegments();
            }

            latestCellPassesOutOfDate = false;
        }

        public bool LoadSegmentFromStorage(IStorageProxy storageProxy, string FileName, SubGridCellPassesDataSegment Segment, bool loadLatestData, bool loadAllPasses, SiteModel SiteModelReference)
        {
            FileSystemErrorStatus FSError;
            uint StoreGranuleIndex;
            uint StoreGranuleCount;

            bool Result = false;

            if (loadAllPasses && Segment.Dirty)
            {
                Debug.Assert(false, "Leaf subgrid segment loads of cell pass data may not be performed while the segment is dirty. The information should be taken from the cache instead");
                return false;
            }

            try
            {
                MemoryStream SMS = null; // = new MemoryStream();

                FSError = storageProxy.ReadSpatialStreamFromPersistentStore
                            (Owner.ID, FileName, OriginX, OriginY,
                             FileSystemSpatialStreamType.SubGridSegment, Segment.SegmentInfo.FSGranuleIndex, out SMS,
                             out StoreGranuleIndex, out StoreGranuleCount);

                Result = FSError == FileSystemErrorStatus.OK;

                if (!Result)
                {
                    /*
                    if (FSError == icfseFileDoesNotExist)
              SIGLogMessage.PublishNoODS(Self,
                                     Format('Expected leaf subgrid segment %s, model %d does not exist.',
                                            [FileName, (FOwner as TICServerSubGridTree).DataModelID]),
                                     slmcError)
            else
          SIGLogMessage.PublishNoODS(Self,
                                     Format('Unable to load leaf subgrid segment %s, model %d. Details: %s',
                                            [FileName, (FOwner as TICServerSubGridTree).DataModelID, FSErrorStatusName(FSError)]),
                                     slmcError);
                     */

                    return false;
                }

                // TODO: Hook into the Ignite caching layer to extract the data to be streamed from using Ignite based serialisation
                //                              Result = LoadFromStream(SMS, Segment, loadLatestData, loadAllPasses, SiteModelReference);
                SMS.Position = 0;
                Result = Segment.LoadFromStream(SMS, loadLatestData, loadAllPasses, SiteModelReference);
            }
            catch (Exception E)
            {
                //SIGLogMessage.Publish(Self, Format('Exception %S thrown in TICServerSubGridTreeLeaf.LoadFromFile reading file %S.',
                //    [E.Message, FileName]), slmcError);
                throw;
            }

            return Result;
        }


        public bool SaveDirectoryToStream(Stream stream)
        {
            bool Result = false;
            BinaryWriter writer = new BinaryWriter(stream);

            SubGridStreamHeader Header = new SubGridStreamHeader()
            {
                MajorVersion = SubGridStreamHeader.kSubGridMajorVersion,
                MinorVersion = SubGridStreamHeader.kSubGridMinorVersion_Latest,
                Identifier = SubGridStreamHeader.kICServerSubgridDirectoryFileMoniker,
                Flags = SubGridStreamHeader.kSubGridHeaderFlag_IsSubgridDirectoryFile,
                StartTime = leafStartTime,
                EndTime = leafEndTime,
                LastUpdateTimeUTC = DateTime.Now - Time.GPS.GetLocalGMTOffset()
            };

            // Write the header/version to the stream
            Header.Write(writer);

            Result = Directory.Write(writer);

            haveSubgridDirectoryDetails = Result;

            return Result;
        }

        public bool SaveDirectoryToFile(IStorageProxy storage,
                                        string FileName//,
                                        //uint SubgridX, uint SubgridY
                                                      /* const AInvalidatedSpatialStreams : TInvalidatedSpatialStreamArray*/)
        {
            MemoryStream MStream = new MemoryStream();
            uint StoreGranuleIndex = 0;
            uint StoreGranuleCount = 0;

            bool Result = false;

            if (!SaveDirectoryToStream(MStream))
            {
                return false;
            }

            Result = storage.WriteSpatialStreamToPersistentStore
             (Owner.ID, FileName, OriginX, OriginY, //AInvalidatedSpatialStreams,
              FileSystemSpatialStreamType.SubGridDirectory, out StoreGranuleIndex, out StoreGranuleCount, MStream) == FileSystemErrorStatus.OK;
            if (Result)
            {
                // update new index location and size
                //                Directory.FSGranuleIndex = StoreGranuleIndex;
                //                Directory.FSGranuleCount = StoreGranuleCount;
            }
            else
            {
                // TODO readd when logging available
                //SIGLogMessage.Publish(Self, Format('Call to WriteSpatialStreamToPersistentStore failed. Filename:%s', [FileName]), slmcWarning);
            }

            return Result;
        }

        public bool LoadDirectoryFromStream(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            SubGridStreamHeader Header = new SubGridStreamHeader(reader);

            // LatestPassData: TICSubGridCellLatestPassData;
            long LatestCellPassDataSize;
            long CellPassStacksDataSize;

            bool Result = false;

            haveSubgridDirectoryDetails = false;

            if (!Header.IdentifierMatches(SubGridStreamHeader.kICServerSubgridDirectoryFileMoniker))
            {
                //TODO add when logging vailable
                //SIGLogMessage.Publish(Self,
                //                      'Subgrid directory file header mismatch (expected [Header: %1, found %2]).', { SKIP}
                //              [String(kICServerSubgridDirectoryFileMoniker), String(Identitifer)],
                //              slmcError);
                return false;
            }

            if (!Header.IsSubGridDirectoryFile)
            {
                // TODO add when logging avbailable
                // SIGLogMessage.Publish(Self, 'Subgrid directory file does not identify itself as such in extended header flags', slmcAssert);
                return false;
            }

            //  FLastUpdateTimeUTC := Header.LastUpdateTimeUTC;
            leafStartTime = Header.StartTime;
            leafEndTime = Header.EndTime;

            // Global latest cell passes are always read in from the subgrid directory, even if the 'latest
            // cells' storage class is not contained in the leaf sorage classes. This is currently done due
            // to some operations (namely aggregation of processed cell passes into the production
            // data model) may request subgrids that have not yet been persisted to the data store.
            // Ultimately such requests result in the subgrid being read from disk if the storage classes
            // in the request do not match the storage classes of the leaf subgrid in the cache.
            // reading the latest cells does impose a small performance penalty, however, this
            // data is likely to be required in common use cases so we will load it until a
            // more concrete case for not doing this is made.
            Directory.AllocateGlobalLatestCells();

            if (Header.MajorVersion == 2)
            {
                switch (Header.MinorVersion)
                {
                    case 0:
                        Result = Directory.Read_2p0(reader);//,
                                                            // Directory.GlobalLatestCells.PassData,
                                                            //out LatestCellPassDataSize, out CellPassStacksDataSize);
                        break;
                    default:
                        /* TODO readd whne logging available
                        SIGLogMessage.Publish(Self,
                                              'Subgrid directory file version or header mismatch (expected [Version: %1.%2, found %3.%4] [Header: %5, found %6]).', { SKIP}
                                          [IntToStr(kSubGridMajorVersion), IntToStr(kSubGridMinorVersion_Latest),
                                           IntToStr(MajorVersion), IntToStr(MinorVersion),
                                           String(kICServerSubgridDirectoryFileMoniker), String(Identitifer)],
                                          slmcError);
                            */
                        break;
                }
            }
            else
            {
                // TODO readd when logging available
                /*
                SIGLogMessage.Publish(Self,
                                      'Subgrid directory file version or header mismatch (expected [Version: %1.%2, found %3.%4] [Header: %5, found %6]).', { SKIP}
                            [IntToStr(kSubGridMajorVersion), IntToStr(kSubGridMinorVersion_Latest),
                             IntToStr(MajorVersion), IntToStr(MinorVersion),
                             String(kICServerSubgridDirectoryFileMoniker), String(Identitifer)],
                            slmcError);
                            */
            }

            if (Result)
            {
                haveSubgridDirectoryDetails = true;
            }

            return Result;
        }


        public bool LoadDirectoryFromFile(/*IStorageProxy storage, */ string fileName)
        {
            MemoryStream SMS = null;
            uint StoreGranuleIndex = 0;
            uint StoreGranuleCount = 0;

            IStorageProxy storage = StorageProxy.SpatialInstance(SubGridCellAddress.ToSpatialDivisionDescriptor(OriginX, OriginY, RaptorConfig.numSpatialProcessingDivisions));

            FileSystemErrorStatus FSError = storage.ReadSpatialStreamFromPersistentStore(Owner.ID, fileName, OriginX, OriginY,
                                                                                         FileSystemSpatialStreamType.SubGridDirectory, 0, out SMS, out StoreGranuleIndex, out StoreGranuleCount);

            bool Result = FSError == FileSystemErrorStatus.OK;

            if (!Result)
            {
                /* TODO Readd when logging available
                    if (FSError == FileSystemErrorStatus.FileDoesNotExist)
                        SIGLogMessage.PublishNoODS(this, "Expected leaf subgrid file %1 does not exist.", [fileName], slmcError);
                    else
                       if (FSError != FileSystemErrorStatus.SpatialStreamIndexGranuleLocationNull)
                        SIGLogMessage.PublishNoODS(this, "Unable to load leaf subgrid file %1. Details: %2",  [fileName, FSErrorStatusName(FSError)], slmcError);
                */

                return Result;
            }

            // To ensure integrity of partial cache memory updates we need to ensure that
            // any subgrid passed to this function is either not contained in the cache,
            // or if it is, that it does not have the out-of-date cache flag set.
            // If the subgrid is in the cache and has it's cache size out of date flag set,
            // then reset the flag by explicitly making that cache size adjustment on behalf of
            // the subgrid prior to reading the directory.

            SMS.Position = 0;
            Result = LoadDirectoryFromStream(SMS);

            if (Result)
            {
                Directory.FSGranuleIndex = StoreGranuleIndex;
                Directory.FSGranuleCount = StoreGranuleCount;
            }

            return Result;
        }

        public void Integrate(ServerSubGridTreeLeaf Source,
                             SubGridSegmentIterator Iterator,
                             bool IntegratingIntoIntermediaryGrid)
        {
            SubGridCellPassesDataSegment Segment;
            SubGridCellPassesDataSegment SourceSegment;
            int StartIndex, EndIndex;
            DateTime EndTime;
            int AddedCount;
            int ModifiedCount;
            int PassCountMinusOne;

            if (Source.Cells.PassesData.Count == 0)
            {
                // No cells added to this subgrid during processing
                // TODO readd when logging available
                //SIGLogMessage.PublishNoODS(Self, Format('Empty subgrid %s passed to TICServerSubGridTreeLeaf.Integrate', [Moniker]), slmcAssert);
                return;
            }

            Debug.Assert(Source != null, "Source subgrid not defined in TICServerSubGridTreeLeaf.Integrate");

            if (Source.Cells.PassesData.Count != 1)
            {
                // TODO readd when logging available
                // SIGLogMessage.PublishNoODS(Self, Format('Source integrated subgrids must have only one segment in TICServerSubGridTreeLeaf.Integrate (%s)', [Moniker]), slmcAssert);
                return;
            }

            Iterator.SubGrid = this;
            Iterator.Directory = this.Directory;

            SourceSegment = Source.Cells.PassesData[0];

            UpdateStartEndTimeRange(Source.LeafStartTime);
            UpdateStartEndTimeRange(Source.LeafEndTime);

            for (int I = 0; I < SubGridTree.SubGridTreeDimension; I++)
            {
                for (int J = 0; J < SubGridTree.SubGridTreeDimension; J++)
                {
                    // Perform the physical integration of the new cell passes into the target subgrid
                    StartIndex = 0;
                    int localPassCount = SourceSegment.PassesData.PassData[I, J].PassCount;

                    if (localPassCount == 0)
                    {
                        continue;
                    }

                    // Restrict the iterator to examining only those segments that fall within the
                    // time range covered by the passes in the cell being processes.
                    Iterator.SetTimeRange(SourceSegment.PassesData.PassData[I, J].Passes[0].Time,
                                          SourceSegment.PassesData.PassData[I, J].Passes[localPassCount - 1].Time);

                    // Now iterate over the time bounded segments in the database and integrate
                    // the new cell passes
                    Iterator.InitialiseIterator();
                    while (Iterator.MoveToNextSubGridSegment())
                    {
                        Segment = Iterator.CurrentSubGridSegment;

                        if (StartIndex < localPassCount && SourceSegment.PassesData.PassData[I, J].Passes[StartIndex].Time >= Segment.SegmentInfo.EndTime)
                        {
                            continue;
                        }

                        EndIndex = StartIndex;
                        EndTime = Segment.SegmentInfo.EndTime;
                        PassCountMinusOne = localPassCount - 1;
                        while (EndIndex < PassCountMinusOne && SourceSegment.PassesData.PassData[I, J].Passes[EndIndex + 1].Time < EndTime)
                        {
                            EndIndex++;
                        }

                        Segment.PassesData.PassData[I, J].Integrate(SourceSegment.PassesData.PassData[I, J], StartIndex, EndIndex, out AddedCount, out ModifiedCount);

                        if (AddedCount > 0 || ModifiedCount > 0)
                        {
                            Segment.Dirty = true;
                        }

                        if (AddedCount != 0)
                        {
                            Segment.PassesData.SegmentPassCount += AddedCount;
                        }

                        StartIndex = EndIndex + 1;

                        if (StartIndex >= localPassCount)
                        {
                            break; // We are finished
                        }
                    }
                }
            }

            // CachedMemorySizeOutOfDate = true;
        }

        /// <summary>
        /// Constructs a 'filename' representing this leaf subgrid
        /// </summary>
        /// <param name="Origin"></param>
        /// <returns></returns>
        public static string FileNameFromOriginPosition(SubGridCellAddress Origin) => String.Format("{0:D10}-{1:D10}.sgl", Origin.X, Origin.Y);
    }
}

