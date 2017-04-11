﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.Utilities
{
    public static class PassCountSize
    {
        /// <summary>
        /// Determine the sizing increment needed to store pass counts (1 for less than 256, 2 for less than 32768, 3 otherwise
        /// </summary>
        /// <param name="MaxPassCounts"></param>
        /// <returns></returns>
        public static int Calculate(int MaxPassCounts)
        {
            return MaxPassCounts < (1 << 8) ? 1 : MaxPassCounts < (1 << 16) ? 2 : 3;
        }
    }
}
