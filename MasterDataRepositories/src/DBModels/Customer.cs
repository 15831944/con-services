﻿using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace Repositories.DBModels
{
  public class Customer
  {
    public string Name { get; set; }
    public CustomerType CustomerType { get; set; }
    public string CustomerUID { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime LastActionedUTC { get; set; }

    public override bool Equals(object obj)
    {
      var otherProject = obj as Customer;
      if (otherProject == null) return false;
      return otherProject.CustomerUID == this.CustomerUID           
            && otherProject.Name == this.Name
            && otherProject.CustomerType == this.CustomerType  
            && otherProject.IsDeleted == this.IsDeleted          
            && otherProject.LastActionedUTC == this.LastActionedUTC
            ;
    }
    public override int GetHashCode() { return 0; }

  }
}
