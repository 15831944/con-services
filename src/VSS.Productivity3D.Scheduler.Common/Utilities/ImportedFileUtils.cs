﻿using System;
using System.IO;

namespace VSS.Productivity3D.Scheduler.Common.Utilities
{
  public static class ImportedFileUtils
  {

    // NhOp includes surveyedUtc/s in name, but Project does not. Samples:
    // JB topo southern motorway_2010-11-29T153300Z.TTM   SS=2010-11-29 15:33:00.0000000
    // Aerial Survey 120819_2012-08-19T035400Z_2016-08-16T003724Z.TTM ssUtc=2016-08-16 00:37:24.0000000
    public static string IncludeSurveyedUtcInName(string name, DateTime surveyedUtc)
    {
      //Note: ':' is an invalid character for filenames in Windows so get rid of them
      return Path.GetFileNameWithoutExtension(name) +
             "_" + surveyedUtc.ToIso8601DateTimeString().Replace(":", string.Empty) +
             Path.GetExtension(name);
    }

    public static string RemoveSurveyedUtcFromName(string name)
    {
      var shortFileName = Path.GetFileNameWithoutExtension(name);
      var format = "yyyy-MM-ddTHHmmssZ";
      if (shortFileName.Length <= format.Length)
        return name;
      return shortFileName.Substring(0, shortFileName.Length - format.Length - 1) + Path.GetExtension(name);
    }

    /// <summary>
    /// Construct the Iso8601 formatted date time for a UTC date time.
    /// </summary>
    /// <param name="dateTimeUtc">The date time in UTC</param>
    /// <returns>Iso8601 formatted string</returns>
    public static string ToIso8601DateTimeString(this DateTime dateTimeUtc)
    {
      // CAUTION - this assumes the DateTime passed in is already UTC!!
      return $"{dateTimeUtc:yyyy-MM-ddTHH:mm:ssZ}";
    }

  }
}
