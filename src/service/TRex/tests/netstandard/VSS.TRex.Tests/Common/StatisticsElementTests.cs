﻿using FluentAssertions;
using VSS.TRex.Common.RequestStatistics;
using Xunit;

namespace VSS.TRex.Tests.Common
{
  public class StatisticsElementTests
  {
    [Fact]
    public void Creation()
    {
      var _ = new StatisticsElement();
    }

    [Fact]
    public void Value()
    {
      var req = new StatisticsElement();

      req.Value.Should().Be(0);
      req.Increment();
      req.Value.Should().Be(1);
    }
  }
}
