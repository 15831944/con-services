﻿using System;
using VSS.TRex.Gateway.Common.Converters;

namespace VSS.TRex.Gateway.Tests.Controllers
{
  public class AutoMapperFixture : IDisposable
  {
    public AutoMapperFixture()
    {
      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();
    }

    public void Dispose()
    {
      // Cleanup here
    }
  }
}
