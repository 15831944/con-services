﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using VSS.AWS.TransferProxy;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.DataOcean.Client;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.Pegasus.Client;
using VSS.Productivity3D.Filter.Abstractions.Interfaces;
using VSS.Productivity3D.Filter.Abstractions.Models;
using VSS.Productivity3D.Models.Models.Designs;
using VSS.Productivity3D.Project.Abstractions.Interfaces.Repository;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Productivity3D.Scheduler.Abstractions;
using VSS.Productivity3D.Scheduler.Models;
using VSS.TCCFileAccess;
using VSS.TRex.Gateway.Common.Abstractions;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;
using VSS.WebApi.Common;
using Xunit;

namespace VSS.MasterData.ProjectTests.Executors
{
  public class ImportFilev6ExecutorTests : UnitTestsDIFixture<ImportFilev6ExecutorTests>
  {
    private static string _userEmailAddress;
    private static long _shortRaptorProjectId;
    private static string _fileSpaceId;

    public ImportFilev6ExecutorTests()
    {
      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();
      _userEmailAddress = "someone@whatever.com";
      _shortRaptorProjectId = 111;
      _fileSpaceId = "u710e3466-1d47-45e3-87b8-81d1127ed4ed";
    }

    [Fact]
    public async Task CopyTCCFile()
    {
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();

      var importedFileTbc = new ImportedFileTbc
      {
        FileSpaceId = _fileSpaceId,
        Name = "MoundRoadlinework.dxf",
        Path = "/BC Data/Sites/Chch Test Site/Designs/Mound Road",
        ImportedFileTypeId = ImportedFileType.Linework,
        CreatedUtc = DateTime.UtcNow
      };

      var fileRepo = new Mock<IFileRepository>();
      fileRepo.Setup(fr => fr.FolderExists(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
      fileRepo.Setup(fr => fr.CopyFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

      await TccHelper.CopyFileWithinTccRepository(importedFileTbc,
        _customerUid.ToString(), Guid.NewGuid().ToString(), "f9sdg0sf9",
        _log, serviceExceptionHandler, fileRepo.Object).ConfigureAwait(false);
    }

    [Fact]
    public async Task CreateImportedFile_HappyPath_GeoTiff()
    {
      // FlowFile uploads the file from client (possibly as a background task via scheduler)
      // Controller uploads file to TCC and/or S3
      //    V2 Note: BCC file has already put the file on TCC.
      //          the controller a) copies within TCC to client project (raptor)
      //                         b) copies locally and hence to S3. (TRex)
      var customHeaders = new HeaderDictionary();
      var importedFileUid = Guid.NewGuid();
      var TCCFilePath = "/BC Data/Sites/Chch Test Site";
      var fileName = "MoundRoad.tif";
      var fileDescriptor = FileDescriptor.CreateFileDescriptor(_fileSpaceId, TCCFilePath, fileName);
      var fileCreatedUtc = DateTime.UtcNow.AddHours(-45);
      var fileUpdatedUtc = fileCreatedUtc.AddHours(1);
      var surveyedUtc = fileCreatedUtc.AddHours(-1);

      var newImportedFile = new ImportedFile
      {
        ProjectUid = _projectUid.ToString(),
        ImportedFileUid = importedFileUid.ToString(),
        ImportedFileId = 999,
        LegacyImportedFileId = 200000,
        ImportedFileType = ImportedFileType.GeoTiff,
        Name = fileDescriptor.FileName,
        FileDescriptor = JsonConvert.SerializeObject(fileDescriptor),
        SurveyedUtc = surveyedUtc
      };

      _ = new CreateImportedFileEvent
      {
        CustomerUID = _customerUid,
        ProjectUID = _projectUid,
        ImportedFileUID = importedFileUid,
        ImportedFileType = ImportedFileType.GeoTiff,
        DxfUnitsType = DxfUnitsType.Meters,
        Name = fileDescriptor.FileName,
        FileDescriptor = JsonConvert.SerializeObject(fileDescriptor),
        FileCreatedUtc = fileCreatedUtc,
        FileUpdatedUtc = fileUpdatedUtc,
        ImportedBy = string.Empty,
        SurveyedUTC = surveyedUtc,
        ParentUID = null,
        Offset = 0,
        ActionUTC = DateTime.UtcNow
      };

      var createImportedFile = new CreateImportedFile(
        _projectUid, fileDescriptor.FileName, fileDescriptor, ImportedFileType.GeoTiff, surveyedUtc, DxfUnitsType.Meters,
        fileCreatedUtc, fileUpdatedUtc, "some folder", null, 0, importedFileUid, "some file");

      var importedFilesList = new List<ImportedFile> { newImportedFile };
      var mockConfigStore = new Mock<IConfigurationStore>();

      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();

      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<CreateImportedFileEvent>())).ReturnsAsync(1);
      projectRepo.Setup(pr => pr.GetImportedFile(It.IsAny<string>())).ReturnsAsync(newImportedFile);
      projectRepo.Setup(pr => pr.GetImportedFiles(It.IsAny<string>())).ReturnsAsync(importedFilesList);

      var project = CreateProjectDetailModel(_customerTrn, _projectTrn);
      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetMyProject(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(project);
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CwsProjectType?>(), It.IsAny<ProjectStatus?>(), It.IsAny<bool>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);

      var scheduler = new Mock<ISchedulerProxy>();
      scheduler.Setup(s => s.ScheduleVSSJob(It.IsAny<JobRequest>(), It.IsAny<HeaderDictionary>())).ReturnsAsync(new ScheduleJobResult());

      var executor = RequestExecutorContainerFactory
        .Build<CreateImportedFileExecutor>(logger, mockConfigStore.Object, serviceExceptionHandler,
          _customerUid.ToString(), _userUid.ToString(), _userEmailAddress, customHeaders,
          projectRepo: projectRepo.Object, schedulerProxy: scheduler.Object,
          cwsProjectClient: cwsProjectClient.Object);
      var result = await executor.ProcessAsync(createImportedFile).ConfigureAwait(false) as ImportedFileDescriptorSingleResult;
      Assert.NotNull(result);
      Assert.Equal(0, result.Code);
      Assert.NotNull(result.ImportedFileDescriptor);
      Assert.Equal(_projectUid.ToString(), result.ImportedFileDescriptor.ProjectUid);
      Assert.Equal(fileDescriptor.FileName, result.ImportedFileDescriptor.Name);
    }

    [Fact]
    public async Task CreateImportedFile_TRexHappyPath_DesignSurface()
    {
      // FlowFile uploads the file from client (possibly as a background task via scheduler)
      // Controller uploads file to TCC and/or S3
      //    V2 Note: BCC file has already put the file on TCC.
      //          the controller a) copies within TCC to client project (raptor)
      //                         b) copies locally and hence to S3. (TRex)
      var customHeaders = new HeaderDictionary();
      var importedFileUid = Guid.NewGuid();
      var TCCFilePath = "/BC Data/Sites/Chch Test Site";
      var fileName = "MoundRoad.ttm";
      var fileDescriptor = FileDescriptor.CreateFileDescriptor(_fileSpaceId, TCCFilePath, fileName);
      var fileCreatedUtc = DateTime.UtcNow.AddHours(-45);
      var fileUpdatedUtc = fileCreatedUtc;

      var newImportedFile = new ImportedFile
      {
        ProjectUid = _projectUid.ToString(),
        ImportedFileUid = importedFileUid.ToString(),
        ImportedFileId = 999,
        LegacyImportedFileId = 200000,
        ImportedFileType = ImportedFileType.DesignSurface,
        Name = fileDescriptor.FileName,
        FileDescriptor = JsonConvert.SerializeObject(fileDescriptor)
      };

      _ = new CreateImportedFileEvent
      {
        CustomerUID = _customerUid,
        ProjectUID = _projectUid,
        ImportedFileUID = importedFileUid,
        ImportedFileType = ImportedFileType.DesignSurface,
        DxfUnitsType = DxfUnitsType.Meters,
        Name = fileDescriptor.FileName,
        FileDescriptor = JsonConvert.SerializeObject(fileDescriptor),
        FileCreatedUtc = fileCreatedUtc,
        FileUpdatedUtc = fileUpdatedUtc,
        ImportedBy = string.Empty,
        SurveyedUTC = null,
        ParentUID = null,
        Offset = 0,
        ActionUTC = DateTime.UtcNow
      };

      var createImportedFile = new CreateImportedFile(
        _projectUid, fileDescriptor.FileName, fileDescriptor, ImportedFileType.DesignSurface, null, DxfUnitsType.Meters,
        DateTime.UtcNow.AddHours(-45), DateTime.UtcNow.AddHours(-44), "some folder", null, 0, importedFileUid, "some file");

      var importedFilesList = new List<ImportedFile> { newImportedFile };

      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();

      var tRexImportFileProxy = new Mock<ITRexImportFileProxy>();
      tRexImportFileProxy.Setup(tr => tr.AddFile(It.IsAny<DesignRequest>(), It.IsAny<HeaderDictionary>())).ReturnsAsync(new ContractExecutionResult());
      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<CreateImportedFileEvent>())).ReturnsAsync(1);
      projectRepo.Setup(pr => pr.GetImportedFile(It.IsAny<string>())).ReturnsAsync(newImportedFile);
      projectRepo.Setup(pr => pr.GetImportedFiles(It.IsAny<string>())).ReturnsAsync(importedFilesList);

