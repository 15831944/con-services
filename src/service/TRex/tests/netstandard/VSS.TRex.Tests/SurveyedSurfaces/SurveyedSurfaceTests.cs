﻿using System;
using FluentAssertions;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;
using VSS.TRex.SurveyedSurfaces;
using VSS.TRex.Tests.BinaryReaderWriter;
using Xunit;

namespace VSS.TRex.Tests.SurveyedSurfaces
{
  public class SurveyedSurfaceTests
  {
    [Fact]
    public void Creation()
    {
      var ss = new TRex.SurveyedSurfaces.SurveyedSurface();
      ss.Should().NotBeNull();

      var ssID = Guid.NewGuid();
      var ddID = Guid.NewGuid();
      var date = DateTime.UtcNow;

      var ss2 = new TRex.SurveyedSurfaces.SurveyedSurface(
        ssID,
        new DesignDescriptor(ddID, "Folder", "FileName"),
        date,
        new BoundingWorldExtent3D(1, 2, 3, 4, 0, 10));

      ss2.Should().NotBeNull();
      ss2.ID.Should().NotBe(Guid.Empty);
      ss2.AsAtDate.Should().Be(date);
      ss2.DesignDescriptor.Should().BeEquivalentTo(new DesignDescriptor(ddID, "Folder", "FileName"));
      ss2.Extents.Should().BeEquivalentTo(new BoundingWorldExtent3D(1, 2, 3, 4, 0, 10));
    }

    [Fact]
    public void Clone()
    {
      var ssID = Guid.NewGuid();
      var ddID = Guid.NewGuid();
      var date = DateTime.UtcNow;

      var ss = new TRex.SurveyedSurfaces.SurveyedSurface(
        ssID,
        new DesignDescriptor(ddID, "Folder", "FileName"),
        date,
        new BoundingWorldExtent3D(1, 2, 3, 4, 0, 10));

      var ss2 = ss.Clone();
      ss.Should().BeEquivalentTo(ss2);
      ss.Equals(ss2).Should().BeTrue();
    }

    [Fact]
    public void Test_ToString()
    {
      var ss = new TRex.SurveyedSurfaces.SurveyedSurface();
      ss.ToString().Should().ContainAll(new[] {"ID:", "DesignID:"});
    }

    [Fact]
    public void ReadWriteBinary()
    {
      var ssID = Guid.NewGuid();
      var ddID = Guid.NewGuid();
      var date = DateTime.UtcNow;

      var ss = new TRex.SurveyedSurfaces.SurveyedSurface(
        ssID,
        new DesignDescriptor(ddID, "Folder", "FileName"),
        date,
        new BoundingWorldExtent3D(1, 2, 3, 4, 0, 10));

      TestBinary_ReaderWriterHelper.RoundTripSerialise(ss);
    }
  }
}
