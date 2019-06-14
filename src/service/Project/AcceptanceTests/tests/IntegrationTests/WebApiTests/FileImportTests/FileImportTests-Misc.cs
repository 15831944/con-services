﻿using System;
using System.Net.Http;
using IntegrationTests.UtilityClasses;
using TestUtility;
using VSS.MasterData.Project.WebAPI.Common.Models;
using Xunit;

namespace IntegrationTests.WebApiTests.FileImportTests
{
  public class FileImportTests
  {
    [Theory]
    [InlineData("api/v4/importedfile")]
    [InlineData("api/v4/importedfile/direct")]
    public void TestNoFileUploads(string uriRoot)
    {
      const string testName = "File Import 1";
      Msg.Title(testName, "Create standard project and customer then get imported files - There should be none.");
      var ts = new TestSupport();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");
      
      var eventsArray = new[] {
       "| TableName           | EventDate   | CustomerUID   | Name       | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
      $"| Customer            | 0d+09:00:00 | {customerUid} | {testName} | 1                 |                   |                |                  |             |                |               |          |                    |",
      $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |            |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
      $"| Subscription        | 0d+09:10:00 |               |            |                   | {subscriptionUid} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                    |",
      $"| ProjectSubscription | 0d+09:20:00 |               |            |                   |                   |                |                  | {startDate} |                | {projectUid}  |          | {subscriptionUid}  |"};
      ts.PublishEventCollection(eventsArray);

      var projectsArray = new[] {
         "| TableName           | EventDate   | ProjectUID   | LegacyProjectID   | Name       | fk_ProjectTypeID | ProjectTimeZone           | LandfillTimeZone          | StartDate   | EndDate   | GeometryWKT   |",
        $"| Project             | 0d+09:10:00 | {projectUid} | {TestSupport.GenerateLegacyProjectId()} | {testName} | 0                | New Zealand Standard Time | New Zealand Standard Time | {startDate} | {endDate} | {Boundaries.Boundary1} |"};
      
      ts.PublishEventCollection(projectsArray);
      
      var importFile = new ImportFile(uriRoot);
      var expectedResults = importFile.ExpectedImportFileDescriptorsListResult;
      var filesResult = importFile.GetImportedFilesFromWebApi<ImportedFileDescriptorListResult>($"api/v4/importedfiles?projectUid={projectUid}", customerUid);
      
      Assert.True(filesResult.ImportedFileDescriptors.Count == expectedResults.ImportedFileDescriptors.Count, " Expected number of fields does not match actual");
      Assert.Equal(expectedResults.ImportedFileDescriptors, filesResult.ImportedFileDescriptors);
    }

    [Theory(Skip ="Cannot run until we have a a TRex system or mock.")]
    [InlineData("api/v4/importedfile")]
    public void ManualTRexTest_CreateImportedFile(string uriRoot)
    {
      const string testName = "File Import 11";
      Msg.Title(testName, "Create standard project then upload a new design surface file.");
      var ts = new TestSupport();
      var importFile = new ImportFile(uriRoot);
      var trexService = new TRex();
      var legacyProjectId = TestSupport.GenerateLegacyProjectId();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");

      var eventsArray = new[]
      {
        "| TableName           | EventDate   | CustomerUID   | Name       | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
        $"| Customer            | 0d+09:00:00 | {customerUid} | {testName} | 1                 |                   |                |                  |             |                |               |          |                    |",
        $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |            |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
        $"| Subscription        | 0d+09:10:00 |               |            |                   | {subscriptionUid} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                    |",
        $"| ProjectSubscription | 0d+09:20:00 |               |            |                   |                   |                |                  | {startDate} |                | {projectUid}  |          | {subscriptionUid}  |"
      };
      ts.PublishEventCollection(eventsArray);

      ts.IsPublishToWebApi = true;
      var projectEventArray = new[]
      {
        "| EventType          | EventDate   | ProjectUID   | ProjectID         | ProjectName | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | Description |",
        $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {legacyProjectId} | {testName}  | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} | {legacyProjectId} |false      | BootCampDimensions.dc | {testName}  |"
      };
      ts.PublishEventCollection(projectEventArray);

      var importFilename = TestFileResolver.File(TestFile.TestDesignSurface3_GoodContent);
      var fullFilePath = TestFileResolver.GetFullPath(importFilename);

