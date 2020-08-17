﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.MasterData.Proxies
{
  /// <summary>
  /// Proxy for TPaaS services.
  /// </summary>
  public class TPaasProxy : BaseProxy, ITPaasProxy
  {
    public TPaasProxy(IWebRequest webRequest, IConfigurationStore configurationStore, ILoggerFactory logger)
      : base(webRequest, configurationStore, logger)
    { }

    /// <summary>
    /// Gets a new bearer token from TPaaS Oauth
    /// </summary>
    public async Task<TPaasOauthResult> GetApplicationBearerToken(string grantType, IHeaderDictionary customHeaders)
    {
      log.LogDebug($"GetApplicationBearerToken: grantType: {grantType} customHeaders: {customHeaders.LogHeaders(LogMaxChar)}");
      var payLoadToSend = $"grant_type={grantType}";
      var tPaasOauthResult = new TPaasOauthResult();
      try
      {
        tPaasOauthResult.tPaasOauthRawResult = await SendRequest<TPaasOauthRawResult>("TPAAS_OAUTH_URL", payLoadToSend, customHeaders, "/token", HttpMethod.Post, string.Empty);
      }
      catch (Exception e)
      {
        tPaasOauthResult.Code = 1902; // todo
        tPaasOauthResult.Message = e.Message;
      }

      var resultString = tPaasOauthResult == null ? "null" : JsonConvert.SerializeObject(tPaasOauthResult);
      var message = $"GetApplicationBearerToken: response: {resultString}";
      log.LogDebug(message);

      return tPaasOauthResult;
    }

    /// <summary>
    /// Revokes a bearer token from TPaaS Oauth
    /// </summary>
    public async Task<BaseDataResult> RevokeApplicationBearerToken(string token, IHeaderDictionary customHeaders)
    {
      log.LogDebug($"RevokeApplicationBearerToken: token: {token} customHeaders: {customHeaders.LogHeaders(LogMaxChar)}");
      var payLoadToSend = $"token={token}";
      var tPaasOauthResult = new BaseDataResult();
      try
      {
        await SendRequest<TPaasOauthRawResult>("TPAAS_OAUTH_URL", payLoadToSend, customHeaders, "/revoke", HttpMethod.Post, string.Empty);
      }
      catch (Exception e)
      {
        tPaasOauthResult.Code = 1902; // todo
        tPaasOauthResult.Message = e.Message;
      }

      var resultString = tPaasOauthResult == null ? "null" : JsonConvert.SerializeObject(tPaasOauthResult);
      var message = $"RevokeApplicationBearerToken: response: {resultString}";
      log.LogDebug(message);

      return tPaasOauthResult;
    }
  }
}
