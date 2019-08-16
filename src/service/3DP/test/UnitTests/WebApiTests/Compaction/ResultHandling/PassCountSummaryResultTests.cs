﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.Productivity3D.Models.Compaction.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Report.ResultHandling;

namespace VSS.Productivity3D.WebApiTests.Compaction.ResultHandling
{
  [TestClass]
  public class PassCountSummaryResultTests
  {
    [TestMethod]
    [DataRow(0, false)]
    [DataRow(0.0009, false)]
    [DataRow(0.001, false)]
    [DataRow(0.1, true)]
    [DataRow(10.234, true)]
    public void HasData_Should_return_expected_result_From_coverage_value(double totalAreaCovered, bool expectedResult)
    {
      var obj = new PassCountSummaryResult(null, false, 0, 0, 0, 0, totalAreaCovered);

      Assert.AreEqual(expectedResult, obj.HasData());
    }
  }
}
