﻿using System;
using System.Collections.Generic;
using System.Linq;
using MockProjectWebApi.Utils;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Filter.Abstractions.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace MockProjectWebApi.Services
{
  public class FiltersService : IFiltersService
  {
    public Dictionary<string, FilterListData> FilterData;

    public FiltersService()
    {
      CreateTestData();
    }

    public FilterData GetFilter(string projectUid, string filterUid)
    {
      var filters = GetFilters(projectUid);
      var filter = filters
        .filterDescriptors
        .SingleOrDefault(s => string.Equals(s.FilterUid, filterUid, StringComparison.CurrentCultureIgnoreCase));

      if (filter == null)
      {
        return new FilterData
        {
          Code = 36,
          Message =
            "GetFilter By filterUid. The requested filter does exist, or does not belong to the requesting customer; project or user."
        };
      }
      return new FilterData { filterDescriptor = filter };
    }

    public FilterListData GetFilters(string projectUid)
    {
      if (FilterData.ContainsKey(projectUid))
      {
        return FilterData[projectUid];
      }
      return new FilterListData { filterDescriptors = new List<FilterDescriptor>() };
    }

    private void CreateTestData()
    {
      FilterData = new Dictionary<string, FilterListData>();

      var filters = new FilterListData
      {
        filterDescriptors = goldenDataFilterDescriptors
      };

      filters.filterDescriptors.Add(FilterDescriptors.GoldenDimensions.ProjectExtentsFilter);
      filters.filterDescriptors.Add(FilterDescriptors.GoldenDimensions.ProjectExtentsFilterElevationTypeFirst);
      filters.filterDescriptors.Add(FilterDescriptors.GoldenDimensions.ProjectExtentsFilterElevationTypeLast);
      filters.filterDescriptors.Add(FilterDescriptors.GoldenDimensions.InvalidDateFilterElevationTypeFirst);
      filters.filterDescriptors.Add(FilterDescriptors.GoldenDimensions.InvalidDateFilterElevationTypeLast);
      filters.filterDescriptors.Add(FilterDescriptors.GoldenDimensions.NoDataFilterElevationTypeFirst);
      filters.filterDescriptors.Add(FilterDescriptors.GoldenDimensions.NoDataFilterElevationTypeLast);
      filters.filterDescriptors.Add(FilterDescriptors.GoldenDimensions.SummaryVolumesBaseFilter20170305);
      filters.filterDescriptors.Add(FilterDescriptors.GoldenDimensions.SummaryVolumesTopFilter20170621);
      filters.filterDescriptors.Add(FilterDescriptors.GoldenDimensions.VolumesFilterWithPassCountRangeEarliest);
      filters.filterDescriptors.Add(FilterDescriptors.GoldenDimensions.VolumesFilterWithPassCountRangeLatest);

      FilterData.Add(ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1, filters);

      var filters2 = new FilterListData
      {
        filterDescriptors = new List<FilterDescriptor>
        {
          FilterDescriptors.Dimensions.DimensionsAsAtCustom,
          FilterDescriptors.Dimensions.ElevationRangeAndPaletteNoDataFilter,
          FilterDescriptors.Dimensions.DimensionsBoundaryFilterWithMachine,
          FilterDescriptors.Dimensions.DimensionsBoundaryFilter,
          FilterDescriptors.Dimensions.DimensionsBoundaryFilterAsAtToday,
          FilterDescriptors.Dimensions.DimensionsBoundaryMdp,
          FilterDescriptors.Dimensions.DimensionsBoundaryMdpPassCountRange,
          FilterDescriptors.Dimensions.DimensionsBoundaryMdpAsAtToday,
          FilterDescriptors.Dimensions.DimensionsBoundaryCmv,
          FilterDescriptors.Dimensions.DimensionsBoundaryCmvPassCountRange,
          FilterDescriptors.Dimensions.DimensionsBoundaryCmvAsAtToday,
          FilterDescriptors.Dimensions.SummaryVolumesBaseFilter,
          FilterDescriptors.Dimensions.SummaryVolumesTopFilter,
          FilterDescriptors.Dimensions.SummaryVolumesFilterExtentsEarliest,
          FilterDescriptors.Dimensions.SummaryVolumesFilterExtentsLatest,
          FilterDescriptors.Dimensions.SummaryVolumesFilterExtentsEarliestWithPassCountRange,
          FilterDescriptors.Dimensions.SummaryVolumesFilterExtentsLatestWithPassCountRange,
          FilterDescriptors.Dimensions.SummaryVolumesFilterToday,
          FilterDescriptors.Dimensions.SummaryVolumesFilterNoDates,
          FilterDescriptors.Dimensions.SummaryVolumesFilterNoLatLonToday,
          FilterDescriptors.Dimensions.SummaryVolumesFilterNoLatLonYesterday,
          FilterDescriptors.Dimensions.SummaryVolumesFilterProjectExtents,
          FilterDescriptors.Dimensions.SummaryVolumesFilterCustom20121101First,
          FilterDescriptors.Dimensions.SummaryVolumesFilterCustom20121101Last,
          FilterDescriptors.Dimensions.SummaryVolumesFilterNull,
          FilterDescriptors.Dimensions.SummaryVolumesTemperature,
          FilterDescriptors.Dimensions.ReportDxfTile,
          FilterDescriptors.Dimensions.DimensionsAlignmentFilter0to200,
          FilterDescriptors.Dimensions.DimensionsAlignmentFilter100to200,
          FilterDescriptors.Dimensions.DimensionsTemperatureRangeFilter,
          FilterDescriptors.Dimensions.DimensionsTempRangeBoundaryFilter,
          FilterDescriptors.Dimensions.DimensionsPassCountRangeFilter,
          FilterDescriptors.Dimensions.DimensionsAutomaticsFilter,
          FilterDescriptors.Dimensions.VolumesWithExplicitFilter1Nov,
          FilterDescriptors.Dimensions.VolumesWithExplicitFilter1NovFirstPass
        }
      };

      FilterData.Add(ConstantsUtil.DIMENSIONS_PROJECT_UID, filters2);
      FilterData.Add(ConstantsUtil.CUSTOM_SETTINGS_DIMENSIONS_PROJECT_UID, filters2);

      var kettlewellDriveFilters = new FilterListData
      {
        filterDescriptors = new List<FilterDescriptor>
        {
          FilterDescriptors.KettlewellDrive.MinElevationSingleCellPassMultiMode,
          FilterDescriptors.KettlewellDrive.MinElevationLastPassMode
        }
      };
      FilterData.Add(ConstantsUtil.KETTLEWELL_DRIVE_PROJECT_UID, kettlewellDriveFilters);

      var christchurchTestSiteFilters = new FilterListData
      {
        filterDescriptors = new List<FilterDescriptor>
        {
          FilterDescriptors.ChristchurchTestSite.NegativeStartStationFilter
        }
      };

      FilterData.Add(ConstantsUtil.CHRISTCHURCH_TEST_SITE_PROJECT_UID, christchurchTestSiteFilters);
    }

    private readonly List<FilterDescriptor> goldenDataFilterDescriptors = new List<FilterDescriptor>
    {
      new FilterDescriptor
      {
        FilterUid = "3d9086f2-3c04-4d92-9141-5134932b1523",
        Name = "Filter 1",
        FilterType = FilterType.Persistent,
        FilterJson =
          "{\"startUTC\": null,\"endUTC\": null,\"designUid\": \"220e12e5-ce92-4645-8f01-1942a2d5a57f\",\"contributingMachines\": null,\"onMachineDesignID\": null,\"elevationType\": null,\"vibeStateOn\": null,\"polygonName\": null,\"polygonUid\": null,\"polygonLL\": null,\"forwardDirection\": null,\"layerNumber\": null,\"layerType\": null,\"SpatialFilter\":false}"
      },
      new FilterDescriptor
      {
        FilterUid = "81422acc-9b0c-401c-9987-0aedbf153f1d",
        Name = "Filter 2",
        FilterType = FilterType.Persistent,
        FilterJson =
          "{\"startUTC\":null,\"endUTC\":null,\"designUid\":\"dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff\",\"contributingMachines\":null,\"onMachineDesignID\":null,\"elevationType\":null,\"vibeStateOn\":null,\"polygonName\": null,\"polygonUid\": \"94dc0ec6-32ef-4b54-a7f8-e8e3eee65642\",\"polygonLL\":null,\"forwardDirection\":null,\"layerNumber\":null,\"layerType\":null}"
      },
      new FilterDescriptor
      {
        FilterUid = "1cf81668-1739-42d5-b068-ea025588796a",
        Name = "Filter 3",
        FilterType = FilterType.Persistent,
        FilterJson =
          "{\"startUTC\":null,\"endUTC\":null,\"designUid\":\"ea97efb9-c0c4-4a7f-9eee-e2b0ef0b0916\",\"contributingMachines\":null,\"onMachineDesignID\":null,\"elevationType\":null,\"vibeStateOn\":null,\"polygonLL\":null,\"forwardDirection\":null,\"layerNumber\":null,\"layerType\":null}"
      },
      new FilterDescriptor
      {
        FilterUid = "d15e65e0-3cb1-476f-8fc6-08507a14a269",
        Name = "Filter 4",
        FilterType = FilterType.Persistent,
        FilterJson =
          "{\"startUTC\":\"2012-11-05\",\"endUTC\":\"2012-11-06\",\"designUid\":null,\"contributingMachines\":null,\"onMachineDesignID\":null,\"elevationType\":null,\"vibeStateOn\":null,\"polygonLL\":null,\"forwardDirection\":null,\"layerNumber\":null,\"layerType\":null}"
      },
      new FilterDescriptor
      {
        FilterUid = "d7cb424d-b012-4618-b3bc-e526ca84bbd6",
        Name = "Filter 5",
        FilterType = FilterType.Persistent,
        FilterJson =
          "{\"startUTC\":\"2017-11-05\",\"endUTC\":\"2017-11-06\",\"designUid\":null,\"contributingMachines\":null,\"onMachineDesignID\":null,\"elevationType\":null,\"vibeStateOn\":null,\"polygonLL\":null,\"forwardDirection\":null,\"layerNumber\":null,\"layerType\":null}"
      },
      new FilterDescriptor
      {
        FilterUid = "9c27697f-ea6d-478a-a168-ed20d6cd9a20",
        Name = "Dimensions boundary filter with machine",
        FilterType = FilterType.Persistent,
        FilterJson =
          "{\"contributingMachines\":[{\"assetID\":4250986182719752,\"machineName\":\"VOLVO G946B\",\"isJohnDoe\":false}],\"polygonUID\":\"ca9c91c3-513b-4082-b2d7-0568899e56d5\",\"polygonName\":\"Dimensions boundary with machine 2\",\"polygonLL\":[{\"Lat\":36.207118,\"Lon\":-115.01848},{\"Lat\":36.207334,\"Lon\":-115.018394},{\"Lat\":36.207492,\"Lon\":-115.019604},{\"Lat\":36.207101,\"Lon\":-115.019478}]}"
      },
      new FilterDescriptor
      {
        FilterUid = "154470b6-15ae-4cca-b281-eae8ac1efa6c",
        Name = "Dimensions boundary filter",
        FilterType = FilterType.Persistent,
        FilterJson =
          "{\"polygonUID\":\"7f2fb9ec-2384-420e-b2e3-72b9cea939a3\",\"polygonName\":\"Dimensions lower right bounday 2\",\"polygonLL\":[{\"Lat\":36.206897,\"Lon\":-115.01869},{\"Lat\":36.206795,\"Lon\":-115.018701},{\"Lat\":36.206823,\"Lon\":-115.018264},{\"Lat\":36.206977,\"Lon\":-115.01828}]}"
      },
      new FilterDescriptor
      {
        FilterUid = "3ef41e3c-d1f5-40cd-b012-99d11ff432ef",
        Name = "Dimensions boundary mdp",
        FilterType = FilterType.Persistent,
        FilterJson = "{\"polygonUID\":\"318f0103-a0c3-4b50-88d4-d4fa12370a63\",\"polygonName\":\"Dimensions boundary mdp\",\"polygonLL\":[{\"Lat\":36.207659,\"Lon\":-115.018943},{\"Lat\":36.207265,\"Lon\":-115.018926},{\"Lat\":36.207412,\"Lon\":-115.018471,\"SpatialFilter\":false}]}"
      },
      new FilterDescriptor
      {
        FilterUid = "a37f3008-65e5-44a8-b406-9a078ec62ece",
        Name = "Dimensions boundary CMV",
        FilterType = FilterType.Persistent,
        FilterJson = "{\"polygonUID\":\"c910d127-5e3c-453f-82c3-e235848ac20e\",\"polygonName\":\"Dimensions boundary CMV\",\"polygonLL\":[{\"Lat\":36.207183,\"Lon\":-115.020509},{\"Lat\":36.206862,\"Lon\":-115.020187},{\"Lat\":36.207174,\"Lon\":-115.019731}]}"
      },
      new FilterDescriptor
      {
        FilterUid = "06ee0bd8-c778-422c-9af2-bd86a370be2e",
        Name = "Filter with reference surface",
        FilterType = FilterType.Persistent,
        FilterJson =
          "{\"startUTC\": null,\"endUTC\": null,\"designUid\": \"c91e56cf-6d5f-436d-9655-cf4f919523f4\",\"contributingMachines\": null,\"onMachineDesignID\": null,\"elevationType\": null,\"vibeStateOn\": null,\"polygonName\": null,\"polygonUid\": null,\"polygonLL\": null,\"forwardDirection\": null,\"layerNumber\": null,\"layerType\": null,\"SpatialFilter\":false}"
      }
    };
  }
}
