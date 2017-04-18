﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;

namespace VSS.Raptor.Service.Common.Proxies
{
    public class GracefulWebRequest
    {
        private readonly ILogger log;

        public GracefulWebRequest(ILoggerFactory logger)
        {
            log = logger.CreateLogger<GracefulWebRequest>();
        }

        public async Task<T> ExecuteRequest<T>(string endpoint, string method,
            IDictionary<string, string> customHeaders = null, string payloadData = null)
        {
            var request = await PrepareWebRequest(endpoint, method, customHeaders, payloadData);

            return await Policy
                .Handle<Exception>()
                .Retry(3)
                .ExecuteAndCapture(async () =>
                {
                    string responseString = null;
                    using (var response = await request.GetResponseAsync())
                    {
                        responseString = GetStringFromResponseStream(response);
                    }
                    if (!string.IsNullOrEmpty(responseString))
                        return JsonConvert.DeserializeObject<T>(responseString);
                    return default(T);
                })
                .Result;
        }

        public async Task<Stream> ExecuteRequest(string endpoint, string method,
            IDictionary<string, string> customHeaders = null, string payloadData = null)
        {
            var request = await PrepareWebRequest(endpoint, method, customHeaders, payloadData);

            return await Policy
                .Handle<Exception>()
                .Retry(3)
                .ExecuteAndCapture(async () =>
                {
                    using (var response = await request.GetResponseAsync())
                    {
                         return GetStreamFromResponse(response);
                    }
                })
                .Result;
        }

        private async Task<WebRequest> PrepareWebRequest(string endpoint, string method, IDictionary<string, string> customHeaders, string payloadData)
        {
            log.LogDebug("Requesting data from {0}", endpoint);
            if (customHeaders != null)
            {
                log.LogDebug("Custom Headers:");
                foreach (var key in customHeaders.Keys)
                {
                    log.LogDebug("   {0}: {1}", key, customHeaders[key]);
                }
            }
            var request = WebRequest.Create(endpoint);
            request.Method = method;
            if (request is HttpWebRequest)
            {
                var httpRequest = request as HttpWebRequest;
                httpRequest.Accept = "application/json";
                //Add custom headers e.g. JWT, CustomerUid, UserUid
                if (customHeaders != null)
                {
                    foreach (var key in customHeaders.Keys)
                    {
                        httpRequest.Headers[key] = customHeaders[key];
                    }
                }
            }
            //Apply payload if any
            if (!String.IsNullOrEmpty(payloadData))
            {
                request.ContentType = "application/json";
                using (var writeStream = await request.GetRequestStreamAsync())
                {
                    UTF8Encoding encoding = new UTF8Encoding();
                    byte[] bytes = encoding.GetBytes(payloadData);
                    writeStream.Write(bytes, 0, bytes.Length);
                }
            }
            return request;
        }

        private string GetStringFromResponseStream(WebResponse response)
        {
            using (var readStream = response.GetResponseStream())
            {

                if (readStream != null)
                {
                    var reader = new StreamReader(readStream, Encoding.UTF8);
                    var responseString = reader.ReadToEnd();
                    log.LogDebug("Response: {0}", responseString);
                    return responseString;
                }
                return string.Empty;
            }
        }

        private Stream GetStreamFromResponse(WebResponse response)
        {
            using (var readStream = response.GetResponseStream())
            {
                var streamFromResponse = new MemoryStream();

                if (readStream != null)
                {
                    readStream.CopyTo(streamFromResponse);
                    return streamFromResponse;
                }
                return null;
            }
        }


        public async Task<T> ExecuteRequest<T>(string endpoint, Stream payload,
            IDictionary<string, string> customHeaders = null)
        {
            log.LogDebug("Requesting project data from {0}", endpoint);
            if (customHeaders != null)
            {
                log.LogDebug("Custom Headers:");
                foreach (var key in customHeaders.Keys)
                {
                    log.LogDebug("   {0}: {1}", key, customHeaders[key]);
                }
            }

            var request = WebRequest.Create(endpoint);
            request.Method = "POST";
            if (request is HttpWebRequest)
            {
                var httpRequest = request as HttpWebRequest;
                httpRequest.Accept = "*/*";
                //Add custom headers e.g. JWT, CustomerUid, UserUid
                if (customHeaders != null)
                {
                    foreach (var key in customHeaders.Keys)
                    {
                        httpRequest.Headers[key] = customHeaders[key];
                    }
                }
            }
            using (var writeStream = await request.GetRequestStreamAsync())
            {
              await payload.CopyToAsync(writeStream);
            }
            return await Policy
                .Handle<Exception>()
                .Retry(3)
                .ExecuteAndCapture(async () =>
                {
                    string responseString = null;
                    using (var response = await request.GetResponseAsync())
                    {
                        responseString = GetStringFromResponseStream(response);
                    }
                    if (!string.IsNullOrEmpty(responseString))
                        return JsonConvert.DeserializeObject<T>(responseString);
                    return default(T);
                }).Result;
        }

    }
}
