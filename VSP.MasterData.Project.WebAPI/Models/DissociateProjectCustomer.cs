﻿using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.Interfaces.Events.MasterData.Models
{
  public class DissociateProjectCustomer : IProjectEvent
  {
    public Guid ProjectUID { get; set; }
    public Guid CustomerUID { get; set; }
    public DateTime ActionUTC { get; set; }
    public DateTime ReceivedUTC { get; set; }
  }
}