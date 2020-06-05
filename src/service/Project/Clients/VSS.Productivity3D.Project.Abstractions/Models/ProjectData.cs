﻿using System;
using System.Collections.Generic;
using VSS.Common.Abstractions.Extensions;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Project.Abstractions.Models
{
  public class ProjectData : IMasterDataModel
  {
    public Guid ProjectUID { get; set; }

    // legacy ProjectID in Gen2 is a bigint. However Raptor can't handle one, and we're unlikely to need to get that big.
    public long ShortRaptorProjectId => ProjectUID.ToLegacyId();
    

    public ProjectType ProjectType { get; set; }

    public string Name { get; set; }
   
    // IanaTimeZone
    public string ProjectTimeZone { get; set; }

    // This should really be named ProjectTimeZoneIana.
    //     It is required for all projects, not just landfill.
    //     ProjectTimeZone is in Windows StandardTime name,
    //         which the UI,and ProjectSvc limit to a known set (contained in PreferencesTimeZones.cs).
    public string IanaTimeZone { get; set; }

    public string CustomerUID { get; set; }

    public string ProjectGeofenceWKT { get; set; }

    public string CoordinateSystemFileName { get; set; }
    public DateTime? CoordinateSystemLastActionedUTC { get; set; }

    public bool IsArchived { get; set; }

    public override bool Equals(object obj)
    {
      if (!(obj is ProjectData otherProject))
      {
        return false;
      }

      return otherProject.ProjectUID == ProjectUID
        && otherProject.ShortRaptorProjectId == ShortRaptorProjectId
        && otherProject.ProjectType == ProjectType
        && otherProject.Name == Name
        && otherProject.ProjectTimeZone == ProjectTimeZone
        && otherProject.IanaTimeZone == IanaTimeZone
        && otherProject.CustomerUID == CustomerUID
        && otherProject.ProjectGeofenceWKT == ProjectGeofenceWKT
        && otherProject.CoordinateSystemFileName == CoordinateSystemFileName
        && otherProject.CoordinateSystemLastActionedUTC == CoordinateSystemLastActionedUTC
        && otherProject.IsArchived == IsArchived;
    }

    public List<string> GetIdentifiers()
    {
      return new List<string>
      {
        CustomerUID,
        ProjectUID.ToString()
      };
    }
  }
}
