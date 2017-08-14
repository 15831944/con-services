﻿using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.MasterData.Models.Models;

namespace VSS.MasterData.Proxies.Interfaces
{
  public interface IFilterServiceProxy : ICacheProxy
  {
    Task<FilterDescriptor> GetFilter(string projectUid, string filterUid,
      IDictionary<string, string> customHeaders = null);
  }
}