      var executor = RequestExecutorContainerFactory
        .Build<CreateImportedFileExecutor>(logger, null, serviceExceptionHandler,
          _customerUid.ToString(), _userUid.ToString(), _userEmailAddress, customHeaders,
          tRexImportFileProxy: tRexImportFileProxy.Object, projectRepo: projectRepo.Object);
      var result = await executor.ProcessAsync(createImportedFile).ConfigureAwait(false) as ImportedFileDescriptorSingleResult;
      Assert.NotNull(result);
      Assert.Equal(0, result.Code);
      Assert.NotNull(result.ImportedFileDescriptor);
      Assert.Equal(_projectUid.ToString(), result.ImportedFileDescriptor.ProjectUid);
      Assert.Equal(fileDescriptor.FileName, result.ImportedFileDescriptor.Name);
    }

    [Fact]
    public async Task UpdateImportedFile_TRexHappyPath_DesignSurface()
    {
      var customHeaders = new HeaderDictionary();
      var importedFileUid = Guid.NewGuid();
      var importedFileId = 9999;
      var TCCFilePath = "/BC Data/Sites/Chch Test Site";
      var fileName = "MoundRoad.ttm";
      var fileDescriptor = FileDescriptor.CreateFileDescriptor(_fileSpaceId, TCCFilePath, fileName);

      var existingImportedFile = new ImportedFile
      {
        ProjectUid = _projectUid.ToString(),
        ImportedFileUid = importedFileUid.ToString(),
        LegacyImportedFileId = 200000,
        ImportedFileType = ImportedFileType.DesignSurface,
        Name = fileName,
        FileDescriptor = JsonConvert.SerializeObject(fileDescriptor)
      };
      var importedFilesList = new List<ImportedFile> { existingImportedFile };
      var updateImportedFile = new UpdateImportedFile(
       _projectUid, _shortRaptorProjectId, ImportedFileType.DesignSurface, null, DxfUnitsType.Meters, DateTime.UtcNow.AddHours(-45),
       DateTime.UtcNow.AddHours(-44), fileDescriptor, importedFileUid, importedFileId, "some folder", 0, "some file"
      );

      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();

      var tRexImportFileProxy = new Mock<ITRexImportFileProxy>();
      tRexImportFileProxy.Setup(tr => tr.UpdateFile(It.IsAny<DesignRequest>(), It.IsAny<HeaderDictionary>())).ReturnsAsync(new ContractExecutionResult());
      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<UpdateImportedFileEvent>())).ReturnsAsync(1);
      projectRepo.Setup(pr => pr.GetImportedFile(It.IsAny<string>())).ReturnsAsync(existingImportedFile);
      projectRepo.Setup(pr => pr.GetImportedFiles(It.IsAny<string>())).ReturnsAsync(importedFilesList);

      var executor = RequestExecutorContainerFactory
        .Build<UpdateImportedFileExecutor>(logger, null, serviceExceptionHandler,
          _customerUid.ToString(), _userUid.ToString(), _userEmailAddress, customHeaders,
          tRexImportFileProxy: tRexImportFileProxy.Object, projectRepo: projectRepo.Object);
      var result = await executor.ProcessAsync(updateImportedFile).ConfigureAwait(false) as ImportedFileDescriptorSingleResult;
      Assert.Equal(0, result.Code);
      Assert.NotNull(result.ImportedFileDescriptor);
      Assert.Equal(_projectUid.ToString(), result.ImportedFileDescriptor.ProjectUid);
      Assert.Equal(fileDescriptor.FileName, result.ImportedFileDescriptor.Name);
    }

    [Fact]
    public async Task DeleteImportedFile_TRexHappyPath_DesignSurface()
    {
      // FlowFile uploads the file from client (possibly as a background task via scheduler)
      // Controller uploads file to TCC and/or S3
      //    V2 Note: BCC file has already put the file on TCC.
      //          the controller a) copies within TCC to client project (raptor)
      //                         b) copies locally and hence to S3. (TRex)
      var customHeaders = new HeaderDictionary();
      var importedFileUid = Guid.NewGuid();
      var importedFileId = 9999;
      var TCCFilePath = "/BC Data/Sites/Chch Test Site";
      var fileName = "MoundRoad.ttm";
      var fileDescriptor = FileDescriptor.CreateFileDescriptor(_fileSpaceId, TCCFilePath, fileName);

      var existingImportedFile = new ImportedFile
      {
        ProjectUid = _projectUid.ToString(),
        ImportedFileUid = importedFileUid.ToString(),
        LegacyImportedFileId = 200000,
        ImportedFileType = ImportedFileType.DesignSurface,
        Name = fileName,
        FileDescriptor = JsonConvert.SerializeObject(fileDescriptor),
        ParentUid = null,
        Offset = 0
      };

      var deleteImportedFile = new DeleteImportedFile(
        _projectUid, ImportedFileType.DesignSurface, fileDescriptor,
        importedFileUid, importedFileId, existingImportedFile.LegacyImportedFileId, "some folder", null
      );

      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();

      var tRexImportFileProxy = new Mock<ITRexImportFileProxy>();
      tRexImportFileProxy.Setup(tr => tr.DeleteFile(It.IsAny<DesignRequest>(), It.IsAny<HeaderDictionary>())).ReturnsAsync(new ContractExecutionResult());

      var filterServiceProxy = new Mock<IFilterServiceProxy>();
      filterServiceProxy.Setup(fs => fs.GetFilters(It.IsAny<string>(), It.IsAny<HeaderDictionary>()))
        .ReturnsAsync(new List<FilterDescriptor>().ToImmutableList);

      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<DeleteImportedFileEvent>())).ReturnsAsync(1);

      var dataOceanClient = new Mock<IDataOceanClient>();
      dataOceanClient.Setup(f => f.FileExists(It.IsAny<string>(), It.IsAny<HeaderDictionary>())).ReturnsAsync(true);
      dataOceanClient.Setup(f => f.DeleteFile(It.IsAny<string>(), It.IsAny<HeaderDictionary>())).ReturnsAsync(true);

      var authn = new Mock<ITPaaSApplicationAuthentication>();
      authn.Setup(a => a.GetApplicationBearerToken()).Returns("some token");

      var pegasusClient = new Mock<IPegasusClient>();

      var transferProxy = new Mock<ITransferProxy>();
      transferProxy.Setup(t => t.RemoveFromBucket(It.IsAny<string>())).Returns(true);
      var transferProxyFactory = new Mock<ITransferProxyFactory>();
      transferProxyFactory.Setup(t => t.NewProxy(TransferProxyType.DesignImport)).Returns(transferProxy.Object);

      var executor = RequestExecutorContainerFactory
        .Build<DeleteImportedFileExecutor>(
          logger, null, serviceExceptionHandler, _customerUid.ToString(), _userUid.ToString(), _userEmailAddress,
          customHeaders, filterServiceProxy: filterServiceProxy.Object, tRexImportFileProxy: tRexImportFileProxy.Object, 
          projectRepo: projectRepo.Object, dataOceanClient: dataOceanClient.Object, authn: authn.Object, pegasusClient: pegasusClient.Object,
          persistantTransferProxyFactory: transferProxyFactory.Object);
      await executor.ProcessAsync(deleteImportedFile);
    }

    [Fact]
    public async Task CreateImportedFile_TRexHappyPath_ReferenceSurface()
    {
      // FlowFile uploads the file from client (possibly as a background task via scheduler)
      // Controller uploads file to TCC and/or S3
      //    V2 Note: BCC file has already put the file on TCC.
      //          the controller a) copies within TCC to client project (raptor)
      //                         b) copies locally and hence to S3. (TRex)
      var customHeaders = new HeaderDictionary();
      var importedFileUid = Guid.NewGuid();
      var parentUid = Guid.NewGuid();
      var offset = 1.5;
      var TCCFilePath = "/BC Data/Sites/Chch Test Site";
      var fileName = "MoundRoad.ttm";
      var fileDescriptor = FileDescriptor.CreateFileDescriptor(_fileSpaceId, TCCFilePath, fileName);
      var fileCreatedUtc = DateTime.UtcNow.AddHours(-45);
      var fileUpdatedUtc = fileCreatedUtc;

      var newImportedFile = new ImportedFile
      {
        ProjectUid = _projectUid.ToString(),
        ImportedFileUid = importedFileUid.ToString(),
        ImportedFileId = 999,
        LegacyImportedFileId = 200000,
        ImportedFileType = ImportedFileType.ReferenceSurface,
        Name = fileDescriptor.FileName,
        FileDescriptor = JsonConvert.SerializeObject(fileDescriptor),
        Offset = offset,
        ParentUid = parentUid.ToString()
      };

      _ = new CreateImportedFileEvent
      {
        CustomerUID = _customerUid,
        ProjectUID = _projectUid,
        ImportedFileUID = importedFileUid,
        ImportedFileType = ImportedFileType.ReferenceSurface,
        DxfUnitsType = DxfUnitsType.Meters,
        Name = fileDescriptor.FileName,
        FileDescriptor = JsonConvert.SerializeObject(fileDescriptor),
        FileCreatedUtc = fileCreatedUtc,
        FileUpdatedUtc = fileUpdatedUtc,
        ImportedBy = string.Empty,
        SurveyedUTC = null,
        ParentUID = parentUid,
        Offset = offset,
        ActionUTC = DateTime.UtcNow
      };

      var createImportedFile = new CreateImportedFile(
        _projectUid, fileDescriptor.FileName, fileDescriptor, ImportedFileType.ReferenceSurface, null, DxfUnitsType.Meters,
        DateTime.UtcNow.AddHours(-45), DateTime.UtcNow.AddHours(-44), "some folder", parentUid, offset, importedFileUid, "some file");

      var importedFilesList = new List<ImportedFile> { newImportedFile };

      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();

      var tRexImportFileProxy = new Mock<ITRexImportFileProxy>();
      tRexImportFileProxy.Setup(tr => tr.AddFile(It.IsAny<DesignRequest>(), It.IsAny<HeaderDictionary>())).ReturnsAsync(new ContractExecutionResult());
      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<CreateImportedFileEvent>())).ReturnsAsync(1);
      projectRepo.Setup(pr => pr.GetImportedFile(It.IsAny<string>())).ReturnsAsync(newImportedFile);
      projectRepo.Setup(pr => pr.GetImportedFiles(It.IsAny<string>())).ReturnsAsync(importedFilesList);

      var executor = RequestExecutorContainerFactory
        .Build<CreateImportedFileExecutor>(logger, null, serviceExceptionHandler,
          _customerUid.ToString(), _userUid.ToString(), _userEmailAddress, customHeaders,
          tRexImportFileProxy: tRexImportFileProxy.Object, projectRepo: projectRepo.Object);
      var result = await executor.ProcessAsync(createImportedFile).ConfigureAwait(false) as ImportedFileDescriptorSingleResult;
      Assert.NotNull(result);
      Assert.Equal(0, result.Code);
      Assert.NotNull(result.ImportedFileDescriptor);
      Assert.Equal(_projectUid.ToString(), result.ImportedFileDescriptor.ProjectUid);
      Assert.Equal(fileDescriptor.FileName, result.ImportedFileDescriptor.Name);
    }

    [Fact]
    public async Task CreateImportedFile_TRex_ReferenceSurface_NoParentDesign()
    {
      // FlowFile uploads the file from client (possibly as a background task via scheduler)
      // Controller uploads file to TCC and/or S3
      //    V2 Note: BCC file has already put the file on TCC.
      //          the controller a) copies within TCC to client project (raptor)
      //                         b) copies locally and hence to S3. (TRex)
      var customHeaders = new HeaderDictionary();
      var importedFileUid = Guid.NewGuid();
      var parentUid = Guid.NewGuid();
      var offset = 1.5;
      var TCCFilePath = "/BC Data/Sites/Chch Test Site";
      var fileName = "MoundRoad.ttm";
      var fileDescriptor = FileDescriptor.CreateFileDescriptor(_fileSpaceId, TCCFilePath, fileName);
      var fileCreatedUtc = DateTime.UtcNow.AddHours(-45);
      var fileUpdatedUtc = fileCreatedUtc;

      var newImportedFile = new ImportedFile
      {
        ProjectUid = _projectUid.ToString(),
        ImportedFileUid = importedFileUid.ToString(),
        ImportedFileId = 999,
        LegacyImportedFileId = 200000,
        ImportedFileType = ImportedFileType.ReferenceSurface,
        Name = fileDescriptor.FileName,
        FileDescriptor = JsonConvert.SerializeObject(fileDescriptor),
        Offset = offset,
        ParentUid = parentUid.ToString()
      };

      _ = new CreateImportedFileEvent
      {
        CustomerUID = _customerUid,
        ProjectUID = _projectUid,
        ImportedFileUID = importedFileUid,
        ImportedFileType = ImportedFileType.ReferenceSurface,
        DxfUnitsType = DxfUnitsType.Meters,
        Name = fileDescriptor.FileName,
        FileDescriptor = JsonConvert.SerializeObject(fileDescriptor),
        FileCreatedUtc = fileCreatedUtc,
        FileUpdatedUtc = fileUpdatedUtc,
        ImportedBy = string.Empty,
        SurveyedUTC = null,
        ParentUID = parentUid,
        Offset = offset,
        ActionUTC = DateTime.UtcNow
      };

      var createImportedFile = new CreateImportedFile(
        _projectUid, fileDescriptor.FileName, fileDescriptor, ImportedFileType.ReferenceSurface, null, DxfUnitsType.Meters,
        DateTime.UtcNow.AddHours(-45), DateTime.UtcNow.AddHours(-44), "some folder", parentUid, offset, importedFileUid, "some file");

      var importedFilesList = new List<ImportedFile> { newImportedFile };

      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();

      var tRexImportFileProxy = new Mock<ITRexImportFileProxy>();
      tRexImportFileProxy.Setup(tr => tr.AddFile(It.IsAny<DesignRequest>(), It.IsAny<HeaderDictionary>())).ReturnsAsync(new ContractExecutionResult());
      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<CreateImportedFileEvent>())).ReturnsAsync(1);
      projectRepo.Setup(pr => pr.GetImportedFile(newImportedFile.ImportedFileUid)).ReturnsAsync(newImportedFile);
      projectRepo.Setup(pr => pr.GetImportedFile(parentUid.ToString())).ReturnsAsync((ImportedFile)null);
      projectRepo.Setup(pr => pr.GetImportedFiles(It.IsAny<string>())).ReturnsAsync(importedFilesList);

      var executor = RequestExecutorContainerFactory
        .Build<CreateImportedFileExecutor>(logger, null, serviceExceptionHandler,
          _customerUid.ToString(), _userUid.ToString(), _userEmailAddress, customHeaders,
          tRexImportFileProxy: tRexImportFileProxy.Object, projectRepo: projectRepo.Object);
      await Assert.ThrowsAsync<ServiceException>(async () =>
       await executor.ProcessAsync(createImportedFile).ConfigureAwait(false));
    }

    [Fact]
    public async Task UpdateImportedFile_TRexHappyPath_ReferenceSurface()
    {
      var customHeaders = new HeaderDictionary();
      var importedFileUid = Guid.NewGuid();
      var parentUid = Guid.NewGuid();
      var oldOffset = 1.5;
      var newOffset = 1.5;
      var importedFileId = 9999;
      var TCCFilePath = "/BC Data/Sites/Chch Test Site";
      var fileName = "MoundRoadlinework.dxf";
      var fileDescriptor = FileDescriptor.CreateFileDescriptor(_fileSpaceId, TCCFilePath, fileName);

      var existingImportedFile = new ImportedFile
      {
        ProjectUid = _projectUid.ToString(),
        ImportedFileUid = importedFileUid.ToString(),
        LegacyImportedFileId = 200000,
        ImportedFileType = ImportedFileType.ReferenceSurface,
        Name = fileName,
        FileDescriptor = JsonConvert.SerializeObject(fileDescriptor),
        Offset = oldOffset,
        ParentUid = parentUid.ToString()
      };
      var importedFilesList = new List<ImportedFile> { existingImportedFile };
      var updateImportedFile = new UpdateImportedFile(
       _projectUid, _shortRaptorProjectId, ImportedFileType.ReferenceSurface, null, DxfUnitsType.Meters, DateTime.UtcNow.AddHours(-45),
       DateTime.UtcNow.AddHours(-44), fileDescriptor, importedFileUid, importedFileId, "some folder", newOffset, "some file"
      );

      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();

      var tRexImportFileProxy = new Mock<ITRexImportFileProxy>();
      tRexImportFileProxy.Setup(tr => tr.UpdateFile(It.IsAny<DesignRequest>(), It.IsAny<HeaderDictionary>())).ReturnsAsync(new ContractExecutionResult());
      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<UpdateImportedFileEvent>())).ReturnsAsync(1);
      projectRepo.Setup(pr => pr.GetImportedFile(It.IsAny<string>())).ReturnsAsync(existingImportedFile);
      projectRepo.Setup(pr => pr.GetImportedFiles(It.IsAny<string>())).ReturnsAsync(importedFilesList);

      var executor = RequestExecutorContainerFactory
        .Build<UpdateImportedFileExecutor>(logger, null, serviceExceptionHandler,
          _customerUid.ToString(), _userUid.ToString(), _userEmailAddress, customHeaders,
          tRexImportFileProxy: tRexImportFileProxy.Object, projectRepo: projectRepo.Object);
      var result = await executor.ProcessAsync(updateImportedFile).ConfigureAwait(false) as ImportedFileDescriptorSingleResult;
      Assert.Equal(0, result.Code);
      Assert.NotNull(result.ImportedFileDescriptor);
      Assert.Equal(_projectUid.ToString(), result.ImportedFileDescriptor.ProjectUid);
      Assert.Equal(fileDescriptor.FileName, result.ImportedFileDescriptor.Name);
      Assert.Equal(newOffset, result.ImportedFileDescriptor.Offset);
    }

    [Fact]
    public async Task DeleteImportedFile_TRexHappyPath_ReferenceSurface()
    {
      // FlowFile uploads the file from client (possibly as a background task via scheduler)
      // Controller uploads file to TCC and/or S3
      //    V2 Note: BCC file has already put the file on TCC.
      //          the controller a) copies within TCC to client project (raptor)
      //                         b) copies locally and hence to S3. (TRex)
      var customHeaders = new HeaderDictionary();
      var importedFileUid = Guid.NewGuid();
      var parentUid = Guid.NewGuid();
      var offset = 1.5;
      var importedFileId = 9999;
      var TCCFilePath = "/BC Data/Sites/Chch Test Site";
      var fileName = "MoundRoad.ttm";
      var fileDescriptor = FileDescriptor.CreateFileDescriptor(_fileSpaceId, TCCFilePath, fileName);

      var existingImportedFile = new ImportedFile
      {
        ProjectUid = _projectUid.ToString(),
        ImportedFileUid = importedFileUid.ToString(),
        LegacyImportedFileId = 200000,
        ImportedFileType = ImportedFileType.ReferenceSurface,
        Name = fileName,
        FileDescriptor = JsonConvert.SerializeObject(fileDescriptor),
        ParentUid = parentUid.ToString(),
        Offset = offset
      };

      var deleteImportedFile = new DeleteImportedFile(
        _projectUid, ImportedFileType.ReferenceSurface, fileDescriptor,
        importedFileUid, importedFileId, existingImportedFile.LegacyImportedFileId, "some folder", null
      );

      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();

      var tRexImportFileProxy = new Mock<ITRexImportFileProxy>();
      tRexImportFileProxy.Setup(tr => tr.DeleteFile(It.IsAny<DesignRequest>(), It.IsAny<HeaderDictionary>())).ReturnsAsync(new ContractExecutionResult());

      var filterServiceProxy = new Mock<IFilterServiceProxy>();
      filterServiceProxy.Setup(fs => fs.GetFilters(It.IsAny<string>(), It.IsAny<HeaderDictionary>()))
        .ReturnsAsync(new List<FilterDescriptor>().ToImmutableList);

      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<DeleteImportedFileEvent>())).ReturnsAsync(1);

      var dataOceanClient = new Mock<IDataOceanClient>();
      dataOceanClient.Setup(f => f.FileExists(It.IsAny<string>(), It.IsAny<HeaderDictionary>())).ReturnsAsync(true);
      dataOceanClient.Setup(f => f.DeleteFile(It.IsAny<string>(), It.IsAny<HeaderDictionary>())).ReturnsAsync(true);

      var authn = new Mock<ITPaaSApplicationAuthentication>();
      authn.Setup(a => a.GetApplicationBearerToken()).Returns("some token");

      var pegasusClient = new Mock<IPegasusClient>();

      var executor = RequestExecutorContainerFactory
        .Build<DeleteImportedFileExecutor>(
          logger, null, serviceExceptionHandler, _customerUid.ToString(), _userUid.ToString(), _userEmailAddress,
          customHeaders, filterServiceProxy: filterServiceProxy.Object, tRexImportFileProxy: tRexImportFileProxy.Object, 
          projectRepo: projectRepo.Object, dataOceanClient: dataOceanClient.Object, authn: authn.Object, pegasusClient: pegasusClient.Object);
      await executor.ProcessAsync(deleteImportedFile);
    }

    [Fact]
    public async Task DeleteImportedFile_TRex_DesignSurface_WithReferenceSurface()
    {
      // FlowFile uploads the file from client (possibly as a background task via scheduler)
      // Controller uploads file to TCC and/or S3
      //    V2 Note: BCC file has already put the file on TCC.
      //          the controller a) copies within TCC to client project (raptor)
      //                         b) copies locally and hence to S3. (TRex)
      var customHeaders = new HeaderDictionary();
      var importedFileUid = Guid.NewGuid();
      var parentUid = Guid.NewGuid();
      var offset = 1.5;
      var importedFileId = 9999;
      var TCCFilePath = "/BC Data/Sites/Chch Test Site";
      var fileName = "MoundRoad.ttm";
      var fileDescriptor = FileDescriptor.CreateFileDescriptor(_fileSpaceId, TCCFilePath, fileName);

      var referenceImportedFile = new ImportedFile
      {
        ProjectUid = _projectUid.ToString(),
        ImportedFileUid = importedFileUid.ToString(),
        LegacyImportedFileId = 200000,
        ImportedFileType = ImportedFileType.DesignSurface,
        Name = fileName,
        FileDescriptor = JsonConvert.SerializeObject(fileDescriptor),
      };

      var parentImportedFile = new ImportedFile
      {
        ProjectUid = _projectUid.ToString(),
        ImportedFileUid = parentUid.ToString(),
        ImportedFileId = 998,
        LegacyImportedFileId = 200001,
        ImportedFileType = ImportedFileType.DesignSurface,
        Name = fileDescriptor.FileName,
        FileDescriptor = JsonConvert.SerializeObject(fileDescriptor),
        ParentUid = parentUid.ToString(),
        Offset = offset
      };

      var deleteImportedFile = new DeleteImportedFile(
        _projectUid, ImportedFileType.DesignSurface, fileDescriptor,
        importedFileUid, importedFileId, parentImportedFile.LegacyImportedFileId, "some folder", null
      );

      var referenceList = new List<ImportedFile> { referenceImportedFile };

      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();

      var tRexImportFileProxy = new Mock<ITRexImportFileProxy>();
      tRexImportFileProxy.Setup(tr => tr.DeleteFile(It.IsAny<DesignRequest>(), It.IsAny<HeaderDictionary>())).ReturnsAsync(new ContractExecutionResult());

      var filterServiceProxy = new Mock<IFilterServiceProxy>();
      filterServiceProxy.Setup(fs => fs.GetFilters(It.IsAny<string>(), It.IsAny<HeaderDictionary>()))
        .ReturnsAsync(new List<FilterDescriptor>().ToImmutableList);

      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<DeleteImportedFileEvent>())).ReturnsAsync(1);
      projectRepo.Setup(pr => pr.GetReferencedImportedFiles(It.IsAny<string>())).ReturnsAsync(referenceList);

      var dataOceanClient = new Mock<IDataOceanClient>();
      dataOceanClient.Setup(f => f.FileExists(It.IsAny<string>(), It.IsAny<HeaderDictionary>())).ReturnsAsync(true);
      dataOceanClient.Setup(f => f.DeleteFile(It.IsAny<string>(), It.IsAny<HeaderDictionary>())).ReturnsAsync(true);

      var authn = new Mock<ITPaaSApplicationAuthentication>();
      authn.Setup(a => a.GetApplicationBearerToken()).Returns("some token");

      var pegasusClient = new Mock<IPegasusClient>();

      var executor = RequestExecutorContainerFactory
        .Build<DeleteImportedFileExecutor>(
          logger, null, serviceExceptionHandler, _customerUid.ToString(), _userUid.ToString(), _userEmailAddress,
          customHeaders,
          filterServiceProxy: filterServiceProxy.Object,
          tRexImportFileProxy: tRexImportFileProxy.Object, projectRepo: projectRepo.Object, dataOceanClient: dataOceanClient.Object, authn: authn.Object, pegasusClient: pegasusClient.Object);
      await Assert.ThrowsAsync<ServiceException>(async () =>
        await executor.ProcessAsync(deleteImportedFile).ConfigureAwait(false));
    }
  }
}