      var importFileArray = new[]
      {
        "| EventType              | ProjectUid   | CustomerUid   | Name           | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc              | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel |",
        $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {fullFilePath} | 1                | {startDateTime} | {startDateTime.AddDays(5)}  | testProjectMDM@trimble.com | true        | 15           | 19           |"
      };
      var filesResult = importFile.SendRequestToFileImportV4(ts, importFileArray, 1,
        new ImportOptions(HttpMethod.Post, new[] {"filename=TestDesignSurfaceTestDesignSurface3_GoodContent.TTM"}));
      Assert.NotNull(filesResult);
      Assert.Equal(0, filesResult.Code);
      Assert.Equal(projectUid, filesResult.ImportedFileDescriptor.ProjectUid);

      var designsResult = trexService.GetDesignsFromTrex(customerUid.ToString(), projectUid);
      Assert.Equal(0, designsResult.Code);
      Assert.Single(designsResult.DesignFileDescriptors);
      Assert.Equal(filesResult.ImportedFileDescriptor.ImportedFileUid, designsResult.DesignFileDescriptors[0].DesignUid);
      Assert.Equal("TestDesignSurface3_GoodContent.TTM", designsResult.DesignFileDescriptors[0].Name);
    }

    /// <summary>
    /// The goal of this method is to validate that the FileDescriptor of an upsert ImportedFile object can be deserialized successfully
    /// "and doesn't contain invalid JSON; i.e. the JSON is not escaped.
    /// </summary>
    [Theory]
    [InlineData("api/v4/importedfile")]
    public void TestImportANewFileThenUpdateTheFileThenDeleteTheFile(string uriRoot)
    {
      const string testName = "File insert, upsert & delete";
      Msg.Title(testName, "Inset a new file, upsert a new version and then delete the file");

      var ts = new TestSupport();
      var importFile = new ImportFile(uriRoot);
      var legacyProjectId = TestSupport.GenerateLegacyProjectId();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");
      
      var eventsArray = new[] {
        "| TableName           | EventDate   | CustomerUID   | Name       | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
       $"| Customer            | 0d+09:00:00 | {customerUid} | {testName} | 1                 |                   |                |                  |             |                |               |          |                    |",
       $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |            |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
       $"| Subscription        | 0d+09:10:00 |               |            |                   | {subscriptionUid} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                    |",
       $"| ProjectSubscription | 0d+09:20:00 |               |            |                   |                   |                |                  | {startDate} |                | {projectUid}  |          | {subscriptionUid}  |"};
      ts.PublishEventCollection(eventsArray);

      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
        "| EventType          | EventDate   | ProjectUID   | ProjectID         | ProjectName | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | Description |",
       $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {legacyProjectId} | {testName}  | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} | {legacyProjectId} |false      | BootCampDimensions.dc | {testName}  |"};
      ts.PublishEventCollection(projectEventArray);

      var importFilename = TestFileResolver.File(TestFile.TestDesignSurface1);
      var fullFilePath = TestFileResolver.GetFullPath(importFilename);

      var importFileArray = new[] {
       "| EventType              | ProjectUid   | CustomerUid   | Name           | ImportedFileType | FileCreatedUtc              | FileUpdatedUtc              | ImportedBy                 | SurveyedUtc     | IsActivated | MinZoomLevel | MaxZoomLevel |",
      $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {fullFilePath} | 2                | {startDateTime}             | {startDateTime.AddDays(5)}  | testProjectMDM@trimble.com | {startDateTime} | true        | 0            | 0            |",
      $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {fullFilePath} | 2                | {startDateTime.AddDays(10)} | {startDateTime.AddDays(10)} | testProjectMDM@trimble.com | {startDateTime} | true        | 0            | 0            |"};
      var filesResult = importFile.SendRequestToFileImportV4(ts, importFileArray, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={importFilename}" }));
      var expectedResult1 = importFile.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor;
      ts.CompareTheActualImportFileWithExpected(filesResult.ImportedFileDescriptor, expectedResult1, true);

      _ = importFile.SendRequestToFileImportV4(ts, importFileArray, 2, new ImportOptions(HttpMethod.Put, new[] { $"filename={importFilename}" }));
      var expectedResult2 = importFile.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor;
      var importFileList = importFile.GetImportedFilesFromWebApi<ImportedFileDescriptorListResult>($"api/v4/importedfiles?projectUid={projectUid}", customerUid);

      Assert.True(importFileList.ImportedFileDescriptors.Count == 1, "Expected 1 imported files but got " + importFileList.ImportedFileDescriptors.Count);
      ts.CompareTheActualImportFileWithExpectedV4(importFileList.ImportedFileDescriptors[0], expectedResult2, true);

      importFile.ImportedFileUid = filesResult.ImportedFileDescriptor.ImportedFileUid;

      var deleteResult = importFile.SendRequestToFileImportV4(ts, importFileArray, 1, new ImportOptions(HttpMethod.Delete));
      Assert.Equal(0, deleteResult.Code);
      Assert.Equal("success", deleteResult.Message);
    }
  }
}