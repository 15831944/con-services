﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TagFileHarvester.Models
{
  public class LoginResult
  {
    [JsonProperty(PropertyName = "success", Required = Required.Default)]
    public bool success;
    [JsonProperty(PropertyName = "ticket", Required = Required.Default)]
    public string ticket;
    [JsonProperty(PropertyName = "errorid", Required = Required.Default)]
    public string errorid;
    [JsonProperty(PropertyName = "orgName", Required = Required.Default)]
    public string orgName;
    [JsonProperty(PropertyName = "reason", Required = Required.Default)]
    public string reason;
    [JsonProperty(PropertyName = "username", Required = Required.Default)]
    public string username;
  }
}
