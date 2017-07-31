﻿using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;

namespace VSS.MasterData.Proxies
{

  public class GracefulWebRequest
  {
    private readonly ILogger log;

    public GracefulWebRequest(ILoggerFactory logger)
    {

      log = logger.CreateLogger<GracefulWebRequest>();
    }


    private class RequestExecutor
    {
      private readonly string endpoint;
      private readonly string method;
      private readonly IDictionary<string, string> customHeaders;
      private readonly string payloadData;
      private readonly ILogger log;
      private const int BUFFER_MAX_SIZE = 1024;

      private async Task<string> GetStringFromResponseStream(WebResponse response)
      {
        var readStream = response.GetResponseStream();
        byte[] buffer = ArrayPool<byte>.Shared.Rent(BUFFER_MAX_SIZE);
        string responseString = String.Empty;

        try
        {
          Array.Clear(buffer, 0, buffer.Length);
          var read = await readStream.ReadAsync(buffer, 0, buffer.Length);
          responseString = Encoding.ASCII.GetString(buffer);
          responseString = responseString.Trim(Convert.ToChar(0));
          while (read > 0)
          {
            Array.Clear(buffer, 0, buffer.Length);
            read = await readStream.ReadAsync(buffer, 0, buffer.Length);
            responseString += Encoding.ASCII.GetString(buffer);
            responseString = responseString.Trim(Convert.ToChar(0));
          }
        }
        catch (Exception ex)
        {
          log.LogDebug($"ExecuteRequest() T: InOddException {ex.Message}");
          if (ex.InnerException != null)
            log.LogDebug($"ExecuteRequestInnerException() T: errorCode: {ex.InnerException.Message}");
          throw;
        }
        finally
        {
          readStream?.Dispose();
          ArrayPool<byte>.Shared.Return(buffer);
          responseString = responseString.Trim(Convert.ToChar(0));
        }
        return responseString;
      }

      private async Task<Stream> GetMemoryStreamFromResponseStream(WebResponse response)
      {
        var readStream = response.GetResponseStream();
        byte[] buffer = ArrayPool<byte>.Shared.Rent(BUFFER_MAX_SIZE);
        var resultStream = new MemoryStream();

        try
        {
          Array.Clear(buffer, 0, buffer.Length);
          var read = await readStream.ReadAsync(buffer, 0, buffer.Length);
          resultStream.Write(buffer, 0, read);
          while (read > 0)
          {
            Array.Clear(buffer, 0, buffer.Length);
            read = await readStream.ReadAsync(buffer, 0, buffer.Length);
            resultStream.Write(buffer, 0, read);
          }
        }
        catch (Exception ex)
        {
          log.LogDebug($"ExecuteRequest() T: InOddException {ex.Message}");
          if (ex.InnerException != null)
            log.LogDebug($"ExecuteRequestInnerException() T: errorCode: {ex.InnerException.Message}");
          throw;
        }
        finally
        {
          readStream?.Dispose();
          ArrayPool<byte>.Shared.Return(buffer);
        }
        return resultStream;
      }

