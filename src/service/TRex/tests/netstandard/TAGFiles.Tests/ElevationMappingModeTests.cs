﻿using FluentAssertions;
using VSS.TRex.Common.Types;
using VSS.TRex.Events;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace TAGFiles.Tests
{
  public class ElevationMappingModeTests : IClassFixture<DITagFileFixture>
  {
    [Theory]
    [InlineData("ElevationMappingMode-KettlewellDrive", "0247J009YU--TNZ 323F GS520--190115000234.tag", 1, ElevationMappingMode.LatestElevation)]
    [InlineData("ElevationMappingMode-KettlewellDrive", "0247J009YU--TNZ 323F GS520--190115001235.tag", 1, ElevationMappingMode.LatestElevation)]
    [InlineData("ElevationMappingMode-KettlewellDrive", "0247J009YU--TNZ 323F GS520--190115001735.tag", 1, ElevationMappingMode.LatestElevation)]
    [InlineData("ElevationMappingMode-KettlewellDrive", "0247J009YU--TNZ 323F GS520--190115002235.tag", 1, ElevationMappingMode.LatestElevation)]
    [InlineData("ElevationMappingMode-KettlewellDrive", "0247J009YU--TNZ 323F GS520--190115002735.tag", 1, ElevationMappingMode.LatestElevation)]
    [InlineData("ElevationMappingMode-KettlewellDrive", "0187J008YU--TNZ 323F GS520--190123002153.tag", 2, ElevationMappingMode.MinimumElevation)]
    [InlineData("ElevationMappingMode-KettlewellDrive", "0187J008YU--TNZ 323F GS520--190123002653.tag", 1, ElevationMappingMode.MinimumElevation)]
    public void ElevationMappingModeTests_Import_ElevationMappingMode(string folder, string fileName, int count, ElevationMappingMode state)
    {
      // Convert a TAG file using a TAGFileConverter into a mini-site model
      var converter = DITagFileFixture.ReadTAGFile(folder, fileName);

      // Check the list is as expected, has one element and extract it
      converter.MachinesTargetValueChangesAggregator[0].ElevationMappingModeStateEvents.EventListType.Should().Be(ProductionEventType.ElevationMappingModeStateChange);
      converter.MachinesTargetValueChangesAggregator[0].ElevationMappingModeStateEvents.Count().Should().Be(count);
      var eventDate = converter.MachinesTargetValueChangesAggregator[0].ElevationMappingModeStateEvents.LastStateDate();
      var eventValue = converter.MachinesTargetValueChangesAggregator[0].ElevationMappingModeStateEvents.LastStateValue();

      // Check date of event falls within the date range of the TAG file.
      eventDate.Should().BeOnOrAfter(converter.Processor.FirstDataTime);
      eventDate.Should().BeOnOrBefore(converter.Processor.DataTime);

      // These test files only contain latest elevation mapping modes.
      eventValue.Should().Be(state);
    }
  }
}
