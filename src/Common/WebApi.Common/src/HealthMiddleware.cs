﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace VSS.WebApi.Common
{ 
  public class HealthMiddleware
  {
    private readonly RequestDelegate NextRequestDelegate;

    public HealthMiddleware(RequestDelegate nextRequestDelegate)
    {
      this.NextRequestDelegate = nextRequestDelegate;
    }

    public async Task Invoke(HttpContext context)
    {
      if (context.Request.Method == "GET" && context.Request.Path == "/areyouok")
      {
        //just return and do nothing
        return;
      }

      await NextRequestDelegate.Invoke(context);
    }
  }
}
