﻿namespace VSS.MasterData.Models.Utilities
{
  /// <summary>
  /// Utility class that defines constants used by data validation.
  /// </summary>
  public class ValidationConstants3D : ValidationConstants
  {
    /// <summary>
    /// Minimum station value.
    /// </summary>
    public const double MIN_STATION = -10000;

    /// <summary>
    /// Maximum station value.
    /// </summary>
    public const double MAX_STATION = 1000000;

    /// <summary>
    /// Minimum offset value.
    /// </summary>
    public const double MIN_OFFSET = -500;

    /// <summary>
    /// Maximum offset value.
    /// </summary>
    public const double MAX_OFFSET = 500;

    /// <summary>
    /// Minimum elevation value.
    /// </summary>
    public const double MIN_ELEVATION = -10000;

    /// <summary>
    /// Maximum elevation value.
    /// </summary>
    public const double MAX_ELEVATION = 10000;

    /// <summary>
    /// Maximum production data thickness.
    /// </summary>
    public const double MAX_THICKNESS = 100;

    /// <summary>
    /// Minimum production data thickness.
    /// </summary>
    public const double MIN_THICKNESS = 0.005;

    /// <summary>
    /// Minimum production data no change tolerance for volumes.
    /// </summary>
    public const double MIN_NO_CHANGE_TOLERANCE = 0.0;

    /// <summary>
    /// Maximum production data no change tolerance for volumes.
    /// </summary>
    public const double MAX_NO_CHANGE_TOLERANCE = 0.1;

    /// <summary>
    /// Minimum spacing interval for the sampled points.
    /// </summary>
    public const double MIN_SPACING_INTERVAL = 0.1;

    /// <summary>
    /// Maximum spacing interval for the sampled points.
    /// </summary>
    public const double MAX_SPACING_INTERVAL = 100.0;

    /// <summary>
    /// Default spacing interval for the sampled points.
    /// </summary>
    public const double DEFAULT_SPACING_INTERVAL = 1.0;

    /// <summary>
    /// Production data minimum patch size.
    /// </summary>
    public const int MIN_PATCH_SIZE = 1;

    /// <summary>
    /// Production data maximum patch size
    /// </summary>
    public const int MAX_PATCH_SIZE = 1000;

    /// <summary>
    /// Minimum number of production data patches.
    /// </summary>
    public const int MIN_PATCH_NUM = 0;

    /// <summary>
    /// Maximum number of production data patches
    /// </summary>
    public const int MAX_PATCH_NUM = 1000;
  }
}
