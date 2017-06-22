﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Cells;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.SubGridTrees.Server.Interfaces
{
    public interface ISubGridCellLatestPassDataWrapper
    {
        SubGridTreeBitmapSubGridBits PassDataExistanceMap { get; set; }

        SubGridTreeBitmapSubGridBits CCVValuesAreFromLastPass { get; set; }
        SubGridTreeBitmapSubGridBits RMVValuesAreFromLastPass { get; set; }
        SubGridTreeBitmapSubGridBits FrequencyValuesAreFromLastPass { get; set; }
        SubGridTreeBitmapSubGridBits AmplitudeValuesAreFromLastPass { get; set; }
        SubGridTreeBitmapSubGridBits GPSModeValuesAreFromLatestCellPass { get; set; }
        SubGridTreeBitmapSubGridBits TemperatureValuesAreFromLastPass { get; set; }
        SubGridTreeBitmapSubGridBits MDPValuesAreFromLastPass { get; set; }
        SubGridTreeBitmapSubGridBits CCAValuesAreFromLastPass { get; set; }

        void Clear();
        void AssignValuesFromLastPassFlags(ISubGridCellLatestPassDataWrapper Source);

        /// <summary>
        /// An indexer supporting accessing the 2D array of 'last' pass information for the cell in a subgrid or segment
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        CellPass this[int x, int y] { get;  set; }

        void Read(BinaryReader reader);
        void Write(BinaryWriter writer);

        short ReadCCV(int Col, int Row);
        short ReadRMV(int Col, int Row);
        ushort ReadFrequency(int Col, int Row);
        ushort ReadAmplitude(int Col, int Row);
        byte ReadCCA(int Col, int Row);
        GPSMode ReadGPSMode(int Col, int Row);
        short ReadMDP(int Col, int Row);
        ushort ReadTemperature(int Col, int Row);
    }
}
