﻿using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Helpers
{
  public interface IProjectSettingsRequestHelper
  {
    ProjectSettingsRequest CreateProjectSettingsRequest(string projectUid, string settings, ProjectSettingsType projectSettingsType);
  }
}
