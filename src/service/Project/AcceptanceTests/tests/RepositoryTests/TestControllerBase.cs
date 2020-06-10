﻿using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using VSS.Common.Abstractions.Configuration;
using VSS.ConfigurationStore;
using VSS.Serilog.Extensions;
using Xunit;

namespace RepositoryTests
{
  public class TestControllerBase
  {
    protected IServiceProvider ServiceProvider;
    protected ILoggerFactory loggerFactory;
    protected IConfigurationStore configStore;


    protected string TargetJsonString = @"<ProjectSettings>  
        < CompactionSettings >
        < OverrideTargetCMV > false </ OverrideTargetCMV >
        < OverrideTargetCMVValue > 50 </ OverrideTargetCMVValue >
        < MinTargetCMVPercent > 80 </ MinTargetCMVPercent >
        < MaxTargetCMVPercent > 130 </ MaxTargetCMVPercent >
        < OverrideTargetPassCount > false </ OverrideTargetPassCount >
        < OverrideTargetPassCountValue > 5 </ OverrideTargetPassCountValue >
        < OverrideTargetLiftThickness > false </ OverrideTargetLiftThickness >
        < OverrideTargetLiftThicknessMeters > 0.5 </ OverrideTargetLiftThicknessMeters >
        < CompactedLiftThickness > true </ CompactedLiftThickness >
        < ShowCCVSummaryTopLayerOnly > true </ ShowCCVSummaryTopLayerOnly >
        < FirstPassThickness > 0 </ FirstPassThickness >
        < OverrideTemperatureRange > false </ OverrideTemperatureRange >
        < MinTemperatureRange > 65 </ MinTemperatureRange >
        < MaxTemperatureRange > 175 </ MaxTemperatureRange >
        < OverrideTargetMDP > false </ OverrideTargetMDP >
        < OverrideTargetMDPValue > 50 </ OverrideTargetMDPValue >
        < MinTargetMDPPercent > 80 </ MinTargetMDPPercent >
        < MaxTargetMDPPercent > 130 </ MaxTargetMDPPercent >
        < ShowMDPSummaryTopLayerOnly > true </ ShowMDPSummaryTopLayerOnly >
        </ CompactionSettings >
        < VolumeSettings >
        < ApplyShrinkageAndBulking > false </ ApplyShrinkageAndBulking >
        < PercentShrinkage > 0 </ PercentShrinkage >
        < PercentBulking > 0 </ PercentBulking >
        < NoChangeTolerance > 0.02 </ NoChangeTolerance >
        </ VolumeSettings >
        < ExpiryPromptDismissed > false </ ExpiryPromptDismissed >
        </ ProjectSettings > ";

    public void SetupLogging()
    {
      ServiceProvider = new ServiceCollection()
                        .AddLogging()
                        .AddSingleton(new LoggerFactory().AddSerilog(SerilogExtensions.Configure("MasterDataConsumerTests.log")))
                        .AddSingleton<IConfigurationStore, GenericConfiguration>()
                        .BuildServiceProvider();

      loggerFactory = ServiceProvider.GetService<ILoggerFactory>();
      configStore = ServiceProvider.GetService<IConfigurationStore>();
      Assert.NotNull(ServiceProvider.GetService<ILoggerFactory>());
    }
  }
}
