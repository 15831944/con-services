﻿using System;

namespace TagFileHarvester.Models
{
  [Serializable]
  public class Bookmark
  {
    private DateTime bookmarkUtc;
    private DateTime lastCycleStartDateTime;
    private int lastFilesErrorneous;


    private int lastFilesProcessed;
    private int lastFilesRefused;

    public DateTime BookmarkUTC
    {
      get => bookmarkUtc;
      set => bookmarkUtc = value;
    }

    public DateTime LastUpdateDateTime { get; set; }

    public DateTime LastTCCScanDateTime { get; set; }

    public DateTime LastCycleStartDateTime
    {
      get => lastCycleStartDateTime;
      set
      {
        CycleLength = (int) (LastCycleStopDateTime - LastCycleStartDateTime).TotalSeconds;
        lastCycleStartDateTime = value;
      }
    }

    public DateTime LastCycleStopDateTime { get; set; }
    public long CyclesCompleted { get; set; }
    public bool OrgIsDisabled { get; set; }
    public int CycleLength { get; set; }
    public bool InProgress { get; set; }

    public int LastFilesProcessed
    {
      get => lastFilesProcessed;
      set
      {
        lastFilesProcessed = value;
        TotalFilesProcessed += value;
      }
    }

    public int LastFilesErrorneous
    {
      get => lastFilesErrorneous;
      set
      {
        lastFilesErrorneous = value;
        TotalFilesErrorneous += value;
      }
    }

    public int LastFilesRefused
    {
      get => lastFilesRefused;
      set
      {
        lastFilesRefused = value;
        TotalFilesRefused += value;
      }
    }


    public string OrgName { get; set; }

    public long TotalFilesProcessed { get; set; }
    public long TotalFilesErrorneous { get; set; }
    public long TotalFilesRefused { get; set; }

    public override bool Equals(object obj)
    {
      // If parameter is null return false.
      if (obj == null) return false;

      // If parameter cannot be cast to Point return false.
      var p = obj as Bookmark;
      if (p == null) return false;

      // Return true if the fields match:
      return BookmarkUTC == p.BookmarkUTC;
    }

    public static void AssignBookmark(Bookmark source, Bookmark dest)
    {
      dest.BookmarkUTC = source.BookmarkUTC;
      dest.InProgress = source.InProgress;
      dest.LastCycleStartDateTime = source.LastCycleStartDateTime;
      dest.LastCycleStopDateTime = source.LastCycleStopDateTime;
      dest.LastFilesErrorneous = source.LastFilesErrorneous;
      dest.LastFilesProcessed = source.LastFilesProcessed;
      dest.LastFilesRefused = source.LastFilesRefused;
      dest.LastUpdateDateTime = source.LastUpdateDateTime;
      dest.OrgIsDisabled = source.OrgIsDisabled;
      dest.OrgName = source.OrgName;
      dest.TotalFilesErrorneous = source.TotalFilesErrorneous;
      dest.TotalFilesProcessed = source.TotalFilesProcessed;
      dest.TotalFilesRefused = source.TotalFilesRefused;
    }
  }


  //key here is a OrgShortName. We don;t need to store filespaceID as this is repository-independed solution
  [Serializable]
  public class Bookmarks : DictionaryProxy<string, Bookmark>
  {
  }
}