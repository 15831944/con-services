﻿using System;
using System.Collections.Generic;
using MockProjectWebApi.Utils;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace MockProjectWebApi.Services
{
  public class ImportedFilesService : IImportedFilesService
  {
    public Dictionary<string, List<FileData>> ImportedFiles;
    public Dictionary<string, List<ProjectConfigurationModel>> ProjectConfigFiles;

    public ImportedFilesService()
    {
      CreateTestData();
      CreateCwsTestData();
    }

    private void CreateCwsTestData()
    {
      ProjectConfigFiles = new Dictionary<string, List<ProjectConfigurationModel>>();
      var dimensionsProjectConfigFiles = new List<ProjectConfigurationModel>
      {
        new ProjectConfigurationModel
        {
          FileName = "dimensions.dc",
          FileDownloadLink = "mock download link",
          FileType = ProjectConfigurationFileType.CALIBRATION.ToString()
        },
        new ProjectConfigurationModel
        {
          FileName = "dimensions.avoid.svl",
          FileDownloadLink = "mock download link",
          SiteCollectorFileName = "dimensions.avoid.dxf",
          SiteCollectorFileDownloadLink = "mock download link",
          FileType = ProjectConfigurationFileType.AVOIDANCE_ZONE.ToString()
        },
        new ProjectConfigurationModel
        {
          FileName = "dimensions.ggf",
          FileDownloadLink = "mock download link",
          FileType = ProjectConfigurationFileType.GEOID.ToString()
        }
      };
      ProjectConfigFiles.Add(ConstantsUtil.DIMENSIONS_PROJECT_UID, dimensionsProjectConfigFiles);
    }

    private void CreateTestData()
    {
      ImportedFiles = new Dictionary<string, List<FileData>>();

      var dimensionsImportedFiles = new List<FileData>
      {
        new FileData
          {
            Name = "CERA.bg.dxf",
            ProjectUid = ConstantsUtil.DIMENSIONS_PROJECT_UID,
            CustomerUid = "DxfTileAcceptanceTest",
            ImportedFileType = ImportedFileType.Linework,
            ImportedFileUid = "cfcd4c01-6fc8-45d5-872f-513a0f619f03",
            LegacyFileId = 1,
            IsActivated = true,
            MinZoomLevel = 15,
            MaxZoomLevel = 18
          },
          new FileData
          {
            Name = "Marylands_Metric.dxf",
            ProjectUid = ConstantsUtil.DIMENSIONS_PROJECT_UID,
            CustomerUid = "DxfTileAcceptanceTest",
            ImportedFileType = ImportedFileType.Linework,
            ImportedFileUid = "ea89be4b-0efb-4b8f-ba33-03f0973bfc7b",
            LegacyFileId = 2,
            IsActivated = true,
            MinZoomLevel = 18,
            MaxZoomLevel = 19
          },
          new FileData
          {
            Name = "Large Sites Road - Trimble Road.TTM",
            ProjectUid = ConstantsUtil.DIMENSIONS_PROJECT_UID,
            CustomerUid = "CutFillAcceptanceTest",
            ImportedFileType = ImportedFileType.DesignSurface,
            ImportedFileUid = "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff",
            LegacyFileId = 3,
            IsActivated = true,
            MinZoomLevel = 15,
            MaxZoomLevel = 20
          },
          new FileData
          {
            Name = "Large Sites Road - Trimble Road.TTM",
            ProjectUid = ConstantsUtil.DIMENSIONS_PROJECT_UID,
            CustomerUid = "CutFillAcceptanceTest",
            ImportedFileType = ImportedFileType.DesignSurface,
            ImportedFileUid = "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff",
            LegacyFileId = 111,
            IsActivated = true,
            MinZoomLevel = 15,
            MaxZoomLevel = 20
          },
          new FileData
          {
            Name = "Large Sites Road.svl",
            ProjectUid = ConstantsUtil.DIMENSIONS_PROJECT_UID,
            CustomerUid = "StationOffsetReportTest",
            ImportedFileType = ImportedFileType.Alignment,
            ImportedFileUid = "6ece671b-7959-4a14-86fa-6bfe6ef4dd62",
            LegacyFileId = 112,
            IsActivated = true,
            MinZoomLevel = 15,
            MaxZoomLevel = 17
          },
          new FileData
          {
            Name = "Topcon Road - Topcon Phil.svl",
            ProjectUid = ConstantsUtil.DIMENSIONS_PROJECT_UID,
            CustomerUid = "StationOffsetReportTest",
            ImportedFileType = ImportedFileType.Alignment,
            ImportedFileUid = "c6662be1-0f94-4897-b9af-28aeeabcd09b",
            LegacyFileId = 113,
            IsActivated = true,
            MinZoomLevel = 16,
            MaxZoomLevel = 18
          },
          new FileData
          {
            Name = "Milling - Milling.svl",
            ProjectUid = ConstantsUtil.DIMENSIONS_PROJECT_UID,
            CustomerUid = "StationOffsetReportTest",
            ImportedFileType = ImportedFileType.Alignment,
            ImportedFileUid = "3ead0c55-1e1f-4d30-aaf8-873526a2ab82",
            LegacyFileId = 114,
            IsActivated = true,
            MinZoomLevel = 15,
            MaxZoomLevel = 19
          },
          new FileData
          {
            Name = "Section 1 IFC Rev J.ttm",
            ProjectUid = ConstantsUtil.DIMENSIONS_PROJECT_UID,
            CustomerUid = "ImportFileProxyTest",
            ImportedFileType = ImportedFileType.DesignSurface,
            ImportedFileUid = "eb798b46-c927-4fdd-b998-b11011ee7365",
            LegacyFileId = 115,
            IsActivated = true,
            MinZoomLevel = 16,
            MaxZoomLevel = 19
          },
          new FileData
          {
            Name = "Large Sites Road - Trimble Road +0.5m",
            ProjectUid = ConstantsUtil.DIMENSIONS_PROJECT_UID,
            CustomerUid = "ImportFileProxyTest",
            ImportedFileType = ImportedFileType.ReferenceSurface,
            ImportedFileUid = "5642ec46-7aa4-4056-8785-c9534a06f54f",
            LegacyFileId = 116,
            IsActivated = true,
            MinZoomLevel = 16,
            MaxZoomLevel = 19,
            ParentUid = "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff",
            Offset = 0.5
          },
          new FileData
          {
            Name = "Large Sites Road - Trimble Road -0.75m",
            ProjectUid = ConstantsUtil.DIMENSIONS_PROJECT_UID,
            CustomerUid = "ImportFileProxyTest",
            ImportedFileType = ImportedFileType.ReferenceSurface,
            ImportedFileUid = "3bb94403-9d42-46ae-85e0-9261c8682a0d",
            LegacyFileId = 117,
            IsActivated = true,
            MinZoomLevel = 16,
            MaxZoomLevel = 19,
            ParentUid = "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff",
            Offset = -0.75
          },
          new FileData
          {
            Name = "Test 1.tif",
            ProjectUid = ConstantsUtil.DIMENSIONS_PROJECT_UID,
            CustomerUid = "DxfTileAcceptanceTest",
            ImportedFileType = ImportedFileType.GeoTiff,
            ImportedFileUid = "2cd59629-de6a-48e8-acfd-bf4c71624e34",
            LegacyFileId = 4,
            IsActivated = true,
            MinZoomLevel = 15,
            MaxZoomLevel = 19,
            SurveyedUtc = DateTime.Parse("2012-05-13T00:02:02")
          },
          new FileData
          {
            Name = "Test 1.tif",
            ProjectUid = ConstantsUtil.DIMENSIONS_PROJECT_UID,
            CustomerUid = "DxfTileAcceptanceTest",
            ImportedFileType = ImportedFileType.GeoTiff,
            ImportedFileUid = "83043e62-9177-46d7-acf0-26edd5281071",
            LegacyFileId = 5,
            IsActivated = true,
            MinZoomLevel = 15,
            MaxZoomLevel = 19,
            SurveyedUtc = DateTime.Parse("2012-05-29T11:13:17")
          },
          new FileData
          {
            Name = "Original Ground Survey - Dimensions.TTM",
            ProjectUid = ConstantsUtil.DIMENSIONS_PROJECT_UID,
            CustomerUid = "SurveyedSurfaceAcceptanceTest",
            ImportedFileType = ImportedFileType.SurveyedSurface,
            ImportedFileUid = "15ef852f-497b-418f-99d8-39a3e6e8b1c7",
            LegacyFileId = 15188,
            IsActivated = true,
            MinZoomLevel = 15,
            MaxZoomLevel = 19,
            SurveyedUtc = DateTime.Parse("2020-09-10T23:21:27")
          }
      };

      ImportedFiles.Add(ConstantsUtil.DIMENSIONS_PROJECT_UID, dimensionsImportedFiles);

      var importedFilesGoldenData1 = new List<FileData>();
      importedFilesGoldenData1.AddRange(surveyedSurfacesFileListIsActivated);
      importedFilesGoldenData1.AddRange(goldenDataDesignSurfaceFileList);
      importedFilesGoldenData1.AddRange(goldenDataReferenceSurfaceFileList);

      ImportedFiles.Add(ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1, importedFilesGoldenData1);

      ImportedFiles.Add(ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_2, surveyedSurfacesFileList);

      // For an alignment file with negative start station...
      var alignmentImportedFiles = new List<FileData>
      {
        new FileData
        {
          Name = "Negative Start Station.svl",
          ProjectUid = ConstantsUtil.CHRISTCHURCH_TEST_SITE_PROJECT_UID,
          CustomerUid = "NegativeStartStationAlignmentTest",
          ImportedFileType = ImportedFileType.Alignment,
          ImportedFileUid = "7816b21b-33a3-499e-888a-a3d449e8b596",
          LegacyFileId = 118,
          IsActivated = true,
          MinZoomLevel = 15,
          MaxZoomLevel = 19
        }
      };

      ImportedFiles.Add(ConstantsUtil.CHRISTCHURCH_TEST_SITE_PROJECT_UID, alignmentImportedFiles);
    }

    private readonly List<FileData> surveyedSurfacesFileListIsActivated = new List<FileData>
      {
        new FileData
        {
          Name = "Original Ground Survey - Dimensions 2012_2016-05-13T000202Z.TTM",
          ProjectUid = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1,
          CustomerUid = "SurveyedSurfaceAcceptanceTest",
          ImportedFileType = ImportedFileType.SurveyedSurface,
          ImportedFileUid = "ff323224-f2ab-4af6-b4bc-95dd0903c003",
          LegacyFileId = 14177,
          IsActivated = true,
          MinZoomLevel = 0,
          MaxZoomLevel = 0,
          SurveyedUtc = DateTime.Parse("2012-05-13T00:02:02")
        },
        new FileData
        {
          Name = "Large Sites Road - Trimble Road_2016-05-13T000000Z.TTM",
          ProjectUid = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1,
          CustomerUid = "SurveyedSurfaceAcceptanceTest",
          ImportedFileType = ImportedFileType.SurveyedSurface,
          ImportedFileUid = "4f9bebe8-812b-4552-9af6-1ddfb2f813ed",
          LegacyFileId = 14176,
          IsActivated = true,
          MinZoomLevel = 0,
          MaxZoomLevel = 0
        },
        new FileData
        {
          Name = "Milling - Milling_2016-05-08T234647Z.TTM",
          ProjectUid = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1,
          CustomerUid = "SurveyedSurfaceAcceptanceTest",
          ImportedFileType = ImportedFileType.SurveyedSurface,
          ImportedFileUid = "dcb41fbd-7d43-4b36-a144-e22bbccc24a8",
          LegacyFileId = 14175,
          IsActivated = true,
          MinZoomLevel = 0,
          MaxZoomLevel = 0,
          SurveyedUtc = DateTime.Parse("2016-05-08T23:46:47")
        },
        new FileData
        {
          Name = "Large Sites Road - Trimble Road_2016-05-08T234455Z.TTM",
          ProjectUid = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1,
          CustomerUid = "SurveyedSurfaceAcceptanceTest",
          ImportedFileType = ImportedFileType.SurveyedSurface,
          ImportedFileUid = "0372718b-534a-430f-bb71-dc71acb9bd5b",
          LegacyFileId = 14174,
          IsActivated = true,
          MinZoomLevel = 0,
          MaxZoomLevel = 0
        },
        new FileData
        {
          Name = "Large Sites Road - Trimble Road_2012-06-01T015500Z.TTM",
          ProjectUid = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1,
          CustomerUid = "SurveyedSurfaceAcceptanceTest",
          ImportedFileType = ImportedFileType.SurveyedSurface,
          ImportedFileUid = "0db110ed-8dc2-487a-901c-0ea5de6fd8dd",
          LegacyFileId = 14222,
          IsActivated = true,
          MinZoomLevel = 0,
          MaxZoomLevel = 0
        }
      };

    private readonly List<FileData> surveyedSurfacesFileList = new List<FileData>
      {
        new FileData
        {
          Name = "Original Ground Survey - Dimensions 2012_2016-05-13T000202Z.TTM",
          ProjectUid = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_2.ToString(),
          CustomerUid = "SurveyedSurfaceAcceptanceTest",
          ImportedFileType = ImportedFileType.SurveyedSurface,
          ImportedFileUid = "ff323224-f2ab-4af6-b4bc-95dd0903c003",
          LegacyFileId = 14177,
          IsActivated = false,
          MinZoomLevel = 0,
          MaxZoomLevel = 0,
          SurveyedUtc = DateTime.Parse("2012-05-13T00:02:02")
        },
        new FileData
        {
          Name = "Large Sites Road - Trimble Road_2016-05-13T000000Z.TTM",
          ProjectUid = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_2.ToString(),
          CustomerUid = "SurveyedSurfaceAcceptanceTest",
          ImportedFileType = ImportedFileType.SurveyedSurface,
          ImportedFileUid = "4f9bebe8-812b-4552-9af6-1ddfb2f813ed",
          LegacyFileId = 14176,
          IsActivated = false,
          MinZoomLevel = 0,
          MaxZoomLevel = 0
        },
        new FileData
        {
          Name = "Milling - Milling_2016-05-08T234647Z.TTM",
          ProjectUid = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_2.ToString(),
          CustomerUid = "SurveyedSurfaceAcceptanceTest",
          ImportedFileType = ImportedFileType.SurveyedSurface,
          ImportedFileUid = "dcb41fbd-7d43-4b36-a144-e22bbccc24a8",
          LegacyFileId = 14175,
          IsActivated = false,
          MinZoomLevel = 0,
          MaxZoomLevel = 0,
          SurveyedUtc = DateTime.Parse("2016-05-08T23:46:47")
        },
        new FileData
        {
          Name = "Large Sites Road - Trimble Road_2016-05-08T234455Z.TTM",
          ProjectUid = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_2.ToString(),
          CustomerUid = "SurveyedSurfaceAcceptanceTest",
          ImportedFileType = ImportedFileType.SurveyedSurface,
          ImportedFileUid = "0372718b-534a-430f-bb71-dc71acb9bd5b",
          LegacyFileId = 14174,
          IsActivated = false,
          MinZoomLevel = 0,
          MaxZoomLevel = 0
        },
        new FileData
        {
          Name = "Large Sites Road - Trimble Road_2012-06-01T015500Z.TTM",
          ProjectUid = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_2.ToString(),
          CustomerUid = "SurveyedSurfaceAcceptanceTest",
          ImportedFileType = ImportedFileType.SurveyedSurface,
          ImportedFileUid = "0db110ed-8dc2-487a-901c-0ea5de6fd8dd",
          LegacyFileId = 14222,
          IsActivated = false,
          MinZoomLevel = 0,
          MaxZoomLevel = 0
        }
      };

    private readonly List<FileData> goldenDataDesignSurfaceFileList = new List<FileData>
      {
        new FileData
        {
          Name = "Original Ground Survey - Dimensions 2012.TTM",
          ProjectUid = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1,
          CustomerUid = "DesignSurfaceAcceptanceTest",
          ImportedFileType = ImportedFileType.DesignSurface,
          ImportedFileUid = "3d255208-8aa2-4172-9046-f97a36eff896",
          LegacyFileId = 15177,
          IsActivated = true,
          MinZoomLevel = 15,
          MaxZoomLevel = 19
        },
        new FileData
        {
          Name = "Large Sites Road - Trimble Road.TTM",
          ProjectUid = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1,
          CustomerUid = "DesignSurfaceAcceptanceTest",
          ImportedFileType = ImportedFileType.DesignSurface,
          ImportedFileUid = "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff",
          LegacyFileId = 15176,
          IsActivated = true,
          MinZoomLevel = 15,
          MaxZoomLevel = 18
        },
        new FileData
        {
          Name = "Milling - Milling.TTM",
          ProjectUid = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1,
          CustomerUid = "DesignSurfaceAcceptanceTest",
          ImportedFileType = ImportedFileType.DesignSurface,
          ImportedFileUid = "220e12e5-ce92-4645-8f01-1942a2d5a57f",
          LegacyFileId = 15175,
          IsActivated = true,
          MinZoomLevel = 16,
          MaxZoomLevel = 17
        },
        new FileData
        {
          Name = "Topcon Road - Topcon.TTM",
          ProjectUid = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1,
          CustomerUid = "DesignSurfaceAcceptanceTest",
          ImportedFileType = ImportedFileType.DesignSurface,
          ImportedFileUid = "ea97efb9-c0c4-4a7f-9eee-e2b0ef0b0916",
          LegacyFileId = 15174,
          IsActivated = true,
          MinZoomLevel = 16,
          MaxZoomLevel = 18
        },
        new FileData
        {
          Name = "Trimble Command Centre.TTM",
          ProjectUid = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1,
          CustomerUid = "DesignSurfaceAcceptanceTest",
          ImportedFileType = ImportedFileType.DesignSurface,
          ImportedFileUid = Guid.NewGuid().ToString(),
          LegacyFileId = 15222,
          IsActivated = true,
          MinZoomLevel = 19,
          MaxZoomLevel = 20
        }
      };

    private readonly List<FileData> goldenDataReferenceSurfaceFileList = new List<FileData>
      {
        new FileData
        {
          Name = "Large Sites Road - Trimble Road +0.5m",
          ProjectUid = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1,
          CustomerUid = "ReferenceSurfaceAcceptanceTest",
          ImportedFileType = ImportedFileType.ReferenceSurface,
          ImportedFileUid = "c91e56cf-6d5f-436d-9655-cf4f919523f4",
          LegacyFileId = 15277,
          IsActivated = true,
          MinZoomLevel = 15,
          MaxZoomLevel = 18,
          ParentUid = "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff",
          Offset = 0.5
        },
        new FileData
        {
          Name = "Large Sites Road - Trimble Road +0.75m",
          ProjectUid = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1,
          CustomerUid = "ReferenceSurfaceAcceptanceTest",
          ImportedFileType = ImportedFileType.ReferenceSurface,
          ImportedFileUid = "1fdb413d-7521-4efc-9916-73d82b9de366",
          LegacyFileId = 15276,
          IsActivated = true,
          MinZoomLevel = 15,
          MaxZoomLevel = 18,
          ParentUid = "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff",
          Offset = 0.75
        },
        new FileData
        {
          Name = "Milling - Milling -0.75m",
          ProjectUid = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1,
          CustomerUid = "ReferenceSurfaceAcceptanceTest",
          ImportedFileType = ImportedFileType.ReferenceSurface,
          ImportedFileUid = "bf0c5759-337a-4721-941b-5349462c15a3",
          LegacyFileId = 15275,
          IsActivated = true,
          MinZoomLevel = 16,
          MaxZoomLevel = 17,
          ParentUid = "220e12e5-ce92-4645-8f01-1942a2d5a57f",
          Offset = -0.75
        },
        new FileData
        {
          Name = "Topcon Road - Topcon -0.25m",
          ProjectUid = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1,
          CustomerUid = "ReferenceSurfaceAcceptanceTest",
          ImportedFileType = ImportedFileType.ReferenceSurface,
          ImportedFileUid = "54c905e2-3bc0-4a17-9ef3-5ddcb6030a1d",
          LegacyFileId = 15274,
          IsActivated = true,
          MinZoomLevel = 16,
          MaxZoomLevel = 18,
          ParentUid = "ea97efb9-c0c4-4a7f-9eee-e2b0ef0b0916",
          Offset = -0.25
        }
      };
  }
}
