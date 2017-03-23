﻿using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using VSS.GenericConfiguration;
using VSS.Raptor.Service.Common.Proxies.Models;


namespace VSS.Raptor.Service.Common.Proxies
{
    /// <summary>
    /// Base class for proxies getting master data from services.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BaseProxy<T> where T : IData
    {
        private readonly ILogger log;
        private readonly ILoggerFactory logger;
        private readonly IConfigurationStore configurationStore;
        private readonly IMemoryCache cache;

        protected BaseProxy(IConfigurationStore configurationStore, ILoggerFactory logger, IMemoryCache cache)
        {
            log = logger.CreateLogger<BaseProxy<T>>();
            this.logger = logger;
            this.configurationStore = configurationStore;
            this.cache = cache;
        }

        /// <summary>
        /// Executes a request against masterdata service
        /// </summary>
        /// <param name="urlKey">The configuration store key for the URL</param>
        /// <param name="customHeaders">The custom headers for the request (authorization, userUid and customerUid)</param>
        /// <param name="payload">The payload of the request</param>
        /// <returns>The item</returns>
        protected T SendRequest(string urlKey, string payload, IDictionary<string, string> customHeaders)
        {
            var url = ExtractUrl(urlKey);
            T result = default(T);
            try
            {
                GracefulWebRequest request = new GracefulWebRequest(logger);
                result = request.ExecuteRequest<T>(url, "POST", customHeaders, payload).Result;
                log.LogDebug("Result of send to master data request: {0}", result);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                string stacktrace = ex.StackTrace;
                //Check for 400 and 500 errors which come through as an inner exception
                if (ex.InnerException != null)
                {
                    message = ex.InnerException.Message;
                    stacktrace = ex.InnerException.StackTrace;
                }
                log.LogWarning("Error sending data from master data: ", message);
                log.LogWarning("Stacktrace: ", stacktrace);
            }
            return result;

        }

        /// <summary>
        /// Gets a list of the specified items from the specified service.
        /// </summary>
        /// <param name="urlKey">The configuration store key for the URL</param>
        /// <param name="customHeaders">The custom headers for the request (authorization, userUid and customerUid)</param>
        /// <returns>List of items</returns>
        private List<T> GetList(string urlKey, IDictionary<string, string> customHeaders)
        {
            var url = ExtractUrl(urlKey);

            List<T> result = null;
            try
            {
                GracefulWebRequest request = new GracefulWebRequest(logger);
                result = request.ExecuteRequest<List<T>>(url, "GET", customHeaders).Result;
                log.LogDebug("Result of get master data list request: {0} items", result.Count);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                string stacktrace = ex.StackTrace;
                //Check for 400 and 500 errors which come through as an inner exception
                if (ex.InnerException != null)
                {
                    message = ex.InnerException.Message;
                    stacktrace = ex.InnerException.StackTrace;
                }
                log.LogWarning("Error getting data from master data: ", message);
                log.LogWarning("Stacktrace: ", stacktrace);
            }
            return result;
        }

        private string ExtractUrl(string urlKey)
        {
            string url = configurationStore.GetValueString(urlKey);
            log.LogInformation(string.Format("{0}: {1}", urlKey, url));

            if (url == null)
            {
                var errorString = string.Format("Your application is missing an environment variable {0}", urlKey);
                log.LogError(errorString);
                throw new InvalidOperationException(errorString);
            }
            return url;
        }

        /// <summary>
        /// Gets a master data item. If the item is not in the cache then requests items from the relevant service and adds them to the cache.
        /// </summary>
        /// <param name="uid">The UID of the item to retrieve</param>
        /// <param name="cacheLife">How long to cache items</param>
        /// <param name="urlKey">The configuration store key for the URL of the master data service</param>
        /// <param name="customHeaders">Custom headers for the request (authorization, userUid and customerUid)</param>
        /// <returns>Master data item</returns>
        protected T GetItem(string uid, TimeSpan cacheLife, string urlKey, IDictionary<string, string> customHeaders)
        {
            T cacheData;
            if (!cache.TryGetValue(uid, out cacheData))
            {
                var opts = new MemoryCacheEntryOptions()
                {
                    SlidingExpiration = cacheLife
                };

                var list = GetList(urlKey, customHeaders);
                foreach (var item in list)
                {
                    var data = item as IData;
                    cache.Set(data.CacheKey, item, opts);
                }
                cache.TryGetValue(uid, out cacheData);
            }
            return cacheData;
        }

        /// <summary>
        /// Gets a list of master data items for a customer. 
        /// If the list is not in the cache then requests items from the relevant service and adds the list to the cache.
        /// </summary>
        /// <param name="customerUid">The customer UID for the list to retrieve</param>
        /// <param name="cacheLife">How long to cache the list</param>
        /// <param name="urlKey">The configuration store key for the URL of the master data service</param>
        /// <param name="customHeaders">Custom headers for the request (authorization, userUid and customerUid)</param>
        /// <returns>Master data item</returns>
        protected List<T> GetList(string customerUid, TimeSpan cacheLife, string urlKey,
            IDictionary<string, string> customHeaders)
        {
            List<T> cacheData;
            if (!cache.TryGetValue(customerUid, out cacheData))
            {
                var opts = new MemoryCacheEntryOptions()
                {
                    SlidingExpiration = cacheLife
                };
                cacheData = GetList(urlKey, customHeaders);
                cache.Set(customerUid, cacheData, opts);
            }
            return cacheData;
        }
    }
}
