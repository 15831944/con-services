﻿using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.MasterData.Proxies
{
  /// <summary>
  /// Proxy to validate and post a CoordinateSystem with Raptor.
  /// </summary>
  public class CustomerProxy : BaseProxy, ICustomerProxy
  {
    public CustomerProxy(IConfigurationStore configurationStore, ILoggerFactory logger, IDataCache cache) : base(configurationStore, logger, cache)
    {
    }

    /// <summary>
    /// list will include any customers (or dealers etc) associated with the User
    /// </summary>
    public async Task<CustomerDataResult> GetCustomersForMe(string userUid, IDictionary<string, string> customHeaders)
    {
      // e.g. https://api-stg.trimble.com/t/trimble.com/vss-alpha-customerservice/1.0/customers/me
      const string urlKey = "CUSTOMERSERVICE_API_URL";
      string url = configurationStore.GetValueString(urlKey);
      log.LogDebug($"{nameof(GetCustomersForMe)} userUid:{userUid} urlKey: {urlKey}  url: {url}");

      var response = await GetContainedMasterDataList<CustomerDataResult>(userUid, null, "CUSTOMER_CACHE_LIFE", urlKey, customHeaders);
      log.LogDebug($"{nameof(GetCustomersForMe)} response: {(response == null ? null : JsonConvert.SerializeObject(response).Truncate(_logMaxChar))}");
      return response;
    }

    /// <summary>
    /// Get list of customers for specified user
    /// </summary>
    public async Task<List<CustomerData>> GetCustomersForUser(string userUid, IDictionary< string, string> customHeaders)
    {
      var result = await GetCustomersForMe(userUid, customHeaders);
      return result.customer;
    }

    /// <summary>
    /// Clears an item from the cache
    /// </summary>
    /// <param name="userUid">The userUid of the item to remove from the cache</param>
    /// <param name="userId">The user ID</param>
    public void ClearCacheItem(string userUid, string userId=null)
    {
      ClearCacheItem<CustomerDataResult>(userUid, userId);
    }

    public async Task<CustomerData> GetCustomerForUser(string userUid, string customerUid,
      IDictionary<string, string> customHeaders = null)
    {
      return await GetItemWithRetry<CustomerDataResult, CustomerData>(GetCustomersForUser, c => string.Equals(c.uid, customerUid, StringComparison.OrdinalIgnoreCase), userUid, customHeaders);
    }

  }
}
