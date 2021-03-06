﻿using System.Collections.Generic;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.TRex.Reports.Gridded;
using VSS.TRex.Reports.Gridded.GridFabric;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Reports.Responses
{
  public class ToFromBinary_GriddedReportRequestResponse
  {
    [Fact] 
    public void Test_GriddedReportRequestResponse_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<GriddedReportRequestResponse>("Empty GriddedReportResponse not same after round trip serialisation");
    }

    [Fact] 
    public void Test_GriddedReportRequestResponse_WithContent()
    {
      var rows = new List<GriddedReportDataRow>
      {
        new GriddedReportDataRow()
        {
          Northing = 1,
          Easting = 2,
          Elevation = 3,
          CutFill = 4,
          Cmv = 5,
          Mdp = 6,
          PassCount = 7,
          Temperature = 8
        },
        new GriddedReportDataRow()
        {
          Northing = 10,
          Easting = 11,
          Elevation = 12,
          CutFill = 13,
          Cmv = 14,
          Mdp = 15,
          PassCount = 16,
          Temperature = 17
        }
      };
      var rowList = new List<GriddedReportDataRow>();
      rowList.AddRange(rows);

      var response = new GriddedReportRequestResponse()
      {
        ResultStatus = RequestErrorStatus.OK,
        ReturnCode = ReportReturnCode.NoError,
        GriddedReportDataRowList = rowList
      };

      SimpleBinarizableInstanceTester.TestClass(response, "Empty GriddedReportResponse not same after round trip serialisation");
    }
  }
}
