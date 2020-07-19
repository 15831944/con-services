﻿using System;
using System.IO;
using System.Threading;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Types;

namespace VSS.TRex.SubGridTrees.Interfaces
{
    public interface ISubGridTree
    {
        /// <summary>
        /// Internal numeric identifier for the sub grid tree. All internal operations will refer to the sub grid
        /// tree using this identifier. 
        /// </summary>
        Guid ID { get; set; }

        /// <summary>
        /// The number of levels defined in this sub grid tree. 
        /// A 6 level tree typically defines leaf cells as relating to on-the-ground cell in the real world
        /// coordinate system (eg: cells tracking passes made by construction machines)
        /// A 5 level tree typically defines leaf cells that represent some aspect of the sub grids in the 
        /// 6th layer of the tree containing on-the-ground leaf cells (eg: sub grid existence map)
        /// This property is assignable only at the time the sub grid tree is constructed.
        /// </summary>
        byte NumLevels { get; }

        /// <summary>
        /// The real world size on the ground of a cell in the grid. This applies to tree with different numbers of levels.
        /// This property is mutable at any time as it does not modify any internal storage concerns, but it will change the 
        /// calculated answers to queries as CellSize relates the spread of cells across the real world coordinate system the
        /// data stored in the sub grid tree was collected.
        /// </summary>
        double CellSize { get; set; }

        /// <summary>
        /// The maximum (positive and negative) real world value for both X and Y axes that may be encompassed by the grid
        /// </summary>
        double MaxOrdinate { get; }

        /// <summary>
        /// The value of the index origin offset for this sub grid tree
        /// </summary>
        int IndexOriginOffset { get; }

        /// <summary>
        /// Root is the top level sub grid in a sub grid tree. All other sub grids are children or descendents from
        /// this node. Root is an INodeSubGrid interface, a descendent from ISubGrid. Root is automatically created when the SubGridTree is created.
        /// </summary>
        INodeSubGrid Root { get; }

        /// <summary>
        /// Clears all content from the sub grid tree and resets the root node to empty
        /// </summary>
        void Clear();

        /// <summary>
        /// ScanSubGrids scans all sub grids at a requested level in the tree that
        /// intersect the given real world extent. Each sub grid that exists in the
        /// extent is passed to the OnProcessLeafSubGrid event for processing 
        /// </summary>
        bool ScanSubGrids(BoundingWorldExtent3D extent,
                                 Func<ISubGrid, bool> leafFunctor = null,
                                 Func<ISubGrid, SubGridProcessNodeSubGridResult> nodeFunctor = null);

        /// <summary>
        /// ScanSubGrids scans all sub grids at a requested level in the tree that
        /// intersect the given cell address space extent. Each sub grid that exists in the
        /// extent is passed to the OnProcessLeafSubGrid event for processing 
        /// </summary>
        bool ScanSubGrids(BoundingIntegerExtent2D extent,
                          Func<ISubGrid, bool> leafFunctor = null,
                          Func<ISubGrid, SubGridProcessNodeSubGridResult> nodeFunctor = null);

        /// <summary>
        /// ScanAllSubGrids scans all sub grids. Each sub grid that exists in the
        /// extent is passed to the OnProcessLeafSub grid event for processing 
        /// </summary>
        bool ScanAllSubGrids(Func<ISubGrid, bool> leafFunctor = null,
                             Func<ISubGrid, SubGridProcessNodeSubGridResult> nodeFunctor = null);

        /// <summary>
        /// CountLeafSubGridsInMemory counts the number of leaf sub grids within the tree that currently reside in memory.
        /// </summary>
        /// <returns>The number of leaf sub grids in the tree</returns>
        int CountLeafSubGridsInMemory();

        /// <summary>
        /// FullGridExtent returns the maximum world extent that this grid is capable of covering.
        /// </summary>
        BoundingWorldExtent3D FullGridExtent();

        /// <summary>
        /// FullCellExtent returns the total extent of cells within this sub grid tree. 
        /// </summary>
        BoundingIntegerExtent2D FullCellExtent();

