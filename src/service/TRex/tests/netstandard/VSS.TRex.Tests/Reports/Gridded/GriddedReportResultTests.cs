﻿using System.IO;
using Microsoft.AspNetCore.Mvc;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Reports.Gridded;
using Xunit;

namespace VSS.TRex.Tests.Reports.Gridded
{
  public class GriddedReportResultTests
  {

    [Fact]
    public void CutFillResult_Population_Successful()
    {
      var original = new GriddedReportResult(ReportType.Gridded);
      var griddedDataRow = new GriddedReportDataRow()
      {
        Northing = 1.0,
        Easting = 2.0,
        Elevation = 3.0,
        CutFill = 4.0,
        Cmv = 5,
        Mdp = 6,
        PassCount = 7,
        Temperature = 8
      };
      original.GriddedData.Rows.Add(griddedDataRow);
      original.GriddedData.ElevationReport = true;
      original.GriddedData.CutFillReport = true;
      original.GriddedData.CmvReport = false;
      original.GriddedData.MdpReport = true;
      original.GriddedData.PassCountReport = false;
      original.GriddedData.TemperatureReport = true;

      var byteArrayOfOriginal = original.Write();
      var copyOfOrig = new GriddedReportResult();
      copyOfOrig.Read(byteArrayOfOriginal);

      Assert.True(original.ReportType == copyOfOrig.ReportType, "Invalid report type");
      Assert.True(original.GriddedData.Rows.Count == copyOfOrig.GriddedData.Rows.Count, "Invalid number of rows");
      Assert.Equal(original, copyOfOrig);
    }

    [Fact]
    public void CutFillResult_Streaming_Successful()
    {
      // When Response returned via MasterData Proxies,
      //  it is converted to byte[], then stream then filestream, then back
      var original = new GriddedReportResult(ReportType.Gridded);
      var griddedDataRow = new GriddedReportDataRow()
      {
        Northing = 1.0,
        Easting = 2.0,
        Elevation = 3.0,
        CutFill = 4.0,
        Cmv = 5,
        Mdp = 6,
        PassCount = 7,
        Temperature = 8
      };
      original.GriddedData.Rows.Add(griddedDataRow);
      original.GriddedData.ElevationReport = true;
      original.GriddedData.CutFillReport = true;
      original.GriddedData.CmvReport = false;
      original.GriddedData.MdpReport = true;
      original.GriddedData.PassCountReport = false;
      original.GriddedData.TemperatureReport = true;

      var byteArrayOfOriginal = original.Write();
      var copyOfOrig = new GriddedReportResult();
      copyOfOrig.Read(byteArrayOfOriginal);

      // GracefullWebReq
      var fileStream = new FileStreamResult(new MemoryStream(byteArrayOfOriginal), "application/octet-stream");
      var memoryStream = (MemoryStream)fileStream.FileStream;
      var resultFromStream = new GriddedReportResult();

      resultFromStream.Read(memoryStream.ToArray());

      Assert.True(original.ReportType == resultFromStream.ReportType, "Invalid report type");
      Assert.True(original.GriddedData.Rows.Count == resultFromStream.GriddedData.Rows.Count, "Invalid number of rows");
      Assert.Equal(original, resultFromStream);
    }
  }
}
