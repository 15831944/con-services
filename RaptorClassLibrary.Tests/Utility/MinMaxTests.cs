﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor.Utilities;

namespace VSS.VisionLink.Raptor.RaptorClassLibrary.MinMax.Tests
{
    [TestClass]
    public class MinMaxTests
    {
        [TestMethod]
        public void Test_Swap()
        {
            int a = 10;
            int b = 20;

            Raptor.Utilities.MinMax.Swap(ref a, ref b);
            Assert.IsTrue(a == 20 && b == 10, "Swap failed to swap items");

            Raptor.Utilities.MinMax.Swap<int>(ref a, ref b);
            Assert.IsTrue(a == 10 && b == 20, "Swap failed to swap items");
        }

        [TestMethod]
        public void Test_SetMinMax()
        {
            double a = 10;
            double b = 20;

            Raptor.Utilities.MinMax.SetMinMax(ref a, ref b);
            Assert.IsTrue(a == 10 && b == 20, "SetMinMax swapped values when it should not");

            double c = 20;
            double d = 10;

            Raptor.Utilities.MinMax.SetMinMax(ref c, ref d);
            Assert.IsTrue(c == 10 && d == 20, "SetMinMax did not swap values when it should");
        }

        [TestMethod]
        public void Test_SetMinMax_In_T()
        {
            int a = 10;
            int b = 20;

            Raptor.Utilities.MinMax.SetMinMax<int>(ref a, ref b);
            Assert.IsTrue(a == 10 && b == 20, "SetMinMax swapped values when it should not");

            int c = 20;
            int d = 10;

            Raptor.Utilities.MinMax.SetMinMax<int>(ref c, ref d);
            Assert.IsTrue(c == 10 && d == 20, "SetMinMax did not swap values when it should");
        }
    }
}
