﻿using System.Threading.Tasks;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Helpers
{
  public interface ITagFileAuthHelper
  {
    Task<GetProjectAndAssetUidsResult> GetProjectUid(string radioSerial, string eCSerial,
      string tccOrgUid, double machineLatitude, double machineLongitude);
  }
}
