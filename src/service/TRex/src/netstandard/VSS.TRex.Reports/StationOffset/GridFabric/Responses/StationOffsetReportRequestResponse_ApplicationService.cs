﻿using System.Collections.Generic;
using System.Linq;
using Apache.Ignite.Core.Binary;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.TRex.Common;

namespace VSS.TRex.Reports.StationOffset.GridFabric.Responses
{
  /// <summary>
  /// The response returned from the StationOffset request executor that contains the response code and the set of
  /// sub grids extracted for the StationOffset report in question
  /// </summary>
  public class StationOffsetReportRequestResponse_ApplicationService : SubGridsPipelinedResponseBase
  {
    private static byte VERSION_NUMBER = 1;

    public ReportReturnCode ReturnCode;  // == TRaptorReportReturnCode
    public ReportType ReportType;        // == TRaptorReportType
    public List<StationOffsetReportDataRow_ApplicationService> StationOffsetReportDataRowList;

    public StationOffsetReportRequestResponse_ApplicationService()
    {
      Clear();
    }

    public void LoadStationOffsets(List<StationOffsetRow> stationOffsets)
    {
      var queryStations = 
        stationOffsets
        .GroupBy(stationOffsetRow => stationOffsetRow.Station)
        .OrderBy(newGroup => newGroup.Key);

      foreach (var stationGroup in queryStations)
      {
        StationOffsetReportDataRowList.Add(new StationOffsetReportDataRow_ApplicationService
          (stationGroup.Key, stationGroup.ToList()));
      }
    }

    private void Clear()
    {
      ReturnCode = ReportReturnCode.NoError;
      ReportType = ReportType.StationOffset;
      StationOffsetReportDataRowList = new List<StationOffsetReportDataRow_ApplicationService>();
    }
    

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      base.InternalToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteInt((int)ReturnCode);
      writer.WriteInt((int)ReportType);
      writer.WriteInt(StationOffsetReportDataRowList.Count);
      for (var i = 0; i < StationOffsetReportDataRowList.Count; i++)
      {
        StationOffsetReportDataRowList[i].ToBinary(writer);
      }
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      base.InternalFromBinary(reader);

      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        ReturnCode = (ReportReturnCode) reader.ReadInt();
        ReportType = (ReportType) reader.ReadInt();
        var stationOffsetRowsCount = reader.ReadInt();
        StationOffsetReportDataRowList = new List<StationOffsetReportDataRow_ApplicationService>(stationOffsetRowsCount);
        for (var i = 0; i < stationOffsetRowsCount; i++)
        {
          var row = new StationOffsetReportDataRow_ApplicationService();
          row.FromBinary(reader);
          StationOffsetReportDataRowList.Add(row);
        }
      }
    }
  }
}
