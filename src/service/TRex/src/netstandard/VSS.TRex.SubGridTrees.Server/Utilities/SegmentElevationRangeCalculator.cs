﻿using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server.Interfaces;

namespace VSS.TRex.SubGridTrees.Server.Utilities
{
    public static class SegmentElevationRangeCalculator
    {
        /// <summary>
        /// Calculates the elevation range that all the passes within the given segment span.
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="minElevation"></param>
        /// <param name="maxElevation"></param>
        /// <returns>True if the result of the calculation yields a valid elevation range</returns>
        public static bool CalculateElevationRangeOfPasses(ISubGridCellSegmentPassesDataWrapper segment, 
            out double minElevation, out double maxElevation)
        {
            minElevation = 1E10;
            maxElevation = -1E10;

            double min = minElevation;
            double max = maxElevation;

            for (int i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
            {
              for (int j = 0; j < SubGridTreeConsts.SubGridTreeDimension; j++)
              {
                int _PassCount = segment.PassCount(i, j);

                for (int PassIndex = 0; PassIndex < _PassCount; PassIndex++)
                {
                  // Todo: Delegate this down to the segment to avoid the PassHeight() abstraction
                  float _height = segment.PassHeight(i, j, PassIndex);

                  if (_height > max)
                    max = _height;

                  if (_height < min)
                    min = _height;
                }
              }
            }

            if (min <= max)
            {
                minElevation = min;
                maxElevation = max;
            }

            return minElevation <= maxElevation;
        }
    }
}
