﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.ExistenceMaps.Interfaces;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;

namespace VSS.TRex.SurveyedSurfaces
{
  public class SurveyedSurfaces : List<ISurveyedSurface>, IComparable<ISurveyedSurface>, ISurveyedSurfaces
  {
    private const byte kMajorVersion = 1;
    private const byte kMinorVersion = 3;

    private bool FSorted;
    private bool SortDescending;

    public bool Sorted => FSorted;

    private IExistenceMaps existenceMaps;
    private IExistenceMaps GetExistenceMaps() => existenceMaps ?? (existenceMaps = DIContext.Obtain<IExistenceMaps>());

    /// <summary>
    /// No-arg constructor
    /// </summary>
    public SurveyedSurfaces()
    {
    }

    /// <summary>
    /// Constructor accepting a Binary Reader instance from which to instantiate itself
    /// </summary>
    /// <param name="reader"></param>
    public SurveyedSurfaces(BinaryReader reader)
    {
      Read(reader);
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(kMajorVersion);
      writer.Write(kMinorVersion);
      writer.Write(Count);

      foreach (ISurveyedSurface ss in this)
      {
        ss.Write(writer);
      }
    }

    public void Read(BinaryReader reader)
    {
      ReadVersionFromStream(reader, out byte MajorVersion, out byte MinorVersion);

      if (MajorVersion != kMajorVersion)
      {
        throw new FormatException("Major version incorrect");
      }

      if (MinorVersion != kMinorVersion)
      {
        throw new FormatException("Minor version incorrect");
      }

      int theCount = reader.ReadInt32();
      for (int i = 0; i < theCount; i++)
      {
        SurveyedSurface surveyedSurface = new SurveyedSurface();
        surveyedSurface.Read(reader);
        Add(surveyedSurface);
      }
    }

    private void ReadVersionFromStream(BinaryReader reader, out byte MajorVersion, out byte MinorVersion)
    {
      // Load file version info
      MajorVersion = reader.ReadByte();
      MinorVersion = reader.ReadByte();
    }

    /// <summary>
    /// Create a new surveyed surface in the list based on the provided details
    /// </summary>
    /// <param name="surveyedSurfaceUid"></param>
    /// <param name="designDescriptor"></param>
    /// <param name="asAtDate"></param>
    /// <param name="extents"></param>
    /// <returns></returns>
    public ISurveyedSurface AddSurveyedSurfaceDetails(Guid surveyedSurfaceUid,
      DesignDescriptor designDescriptor,
      DateTime asAtDate,
      BoundingWorldExtent3D extents)
    {
      ISurveyedSurface match = Find(x => x.ID == surveyedSurfaceUid);

      if (match != null)
      {
        return match;
      }

      ISurveyedSurface ss = new SurveyedSurface(surveyedSurfaceUid, designDescriptor, asAtDate, extents);
      Add(ss);

      Sort();

      return ss;
    }

    /// <summary>
    /// Remove a given surveyed surface from the list of surveyed surfaces for a site model
    /// </summary>
    /// <param name="surveyedSurfaceUid"></param>
    /// <returns></returns>
    public bool RemoveSurveyedSurface(Guid surveyedSurfaceUid)
    {
      ISurveyedSurface match = Find(x => x.ID == surveyedSurfaceUid);

      return match != null && Remove(match);
    }

    /// <summary>
    /// Locates a surveyed surface in the list with the given GUID
    /// </summary>
    /// <param name="surveyedSurfaceUid"></param>
    /// <returns></returns>
    public ISurveyedSurface Locate(Guid surveyedSurfaceUid)
    {
      // Note: This happens a lot and the for loop is faster than foreach or Find(x => x.ID)
      // If numbers of surveyed surfaces become large a Dictionary<Guid, SS> would be good...
      for (int i = 0; i < Count; i++)
        if (this[i].ID == surveyedSurfaceUid)
          return this[i];

      return null;
    }

    public void Assign(ISurveyedSurfaces source)
    {
      Clear();

      foreach (ISurveyedSurface ss in source)
      {
        Add(ss); // formerly Add(ss.Clone());
      }
    }

    public void SortChronologically(bool Descending = true)
    {
      SortDescending = Descending;

      Sort();

      FSorted = true;
    }

    private new void Sort()
    {
      Sort((x, y) => SortDescending ? y.AsAtDate.CompareTo(x.AsAtDate) : x.AsAtDate.CompareTo(y.AsAtDate));
    }

