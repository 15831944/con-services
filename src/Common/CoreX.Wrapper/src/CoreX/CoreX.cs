﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using CoreX.Extensions;
using CoreX.Models;
using CoreX.Types;
using CoreX.Wrapper.Extensions;
using CoreX.Wrapper.Types;
using Microsoft.Extensions.Logging;
using Trimble.CsdManagementWrapper;
using Trimble.GeodeticXWrapper;
using VSS.Common.Abstractions.Configuration;

namespace CoreX.Wrapper
{
  public class CoreX : IDisposable
  {
    public string GeodeticDatabasePath;

    private static readonly object _lock = new object();
    private readonly ILogger _log;

    public CoreX(ILoggerFactory loggerFactory, IConfigurationStore configStore)
    {
      _log = loggerFactory.CreateLogger<CoreX>();

      // CoreX static classes aren't thread safe singletons.
      lock (_lock)
      {
        GeodeticDatabasePath = configStore.GetValueString("TGL_GEODATA_PATH", "Geodata");
        _log.LogInformation($"CoreX {nameof(SetupTGL)}: TGL_GEODATA_PATH='{GeodeticDatabasePath}'");

        SetupTGL();
      }
    }

    /// <summary>
    /// Setup the underlying CoreXDotNet singleton management classes.
    /// </summary>
    private void SetupTGL()
    {
      var xmlFilePath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "CoordSystemDatabase.xml");

      if (!File.Exists(xmlFilePath))
      {
        throw new Exception($"Failed to find TGL CSD database file '{xmlFilePath}'.");
      }

      using var reader = new StreamReader(xmlFilePath);
      var xmlData = reader.ReadToEnd();
      var resultCode = CsdManagement.csmLoadCoordinateSystemDatabase(xmlData);

      if (resultCode != (int)csmErrorCode.cecSuccess)
      {
        throw new Exception($"Error '{resultCode}' attempting to load coordinate system database '{xmlFilePath}'");
      }

      _log.LogInformation($"CoreX {nameof(SetupTGL)}: GeodeticDatabasePath='{GeodeticDatabasePath}'");

      if (string.IsNullOrEmpty(GeodeticDatabasePath))
      {
        throw new Exception("Environment variable TGL_GEODATA_PATH must be set to the Geodetic data folder.");
      }
      if (!Directory.Exists(GeodeticDatabasePath))
      {
        _log.LogInformation($"Failed to find directory '{GeodeticDatabasePath}' defined by environment variable TGL_GEODATA_PATH.");
      }
      else
      {
        CoreXGeodataLogger.DumpGeodataFiles(_log, GeodeticDatabasePath);
      }

      CsdManagement.csmSetGeodataPath(GeodeticDatabasePath);
      GeodeticX.geoSetGeodataPath(GeodeticDatabasePath);
    }

    /// <summary>
    /// Returns the CSIB from a DC file string.
    /// </summary>
    public string GetCSIBFromDCFileContent(string fileContent)
    {
      // We may receive coordinate system file content that's been uploaded (encoded) from a web api, must decode first.
      fileContent = fileContent.DecodeFromBase64();

      using var csmCsibBlobContainer = new CSMCsibBlobContainer();

      lock (_lock)
      {
        // Slow, takes 2.5 seconds, need to speed up somehow?
        var result = CsdManagement.csmGetCSIBFromDCFileData(
          fileContent,
          false,
          Utils.FileListCallBack,
          Utils.EmbeddedDataCallback,
          csmCsibBlobContainer);

        if (result != (int)csmErrorCode.cecSuccess)
        {
          switch ($"{result}")
          {
            case "cecGRID_FILE_OPEN_ERROR":
              {
                var geoidModelName = CalibrationFileHelper.GetGeoidModelName(Encoding.UTF8.GetBytes(fileContent));
                throw new InvalidOperationException($"{nameof(GetCSIBFromDCFileContent)}: Geodata file not found for geoid model '{geoidModelName}'");
              }
            default:
              {
                throw new InvalidOperationException($"{nameof(GetCSIBFromDCFileContent)}: Get CSIB from file content failed, error {result}");
              }
          }
        }
      }

      return GetCSIB(csmCsibBlobContainer);
    }

    /// <summary>
    /// Returns the CSIB from a DC file given it's filepath.
    /// </summary>
    public string GetCSIBFromDCFile(string filePath)
    {
      using var streamReader = new StreamReader(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read), Encoding.UTF8);
      var dcStr = streamReader.ReadToEnd();

