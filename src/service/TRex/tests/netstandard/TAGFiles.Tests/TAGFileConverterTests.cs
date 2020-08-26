﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreX.Interfaces;
using CoreX.Wrapper;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Nito.AsyncEx;
using VSS.TRex.DI;
using VSS.TRex.Geometry;
using VSS.TRex.TAGFiles.Executors;
using VSS.TRex.TAGFiles.Models;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace TAGFiles.Tests
{
  public class TAGFileConverterTests : IClassFixture<DITagFileFixture>
  {
    /// <summary>
    ///  The real deal test setup for ACS conversion
    /// </summary>
    private void InjectACSDependencies()
    {
      DIBuilder
        .Continue()
        .Add(x => x.AddSingleton<IACSTranslator, ACSTranslator>())
        .Add(x => x.AddSingleton<IConvertCoordinates, ConvertCoordinates>())
        .Complete();
    }


    [Fact()]
    public void Test_ACS_Coordinate_Conversion_Mock()
    {

      var converter = DITagFileFixture.ReadTAGFile("TestTAGFile.tag", Guid.NewGuid(), false);
      Assert.True(converter.IsUTMCoordinateSystem, "Tagfile should be ACS coordinate system");
      converter.Processor.ConvertedBladePositions.Should().HaveCount(1478);
      converter.Processor.ConvertedRearAxlePositions.Should().HaveCount(1478);
      converter.Processor.ConvertedTrackPositions.Should().HaveCount(0);
      converter.Processor.ConvertedWheelPositions.Should().HaveCount(0);
      Assert.True(converter.ReadResult == TAGReadResult.NoError, $"converter.ReadResult == TAGReadResult.NoError [= {converter.ReadResult}");
      Assert.True(converter.ProcessedCellPassCount == 16525,$"converter.ProcessedCellPassCount != 16525 [={converter.ProcessedCellPassCount}]");
      Assert.True(converter.ProcessedEpochCount == 1478, $"converter.ProcessedEpochCount != 1478, [= {converter.ProcessedEpochCount}]");
    }


    [Fact()]
    public void Test_ACS_Coordinate_Conversion()
    {
      InjectACSDependencies();

      var converter = DITagFileFixture.ReadTAGFile("TestTAGFile.tag", Guid.NewGuid(), false);
      Assert.True(converter.IsUTMCoordinateSystem, "Tagfile should be ACS coordinate system");
      converter.Processor.ConvertedBladePositions.Should().HaveCount(1478);
      converter.Processor.ConvertedRearAxlePositions.Should().HaveCount(1478);
      converter.Processor.ConvertedTrackPositions.Should().HaveCount(0);
      converter.Processor.ConvertedWheelPositions.Should().HaveCount(0);
      Assert.True(converter.ReadResult == TAGReadResult.NoError, $"converter.ReadResult == TAGReadResult.NoError [= {converter.ReadResult}");
      Assert.True(converter.ProcessedCellPassCount == 16525, $"converter.ProcessedCellPassCount != 16525 [={converter.ProcessedCellPassCount}]");
      Assert.True(converter.ProcessedEpochCount == 1478, $"converter.ProcessedEpochCount != 1478, [= {converter.ProcessedEpochCount}]");
    }

    [Fact()]
    public void Test_Not_ACS_Coordinate_Conversion()
    {
      var converter = DITagFileFixture.ReadTAGFile("TestTAGFile-CMV-1.tag", Guid.NewGuid(), false);
      Assert.False(converter.IsUTMCoordinateSystem, "Tagfile should not be ACS coordinate system");
      converter.Processor.ConvertedBladePositions.Should().HaveCount(0);
      converter.Processor.ConvertedRearAxlePositions.Should().HaveCount(0);
      converter.Processor.ConvertedTrackPositions.Should().HaveCount(0);
      converter.Processor.ConvertedWheelPositions.Should().HaveCount(0);
      Assert.True(converter.ReadResult == TAGReadResult.NoError, $"converter.ReadResult == TAGReadResult.NoError [= {converter.ReadResult}");
      Assert.True(converter.ProcessedCellPassCount == 2810, $"converter.ProcessedCellPassCount != 2810 [={converter.ProcessedCellPassCount}]");
      Assert.True(converter.ProcessedEpochCount == 1428, $"converter.ProcessedEpochCount != 1428, [= {converter.ProcessedEpochCount}]");
    }

    [Fact()]
    public void Test_TAGFileConverter_Creation()
    {
      var converter = new TAGFileConverter();

      Assert.True(converter.Machines != null &&
                  converter.SiteModel != null &&
                  converter.SiteModelGridAggregator != null &&
                  converter.MachinesTargetValueChangesAggregator != null &&
                  converter.ReadResult == TAGReadResult.NoError &&
                  converter.ProcessedCellPassCount == 0 &&
                  converter.ProcessedEpochCount == 0,
        "TAGFileConverter not created as expected");
    }

    [Fact()]
    public void Test_TAGFileConverter_Execute_SingleFileOnce()
    {
      var converter = DITagFileFixture.ReadTAGFile("TestTAGFile.tag", Guid.NewGuid(), false);

      Assert.True(converter.Machines != null, "converter.Machines == null");
      Assert.True(converter.MachinesTargetValueChangesAggregator[0] != null,
        "converter.MachineTargetValueChangesAggregator");
      Assert.True(converter.ReadResult == TAGReadResult.NoError,
        $"converter.ReadResult == TAGReadResult.NoError [= {converter.ReadResult}");
      Assert.True(converter.ProcessedCellPassCount == 16525,
        $"converter.ProcessedCellPassCount != 16525 [={converter.ProcessedCellPassCount}]");
      Assert.True(converter.ProcessedEpochCount == 1478, $"converter.ProcessedEpochCount != 1478, [= {converter.ProcessedEpochCount}]");
      Assert.True(converter.SiteModelGridAggregator != null, "converter.SiteModelGridAggregator == null");
    }

    [Fact()]
    public void Test_TAGFileConverter_Execute_SingleFileTwice()
    {
      var newMachineId = Guid.NewGuid();

      var converter1 = DITagFileFixture.ReadTAGFile("TestTAGFile.tag", newMachineId, false);
      var converter2 = DITagFileFixture.ReadTAGFile("TestTAGFile.tag", newMachineId, false);

      converter1.ReadResult.Should().Be(TAGReadResult.NoError);
      converter2.ReadResult.Should().Be(TAGReadResult.NoError);

      converter1.ProcessedCellPassCount.Should().Be(converter2.ProcessedCellPassCount);

      converter1.ProcessedCellPassCount.Should().Be(converter2.ProcessedCellPassCount);
      converter1.ProcessedEpochCount.Should().Be(converter2.ProcessedEpochCount);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task Test_TAGFileConverter_Execute_SingleFileMultipleTimesConcurrently(int instanceCount)
    {

      var result = await Enumerable.Range(1, instanceCount).Select(x => Task.Run(() => DITagFileFixture.ReadTAGFile("TestTAGFile.tag", Guid.NewGuid(), false))).WhenAll();

      result.Length.Should().Be(instanceCount);

      result.All(x => x.ProcessedCellPassCount == 16525).Should().Be(true);
      result.All(x => x.ProcessedEpochCount == 1478).Should().Be(true);
      result.All(x => x.SiteModelGridAggregator.CountLeafSubGridsInMemory() == 12).Should().Be(true);
    }

    [Fact]
    public void Test_TAGFileConverter_OnGroundState()
    {
      var converter = DITagFileFixture.ReadTAGFile("Dimensions2018-CaseMachine", "2652J085SW--CASE CX160C--121101215100.tag");
      converter.Processor.OnGroundFlagSet.Should().Be(true);

      var theTime = new DateTime(2012, 11, 1, 20, 53, 23, 841, DateTimeKind.Utc);
      converter.Processor.OnGrounds.GetValueAtDateTime(theTime, OnGroundState.No).Should().Be(OnGroundState.YesMachineSoftware);
    }

    /// <summary>
    /// This test ensures a TAG file that contains epochs with Valid_Positions set to No, does not produce a 'Sink Finishing Failure'
    /// </summary>
    [Fact()]
    public void Test_TAGFileConverter_Execute_NotSinkError()
    {
      var converter = DITagFileFixture.ReadTAGFile("DimensionsNotSinkError.tag", Guid.NewGuid(), false);
      converter.ReadResult.Should().Be(TAGReadResult.NoError);
      converter.ProcessedCellPassCount.Should().Be(52);
      converter.ProcessedEpochCount.Should().Be(65);
    }
  }
}
