﻿using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VSS.Common.Abstractions.Http;
using VSS.Tpaas.Client.Abstractions;
using VSS.Tpaas.Client.Models;

namespace VSS.TRex.HttpClients
{
  /// <summary>
  /// This is a request handler which acts a middleware to outingoing requests
  /// which handles all necessary TPaaS authentication for the application.
  /// 
  /// Example usage (in Startup.cs)
  /// 
  ///   ...
  ///    services.AddTransient<TPaaSAuthenticatedRequestHandler>();
  ///    services.AddHttpClient<YOUR_TYPED_HTTP_CLIENT>(client => 
  ///        client.BaseAddress = new Uri("YOUR_BASE_URI")
  ///      )
  ///      .AddHttpMessageHandler<TPaaSAuthenticatedRequestHandler>();
  ///   ...
  ///   
  /// see <a href="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-2.1">MS Documentation</a>
  /// for full details of the pattern.
  /// </summary>
  public class TRexTPaaSAuthenticatedRequestHandler : DelegatingHandler
  {

    public TRexTPaaSAuthenticatedRequestHandler(HttpMessageHandler innerHandler) :
      base(innerHandler)
    {

    }

    /// <summary>
    /// Default constructor; required for dependency injection.
    /// </summary>
    public TRexTPaaSAuthenticatedRequestHandler()
      : base()
    { }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
      try
      {
        if (!request.Headers.Contains(HeaderConstants.AUTHORIZATION))
        {
          string bearerToken = await DI.DIContext.Obtain<ITPaaSClient>().GetBearerTokenAsync();

          request.Headers.Add(HeaderConstants.AUTHORIZATION, bearerToken);
        }

        return await base.SendAsync(request, cancellationToken);
      }
      catch (NullReferenceException ex)
      {
        throw new TPaaSAuthenticatedRequestHandlerException("Bearer could not be obtained, have you DI'd the TPaaSAppCreds Client?", ex);
      }
    }
  }
}
