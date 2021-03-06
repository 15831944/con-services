﻿using System;
using System.Runtime.CompilerServices;
using VSS.TRex.Common;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.Types;

namespace VSS.TRex.Compression
{
    /// <summary>
    /// Supports logic for consistent modification of attributes used during compression using bit field arrays
    /// </summary>
    public static class AttributeValueModifiers
    {
        public const int MILLISECONDS_TO_DECISECONDS_FACTOR = 100;

        /// <summary>
        /// Performs a computation to modify the height into the form used by the compressed static version
        /// of the segment cell pass information, which is an integer number of millimeters above datum.
        /// </summary>
        /// <param name="height"></param>
        /// <returns></returns>
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ModifiedHeight(float height) => (long)(height == Consts.NullHeight ? int.MaxValue : Math.Round(height * 1000));

        public const long MODIFIED_TIME_PROJECTED_NULL_VALUE = -1;

        /// <summary>
        /// Performs a computation to modify the time into the form used by the compressed static version
        /// of the segment cell pass information, which is a relative time offset from an origin, expressed with 
        /// a resolution of 100 milliseconds.
        /// </summary>
        /// <param name="time"></param>
        /// <param name="timeOrigin"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ModifiedTime(DateTime time, DateTime timeOrigin)
        {
          if (time.Kind != DateTimeKind.Utc || timeOrigin.Kind != DateTimeKind.Utc)
            throw new ArgumentException("Time and time origin must be a UTC date time");

          if (time == Consts.MIN_DATETIME_AS_UTC)
            return MODIFIED_TIME_PROJECTED_NULL_VALUE;

          var span = time - timeOrigin;
          if (span.TotalMilliseconds < 0)
            throw new ArgumentException($"Time argument [{time}] should not be less that the origin [{timeOrigin}]");

         return (long)Math.Truncate(span.TotalMilliseconds) / MILLISECONDS_TO_DECISECONDS_FACTOR;
        }
    }
}
