﻿using VSS.VisionLink.Interfaces.Events.Telematics.Context;
using VSS.VisionLink.Interfaces.Events.Telematics.Interfaces;

namespace VSS.VisionLink.Interfaces.Events.Telematics.Machine
{
  public class BatteryChargingVoltageEvent : IAnnotatedEvent
  {
    public AssetDetail Asset { get; set; }
    public DeviceDetail Device { get; set; }
    public OwnerDetail Owner { get; set; }
    public TimestampDetail Timestamp { get; set; }
    public TracingMetadataDetail TracingMetadata { get; set; }

    public double? Voltage { get; set; }
  }
}