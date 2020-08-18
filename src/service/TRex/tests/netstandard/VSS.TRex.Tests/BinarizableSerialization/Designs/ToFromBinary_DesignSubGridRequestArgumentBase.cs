﻿using System;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.Models;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Designs
{
  public class ToFromBinary_DesignSubGridRequestArgumentBase
  {
    [Fact]
    public void Test_DesignSubGridRequestArgumentBase_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<DesignSubGridRequestArgumentBase>("Empty DesignSubGridRequestArgumentBase not same after round trip serialisation");
    }

    [Fact]
    public void Test_DesignSubGridRequestArgumentBase_SubgridDetail()
    {
      var argument = new DesignSubGridRequestArgumentBase
      {
        ProjectID = Guid.NewGuid(),
        ReferenceDesign = new DesignOffset(Guid.NewGuid(), 123.4),
        TRexNodeID = Guid.NewGuid(),
        Filters = null
      };

      SimpleBinarizableInstanceTester.TestClass(argument, "Empty DesignSubGridRequestArgumentBase not same after round trip serialisation");
    }
  }
}
