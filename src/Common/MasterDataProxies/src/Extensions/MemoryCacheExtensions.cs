﻿using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Serilog.Extensions;

namespace VSS.MasterData.Proxies
{
  public static class MemoryCacheExtensions
  {
    /// <summary>
    /// Get the cache options for the cache items. Sets the cache life.
    /// </summary>
    /// <param name="cacheLifeKey">The configuration key for the cache life</param>
    /// <returns>Memory cache options for the items</returns>
    public static MemoryCacheEntryOptions GetCacheOptions(this MemoryCacheEntryOptions opts, string cacheLifeKey, IConfigurationStore configurationStore,
      ILogger log)
    {
      const string defaultTimespanMessage = "Using default 15 mins.";

      var cacheLife = configurationStore.GetValueString(cacheLifeKey);

      if (log.IsTraceEnabled())
        log.LogTrace($"Cache Life: {cacheLifeKey}: {cacheLife}");

      if (string.IsNullOrEmpty(cacheLife))
      {
        log.LogWarning(
          $"Your application is missing an environment variable {cacheLifeKey}. {defaultTimespanMessage}");
        cacheLife = "00:15:00";
      }

      if (!TimeSpan.TryParse(cacheLife, out var result))
      {
        log.LogWarning($"Invalid timespan for environment variable {cacheLifeKey}. {defaultTimespanMessage}");
        result = new TimeSpan(0, 15, 0);
      }

      opts.SlidingExpiration = result;
      return opts;
    }
  }
}
