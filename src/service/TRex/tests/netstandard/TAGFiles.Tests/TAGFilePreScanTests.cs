﻿using System.IO;
using FluentAssertions;
using VSS.TRex.Common;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.TAGFiles.Executors;
using VSS.TRex.TAGFiles.Types;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace TAGFiles.Tests
{
  public class TAGFilePreScanTests : IClassFixture<DITagFileFixture>
  {
    [Fact()]
    public void Test_TAGFilePreScan_Creation()
    {
      TAGFilePreScan preScan = new TAGFilePreScan();

      preScan.ReadResult.Should().Be(TAGReadResult.NoError);
      preScan.SeedLatitude.Should().BeNull();
      preScan.SeedLongitude.Should().BeNull();
      preScan.RadioType.Should().Be(string.Empty);
      preScan.RadioSerial.Should().Be(string.Empty);
      preScan.MachineType.Should().Be(CellPassConsts.MachineTypeNull);
      preScan.MachineID.Should().Be(string.Empty);
      preScan.HardwareID.Should().Be(string.Empty);
      preScan.SeedHeight.Should().BeNull();
      preScan.SeedTimeUTC.Should().BeNull();
      preScan.DesignName.Should().Be(string.Empty);
      preScan.ApplicationVersion.Should().Be(string.Empty);

    }

    [Fact()]
    public void Test_TAGFilePreScan_Execute()
    {
      TAGFilePreScan preScan = new TAGFilePreScan();

      Assert.True(preScan.Execute(new FileStream(Path.Combine("TestData", "TAGFiles", "TestTAGFile.tag"), FileMode.Open, FileAccess.Read)),
          "Pre-scan execute returned false");

      preScan.ProcessedEpochCount.Should().Be(1478);
      preScan.ReadResult.Should().Be(TAGReadResult.NoError);
      preScan.SeedLatitude.Should().Be(0.8551829920414814);
      preScan.SeedLongitude.Should().Be(-2.1377653549870974);
      preScan.SeedHeight.Should().Be(25.045071376845993);
      preScan.SeedTimeUTC.Should().Be(System.DateTime.Parse("2014-08-26T17:40:39.3550000", System.Globalization.NumberFormatInfo.InvariantInfo));
      preScan.RadioType.Should().Be("torch");
      preScan.RadioSerial.Should().Be("5411502448");
      preScan.MachineType.Should().Be(39);
      preScan.MachineID.Should().Be("CB54XW  JLM00885");
      preScan.HardwareID.Should().Be("0523J019SW");
      preScan.DesignName.Should().Be("CAT DAY 22");
      preScan.ApplicationVersion.Should().Be("12.61-75222");
    }

    [Fact()]
    public void Test_TAGFilePreScan_Execute_JapaneseDesign()
    {
      TAGFilePreScan preScan = new TAGFilePreScan();

      Assert.True(preScan.Execute(new FileStream(Path.Combine("TestData", "TAGFiles", "JapaneseDesignTagfileTest.tag"), FileMode.Open, FileAccess.Read)),
        "Pre-scan execute returned false");

      preScan.ProcessedEpochCount.Should().Be(1222);
      preScan.ReadResult.Should().Be(TAGReadResult.NoError);
      preScan.SeedLatitude.Should().Be(0.65955923731934751);
      preScan.SeedLongitude.Should().Be(2.45317108556434);
      preScan.SeedHeight.Should().Be(159.53982475668218);
      preScan.SeedTimeUTC.Should().Be(System.DateTime.Parse("2019-06-17T01:43:14.8640000", System.Globalization.NumberFormatInfo.InvariantInfo));
      preScan.RadioType.Should().Be("torch");
      preScan.RadioSerial.Should().Be("5750F00368");
      preScan.MachineType.Should().Be(25);
      preScan.MachineID.Should().Be("320E03243");
      preScan.HardwareID.Should().Be("3337J201SW");
      preScan.DesignName.Should().Be("所沢地区　NO.210-NO.255");
      preScan.ApplicationVersion.Should().Be("13.11-RC1");
    }

    [Fact()]
    public void Test_TAGFilePreScan_NEEposition()
    {
      //  Lat/Long refers to the GPS_BASE_Position
      //    therefore SeedLatitude and SeedLongitude == Consts.NullDouble 
      //  CCSSSCON-507 should resolve this to a valid SeedLocation

      var preScan = new TAGFilePreScan();

      Assert.True(preScan.Execute(new FileStream(Path.Combine("TestData", "TAGFiles", "Bug ccssscon-401 NEE SeedPosition.tag"), FileMode.Open, FileAccess.Read)),
        "Pre-scan execute returned false");

      preScan.ProcessedEpochCount.Should().Be(1);
      preScan.ReadResult.Should().Be(TAGReadResult.NoError);
      preScan.SeedLatitude.Should().Be(Consts.NullDouble);
      preScan.SeedLongitude.Should().Be(Consts.NullDouble);
      preScan.SeedHeight.Should().Be(Consts.NullDouble);
      preScan.SeedTimeUTC.Should().BeNull();
      preScan.RadioType.Should().Be("torch");
      preScan.RadioSerial.Should().Be("5850F00892");
      preScan.MachineType.Should().Be(MachineType.Excavator);
      preScan.MachineID.Should().Be("M316F PAK115");
      preScan.HardwareID.Should().Be("1639J101YU");
      preScan.DesignName.Should().Be("L03P");
      preScan.ApplicationVersion.Should().Be("EW-1.11.0-2019_3 672");
    }

  }
}
