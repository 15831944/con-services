﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.Compression
{
    /// <summary>
    /// Describes the number of records and number of bits per record stored within a bit field array
    /// </summary>
    public struct BitFieldArrayRecordsDescriptor
    {
        public int NumRecords;
        public int BitsPerRecord;
    }
}
