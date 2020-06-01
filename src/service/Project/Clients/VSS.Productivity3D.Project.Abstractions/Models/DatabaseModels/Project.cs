﻿using System;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels
{
  public class Project
  {
    public string ProjectUID { get; set; }

    public string CustomerUID { get; set; }

    // legacy ProjectID in Gen2 is a bigint. However Raptor can't handle one, and we're unlikely to need to get that big.
    public int ShortRaptorProjectId { get; set; }

    public string Name { get; set; }

    public ProjectType ProjectType { get; set; }

    public UserProjectRoleEnum UserProjectRole { get; set; }

    public string ProjectTimeZone { get; set; }
     
    public string ProjectTimeZoneIana { get; set; }

    public string Boundary { get; set; }

    public string CoordinateSystemFileName { get; set; }

    public DateTime? CoordinateSystemLastActionedUTC { get; set; }

    public bool IsArchived { get; set; }

    public DateTime LastActionedUTC { get; set; }

    public override bool Equals(object obj)
    {
      if (!(obj is Project otherProject))
      {
        return false;
      }

      return otherProject.ProjectUID == ProjectUID
        && otherProject.CustomerUID == CustomerUID
        && otherProject.ShortRaptorProjectId == ShortRaptorProjectId
        && otherProject.Name == Name
        && otherProject.ProjectType == ProjectType
        && otherProject.ProjectTimeZone == ProjectTimeZone
        && otherProject.ProjectTimeZoneIana == ProjectTimeZoneIana        
        && otherProject.Boundary == Boundary
        && otherProject.CoordinateSystemFileName == CoordinateSystemFileName
        && otherProject.CoordinateSystemLastActionedUTC == CoordinateSystemLastActionedUTC
        && otherProject.IsArchived == IsArchived;
    }

    public override int GetHashCode()
    {
      return 0;
    }
  }
}
