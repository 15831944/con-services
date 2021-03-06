﻿using System;
using System.Net;
using System.Security.Authentication;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Serilog;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Serilog.Extensions;

namespace VSS.WebApi.Common.UnitTests
{
  [TestClass]
  public class ExceptionTrapTests 
  {
    protected IServiceProvider ServiceProvider;

    [TestInitialize]
    public virtual void InitTest()
    {
      var loggerFactory = new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.WebApi.Common.UnitTests.log"));
      var serviceCollection = new ServiceCollection();

      serviceCollection.AddLogging()
                       .AddSingleton(loggerFactory)
                       .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()
                       .AddTransient<IErrorCodesProvider, ContractExecutionStatesEnum>();

      ServiceProvider = serviceCollection.BuildServiceProvider();
    }

    [TestMethod]
    public void CanCreateExceptionTrap()
    {
      Assert.IsNotNull(new ExceptionsTrap(null,null));
    }

    [TestMethod]
    public async Task CanExecuteExceptionTrap()
    {
      var mockHttpContext = new Mock<HttpContext>();
      RequestDelegate mockRequestDelegate = context => Task.FromResult(mockHttpContext.Object);
      var trap = new ExceptionsTrap(mockRequestDelegate,null);
      await trap.Invoke(mockHttpContext.Object);
      //nothing to assert here - make sure that no exceptions are thrown
    }

    /* TODO: DefaultHttpResponse is not available in .Net Core 3.0+. Need to understand how to replace it use in the two tests below      
    [TestMethod]
    public async Task ThrowsNonAuthenticatedException()
    {
      var mockHttpContext = new Mock<HttpContext>();
      var defaultResponse = new DefaultHttpResponse(new DefaultHttpContext());
      mockHttpContext.SetupGet(mc => mc.Response).Returns(defaultResponse);
      RequestDelegate mockRequestDelegate = context => throw new AuthenticationException();
      var trap = new ExceptionsTrap(mockRequestDelegate, null);
      await trap.Invoke(mockHttpContext.Object);
      Assert.AreEqual((int)HttpStatusCode.Unauthorized,defaultResponse.StatusCode);
    }

    [TestMethod]
    public async Task ThrowsServiceException()
    {
      var mockHttpContext = new Mock<HttpContext>();
      var defaultResponse = new DefaultHttpResponse(new DefaultHttpContext());
      mockHttpContext.SetupGet(mc => mc.Response).Returns(defaultResponse);
      RequestDelegate mockRequestDelegate = context => throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult());
      var trap = new ExceptionsTrap(mockRequestDelegate, this.ServiceProvider.GetRequiredService<ILogger<ExceptionsTrap>>());
      await trap.Invoke(mockHttpContext.Object);
      Assert.AreEqual((int)HttpStatusCode.BadRequest, defaultResponse.StatusCode);
    }
    */
  }
}
