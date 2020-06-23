﻿using System;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Visionlink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.Visionlink.Interfaces.Events.MasterData.Models
{
  public class UpdateProjectEvent : IProjectEvent
  {
    public string ProjectTimezone { get; set; }
    public string ProjectName { get; set; }
    public CwsProjectType ProjectType { get; set; }

    public Guid ProjectUID { get; set; }
    public string ProjectBoundary { get; set; } // this is an addition later in the game, so optional

    public string CoordinateSystemFileName { get; set; }
    public byte[] CoordinateSystemFileContent { get; set; }

    public DateTime ActionUTC { get; set; }
  }
}
