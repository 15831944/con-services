﻿using System.Collections.Generic;
using VSS.Common.Abstractions.MasterData.Interfaces;

namespace VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling
{
  /// <summary>
  ///   Describes VL customer
  /// </summary>
  public class CustomerData : IMasterDataModel
  {
    /// <summary>
    /// Gets or sets the customer uid.
    /// </summary>
    /// <value>
    /// The customer uid.
    /// </value>
    public string uid { get; set; }

    /// <summary>
    /// Gets or sets the customer name.
    /// </summary>
    /// <value>
    /// The customer name.
    /// </value>
    public string name { get; set; }

    /// <summary>
    /// Gets or sets the type of customer
    /// </summary>
    /// <value>
    /// The Customer Type
    /// </value>
    public string type { get; set; }

    public List<string> GetIdentifiers() => new List<string>
    {
      uid
    };
  }
}