      private async Task<WebRequest> PrepareWebRequest(string endpoint, string method,
        IDictionary<string, string> customHeaders, string payloadData = null, Stream requestStream = null)
      {
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
              if (key == "Content-Type")
              {
                httpRequest.ContentType = customHeaders[key];
              }
              else
              {
                httpRequest.Headers[key] = customHeaders[key];
              }
            }
          }
        }
        if (requestStream != null)
        {
          using (var writeStream = await request.GetRequestStreamAsync())
          {
            await requestStream.CopyToAsync(writeStream);
          }
        }
        else
          //Apply payload if any
        if (!String.IsNullOrEmpty(payloadData))
        {
          request.ContentType = "application/json";
          using (var writeStream = await request.GetRequestStreamAsync())
          {
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] bytes = encoding.GetBytes(payloadData);
            await writeStream.WriteAsync(bytes, 0, bytes.Length);
          }
        }
        return request;
      }

      public RequestExecutor(string endpoint, string method, IDictionary<string, string> customHeaders,
        string payloadData, ILogger log)
      {
        this.endpoint = endpoint;
        this.method = method;
        this.customHeaders = customHeaders;
        this.payloadData = payloadData;
        this.log = log;
      }


      public async Task<Stream> ExecuteActualStreamRequest()
      {
        var request = PrepareWebRequest(endpoint, method, customHeaders, payloadData).Result;

        WebResponse response = null;
        try
        {
          response = await request.GetResponseAsync();
          if (response != null)
          {
            log.LogDebug($"ExecuteRequest() T executed the request");
            return await GetMemoryStreamFromResponseStream(response);
          }
        }
        catch (WebException ex)
        {
          log.LogDebug($"ExecuteRequest() T: InWebException");
          using (WebResponse exResponse = ex.Response)
          {
            HttpWebResponse httpResponse = (HttpWebResponse) exResponse;
            log.LogDebug(
              $"ExecuteRequestException() T: errorCode: {httpResponse.StatusCode}");
            throw new Exception($"{httpResponse.StatusCode}");
          }
        }
        catch (Exception ex)
        {
          log.LogDebug($"ExecuteRequestException() T: errorCode: {ex.Message}");
          if (ex.InnerException != null)
            log.LogDebug($"ExecuteRequestInnerException() T: errorCode: {ex.InnerException.Message}");
          throw;
        }
        finally
        {
          response?.Dispose();
        }
        return null;
      }

      public async Task<T> ExecuteActualRequest<T>(Stream requestSteam = null)
      {
        var request = await PrepareWebRequest(endpoint, method, customHeaders, payloadData, requestSteam);
        string responseString = null;
        WebResponse response = null;
        try
        {
          response = await request.GetResponseAsync();
          if (response != null)
          {
            log.LogDebug($"ExecuteRequest() T executed the request");
            responseString = await GetStringFromResponseStream(response);
            log.LogDebug($"ExecuteRequest() T success: responseString {responseString}");
          }
        }
        catch (WebException ex)
        {
          log.LogDebug($"ExecuteRequest() T: InWebException");
          using (WebResponse exResponse = ex.Response)
          {
            if (exResponse == null) throw;
            log.LogDebug("ExecuteRequestException() T: going to read stream");
            responseString = await GetStringFromResponseStream(exResponse);
            HttpWebResponse httpResponse = (HttpWebResponse) exResponse;
            log.LogDebug(
              $"ExecuteRequestException() T: errorCode: {httpResponse.StatusCode} responseString: {responseString}");
            throw new Exception($"{httpResponse.StatusCode} {responseString}");
          }
        }
        catch (Exception ex)
        {
          log.LogDebug($"ExecuteRequestException() T: errorCode: {ex.Message}");
          if (ex.InnerException != null)
            log.LogDebug($"ExecuteRequestInnerException() T: errorCode: {ex.InnerException.Message}");
          throw;
        }
        finally
        {
          response?.Dispose();
        }

        if (!string.IsNullOrEmpty(responseString))
        {
          var toReturn = JsonConvert.DeserializeObject<T>(responseString);
          log.LogDebug($"ExecuteRequest() T. toReturn:{JsonConvert.SerializeObject(toReturn)}");
          return toReturn;
        }
        var defaultToReturn = default(T);
        log.LogDebug($"ExecuteRequest() T. defaultToReturn:{JsonConvert.SerializeObject(defaultToReturn)}");
        return defaultToReturn;

      }
    }

    /// <summary>
    /// Executes the request.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="endpoint">The endpoint.</param>
    /// <param name="method">The method.</param>
    /// <param name="customHeaders">The custom headers.</param>
    /// <param name="payloadData">The payload data.</param>
    /// <param name="retries">The retries.</param>
    /// <param name="suppressExceptionLogging">if set to <c>true</c> [suppress exception logging].</param>
    /// <returns></returns>
    public async Task<T> ExecuteRequest<T>(string endpoint, string method,
      IDictionary<string, string> customHeaders = null,
      string payloadData = null, int retries = 3, bool suppressExceptionLogging = false)
    {
      log.LogDebug(
        $"ExecuteRequest() T : endpoint {endpoint} method {method}, customHeaders {(customHeaders == null ? null : JsonConvert.SerializeObject(customHeaders))} payloadData {payloadData}");

      var policyResult = await Policy
        .Handle<Exception>()
        .RetryAsync(retries)
        .ExecuteAndCaptureAsync(() =>
        {
          log.LogDebug($"Trying to execute request {endpoint}");
          var executor = new RequestExecutor(endpoint, method, customHeaders, payloadData, log);
          return executor.ExecuteActualRequest<T>();
        });

      if (policyResult.FinalException != null)
      {
        if (!suppressExceptionLogging)
        {
          log.LogDebug(
            $"ExecuteRequest() T. exceptionToRethrow:{policyResult.FinalException} endpoint: {endpoint} method: {method}");
        }
        throw policyResult.FinalException;
      }
      if (policyResult.Outcome == OutcomeType.Successful)
      {
        return policyResult.Result;
      }
      return default(T);
    }


    /// <summary>
    /// Executes the request.
    /// </summary>
    /// <param name="endpoint">The endpoint.</param>
    /// <param name="method">The method.</param>
    /// <param name="customHeaders">The custom headers.</param>
    /// <param name="payloadData">The payload data.</param>
    /// <param name="retries">The retries.</param>
    /// <param name="suppressExceptionLogging">if set to <c>true</c> [suppress exception logging].</param>
    /// <returns></returns>
    public async Task<Stream> ExecuteRequest(string endpoint, string method,
      IDictionary<string, string> customHeaders = null,
      string payloadData = null, int retries = 3, bool suppressExceptionLogging = false)
    {
      log.LogDebug(
        $"ExecuteRequest() Stream: endpoint {endpoint} method {method}, customHeaders {(customHeaders == null ? null : JsonConvert.SerializeObject(customHeaders))} payloadData {payloadData}");

      var policyResult = await Policy
        .Handle<Exception>()
        .RetryAsync(retries)
        .ExecuteAndCaptureAsync(async () =>
        {
          log.LogDebug($"Trying to execute request {endpoint}");
          var executor = new RequestExecutor(endpoint, method, customHeaders, payloadData, log);
          return await executor.ExecuteActualStreamRequest();
        });

      if (policyResult.FinalException != null)
      {
        if (!suppressExceptionLogging)
        {
          log.LogDebug(
            $"ExecuteRequest() Stream: exceptionToRethrow:{policyResult.FinalException.ToString()} endpoint: {endpoint} method: {method}");
        }
        throw policyResult.FinalException;
      }
      if (policyResult.Outcome == OutcomeType.Successful)
      {
        return policyResult.Result;
      }
      return null;
    }


    /// <summary>
    /// Executes the request.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="endpoint">The endpoint.</param>
    /// <param name="payload">The payload.</param>
    /// <param name="customHeaders">The custom headers.</param>
    /// <param name="method">The method.</param>
    /// <param name="retries">The retries.</param>
    /// <param name="suppressExceptionLogging">if set to <c>true</c> [suppress exception logging].</param>
    /// <returns></returns>
    public async Task<T> ExecuteRequest<T>(string endpoint, Stream payload,
      IDictionary<string, string> customHeaders = null, string method = "POST", int retries = 3, bool suppressExceptionLogging = false)
    {
      log.LogDebug(
        $"ExecuteRequest() T(no method) : endpoint {endpoint} customHeaders {(customHeaders == null ? null : JsonConvert.SerializeObject(customHeaders))}");

      var policyResult = await Policy
        .Handle<Exception>()
        .RetryAsync(retries)
        .ExecuteAndCaptureAsync(async () =>
        {
          log.LogDebug($"Trying to execute request {endpoint}");
          var executor = new RequestExecutor(endpoint, method, customHeaders, "", log);
          return await executor.ExecuteActualRequest<T>(payload);
        });

      if (policyResult.FinalException != null)
      {
        if (!suppressExceptionLogging)
        {
          log.LogDebug(
            "ExecuteRequest_multi(). exceptionToRethrow:{0} endpoint: {1} customHeaders: {2}",
            policyResult.FinalException.ToString(), endpoint, customHeaders);
        }
        throw policyResult.FinalException;
      }
      if (policyResult.Outcome == OutcomeType.Successful)
      {
        return policyResult.Result;
      }
      return default(T);
    }

  }
}