      return GetCSIBFromDCFileContent(dcStr);
    }

    public static bool ValidateCsibString(string csib) => ValidateCsib(csib);

    /// <summary>
    /// Transform an NEE to LLH with variable from and to coordinate type inputs.
    /// </summary>
    /// <returns>Returns LLH object in radians.</returns>
    public LLH TransformNEEToLLH(string csib, NEE nee, CoordinateTypes fromType, CoordinateTypes toType)
    {
      using var transformer = GeodeticXTransformer(csib);

      transformer.Transform(
        (geoCoordinateTypes)fromType,
        nee.North,
        nee.East,
        nee.Elevation,
        (geoCoordinateTypes)toType,
        out var toY, out var toX, out var toZ);

      // The toX and toY parameters mirror the order of the input parameters fromX and fromY; they are not grid coordinate positions.
      return new LLH
      {
        Latitude = toY,
        Longitude = toX,
        Height = toZ
      };
    }

    /// <summary>
    /// Transform an array of NEE points to an array of LLH coordinates with variable from and to coordinate type inputs.
    /// </summary>
    /// <returns>Returns an array of LLH coordinates in radians.</returns>
    public LLH[] TransformNEEToLLH(string csib, NEE[] coordinates, CoordinateTypes fromType, CoordinateTypes toType)
    {
      var llhCoordinates = new LLH[coordinates.Length];

      using var transformer = GeodeticXTransformer(csib);

      for (var i = 0; i < coordinates.Length; i++)
      {
        var nee = coordinates[i];

        transformer.Transform(
          (geoCoordinateTypes)fromType,
          nee.North,
          nee.East,
          nee.Elevation,
          (geoCoordinateTypes)toType,
          out var toY, out var toX, out var toZ);

        // The toX and toY parameters mirror the order of the input parameters fromX and fromY; they are not grid coordinate positions.
        llhCoordinates[i] = new LLH
        {
          Latitude = toY,
          Longitude = toX,
          Height = toZ
        };
      }

      return llhCoordinates;
    }

    /// <summary>
    /// Transform an LLH to NEE with variable from and to coordinate type inputs.
    /// </summary>
    /// <returns>A NEE point of the LLH provided coordinates in radians.</returns>
    public NEE TransformLLHToNEE(string csib, LLH coordinates, CoordinateTypes fromType, CoordinateTypes toType)
    {
      using var transformer = GeodeticXTransformer(csib);

      transformer.Transform(
        (geoCoordinateTypes)fromType,
        coordinates.Latitude,
        coordinates.Longitude,
        coordinates.Height,
        (geoCoordinateTypes)toType,
        out var toY, out var toX, out var toZ);

      return new NEE
      {
        North = toY,
        East = toX,
        Elevation = toZ
      };
    }

    /// <summary>
    /// Transform an array of LLH coordinates to an array of NEE points with variable from and to coordinate type inputs.
    /// </summary>
    /// <returns>Returns an array of NEE points in radians.</returns>
    public NEE[] TransformLLHToNEE(string csib, LLH[] coordinates, CoordinateTypes fromType, CoordinateTypes toType)
    {
      var neeCoordinates = new NEE[coordinates.Length];

      using var transformer = GeodeticXTransformer(csib);

      for (var i = 0; i < coordinates.Length; i++)
      {
        var llh = coordinates[i];

        transformer.Transform(
          (geoCoordinateTypes)fromType,
          llh.Latitude,
          llh.Longitude,
          llh.Height,
          (geoCoordinateTypes)toType,
          out var toY, out var toX, out var toZ);

        neeCoordinates[i] = new NEE
        {
          North = toY,
          East = toX,
          Elevation = toZ
        };
      }

      return neeCoordinates;
    }

    private GEOCsibBlobContainer CreateCsibBlobContainer(string csibStr)
    {
      if (string.IsNullOrEmpty(csibStr))
      {
        throw new ArgumentNullException(csibStr, $"{nameof(CreateCsibBlobContainer)}: csibStr cannot be null");
      }

      var bytes = Array.ConvertAll(Convert.FromBase64String(csibStr), b => unchecked((sbyte)b));
      var geoCsibBlobContainer = new GEOCsibBlobContainer(bytes);

      if (geoCsibBlobContainer.Length < 1)
      {
        throw new Exception($"Failed to set CSIB from base64 string, '{csibStr}'");
      }

      return geoCsibBlobContainer;
    }

    private IGeodeticXTransformer GeodeticXTransformer(string csib)
    {
      using var geoCsibBlobContainer = CreateCsibBlobContainer(csib);
      using var transformer = new PointerPointer_IGeodeticXTransformer();

      var result = GeodeticX.geoCreateTransformer(geoCsibBlobContainer, transformer);

      if (result != geoErrorCode.gecSuccess)
      {
        throw new Exception($"Failed to create GeodeticX transformer, error '{result}'");
      }

      return transformer.get();
    }

    private static bool ValidateCsib(string csib)
    {
      var sb = new StringBuilder();
      var bytes = Encoding.ASCII.GetBytes(csib);

      for (var i = 0; i < bytes.Length; i++)
      {
        sb.Append(bytes[i] + " ");
      }

      var blocks = sb.ToString().TrimEnd().Split(' ');
      var data = new sbyte[blocks.Length];

      var index = 0;
      foreach (var b in blocks)
      {
        data[index++] = (sbyte)Convert.ToByte(b);
      }

      var csmCsibData = new CSMCsibBlobContainer(data);
      var csFromCSIB = new CSMCoordinateSystemContainer();
      var csmErrorCode = CsdManagement.csmImportCoordSysFromCsib(csmCsibData, csFromCSIB);

      return csmErrorCode == csmErrorCode.cecSuccess;
    }

    private string GetCSIB(CSMCsibBlobContainer csibBlobContainer) =>
      Convert.ToBase64String(
        Array.ConvertAll(Utils.IntPtrToSByte(csibBlobContainer.pCSIBData, (int)csibBlobContainer.CSIBDataLength), sb => unchecked((byte)sb)));

    public Datum[] GetDatums()
    {
      using var returnListStruct = new CSMStringListContainer();

      var resultCode = CsdManagement.csmGetListOfDatums(returnListStruct);

      if (resultCode == csmErrorCode.cecSuccess)
      {
        var datums = returnListStruct.stringList.Split(new[] { CsdManagement.STRING_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => new Datum(
              datumSystemId: int.Parse(s.Split(CsdManagement.ITEM_SEPERATOR)[0]),
              datumType: int.Parse(s.Split(CsdManagement.ITEM_SEPERATOR)[1]),
              datumName: s.Split(CsdManagement.ITEM_SEPERATOR)[3]));

        if (datums == null)
        {
          throw new Exception($"Error attempting to retrieve list of datums, null result");
        }

        if (!datums.Any())
        {
          throw new Exception($"No datums found");
        }

        return datums.ToArray();


        //foreach (var datum in datums)
        //{
        //  using var resultContainer = new CSMCoordinateSystemContainer();
        //  resultCode = CsdManagement.csmGetDatumFromCSDSelection(
        //    datum.DatumName, (csmDatumTypes)datum.DatumType, false, null, null, resultContainer);

        //  //Assert.AreEqual(csmErrorCode.cecSuccess, resultCode);

        //  //ValidateRecord(resultContainer.GetSelectedRecord());

        //  var a = resultContainer.GetSelectedRecord();
        //}
      }

      return null;
    }

    public ICoordinateSystem GetDatumBySystemId(int datumSystemId)
    {
      var datumContainer = new CSMCoordinateSystemContainer();

      var resultCode = CsdManagement.csmGetDatumFromCSDSelectionById(
        (uint)datumSystemId, false, null, null, datumContainer);

      if (resultCode != csmErrorCode.cecSuccess)
      {
        throw new Exception($"Error attempting to retrieve datum {datumSystemId} by id.");
      }

      return datumContainer.GetSelectedRecord();
    }

    public string GetCoordinateSystemFromCSDSelection(string zoneGroupNameString, string zoneNameString)
    {
      lock (_lock)
      {
        using var retStructZoneGroups = new CSMStringListContainer();
        var resultCode = CsdManagement.csmGetListOfZoneGroups(retStructZoneGroups);

        if (resultCode != (int)csmErrorCode.cecSuccess)
        {
          throw new Exception($"Error '{resultCode}' attempting to retrieve list of zone groups");
        }

        var zoneGroups = retStructZoneGroups
          .stringList
          .Split(new[] { CsdManagement.STRING_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries)
          .Select(s => s.Split(CsdManagement.ITEM_SEPERATOR)[1]);

        if (!zoneGroups.Any())
        {
          throw new Exception("The count of zone groups should be greater than 0");
        }

        var zoneGroupName = zoneGroupNameString.Substring(zoneGroupNameString.IndexOf(",") + 1);
        var retStructListOfZones = new CSMStringListContainer();

        resultCode = CsdManagement.csmGetListOfZones(zoneGroupName, retStructListOfZones);

        if (resultCode != (int)csmErrorCode.cecSuccess)
        {
          throw new Exception($"Error '{resultCode}' attempting to retrieve list of zones for group '{zoneGroupName}'");
        }

        var zones = retStructListOfZones
          .stringList
          .Split(new[] { CsdManagement.STRING_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);

        if (!zones.Any())
        {
          throw new Exception($"The count of zones in {zoneGroupName} should be greater than 0");
        }

        if (Array.IndexOf(zones, zoneNameString) < 0)
        {
          throw new Exception($"Could not find '{zoneNameString}' in the list of zones for group '{zoneGroupName}'");
        }

        var zoneName = zoneNameString.Substring(zoneNameString.IndexOf(",") + 1);
        var items = zoneNameString.Split(CsdManagement.ITEM_SEPERATOR);

        var zoneId = uint.Parse(items[0]);
        //var zone = items[1];

        using var retCsStruct = new CSMCoordinateSystemContainer();
        var result = CsdManagement.csmGetCoordinateSystemFromCSDSelectionDefaults(zoneGroupName, zoneName, false, Utils.FileListCallBack, Utils.EmbeddedDataCallback, retCsStruct);

        if (resultCode != (int)csmErrorCode.cecSuccess)
        {
          throw new Exception($"Error '{resultCode}' attempting to retrieve coordinate system from CSD selection; zone group: '{zoneGroupName}', zone: {zoneName}");
        }

        var coordinateSystem = retCsStruct.GetSelectedRecord();
        coordinateSystem.Validate();

        var zoneID = unchecked((uint)coordinateSystem.ZoneSystemId());
        var datumID = unchecked((uint)coordinateSystem.DatumSystemId());
        var geoidID = unchecked((uint)coordinateSystem.GeoidSystemId());

        if (coordinateSystem.DatumSystemId() > 0)
        {
          return GetCSIBFrom(coordinateSystem);
        }
        else
        {
          var datumResult = GetDatumBySystemId(1034);

          static void SetDatumProperty(Func<bool> funct)
          {
            if (!funct())
            {
              throw new Exception($"Failed to set datum property Func.Method: {funct.Method}, Func.Target: {funct.Target}");
            }
          }

          SetDatumProperty(() => coordinateSystem.SetDatumHeightShiftGridFileName(datumResult.DatumHeightShiftGridFileName()));
          SetDatumProperty(() => coordinateSystem.SetDatumName(datumResult.DatumName()));
          SetDatumProperty(() => coordinateSystem.SetDatumRotationX(datumResult.DatumRotationX()));
          SetDatumProperty(() => coordinateSystem.SetDatumRotationY(datumResult.DatumRotationY()));
          SetDatumProperty(() => coordinateSystem.SetDatumRotationZ(datumResult.DatumRotationZ()));
          SetDatumProperty(() => coordinateSystem.SetDatumScale(datumResult.DatumScale()));
          SetDatumProperty(() => coordinateSystem.SetDatumSystemId(datumResult.DatumSystemId()));
          SetDatumProperty(() => coordinateSystem.SetDatumTransfoEPSG(datumResult.DatumTransfoEPSG()));
          SetDatumProperty(() => coordinateSystem.SetDatumTranslationX(datumResult.DatumTranslationX()));
          SetDatumProperty(() => coordinateSystem.SetDatumTranslationY(datumResult.DatumTranslationY()));
          SetDatumProperty(() => coordinateSystem.SetDatumTranslationZ(datumResult.DatumTranslationZ()));
          SetDatumProperty(() => coordinateSystem.SetDatumType(datumResult.DatumType()));

          if (coordinateSystem.DatumSystemId() > 0)
          {
            return GetCSIBFrom(coordinateSystem);
          }
        }

        throw new Exception($"Error attempting to retrieve coordinate system from CSD selection; zone group: '{zoneGroupName}', zone: {zoneName}, no datum found");
      }
    }

    private string GetCSIBFrom(ICoordinateSystem coordinateSystem)
    {
      using var retStructFromICoordinateSystem = new CSMCsibBlobContainer();

      var csibResultFromCS = CsdManagement.csmGetCSIBFromCoordinateSystem(coordinateSystem, false, Utils.FileListCallBack, Utils.EmbeddedDataCallback, retStructFromICoordinateSystem);

      var csib = GetCSIB(retStructFromICoordinateSystem);

      //// Create CSIB by zoneId, datumId, geoidId
      //using var retStructFromIds = new CSMCsibBlobContainer();
      //csibResult = CsdManagement.csmGetCSIBFromCSDSelectionById(zoneID, datumID, geoidID, false, Utils.FileListCallBack, Utils.EmbeddedDataCallback, retStructFromIds);

      //// Create CSIB by just zoneId
      //using var retStructFromZoneId = new CSMCsibBlobContainer();
      //csibResult = CsdManagement.csmGetCSIBFromCSDSelectionDefaultById(zoneId, false, Utils.FileListCallBack, Utils.EmbeddedDataCallback, retStructFromZoneId);

      return csib;
    }


    private bool _disposed = false;

    public void Dispose() => Dispose(true);

    protected virtual void Dispose(bool disposing)
    {
      if (_disposed)
      {
        return;
      }

      if (disposing)
      { }

      _disposed = true;
    }
  }
}
