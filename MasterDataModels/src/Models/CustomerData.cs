﻿using Newtonsoft.Json;

namespace MasterDataModels.Models
{
  /// <summary>
  ///   Describes VL customer
  /// </summary>
  public class CustomerData : IData
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


    /// <summary>
    /// Key to use for caching customer master data
    /// </summary>
    [JsonIgnore]
    public string CacheKey => uid;
  }
}
