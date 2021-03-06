﻿using System;
using System.Collections.Generic;
using MockProjectWebApi.Utils;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Productivity3D.Project.Abstractions.Models;

namespace MockProjectWebApi.Services
{
  public class ProjectService : IProjectService
  {
    public List<ProjectData> ProjectList;

    public ProjectService()
    {
      CreateTestData();
    }

    private void CreateTestData()
    {
      ProjectList = new List<ProjectData>
      {
        new ProjectData {ProjectUID = Guid.NewGuid().ToString()},
        new ProjectData {ProjectUID = Guid.NewGuid().ToString()},
        new ProjectData {ProjectUID = Guid.NewGuid().ToString()},
        new ProjectData {ProjectUID = Guid.NewGuid().ToString()},
        new ProjectData {ProjectUID = Guid.NewGuid().ToString()},
        new ProjectData {ProjectUID = "dc509939-88b5-49b6-8c2c-9e8131122e96", ProjectType = CwsProjectType.AcceptsTagFiles},
        new ProjectData {ProjectUID = Guid.NewGuid().ToString()},
        new ProjectData {ProjectUID = Guid.NewGuid().ToString()},
        new ProjectData {ProjectUID = Guid.NewGuid().ToString()},
        new ProjectData {ProjectUID = "b14bb927-3c10-47b2-b958-4ce7aabbc594", ProjectType = CwsProjectType.AcceptsTagFiles, ProjectGeofenceWKT = "POLYGON((6.96461375644884 46.250301540882, 6.96643887353764 46.2509268520462, 6.97460415600528 46.2477169036207, 6.97269423208211 46.2470325441392, 6.96461375644884 46.250301540882))"},
        new ProjectData {
          ProjectUID = ConstantsUtil.DIMENSIONS_PROJECT_UID,
          ProjectType = CwsProjectType.AcceptsTagFiles,
          Name = "DimensionsProject",
          ProjectTimeZone = "Pacific Standard Time",
          IanaTimeZone = "America/Los_Angeles",
          CustomerUID = ConstantsUtil.DIMENSIONS_CUSTOMER_UID,
          ProjectGeofenceWKT = "POLYGON((-115.025723657623 36.2101347890754,-115.026281557098 36.2056332151707,-115.018041811005 36.205460072542,-115.017698488251 36.2102040420362, -115.025723657623 36.2101347890754))",
          IsArchived = false
          
        },
        new ProjectData {
          ProjectUID = ConstantsUtil.DIMENSIONS_EMPTY_PROJECT_UID, 
          ProjectType = CwsProjectType.AcceptsTagFiles,
          ProjectTimeZone = "Pacific Standard Time",
          IanaTimeZone = "America/Los_Angeles",
          ProjectGeofenceWKT = "POLYGON((-115.025723657623 36.2101347890754,-115.026281557098 36.2056332151707,-115.018041811005 36.205460072542,-115.017698488251 36.2102040420362, -115.025723657623 36.2101347890754))"
        },
        new ProjectData {
          ProjectUID = ConstantsUtil.CUSTOM_SETTINGS_DIMENSIONS_PROJECT_UID, ProjectType = CwsProjectType.AcceptsTagFiles
        },
        new ProjectData {ProjectUID = "a2cb39c7-95a0-4bb1-845f-cb1052467e98", ProjectType = CwsProjectType.AcceptsTagFiles, ProjectTimeZone = "W. Europe Standard Time"},
        new ProjectData {ProjectUID = Guid.NewGuid().ToString()},
        new ProjectData {ProjectUID = "8590b7fc-079e-4b5a-b5ff-8514dadfe985", ProjectType = CwsProjectType.AcceptsTagFiles},
        new ProjectData {ProjectUID = Guid.NewGuid().ToString()},
        new ProjectData {ProjectUID = Guid.NewGuid().ToString()},
        new ProjectData {
          ProjectUID = "d0a0410e-9fcc-44b1-bf1a-378c891d2ddb", ProjectType = CwsProjectType.AcceptsTagFiles,
          ProjectGeofenceWKT = "POLYGON((3.010411822 -0.759611604,3.010913674 -0.759617032,3.010916659 -0.759813626,3.010414824 -0.759812544))",
        },

        new ProjectData {ProjectUID = "8aed6003-b8eb-47b1-941f-096a17468bf0", ProjectType = CwsProjectType.AcceptsTagFiles},
        new ProjectData {ProjectUID = Guid.NewGuid().ToString()},
        new ProjectData {ProjectUID = "04c94921-6343-4ffb-9d35-db9d281743fc", ProjectType = CwsProjectType.AcceptsTagFiles},
        new ProjectData {ProjectUID = Guid.NewGuid().ToString()},
        new ProjectData {ProjectUID = Guid.NewGuid().ToString()},
        new ProjectData {ProjectUID = Guid.NewGuid().ToString()},
        new ProjectData {
          ProjectUID = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1, 
          ProjectType = CwsProjectType.AcceptsTagFiles,
          ProjectTimeZone = "Mountain Standard Time",
          IanaTimeZone = "America/Creston",
          ProjectGeofenceWKT = "POLYGON((-115.025723657623 36.2101347890754,-115.026281557098 36.2056332151707,-115.018041811005 36.205460072542,-115.017698488251 36.2102040420362,-115.025723657623 36.2101347890754))"
        },
        new ProjectData {ProjectUID = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_2, ProjectType = CwsProjectType.AcceptsTagFiles},
        new ProjectData {ProjectUID = Guid.NewGuid().ToString()},
        new ProjectData {ProjectUID = Guid.NewGuid().ToString()},
        new ProjectData {ProjectUID = Guid.NewGuid().ToString()},
        new ProjectData
        {
          ProjectUID = ConstantsUtil.CHRISTCHURCH_TEST_SITE_PROJECT_UID,
          ProjectType = CwsProjectType.AcceptsTagFiles,
          ProjectTimeZone = "New Zealand Standard Time",
          IsArchived = false
        },
        new ProjectData {
          ProjectUID = "b7f4af55-2fdb-4878-b3d0-ce748d5dde08",
          ProjectType = CwsProjectType.AcceptsTagFiles,
          IsArchived = true
        },
        new ProjectData
        {
          ProjectUID = "0583c019-f92a-4762-94c1-ad13a98ecab6",
          ProjectType = CwsProjectType.AcceptsTagFiles,
          Name = "Kettlewell Drive - Golden DS",
          ProjectGeofenceWKT = "POLYGON((172.437121306867 -43.5121438699137,172.438280021162 -43.5160497338425,172.442228232831 -43.5154584240685,172.44089785716 -43.5114747124825,172.437121306867 -43.5121438699137))",
          ProjectTimeZone = "New Zealand Standard Time",
          IsArchived = false
        }
      };
    }

