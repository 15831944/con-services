﻿using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Project.Abstractions.Utilities;

namespace VSS.MasterData.Project.WebAPI.Common.Models
{
  /// <summary>
  /// Additional functionality for FileDescriptor.
  /// </summary>
  public static class FileDescriptorExtensions
  {
    public static string BaseFileName(this FileDescriptor fileDescr)
    {
      return ImportedFileUtils.RemoveSurveyedUtcFromName(fileDescr.FileName);
    }
  }
}
