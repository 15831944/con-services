﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VSS.Productivity3D.Productivity3D.Models.Coord.ResultHandling;

namespace VSS.Productivity3D.Productivity3D.Abstractions.Interfaces
{
  public interface IProductivity3dV1ProxyCoord : IProductivity3dV1Proxy
  {
    Task<CoordinateSystemSettingsResult> CoordinateSystemValidate(byte[] coordinateSystemFileContent,
      string coordinateSystemFilename, IHeaderDictionary customHeaders = null);

    #region SoonToBeObsoleteCCSSSCON-351
    Task<CoordinateSystemSettingsResult> CoordinateSystemPost(long legacyProjectId, byte[] coordinateSystemFileContent,
      string coordinateSystemFilename, IHeaderDictionary customHeaders = null);
    #endregion SoonToBeObsoleteCCSSSCON-351

    Task<CoordinateSystemSettingsResult> CoordinateSystemPost(Guid projectUid, byte[] coordinateSystemFileContent,
      string coordinateSystemFilename, IHeaderDictionary customHeaders = null);

  }
}
