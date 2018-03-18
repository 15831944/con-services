﻿using MockProjectWebApi.Json;
using VSS.MasterData.Models.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace MockProjectWebApi.Utils
{
  public class FilterDescriptors
  {
    public class GoldenDimensions
    {
      public static FilterDescriptor ProjectExtentsFilter => new FilterDescriptor
      {
        FilterUid = "5e089924-98cb-49a6-8323-19537dc6d665",
        Name = "Golden Dimensions Project Extents Filter",
        FilterJson = JsonResourceHelper.GetGoldenDimensionsFilterJson("ProjectExtentsFilter")
      };

      public static FilterDescriptor ProjectExtentsFilterElevationTypeFirst => new FilterDescriptor
      {
        FilterUid = "f4e9b4dd-e8c4-4edb-b9aa-59a209c17de7",
        Name = "Golden Dimensions Project Extents Filter",
        FilterJson = JsonResourceHelper.GetGoldenDimensionsFilterJson("ProjectExtentsFilterElevationTypeFirst")
      };

      public static FilterDescriptor ProjectExtentsFilterElevationTypeLast => new FilterDescriptor
      {
        FilterUid = "7730ea54-6c6f-4450-ae94-1933471d7961",
        Name = "Golden Dimensions Project Extents Filter",
        FilterJson = JsonResourceHelper.GetGoldenDimensionsFilterJson("ProjectExtentsFilterElevationTypeLast")
      };

      #region Invalid date range filters (first -> last)

      public static FilterDescriptor InvalidDateFilterElevationTypeFirst => new FilterDescriptor
      {
        FilterUid = "f92100c6-5397-4574-9688-be375d40625e",
        Name = "Golden Dimensions Project Extents Filter",
        FilterJson = JsonResourceHelper.GetGoldenDimensionsFilterJson("InvalidDateFilterElevationTypeFirst")
      };

      public static FilterDescriptor InvalidDateFilterElevationTypeLast => new FilterDescriptor
      {
        FilterUid = "7bc0bfa5-b0e9-463d-9f17-bdc8b18c0b8f",
        Name = "Golden Dimensions Project Extents Filter",
        FilterJson = JsonResourceHelper.GetGoldenDimensionsFilterJson("InvalidDateFilterElevationTypeLast")
      };

      #endregion

      #region No Data date range filters (first -> last)

      public static FilterDescriptor NoDataFilterElevationTypeFirst => new FilterDescriptor
      {
        FilterUid = "ce4497d9-76d0-4477-aa23-2ee1acd8c4f0",
        Name = "Golden Dimensions Project Extents Filter",
        FilterJson = JsonResourceHelper.GetGoldenDimensionsFilterJson("NoDataFilterElevationTypeFirst")
      };

      public static FilterDescriptor NoDataFilterElevationTypeLast => new FilterDescriptor
      {
        FilterUid = "fe6065a7-21fe-4db0-8f47-3ea6c320dac7",
        Name = "Golden Dimensions Project Extents Filter",
        FilterJson = JsonResourceHelper.GetGoldenDimensionsFilterJson("NoDataFilterElevationTypeLast")
      };

      #endregion

      public static FilterDescriptor SummaryVolumesBaseFilter20170305 => new FilterDescriptor
      {
        FilterUid = "abd72636-43d3-4b04-9c3b-6383743659e4",
        Name = "GD Dimensions Base Filter 5/3/2017",
        FilterJson = JsonResourceHelper.GetGoldenDimensionsFilterJson("SummaryVolumesBaseFilter20170305")
      };

      public static FilterDescriptor SummaryVolumesTopFilter20170621 => new FilterDescriptor
      {
        FilterUid = "f0e02abf-995c-44ef-bd89-1936d2564e57",
        Name = "GD Dimensions Base Filter 21/6/2017",
        FilterJson = JsonResourceHelper.GetGoldenDimensionsFilterJson("SummaryVolumesTopFilter20170621")
      };
    }

    public class Dimensions
    {
      public static FilterDescriptor DimensionsBoundaryCmv => new FilterDescriptor
      {
        FilterUid = "a37f3008-65e5-44a8-b406-9a078ec62ece",
        Name = "Dimensions boundary CMV",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetFilterJson("DimensionsBoundaryCMV")
      };

      public static FilterDescriptor DimensionsBoundaryFilter => new FilterDescriptor
      {
        FilterUid = "154470b6-15ae-4cca-b281-eae8ac1efa6c",
        Name = "Dimensions boundary filter",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetFilterJson("DimensionsBoundaryFilter")
      };

      public static FilterDescriptor DimensionsBoundaryMdp => new FilterDescriptor
      {
        FilterUid = "3ef41e3c-d1f5-40cd-b012-99d11ff432ef",
        Name = "Dimensions boundary mdp",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetFilterJson("DimensionsBoundaryMDP")
      };

      public static FilterDescriptor DimensionsBoundaryFilterWithMachine => new FilterDescriptor
      {
        FilterUid = "9c27697f-ea6d-478a-a168-ed20d6cd9a20",
        Name = "Dimensions boundary filter with machine",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetFilterJson("DimensionsBoundaryFilterWithMachine")
      };

      public static FilterDescriptor ElevationRangeAndPaletteNoDataFilter => new FilterDescriptor
      {
        FilterUid = "200c7b47-b5e6-48ee-a731-7df6623412da",
        Name = "Elevation Range and Palette No Data Filter",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetFilterJson("ElevationRangeAndPaletteNoDataFilter")
      };

      public static FilterDescriptor SummaryVolumesBaseFilter => new FilterDescriptor
      {
        FilterUid = "F07ED071-F8A1-42C3-804A-1BDE7A78BE5B",
        Name = "Summary Volumes Base Filter",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetFilterJson("SummaryVolumesBaseFilter")
      };

      public static FilterDescriptor SummaryVolumesTopFilter => new FilterDescriptor
      {
        FilterUid = "A40814AA-9CDB-4981-9A21-96EA30FFECDD",
        Name = "Summary Volumes Top Filter",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetFilterJson("SummaryVolumesTopFilter")
      };

      public static FilterDescriptor SummaryVolumesFilterToday => new FilterDescriptor
      {
        FilterUid = "A54E5945-1AAA-4921-9CC1-C9D8C0A343D3",
        Name = "Summary Volumes Filter Today",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetDimensionsFilterJson("SummaryVolumesFilterToday")
      };

      public static FilterDescriptor SummaryVolumesFilterYesterday => new FilterDescriptor
      {
        FilterUid = "A325F48A-3F3D-489A-976A-B4780EF84045",
        Name = "Summary Volumes Filter Yesterday",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetDimensionsFilterJson("SummaryVolumesFilterYesterday")
      };

      public static FilterDescriptor SummaryVolumesFilterNoLatLonToday => new FilterDescriptor
      {
        FilterUid = "F9D55290-27F2-4B70-BC63-9FD23218E6E6",
        Name = "Summary Volumes Filter No Lat Lon Today",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetDimensionsFilterJson("SummaryVolumesFilterNoLatLonToday")
      };
      public static FilterDescriptor SummaryVolumesFilterNoLatLonYesterday => new FilterDescriptor
      {
        FilterUid = "D6B254A0-C047-4805-9CCD-F847FAB05B14",
        Name = "Summary Volumes Filter No Lat Lon Yesterday",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetDimensionsFilterJson("SummaryVolumesFilterNoLatLonYesterday")
      };

      public static FilterDescriptor SummaryVolumesFilterProjectExtents => new FilterDescriptor
      {
        FilterUid = "03914de8-dce7-403b-8790-1e07773db5e1",
        Name = "Summary Volumes Filter Project Extents",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetDimensionsFilterJson("SummaryVolumesFilterProjectExtents")
      };

      public static FilterDescriptor SummaryVolumesFilterCustom20121101First => new FilterDescriptor
      {
        FilterUid = "9244d3f1-af2b-41ed-aa16-5a776278b6eb",
        Name = "Summary Volumes Filter Custom 2012-11-01 First",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetDimensionsFilterJson("SummaryVolumesFilterCustom20121101First")
      };

      public static FilterDescriptor SummaryVolumesFilterExtentsEarliest => new FilterDescriptor
      {
        FilterUid = "9c27697f-ea6d-478a-a168-ed20d6cd9a22",
        Name = "Summary Volumes Filter Extents Earliest",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetDimensionsFilterJson("SummaryVolumesFilterProjectExtentsEarliest")
      };

      public static FilterDescriptor SummaryVolumesFilterExtentsLatest => new FilterDescriptor
      {
        FilterUid = "9c27697f-ea6d-478a-a168-ed20d6cd9a21",
        Name = "Summary Volumes Filter Extents Latest",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetDimensionsFilterJson("SummaryVolumesFilterProjectExtentsLatest")
      };

      public static FilterDescriptor SummaryVolumesFilterCustom20121101Last => new FilterDescriptor
      {
        FilterUid = "279ed62b-06a2-4184-ab14-dd7462dcc8c1",
        Name = "Summary Volumes Filter Custom 2012-11-01 Last",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetDimensionsFilterJson("SummaryVolumesFilterCustom20121101Last")
      };

      public static FilterDescriptor SummaryVolumesFilterNull => new FilterDescriptor
      {
        FilterUid = "e2c7381d-1a2e-4dc7-8c0e-45df2f92ba0e",
        Name = "Summary Volumes Filter Null",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetDimensionsFilterJson("SummaryVolumesFilterNull20121101")
      };

      public static FilterDescriptor SummaryVolumesTemperature => new FilterDescriptor
      {
        FilterUid = "601afff6-844e-448d-a16c-bd40a5dc9124",
        Name = "Summary Volumes Temperature",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetDimensionsFilterJson("SummaryVolumesTemperature")
      };

      public static FilterDescriptor ReportDxfTile => new FilterDescriptor
      {
        FilterUid = "7b2bd262-8355-44ba-938a-d50f9712dafc",
        Name = "Report Dxf Tile",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetFilterJson("ReportDxfTile")
      };
      public static FilterDescriptor DimensionsAlignmentFilter0to200 => new FilterDescriptor
      {
        FilterUid = "2811c7c3-d270-4d63-97e2-fc3340bf6c7a",
        Name = "Dimensions Alignment Filter 0-200",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetFilterJson("DimensionsAlignmentFilter0to200")
      };
      public static FilterDescriptor DimensionsAlignmentFilter100to200 => new FilterDescriptor
      {
        FilterUid = "2811c7c3-d270-4d63-97e2-fc3340bf6c6b",
        Name = "Dimensions Alignment Filter 100t-200",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetFilterJson("DimensionsAlignmentFilter100to200")
      };
    }
  }
}