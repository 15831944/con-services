﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using VSS.MasterData.Models.Models;
using VSS.TRex.Alignments.Interfaces;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Geometry;
using VSS.TRex.Machines;
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.GridFabric.ComputeFuncs;
using VSS.TRex.SiteModels.GridFabric.Requests;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.SiteModels.GridFabric.Requests
{
  [UnitTestCoveredRequest(RequestType = typeof(DeleteSiteModelRequest))]
  public class DeleteSiteModelRequestTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private void AddApplicationGridRouting() => IgniteMock.Mutable.AddApplicationGridRouting<DeleteSiteModelRequestComputeFunc, DeleteSiteModelRequestArgument, DeleteSiteModelRequestResponse>();

    public DeleteSiteModelRequestTests()
    {
      // This resets all modified content in the Ignite mocks between tests
      DITAGFileAndSubGridRequestsWithIgniteFixture.ResetDynamicMockedIgniteContent();
    }

    private static bool IsModelEmpty(ISiteModel model)
    {
      var clear1 = !IgniteMock.Mutable.MockedCacheDictionaries.Any(cache => cache.Keys.Count > 0) &&
                   !IgniteMock.Immutable.MockedCacheDictionaries.Any(cache => cache.Keys.Count > 0);

      // Perform a belt and braces check to ensure there were no pending uncommitted changes.
      model.PrimaryStorageProxy.Commit();

      var clear2 = !IgniteMock.Mutable.MockedCacheDictionaries.Any(cache => cache.Keys.Count > 0) &&
                   !IgniteMock.Immutable.MockedCacheDictionaries.Any(cache => cache.Keys.Count > 0);

      return clear1 && clear2;
    }

    private void VerifyModelIsEmpty(ISiteModel model)
    {
      IsModelEmpty(model).Should().BeTrue();
    }

    private void DeleteTheModel(ISiteModel model)
    {
      var modelId = model.ID;

      var request = new DeleteSiteModelRequest();
      var response = request.Execute(new DeleteSiteModelRequestArgument {ProjectID = modelId});

      response.Result.Should().Be(DeleteSiteModelResult.OK);

      VerifyModelIsEmpty(model);
    }

    private void SaveAndVerifyNotEmpty(ISiteModel model)
    {
      model.SaveToPersistentStoreForTAGFileIngest(model.PrimaryStorageProxy).Wait();
      model.PrimaryStorageProxy.Commit();
      IsModelEmpty(model).Should().BeFalse();
    }

    [Fact]
    public void Creation()
    {
      var req = new DeleteSiteModelRequest();
      req.Should().NotBeNull();
    }

    [Fact]
    public void EmptyNonCommittedModelIsEmpty()
    {
      AddApplicationGridRouting();

      var model = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      model.Should().NotBeNull();

      IsModelEmpty(model).Should().BeTrue();
    }

    [Fact]
    public void DeleteEmptyModel_StandardMetaDataPersistence()
    {
      AddApplicationGridRouting();

      var model = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel(false);
      model.Should().NotBeNull();

      model.SaveMetadataToPersistentStore(model.PrimaryStorageProxy, true);
      IsModelEmpty(model).Should().BeFalse();

      DeleteTheModel(model);
    }

    [Fact]
    public void DeleteEmptyModel_TAGFileIngestPersistence()
    {
      AddApplicationGridRouting();

      var model = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel(false);
      model.Should().NotBeNull();

      SaveAndVerifyNotEmpty(model);

      DeleteTheModel(model);
    }

    [Fact]
    public void DeleteModel_WithMachines()
    {
      AddApplicationGridRouting();

      var model = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel(false);
      model.Should().NotBeNull();

      model.Machines.Add(new Machine("Test Delete Machine", "HardwareId", MachineType.Dozer, DeviceTypeEnum.SNM940, Guid.NewGuid(), 1, false));
      _ = new SiteProofingRun("Test Proofing Run", 0, DateTime.UtcNow.AddHours(-1), DateTime.UtcNow, new BoundingWorldExtent3D(0, 0, 1, 1));
      SaveAndVerifyNotEmpty(model);

      DeleteTheModel(model);
    }

    [Fact]
    public void DeleteModel_WithMachineEvents()
    {
      AddApplicationGridRouting();

      var model = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      model.Should().NotBeNull();

      model.MachinesTargetValues[0].AutoVibrationStateEvents.PutValueAtDate(DateTime.UtcNow, AutoVibrationState.Auto);
      SaveAndVerifyNotEmpty(model);

      DeleteTheModel(model);
    }

    [Fact]
    public void DeleteModel_WithProofingRuns()
    {
      AddApplicationGridRouting();

      var model = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel(false);
      model.Should().NotBeNull();

      model.SiteProofingRuns.Add(new SiteProofingRun("Test Proofing Run", 0, DateTime.UtcNow.AddHours(-1), DateTime.UtcNow, new BoundingWorldExtent3D(0, 0, 1, 1)));
      SaveAndVerifyNotEmpty(model);

      DeleteTheModel(model);
    }

    [Fact]
    public void DeleteModel_WithSiteModelMachineDesigns()
    {
      AddApplicationGridRouting();

      var model = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel(false);
      model.Should().NotBeNull();

      model.SiteModelMachineDesigns.Add(new SiteModelMachineDesign(-1, "Test Name"));
      SaveAndVerifyNotEmpty(model);

      DeleteTheModel(model);
    }

    [Fact]
    public void DeleteModel_WithSiteModelDesigns()
    {
      AddApplicationGridRouting();

      var model = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel(false);
      model.Should().NotBeNull();

      model.SiteModelDesigns.Add(new SiteModelDesign("Test name", new BoundingWorldExtent3D(0, 0, 1, 1)));
      SaveAndVerifyNotEmpty(model);

      DeleteTheModel(model);
    }

    [Fact]
    public void DeleteModel_WithSiteDesigns()
    {
      AddApplicationGridRouting();

      var model = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel(false);
      model.Should().NotBeNull();

      DIContext.Obtain<IDesignManager>().Add(model.ID, new DesignDescriptor(Guid.NewGuid(), "", ""), new BoundingWorldExtent3D(0, 0, 1, 1));
      SaveAndVerifyNotEmpty(model);

      DeleteTheModel(model);
    }

    [Fact]
    public void DeleteModel_WithSurveyedSurfaces()
    {
      AddApplicationGridRouting();

      var model = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel(false);
      model.Should().NotBeNull();

      DIContext.Obtain<ISurveyedSurfaceManager>().Add(model.ID, new DesignDescriptor(Guid.NewGuid(), "", ""), DateTime.UtcNow, new BoundingWorldExtent3D(0, 0, 1, 1));
      SaveAndVerifyNotEmpty(model);

      DeleteTheModel(model);
    }

    [Fact]
    public void DeleteModel_WithAlignments()
    {
      AddApplicationGridRouting();

      var model = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel(false);
      model.Should().NotBeNull();

      DIContext.Obtain<IAlignmentManager>().Add(model.ID, new DesignDescriptor(Guid.NewGuid(), "", ""), new BoundingWorldExtent3D(0, 0, 1, 1));
      SaveAndVerifyNotEmpty(model);

      DeleteTheModel(model);
    }

    [Fact]
    public async Task DeleteModel_WithCSIB()
    {
      AddApplicationGridRouting();

      var model = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel(false);
      model.Should().NotBeNull();

      var csibStream = new MemoryStream();
      csibStream.Write(new byte[] {70, 71, 72, 73}, 0, 4);
      csibStream.Position = 0;

      await model.PrimaryStorageProxy.WriteStreamToPersistentStore(model.ID,
        CoordinateSystemConsts.CoordinateSystemCSIBStorageKeyName,
        FileSystemStreamType.CoordinateSystemCSIB,
        csibStream, null);

      model.CSIB().Should().NotBeEmpty();
      SaveAndVerifyNotEmpty(model);

      DeleteTheModel(model);
    }

    [Fact]
    public void DeleteModel_WithSummaryMetadata()
    {
      AddApplicationGridRouting();

      var model = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel(false);
      model.Should().NotBeNull();

      DIContext.Obtain<ISiteModelMetadataManager>().Add(model.ID, new SiteModelMetadata());
      SaveAndVerifyNotEmpty(model);

      DeleteTheModel(model);
    }

    [Fact]
    public void DeleteModel_WithExistenceMap()
    {
      AddApplicationGridRouting();

      var model = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel(false);
      model.Should().NotBeNull();

      model.ExistenceMap[0, 0] = true;
      SaveAndVerifyNotEmpty(model);

      DeleteTheModel(model);
    }

    [Fact]
    public void DeleteModel_WithTagFile()
    {
      var tagFiles = new[] {Path.Combine(TestHelper.CommonTestDataPath, "TestTAGFile.tag"),};

      AddApplicationGridRouting();

      var model = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _);
      model.Should().NotBeNull();

      SaveAndVerifyNotEmpty(model);

      // Delete project requests must be made to the mutable grid
      model.SetStorageRepresentationToSupply(StorageMutability.Mutable);

      DeleteTheModel(model);
    }
  }
}
