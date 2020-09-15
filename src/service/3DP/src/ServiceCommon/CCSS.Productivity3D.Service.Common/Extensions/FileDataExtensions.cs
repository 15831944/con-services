﻿using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace CCSS.Productivity3D.Service.Common.Extensions
{
  /// <summary>
  /// Extension methods for the <see cref="FileData"/> type.
  /// </summary>
  public static class FileDataExtensions
  {
    /// <summary>
    /// Validates the <see cref="FileData.ImportedFileType"/> is supported by the profiler.
    /// </summary>
    /// <param name="fileData">The receiver object to validate <see cref="FileData.ImportedFileType"/> against.</param>
    /// <returns>Boolean value reflecting whether the input <see cref="ImportedFileType"/> is supported or not.</returns>
    public static bool IsProfileSupportedFileType(this FileData fileData)
    {
      switch (fileData.ImportedFileType)
      {
        case ImportedFileType.DesignSurface:
        case ImportedFileType.SurveyedSurface:
        case ImportedFileType.ReferenceSurface:
          return true;
        default:
          return false;
      }
    }
  }
}
