﻿using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling
{
  public class DeviceDataSingleResult : ContractExecutionResult
  {
    private DeviceData _deviceData { get; set; }

    public DeviceDataSingleResult()
    {  }

    public DeviceDataSingleResult(DeviceData deviceData)
    {
      _deviceData = deviceData;
    }

    public DeviceData DeviceDescriptor { get { return _deviceData; } set { _deviceData = value; } }
  }
}