    public const string PROJECT_SETTINGS_TARGETS = @"{
            customBulkingPercent: 6,
            customCutFillTolerances: [0.22, 0.11, 0.055, 0, -0.055, -0.11, -0.22],
            customPassCountTargets: [1,2,3,4,5,10,20,30],
            customShrinkagePercent: 3,
            customTargetCmv: 10,
            customTargetCmvPercentMaximum: 100,
            customTargetCmvPercentMinimum: 75,
            customTargetMdp: 145,
            customTargetMdpPercentMaximum: 100,
            customTargetMdpPercentMinimum: 90,
            customTargetPassCountMaximum: 3,
            customTargetPassCountMinimum: 2,
            customTargetSpeedMaximum: 11,
            customTargetSpeedMinimum: 7,
            customTargetTemperatureMaximum: 130,
            customTargetTemperatureMinimum: 75,
            customTemperatureTargets: [0, 1, 2, 3, 4, 5, 6],
            useDefaultCutFillTolerances: false,
            useDefaultPassCountTargets: false,
            useDefaultTargetRangeCmvPercent: false,
            useDefaultTargetRangeMdpPercent: false,
            useDefaultTargetRangeSpeed: false,
            useDefaultVolumeShrinkageBulking: false,
            useMachineTargetCmv: false,
            useMachineTargetMdp: false,
            useMachineTargetPassCount: false,
            useMachineTargetTemperature: false,
            useDefaultTemperatureTargets: false
          }";

