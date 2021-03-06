﻿using System.ComponentModel;
using CoreX.Interfaces;
using CoreX.Types;
using CoreX.Wrapper.UnitTests.Types;
using CoreXModels;
using FluentAssertions;
using Xunit;

namespace CoreX.Wrapper.UnitTests.Tests
{
  public class CoreXWrapperTests : IClassFixture<UnitTestBaseFixture>
  {
    private readonly ICoreXWrapper _convertCoordinates;

    public CoreXWrapperTests(UnitTestBaseFixture testFixture)
    {
      _convertCoordinates = testFixture.CoreXWrapper;
    }

    [Fact]
    public void Should_throw_when_loading_null_DC_content()
    {
      var exObj = Record.Exception(() => _convertCoordinates.GetCSIBFromDCFileContent(string.Empty));

      exObj.Message.Should().Be("GetCSIBFromDCFileContent: Get CSIB from file content failed, error cecInvalidLengthPassed");
    }

    [Fact]
    public void Should_throw_When_DC_file_hasnt_been_encoded_correctly()
    {
      var base64EncodedDCFileWithNoLineEndings = "MDBOTVNDIFYxMC03MCAgICAgICAgICAgMjktMDUtMjAxOSAwNDowNTIzMTExMTEwTk1Kb2IgTmFtZSAgICAgICAgMTIxMTExNzhOTTExMTNOTUNvcmVYIDIuMDY1S0kyMDkyNTYwNC40NzQxOTEzMjk4LjI1NzIyMjkzMjg5NUQ1S0kwLjAwMDAwMDAwMDAwMDAwMC4wMDAwMDAwMDAwMDAwMDAuMDAwMDAwMDAwMDAwMDAxLjAwMDAwMDAwMDAwMDAwMC4wMDAwMDAwMDAwMDAwMDAuMDAwMDAwMDAwMDAwMDA2NEtJMzM2LjIwNjU1NTM3MTAwMDAgLSAxMTUuMDI2MjY3ODE4MDAwLjAwMDAwMDAwMDAwMDAwMzY3My43MDgwMDAwMDM3MzcxOTguMDgxMDAwMDA3MzEwLjAwMDAwMDAwMDAwMDAwMS4wMDAwODY3MjMwMDAwMDAuMDAwMDAwMDAwMDAwMDAwLjAwMDAwMDAwMDAwMDAwNDlLSTMyMDkyNTYwNC40NzQxNjY3Mjk4LjI1NzIyMzU2MDAwMDAuMDAwMDAwMDAwMDAwMDAwLjAwMDAwMDAwMDAwMDAwMC4wMDAwMDAwMDAwMDAwMDAuMDAwMDAwMDAwMDAwMDAwLjAwMDAwMDAwMDAwMDAwMC4wMDAwMDAwMDAwMDAwMDAuMDAwMDAwMDAwMDAwMDA1MEtJMTE5OC43OTczMzkwOTE5MDI0OTEuNDgyODkxNTQ0MzEwLjAwMjY2MDkwOTMyMTgyMC4wMDAxMzcxNjAyNzQzMjAuMDAwMDMzMzAwODI5NzYxLjAwMDAxMzAxMzAwMDAwODFLSTEwLjAwMDAwMDAwMDAwMDAwMC4wMDAwMDAwMDAwMDAwMDAuMDAwMDAwMDAwMDAwMDAwLjAwMDAwMDAwMDAwMDAwMC4wMDAwMDAwMDAwMDAwMEM4S0k0UHJvamVjdGlvbiBmcm9tIERhdGEgQ29sbGVjdG9yICBab25lIGZyb20gRGF0YSBDb2xsZWN0b3IoV0dTIDg0KQ==";

      var ex = Record.Exception(() => _convertCoordinates.GetCSIBFromDCFileContent(base64EncodedDCFileWithNoLineEndings));

      ex.Message.Should().StartWith("GetCSIBFromDCFileContent: Get CSIB from file content failed, error cecDCParseLineNotEndingWithLF");
    }

    [Fact]
    [Description("Tests that a CS file read from disk can be passed to CoreX to return a valid CSIB")]
    public void Should_return_expected_CSIB_string()
    {
      var csibStr = _convertCoordinates.DCFileToCSIB(DCFile.GetFilePath(DCFile.DIMENSIONS_2012_DC_FILE_WITH_VERT_ADJUST));

      // Due to encoding differences between Windows and Linux it's not possible to expect the two to encode to the same result.
      // We only check content here; validity is exercised in the base fixture where it loads a DC file from disk to get the CSIB.
      csibStr.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Description("Tests that a CS file base64 encoded can be passed to CoreX to return a valid CSIB")]
    public void Should_decode_base64_DC_file_to_expected_CSIB()
    {
      var csibStr = _convertCoordinates.GetCSIBFromDCFileContent(TestConsts.DIMENSIONS_2012_DC_FILE_WITH_VERT_ADJUST);

      // Due to encoding differences between Windows and Linux it's not possible to expect the two to encode to the same result.
      // We only check content here; validity is exercised in the base fixture where it loads a DC file from disk to get the CSIB.
      csibStr.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Should_throw_when_CSIB_is_invalid()
    {
      var ex = Record.Exception(() => _convertCoordinates.LLHToNEE(TestConsts.DIMENSIONS_2012_DC_COORDINATE_SYSTEM_ID, new LLH(), InputAs.Degrees));

      ex.Message.Should().Be("Error 'gecCSIB_INVALID_CSIB' attempting to create GeodeticX transformer");
    }

    [Fact]
    public void CoreX_GeodeticDatabasePath_Should_not_be_null() => _convertCoordinates.GeodeticDatabasePath.Should().NotBeNullOrEmpty();
  }
}
