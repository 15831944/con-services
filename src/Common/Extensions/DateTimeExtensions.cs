﻿using NodaTime;
using System;
using System.Globalization;
using VSS.MasterData.Models.Internal;

namespace VSS.Productivity3D.Common.Extensions
{
  public static class DateTimeExtensions
  {
    /// <summary>
    /// Determines if the string contains an ISO8601 date time
    /// </summary>
    /// <param name="inputStringUtc">The string to check</param>
    /// <param name="format">The format to use when checking</param>
    /// <returns>The date time from the string if ISO8601 else DateTime.MinDate</returns>
    public static DateTime IsDateTimeISO8601(this string inputStringUtc, string format)
    {
      DateTime utcDate = DateTime.MinValue;
      if (!string.IsNullOrWhiteSpace(inputStringUtc))
      {
        if (!DateTime.TryParseExact(inputStringUtc, format, new CultureInfo("en-US"), DateTimeStyles.AdjustToUniversal,
          out utcDate))
        {
          utcDate = DateTime.MinValue;
        }
      }
      return utcDate;
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

    /// <summary>
    /// Returns the date representing the Monday of the week the given date falls in.
    /// </summary>
    /// <param name="date">The date for which to find the start of the week</param>
    /// <returns>The date of the Monday of that week</returns>
    public static DateTime CurrentWeekMonday(this DateTime date)
    {
      int daysToSubtract = 0;
      switch (date.DayOfWeek)
      {
        case DayOfWeek.Sunday:
          daysToSubtract = 6;
          break;
        case DayOfWeek.Saturday:
          daysToSubtract = 5;
          break;
        case DayOfWeek.Friday:
          daysToSubtract = 4;
          break;
        case DayOfWeek.Thursday:
          daysToSubtract = 3;
          break;
        case DayOfWeek.Wednesday:
          daysToSubtract = 2;
          break;
        case DayOfWeek.Tuesday:
          daysToSubtract = 1;
          break;
        case DayOfWeek.Monday:
          daysToSubtract = 0;
          break;
      }
      return date.AddDays(-daysToSubtract);
    }

    /// <summary>
    /// Gets the offset from UTC for the time zone
    /// </summary>
    /// <param name="timeZoneName">The name of the time zone</param>
    /// <returns>The offset from UTC for the time zone</returns>
    public static TimeSpan TimeZoneOffsetFromUtc(this string timeZoneName)
    {
      var timeZone = DateTimeZoneProviders.Tzdb[timeZoneName];
      DateTime utcNow = DateTime.UtcNow;
      return timeZone.GetUtcOffset(Instant.FromDateTimeUtc(utcNow)).ToTimeSpan();
    }

    /// <summary>
    /// Get the start or end date time in a time zone for the specified date range type.
    /// </summary>
    /// <param name="nowInTimeZone">Now in time zone</param>
    /// <param name="dateRangeType">The date range type (today, current week etc.)</param>
    /// <param name="isStart">True for start and false for end of date range</param>
    /// <returns>The start or end date time for the range in the time zone</returns>
    public static DateTime DateTimeForDateRangeType(this DateTime nowInTimeZone, DateRangeType dateRangeType, bool isStart)
    {
      //Note: This date range is used for filters to pass to Raptor. So that Raptor's caching works we pass the end of the current day
      //rather than now for the end date time for current day/week/month. However in the filter service we use now as the value is
      //displayed in the UI.

      DateTime startToday = nowInTimeZone.Date;
      DateTime endToday = startToday.AddDays(1).AddSeconds(-1);
      var startThisWeek = startToday.CurrentWeekMonday();
      var startThisMonth = new DateTime(startToday.Year, startToday.Month, 1);

      DateTime dateTimeInTimeZone = DateTime.MinValue;
      switch (dateRangeType)
      {
        case DateRangeType.Today:
          dateTimeInTimeZone = isStart ? startToday : endToday;
          break;
        case DateRangeType.Yesterday:
          dateTimeInTimeZone = isStart ? startToday.AddDays(-1) : startToday.AddSeconds(-1);
          break;
        case DateRangeType.CurrentWeek:
          dateTimeInTimeZone = isStart ? startThisWeek : endToday;
          break;
        case DateRangeType.PreviousWeek:
          dateTimeInTimeZone = isStart ? startThisWeek.AddDays(-7) : startThisWeek.AddSeconds(-1);
          break;
        case DateRangeType.CurrentMonth:
          dateTimeInTimeZone = isStart ? startThisMonth : endToday;
          break;
        case DateRangeType.PreviousMonth:
          dateTimeInTimeZone = isStart ? startThisMonth.AddMonths(-1) : startThisMonth.AddSeconds(-1);
          break;
        case DateRangeType.ProjectExtents:
        case DateRangeType.Custom:
          //do nothing
          break;
      }
      return dateTimeInTimeZone;
    }

    /// <summary>
    /// Get the start or end date time in UTC for the specified date range type in the specified time zone.
    /// </summary>
    /// <param name="utcNow">Now in UTC</param>
    /// <param name="dateRangeType">The date range type (today, current week etc.)</param>
    /// <param name="timeZoneName">The IANA time zone name</param>
    /// <param name="isStart">True for start and false for end of date range</param>
    /// <returns>The start or end UTC for the range in the time zone</returns>
    public static DateTime? UtcForDateRangeType(this DateTime utcNow, DateRangeType dateRangeType, string timeZoneName, bool isStart)
    {
      if (dateRangeType == DateRangeType.Custom || dateRangeType == DateRangeType.ProjectExtents || string.IsNullOrEmpty(timeZoneName))
      {
        return null;
      }

      var offset = timeZoneName.TimeZoneOffsetFromUtc();
      DateTime nowInTimeZone = utcNow + offset;

      return nowInTimeZone.DateTimeForDateRangeType(dateRangeType, isStart) - offset;
    }
  }
}