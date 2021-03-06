﻿using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Gateway.Common.Abstractions;

namespace VSS.TRex.Gateway.Common.Proxy
{
  /// <summary>
  /// Proxy for TRex tag files and connected service.
  /// </summary>
  public class TRexTagFileV2Proxy : BaseTRexServiceDiscoveryProxy, ITRexTagFileProxy
  {
    public TRexTagFileV2Proxy(IWebRequest webRequest, IConfigurationStore configurationStore,
      ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(webRequest, configurationStore, logger, dataCache, serviceResolution)
    {
    }

    public override bool IsInsideAuthBoundary => true;

    public override ApiService InternalServiceType => ApiService.None;

    public override string ExternalServiceName => null;

    public override ApiVersion Version => ApiVersion.V2;

    public override ApiType Type => ApiType.Public;

    public override string CacheLifeKey => "TREX_TAGFILE_CACHE_LIFE"; // not used

    public async Task<ContractExecutionResult> SendTagFile(CompactionTagFileRequest compactionTagFileRequest,
      IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(SendTagFile)}: Filename: {compactionTagFileRequest.FileName}");
      Gateway = GatewayType.Mutable;
      return await SendTagFileRequest(compactionTagFileRequest, customHeaders, "/tagfiles");
    }

    public virtual async Task<ContractExecutionResult> SendTagFileRequest(CompactionTagFileRequest compactionTagFileRequest,
      IHeaderDictionary customHeaders,string route)
    {
      var jsonData = JsonConvert.SerializeObject(compactionTagFileRequest);
      using (var payload = new MemoryStream(Encoding.UTF8.GetBytes(jsonData)))
        return await MasterDataItemServiceDiscoveryNoCache<ContractExecutionResult>(route, customHeaders, HttpMethod.Post, payload: payload);
    }
  }
}
