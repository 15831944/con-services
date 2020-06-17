﻿using System;

namespace TagFiles.Utils
{
  public static class TagUtils
  {

    private static readonly DateTime UnixEpoch =
        new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static long GetCurrentUnixTimestampMillis()
    {
      return (long)(DateTime.UtcNow - UnixEpoch).TotalMilliseconds;
    }

    public static long GetCustomTimestampMillis(DateTime customDate)
    {
      return (long)(customDate - UnixEpoch).TotalMilliseconds;
    }

    public static DateTime DateTimeFromUnixTimestampMillis(long millis)
    {
      return UnixEpoch.AddMilliseconds(millis);
    }

    public static long GetCurrentUnixTimestampSeconds()
    {
      return (long)(DateTime.UtcNow - UnixEpoch).TotalSeconds;
    }

    public static DateTime DateTimeFromUnixTimestampSeconds(long seconds)
    {
      return UnixEpoch.AddSeconds(seconds);
    }

    public static double ToRadians(double val)
    {
      return (Math.PI / 180) * val;
    }

    public static string MakeTagfileName(string deviceID, string machineName)
    {
      if (deviceID == "")
        deviceID = "unknowndeviceid";
      if (machineName == "")
        machineName = "unknownname";
      return $"{deviceID}--{machineName}--{DateTime.UtcNow.ToString("yyMMddHHmmss")}.tag"; 
    }

    public static DateTime UnixTimeStampToUTCDateTime(double unixTimeStamp)
    {
      // Unix timestamp is seconds past epoch
      DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
      dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToUniversalTime();
      return dtDateTime;
    }

    public static byte ConvertToMachineType(string value)
    {
      var upperValue = value.ToUpper();
      switch (upperValue)
      {
        case "CSD": return 70;
        case "HEX": return 71;
        default:
          {
            throw new System.ArgumentException($"Unknown machine type supplied. value:{value}");
          }
      }
    }

  }
}