﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MockProjectWebApi.Json
{
  public class JsonResourceHelper
  {
    // Remember to set the Build Action of new .json resource files to 'Embedded Resource'.

    public static string GetFilterJson(string resourceName)
    {
      var jObj = GetJsonFromEmbeddedResource($"Filters.{resourceName}");
      return jObj.ToString(Formatting.None);
    }

    public static string GetGoldenDimensionsFilterJson(string resourceName)
    {
      var jObj = GetJsonFromEmbeddedResource($"Filters.GoldenDimensions.{resourceName}");
      return jObj.ToString(Formatting.None);
    }

    public static string GetDimensionsFilterJson(string resourceName)
    {
      var jObj = GetJsonFromEmbeddedResource($"Filters.Dimensions.{resourceName}");
      return jObj.ToString(Formatting.None);
    }

    public static string GetKettlewellDriveFilterJson(string resourceName)
    {
      var jObj = GetJsonFromEmbeddedResource($"Filters.KettlewellDrive.{resourceName}");
      return jObj.ToString(Formatting.None);
    }

    public static string GetUserPreferencesJson(string resourceName)
    {
      var jObj = GetJsonFromEmbeddedResource($"UserPreferences.{resourceName}");
      return jObj.ToString(Formatting.None);
    }

    /// <summary>
    /// Gets the color settings JSON for a given project uid.
    /// </summary>
    public static JObject GetColorSettings(string projectUid)
    {
      return GetJsonFromEmbeddedResource($"ColorSettings.{projectUid}");
    }

    public static byte[] GetBaseMap(string resourceName)
    {
      return GetJsonFromEmbeddedResource<byte[]>($"ALK.{resourceName}");
    }

    private static JObject GetJsonFromEmbeddedResource(string resourceName)
    {
      return GetJsonFromEmbeddedResource<JObject>(resourceName);
    }

    private static T GetJsonFromEmbeddedResource<T>(string resourceName)
    {
      resourceName = $".Json.{resourceName}.json";

      // this allows for .Local solution
      resourceName = Assembly.GetEntryAssembly().GetManifestResourceNames().FirstOrDefault(n => n.Contains(resourceName));
      if (string.IsNullOrEmpty(resourceName))
      {
        throw new Exception($"Error attempting to find resource name {resourceName}.");
      }

      using (var stream = Assembly.GetEntryAssembly().GetManifestResourceStream(resourceName))
      {
        if (stream == null)
        {
          throw new Exception($"Error attempting to load resource '{resourceName}', stream cannot be null. Is the file marked as 'Embedded Resource'?");
        }

        var json = DeserializeFromStream<T>(stream);
        return json;
      }
    }

    private static T DeserializeFromStream<T>(Stream stream)
    {
      var serializer = new JsonSerializer();

      using (var sr = new StreamReader(stream))
      using (var jsonTextReader = new JsonTextReader(sr))
      {
        return serializer.Deserialize<T>(jsonTextReader);
      }
    }
  }
}