        /// <summary>
        /// ConstructPathToCell constructs all necessary sub grids in all levels in
        /// the tree so that there is a path that is can be traversed from the root of the
        /// tree to the leaf sub grid that will contain the cell identified by
        /// CellX and CellY. If PathType is pctCreateLeaf it returns the leaf
        /// sub grid instance into which the caller may place the cell data. If
        /// PathType is pctCreatePathToLeaf it returns the node sub grid instance that
        /// owns the leaf sub grid that contains the cell
        /// </summary>

        ISubGrid ConstructPathToCell(int cellX, int cellY, SubGridPathConstructionType pathType);

        /// <summary>
        /// CalculateIndexOfCellContainingPosition takes a world position and determines
        /// the X/Y index of the cell that the position lies in. If the position is
        /// outside of the extent covered by the grid the function returns false.
        /// </summary>

        bool CalculateIndexOfCellContainingPosition(double x, double y,
                                                    out int cellX, out int cellY);

        /// <summary>
        /// LocateSubGridContaining attempts to locate a sub grid at the level in the tree
        /// given by Level that contains the on-the-ground cell identified by
        /// CellX and CellY
        /// </summary>
        ISubGrid LocateSubGridContaining(int cellX, int cellY, byte level);

        /// <summary>
        /// LocateSubGridContaining attempts to locate a sub grid at the level in the tree,
        /// but defaults to looking at the bottom level
        /// CellX and CellY
        /// </summary>
        ISubGrid LocateSubGridContaining(int cellX, int cellY);

        /// <summary>
        /// LocateClosestSubGridContaining behaves much like LocateSubGridContaining()
        /// except that it walks as far through the tree as it can up to the designated
        /// Level to find the requested cell, then returns that sub grid.
        /// The returned node may be a leaf sub grid or a node sub grid
        /// </summary>
        ISubGrid LocateClosestSubGridContaining(int cellX, int cellY, byte level);

        /// <summary>
        /// GetCellCenterPosition computes the real world location of the center
        /// of the on-the-ground cell identified by X and Y. X and Y are in the
        /// bottom left origin of the grid. The returned CX, CY values are translated
        /// to the centered origin of the real world coordinate system
        /// </summary>
        void GetCellCenterPosition(int x, int y, out double cx, out double cy);

        /// <summary>
        /// GetCellOriginPosition computes the real world location of the origin
        /// of the on-the-ground cell identified by X and Y. X and Y are in the
        /// bottom left origin of the grid. The returned OX, OY values are translated
        /// to the centered origin of the real world coordinate system
        /// </summary>
        void GetCellOriginPosition(int x, int y, out double ox, out double oy);

        /// <summary>
        /// GetCellExtents computes the real world extents of the OTG cell identified
        /// by X and Y. X and Y are in the bottom left origin of the grid.
        /// The returned extents are translated to the centered origin of the real
        /// world coordinate system
        /// </summary>
        BoundingWorldExtent3D GetCellExtents(int x, int y);

        /// <summary>
        /// GetCellExtents computes the real world extents of the OTG cell identified
        /// by X and Y. X and Y are in the bottom left origin of the grid.
        /// The returned extents are translated to the centered origin of the real
        /// world coordinate system
        /// </summary>
        void GetCellExtents(int x, int y, ref BoundingWorldExtent3D extents);
   
        /// <summary>
        /// CreateUnattachedLeaf Creates an instance of a sub grid leaf node and returns
        /// it to the caller. The newly created sub grid is _not_ attached to this grid.
        /// </summary>
        ILeafSubGrid CreateUnattachedLeaf();

        /// <summary>
        /// CalculateRegionGridCoverage determines the extent of on-the-ground grid cells that correspond to the given world extent.
        /// </summary>
        void CalculateRegionGridCoverage(BoundingWorldExtent3D worldExtent, out BoundingIntegerExtent2D cellExtent);

        /// <summary>
        /// CreateNewSubGrid creates a new sub grid relevant to the requested level
        /// in the tree. This new sub grid is not added into the tree structure -
        /// it is unattached until explicitly inserted.
        /// </summary>
        ISubGrid CreateNewSubGrid(byte level);

        void InitialiseReaderWriterLocking();
        ReaderWriterLockSlim ReaderWriterLock { get; }

        byte[] ToBytes();

        void FromBytes(byte[] bytes);

        MemoryStream ToStream();

        void ToStream(Stream stream);

        void FromStream(MemoryStream stream);

        void Dispose();
    }
}
