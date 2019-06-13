﻿using System;
using VSS.TRex.TAGFiles.Types;
using Xunit;

namespace TAGFiles.Tests
{
        public class IntegerFieldNybbleSizesTests
    {
        [Fact]
        public void Test_IntegerFieldNybbleSizes_Sizes()
        {
            Assert.Equal(1, IntegerNybbleSizes.GetNybbles(TAGDataType.t4bitInt));
            Assert.Equal(1, IntegerNybbleSizes.GetNybbles(TAGDataType.t4bitUInt));

            Assert.Equal(2, IntegerNybbleSizes.GetNybbles(TAGDataType.t8bitInt));
            Assert.Equal(2, IntegerNybbleSizes.GetNybbles(TAGDataType.t8bitUInt));

            Assert.Equal(3, IntegerNybbleSizes.GetNybbles(TAGDataType.t12bitInt));
            Assert.Equal(3, IntegerNybbleSizes.GetNybbles(TAGDataType.t12bitUInt));

            Assert.Equal(4, IntegerNybbleSizes.GetNybbles(TAGDataType.t16bitInt));
            Assert.Equal(4, IntegerNybbleSizes.GetNybbles(TAGDataType.t16bitUInt));

            Assert.Equal(8, IntegerNybbleSizes.GetNybbles(TAGDataType.t32bitInt));
            Assert.Equal(8, IntegerNybbleSizes.GetNybbles(TAGDataType.t32bitUInt));
        }
    }
}
