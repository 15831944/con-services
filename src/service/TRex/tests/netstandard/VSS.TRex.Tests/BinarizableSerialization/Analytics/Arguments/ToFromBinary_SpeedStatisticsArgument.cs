﻿using System;
using VSS.TRex.Analytics.SpeedStatistics.GridFabric;
using VSS.TRex.Common.Models;
using VSS.TRex.Common.Records;
using VSS.TRex.Designs.Models;
using VSS.TRex.Filters;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Analytics.Arguments
{
  public class ToFromBinary_SpeedStatisticsArgument : IClassFixture<AnalyticsTestsDIFixture>
  {
    [Fact]
    public void Test_SpeedStatisticsArgument_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<SpeedStatisticsArgument>("Empty SpeedStatisticsArgument not same after round trip serialisation");
    }

    [Fact]
    public void Test_SpeedStatisticsArgument()
    {
      var argument = new SpeedStatisticsArgument()
      {
        TRexNodeID = Guid.NewGuid(),
        ProjectID = Guid.NewGuid(),
        Filters = new FilterSet(new CombinedFilter()),
        ReferenceDesign = new DesignOffset(Guid.NewGuid(), 1.5),
        Overrides = new OverrideParameters
        { TargetMachineSpeed = new MachineSpeedExtendedRecord(5, 50) }
      };

      SimpleBinarizableInstanceTester.TestClass(argument, "Custom SpeedStatisticsArgument not same after round trip serialisation");
    }
  }
}
