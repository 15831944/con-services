﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace VSS.MasterData.Proxies
{
  public static class LoggingExtensions
  {
    public static string LogHeaders(this IDictionary<string, string> headers)
    {
      return headers == null ? null : JsonConvert.SerializeObject(headers);
    }

    public static string LogHeaders(this IDictionary<string, string> headers, int logMaxChar)
    {
      return headers == null ? null : JsonConvert.SerializeObject(headers).Truncate(logMaxChar);
    }
  }
}
