﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using VSS.GenericConfiguration;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Proxies.Models;

namespace VSS.Raptor.Service.Common.Proxies
{
  public class ProjectListProxy : BaseProxy, IProjectListProxy
  {
    private static TimeSpan projectListCacheLife = new TimeSpan(0, 15, 0);//TODO: how long to cache ?

    public ProjectListProxy(IConfigurationStore configurationStore, ILoggerFactory logger, IMemoryCache cache) : base(configurationStore, logger, cache)
    {
    }

    public async Task<List<ProjectData>> GetProjects(string customerUid, IDictionary<string, string> customHeaders = null)
    {
      return await GetList<ProjectData>(customerUid, projectListCacheLife, "PROJECT_API_URL", customHeaders);
    }
  }
}
