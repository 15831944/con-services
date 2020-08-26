﻿using System.Linq;
using Apache.Ignite.Core.Binary;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common;
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
    private static readonly ILogger _log = Logging.Logger.CreateLogger<FilterSet>();

    private const byte VERSION_NUMBER = 1;

    public const int MAX_REASONABLE_NUMBER_OF_FILTERS = 10;

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
    public FilterSet(ICombinedFilter filter)
    {
      Filters = filter != null ? new [] { filter } : new ICombinedFilter[0];
    }

    /// <summary>
    /// Constructor accepting a pair of filter to be set into the filter set
    /// Null filters are not incorporated into the resulting filter set
    /// </summary>
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
    public void ApplyFilterAndSubsetBoundariesToExtents(BoundingWorldExtent3D extents)
    {
      foreach (var filter in Filters)
      {
        filter?.SpatialFilter?.CalculateIntersectionWithExtents(extents);
      }
    }

    public void ToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

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
      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        var filterCount = reader.ReadInt();
        if (!Range.InRange(filterCount, 0, MAX_REASONABLE_NUMBER_OF_FILTERS))
        {
          _log.LogError("$Invalid number of filters { filterCount} in deserialisation. Setting to a single default filter");
          Filters = new ICombinedFilter[] {new CombinedFilter()};
        }
        else
        {
          Filters = new ICombinedFilter[filterCount];
          for (var i = 0; i < Filters.Length; i++)
            Filters[i] = reader.ReadBoolean() ? new CombinedFilter(reader) : null;
        }
      }
    }
  }
}
