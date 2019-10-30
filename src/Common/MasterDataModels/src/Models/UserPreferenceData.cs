﻿namespace VSS.MasterData.Models.Models
{
  /// <summary>
  ///  Describes user preference data returned by the preference master data service.
  /// </summary>
  public class UserPreferenceData
  {
    public string Timezone { get; set; }
    public string Language { get; set; }
    public string Units { get; set; }
    public string DateFormat { get; set; }
    public string TimeFormat { get; set; }
    public string ThousandsSeparator { get; set; }
    public string DecimalSeparator { get; set; }
    public string DecimalPrecision { get; set; }
    public string AssetLabelDisplay { get; set; }
    public string MeterLabelDisplay { get; set; }
    public string LocationDisplay { get; set; }
    public string CurrencySymbol { get; set; }
    public string TemperatureUnit { get; set; }
    public string PressureUnit { get; set; }
    public string MapProvider { get; set; }
    public string BrowserRefresh { get; set; }

    public static UserPreferenceData GetDefaultSIPreferences()
    {
      return new UserPreferenceData()
        {
          AssetLabelDisplay = "Asset ID",
          BrowserRefresh = "Hourly",
          CurrencySymbol = "US Dollar",
          DateFormat = "dd/MM/yy",
          DecimalPrecision = "1",
          DecimalSeparator = ",",
          Language = "en-US",
          LocationDisplay = "Address",
          MapProvider = "ALK",
          MeterLabelDisplay = "Hour Meter",
          PressureUnit = "BAR",
          TemperatureUnit = "Celsius",
          ThousandsSeparator = ".",
          TimeFormat = "HH: mm",
          Timezone = "UTC",
          Units = "Metric"
      };
    }

  }
}
