﻿using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSS.ConfigurationStore;
using VSS.TRex.Cells;
using VSS.TRex.DI;
using VSS.TRex.Events;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Machines.Interfaces;
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage;
using VSS.TRex.Storage.Models;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests
{
  public class MutabilityConverterTests : IClassFixture<DILoggingFixture>
  {
    /// <summary>
    /// A handy test cell pass for the unit tests below to use
    /// </summary>
    /// <returns></returns>
    private CellPass TestCellPass()
    {
      return new CellPass()
      {
        Amplitude = 100,
        CCA = 101,
        CCV = 102,
        Frequency = 103,
        gpsMode = GPSMode.Fixed,
        HalfPass = false,
        Height = 104,
        //MachineID = 105,
        InternalSiteModelMachineIndex = 105,
        GPSModeStore = 106,
        MachineSpeed = 106,
        MaterialTemperature = 107,
        MDP = 108,
        PassType = PassType.Track,
        RadioLatency = 109,
        RMV = 110,
        Time = new DateTime(2000, 1, 2, 3, 4, 5)
      };
    }

    [Fact]
    public void Test_MutabilityConverterTests_NoValidSource()
    {
      var mutabilityConverter = new MutabilityConverter();
      MemoryStream immutableStream;
      Assert.Throws<TRexException>(() => mutabilityConverter.ConvertToImmutable(FileSystemStreamType.Events, null, null, out immutableStream));
    }

    [Fact]
    public void Test_MutabilityConverterTests_ConvertSubgridDirectoryTest()
    {
      // Create a subgrid directory with a single segment and some cells. Create a stream from it then use the
      // mutability converter to convert it to the immutable form. Read this back into an immutable representation
      // and compare the mutable and immutable versions for consistency.

      // Create a leaf to contain the mutable directory
      IServerLeafSubGrid mutableLeaf = new ServerSubGridTreeLeaf(null, null, SubGridTreeConsts.SubGridTreeLevels);
      mutableLeaf.Directory.GlobalLatestCells = SubGridCellLatestPassesDataWrapperFactory.Instance().NewWrapper(true, false);

      // Load the mutable stream of information
      mutableLeaf.Directory.CreateDefaultSegment();

      SubGridUtilities.SubGridDimensionalIterator((x, y) => { (mutableLeaf.Directory.GlobalLatestCells as SubGridCellLatestPassDataWrapper_NonStatic).PassData[x, y].Height = 1234.5678F; });

      // Take a copy of the mutable cells for later reference
      SubGridCellLatestPassDataWrapper_NonStatic mutableCells = (mutableLeaf.Directory.GlobalLatestCells as SubGridCellLatestPassDataWrapper_NonStatic);

      MemoryStream mutableStream = new MemoryStream();
      mutableLeaf.SaveDirectoryToStream(mutableStream);

      var mutabilityConverter = new MutabilityConverter();
      /* todo also test using the mutableStream */
      mutabilityConverter.ConvertToImmutable(FileSystemStreamType.SubGridDirectory, null, mutableLeaf, out MemoryStream immutableStream);

      IServerLeafSubGrid immutableLeaf = new ServerSubGridTreeLeaf(null, null, SubGridTreeConsts.SubGridTreeLevels);
      immutableLeaf.Directory.GlobalLatestCells = SubGridCellLatestPassesDataWrapperFactory.Instance().NewWrapper(false, true);

      immutableStream.Position = 0;
      immutableLeaf.LoadDirectoryFromStream(immutableStream);

      SubGridCellLatestPassDataWrapper_StaticCompressed immutableCells = (immutableLeaf.Directory.GlobalLatestCells as SubGridCellLatestPassDataWrapper_StaticCompressed);

      // Check height of the cells match to tolerance given the compressed lossiness.
      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        double mutableValue = mutableCells.PassData[x, y].Height;
        double immutableValue = immutableCells.ReadHeight(x, y);

        double diff = immutableValue - mutableValue;

        Assert.True(Math.Abs(diff) <= 0.001,
          $"Cell height at ({x}, {y}) has unexpected value: {immutableValue} vs {mutableValue}, diff = {diff}");
      });
    }

    [Fact]
    public void Test_MutabilityConverterTests_ConvertSubgridSegmentTest()
    {
      // Create a segment with some cell passes. Create a stream fron it then use the
      // mutability converter to convert it to the immutable form. Read this back into an immutable representation
      // and compare the mutable and immutable versions for consistency.

      // Create a mutable segment to contain the passes
      SubGridCellPassesDataSegment mutableSegment = new SubGridCellPassesDataSegment
      (SubGridCellLatestPassesDataWrapperFactory.Instance().NewWrapper(true, false),
        SubGridCellSegmentPassesDataWrapperFactory.Instance().NewWrapper(true, false));

      // Load the mutable stream of information
      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        (mutableSegment.LatestPasses as SubGridCellLatestPassDataWrapper_NonStatic).PassData[x, y].Height = 1234.5678F;

        // Add 5 passes to each cell
        for (int i = 0; i < 5; i++)
        {
          CellPass cellPass = TestCellPass();

          // Adjust the height/time so there is a range of values
          cellPass.Time = cellPass.Time.AddMinutes(i);
          cellPass.Height += i;

          mutableSegment.PassesData.AddPass(x, y, cellPass);
        }
      });

      mutableSegment.SegmentInfo = new SubGridCellPassesDataSegmentInfo(DateTime.MinValue, DateTime.MaxValue, null);

      // Take a copy of the mutable cells and cell passes for later reference
      SubGridCellLatestPassDataWrapper_NonStatic mutableLatest = (mutableSegment.LatestPasses as SubGridCellLatestPassDataWrapper_NonStatic);
      CellPass[,][] mutablePasses = mutableSegment.PassesData.GetState();

      MemoryStream mutableStream = new MemoryStream();
      using (var writer = new BinaryWriter(mutableStream, Encoding.UTF8, true))
      {
        mutableSegment.Write(writer);
      }

      // Convert the mutable data, using the sourceSegment, into the immutable form and reload it into an immutable segment
      /* todo also test using the mutableStream */
      var mutabilityConverter = new MutabilityConverter();
      mutabilityConverter.ConvertToImmutable(FileSystemStreamType.SubGridSegment, null, mutableSegment, out MemoryStream immutableStream);

      SubGridCellPassesDataSegment immutableSegment = new SubGridCellPassesDataSegment
      (SubGridCellLatestPassesDataWrapperFactory.Instance().NewWrapper(false, true),
        SubGridCellSegmentPassesDataWrapperFactory.Instance().NewWrapper(false, true));

      immutableStream.Position = 0;

      using (var reader = new BinaryReader(immutableStream, Encoding.UTF32, true))
      {
        immutableSegment.Read(reader, true, true);
      }

      SubGridCellLatestPassDataWrapper_StaticCompressed immutableLatest = (immutableSegment.LatestPasses as SubGridCellLatestPassDataWrapper_StaticCompressed);
      ISubGridCellSegmentPassesDataWrapper immutablePasses = immutableSegment.PassesData;

      // Check height of the latest cells match to tolerance given the compressed lossiness.
      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        double mutableValue = mutableLatest.PassData[x, y].Height;
        double immutableValue = immutableLatest.ReadHeight(x, y);

        double diff = immutableValue - mutableValue;

        Assert.True(Math.Abs(diff) <= 0.001, $"Cell height at ({x}, {y}) has unexpected value: {immutableValue} vs {mutableValue}, diff = {diff}");
      });

      // Check the heights specially to account for tolerance differences
      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        for (uint i = 0; i < immutableSegment.PassesData.PassCount(x, y); i++)
        {
          double mutableValue = mutableSegment.PassesData.PassHeight(x, y, i);
          double immutableValue = immutableSegment.PassesData.PassHeight(x, y, i);

          double diff = immutableValue - mutableValue;

          Assert.True(Math.Abs(diff) <= 0.001, $"Cell height at ({x}, {y}) has unexpected value: {immutableValue} vs {mutableValue}, diff = {diff}");
        }
      });

      // Check the times specially to account for tolerance differences
      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        for (uint i = 0; i < immutableSegment.PassesData.PassCount(x, y); i++)
        {

          DateTime mutableValue = mutableSegment.PassesData.PassTime(x, y, i);
          DateTime immutableValue = immutableSegment.PassesData.PassTime(x, y, i);

          TimeSpan diff = mutableValue - immutableValue;

          Assert.True(diff.Duration() <= TimeSpan.FromSeconds(1), $"Cell time at ({x}, {y}) has unexpected value: {immutableValue} vs {mutableValue}, diff = {diff}");
        }
      });

      // Check that the cell passes in the cell pass stacks for the segment match to tolerance given the compressed lossiness
      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        for (uint i = 0; i < immutableSegment.PassesData.PassCount(x, y); i++)
        {
          CellPass cellPass = immutableSegment.PassesData.ExtractCellPass(x, y, i);

          // Force the height and time in the immutable record to be the same as the immutable record
          // as they have been independently checked above. Also set the machine ID to be the same as the mutable
          // machine ID as the immutable representation does not include it in the Ignite POC
          cellPass.Time = mutablePasses[x, y][i].Time;
          cellPass.Height = mutablePasses[x, y][i].Height;
          //cellPass.MachineID = mutablePasses[x, y][i].MachineID;
          cellPass.InternalSiteModelMachineIndex = mutablePasses[x, y][i].InternalSiteModelMachineIndex;

          CellPass mutableCellPass = mutablePasses[x, y][i];
          Assert.True(mutableCellPass.Equals(cellPass), $"Cell passes not equal at Cell[{x}, {y}], cell pass index {i}");
        }
      });
    }

    [Fact]
    public void Test_MutabilityConverterTests_ConvertEventListTest()
    {
      DIBuilder
        .New()
        .AddLogging()
        .Add(x => x.AddSingleton<IConfigurationStore, GenericConfiguration>())
        .Add(VSS.TRex.Storage.Utilities.DIUtilities.AddProxyCacheFactoriesToDI)
        .Build();

      var storageProxy = new StorageProxy_Ignite_Transactional(StorageMutability.Mutable);
      storageProxy.SetImmutableStorageProxy(new StorageProxy_Ignite_Transactional(StorageMutability.Immutable));

      var moqSiteModels = new Mock<ISiteModels>();
      moqSiteModels.Setup(mk => mk.StorageProxy).Returns(storageProxy);

      DIBuilder
        .Continue()
        .Add(x => x.AddSingleton<IProductionEventsFactory>(new ProductionEventsFactory()))
        .Add(x => x.AddSingleton<ISiteModels>(moqSiteModels.Object))
        .Complete();

      var siteModel = new SiteModel(Guid.Empty, false);
      var events = new ProductionEventLists(siteModel, MachineConsts.kNullInternalSiteModelMachineIndex);

      DateTime ReferenceTime = DateTime.UtcNow;
      events.MachineDesignNameIDStateEvents.PutValueAtDate(ReferenceTime.AddMinutes(-60), 0);
      events.MachineDesignNameIDStateEvents.PutValueAtDate(ReferenceTime.AddMinutes(-30), 1);
      events.MachineDesignNameIDStateEvents.PutValueAtDate(ReferenceTime.AddMinutes(-29), 1);
      events.MachineDesignNameIDStateEvents.PutValueAtDate(ReferenceTime.AddMinutes(-29), 2);
      events.MachineDesignNameIDStateEvents.PutValueAtDate(ReferenceTime.AddMinutes(-29), 3);
      Assert.True(3 == events.MachineDesignNameIDStateEvents.Count(), $"List contains {events.MachineDesignNameIDStateEvents.Count()} MachineDesignName events, instead of 3");

      var mutableStream = events.MachineDesignNameIDStateEvents.GetMutableStream();
      var targetEventList = Deserialize(mutableStream);
      Assert.Equal(3, targetEventList.Count());

      var mutabilityConverter = new MutabilityConverter();
      mutabilityConverter.ConvertToImmutable(FileSystemStreamType.Events, null, events.MachineDesignNameIDStateEvents, out MemoryStream immutableStream);

      targetEventList = Deserialize(immutableStream);
      Assert.Equal(2, targetEventList.Count());

      mutabilityConverter.ConvertToImmutable(FileSystemStreamType.Events, mutableStream, null, out immutableStream);
      targetEventList = Deserialize(immutableStream);
      Assert.Equal(2, targetEventList.Count());
    }

    private IProductionEvents Deserialize(MemoryStream stream)
    {
      IProductionEvents events = null;
      using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
      {
        if (stream.Length < 16)
        {
          return events;
        }

        stream.Position = 8;

        var eventType = reader.ReadInt32();
        if (!Enum.IsDefined(typeof(ProductionEventType), eventType))
        {
          return events;
        }

        events = DIContext.Obtain<IProductionEventsFactory>().NewEventList(-1, Guid.Empty, (ProductionEventType) eventType);

        stream.Position = 0;
        events.ReadEvents(reader);
      }

      return events;
    }
  }
}
