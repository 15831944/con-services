﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.VisionLink.Interfaces.Events.MasterData.Models
{
    public class CreateDeviceEvent : IDeviceEvent
    {
        public Guid DeviceUID { get; set; }
        public string DeviceSerialNumber { get; set; }
        public string DeviceType { get; set; } //Required Field
        public string DeviceState { get; set; } //Required Field
        public DateTime? DeregisteredUTC { get; set; }
        public string ModuleType { get; set; }
        public string MainboardSoftwareVersion { get; set; }
        public string RadioFirmwarePartNumber { get; set; }
        public string GatewayFirmwarePartNumber { get; set; }
        public string DataLinkType { get; set; }
        public DateTime ActionUTC { get; set; }
        public DateTime ReceivedUTC { get; set; }
    }
}