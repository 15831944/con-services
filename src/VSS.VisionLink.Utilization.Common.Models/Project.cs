﻿using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.VisionLink.Landfill.Common.Models
{
  public class Project
  {
    public DateTime lastActionedUtc { get; set; }
    public int projectId { get; set; }
    public string name { get; set; }
    public string timeZone { get; set; }
    public DateTime retrievalStartedAt { get; set; }
    public int daysToSubscriptionExpiry { get; set; }
    public string projectUid { get; set; }
    public string customerUid { get; set; }
    public string subscriptionUid { get; set; }
    public DateTime projectEndDate { get; set; }
    public DateTime projectStartDate { get; set; }
    public ProjectType projectType { get; set; }
  
    public override bool Equals(object obj)
    {
      var otherProject = obj as Project;
      if (otherProject == null) return false;
      return otherProject.projectId == projectId && otherProject.name == name && otherProject.timeZone == timeZone
             && otherProject.projectUid == projectUid;
    }
    public override int GetHashCode() { return 0; }
  }
}