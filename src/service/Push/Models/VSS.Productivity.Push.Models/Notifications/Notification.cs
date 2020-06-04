﻿using System;
using Newtonsoft.Json;
using VSS.Productivity.Push.Models.Enums;

namespace VSS.Productivity.Push.Models.Notifications
{
  public class Notification
  {
    /// <summary>
    /// Only used by Json - do not call directly
    /// </summary>
    [JsonConstructor]
    private Notification()
    { }

    /// <summary>
    /// Constructor to create a notification
    /// </summary>
    /// <param name="key">Notification Key to identify the type of notification</param>
    /// <param name="uid">Uid for the Type (e.g ProjectUid or CustomerUid)</param>
    /// <param name="type">Type of Uid Provided (e.g Customer or Project)</param>
    public Notification(string key, Guid uid, NotificationUidType type)
    {
      Key = key;
      Uid = uid;
      Type = type;
    }

    /// <summary>
    /// Constructor to create a notification with arbitrary parameters
    /// </summary>
    /// <param name="key">Notification Key to identify the type of notification</param>
    /// <param name="parameters">Some data that is specific to the notification</param>
    /// <param name="type">Type of Uid Provided (e.g Customer or Project)</param>
    public Notification(string key, object parameters, NotificationUidType type)
    {
      Key = key;
      Parameters = parameters;
      Type = type;
    }

    /// <summary>
    /// Uid for the Type provided
    /// </summary>
    [JsonProperty]
    public Guid? Uid { get; set; }

    /// <summary>
    /// Uid Type
    /// </summary>
    [JsonProperty]
    public NotificationUidType Type { get; set; }

    /// <summary>
    /// Uid Type string 
    /// </summary>
    [JsonProperty] 
    public string TypeString => Type.ToString();

    /// <summary>
    /// Notification Key
    /// </summary>
    [JsonProperty]
    public string Key { get; set; }

    /// <summary>
    /// Notification parameters
    /// </summary>
    [JsonProperty]
    public object Parameters { get; set; }
  }
}
