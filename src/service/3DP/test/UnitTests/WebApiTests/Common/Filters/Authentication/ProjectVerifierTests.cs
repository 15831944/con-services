﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Common.Abstractions.Extensions;
using VSS.Common.Exceptions;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Productivity3D.Models;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Models;

namespace VSS.Productivity3D.WebApiTests.Common.Filters.Authentication
{
  [TestClass]
  public class ProjectVerifierTests
  {
    private long legacyProjectId => projectUid.ToLegacyId();
    private readonly Guid projectUid = Guid.NewGuid();
    private readonly Guid customerUid = Guid.NewGuid();
    private readonly DefaultHttpContext httpContext = new DefaultHttpContext();

    [TestMethod]
    public void Should_throw_When_no_project_identifier_is_found()
    {
      var context = new ActionExecutingContext(
        new ActionContext
        {
          HttpContext = httpContext,
          RouteData = new RouteData(),
          ActionDescriptor = new ActionDescriptor()
        },
        new List<IFilterMetadata>(),
        new Dictionary<string, object>(),
        new Mock<Controller>().Object);

      var projectVerifier = new ProjectVerifier();

      var ex = Assert.ThrowsException<ServiceException>(() => projectVerifier.OnActionExecuting(context));
      Assert.AreEqual("{\"code\":-1,\"message\":\"ProjectId and ProjectUID cannot both be null.\"}", ex.GetContent);
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);
    }