    /// <summary>
    /// Determines if there is at least one surveyed surface with an as at date later than the data provided as a DateTime
    /// Optimal performance will be observed if the list is sorted in ascending chronological order
    /// </summary>
    /// <param name="timeStamp"></param>
    /// <returns></returns>
    public bool HasSurfaceLaterThan(DateTime timeStamp)
    {
      for (int i = Count - 1; i >= 0; i--)
      {
        if (this[i].AsAtDate.CompareTo(timeStamp) > 0)
        {
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Determines if there is at least one surveyed surface with an as at date later than the data provided as a DateTime.ToBinary() Int64
    /// Optimal performance will be observed if the list is sorted in ascending chronological order
    /// </summary>
    /// <param name="timeStamp"></param>
    /// <returns></returns>
    public bool HasSurfaceLaterThan(long timeStamp)
    {
      DateTime _TimeStamp = DateTime.FromBinary(timeStamp);

      for (int i = Count - 1; i >= 0; i--)
      {
        if (this[i].AsAtDate.CompareTo(_TimeStamp) > 0)
        {
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Determines if there is at least one surveyed surface with an as at date earlier than the data provided as a DateTime
    /// Optimal performance will be observed if the list is sorted in ascending chronological order
    /// </summary>
    /// <param name="timeStamp"></param>
    /// <returns></returns>
    public bool HasSurfaceEarlierThan(DateTime timeStamp)
    {
      for (int i = 0; i < Count; i++)
      {
        if (this[i].AsAtDate.CompareTo(timeStamp) < 0)
        {
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Determines if there is at least one surveyed surface with an as at date earlier than the data provided as a DateTime.ToBinary() Int64
    /// Optimal performance will be observed if the list is sorted in ascending chronological order
    /// </summary>
    /// <param name="timeStamp"></param>
    /// <returns></returns>
    public bool HasSurfaceEarlierThan(long timeStamp)
    {
      DateTime _TimeStamp = DateTime.FromBinary(timeStamp);

      for (int i = 0; i < Count; i++)
      {
        if (this[i].AsAtDate.CompareTo(_TimeStamp) < 0)
        {
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Determine if the surveyed surfaces in this list are the same as the surveyed surfaces in the other list, based on ID comparison
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool IsSameAs(SurveyedSurfaces other)
    {
      if (Count != other.Count)
      {
        return false;
      }

      for (int I = 0; I < Count; I++)
      {
        if (this[I].ID != other[I].ID)
        {
          return false;
        }
      }

      return true;
    }

    /// <summary>
    /// Perform filtering on a set of surveyed surfaces according to the supplied time constraints.
    /// Note: The list of filtered surveyed surfaces is assumed to be empty at the point it is passed to this method
    /// </summary>
    /// <param name="HasTimeFilter"></param>
    /// <param name="StartTime"></param>
    /// <param name="EndTime"></param>
    /// <param name="ExcludeSurveyedSurfaces"></param>
    /// <param name="FilteredSurveyedSurfaceDetails"></param>
    /// <param name="ExclusionList"></param>
    public void FilterSurveyedSurfaceDetails(bool HasTimeFilter,
      DateTime StartTime, DateTime EndTime,
      bool ExcludeSurveyedSurfaces,
      ISurveyedSurfaces FilteredSurveyedSurfaceDetails,
      Guid[] ExclusionList)
    {
      if (ExcludeSurveyedSurfaces)
        return;

      if (!HasTimeFilter && (ExclusionList?.Length ?? 0) == 0)
      {
        FilteredSurveyedSurfaceDetails.Assign(this);
        return;
      }

      FilteredSurveyedSurfaceDetails.Clear();
      foreach (ISurveyedSurface ss in this)
      {
        if (!HasTimeFilter)
        {
          if (ExclusionList == null || !ExclusionList.Any(x => x == ss.ID)) // if SS not excluded from project
            FilteredSurveyedSurfaceDetails.Add(ss); // Formerly ss.Clone
        }
        else
        {
          if (ss.AsAtDate >= StartTime && ss.AsAtDate <= EndTime &&
              (ExclusionList == null || !ExclusionList.Any(x => x == ss.ID))) // if SS not excluded from project
            FilteredSurveyedSurfaceDetails.Add(ss); // Formerly ss.Clone
        }
      }
    }

    public int CompareTo(ISurveyedSurface other)
    {
      throw new NotImplementedException();
    }

    public void Write(BinaryWriter writer, byte[] buffer) => Write(writer);

    /// <summary>
    /// Given a filter compute which of the surfaces in the list match any given time aspect
    /// of the filter, and the overall existence map of the surveyed surfaces that match the filter.
    /// ComparisonList denotes a possibly pre-filtered set of surfaces for another filter; if this is the same as the 
    /// filtered set of surfaces then the overall existence map for those surfaces will not be computed as it is 
    /// assumed to be the same.
    /// </summary>
    /// <param name="surveyedSurfaceUid"></param>
    /// <param name="Filter"></param>
    /// <param name="ComparisonList"></param>
    /// <param name="FilteredSurveyedSurfaces"></param>
    /// <param name="OverallExistenceMap"></param>
    /// <returns></returns>
    public bool ProcessSurveyedSurfacesForFilter(Guid surveyedSurfaceUid,
      ICombinedFilter Filter,
      ISurveyedSurfaces ComparisonList,
      ISurveyedSurfaces FilteredSurveyedSurfaces,
      ISubGridTreeBitMask OverallExistenceMap)
    {
      // Filter out any surveyed surfaces which don't match current filter (if any) - realistically, this is time filters we're thinking of here
      FilterSurveyedSurfaceDetails(Filter.AttributeFilter.HasTimeFilter,
        Filter.AttributeFilter.StartTime, Filter.AttributeFilter.EndTime,
        Filter.AttributeFilter.ExcludeSurveyedSurfaces(),
        FilteredSurveyedSurfaces,
        Filter.AttributeFilter.SurveyedSurfaceExclusionList);

      if (FilteredSurveyedSurfaces != null)
      {
        if (FilteredSurveyedSurfaces.Equals(ComparisonList))
          return true;

        if (FilteredSurveyedSurfaces.Count > 0)
        {
          ISubGridTreeBitMask surveyedSurfaceExistenceMap = GetExistenceMaps().GetCombinedExistenceMap(surveyedSurfaceUid,
            FilteredSurveyedSurfaces.Select(x => new Tuple<long, Guid>(Consts.EXISTENCE_SURVEYED_SURFACE_DESCRIPTOR, x.ID)).ToArray());

          if (OverallExistenceMap == null)
            return false;

          if (surveyedSurfaceExistenceMap != null)
            OverallExistenceMap.SetOp_OR(surveyedSurfaceExistenceMap);
        }
      }

      return true;
    }
  }
}
