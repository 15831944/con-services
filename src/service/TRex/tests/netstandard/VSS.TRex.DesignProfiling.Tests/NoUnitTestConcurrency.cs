﻿using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace VSS.TRex.DesignProfiling.Tests
{
  public class NoConcurrency
  {
    [Fact]
    public void DummyTestForForceNoTestConcurrency()
    {
      Assert.True(true);
    }
  }
}
