﻿namespace VSS.TRex.SubGridTrees.Server.Interfaces
{
    /// <summary>
    /// The time iteration direction for iterating through the segments in the sub grid
    /// </summary>
    public enum IterationDirection
    {
        /// <summary>
        /// Iteration proceeds from oldest segment to newest segment
        /// </summary>
        Forwards,

        /// <summary>
        /// Iteration proceeds from newest segment to oldest segment
        /// </summary>
        Backwards
    }
}
