﻿using System;
using System.Linq;
using FluentAssertions;
using VSS.TRex.Cells;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Common.Utilities.ExtensionMethods;
using VSS.TRex.Designs.Models;
using VSS.TRex.Events;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.SiteModels
{
  public class SiteModelTests : IClassFixture<DITAGFileAndSubGridRequestsFixture>
  {
    [Fact]
    public void Test_SiteModel_Creation_Default()
    {
      var siteModel = new SiteModel(TRex.Storage.Models.StorageMutability.Immutable);
      siteModel.GridLoaded.Should().Be(false);

      siteModel.CreationDate.Should().BeAfter(DateTime.UtcNow.AddSeconds(-1));
      siteModel.LastModifiedDate.Should().Be(siteModel.CreationDate);

      siteModel.ID.Should().Be(Guid.Empty);
      siteModel.IsTransient.Should().Be(true);
    }

    [Fact]
    public void Test_SiteModel_Creation_GuidOnly_Transient()
    {
      var guid = Guid.NewGuid();
      var siteModel = new SiteModel(guid, TRex.Storage.Models.StorageMutability.Immutable);

      siteModel.ID.Should().Be(guid);
      siteModel.IsTransient.Should().Be(true);
    }

    [Fact]
    public void Test_SiteModel_Creation_GuidOnly_NonTransient()
    {
      var guid = Guid.NewGuid();
      var siteModel = new SiteModel(guid, TRex.Storage.Models.StorageMutability.Immutable, false);

      siteModel.ID.Should().Be(guid);
      siteModel.IsTransient.Should().Be(false);
    }

    [Fact]
    public void Test_SiteModel_Creation_GuidAndCellSize()
    {
      var guid = Guid.NewGuid();
      var siteModel = new SiteModel(guid, TRex.Storage.Models.StorageMutability.Immutable, 1.23);

      siteModel.ID.Should().Be(guid);
      siteModel.CellSize.Should().Be(1.23);
      siteModel.Grid.CellSize.Should().Be(1.23);
      siteModel.IsTransient.Should().Be(true);
    }

    [Fact]
    public void Test_SiteModel_Creation_WithTransientOriginModel()
    {
      var guid = Guid.NewGuid();
      var originSiteModel = new SiteModel(guid, TRex.Storage.Models.StorageMutability.Immutable);

      Action act = () =>
      {
        var _ = new SiteModel(originSiteModel, SiteModelOriginConstructionFlags.PreserveNothing);
      };

      act.Should().Throw<TRexSiteModelException>().WithMessage("Cannot use a transient site model as an origin for constructing a new site model");
    }

    [Fact]
    public void Test_SiteModel_Creation_WithNonTransientOriginModel_NothingPreserved()
    {
      var guid = Guid.NewGuid();
      var originSiteModel = new SiteModel(guid, TRex.Storage.Models.StorageMutability.Immutable, false);
      var originModelModifiedDate = originSiteModel.LastModifiedDate;
      var originModelCreationDate = originSiteModel.CreationDate;

      var newSiteModel = new SiteModel(originSiteModel, SiteModelOriginConstructionFlags.PreserveNothing);

      newSiteModel.Should().NotBe(originSiteModel);
      newSiteModel.ID.Should().Be(guid);
      newSiteModel.IsTransient.Should().Be(false);
      newSiteModel.LastModifiedDate.Should().Be(originModelModifiedDate);
      newSiteModel.CreationDate.Should().Be(originModelCreationDate);
      newSiteModel.CellSize.Should().Be(originSiteModel.Grid.CellSize);

      newSiteModel.Grid.Should().NotBe(originSiteModel.Grid);
      newSiteModel.Grid.CellSize.Should().Be(originSiteModel.Grid.CellSize);
    }

    private void TestIt(Func<ISiteModel, bool> loadedFunc,
      Func<ISiteModel, object> loadAction,
      SiteModelOriginConstructionFlags preservationFlags,
      bool finalState)
    {
      var originSiteModel = new SiteModel(Guid.NewGuid(), TRex.Storage.Models.StorageMutability.Immutable, false);
      loadedFunc.Invoke(originSiteModel).Should().BeFalse();

      var original = loadAction.Invoke(originSiteModel);
      loadedFunc.Invoke(originSiteModel).Should().BeTrue();

      var newSiteModel = new SiteModel(originSiteModel, preservationFlags);
      loadedFunc.Invoke(newSiteModel).Should().Be(finalState);

      if (preservationFlags == SiteModelOriginConstructionFlags.PreserveNothing)
        loadAction.Invoke(newSiteModel).Should().NotBe(original);
      else
        loadAction.Invoke(newSiteModel).Should().Be(original);
    }

    private void TestItBothWays(Func<ISiteModel, bool> loadedFunc,
      Func<ISiteModel, object> loadAction,
      SiteModelOriginConstructionFlags preservationFlags)
    {
      TestIt(loadedFunc, loadAction, preservationFlags, true);
      TestIt(loadedFunc, loadAction, SiteModelOriginConstructionFlags.PreserveNothing, false);
    }

    [Fact]
    public void Test_SiteModel_Creation_WithNonTransientOriginModel_ExistenceMap()
    {
      TestItBothWays(siteModel => siteModel.ExistenceMapLoaded, siteModel => siteModel.ExistenceMap, SiteModelOriginConstructionFlags.PreserveExistenceMap);
    }

    /// <summary>
    /// Grid related tests have subtlely different semantics so are kept long hand...
    /// </summary>
    [Fact]
    public void Test_SiteModel_Creation_WithNonTransientOriginModel_PreserveGrid()
    {
      var originSiteModel = new SiteModel(Guid.NewGuid(), TRex.Storage.Models.StorageMutability.Immutable, false);
      var _ = originSiteModel.Grid; // force it to be loaded.
      originSiteModel.GridLoaded.Should().Be(true);

      var original = originSiteModel.Grid;

      var newSiteModel = new SiteModel(originSiteModel, SiteModelOriginConstructionFlags.PreserveGrid);
      newSiteModel.GridLoaded.Should().Be(true);
      newSiteModel.Grid.Should().Be(original);
    }

    [Fact]
    public void Test_SiteModel_Creation_WithNonTransientOriginModel_DoNotPreserveGrid()
    {
      var originSiteModel = new SiteModel(Guid.NewGuid(), TRex.Storage.Models.StorageMutability.Immutable, false);
      var _ = originSiteModel.Grid; // force it to be loaded.
      originSiteModel.GridLoaded.Should().Be(true);

      var newSiteModel = new SiteModel(originSiteModel, SiteModelOriginConstructionFlags.PreserveNothing);
      newSiteModel.GridLoaded.Should().Be(false);
      newSiteModel.Grid.Should().NotBe(null);
    }

    /// <summary>
    /// CSIB related tests have subtlely different semantics so are kept long hand...
    /// </summary>
    [Fact]
    public void Test_SiteModel_Creation_WithNonTransientOriginModel_PreserveCSIB()
    {
      var originSiteModel = new SiteModel(Guid.NewGuid(), TRex.Storage.Models.StorageMutability.Immutable, false);
      originSiteModel.CSIBLoaded.Should().Be(false);

      var original = originSiteModel.CSIB();
      originSiteModel.CSIBLoaded.Should().Be(true);

      var newSiteModel = new SiteModel(originSiteModel, SiteModelOriginConstructionFlags.PreserveCsib);
      newSiteModel.CSIBLoaded.Should().Be(true);
      newSiteModel.CSIB().Should().Be(original);
    }
    
    /// <summary>
    /// CSIB related tests have subtlely different semantics so are kept long hand...
    /// </summary>
    [Fact]
    public void Test_SiteModel_UnloadCSIB()
    {
      var siteModel = new SiteModel(Guid.NewGuid(), TRex.Storage.Models.StorageMutability.Immutable, false);
      siteModel.CSIBLoaded.Should().Be(false);

      var _ = siteModel.CSIB();
      siteModel.CSIBLoaded.Should().Be(true);

      siteModel.UnloadCSIB();
      siteModel.CSIBLoaded.Should().Be(false);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("A dummy coordinate system")]
    public void Test_SiteModel_SetCSIB(string csib)
    {
      var siteModel = new SiteModel(Guid.NewGuid(), TRex.Storage.Models.StorageMutability.Immutable, false);
      siteModel.CSIBLoaded.Should().Be(false);

      siteModel.SetCSIB(csib);

      // Setting a CSIB does not load it
      siteModel.CSIBLoaded.Should().Be(false);

      var _ = siteModel.CSIB();
      siteModel.CSIBLoaded.Should().Be(true);
    }

    [Fact]
    public void Test_SiteModel_Creation_WithNonTransientOriginModel_DoNotPreserveCSIB()
    {
      var originSiteModel = new SiteModel(Guid.NewGuid(), TRex.Storage.Models.StorageMutability.Immutable, false);
      originSiteModel.CSIBLoaded.Should().Be(false);

      var original = originSiteModel.CSIB();
      originSiteModel.CSIBLoaded.Should().Be(true);

      var newSiteModel = new SiteModel(originSiteModel, SiteModelOriginConstructionFlags.PreserveNothing);
      newSiteModel.CSIBLoaded.Should().Be(false);
      newSiteModel.CSIB().Should().Be("");
    }

    [Fact]
    public void Test_SiteModel_Creation_WithNonTransientOriginModel_Designs()
    {
      TestItBothWays(siteModel => siteModel.DesignsLoaded, siteModel => siteModel.Designs, SiteModelOriginConstructionFlags.PreserveDesigns);
    }

    [Fact]
    public void Test_SiteModel_Creation_WithNonTransientOriginModel_SurveyedSurfaces()
    {
      TestItBothWays(siteModel => siteModel.SurveyedSurfacesLoaded, siteModel => siteModel.SurveyedSurfaces, SiteModelOriginConstructionFlags.PreserveSurveyedSurfaces);
    }

    [Fact]
    public void Test_SiteModel_Creation_WithNonTransientOriginModel_Machines()
    {
      TestItBothWays(siteModel => siteModel.MachinesLoaded, siteModel => siteModel.Machines, SiteModelOriginConstructionFlags.PreserveMachines);
    }

    [Fact]
    public void Test_SiteModel_Creation_WithNonTransientOriginModel_MachineTargetValues()
    {
      TestItBothWays(siteModel => siteModel.MachineTargetValuesLoaded, siteModel => siteModel.MachinesTargetValues, SiteModelOriginConstructionFlags.PreserveMachineTargetValues);
    }

    [Fact]
    public void Test_SiteModel_Creation_WithNonTransientOriginModel_MachineDesigns()
    {
      TestItBothWays(siteModel => siteModel.SiteModelMachineDesignsLoaded, siteModel => siteModel.SiteModelMachineDesigns, SiteModelOriginConstructionFlags.PreserveMachineDesigns);
    }

    [Fact]
    public void Test_SiteModel_Creation_WithNonTransientOriginModel_ProofingRuns()
    {
      TestItBothWays(siteModel => siteModel.SiteProofingRunsLoaded, siteModel => siteModel.SiteProofingRuns, SiteModelOriginConstructionFlags.PreserveProofingRuns);
    }

    [Fact]
    public void Test_SiteModel_Creation_WithNonTransientOriginModel_Alignments()
    {
      TestItBothWays(siteModel => siteModel.AlignmentsLoaded, siteModel => siteModel.Alignments, SiteModelOriginConstructionFlags.PreserveAlignments);
    }

    [Fact]
    public void Test_SiteModel_Creation_WithNonTransientOriginModel_SiteModelDesigns()
    {
      TestItBothWays(siteModel => siteModel.SiteModelDesignsLoaded, siteModel => siteModel.SiteModelDesigns, SiteModelOriginConstructionFlags.PreserveSiteModelDesigns);
    }

    [Fact]
    public void Test_SiteModel_Creation_WithNonTransientOriginModel_PreserveMultiple()
    {
      var originSiteModel = new SiteModel(Guid.NewGuid(), TRex.Storage.Models.StorageMutability.Immutable, false);
      var _1 = originSiteModel.Alignments;
      var _2 = originSiteModel.Designs;
      var _3 = originSiteModel.SiteModelDesigns;
      var _4 = originSiteModel.SiteProofingRuns;
      var _5 = originSiteModel.MachinesTargetValues;
      var _6 = originSiteModel.SiteModelMachineDesigns;
      var _7 = originSiteModel.Machines;
      var _8 = originSiteModel.SurveyedSurfaces;
      var _9 = originSiteModel.Machines;
      var _10 = originSiteModel.ExistenceMap;

      var newSiteModel = new SiteModel(originSiteModel,
                                       SiteModelOriginConstructionFlags.PreserveExistenceMap |
                                       SiteModelOriginConstructionFlags.PreserveGrid |
                                       SiteModelOriginConstructionFlags.PreserveCsib |
                                       SiteModelOriginConstructionFlags.PreserveDesigns |
                                       SiteModelOriginConstructionFlags.PreserveSurveyedSurfaces |
                                       SiteModelOriginConstructionFlags.PreserveMachines |
                                       SiteModelOriginConstructionFlags.PreserveMachineTargetValues |
                                       SiteModelOriginConstructionFlags.PreserveMachineDesigns |
                                       SiteModelOriginConstructionFlags.PreserveSiteModelDesigns |
                                       SiteModelOriginConstructionFlags.PreserveProofingRuns |
                                       SiteModelOriginConstructionFlags.PreserveAlignments);

      newSiteModel.AlignmentsLoaded.Should().Be(true);
      newSiteModel.DesignsLoaded.Should().Be(true);
      newSiteModel.SiteModelDesignsLoaded.Should().Be(true);
      newSiteModel.SiteProofingRunsLoaded.Should().Be(true);
      newSiteModel.MachineTargetValuesLoaded.Should().Be(true);
      newSiteModel.SiteModelMachineDesignsLoaded.Should().Be(true);
      newSiteModel.MachinesLoaded.Should().Be(true);
      newSiteModel.SurveyedSurfacesLoaded.Should().Be(true);
      newSiteModel.MachinesLoaded.Should().Be(true);
      newSiteModel.ExistenceMapLoaded.Should().Be(true);

      newSiteModel.Alignments.Should().BeSameAs(originSiteModel.Alignments);
      newSiteModel.Designs.Should().BeSameAs(originSiteModel.Designs);
      newSiteModel.SiteModelDesigns.Should().BeSameAs(originSiteModel.SiteModelDesigns);
      newSiteModel.SiteProofingRuns.Should().BeSameAs(originSiteModel.SiteProofingRuns);
      newSiteModel.MachinesTargetValues.Should().Be(originSiteModel.MachinesTargetValues);
      newSiteModel.SiteModelMachineDesigns.Should().BeSameAs(originSiteModel.SiteModelMachineDesigns);
      newSiteModel.Machines.Should().BeSameAs(originSiteModel.Machines);
      newSiteModel.SurveyedSurfaces.Should().BeSameAs(originSiteModel.SurveyedSurfaces);
      newSiteModel.Machines.Should().BeSameAs(originSiteModel.Machines);
      newSiteModel.ExistenceMap.Should().Be(originSiteModel.ExistenceMap);
    }

    [Fact]
    public void Test_SiteModel_Creation_WithNonTransientOriginModel_DoNotPreserveMultiple()
    {
      var originSiteModel = new SiteModel(Guid.NewGuid(), TRex.Storage.Models.StorageMutability.Immutable, false);
      var _1 = originSiteModel.Alignments;
      var _2 = originSiteModel.Designs;
      var _3 = originSiteModel.SiteModelDesigns;
      var _4 = originSiteModel.SiteProofingRuns;
      var _5 = originSiteModel.MachinesTargetValues;
      var _6 = originSiteModel.SiteModelMachineDesigns;
      var _7 = originSiteModel.Machines;
      var _8 = originSiteModel.SurveyedSurfaces;
      var _9 = originSiteModel.Machines;
      var _10 = originSiteModel.ExistenceMap;

      var newSiteModel = new SiteModel(originSiteModel, SiteModelOriginConstructionFlags.PreserveNothing);

      newSiteModel.AlignmentsLoaded.Should().Be(false);
      newSiteModel.DesignsLoaded.Should().Be(false);
      newSiteModel.SiteModelDesignsLoaded.Should().Be(false);
      newSiteModel.SiteProofingRunsLoaded.Should().Be(false);
      newSiteModel.MachineTargetValuesLoaded.Should().Be(false);
      newSiteModel.SiteModelMachineDesignsLoaded.Should().Be(false);
      newSiteModel.MachinesLoaded.Should().Be(false);
      newSiteModel.SurveyedSurfacesLoaded.Should().Be(false);
      newSiteModel.MachinesLoaded.Should().Be(false);
      newSiteModel.ExistenceMapLoaded.Should().Be(false);

      newSiteModel.Alignments.Should().NotBeSameAs(originSiteModel.Alignments);
      newSiteModel.Designs.Should().NotBeSameAs(originSiteModel.Designs);
      newSiteModel.SiteModelDesigns.Should().NotBeSameAs(originSiteModel.SiteModelDesigns);
      newSiteModel.SiteProofingRuns.Should().NotBeSameAs(originSiteModel.SiteProofingRuns);
      newSiteModel.MachinesTargetValues.Should().NotBe(originSiteModel.MachinesTargetValues);
      newSiteModel.SiteModelMachineDesigns.Should().NotBeSameAs(originSiteModel.SiteModelMachineDesigns);
      newSiteModel.Machines.Should().NotBeSameAs(originSiteModel.Machines);
      newSiteModel.SurveyedSurfaces.Should().NotBeSameAs(originSiteModel.SurveyedSurfaces);
      newSiteModel.Machines.Should().NotBeSameAs(originSiteModel.Machines);
      newSiteModel.ExistenceMap.Should().NotBe(originSiteModel.ExistenceMap);
    }

    [Fact]
    public void Test_SiteModel_Serialization()
    {
      const int expectedSiteModelSerializedStreamSize = 90;

      var guid = Guid.NewGuid();
      var siteModel = new SiteModel(guid, TRex.Storage.Models.StorageMutability.Immutable, 1.23);

      var stream = siteModel.ToStream();
      stream.Length.Should().Be(expectedSiteModelSerializedStreamSize);

      stream.Position = 0;

      var siteModel2 = new SiteModel(TRex.Storage.Models.StorageMutability.Immutable);
      siteModel2.FromStream(stream);

      siteModel2.ID.Should().Be(siteModel.ID);
      siteModel2.CellSize.Should().Be(siteModel.Grid.CellSize);
      siteModel2.Grid.ID.Should().Be(siteModel.Grid.ID);
      siteModel2.Grid.CellSize.Should().Be(siteModel.Grid.CellSize);
      siteModel2.CreationDate.Should().Be(siteModel.CreationDate);
      siteModel2.LastModifiedDate.Should().Be(siteModel.LastModifiedDate);
      siteModel2.IsTransient.Should().Be(siteModel.IsTransient);
      siteModel2.SiteModelExtent.Should().BeEquivalentTo(siteModel.SiteModelExtent);
    }

    [Theory]
    [InlineData(7, null, 7, 7)]//SS only
    [InlineData(null, 10, 0, 10)]//Production data only
    [InlineData(-5, 10, -5, 10)]//SS before production data
    [InlineData(5, 10, 0, 10)]//SS within production data
    [InlineData(15, 10, 0, 15)]//SS after production data
    public void Test_SiteModel_GetDateRange_SS_And_ProductionData(int? ssMins, int? totalMins, int expectedStartMins, int expectedEndMins)
    {
      var baseTime = DateTime.UtcNow;
      var siteModel = BuildModelForProductionDataAndOrSurveyedSurface(baseTime, totalMins, ssMins);

      var (startUtc, endUtc) = siteModel.GetDateRange();
      startUtc.Should().Be(baseTime.AddMinutes(expectedStartMins));
      endUtc.Should().Be(baseTime.AddMinutes(expectedEndMins));
    }

    private ISiteModel BuildModelForProductionDataAndOrSurveyedSurface(DateTime baseTime, int? totalMins, int? ssMins)
    {
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      if (totalMins.HasValue)
      {
        var bulldozerMachineIndex = siteModel.Machines.Locate("Bulldozer", false).InternalSiteModelMachineIndex;

        // Ensure the machine has start and stop events (don't need actual cell passes)
        siteModel.MachinesTargetValues[bulldozerMachineIndex].StartEndRecordedDataEvents.PutValueAtDate(baseTime, ProductionEventType.StartEvent);
        siteModel.MachinesTargetValues[bulldozerMachineIndex].StartEndRecordedDataEvents.PutValueAtDate(baseTime.AddMinutes(totalMins.Value), ProductionEventType.EndEvent);

        siteModel.MachinesTargetValues[bulldozerMachineIndex].SaveMachineEventsToPersistentStore(siteModel.PrimaryStorageProxy);
      }

      if (ssMins.HasValue)
        siteModel.SurveyedSurfaces.AddSurveyedSurfaceDetails(Guid.NewGuid(), DesignDescriptor.Null(), baseTime.AddMinutes(ssMins.Value), BoundingWorldExtent3D.Null());
     
      DITAGFileAndSubGridRequestsFixture.ConvertSiteModelToImmutable(siteModel);

      return siteModel;
    }

  }
}
