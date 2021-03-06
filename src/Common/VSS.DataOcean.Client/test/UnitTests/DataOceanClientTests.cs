﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies.Interfaces;
using Xunit;
using Moq;
using System.IO;
using VSS.DataOcean.Client.ResultHandling;
using VSS.DataOcean.Client.Models;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Caching.Memory;
using Serilog;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Cache.MemoryCache;
using VSS.Serilog.Extensions;

namespace VSS.DataOcean.Client.UnitTests
{
  public class DataOceanClientTests
  {
    private readonly IServiceProvider serviceProvider;
    private readonly IServiceCollection serviceCollection;

    public DataOceanClientTests()
    {
      serviceCollection = new ServiceCollection()
        .AddLogging()
        .AddSingleton(new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.DataOcean.Client.UnitTests.log")))
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .AddSingleton<IMemoryCache, MemoryCache>()
        .AddSingleton<IDataCache, InMemoryDataCache>();

      //This is real one to be added in services using DataOcean client. We mock it below for unit tests.
      //serviceCollection.AddSingleton<IWebRequest, GracefulWebRequest>();
      serviceCollection.AddSingleton<IDataOceanClient, DataOceanClient>();

      serviceProvider = serviceCollection.BuildServiceProvider();
    }

    [Fact]
    public async Task CanCheckTopLevelFolderExists()
    {
      var config = serviceProvider.GetRequiredService<IConfigurationStore>();
      var dataOceanBaseUrl = config.GetValueString("DATA_OCEAN_URL");
      var dataOceanRootFolderId = config.GetValueString("DATA_OCEAN_ROOT_FOLDER_ID");
      const string folderName = "unittest";
      var expectedFolderResult = new DataOceanDirectory { Id = Guid.NewGuid(), Name = folderName, ParentId = Guid.Parse(dataOceanRootFolderId) };
      var expectedBrowseResult =
        new BrowseDirectoriesResult { Directories = new List<DataOceanDirectory> { expectedFolderResult } };

      var browseUrl = $"{dataOceanBaseUrl}/api/browse/keyset_directories?name={folderName}&owner=true&parent_id={dataOceanRootFolderId}";

      var gracefulMock = new Mock<IWebRequest>();
      gracefulMock.Setup(g => g.ExecuteRequest<BrowseDirectoriesResult>(browseUrl, null, null, HttpMethod.Get, null, 0, false))
        .Returns(Task.FromResult(expectedBrowseResult));

      serviceCollection.AddTransient(g => gracefulMock.Object);
      var serviceProvider2 = serviceCollection.BuildServiceProvider();
      var client = serviceProvider2.GetRequiredService<IDataOceanClient>();
      var success = await client.FolderExists($"{DataOceanUtil.PathSeparator}{dataOceanRootFolderId}{DataOceanUtil.PathSeparator}{folderName}", null);
      Assert.True(success);
    }

    [Fact]
    public async Task CanCheckSubFolderExists()
    {
      var config = serviceProvider.GetRequiredService<IConfigurationStore>();
      var dataOceanBaseUrl = config.GetValueString("DATA_OCEAN_URL");
      var dataOceanRootFolderId = config.GetValueString("DATA_OCEAN_ROOT_FOLDER_ID");
      const string topLevelFolderName = "unittest";
      var expectedTopFolderResult = new DataOceanDirectory { Id = Guid.NewGuid(), Name = topLevelFolderName, ParentId = Guid.Parse(dataOceanRootFolderId) };
      var expectedTopBrowseResult =
        new BrowseDirectoriesResult { Directories = new List<DataOceanDirectory> { expectedTopFolderResult } };
      const string subFolderName = "anything";
      var expectedSubFolderResult = new DataOceanDirectory
      {
        Id = Guid.NewGuid(),
        Name = subFolderName,
        ParentId = expectedTopFolderResult.Id
      };
      var expectedSubBrowseResult =
        new BrowseDirectoriesResult { Directories = new List<DataOceanDirectory> { expectedSubFolderResult } };

      var browseTopUrl = $"{dataOceanBaseUrl}/api/browse/keyset_directories?name={topLevelFolderName}&owner=true&parent_id={dataOceanRootFolderId}";
      var browseSubUrl = $"{dataOceanBaseUrl}/api/browse/keyset_directories?name={subFolderName}&owner=true&parent_id={expectedSubFolderResult.Id}";

      var gracefulMock = new Mock<IWebRequest>();
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseDirectoriesResult>(browseTopUrl, null, null, HttpMethod.Get, null, 0, false))
        .Returns(Task.FromResult(expectedTopBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseDirectoriesResult>(browseSubUrl, null, null, HttpMethod.Get, null, 0, false))
        .Returns(Task.FromResult(expectedSubBrowseResult));

      serviceCollection.AddTransient(g => gracefulMock.Object);
      var serviceProvider2 = serviceCollection.BuildServiceProvider();
      var client = serviceProvider2.GetRequiredService<IDataOceanClient>();
      var success =
        await client.FolderExists(
          $"{DataOceanUtil.PathSeparator}{dataOceanRootFolderId}{DataOceanUtil.PathSeparator}{topLevelFolderName}{DataOceanUtil.PathSeparator}{subFolderName}", null);
      Assert.True(success);
    }

    [Fact]
    public async Task CanCheckFolderDoesNotExist()
    {
      const string folderName = "unittest";
      var expectedBrowseResult = new BrowseDirectoriesResult { Directories = new List<DataOceanDirectory>() };

      var config = serviceProvider.GetRequiredService<IConfigurationStore>();
      var dataOceanBaseUrl = config.GetValueString("DATA_OCEAN_URL");
      var browseUrl = $"{dataOceanBaseUrl}/api/browse/keyset_directories?name={folderName}&owner=true";

      var gracefulMock = new Mock<IWebRequest>();
      gracefulMock.Setup(g => g.ExecuteRequest<BrowseDirectoriesResult>(browseUrl, null, null, HttpMethod.Get, null, 0, false))
        .Returns(Task.FromResult(expectedBrowseResult));

      serviceCollection.AddTransient(g => gracefulMock.Object);
      var serviceProvider2 = serviceCollection.BuildServiceProvider();
      var client = serviceProvider2.GetRequiredService<IDataOceanClient>();
      var success = await client.FolderExists($"{DataOceanUtil.PathSeparator}{folderName}", null);
      Assert.False(success);
    }

    [Fact]
    public async Task CanCheckFileExists()
    {
      var config = serviceProvider.GetRequiredService<IConfigurationStore>();
      var dataOceanBaseUrl = config.GetValueString("DATA_OCEAN_URL");
      var dataOceanRootFolderId = config.GetValueString("DATA_OCEAN_ROOT_FOLDER_ID");
      var folderName = "unittest";
      var expectedFolderResult = new DataOceanDirectory { Id = Guid.NewGuid(), Name = folderName, ParentId = Guid.Parse(dataOceanRootFolderId) };
      var expectedFolderBrowseResult =
        new BrowseDirectoriesResult { Directories = new List<DataOceanDirectory> { expectedFolderResult } };

      const string fileName = "dummy.dxf";
      var expectedFileResult = new DataOceanFile { Id = Guid.NewGuid(), Name = fileName, ParentId = expectedFolderResult.Id };
      var expectedFileBrowseResult = new BrowseFilesResult { Files = new List<DataOceanFile> { expectedFileResult } };

      var browseFolderUrl = $"{dataOceanBaseUrl}/api/browse/keyset_directories?name={folderName}&owner=true&parent_id={dataOceanRootFolderId}";
      var browseFileUrl = $"{dataOceanBaseUrl}/api/browse/keyset_files?name={fileName}&owner=true&parent_id={expectedFolderResult.Id}";

      var gracefulMock = new Mock<IWebRequest>();
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseDirectoriesResult>(browseFolderUrl, null, null, HttpMethod.Get, null, 0, false))
        .Returns(Task.FromResult(expectedFolderBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseFilesResult>(browseFileUrl, null, null, HttpMethod.Get, null, 0, false))
        .Returns(Task.FromResult(expectedFileBrowseResult));

      serviceCollection.AddTransient(g => gracefulMock.Object);
      var serviceProvider2 = serviceCollection.BuildServiceProvider();
      var client = serviceProvider2.GetRequiredService<IDataOceanClient>();
      var success =
        await client.FileExists($"{DataOceanUtil.PathSeparator}{dataOceanRootFolderId}{DataOceanUtil.PathSeparator}{folderName}{DataOceanUtil.PathSeparator}{fileName}",null);
      Assert.True(success);
    }

    [Fact]
    public async Task CanCheckFileDoesNotExist()
    {
      const string folderName = "unittest";
      var expectedFolderResult = new DataOceanDirectory { Id = Guid.NewGuid(), Name = folderName };
      var expectedFolderBrowseResult =
        new BrowseDirectoriesResult { Directories = new List<DataOceanDirectory> { expectedFolderResult } };

      const string fileName = "dummy.dxf";
      var expectedFileBrowseResult = new BrowseFilesResult { Files = new List<DataOceanFile>() };

      var config = serviceProvider.GetRequiredService<IConfigurationStore>();
      var dataOceanBaseUrl = config.GetValueString("DATA_OCEAN_URL");
      var browseFolderUrl = $"{dataOceanBaseUrl}/api/browse/keyset_directories?name={folderName}&owner=true";
      var browseFileUrl = $"{dataOceanBaseUrl}/api/browse/keyset_files?name={fileName}&owner=true&parent_id={expectedFolderResult.Id}";

      var gracefulMock = new Mock<IWebRequest>();
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseDirectoriesResult>(browseFolderUrl, null, null, HttpMethod.Get, null, 0, false))
        .Returns(Task.FromResult(expectedFolderBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseFilesResult>(browseFileUrl, null, null, HttpMethod.Get, null, 0, false))
        .Returns(Task.FromResult(expectedFileBrowseResult));

      serviceCollection.AddTransient(g => gracefulMock.Object);
      var serviceProvider2 = serviceCollection.BuildServiceProvider();
      var client = serviceProvider2.GetRequiredService<IDataOceanClient>();
      var success =
        await client.FileExists($"{DataOceanUtil.PathSeparator}{folderName}{DataOceanUtil.PathSeparator}{fileName}",null);
      Assert.False(success);
    }

    [Fact]
    public async Task CanCreateTopLevelFolder()
    {
      var config = serviceProvider.GetRequiredService<IConfigurationStore>();
      var dataOceanBaseUrl = config.GetValueString("DATA_OCEAN_URL");
      var dataOceanRootFolderId = config.GetValueString("DATA_OCEAN_ROOT_FOLDER_ID");
      const string folderName = "unittest";
      var expectedFolderResult = new DataOceanDirectory { Id = Guid.NewGuid(), Name = folderName, ParentId = Guid.Parse(dataOceanRootFolderId) };
      var expectedBrowseResult = new BrowseDirectoriesResult { Directories = new List<DataOceanDirectory>() };

      var browseUrl = $"{dataOceanBaseUrl}/api/browse/keyset_directories?name={folderName}&owner=true&parent_id={dataOceanRootFolderId}";
      var createUrl = $"{dataOceanBaseUrl}/api/directories";

      var gracefulMock = new Mock<IWebRequest>();
      gracefulMock.Setup(g => g.ExecuteRequest<BrowseDirectoriesResult>(browseUrl, null, null, HttpMethod.Get, null, 0, false))
        .Returns(Task.FromResult(expectedBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequest<DataOceanDirectoryResult>(createUrl, It.IsAny<MemoryStream>(), null, HttpMethod.Post, null, 0,
          false)).ReturnsAsync(new DataOceanDirectoryResult { Directory = expectedFolderResult });

      serviceCollection.AddTransient(g => gracefulMock.Object);
      var serviceProvider2 = serviceCollection.BuildServiceProvider();
      var client = serviceProvider2.GetRequiredService<IDataOceanClient>();
      var success = await client.MakeFolder($"{DataOceanUtil.PathSeparator}{dataOceanRootFolderId}{DataOceanUtil.PathSeparator}{folderName}", null);
      Assert.True(success);

      //Check it also succeeds when the folder already exists
      expectedBrowseResult.Directories = new List<DataOceanDirectory> { expectedFolderResult };
      success = await client.MakeFolder($"{DataOceanUtil.PathSeparator}{dataOceanRootFolderId}{DataOceanUtil.PathSeparator}{folderName}", null);
      Assert.True(success);
    }

    [Fact]
    public async Task CanCreateSubFolder()
    {
      var config = serviceProvider.GetRequiredService<IConfigurationStore>();
      var dataOceanBaseUrl = config.GetValueString("DATA_OCEAN_URL");
      var dataOceanRootFolderId = config.GetValueString("DATA_OCEAN_ROOT_FOLDER_ID"); 
      const string topLevelFolderName = "unittest";
      var expectedTopFolderResult = new DataOceanDirectory { Id = Guid.NewGuid(), Name = topLevelFolderName, ParentId = Guid.Parse(dataOceanRootFolderId) };
      var expectedTopBrowseResult =
        new BrowseDirectoriesResult { Directories = new List<DataOceanDirectory> { expectedTopFolderResult } };
      const string subFolderName = "anything";
      var expectedSubFolderResult = new DataOceanDirectory
      {
        Id = Guid.NewGuid(),
        Name = subFolderName,
        ParentId = expectedTopFolderResult.Id
      };
      var expectedSubBrowseResult = new BrowseDirectoriesResult { Directories = new List<DataOceanDirectory>() };

      var browseTopUrl = $"{dataOceanBaseUrl}/api/browse/keyset_directories?name={topLevelFolderName}&owner=true&parent_id={dataOceanRootFolderId}";
      var browseSubUrl = $"{dataOceanBaseUrl}/api/browse/keyset_directories?name={subFolderName}&owner=true&parent_id={expectedSubFolderResult.Id}";
      var createUrl = $"{dataOceanBaseUrl}/api/directories";

      var gracefulMock = new Mock<IWebRequest>();
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseDirectoriesResult>(browseTopUrl, null, null, HttpMethod.Get, null, 0, false))
        .Returns(Task.FromResult(expectedTopBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseDirectoriesResult>(browseSubUrl, null, null, HttpMethod.Get, null, 0, false))
        .Returns(Task.FromResult(expectedSubBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequest<DataOceanDirectoryResult>(createUrl, It.IsAny<MemoryStream>(), null, HttpMethod.Post, null, 0,
          false)).ReturnsAsync(new DataOceanDirectoryResult { Directory = expectedSubFolderResult });

      serviceCollection.AddTransient(g => gracefulMock.Object);
      var serviceProvider2 = serviceCollection.BuildServiceProvider();
      var client = serviceProvider2.GetRequiredService<IDataOceanClient>();
      var success =
        await client.MakeFolder(
          $"{DataOceanUtil.PathSeparator}{dataOceanRootFolderId}{DataOceanUtil.PathSeparator}{topLevelFolderName}{DataOceanUtil.PathSeparator}{subFolderName}", null);
      Assert.True(success);

      //Check it also succeeds when the folder already exists
      expectedSubBrowseResult.Directories = new List<DataOceanDirectory> { expectedSubFolderResult };
      success = await client.MakeFolder(
        $"{DataOceanUtil.PathSeparator}{dataOceanRootFolderId}{DataOceanUtil.PathSeparator}{dataOceanRootFolderId}{DataOceanUtil.PathSeparator}{topLevelFolderName}{DataOceanUtil.PathSeparator}{subFolderName}", null);
      Assert.True(success);
    }

    [Fact]
    public async Task CanPutFileSuccess() => Assert.True(await CanPutFile("AVAILABLE"));

    [Fact]
    public async Task CanPutFileUploadFailed() => Assert.False(await CanPutFile("UPLOAD_FAILED"));

    [Fact]
    public async Task CanPutFileTimeout() => Assert.False(await CanPutFile("UPLOADABLE"));

    [Fact]
    public async Task CanDeleteExistingFile()
    {
      var config = serviceProvider.GetRequiredService<IConfigurationStore>();
      var dataOceanBaseUrl = config.GetValueString("DATA_OCEAN_URL");
      var dataOceanRootFolderId = config.GetValueString("DATA_OCEAN_ROOT_FOLDER_ID");
      const string folderName = "unittest";
      var expectedFolderResult = new DataOceanDirectory { Id = Guid.NewGuid(), Name = folderName, ParentId = Guid.Parse(dataOceanRootFolderId) };
      var expectedFolderBrowseResult =
        new BrowseDirectoriesResult { Directories = new List<DataOceanDirectory> { expectedFolderResult } };

      const string fileName = "dummy.dxf";
      var expectedFileResult = new DataOceanFile { Id = Guid.NewGuid(), Name = fileName, ParentId = expectedFolderResult.Id };
      var expectedFileBrowseResult = new BrowseFilesResult { Files = new List<DataOceanFile> { expectedFileResult } };

      var browseFolderUrl = $"{dataOceanBaseUrl}/api/browse/keyset_directories?name={folderName}&owner=true&parent_id={dataOceanRootFolderId}";
      var browseFileUrl = $"{dataOceanBaseUrl}/api/browse/keyset_files?name={fileName}&owner=true&parent_id={expectedFolderResult.Id}";
      var deleteFileUrl = $"{dataOceanBaseUrl}/api/files/{expectedFileResult.Id}";

      var gracefulMock = new Mock<IWebRequest>();
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseDirectoriesResult>(browseFolderUrl, null, null, HttpMethod.Get, null, 0, false))
        .Returns(Task.FromResult(expectedFolderBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseFilesResult>(browseFileUrl, null, null, HttpMethod.Get, null, 0, false))
        .Returns(Task.FromResult(expectedFileBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequest(deleteFileUrl, null, null, HttpMethod.Delete, null, 0, false))
        .Returns(Task.FromResult(HttpStatusCode.Accepted));

      serviceCollection.AddTransient(g => gracefulMock.Object);
      var serviceProvider2 = serviceCollection.BuildServiceProvider();
      var client = serviceProvider2.GetRequiredService<IDataOceanClient>();
      var success =
        await client.DeleteFile($"{DataOceanUtil.PathSeparator}{dataOceanRootFolderId}{DataOceanUtil.PathSeparator}{folderName}{DataOceanUtil.PathSeparator}{fileName}",null);
      Assert.True(success);
    }

    [Fact]
    public async Task CanDeleteNonExistingFile()
    {
      const string folderName = "unittest";
      var expectedFolderResult = new DataOceanDirectory { Id = Guid.NewGuid(), Name = folderName };
      var expectedFolderBrowseResult =
        new BrowseDirectoriesResult { Directories = new List<DataOceanDirectory> { expectedFolderResult } };

      const string fileName = "dummy.dxf";
      var expectedFileBrowseResult = new BrowseFilesResult { Files = new List<DataOceanFile>() };

      var config = serviceProvider.GetRequiredService<IConfigurationStore>();
      var dataOceanBaseUrl = config.GetValueString("DATA_OCEAN_URL");
      var browseFolderUrl = $"{dataOceanBaseUrl}/api/browse/keyset_directories?name={folderName}&owner=true";
      var browseFileUrl = $"{dataOceanBaseUrl}/api/browse/keyset_files?name={fileName}&owner=true&parent_id={expectedFolderResult.Id}";

      var gracefulMock = new Mock<IWebRequest>();
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseDirectoriesResult>(browseFolderUrl, null, null, HttpMethod.Get, null, 0, false))
        .Returns(Task.FromResult(expectedFolderBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseFilesResult>(browseFileUrl, null, null, HttpMethod.Get, null, 0, false))
        .Returns(Task.FromResult(expectedFileBrowseResult));

      serviceCollection.AddTransient(g => gracefulMock.Object);
      var serviceProvider2 = serviceCollection.BuildServiceProvider();
      var client = serviceProvider2.GetRequiredService<IDataOceanClient>();
      var success =
        await client.DeleteFile($"{DataOceanUtil.PathSeparator}{folderName}{DataOceanUtil.PathSeparator}{fileName}",null);
      Assert.False(success);
    }

    [Fact]
    public async Task CanGetExistingFolderId()
    {
      var config = serviceProvider.GetRequiredService<IConfigurationStore>();
      var dataOceanBaseUrl = config.GetValueString("DATA_OCEAN_URL");
      var dataOceanRootFolderId = config.GetValueString("DATA_OCEAN_ROOT_FOLDER_ID");
      const string folderName = "unittest";
      var expectedFolderResult = new DataOceanDirectory { Id = Guid.NewGuid(), Name = folderName, ParentId = Guid.Parse(dataOceanRootFolderId) };
      var expectedBrowseResult =
        new BrowseDirectoriesResult { Directories = new List<DataOceanDirectory> { expectedFolderResult } };

      var browseUrl = $"{dataOceanBaseUrl}/api/browse/keyset_directories?name={folderName}&owner=true&parent_id={dataOceanRootFolderId}";

      var gracefulMock = new Mock<IWebRequest>();
      gracefulMock.Setup(g => g.ExecuteRequest<BrowseDirectoriesResult>(browseUrl, null, null, HttpMethod.Get, null, 0, false))
        .Returns(Task.FromResult(expectedBrowseResult));

      serviceCollection.AddTransient(g => gracefulMock.Object);
      var serviceProvider2 = serviceCollection.BuildServiceProvider();
      var client = serviceProvider2.GetRequiredService<IDataOceanClient>();
      var Id = await client.GetFolderId($"{DataOceanUtil.PathSeparator}{dataOceanRootFolderId}{DataOceanUtil.PathSeparator}{folderName}", null);
      Assert.Equal(expectedFolderResult.Id, Id);
    }

    [Fact]
    public async Task CanGetNonExistingFolderId()
    {
      const string folderName = "unittest";
      var expectedBrowseResult = new BrowseDirectoriesResult { Directories = new List<DataOceanDirectory>() };

      var config = serviceProvider.GetRequiredService<IConfigurationStore>();
      var dataOceanBaseUrl = config.GetValueString("DATA_OCEAN_URL");
      var browseUrl = $"{dataOceanBaseUrl}/api/browse/keyset_directories?name={folderName}&owner=true";

      var gracefulMock = new Mock<IWebRequest>();
      gracefulMock.Setup(g => g.ExecuteRequest<BrowseDirectoriesResult>(browseUrl, null, null, HttpMethod.Get, null, 0, false))
        .Returns(Task.FromResult(expectedBrowseResult));

      serviceCollection.AddTransient(g => gracefulMock.Object);
      var serviceProvider2 = serviceCollection.BuildServiceProvider();
      var client = serviceProvider2.GetRequiredService<IDataOceanClient>();
      var Id = await client.GetFolderId($"{DataOceanUtil.PathSeparator}{folderName}", null);
      Assert.Null(Id);
    }

    [Fact]
    public async Task CanGetExistingFileId()
    {
      var config = serviceProvider.GetRequiredService<IConfigurationStore>();
      var dataOceanBaseUrl = config.GetValueString("DATA_OCEAN_URL");
      var dataOceanRootFolderId = config.GetValueString("DATA_OCEAN_ROOT_FOLDER_ID");
      const string folderName = "unittest";
      var expectedFolderResult = new DataOceanDirectory { Id = Guid.NewGuid(), Name = folderName, ParentId = Guid.Parse(dataOceanRootFolderId) };
      var expectedFolderBrowseResult =
        new BrowseDirectoriesResult { Directories = new List<DataOceanDirectory> { expectedFolderResult } };

      const string fileName = "dummy.dxf";
      var expectedFileResult = new DataOceanFile { Id = Guid.NewGuid(), Name = fileName, ParentId = expectedFolderResult.Id };
      var expectedFileBrowseResult = new BrowseFilesResult { Files = new List<DataOceanFile> { expectedFileResult } };

      var browseFolderUrl = $"{dataOceanBaseUrl}/api/browse/keyset_directories?name={folderName}&owner=true&parent_id={dataOceanRootFolderId}";
      var browseFileUrl = $"{dataOceanBaseUrl}/api/browse/keyset_files?name={fileName}&owner=true&parent_id={expectedFolderResult.Id}";

      var gracefulMock = new Mock<IWebRequest>();
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseDirectoriesResult>(browseFolderUrl, null, null, HttpMethod.Get, null, 0, false))
        .Returns(Task.FromResult(expectedFolderBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseFilesResult>(browseFileUrl, null, null, HttpMethod.Get, null, 0, false))
        .Returns(Task.FromResult(expectedFileBrowseResult));

      serviceCollection.AddTransient(g => gracefulMock.Object);
      var serviceProvider2 = serviceCollection.BuildServiceProvider();
      var client = serviceProvider2.GetRequiredService<IDataOceanClient>();
      var Id =
        await client.GetFileId($"{DataOceanUtil.PathSeparator}{dataOceanRootFolderId}{DataOceanUtil.PathSeparator}{folderName}{DataOceanUtil.PathSeparator}{fileName}",null);
      Assert.Equal(expectedFileResult.Id, Id);
    }

    [Fact]
    public async Task CanGetNonExistingFileId()
    {
      const string folderName = "unittest";
      var expectedFolderResult = new DataOceanDirectory { Id = Guid.NewGuid(), Name = folderName };
      var expectedFolderBrowseResult =
        new BrowseDirectoriesResult { Directories = new List<DataOceanDirectory> { expectedFolderResult } };

      const string fileName = "dummy.dxf";
      var expectedFileBrowseResult = new BrowseFilesResult { Files = new List<DataOceanFile>() };

      var config = serviceProvider.GetRequiredService<IConfigurationStore>();
      var dataOceanBaseUrl = config.GetValueString("DATA_OCEAN_URL");
      var browseFolderUrl = $"{dataOceanBaseUrl}/api/browse/keyset_directories?name={folderName}&owner=true";
      var browseFileUrl = $"{dataOceanBaseUrl}/api/browse/keyset_files?name={fileName}&owner=true&parent_id={expectedFolderResult.Id}";

      var gracefulMock = new Mock<IWebRequest>();
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseDirectoriesResult>(browseFolderUrl, null, null, HttpMethod.Get, null, 0, false))
        .Returns(Task.FromResult(expectedFolderBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseFilesResult>(browseFileUrl, null, null, HttpMethod.Get, null, 0, false))
        .Returns(Task.FromResult(expectedFileBrowseResult));

      serviceCollection.AddTransient(g => gracefulMock.Object);
      var serviceProvider2 = serviceCollection.BuildServiceProvider();
      var client = serviceProvider2.GetRequiredService<IDataOceanClient>();
      var Id =
        await client.GetFileId($"{DataOceanUtil.PathSeparator}{folderName}{DataOceanUtil.PathSeparator}{fileName}",null);
      Assert.Null(Id);
    }

    [Fact]
    public async Task CanGetExistingSingleFile()
    {
      var expectedResult = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3 };
      var stream = new MemoryStream(expectedResult);
      var expectedDownloadResult = new StreamContent(stream);

      var config = serviceProvider.GetRequiredService<IConfigurationStore>();
      var dataOceanBaseUrl = config.GetValueString("DATA_OCEAN_URL");
      var dataOceanRootFolderId = config.GetValueString("DATA_OCEAN_ROOT_FOLDER_ID");
      const string folderName = "unittest";
      var expectedFolderResult = new DataOceanDirectory { Id = Guid.NewGuid(), Name = folderName, ParentId = Guid.Parse(dataOceanRootFolderId) };
      var expectedFolderBrowseResult =
        new BrowseDirectoriesResult { Directories = new List<DataOceanDirectory> { expectedFolderResult } };

      const string fileName = "dummy.dxf";
      var expectedFileResult = new DataOceanFile
      {
        Id = Guid.NewGuid(),
        Name = fileName,
        ParentId = expectedFolderResult.Id,
        Multifile = false,
        RegionPreferences = new List<string> { "us1" },
        Status = "AVAILABLE",
        DataOceanDownload = new DataOceanTransfer { Url = TestConstants.DownloadUrl }
      };
      var expectedFileBrowseResult = new BrowseFilesResult { Files = new List<DataOceanFile> { expectedFileResult } };

      var browseFolderUrl = $"{dataOceanBaseUrl}/api/browse/keyset_directories?name={folderName}&owner=true&parent_id={dataOceanRootFolderId}";
      var browseFileUrl = $"{dataOceanBaseUrl}/api/browse/keyset_files?name={fileName}&owner=true&parent_id={expectedFolderResult.Id}";

      var gracefulMock = new Mock<IWebRequest>();
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseDirectoriesResult>(browseFolderUrl, null, null, HttpMethod.Get, null, 0, false))
        .Returns(Task.FromResult(expectedFolderBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseFilesResult>(browseFileUrl, null, null, HttpMethod.Get, null, 0, false))
        .Returns(Task.FromResult(expectedFileBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequestAsStreamContent(TestConstants.DownloadUrl, HttpMethod.Get, null, null, null, 0, false))
        .ReturnsAsync(expectedDownloadResult);

      serviceCollection.AddTransient(g => gracefulMock.Object);
      var serviceProvider2 = serviceCollection.BuildServiceProvider();
      var client = serviceProvider2.GetRequiredService<IDataOceanClient>();

      var resultStream = await client.GetFile($"{DataOceanUtil.PathSeparator}{dataOceanRootFolderId}{DataOceanUtil.PathSeparator}{folderName}{DataOceanUtil.PathSeparator}{fileName}",null);
      using (var ms = new MemoryStream())
      {
        resultStream.CopyTo(ms);
        var result = ms.ToArray();
        Assert.Equal(expectedResult, result);
      }
    }

    [Fact]
    public async Task CanGetNonExistingSingleFile()
    {
      const string folderName = "unittest";
      var expectedFolderResult = new DataOceanDirectory { Id = Guid.NewGuid(), Name = folderName };
      var expectedFolderBrowseResult =
        new BrowseDirectoriesResult { Directories = new List<DataOceanDirectory> { expectedFolderResult } };

      const string fileName = "dummy.dxf";
      var expectedFileBrowseResult = new BrowseFilesResult { Files = new List<DataOceanFile>() };

      var config = serviceProvider.GetRequiredService<IConfigurationStore>();
      var dataOceanBaseUrl = config.GetValueString("DATA_OCEAN_URL");
      var browseFolderUrl = $"{dataOceanBaseUrl}/api/browse/keyset_directories?name={folderName}&owner=true";
      var browseFileUrl = $"{dataOceanBaseUrl}/api/browse/keyset_files?name={fileName}&owner=true&parent_id={expectedFolderResult.Id}";

      var gracefulMock = new Mock<IWebRequest>();
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseDirectoriesResult>(browseFolderUrl, null, null, HttpMethod.Get, null, 0, false))
        .Returns(Task.FromResult(expectedFolderBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseFilesResult>(browseFileUrl, null, null, HttpMethod.Get, null, 0, false))
        .Returns(Task.FromResult(expectedFileBrowseResult));

      serviceCollection.AddTransient(g => gracefulMock.Object);
      var serviceProvider2 = serviceCollection.BuildServiceProvider();
      var client = serviceProvider2.GetRequiredService<IDataOceanClient>();

      var resultStream = await client.GetFile($"{DataOceanUtil.PathSeparator}{folderName}{DataOceanUtil.PathSeparator}{fileName}",null);
      Assert.Null(resultStream);
    }

    [Fact]
    public async Task CanGetExistingMultiFile()
    {
      var fileName = $"{DataOceanUtil.PathSeparator}tiles{DataOceanUtil.PathSeparator}tiles.json";
      var downloadUrl = TestConstants.DownloadUrl;
      var substitutedDownloadUrl = downloadUrl.Replace("{path}", fileName.Substring(1));
      var expectedResult = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3 };
      var stream = new MemoryStream(expectedResult);
      var expectedDownloadResult = new StreamContent(stream);

      var config = serviceProvider.GetRequiredService<IConfigurationStore>();
      var dataOceanBaseUrl = config.GetValueString("DATA_OCEAN_URL");
      var dataOceanRootFolderId = config.GetValueString("DATA_OCEAN_ROOT_FOLDER_ID");
      const string folderName = "unittest";
      var expectedFolderResult = new DataOceanDirectory { Id = Guid.NewGuid(), Name = folderName, ParentId = Guid.Parse(dataOceanRootFolderId) };
      var expectedFolderBrowseResult =
        new BrowseDirectoriesResult { Directories = new List<DataOceanDirectory> { expectedFolderResult } };

      const string multiFileName = "dummy.dxf_Tiles$";
      var fileUid = Guid.NewGuid();
      var updatedAt = DateTime.UtcNow.AddHours(-2);
      var expectedFileResult = new DataOceanFile
      {
        Id = fileUid,
        Name = multiFileName,
        ParentId = expectedFolderResult.Id,
        Multifile = true,
        RegionPreferences = new List<string> { "us1" },
        Status = "AVAILABLE",
        DataOceanDownload = new DataOceanTransfer { Url = downloadUrl },
        UpdatedAt = updatedAt
      };
      var otherFileResult = new DataOceanFile
      {
        Id = fileUid,
        Name = multiFileName,
        ParentId = expectedFolderResult.Id,
        Multifile = true,
        RegionPreferences = new List<string> { "us1" },
        Status = "AVAILABLE",
        DataOceanDownload = new DataOceanTransfer { Url = downloadUrl },
        UpdatedAt = updatedAt.AddHours(-5)
      };
      var expectedFileBrowseResult = new BrowseFilesResult { Files = new List<DataOceanFile> { expectedFileResult, otherFileResult } };

      var browseFolderUrl = $"{dataOceanBaseUrl}/api/browse/keyset_directories?name={folderName}&owner=true&parent_id={dataOceanRootFolderId}";
      var browseFileUrl = $"{dataOceanBaseUrl}/api/browse/keyset_files?name={multiFileName}&owner=true&parent_id={expectedFolderResult.Id}";

      var gracefulMock = new Mock<IWebRequest>();
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseDirectoriesResult>(browseFolderUrl, null, null, HttpMethod.Get, null, 0, false))
        .Returns(Task.FromResult(expectedFolderBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseFilesResult>(browseFileUrl, null, null, HttpMethod.Get, null, 0, false))
        .Returns(Task.FromResult(expectedFileBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequestAsStreamContent(substitutedDownloadUrl, HttpMethod.Get, null, null, null, 0, false))
        .ReturnsAsync(expectedDownloadResult);

      serviceCollection.AddTransient(g => gracefulMock.Object);
      var serviceProvider2 = serviceCollection.BuildServiceProvider();
      var client = serviceProvider2.GetRequiredService<IDataOceanClient>();

      var fullFileName = $"{DataOceanUtil.PathSeparator}{dataOceanRootFolderId}{DataOceanUtil.PathSeparator}{folderName}{DataOceanUtil.PathSeparator}{multiFileName}{fileName}";
      var resultStream = await client.GetFile(fullFileName, null);
      using (var ms = new MemoryStream())
      {
        resultStream.CopyTo(ms);
        var result = ms.ToArray();
        Assert.Equal(expectedResult, result);
      }
    }

    [Fact]
    public async Task CanGetNonExistingMultiFile()
    {
      var fileName = $"{DataOceanUtil.PathSeparator}tiles{DataOceanUtil.PathSeparator}15{DataOceanUtil.PathSeparator}18756{DataOceanUtil.PathSeparator}2834.png";
      var downloadUrl = TestConstants.DownloadUrl;
      var substitutedDownloadUrl = downloadUrl.Replace("{path}", fileName.Substring(1));

      const string folderName = "unittest";
      var expectedFolderResult = new DataOceanDirectory { Id = Guid.NewGuid(), Name = folderName };
      var expectedFolderBrowseResult =
        new BrowseDirectoriesResult { Directories = new List<DataOceanDirectory> { expectedFolderResult } };

      const string multiFileName = "dummy.dxf_Tiles$";
      var expectedFileResult = new DataOceanFile
      {
        Id = Guid.NewGuid(),
        Name = multiFileName,
        ParentId = expectedFolderResult.Id,
        Multifile = true,
        RegionPreferences = new List<string> { "us1" },
        Status = "AVAILABLE",
        DataOceanDownload = new DataOceanTransfer { Url = downloadUrl }
      };
      var expectedFileBrowseResult = new BrowseFilesResult { Files = new List<DataOceanFile> { expectedFileResult } };

      var config = serviceProvider.GetRequiredService<IConfigurationStore>();
      var dataOceanBaseUrl = config.GetValueString("DATA_OCEAN_URL");
      var browseFolderUrl = $"{dataOceanBaseUrl}/api/browse/keyset_directories?name={folderName}&owner=true";
      var browseFileUrl = $"{dataOceanBaseUrl}/api/browse/keyset_files?name={multiFileName}&owner=true&parent_id={expectedFolderResult.Id}";

      var gracefulMock = new Mock<IWebRequest>();
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseDirectoriesResult>(browseFolderUrl, null, null, HttpMethod.Get, null, 0, false))
        .Returns(Task.FromResult(expectedFolderBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseFilesResult>(browseFileUrl, null, null, HttpMethod.Get, null, 0, false))
        .Returns(Task.FromResult(expectedFileBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequestAsStreamContent(substitutedDownloadUrl, HttpMethod.Get, null, null, null, 0, false))
        .ReturnsAsync((HttpContent)null);

      serviceCollection.AddTransient(g => gracefulMock.Object);
      var serviceProvider2 = serviceCollection.BuildServiceProvider();
      var client = serviceProvider2.GetRequiredService<IDataOceanClient>();

      var fullFileName = $"{DataOceanUtil.PathSeparator}{folderName}{DataOceanUtil.PathSeparator}{multiFileName}{fileName}";
      var resultStream = await client.GetFile(fullFileName, null);
      Assert.Null(resultStream);
    }

    private Task<bool> CanPutFile(string status)
    {
      const string fileName = "dummy.dxf";
      var expectedFile = new DataOceanFile
      {
        Id = Guid.NewGuid(),
        Name = fileName,
        Multifile = false,
        RegionPreferences = new List<string> { "us1" },
        Status = status,
        DataOceanUpload = new DataOceanTransfer { Url = TestConstants.UploadUrl }
      };
      var expectedFileResult = new DataOceanFileResult { File = expectedFile };
      var folderName = $"{DataOceanUtil.PathSeparator}";
      var expectedBrowseResult = new BrowseDirectoriesResult { Directories = new List<DataOceanDirectory>() };
      var expectedUploadResult = new StringContent("some ok result");

      var config = serviceProvider.GetRequiredService<IConfigurationStore>();
      var dataOceanBaseUrl = config.GetValueString("DATA_OCEAN_URL");
      var browseUrl = $"{dataOceanBaseUrl}/api/browse/keyset_directories?name={folderName}&owner=true";
      var createUrl = $"{dataOceanBaseUrl}/api/files";
      var getUrl = $"{createUrl}/{expectedFile.Id}";
      var deleteFileUrl = $"{dataOceanBaseUrl}/api/files/{expectedFileResult.File.Id}";

      var gracefulMock = new Mock<IWebRequest>();
      gracefulMock.Setup(g => g.ExecuteRequest<BrowseDirectoriesResult>(browseUrl, null, null, HttpMethod.Get, null, 0, false))
      .Returns(Task.FromResult(expectedBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequest<DataOceanFileResult>(createUrl, It.IsAny<MemoryStream>(), null, HttpMethod.Post, null, 0, false))
        .ReturnsAsync(expectedFileResult);
      gracefulMock
        .Setup(g => g.ExecuteRequestAsStreamContent(TestConstants.UploadUrl, HttpMethod.Put, null, It.IsAny<Stream>(), null, 0, false))
        .ReturnsAsync(expectedUploadResult);
      gracefulMock.Setup(g => g.ExecuteRequest<DataOceanFileResult>(getUrl, null, null, HttpMethod.Get, null, 0, false))
        .Returns(Task.FromResult(expectedFileResult));
      gracefulMock
        .Setup(g => g.ExecuteRequest(deleteFileUrl, null, null, HttpMethod.Delete, null, 0, false))
        .Returns(Task.FromResult(HttpStatusCode.Accepted));

      serviceCollection.AddTransient(g => gracefulMock.Object);
      var serviceProvider2 = serviceCollection.BuildServiceProvider();
      var client = serviceProvider2.GetRequiredService<IDataOceanClient>();

      return client.PutFile(folderName, fileName, null);
    }
  }
}
