﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.Http;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.Common.Abstractions.Proxy.Interfaces;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.MasterData.Proxies
{
  /// <summary>
  /// Service Discovery enabled Proxy
  /// For now, we inherit from the BaseProxy to get code related to caching
  /// But we should create brand new fetch methods than don't accept URL values
  /// As these should be 'resolved' by the Service Resolution class
  /// </summary>
  public abstract class BaseServiceDiscoveryProxy : BaseProxy, IServiceDiscoveryProxy
  {
    protected readonly IWebRequest webRequest;
    private readonly IServiceResolution _serviceResolution;
    private const int _defaultLogMaxchar = 1000;
    protected readonly int logMaxChar;

    protected BaseServiceDiscoveryProxy(IWebRequest webRequest, IConfigurationStore configurationStore, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(configurationStore, logger, dataCache)
    {
      this.webRequest = webRequest;
      _serviceResolution = serviceResolution;
      logMaxChar = configurationStore.GetValueInt("LOG_MAX_CHAR", _defaultLogMaxchar);
    }

    #region Properties

    /// <summary>
    /// Is this Service Local to us?
    /// If the service is near this service in terms of network layout and doesn't need to go via TPaaS or other
    /// When the Service is Inside our Authentication Boundary then we will pass extra authentication information that would normally be added by the Authentication layer if it were an external request
    /// </summary>
    public abstract bool IsInsideAuthBoundary{ get; }

    /// <summary>
    /// The service this proxy is for, if we are accessing an internal (inside our authentication) service
    /// </summary>
    public abstract ApiService InternalServiceType { get; }

    /// <summary>
    /// If we are not accessing an internal service, this variable will be used for the service discovery
    /// </summary>
    public abstract string ExternalServiceName { get; }

    /// <summary>
    /// The version of the API this proxy is for
    /// </summary>
    public abstract ApiVersion Version { get; }

    /// <summary>
    /// The Type of API this service is for, public means it is exposed via TPaaS, so the URL includes /api/version/endpoint
    /// </summary>
    public abstract ApiType Type { get; }

    /// <summary>
    /// If we have a specific cache key for the expiry for cached items
    /// </summary>
    public abstract string CacheLifeKey { get; }

    #endregion

    #region Protected Methods

    /// <summary>
    /// Execute a Post to an endpoint, and cache the result
    /// NOTE: Must have a uid or userid for cache key
    /// </summary>
    protected Task<T> GetMasterDataItemServiceDiscovery<T>(string route, string uid, string userId, IHeaderDictionary customHeaders,
      IList<KeyValuePair<string, string>> queryParameters = null, string uniqueIdCacheKey = null)
      where T : class, IMasterDataModel
    {
      return WithMemoryCacheExecute(uid, userId, CacheLifeKey, customHeaders,
        () => RequestAndReturnData<T>(customHeaders, HttpMethod.Get, route, queryParameters),
        uniqueIdCacheKey);
    }

    /// <summary>
    /// Execute a Post to an endpoint, and cache the result
    /// NOTE: Must have a uid or userid for cache key
    /// </summary>
    protected Task<T> PostMasterDataItemServiceDiscovery<T>(string route, string uid, string userId, IHeaderDictionary customHeaders,
      IList<KeyValuePair<string, string>> queryParameters = null, Stream payload = null)
      where T : class, IMasterDataModel
    {
      return WithMemoryCacheExecute(uid, userId, CacheLifeKey, customHeaders,
        () => RequestAndReturnData<T>(customHeaders, HttpMethod.Post, route, queryParameters, payload));
    }

    protected Task<T> GetMasterDataItemServiceDiscoveryNoCache<T>(string route, IHeaderDictionary customHeaders,
      IList<KeyValuePair<string, string>> queryParameters = null)
      where T : class, IMasterDataModel
    {
        return RequestAndReturnData<T>(customHeaders, HttpMethod.Get, route, queryParameters);
    }

    protected Task<Stream> GetMasterDataStreamItemServiceDiscoveryNoCache(string route, IHeaderDictionary customHeaders,
     HttpMethod method, IList<KeyValuePair<string, string>> queryParameters = null, string payload = null)
    {
      return RequestAndReturnDataStream(customHeaders, method, route, queryParameters, payload);
    }

    protected Task<T> SendMasterDataItemServiceDiscoveryNoCache<T>(string route, IHeaderDictionary customHeaders,
      HttpMethod method, IList<KeyValuePair<string, string>> queryParameters = null, Stream payload = null, int retries = 0)
      where T : class
    {
      return RequestAndReturnData<T>(customHeaders, method, route, queryParameters, payload, retries);
    }

    /// <summary>
    /// Execute a Post/Put/Delete to an endpoint that returns only an HttpStatusCode.
    /// </summary>
    protected Task SendMasterDataItemServiceDiscoveryNoCache(string route, IHeaderDictionary customHeaders,
     HttpMethod method, IList<KeyValuePair<string, string>> queryParameters = null, Stream payload = null)
    {
      return Request(customHeaders, method, route, queryParameters, payload);
    }

    /// <summary>
    /// Execute a Post/Put/Delete to an endpoint, do not cache the result
    /// NOTE: Must have a uid or userid for cache key
    /// </summary>
    protected Task<T> MasterDataItemServiceDiscoveryNoCache<T>(string route, IHeaderDictionary customHeaders,
      HttpMethod method, IList<KeyValuePair<string, string>> queryParameters = null, Stream payload = null) where T : ContractExecutionResult
    {
      return RequestAndReturnResult<T>(customHeaders, method, route, queryParameters, payload);
    }

    /// <summary>
    /// Get the service name string based on the settings provided via the proxy.
    /// E.g convert the API Service enum to a string, or return the external service name
    /// TRex defines it's own urls as it can changed based on immutable / mutable etc.
    /// </summary>
    protected virtual string GetServiceName()
    {
      if (IsInsideAuthBoundary && InternalServiceType == ApiService.None)
        throw new ArgumentException($"{nameof(InternalServiceType)} has not been defined, it is required for Services Inside our Authentication Boundary");

      if (!IsInsideAuthBoundary && string.IsNullOrEmpty(ExternalServiceName))
        throw new ArgumentException($"{nameof(ExternalServiceName)} has not been defined, it is required for Remote Services");

      var serviceName = IsInsideAuthBoundary
        ? _serviceResolution.GetServiceName(InternalServiceType)
        : ExternalServiceName;
      return serviceName;
    }

    #endregion

    protected Task<string> GetUrl(string route, IHeaderDictionary customHeaders, IList<KeyValuePair<string, string>> queryParameters = null)
    {
      var serviceName = ResolveServiceNameFromHeaders(customHeaders);
      return IsInsideAuthBoundary
        ? _serviceResolution.ResolveLocalServiceEndpoint(serviceName, Type, Version, route, queryParameters)
        : _serviceResolution.ResolveRemoteServiceEndpoint(serviceName, Type, Version, route, queryParameters);
    }

    #region Private Methods

    /// <summary>
    /// In some cases we want to be able to override service discovery checks externally, custom headers allow us to override services
    /// This method checks for any of these overrides
    /// If no overrides are found, the service name configured by the proxy is returned.
    /// </summary>
    private string ResolveServiceNameFromHeaders(IHeaderDictionary customHeaders)
    {
      // Get the original Service Name
      var serviceName = GetServiceName();

      if (customHeaders == null)
        return serviceName;

      // Check to see if we have an override
      var overrideHeader = HeaderConstants.X_VSS_SERVICE_OVERRIDE_PREFIX + serviceName;
      var header = customHeaders.FirstOrDefault(k => string.Equals(k.Key, overrideHeader, StringComparison.OrdinalIgnoreCase));

      if (string.IsNullOrEmpty(header.Key) || string.IsNullOrEmpty(header.Value))
        return serviceName;

      log.LogInformation($"Service Discovery Override: Service '{serviceName}' replaced with '{header.Value}' from headers.");
      return header.Value;
    }

    private async Task<Stream> RequestAndReturnDataStream(IHeaderDictionary customHeaders,
     HttpMethod method, string route = null, IList<KeyValuePair<string, string>> queryParameters = null, string payload = null)
    {
      var url = await GetUrl(route, customHeaders, queryParameters);

      // If we are calling to our own services, keep the JWT assertion
      var strippedHeaders = customHeaders.StripHeaders(IsInsideAuthBoundary);

      var streamPayload = payload != null ? new MemoryStream(Encoding.UTF8.GetBytes(payload)) : null;
      var result = await (await webRequest.ExecuteRequestAsStreamContent(url, method, strippedHeaders, streamPayload)).ReadAsStreamAsync();
      BaseProxyHealthCheck.SetStatus(true, GetType());
      return result;
    }

    private async Task<TResult> RequestAndReturnData<TResult>(IHeaderDictionary customHeaders, HttpMethod method, string route = null, IList<KeyValuePair<string, string>> queryParameters = null, Stream payload = null, int retries = 0) where TResult : class
    {
      var url = await GetUrl(route, customHeaders, queryParameters);

      // If we are calling to our own services, keep the JWT assertion
      var strippedHeaders = customHeaders.StripHeaders(IsInsideAuthBoundary);

      var result = await webRequest.ExecuteRequest<TResult>(url, payload: payload, customHeaders: strippedHeaders, method: method, retries: retries);
      log.LogDebug($"{nameof(RequestAndReturnData)} Result: {JsonConvert.SerializeObject(result).Truncate(logMaxChar)}");

      return result;
    }

    private async Task<T> RequestAndReturnResult<T>(IHeaderDictionary customHeaders, HttpMethod method, string route = null, IList<KeyValuePair<string, string>> queryParameters = null, Stream payload = null) where T : ContractExecutionResult
    {
      var url = await GetUrl(route, customHeaders, queryParameters);

      // If we are calling to our own services, keep the JWT assertion
      var strippedHeaders = customHeaders.StripHeaders(IsInsideAuthBoundary);

      var result = await webRequest.ExecuteRequest<T>(url, payload: payload, customHeaders: strippedHeaders, method: method);
      log.LogDebug($"{nameof(RequestAndReturnResult)} Result: {JsonConvert.SerializeObject(result)}");

      return result;
    }

    private async Task Request(IHeaderDictionary customHeaders, HttpMethod method, string route = null, IList<KeyValuePair<string, string>> queryParameters = null, Stream payload = null)
    {
      var url = await GetUrl(route, customHeaders, queryParameters);

      // If we are calling to our own services, keep the JWT assertion
      var strippedHeaders = customHeaders.StripHeaders(IsInsideAuthBoundary);

      await webRequest.ExecuteRequest(url, payload: payload, customHeaders: strippedHeaders, method: method);
    }

    #endregion
  }
}
