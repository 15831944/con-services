﻿using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCaching.Internal;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace VSS.Productivity3D.Common.Filters
{
  //Based on reference implementation
  public class CustomCachingPolicyProvider : ResponseCachingPolicyProvider
  {

    private static readonly CacheControlHeaderValue EmptyCacheControl = new CacheControlHeaderValue();

    public override bool AttemptResponseCaching(ResponseCachingContext context)
    {
      var request = context.HttpContext.Request;

      // Verify the method
      if (!HttpMethods.IsGet(request.Method) && !HttpMethods.IsHead(request.Method))
      {
        return false;
      }

      return true;
    }
   
    public override bool IsResponseCacheable(ResponseCachingContext context)
    {

      var typedHeaders = context.HttpContext.Response.GetTypedHeaders();
      var responseHeaders = typedHeaders.CacheControl ?? EmptyCacheControl;


      // Check no-store
      if (responseHeaders.NoStore)
      {
        return false;
      }

      // Check no-cache
      if (responseHeaders.NoCache)
      {
        return false;
      }

      var response = context.HttpContext.Response;

      // Do not cache responses with Set-Cookie headers
      if (!StringValues.IsNullOrEmpty(response.Headers[HeaderNames.SetCookie]))
      {
        return false;
      }

      // Do not cache responses varying by *
      var varyHeader = response.Headers[HeaderNames.Vary];
      if (varyHeader.Count == 1 && string.Equals(varyHeader, "*", StringComparison.OrdinalIgnoreCase))
      {
        return false;
      }

      // Check private
      if (responseHeaders.Private)
      {
        return false;
      }

      // Check response code
      if (response.StatusCode != StatusCodes.Status200OK)
      {
        return false;
      }

      // Check response freshness
      if (!typedHeaders.Date.HasValue)
      {
        if (!responseHeaders.SharedMaxAge.HasValue &&
            !responseHeaders.MaxAge.HasValue &&
            context.ResponseTime.Value >= typedHeaders.Expires)
        {
          return false;
        }
      }
      else
      {
        var age = context.ResponseTime.Value - typedHeaders.Date.Value;

        // Validate shared max age
        var sharedMaxAge = responseHeaders.SharedMaxAge;
        if (age >= sharedMaxAge)
        {
          return false;
        }
        else if (!sharedMaxAge.HasValue)
        {
          // Validate max age
          var maxAge = responseHeaders.MaxAge;
          if (age >= maxAge)
          {
            return false;
          }
          else if (!maxAge.HasValue)
          {
            // Validate expiration
            if (context.ResponseTime.Value >= typedHeaders.Expires)
            {
              return false;
            }
          }
        }
      }

      return true;
    }
  }
}
