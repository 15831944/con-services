﻿using Newtonsoft.Json;
using System;
using System.Linq;

namespace VSS.Project.Data
{
  /// <summary>
  /// Encapsulates time zone conversion functions
  /// </summary>
  public class TimeZone
  {
    // This will return the Windows zone that matches the IANA zone, if one exists.
    public static string IanaToWindows(string ianaZoneId)
    {
      var utcZones = new[] { "Etc/UTC", "Etc/UCT" };
      if (utcZones.Contains(ianaZoneId, StringComparer.OrdinalIgnoreCase))
        return "UTC";

      var tzdbSource = NodaTime.TimeZones.TzdbDateTimeZoneSource.Default;

      // resolve any link, since the CLDR doesn't necessarily use canonical IDs
      var links = tzdbSource.CanonicalIdMap
          .Where(x => x.Value.Equals(ianaZoneId, StringComparison.OrdinalIgnoreCase))
          .Select(x => x.Key);

      var mappings = tzdbSource.WindowsMapping.MapZones;
      var item = mappings.FirstOrDefault(x => x.TzdbIds.Any(links.Contains));
      if (item == null) return null;
      return item.WindowsId;
    }

    // This will return the "primary" IANA zone that matches the given windows zone.
    // If the primary zone is a link, it then resolves it to the canonical ID.
    public static string WindowsToIana(string windowsZoneId)
    {
      if (String.IsNullOrEmpty(windowsZoneId))
        return String.Empty;

      if (windowsZoneId.Equals("UTC", StringComparison.OrdinalIgnoreCase))
        return "Etc/UTC";

      var tzdbSource = NodaTime.TimeZones.TzdbDateTimeZoneSource.Default;

      /* temp display stuff */
      //var OSZones = string.Format("systems tzones {0}", JsonConvert.SerializeObject(TimeZoneInfo.GetSystemTimeZones().Select(t => t)));
      //Console.WriteLine("WindowsToIana: windowsZoneId {0} the OSystems tzones {1}", windowsZoneId, OSZones);
      //var nodaMappings = string.Format("systems tzones {0}", JsonConvert.SerializeObject(TimeZoneInfo.GetSystemTimeZones().Select(t => t)));
      //Console.WriteLine("the Noda tzones {0}", nodaMappings);

      // map the windows id to an iana one - regardless of platform
      var mappings = tzdbSource.WindowsMapping.MapZones;
      var item = mappings.FirstOrDefault(x => x.WindowsId == windowsZoneId);
      if (item == null || item.TzdbIds.Count == 0) return String.Empty;

      return item.TzdbIds[0];

      // this doesn't work on linux
      //var tzdbSource = NodaTime.TimeZones.TzdbDateTimeZoneSource.Default;
      //var tzi = TimeZoneInfo.FindSystemTimeZoneById(windowsZoneId);
      //var tzid = tzdbSource.MapTimeZoneId(tzi);
      //return tzdbSource.CanonicalIdMap[tzid];

    }

  }
}