    public const string PROJECT_SETTINGS_TARGETS_EX = @"{
            customBulkingPercent: 6,
            customCutFillTolerances: [0.22, 0.11, 0.055, 0, -0.055, -0.11, -0.22],
            customPassCountTargets: [1,2,3,4,5,10,20,30],
            customShrinkagePercent: 3,
            customTargetCmv: 10,
            customTargetCmvPercentMaximum: 100,
            customTargetCmvPercentMinimum: 75,
            customTargetMdp: 145,
            customTargetMdpPercentMaximum: 100,
            customTargetMdpPercentMinimum: 90,
            customTargetPassCountMaximum: 3,
            customTargetPassCountMinimum: 2,
            customTargetSpeedMaximum: 11,
            customTargetSpeedMinimum: 7,
            customTargetTemperatureMaximum: 130,
            customTargetTemperatureMinimum: 75,
            customCMVTargets: [0, 75, 150, 300, 480],
            useDefaultCutFillTolerances: false,
            useDefaultPassCountTargets: false,
            useDefaultTargetRangeCmvPercent: false,
            useDefaultTargetRangeMdpPercent: false,
            useDefaultTargetRangeSpeed: false,
            useDefaultVolumeShrinkageBulking: false,
            useMachineTargetCmv: false,
            useMachineTargetMdp: false,
            useMachineTargetPassCount: false,
            useMachineTargetTemperature: false,
            useDefaultCMVTargets: false
          }";

    public const string PROJECT_SETTINGS_COLORS = @"{
            useDefaultElevationColors: false,
            elevationColors: [0xC80000, 0xFF0000, 0xFF3C00, 0xFF5A00, 0xFF8200, 0xFFAA00, 0xFFC800, 
                                0xFFDC00, 0xFAE600, 0xDCE600, 0xD2E600, 0xC8E600, 0xB4E600, 0x96E600, 
                                0x82E600, 0x64F000, 0x00FF00, 0x00F064, 0x00E682, 0x00E696, 0x00E6B4,
                                0x00E6C8, 0x00E6D2, 0x00DCDC, 0x00E6E6, 0x00C8E6, 0x00B4F0, 0x0096F5,
                                0x0078FA, 0x005AFF, 0x0000FF],
            useDefaultCMVDetailsColors: false,
            cmvDetailsColors: [0x01579B, 0x2473AE, 0x488FC1, 0x2D681D, 0xE55154],
            useDefaultCMVSummaryColors: false,
            cmvOnTargetColor: 0x8BC34A,
            cmvOverTargetColor: 0xD50000,
            cmvUnderTargetColor: 0x1579B,
            useDefaultCMVPercentColors: false,
            cmvPercentColors: [0xD50000, 0xE57373, 0xFFCDD2, 0x8BC34A, 0xB3E5FC, 0x005AFF, 0x039BE5, 0x01579B],
            useDefaultPassCountDetailsColors: false,
            passCountDetailsColors: [0x2D5783, 0x439BDC, 0xBEDFF1, 0x9DCE67, 0x6BA03E, 0x3A6B25, 0xF6CED3, 0xD57A7C, 0xC13037],
            useDefaultPassCountSummaryColors: false,
            passCountOnTargetColor: 0x8BC34A,
            passCountOverTargetColor: 0xD50000,
            passCountUnderTargetColor: 0x1579B,
            useDefaultCutFillColors: false,
            cutFillColors: [0xD50000, 0xE57373, 0xFFCDD2, 0x8BC34A, 0xB3E5FC, 0x039BE5, 0x01579B],
            useDefaultTemperatureDetailsColors: false,
            temperatureDetailsColors: [0xD50000, 0xFFCDD2, 0xB3E5FC, 0x01579B, 0xC13037, 0x00E682, 0x00E6C8],
            useDefaultTemperatureSummaryColors: false,
            temperatureOnTargetColor: 0x8BC34A,
            temperatureOverTargetColor: 0xD50000,
            temperatureUnderTargetColor: 0x1579B,
            useDefaultSpeedSummaryColors: false,
            speedOnTargetColor: 0x8BC34A,
            speedOverTargetColor: 0xD50000,
            speedUnderTargetColor: 0x1579B,
            useDefaultMDPSummaryColors: false,
            mdpOnTargetColor: 0x8BC34A,
            mdpOverTargetColor: 0xD50000,
            mdpUnderTargetColor: 0x1579B
          }";
  }
}
