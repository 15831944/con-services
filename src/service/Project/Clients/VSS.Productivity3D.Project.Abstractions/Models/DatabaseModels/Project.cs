﻿using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels
{
  public class Project
  {
    public string ProjectUID { get; set; }

    // legacy ProjectID in Gen2 is a bigint. However Raptor can't handle one, and we're unlikely to need to get that big.
    public int LegacyProjectID { get; set; }

    public ProjectType ProjectType { get; set; }

    public string Name { get; set; }
    public string Description { get; set; }

    public string ProjectTimeZone { get; set; }

    // This should really be named ProjectTimeZoneIana.
    //     It is required for all projects, not just landfill.
    //     ProjectTimeZone is in Windows StandardTime name,
    //         which the UI,and ProjectSvc limit to a known set (contained in PreferencesTimeZones.cs).
    public string LandfillTimeZone { get; set; }

    // start and end are actually only date with no time component. However C# has no date-only.
    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }


    public string CustomerUID { get; set; }

    // legacy CustomerID in Gen2 is a bigint. Unlike LegacyProjectID, this is passed around as a long. I don't know why.
    public long LegacyCustomerID { get; set; }

    public string SubscriptionUID { get; set; }
    public DateTime? SubscriptionStartDate { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
    public int ServiceTypeID { get; set; } = 0;

    public string GeometryWKT { get; set; }

    public string CoordinateSystemFileName { get; set; }
    public DateTime? CoordinateSystemLastActionedUTC { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime LastActionedUTC { get; set; }

    public override bool Equals(object obj)
    {
      if (!(obj is Project otherProject))
      {
        return false;
      }

      return otherProject.ProjectUID == ProjectUID
        && otherProject.LegacyProjectID == LegacyProjectID
        && otherProject.ProjectType == ProjectType
        && otherProject.Name == Name
        && otherProject.Description == Description
        && otherProject.ProjectTimeZone == ProjectTimeZone
        && otherProject.LandfillTimeZone == LandfillTimeZone
        && otherProject.StartDate == StartDate
        && otherProject.EndDate == EndDate
        && otherProject.CustomerUID == CustomerUID
        && otherProject.LegacyCustomerID == LegacyCustomerID
        && otherProject.SubscriptionUID == SubscriptionUID
        && otherProject.SubscriptionStartDate == SubscriptionStartDate
        && otherProject.SubscriptionEndDate == SubscriptionEndDate
        && otherProject.ServiceTypeID == ServiceTypeID
        && otherProject.GeometryWKT == GeometryWKT
        && otherProject.CoordinateSystemFileName == CoordinateSystemFileName
        && otherProject.CoordinateSystemLastActionedUTC == CoordinateSystemLastActionedUTC
        && otherProject.IsDeleted == IsDeleted
        && otherProject.LastActionedUTC == LastActionedUTC;
    }

    public override int GetHashCode()
    {
      return 0;
    }
  }
}