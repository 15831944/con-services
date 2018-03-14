﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Geometry;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;

namespace VSS.VisionLink.Raptor.SubGridTrees
{
    /// <summary>
    ///  A subgrid variant that contains a bit mask construct to represent a one-bit-per-pixel map
    /// </summary>
    public class SubGridTreeLeafBitmapSubGrid : SubGrid, ISubGrid, ILeafSubGrid
    {
        public SubGridTreeBitmapSubGridBits Bits;

        /// <summary>
        /// Writes the contents of the subgrid bit mask to the writer
        /// </summary>
        /// <param name="writer"></param>
        public override void Write(BinaryWriter writer, byte [] buffer)
        {
            Bits.Write(writer, buffer);
        }

        /// <summary>
        /// Reads the contents of the subgrid bit mask from the reader
        /// </summary>
        /// <param name="reader"></param>
        public override void Read(BinaryReader reader, byte [] buffer)
        {
            Bits.Read(reader, buffer);
        }

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public SubGridTreeLeafBitmapSubGrid()
        {
            Bits = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);
        }

        /// <summary>
        /// Constructor taking the tree reference, parent and level of the subgrid to be created
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="parent"></param>
        /// <param name="level"></param>
        public SubGridTreeLeafBitmapSubGrid(ISubGridTree owner,
            SubGrid parent,
            byte level /*,
            double cellSize,
            int indexOriginOffset*/) : base(owner, parent, level /*, cellSize, indexOriginOffset*/)
        {
            Bits = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);
        }

        /// <summary>
        /// CountBits counts the number of bits that are set to 1 (true) in the subgrid 
        /// </summary>
        /// <returns></returns>
        public uint CountBits() => Bits.CountBits();

        /// <summary>
        /// Computes the bounding extent of the cells (bits) in the subgrid that are set to 1 (true)
        /// </summary>
        /// <returns></returns>
        public BoundingIntegerExtent2D ComputeCellsExtents()
        {
            BoundingIntegerExtent2D Result = Bits.ComputeCellsExtents();

            if (Result.IsValidExtent)
            {
                Result.Offset((int)OriginX, (int)OriginY);
            }

            return Result;
        }
    }
}
