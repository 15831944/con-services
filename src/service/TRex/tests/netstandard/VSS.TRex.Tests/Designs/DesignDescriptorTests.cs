﻿using System;
using System.IO;
using FluentAssertions;
using VSS.TRex.Designs.Models;
using VSS.TRex.Tests.BinarizableSerialization;
using VSS.TRex.Tests.BinaryReaderWriter;
using Xunit;

namespace VSS.TRex.Tests.Designs
{
  public class DesignDescriptorTests
  {
    [Fact]
    public void Creation()
    {
      var dd = new DesignDescriptor();

      dd.DesignID.Should().Be(Guid.Empty);
      dd.FileName.Should().BeNullOrEmpty();
      dd.Folder.Should().BeNullOrEmpty();
      dd.FullPath.Should().BeNullOrEmpty();
      dd.IsNull.Should().BeTrue();
    }

    [Fact()]
    public void Creation2()
    {
      Guid newGuid = Guid.NewGuid();

      var dd = new DesignDescriptor(newGuid, "folder", "file.name");

      dd.DesignID.Should().Be(newGuid);
      dd.FileName.Should().Be("file.name");
      dd.Folder.Should().Be("folder");
      dd.FullPath.Should().Be(Path.Combine("folder", "file.name"));
      dd.IsNull.Should().BeFalse();
    }

    [Fact]
    public void Init()
    {
      Guid newGuid = Guid.NewGuid();

      var dd = new DesignDescriptor(newGuid, "folder", "file.name");
      var dd2 = new DesignDescriptor();

      dd2.Init(newGuid, "folder", "file.name");

      dd.Should().BeEquivalentTo(dd2);
    }

    [Fact]
    public void Test_ToString()
    {
      var dd = new DesignDescriptor(Guid.NewGuid(), "folder", "filename");
      dd.ToString().Should().Match("*folder*filename*");
    }

    [Fact]
    public void Test_Equals()
    {
      Guid newGuid = Guid.NewGuid();
      var dd = new DesignDescriptor(newGuid, "folder", "filename");
      var dd2 = new DesignDescriptor(newGuid, "folder", "filename");
      var dd3 = new DesignDescriptor(newGuid, "bob", "filename");

      dd.Equals(dd2).Should().BeTrue();
      dd.Equals(dd3).Should().BeFalse();
    }

    [Fact]
    public void Clear()
    {
      var dd = new DesignDescriptor(Guid.Empty, "", "");
      var dd2 = new DesignDescriptor();
      dd2.Clear();

      dd.Should().BeEquivalentTo(dd2);
    }

    [Fact]
    public void Null()
    {
      DesignDescriptor.Null().Should().BeEquivalentTo(new DesignDescriptor());
    }

    /// <summary>
    /// Test reading and writing binary format
    /// </summary>
    [Fact]
    public void FromToBinary()
    {
      Guid newGuid = Guid.NewGuid();
      var dd = new DesignDescriptor(newGuid, "folder", "filename");

      TestBinarizable_ReaderWriterHelper.RoundTripSerialise(dd);
    }

    [Fact]
    public void BinaryReaderWriter()
    {
      Guid newGuid = Guid.NewGuid();
      var dd = new DesignDescriptor(newGuid, "folder", "filename");

      TestBinary_ReaderWriterHelper.RoundTripSerialise(dd);
    }
  }
}
