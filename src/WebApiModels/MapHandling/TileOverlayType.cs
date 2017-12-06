﻿namespace VSS.Productivity3D.WebApi.Models.MapHandling
{
  /// <summary>
  /// The types of data that can be overlayed in a map tile for reports
  /// </summary>
  public enum TileOverlayType
  {
    BaseMap,
    ProjectBoundary,
    ProductionData,
    DxfLinework,
    Alignments,
    Geofences
  }
}
