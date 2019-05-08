﻿using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VSS.Tpaas.Client.Abstractions;
using VSS.Tpaas.Client.Models;

namespace VSS.Tpaas.Client.RequestHandlers
{
  /// <summary>
  /// This is a request handler which acts a middleware to outingoing requests
  /// which handles all necessary TPaaS authentication for the application.
  /// 
  /// Example usage (in Startup.cs)
  /// 
  ///   ...
  ///   services.AddHttpClient<ITPaaSClient, TPaaSClient>(client =>
  ///     client.BaseAddress = new Uri(Configuration.GetValueString(TPaaSClient.TPAAS_AUTH_URL_ENV_KEY))
  ///   ).ConfigurePrimaryHttpMessageHandler(() => new TPaaSApplicationCredentialsRequestHandler
  ///  {
  ///     TPaaSToken = Configuration.GetValueString(TPaaSApplicationCredentialsRequestHandler.TPAAS_APP_TOKEN_ENV_KEY),
  ///     InnerHandler = new HttpClientHandler()
  ///   });
  ///
  ///   services.AddTransient(context => new TPaaSAuthenticatedRequestHandler
  ///   {
  ///     TPaaSClient = context.GetService<ITPaaSClient>()
  ///   });
  ///
  ///   services.AddHttpClient<IYOUR_TYPED_HTTP_CLIENT, YOUR_TYPED_HTTP_CLIENT>(client =>
  ///   client.BaseAddress = new Uri("YOUR_BASE_URI")
  ///   ).AddHttpMessageHandler<TPaaSAuthenticatedRequestHandler>();
  ///   ...
  ///   
  /// see <a href="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-2.1">MS Documentation</a>
  /// for full details of the pattern.
  /// </summary>
  public class TPaaSAuthenticatedRequestHandler : DelegatingHandler
  {

    public ITPaaSClient TPaaSClient { get; set; }

    public TPaaSAuthenticatedRequestHandler(HttpMessageHandler innerHandler) :
      base(innerHandler)
    {
    }

    /// <summary>
    /// Default constructor; required for dependency injection.
    /// </summary>
    public TPaaSAuthenticatedRequestHandler()
      : base()
    { }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
      try
      {
        if (!request.Headers.Contains("Authorization"))
        {
          string bearerToken = await TPaaSClient.GetBearerTokenAsync();

          request.Headers.Add("Authorization", bearerToken);
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
