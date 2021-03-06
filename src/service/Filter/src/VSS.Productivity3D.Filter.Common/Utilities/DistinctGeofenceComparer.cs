﻿using System.Collections.Generic;
using VSS.MasterData.Models.Models;

namespace VSS.Productivity3D.Filter.Common.Utilities
{
  /// <summary>
  /// For comparing two geofences for equality. Only uses relevant properties in the comparison.
  /// </summary>
  public class DistinctGeofenceComparer : IEqualityComparer<GeofenceData>
  {
    public bool Equals(GeofenceData x, GeofenceData y)
    {
      if (ReferenceEquals(x, y))
        return true;

      //Note: associated geofences don't set the CustomerUID so don't include it here
      return x != null && y != null &&
             x.GeofenceUID == y.GeofenceUID &&
             x.GeofenceType == y.GeofenceType &&
             x.GeofenceName == y.GeofenceName &&
             x.GeometryWKT == y.GeometryWKT;
    }

    public int GetHashCode(GeofenceData obj)
    {
      unchecked
      {
        var hashCode = obj.GeofenceUID.GetHashCode();
        hashCode = (hashCode * 397) ^ obj.GeofenceType.GetHashCode();
        hashCode = (hashCode * 397) ^ obj.GeofenceName.GetHashCode();
        hashCode = (hashCode * 397) ^ obj.GeometryWKT.GetHashCode();
        return hashCode;
      }
    }
  }
}
