﻿using System;
using VSS.TRex.Common.Models;
using VSS.TRex.Common.Types;
using VSS.TRex.Designs.Models;
using VSS.TRex.Filters;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.Tests.BinarizableSerialization.Analytics;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization
{
  public class ToFromBinary_SubGridRequestArgument : IClassFixture<AnalyticsTestsDIFixture>
  {
    [Fact]
    public void Test_SubGridsRequestArgument_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<SubGridsRequestArgument>("Empty SubGridsRequestArgument not same after round trip serialisation");
    }

    [Fact]
    public void Test_SubGridsRequestArgument()
    {
      var argument = new SubGridsRequestArgument()
      {
        TRexNodeID = "1",
        ProjectID = Guid.NewGuid(),
        Filters = new FilterSet(new CombinedFilter()),
        ReferenceDesign = new DesignOffset(Guid.NewGuid(), 1.5),
        RequestID = Guid.NewGuid(),
        GridDataType = GridDataType.CCV,
        ProdDataMaskBytes = new byte[]{ 1, 5, 3, 7 },
        SurveyedSurfaceOnlyMaskBytes = new byte[] { 0, 4, 1, 2 },
        MessageTopic = "Who cares",
        IncludeSurveyedSurfaceInformation = true,
        AreaControlSet = new AreaControlSet(true, 12345, 6789, 3344, 5566, 44)
      };

      SimpleBinarizableInstanceTester.TestClass(argument, "Custom SubGridsRequestArgument not same after round trip serialisation");
    }
  }
}
