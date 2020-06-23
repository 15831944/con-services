﻿using System;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Visionlink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.Visionlink.Interfaces.Events.MasterData.Models
{
  public class CreateProjectEvent : IProjectEvent
  {
    public Guid ProjectUID { get; set; }
    public Guid CustomerUID { get; set; }
    public int ShortRaptorProjectId { get; set; }
    public string ProjectName { get; set; }   
    public CwsProjectType ProjectType { get; set; }
    public string ProjectTimezone { get; set; }   
    public string ProjectBoundary { get; set; }
    public string CoordinateSystemFileName { get; set; }
    public byte[] CoordinateSystemFileContent { get; set; }
    public DateTime ActionUTC { get; set; }
  }

}
