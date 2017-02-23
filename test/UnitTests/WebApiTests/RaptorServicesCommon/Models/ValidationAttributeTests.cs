﻿using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Raptor.Service.Common.Filters.Validation;
using VSS.Raptor.Service.Common.Models;

namespace VSS.Raptor.Service.WebApiTests.Common.Modelss
{
    [TestClass()]
    public class ValidationAttributeTests
    {
        [TestMethod()]
        public void DecimalIsWithinRangeAttributeTest()
        {
            DecimalIsWithinRangeAttribute attribute = new DecimalIsWithinRangeAttribute(-10, 10);
            Assert.IsTrue(attribute.IsValid(2));
            Assert.IsFalse(attribute.IsValid(20));
        }

        [TestMethod()]
        public void IntIsWithinRangeAttributeTest()
        {
            IntIsWithinRangeAttribute attribute = new IntIsWithinRangeAttribute(-10, 10);
            Assert.IsTrue(attribute.IsValid(2));
            Assert.IsFalse(attribute.IsValid(20));
        }

        [TestMethod()]
        public void MoreThanTwoPointsAttributeTest()
        {
            MoreThanTwoPointsAttribute attribute = new MoreThanTwoPointsAttribute();
            List<WGSPoint> list = new List<WGSPoint>();
            for (int i=0;i<5;i++)
                list.Add(WGSPoint.CreatePoint(3,3));
            List<WGSPoint> list2 = new List<WGSPoint>();
            for (int i = 0; i < 55; i++)
                list2.Add(WGSPoint.CreatePoint(3, 3));
            List<WGSPoint> list3 = new List<WGSPoint>();
            for (int i = 0; i < 1; i++)
                list3.Add(WGSPoint.CreatePoint(3, 3));

            Assert.IsTrue(attribute.IsValid(list.ToArray()));
            Assert.IsFalse(attribute.IsValid(list2.ToArray()));
            Assert.IsFalse(attribute.IsValid(list3.ToArray()));

        }

        [TestMethod()]
        public void ValidFilenameAttributeTest()
        {
            ValidFilenameAttribute attribute = new ValidFilenameAttribute(16);
            const string validFileName = "c:\\test\\test.txt";
            const string invalidFileName = "c:\\te%@$#s><t**est.>txt";
            const string longinvalidFileName = "c:\\te%@$#gfdsgfdhytueytjuegrhrthjetrgshstest.>txt";
            Assert.IsTrue(attribute.IsValid(validFileName));
            Assert.IsFalse(attribute.IsValid(invalidFileName));
            Assert.IsFalse(attribute.IsValid(longinvalidFileName));

        }
    }
}
