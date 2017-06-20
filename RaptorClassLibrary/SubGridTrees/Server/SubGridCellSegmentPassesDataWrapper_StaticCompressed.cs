﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Cells;
using VSS.VisionLink.Raptor.Common;
using VSS.VisionLink.Raptor.Compression;
using VSS.VisionLink.Raptor.SubGridTrees.Server.Interfaces;
using VSS.VisionLink.Raptor.SubGridTrees.Utilities;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.SubGridTrees.Server
{
    /// <summary>
    /// Represents an efficiently compressed version of the cell pass information included within a segment.
    /// The compression achieves approximately 10:1 reduction in memory use while preserving random access with
    /// good performance.
    /// </summary>
    [Serializable]
    public class SubGridCellSegmentPassesDataWrapper_StaticCompressed : SubGridCellSegmentPassesDataWrapperBase, ISubGridCellSegmentPassesDataWrapper
    {
        /// <summary>
        /// The set of field descriptors for the attribute being stored in the bit field array compressed form
        /// </summary>
        private struct EncodedFieldDescriptorsStruct
        {
            public EncodedBitFieldDescriptor MachineIDIndex;
            public EncodedBitFieldDescriptor Time;
            public EncodedBitFieldDescriptor Height;
            public EncodedBitFieldDescriptor CCV;
            public EncodedBitFieldDescriptor RMV;          
            public EncodedBitFieldDescriptor MDP;
            public EncodedBitFieldDescriptor MaterialTemperature;
            public EncodedBitFieldDescriptor MachineSpeed;
            public EncodedBitFieldDescriptor RadioLatency;
            public EncodedBitFieldDescriptor GPSModeStore;
            public EncodedBitFieldDescriptor Frequency;
            public EncodedBitFieldDescriptor Amplitude;
            public EncodedBitFieldDescriptor CCA;

            /// <summary>
            /// Initialise all descriptors
            /// </summary>
            public void Init()
            {
                MachineIDIndex.Init();
                Time.Init();
                Height.Init();
                CCV.Init();
                RMV.Init();
                MDP.Init();
                MaterialTemperature.Init();
                MachineSpeed.Init();
                RadioLatency.Init();
                GPSModeStore.Init();
                Frequency.Init();
                Amplitude.Init();
                CCA.Init();
            }

            /// <summary>
            /// Serialise all descriptors to the supplied writer
            /// </summary>
            /// <param name="writer"></param>
            public void Write(BinaryWriter writer)
            {
                MachineIDIndex.Write(writer);
                Time.Write(writer);
                Height.Write(writer);
                CCV.Write(writer);
                RMV.Write(writer);
                MDP.Write(writer);
                MaterialTemperature.Write(writer);
                MachineSpeed.Write(writer);
                RadioLatency.Write(writer);
                GPSModeStore.Write(writer);
                Frequency.Write(writer);
                Amplitude.Write(writer);
                CCA.Write(writer);
            }

            /// <summary>
            /// Deserialise all descriptors using the supplied reader
            /// </summary>
            /// <param name="reader"></param>
            public void Read(BinaryReader reader)
            {
                MachineIDIndex.Read(reader);
                Time.Read(reader);
                Height.Read(reader);
                CCV.Read(reader);
                RMV.Read(reader);
                MDP.Read(reader);
                MaterialTemperature.Read(reader);
                MachineSpeed.Read(reader);
                RadioLatency.Read(reader);
                GPSModeStore.Read(reader);
                Frequency.Read(reader);
                Amplitude.Read(reader);
                CCA.Read(reader);
            }

            /// <summary>
            /// Calculate the chained offsets and numbers of requiredbits for each attribute being stored
            /// </summary>
            /// <param name="NumBitsPerCellPass"></param>
            public void CalculateTotalOffsetBits(ref int NumBitsPerCellPass)
            {
                MachineIDIndex.OffsetBits = 0;
                Time.OffsetBits = (byte)(MachineIDIndex.OffsetBits + MachineIDIndex.RequiredBits);
                Height.OffsetBits = (byte)(Time.OffsetBits + Time.RequiredBits);
                CCV.OffsetBits = (byte)(Height.OffsetBits + Height.RequiredBits);
                RMV.OffsetBits = (byte)(CCV.OffsetBits + CCV.RequiredBits);
                MDP.OffsetBits = (byte)(RMV.OffsetBits + RMV.RequiredBits);
                MaterialTemperature.OffsetBits = (byte)(MDP.OffsetBits + MDP.RequiredBits);
                MachineSpeed.OffsetBits = (byte)(MaterialTemperature.OffsetBits + MaterialTemperature.RequiredBits);
                RadioLatency.OffsetBits = (byte)(MachineSpeed.OffsetBits + MachineSpeed.RequiredBits);
                GPSModeStore.OffsetBits = (byte)(RadioLatency.OffsetBits + RadioLatency.RequiredBits);
                Frequency.OffsetBits = (byte)(GPSModeStore.OffsetBits + GPSModeStore.RequiredBits);
                Amplitude.OffsetBits = (byte)(Frequency.OffsetBits + Frequency.RequiredBits);
                CCA.OffsetBits = (byte)(Amplitude.OffsetBits + Amplitude.RequiredBits);

                // Calculate the total number of bits required and pass back
                NumBitsPerCellPass = CCA.OffsetBits + CCA.RequiredBits;
            }
        }

        /// <summary>
        /// The time stamp of the earliest recorded cell pass stored in the segment. All time stamps for cell passes
        /// in the segment store times that are relative to this time stamp
        /// </summary>
        DateTime FirstRealCellPassTime;

        /// <summary>
        /// BF_CellPasses contains all the cell pass information for the segment (read in via
        /// the transient CellPassesStorage reference and then encoded into the cache format)
        /// </summary>
        BitFieldArray BF_CellPasses;

        // BF_PassCounts contains the pass count and first cell pass index information
        // for each cell in the segment. It is arranges in two parts:
        // 1. For each column, a value containing the summation of the pass counts up
        //    to the first cell in that column, stored as an entropic bitfield array
        //    at the start of BF_PassCounts.
        // 2. For each cell, the offset from the column value for the cell pass index
        //    of the first cell pass in that cell (so, the first cell passes index
        //    is always given as <FirstCellPassIndexOfColumn> + <FirstCellPassIndexForCellInColumn>
        BitFieldArray BF_PassCounts;

        /// <summary>
        /// EncodedColPassCountsBits containes the number of bits required to store the per column counts in BF_PassCounts.
        /// </summary>
        byte EncodedColPassCountsBits;

        /// <summary>
        /// The offset from the start of the bit field array containing the cell pass information for the cells in the 
        /// segment after the column index information that is also stored in the bit field arrat
        /// </summary>
        int FirstPerCellPassIndexOffset;

        /// <summary>
        /// The set of encoded field descriptors that track ate attributes and parameters of each vector of values
        /// stored in the bit field array.
        /// </summary>
        EncodedFieldDescriptorsStruct EncodedFieldDescriptors = new EncodedFieldDescriptorsStruct();

        // TODO Machines are not yet supported
        // FMachineIDs : Array of TICSubgridCellSegmentMachineReference; //TICMachineID;

        /// <summary>
        /// The end coded field descriptor for the vector of pass counts of cell passes in the segment
        /// </summary>
        EncodedBitFieldDescriptor PassCountEncodedFieldDescriptor;

        /// <summary>
        /// The number of bits required to store all the fields from a cell pass within the bit field array
        /// representation of that record.
        /// </summary>
        int NumBitsPerCellPass;

        /// <summary>
        /// Default no-arg constructor that does not instantiate any state
        /// </summary>
        public SubGridCellSegmentPassesDataWrapper_StaticCompressed()
        {
        }

        /// <summary>
        /// Add a pass to a cell pass list. Not supported as this is an immutable structure.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="pass"></param>
        /// <param name="position"></param>
        public void AddPass(uint X, uint Y, CellPass pass, int position = -1)
        {
            throw new InvalidOperationException("Immutable cell pass segment.");
        }

        /// <summary>
        /// Allocate cell passes for a cell. Not supported as this is an immutable structure.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="passCount"></param>
        public void AllocatePasses(uint X, uint Y, uint passCount)
        {
            throw new InvalidOperationException("Immutable cell pass segment.");
        }

        public Cell_NonStatic Cell(uint X, uint Y)
        {
            throw new InvalidOperationException("Non-static cell descriptions not supported by compressed static segments");
        }

        /// <summary>
        /// Represents the number of passes and the location of the first cell pass for a cell within the set of cell passes
        /// stored in this subgrid segment
        /// </summary>
        public struct SubGridCellPassCountRecord
        {
            /// <summary>
            /// The number of passes in this cell in this segment
            /// </summary>
            public int PassCount;

            // The index of the first cell pass inteh cell in this segment within the overall list of cell passes
            public int FirstCellPass;
        }

        /// <summary>
        /// Retrieves the number of passes present in this segment for the cell identified by col and row, as well as
        /// the index of the first cell pass in that list within the overall list of passes for the cell in this segment
        /// </summary>
        /// <param name="Col"></param>
        /// <param name="Row"></param>
        /// <returns></returns>
        public SubGridCellPassCountRecord GetPassCountAndFirstCellPassIndex(uint Col, uint Row)
        {
            // Read the per column first cell pass index, then read the first cell pass offset for the cell itself.
            // Adding the two gives us the first cell pass index for this cell. Then, calculate the passcount by reading the
            // cell pass index for the next cell. This may be in the next column, in which case the relavant first cell pass index for
            // that cell is the  per column first cell pass index for the next column in the subgrid. I the cell is the last cell in the last
            // column then the pass count is the difference between the segment pass count and per cell first cell pass count.

            SubGridCellPassCountRecord Result = new SubGridCellPassCountRecord();

            // Remember, the counts are written in column order first in the bit field array.
            int PerColBitFieldLocation = (int)Col * EncodedColPassCountsBits;
            int PerColFirstCellPassIndex = BF_PassCounts.ReadBitField(ref PerColBitFieldLocation, EncodedColPassCountsBits);

            int PerCellBitFieldLocation = (int)(FirstPerCellPassIndexOffset + ((Col * SubGridTree.SubGridTreeDimension) + Row) * PassCountEncodedFieldDescriptor.RequiredBits);
            int PerCellFirstCellPassIndexOffset = BF_PassCounts.ReadBitField(ref PerCellBitFieldLocation, PassCountEncodedFieldDescriptor.RequiredBits);

            Result.FirstCellPass = PerColFirstCellPassIndex + PerCellFirstCellPassIndexOffset;

            if (Row < SubGridTree.SubGridTreeDimension - 1)
            {
                Result.PassCount = BF_PassCounts.ReadBitField(ref PerCellBitFieldLocation, PassCountEncodedFieldDescriptor.RequiredBits) - PerCellFirstCellPassIndexOffset;
            }
            else
            {
                if (Col < SubGridTree.SubGridTreeDimension - 1)
                {
                    int NextPerColFirstCellPassIndex = BF_PassCounts.ReadBitField(ref PerColBitFieldLocation, EncodedColPassCountsBits);
                    Result.PassCount = NextPerColFirstCellPassIndex - Result.FirstCellPass;
                }
                else
                {
                    Result.PassCount = SegmentPassCount - Result.FirstCellPass;
                }
            }

            return Result;
        }

        /// <summary>
        /// Decodes all passes for the identified cell within this subgrid segment
        /// </summary>
        /// <param name="CellX"></param>
        /// <param name="CellY"></param>
        /// <returns></returns>
        public CellPass[] DecodePasses(uint CellX, uint CellY)
        {
            SubGridCellPassCountRecord Index = GetPassCountAndFirstCellPassIndex(CellX, CellY);
            CellPass[] cellPasses = new CellPass[Index.PassCount];

            for (int i = 0; i < Index.PassCount; i++)
            {
                cellPasses[i] = ExtractCellPass(Index.FirstCellPass + i);
            }

            return cellPasses;
        }

        /// <summary>
        /// Extracts a single cell pass from the cell passes within this segment from the cell identified by X and Y
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="passNumber"></param>
        /// <returns></returns>
        public CellPass ExtractCellPass(uint X, uint Y, int passNumber)
        {
            // X & Y indicate the cell lcoation in the subgrid, and passNumber represents the index of the pass in the cell that is required

            // First determine the starting cell pass index for that location in the segment
            SubGridCellPassCountRecord Index = GetPassCountAndFirstCellPassIndex(X, Y);

            // Then extract the appropriate cell pass from the list
            return ExtractCellPass(Index.FirstCellPass + passNumber);
        }

        /// <summary>
        /// Extracts a single cell pass from the cell passes held within this segment where the cell pass is identified by its index
        /// within the set of cell passes stored for the segment
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
        public CellPass ExtractCellPass(int Index)
        {
            // IMPORTANT: The fields read in this method must be read in the  same order as they were written during encoding

            CellPass Result = new CellPass();

            int CellPassBitLocation = Index * NumBitsPerCellPass;

            // TODO Machines are not yet supported
            // int MachineIndex;
            // Result.MachineIndex  = BF_CellPasses.ReadBitField(ref CellPassBitLocation, EncodedFieldDescriptors.MachineIDIndex]);
            // with FMachineIDs[MachineIndex] do
            //   begin
            //     MachineID             := _MachineID;
            //     SiteModelMachineIndex:= _SiteModelMachineIndex;
            //   end;

            Result.Time = FirstRealCellPassTime.AddSeconds(BF_CellPasses.ReadBitField(ref CellPassBitLocation, EncodedFieldDescriptors.Time));

            int IntegerHeight = BF_CellPasses.ReadBitField(ref CellPassBitLocation, EncodedFieldDescriptors.Height);
            Result.Height = IntegerHeight == EncodedFieldDescriptors.Height.NativeNullValue ? Consts.NullHeight : IntegerHeight / 1000;

            Result.CCV = (short)(BF_CellPasses.ReadBitField(ref CellPassBitLocation, EncodedFieldDescriptors.CCV));
            Result.RMV = (short)(BF_CellPasses.ReadBitField(ref CellPassBitLocation, EncodedFieldDescriptors.RMV));
            Result.MDP = (short)(BF_CellPasses.ReadBitField(ref CellPassBitLocation, EncodedFieldDescriptors.MDP));
            Result.MaterialTemperature = (ushort)(BF_CellPasses.ReadBitField(ref CellPassBitLocation, EncodedFieldDescriptors.MaterialTemperature));
            Result.MachineSpeed = (ushort)(BF_CellPasses.ReadBitField(ref CellPassBitLocation, EncodedFieldDescriptors.MachineSpeed));
            Result.RadioLatency = (byte)(BF_CellPasses.ReadBitField(ref CellPassBitLocation, EncodedFieldDescriptors.RadioLatency));

            byte gpsStore = (byte)(BF_CellPasses.ReadBitField(ref CellPassBitLocation, EncodedFieldDescriptors.GPSModeStore));
            Result.gpsMode = (GPSMode)(gpsStore & 0x0F);
            Result.passType = CellPass.PassTypeHelper.GetPassType(gpsStore);
            Result.halfPass = (gpsStore & (1 << (int)GPSFlagBits.GPSSBitHalfPass)) != 0;

            Result.Frequency = (ushort)(BF_CellPasses.ReadBitField(ref CellPassBitLocation, EncodedFieldDescriptors.Frequency));
            Result.Amplitude = (ushort)(BF_CellPasses.ReadBitField(ref CellPassBitLocation, EncodedFieldDescriptors.Amplitude));
            Result.CCA = (byte)(BF_CellPasses.ReadBitField(ref CellPassBitLocation, EncodedFieldDescriptors.CCA));

            return Result;
        }

        public void Integrate(uint X, uint Y, Cell_NonStatic source, uint StartIndex, uint EndIndex, out int AddedCount, out int ModifiedCount)
        {
            throw new InvalidOperationException("Immutable cell pass segment.");
        }

        public bool LocateTime(uint X, uint Y, DateTime time, out int index)
        {
            throw new NotImplementedException();
        }

        public CellPass Pass(uint X, uint Y, uint passIndex)
        {
            return ExtractCellPass(X, Y, (int)passIndex);
        }

        public uint PassCount(uint X, uint Y)
        {
            return (uint)GetPassCountAndFirstCellPassIndex(X, Y).PassCount;
        }

        public float PassHeight(uint passIndex)
        {
            int BitLocation = (int)(passIndex * NumBitsPerCellPass) + (EncodedFieldDescriptors.Height.OffsetBits);

            int IntegerHeight = BF_CellPasses.ReadBitField(ref BitLocation, EncodedFieldDescriptors.Height);

            return (IntegerHeight == EncodedFieldDescriptors.Height.NativeNullValue) ? Consts.NullHeight : IntegerHeight / 1000;
        }

        public float PassHeight(uint X, uint Y, uint passNumber)
        {
            // Translate the cell based pass number to the segment cell list based pass number 
            return PassHeight((uint)GetPassCountAndFirstCellPassIndex(X, Y).FirstCellPass + passNumber);
        }

        public DateTime PassTime(uint passIndex)
        {
            int BitLocation = (int)((passIndex * NumBitsPerCellPass) + EncodedFieldDescriptors.Time.OffsetBits);
            return FirstRealCellPassTime.AddSeconds(BF_CellPasses.ReadBitField(ref BitLocation, EncodedFieldDescriptors.Time));
        }

        public DateTime PassTime(uint X, uint Y, uint passNumber)
        {
            // Translate the cell based pass number to the segment cell list based pass number 
            return PassTime((uint)GetPassCountAndFirstCellPassIndex(X, Y).FirstCellPass + passNumber);
        }

        public void Read(BinaryReader reader)
        {
            FirstRealCellPassTime = DateTime.FromBinary(reader.ReadInt64());

            SegmentPassCount = reader.ReadInt32();

            BF_CellPasses.Read(reader);
            BF_PassCounts.Read(reader);

            EncodedColPassCountsBits = reader.ReadByte();
            FirstPerCellPassIndexOffset = reader.ReadInt32();

            EncodedFieldDescriptors.Read(reader);

            /* TODO: Machine are not yet supported
            int Count = reader.ReadInt32();
            SetLength(MachineIDs, Count);
            for (int j = 0; j < Count; j++) do
                {
                    MachineIDs[j].Read(reader);
                }
            */

            NumBitsPerCellPass = reader.ReadInt32();

            PassCountEncodedFieldDescriptor.Read(reader);

            // TODO: Machines are not supported yet
            // InitialiseMachineIDsSet(SiteModelReference);
        }

        public void Read(uint X, uint Y, BinaryReader reader)
        {
            throw new NotImplementedException();
        }

        public void Read(uint X, uint Y, uint passNumber, BinaryReader reader)
        {
            throw new NotImplementedException();
        }

        public void ReplacePass(uint X, uint Y, int position, CellPass pass)
        {
            throw new InvalidOperationException("Immutable cell pass segment.");
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(FirstRealCellPassTime.ToBinary());
            writer.Write(SegmentPassCount);

            BF_CellPasses.Write(writer);
            BF_PassCounts.Write(writer);

            writer.Write(EncodedColPassCountsBits);
            writer.Write(FirstPerCellPassIndexOffset);

            EncodedFieldDescriptors.Write(writer);

            // TODO: Machines not yet supported
            //writer.Write((int)MachineIDs.Length);
            //for (int j = 0; j < MachineIDs.Length; j++)
            //  FMachineIDs[j].SaveToStream(Stream);

            writer.Write(NumBitsPerCellPass);

            PassCountEncodedFieldDescriptor.Write(writer);
        }

        public void Write(uint X, uint Y, BinaryWriter writer)
        {
            throw new NotImplementedException();
        }

        public void Write(uint X, uint Y, uint passNumber, BinaryWriter writer)
        {
            throw new NotImplementedException();
        }

        public void PerformEncodingStaticCompressedCache(CellPass[,][] cellPasses)
        {
            int segmentPassCount = 0;
            BitFieldArrayRecordsDescriptor[] recordDescriptors;
            int[] ColFirstCellPassIndexes = new int[SubGridTree.SubGridTreeDimension];
            int[,] PerCellColRelativeFirstCellPassIndexes = new int[SubGridTree.SubGridTreeDimension, SubGridTree.SubGridTreeDimension];
            int cellPassIndex;
            int testValue;
            bool observedANullValue;
            bool firstValue;

            // TODO Machines not supported yet
            //  bool foundMachineID;

            // Given the value range for each attribute, calculate the number of bits required to store the values.
            EncodedFieldDescriptors.Init();

            // Calculate the set of machine IDs present in the cell passes in the
            // segment. Calculate the cell pass time of the earliest cell pass in the segment
            // and the lowest elevation of all cell passes in the segment.

            //TODO: Not handling machines yet...
            //    SetLength(FMachineIDs, 0); 

            // Construct the first cell pass index map for the segment
            // First calculate the values of the first cell pass index for each column in the segment
            ColFirstCellPassIndexes[0] = 0;
            for (int Col = 0; Col < SubGridTree.SubGridTreeDimension - 1; Col++)
            {
                ColFirstCellPassIndexes[Col + 1] = ColFirstCellPassIndexes[Col];

                for (int Row = 0; Row < SubGridTree.SubGridTreeDimension; Row++)
                {
                    segmentPassCount += cellPasses[Col, Row].Length;
                    ColFirstCellPassIndexes[Col + 1] += cellPasses[Col, Row].Length;
                }
            }

            // Next modify the cell passes array to hold first cell pass indices relative to the
            // 'per column' first cell pass indices
            for (int Col = 0; Col < SubGridTree.SubGridTreeDimension - 1; Col++)
            {
                PerCellColRelativeFirstCellPassIndexes[Col, 0] = 0;

                for (int Row = 0; Row < SubGridTree.SubGridTreeDimension; Row++)
                {
                    PerCellColRelativeFirstCellPassIndexes[Col, Row] = PerCellColRelativeFirstCellPassIndexes[Col, Row - 1] + cellPasses[Col, Row - 1].Length;
                }
            }

            // Compute the value range and number of bits required to store the column first cell passes indices
            PassCountEncodedFieldDescriptor.Init();

            PassCountEncodedFieldDescriptor.MinValue = int.MaxValue;
            PassCountEncodedFieldDescriptor.MaxValue = 0;

            for (int Col = 0; Col < SubGridTree.SubGridTreeDimension - 1; Col++)
            {
                cellPassIndex = ColFirstCellPassIndexes[Col];

                if (cellPassIndex < PassCountEncodedFieldDescriptor.MinValue)
                {
                    PassCountEncodedFieldDescriptor.MinValue = cellPassIndex;
                }
                if (cellPassIndex > PassCountEncodedFieldDescriptor.MaxValue)
                {
                    PassCountEncodedFieldDescriptor.MaxValue = cellPassIndex;
                }
            }

            PassCountEncodedFieldDescriptor.CalculateRequiredBitFieldSize();
            EncodedColPassCountsBits = PassCountEncodedFieldDescriptor.RequiredBits;
            FirstPerCellPassIndexOffset = SubGridTree.SubGridTreeDimension * EncodedColPassCountsBits;

            // Compute the value range and number of bits required to store the cell first cell passes indices
            PassCountEncodedFieldDescriptor.Init();

            PassCountEncodedFieldDescriptor.NativeNullValue = 0;
            PassCountEncodedFieldDescriptor.MinValue = 0;
            PassCountEncodedFieldDescriptor.MaxValue = 0;

            observedANullValue = false;
            firstValue = true;

            SubGridUtilities.SubGridDimensionalIterator((Col, Row) =>
            {
                testValue = PerCellColRelativeFirstCellPassIndexes[Col, Row];

                if (PassCountEncodedFieldDescriptor.Nullable)
                {
                    if (PassCountEncodedFieldDescriptor.MinValue == PassCountEncodedFieldDescriptor.NativeNullValue
                              || (testValue != PassCountEncodedFieldDescriptor.NativeNullValue) && (testValue < PassCountEncodedFieldDescriptor.MinValue))
                    {
                        PassCountEncodedFieldDescriptor.MinValue = testValue;
                    }

                    if (PassCountEncodedFieldDescriptor.MaxValue == PassCountEncodedFieldDescriptor.NativeNullValue ||
                        (testValue != PassCountEncodedFieldDescriptor.NativeNullValue) && (testValue > PassCountEncodedFieldDescriptor.MaxValue))
                    {
                        PassCountEncodedFieldDescriptor.MaxValue = testValue;
                    }
                }
                else
                {
                    if (firstValue || testValue < PassCountEncodedFieldDescriptor.MinValue)
                    {
                        PassCountEncodedFieldDescriptor.MinValue = testValue;
                    }

                    if (firstValue || testValue > PassCountEncodedFieldDescriptor.MaxValue)
                    {
                        PassCountEncodedFieldDescriptor.MaxValue = testValue;
                    }
                }

                if (!observedANullValue && testValue == PassCountEncodedFieldDescriptor.NativeNullValue)
                {
                    observedANullValue = true;
                }

                firstValue = false;
            });

            // If the data stream processed contained no null values, then force the
            // nullable flag to flas so we don;t encode an extra token for a null value
            // that will never be written.
            if (!observedANullValue)
            {
                PassCountEncodedFieldDescriptor.Nullable = false;
            }

            if (PassCountEncodedFieldDescriptor.Nullable && PassCountEncodedFieldDescriptor.MaxValue != PassCountEncodedFieldDescriptor.NativeNullValue)
            {
                PassCountEncodedFieldDescriptor.MaxValue++;
                PassCountEncodedFieldDescriptor.EncodedNullValue = PassCountEncodedFieldDescriptor.MaxValue;
            }
            else
            {
                PassCountEncodedFieldDescriptor.EncodedNullValue = 0;
            }

            PassCountEncodedFieldDescriptor.CalculateRequiredBitFieldSize();

            // For ease of management convert all the cell passes into a single list for the following operations
            CellPass[] allCellPassesArray = new CellPass[SegmentPassCount];
            cellPassIndex = 0;

            SubGridUtilities.SubGridDimensionalIterator((col, row) =>
            {
                CellPass[] passes = cellPasses[col, row];
                Array.Copy(passes, 0, allCellPassesArray, cellPassIndex, passes.Length);
                cellPassIndex += passes.Length;
            });

            // Compute the time of the earliest real cell pass within the segment
            FirstRealCellPassTime = allCellPassesArray.Length > 0 ? allCellPassesArray.Min(x => x.Time) : DateTime.MinValue;

            // Convert time and elevation value to offset values in the appropriate units
            // from the lowest values of those attributes. 

            /* TODO Machines not supported yet
            int modifiedIndex = -1;
            int[] ModifiedMachineIDs = new int[segmentPassCount];

            SubGridUtilities.SubGridDimensiontalIterator((col, row) =>
            {
                CellPass[] passes = cellPasses[col, row];

                for (int I = 0; I < passes.Length; I++)
                {
                    modifiedIndex++;

                    foundMachineID = false;
                    for (int J = 0; J < MachineIDs.Length; J++)
                    {
                        if (MachineIDs[J]._MachineID == MachineID)
                        {
                            ModifiedMachineIDs[modifiedIndex] = J;
                            foundMachineID = true;
                            break;
                        }
                    }

                    if (!foundMachineID)
                    {
                        SetLength(MachineIDs, MachineIDs.Length + 1);
                        MachineIDs[MachineIDs.Length - 1]._MachineID := MachineID;

                        // Determine the sitemodel relevant machine index (ie: the index in the list
                        // of machines held in the site model) for the machine. These indexes provide rapid
                        // location of the machine in the sitemodel machines list.
                        if (SiteModelReference != null)
                        {
                            MachineIDS[Length(FMachineIDs) - 1]._SiteModelMachineIndex = TICSiteModel(SiteModelReference).Machines.IndexOfID(MachineID);
                        }
                        else
                        {
                            MachineIDS[Length(FMachineIDs) - 1]._SiteModelMachineIndex = High(MachineIDS[MachineIDs.Length - 1]._SiteModelMachineIndex);
                        }

                        ModifiedMachineIDs[modifiedIndex] = MachineIDs.Length - 1;
                    }
                }
            });
            */

            // TODO not handling machines yet
            // InitialiseMachineIDsSet(SiteModelReference);

            // Work out the value ranges of all the attributes and given the value range
            // for each attribute, calculate the number of bits required to store the values.
            // Note:
            // Time - based on the longword, second accurate times overriding the TDateTime times
            // Height - based on the longword, millimeter accurate elevations overriding the IEEE double elevations
            // GPSMode - take the least significant 4 bits of the GPSModeStore

            // TODO Machines are not supported yet
            // AttributeValueRangeCalculator.CalculateAttributeValueRange(ModifiedMachines, 0xffffffff, 0, false, ref EncodedFieldDescriptors[EncodedFieldType.MachineIDIndex]);
            AttributeValueRangeCalculator.CalculateAttributeValueRange(allCellPassesArray.Select(x => AttributeValueModifiers.ModifiedTime(x.Time, FirstRealCellPassTime)).ToArray(), 0xffffffff, 0, false, ref EncodedFieldDescriptors.Time);
            AttributeValueRangeCalculator.CalculateAttributeValueRange(allCellPassesArray.Select(x => AttributeValueModifiers.ModifiedHeight(x.Height)).ToArray(), 0xffffffff, 0x7fffffff, true, ref EncodedFieldDescriptors.Height);
            AttributeValueRangeCalculator.CalculateAttributeValueRange(allCellPassesArray.Select(x => (int)x.CCV).ToArray(), 0xffffffff, CellPass.NullCCV, true, ref EncodedFieldDescriptors.CCV);
            AttributeValueRangeCalculator.CalculateAttributeValueRange(allCellPassesArray.Select(x => (int)x.RMV).ToArray(), 0xffffffff, CellPass.NullRMV, true, ref EncodedFieldDescriptors.RMV);
            AttributeValueRangeCalculator.CalculateAttributeValueRange(allCellPassesArray.Select(x => (int)x.MDP).ToArray(), 0xffffffff, CellPass.NullMDP, true, ref EncodedFieldDescriptors.MDP);
            AttributeValueRangeCalculator.CalculateAttributeValueRange(allCellPassesArray.Select(x => (int)x.MaterialTemperature).ToArray(), 0xffffffff, CellPass.NullMaterialTemp, true, ref EncodedFieldDescriptors.MaterialTemperature);
            AttributeValueRangeCalculator.CalculateAttributeValueRange(allCellPassesArray.Select(x => AttributeValueModifiers.ModifiedGPSMode(x.gpsMode)).ToArray(), 0xff, (int)CellPass.NullGPSMode, true, ref EncodedFieldDescriptors.GPSModeStore);
            AttributeValueRangeCalculator.CalculateAttributeValueRange(allCellPassesArray.Select(x => (int)x.MachineSpeed).ToArray(), 0xffffffff, CellPass.NullMachineSpeed, true, ref EncodedFieldDescriptors.MachineSpeed);
            AttributeValueRangeCalculator.CalculateAttributeValueRange(allCellPassesArray.Select(x => (int)x.RadioLatency).ToArray(), 0xffffffff, CellPass.NullRadioLatency, true, ref EncodedFieldDescriptors.RadioLatency);
            AttributeValueRangeCalculator.CalculateAttributeValueRange(allCellPassesArray.Select(x => (int)x.Frequency).ToArray(), 0xffffffff, CellPass.NullFrequency, true, ref EncodedFieldDescriptors.Frequency);
            AttributeValueRangeCalculator.CalculateAttributeValueRange(allCellPassesArray.Select(x => (int)x.Amplitude).ToArray(), 0xffffffff, CellPass.NullAmplitude, true, ref EncodedFieldDescriptors.Amplitude);
            AttributeValueRangeCalculator.CalculateAttributeValueRange(allCellPassesArray.Select(x => (int)x.CCA).ToArray(), 0xff, CellPass.NullCCA, true, ref EncodedFieldDescriptors.CCA);

            // Calculate the offset bit locations for the cell pass attributes
            EncodedFieldDescriptors.CalculateTotalOffsetBits(ref NumBitsPerCellPass);

            // Create the bit field arrays to contain the segment call pass index & count plus passes.
            recordDescriptors = new BitFieldArrayRecordsDescriptor[] 
            {
                new BitFieldArrayRecordsDescriptor()
                {
                    NumRecords = SubGridTree.SubGridTreeDimension,
                    BitsPerRecord = EncodedColPassCountsBits
                },
                new BitFieldArrayRecordsDescriptor()
                {
                    NumRecords = SubGridTree.SubGridTreeDimension * SubGridTree.SubGridTreeDimension,
                    BitsPerRecord = PassCountEncodedFieldDescriptor.RequiredBits
                }
            };

            BF_PassCounts.Initialise(recordDescriptors);
            BF_PassCounts.StreamWriteStart();
            try
            {
                // Write the column based first cell pass indexes into BF_PassCounts
                foreach (int firstPassIndex in ColFirstCellPassIndexes)
                {
                    BF_PassCounts.StreamWrite(firstPassIndex, EncodedColPassCountsBits);
                }

                // Write the cell pass count for each cell relative to the column based cell pass count
                SubGridUtilities.SubGridDimensionalIterator((col, row) => BF_PassCounts.StreamWrite(PerCellColRelativeFirstCellPassIndexes[col, row], PassCountEncodedFieldDescriptor.RequiredBits));
            }
            finally
            {
                BF_PassCounts.StreamWriteEnd();
            }

            // Copy the call passes themselves into BF
            recordDescriptors = new BitFieldArrayRecordsDescriptor[] 
            {            
                new BitFieldArrayRecordsDescriptor()
                {
                    NumRecords = SegmentPassCount,
                    BitsPerRecord = NumBitsPerCellPass
                }
            };

            BF_CellPasses.Initialise(recordDescriptors);
            BF_CellPasses.StreamWriteStart();
            try
            {
                foreach (CellPass pass in allCellPassesArray)
                {
                    // TODO Machine are not yet supported
                    // BF_CellPasses.StreamWrite(ModifiedMachineIDs, EncodedFieldDescriptors.MachineIDIndex);

                    BF_CellPasses.StreamWrite(AttributeValueModifiers.ModifiedTime(pass.Time, FirstRealCellPassTime), EncodedFieldDescriptors.Time);
                    BF_CellPasses.StreamWrite(AttributeValueModifiers.ModifiedHeight(pass.Height), EncodedFieldDescriptors.Height);
                    BF_CellPasses.StreamWrite(pass.CCV, EncodedFieldDescriptors.CCV);
                    BF_CellPasses.StreamWrite(pass.RMV, EncodedFieldDescriptors.RMV);
                    BF_CellPasses.StreamWrite(pass.MDP, EncodedFieldDescriptors.MDP);
                    BF_CellPasses.StreamWrite(pass.MaterialTemperature, EncodedFieldDescriptors.MaterialTemperature);
                    BF_CellPasses.StreamWrite(pass.MachineSpeed, EncodedFieldDescriptors.MachineSpeed);
                    BF_CellPasses.StreamWrite(pass.RadioLatency, EncodedFieldDescriptors.RadioLatency);
                    BF_CellPasses.StreamWrite(AttributeValueModifiers.ModifiedGPSMode(pass.gpsMode), EncodedFieldDescriptors.GPSModeStore);
                    BF_CellPasses.StreamWrite(pass.Frequency, EncodedFieldDescriptors.Frequency);
                    BF_CellPasses.StreamWrite(pass.Amplitude, EncodedFieldDescriptors.Amplitude);
                    BF_CellPasses.StreamWrite(pass.CCA, EncodedFieldDescriptors.CCA);
                }
            }
            finally
            {
                BF_CellPasses.StreamWriteEnd();
            }

            /*
            {$IFDEF DEBUG}
            // Read the values back again to check they were written as expected
            TestReadIndex:= 0;

            with BF_CellPasses do
              for I := 0 to SegmentPassCount - 1 do
                with CellPassesStorage[I] do
                  begin
                    ReadBitField(TestReadIndex, FEncodedFieldDescriptors[eftMachineIDIndex]);
                    ReadBitField(TestReadIndex, FEncodedFieldDescriptors[eftTime]);
                    ReadBitField(TestReadIndex, FEncodedFieldDescriptors[eftHeight]);
                    ReadBitField(TestReadIndex, FEncodedFieldDescriptors[eftCCV]);
                    ReadBitField(TestReadIndex, FEncodedFieldDescriptors[eftRMV]);
                    TestMDP:= ReadBitField(TestReadIndex, FEncodedFieldDescriptors[eftMDP]);
                    if TestMDP <> MDP then
                      TestReadIndex := TestReadIndex;
          
                    ReadBitField(TestReadIndex, FEncodedFieldDescriptors[eftMaterialTemperature]);
                 end;
            {$ENDIF}
            */

            // if not VerifyCellPassEncoding then
            //   SIGLogMessage.PublishNoODS(Self, 'Segment VerifyCellPassEncoding failed', slmcMessage);
        }

        /// <summary>
        /// Takes a description of the cell passes to be placed into this segment and converts them into the
        /// internal compressed representation
        /// </summary>
        /// <param name="cellPasses"></param>
        public void SetState(CellPass[,][] cellPasses)
        {
            // Convert the supplied cell passes into the appropriate bit field arrays
            PerformEncodingStaticCompressedCache(cellPasses);
        }
    }
}
