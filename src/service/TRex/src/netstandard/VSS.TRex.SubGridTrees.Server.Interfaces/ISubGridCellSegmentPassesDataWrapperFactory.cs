﻿namespace VSS.TRex.SubGridTrees.Server.Interfaces
{
    /// <summary>
    /// Interface for the sub grid cell segment cell pass collection wrapper factory
    /// </summary>
    public interface ISubGridCellSegmentPassesDataWrapperFactory
    {
        ISubGridCellSegmentPassesDataWrapper NewMutableWrapper();
        ISubGridCellSegmentPassesDataWrapper NewImmutableWrapper();
    }
}
