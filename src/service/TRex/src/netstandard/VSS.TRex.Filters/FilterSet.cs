﻿using System.Linq;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;

namespace VSS.TRex.Filters
{
  /// <summary>
  /// FilterSet represents a set of filters to be applied to each subgrid in a query within a single operation
  /// </summary>
  public class FilterSet : IFilterSet
  {
    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// The list of combined attribute and spatial filters to be used
    /// </summary>
    public ICombinedFilter[] Filters { get; set; }

    /// <summary>
    /// Default no-arg constructor that creates a zero-sized array of combined filters
    /// </summary>
    public FilterSet()
    {
      Filters = new ICombinedFilter[0];
    }

    /// <summary>
    /// Constructor accepting a single filters to be set into the filter set
    /// Null filters are not incorporated into the resulting filter set
    /// </summary>
    /// <param name="filter"></param>
    public FilterSet(ICombinedFilter filter)
    {      
      Filters = filter != null ? new [] { filter } : new ICombinedFilter[0];
    }

    /// <summary>
    /// Constructor accepting a pair of filter to be set into the filter set
    /// Null filters are not incorporated into the resulting filter set
    /// </summary>
    /// <param name="filter1"></param>
    /// <param name="filter2"></param>
    public FilterSet(ICombinedFilter filter1, ICombinedFilter filter2)
    {
      Filters = filter1 == null && filter2 == null 
        ? new ICombinedFilter[0] 
        : filter2 == null 
          ? new[] { filter1 } 
          : filter1 == null 
            ? new [] {filter2} 
            : new [] { filter1, filter2 };
    }

    /// <summary>
    /// Constructor accepting a pre-initialized array of filters to be included in the filter set
    /// </summary>
    /// <param name="filters"></param>
    public FilterSet(ICombinedFilter[] filters)
    {
      if (filters == null || filters.Length == 0)
        Filters = new ICombinedFilter[0];
      else
        Filters = filters.Where(x => x != null).ToArray();
    }

    /// <summary>
    /// Applies spatial filter restrictions to the extents required to request data for.
    /// </summary>
    /// <param name="extents"></param>
    public void ApplyFilterAndSubsetBoundariesToExtents(BoundingWorldExtent3D extents)
    {
      foreach (var filter in Filters)
      {
        filter?.SpatialFilter?.CalculateIntersectionWithExtents(extents);
      }
    }

    public void ToBinary(IBinaryRawWriter writer)
    {
      writer.WriteByte(VERSION_NUMBER);

      writer.WriteInt(Filters.Length);
      foreach (var filter in Filters)
      { 
        // Handle cases where filter entry is null
        writer.WriteBoolean(filter != null);
        filter?.ToBinary(writer);
      }
    }

    public void FromBinary(IBinaryRawReader reader)
    {
      const int MAX_REASONABLE_NUMBER_OF_FILTERS = 100;
      byte readVersionNumber = reader.ReadByte();

      if (readVersionNumber != VERSION_NUMBER)
        throw new TRexSerializationVersionException(VERSION_NUMBER, readVersionNumber);

      var filterCount = reader.ReadInt();
      if (!Range.InRange(filterCount, 0, MAX_REASONABLE_NUMBER_OF_FILTERS))
        throw new TRexException($"Invalid number of filters {filterCount} in deserialisation");

      Filters = new ICombinedFilter[filterCount];
      for(int i = 0; i < Filters.Length; i++)
        Filters[i] = reader.ReadBoolean() ? new CombinedFilter(reader) : null;
    }
  }
}
