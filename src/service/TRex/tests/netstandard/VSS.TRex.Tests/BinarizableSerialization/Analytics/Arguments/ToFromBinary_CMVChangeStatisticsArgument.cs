﻿using System;
using VSS.TRex.Analytics.CMVChangeStatistics.GridFabric;
using VSS.TRex.Designs.Models;
using VSS.TRex.Filters;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Analytics.Arguments
{
  public class ToFromBinary_CMVChangeStatisticsArgument : IClassFixture<AnalyticsTestsDIFixture>
  {
    [Fact]
    public void Test_CMVChangeStatisticsArgument_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<CMVChangeStatisticsArgument>("Empty CMVChangeStatisticsArgument not same after round trip serialisation");
    }

    [Fact]
    public void Test_CMVChangeStatisticsArgument()
    {
      var argument = new CMVChangeStatisticsArgument
      {
        TRexNodeID = Guid.NewGuid(),
        ProjectID = Guid.NewGuid(),
        Filters = new FilterSet(new CombinedFilter()),
        ReferenceDesign = new DesignOffset(Guid.NewGuid(), 1.5),
        CMVChangeDetailsDataValues = new[] { -50.0, -20.0, -10.0, 0.0, 10.0, 20.0, 50.0 }
      };

      SimpleBinarizableInstanceTester.TestClass(argument, "Custom CMVChangeStatisticsArgument not same after round trip serialisation");
    }
  }
}