    [TestMethod]
    public void Should_throw_When_actionArguments_contains_no_request_parameter()
    {
      var actionArguments = new Dictionary<string, object>
      {
        {"not_request", new ProjectID()}
      };

      var context = new ActionExecutingContext(
        new ActionContext
        {
          HttpContext = httpContext,
          RouteData = new RouteData(),
          ActionDescriptor = new ActionDescriptor()
        },
        new List<IFilterMetadata>(),
        actionArguments,
        new Mock<Controller>().Object);

      var projectVerifier = new ProjectVerifier();

      var ex = Assert.ThrowsException<ServiceException>(() => projectVerifier.OnActionExecuting(context));
      Assert.AreEqual("{\"code\":-1,\"message\":\"ProjectId and ProjectUID cannot both be null.\"}", ex.GetContent);
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);
    }

    [TestMethod]
    public void Should_throw_When_actionArguments_contains_no_project_identifier()
    {
      var actionArguments = new Dictionary<string, object>
      {
        {"request", new ProjectID()}
      };

      var projectData = new ProjectData { ProjectUID = projectUid.ToString() };
      var contextHeaders = new HeaderDictionary();

      var mockProxy = new Mock<IProjectProxy>();
      mockProxy.Setup(proxy => proxy.GetProjectForCustomer(customerUid.ToString(), projectUid.ToString(), contextHeaders)).ReturnsAsync(projectData);

      httpContext.User = new RaptorPrincipal(new ClaimsIdentity(), Guid.NewGuid().ToString(), "customerName", "merino@vss.com", true, "3D Productivity", mockProxy.Object, contextHeaders);

      var context = new ActionExecutingContext(
        new ActionContext
        {
          HttpContext = httpContext,
          RouteData = new RouteData(),
          ActionDescriptor = new ActionDescriptor()
        },
        new List<IFilterMetadata>(),
        actionArguments,
        new Mock<Controller>().Object);

      var projectVerifier = new ProjectVerifier();

      var ex = Assert.ThrowsException<ServiceException>(() => projectVerifier.OnActionExecuting(context));

      Assert.IsNotNull(ex);
      Assert.AreEqual("{\"code\":-1,\"message\":\"ProjectId and ProjectUID cannot both be null.\"}", ex.GetContent);
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);
    }

    [TestMethod]
    public void Should_throw_When_RaptorPrincipal_Returns_null_projectDescriptor()
    {
      var actionArguments = new Dictionary<string, object>
      {
        {"request", new ProjectID{ProjectUid = projectUid }}
      };

      var projectData = new ProjectData { ProjectUID = projectUid.ToString() };
      var contextHeaders = new HeaderDictionary();

      var mockProxy = new Mock<IProjectProxy>();
      mockProxy.Setup(proxy => proxy.GetProjectForCustomer(customerUid.ToString(), projectUid.ToString(), contextHeaders)).ReturnsAsync(projectData);

      httpContext.User = new RaptorPrincipal(new ClaimsIdentity(), Guid.NewGuid().ToString(), "customerName", "merino@vss.com", true, "3D Productivity", mockProxy.Object, contextHeaders);

      var context = new ActionExecutingContext(
        new ActionContext
        {
          HttpContext = httpContext,
          RouteData = new RouteData(),
          ActionDescriptor = new ActionDescriptor()
        },
        new List<IFilterMetadata>(),
        actionArguments,
        new Mock<Controller>().Object);

      var projectVerifier = new ProjectVerifier();

      var ex = Assert.ThrowsException<AggregateException>(() => projectVerifier.OnActionExecuting(context));

      var innerException = ex.InnerExceptions[0] as ServiceException;

      Assert.IsNotNull(innerException);
      Assert.AreEqual($"{{\"code\":-5,\"message\":\"Missing Project or project does not belong to specified customer or don\'t have access to the project {projectUid}\"}}", innerException.GetContent);
      Assert.AreEqual(HttpStatusCode.Unauthorized, innerException.Code);
    }

    [TestMethod]
    public void Should_throw_When_request_body_contains_projectId_and_project_isnt_found()
    {
      var actionArguments = new Dictionary<string, object>
      {
        {"request", new ProjectID{ProjectId = legacyProjectId } }
      };

      var projectData = new ProjectData { ProjectUID = projectUid.ToString() };
      var contextHeaders = new HeaderDictionary();

      var mockProxy = new Mock<IProjectProxy>();
      mockProxy.Setup(proxy => proxy.GetProjectForCustomer(customerUid.ToString(), legacyProjectId, contextHeaders)).ReturnsAsync(projectData);

      httpContext.User = new RaptorPrincipal(new ClaimsIdentity(), Guid.NewGuid().ToString(), "customerName", "merino@vss.com", true, "3D Productivity", mockProxy.Object, contextHeaders);

      var context = new ActionExecutingContext(
        new ActionContext
        {
          HttpContext = httpContext,
          RouteData = new RouteData(),
          ActionDescriptor = new ActionDescriptor()
        },
        new List<IFilterMetadata>(),
        actionArguments,
        new Mock<Controller>().Object);

      var projectVerifier = new ProjectVerifier();

      var ex = Assert.ThrowsException<AggregateException>(() => projectVerifier.OnActionExecuting(context));
      var innerException = ex.InnerExceptions[0] as ServiceException;

      Assert.IsNotNull(innerException);
      Assert.AreEqual($"{{\"code\":-5,\"message\":\"Missing Project or project does not belong to specified customer or don\'t have access to the project {legacyProjectId}\"}}", innerException.GetContent);
      Assert.AreEqual(HttpStatusCode.Unauthorized, innerException.Code);
    }

    [TestMethod]
    public void Should_not_throw_When_request_body_contains_projectId()
    {
      var actionArguments = new Dictionary<string, object>
      {
        {"request", new ProjectID { ProjectId = legacyProjectId } }
      };

      var projectData = new ProjectData { ProjectUID = projectUid.ToString() };
      var contextHeaders = new HeaderDictionary();

      var mockProxy = new Mock<IProjectProxy>();
      mockProxy.Setup(proxy => proxy.GetProjectForCustomer(customerUid.ToString(), legacyProjectId, contextHeaders)).ReturnsAsync(projectData);

      httpContext.User = new RaptorPrincipal(new ClaimsIdentity(), customerUid.ToString(), "customerName", "merino@vss.com", true, "3D Productivity", mockProxy.Object, contextHeaders);

      var context = new ActionExecutingContext(
        new ActionContext
        {
          HttpContext = httpContext,
          RouteData = new RouteData(),
          ActionDescriptor = new ActionDescriptor()
        },
        new List<IFilterMetadata>(),
        actionArguments,
        new Mock<Controller>().Object);

      var projectVerifier = new ProjectVerifier();

      projectVerifier.OnActionExecuting(context);

      var request = context.ActionArguments["request"] as ProjectID;

      Assert.IsNotNull(request);
      Assert.AreEqual(projectUid, request.ProjectUid);
    }

    [TestMethod]
    public void Should_throw_When_request_body_contains_projectUid_and_project_isnt_found()
    {
      var actionArguments = new Dictionary<string, object>
      {
        {"request", new ProjectID{ProjectUid = projectUid } }
      };

      var projectData = new ProjectData { ProjectUID = projectUid.ToString() };
      var contextHeaders = new HeaderDictionary();

      var mockProxy = new Mock<IProjectProxy>();
      mockProxy.Setup(proxy => proxy.GetProjectForCustomer(customerUid.ToString(), projectUid.ToString(), contextHeaders)).ReturnsAsync(projectData);

      httpContext.User = new RaptorPrincipal(new ClaimsIdentity(), Guid.NewGuid().ToString(), "customerName", "merino@vss.com", true, "3D Productivity", mockProxy.Object, contextHeaders);

      var context = new ActionExecutingContext(
        new ActionContext
        {
          HttpContext = httpContext,
          RouteData = new RouteData(),
          ActionDescriptor = new ActionDescriptor()
        },
        new List<IFilterMetadata>(),
        actionArguments,
        new Mock<Controller>().Object);

      var projectVerifier = new ProjectVerifier();

      var ex = Assert.ThrowsException<AggregateException>(() => projectVerifier.OnActionExecuting(context));
      var innerException = ex.InnerExceptions[0] as ServiceException;

      Assert.IsNotNull(innerException);
      Assert.AreEqual($"{{\"code\":-5,\"message\":\"Missing Project or project does not belong to specified customer or don\'t have access to the project {projectUid}\"}}", innerException.GetContent);
      Assert.AreEqual(HttpStatusCode.Unauthorized, innerException.Code);
    }

    [TestMethod]
    public void Should_not_throw_When_request_body_contains_projectUid()
    {
      var actionArguments = new Dictionary<string, object>
      {
        {"request", new ProjectID{ProjectUid = projectUid } }
      };

      var projectData = new ProjectData { ProjectUID = projectUid.ToString() };
      var contextHeaders = new HeaderDictionary();

      var mockProxy = new Mock<IProjectProxy>();
      mockProxy.Setup(proxy => proxy.GetProjectForCustomer(customerUid.ToString(), projectUid.ToString(), contextHeaders)).ReturnsAsync(projectData);

      httpContext.User = new RaptorPrincipal(new ClaimsIdentity(), customerUid.ToString(), "customerName", "merino@vss.com", true, "3D Productivity", mockProxy.Object, contextHeaders);

      var context = new ActionExecutingContext(
        new ActionContext
        {
          HttpContext = httpContext,
          RouteData = new RouteData(),
          ActionDescriptor = new ActionDescriptor()
        },
        new List<IFilterMetadata>(),
        actionArguments,
        new Mock<Controller>().Object);

      var projectVerifier = new ProjectVerifier();

      projectVerifier.OnActionExecuting(context);

      var request = context.ActionArguments["request"] as ProjectID;

      Assert.IsNotNull(request);
      Assert.AreEqual(legacyProjectId, request.ProjectId);
    }

    [TestMethod]
    public void Should_not_throw_When_actionArguments_contains_projectId()
    {
      var actionArguments = new Dictionary<string, object>
      {
        {"projectid", legacyProjectId}
      };

      var context = new ActionExecutingContext(
        new ActionContext
        {
          HttpContext = httpContext,
          RouteData = new RouteData(),
          ActionDescriptor = new ActionDescriptor()
        },
        new List<IFilterMetadata>(),
        actionArguments,
        new Mock<Controller>().Object);

      new ProjectVerifier().OnActionExecuting(context);
    }

    [TestMethod]
    public void Should_not_throw_When_actionArguments_contains_projectUid()
    {
      var actionArguments = new Dictionary<string, object>
      {
        {"projectuid", projectUid}
      };

      var context = new ActionExecutingContext(
        new ActionContext
        {
          HttpContext = httpContext,
          RouteData = new RouteData(),
          ActionDescriptor = new ActionDescriptor()
        },
        new List<IFilterMetadata>(),
        actionArguments,
        new Mock<Controller>().Object);

      new ProjectVerifier().OnActionExecuting(context);
    }
  }
}
