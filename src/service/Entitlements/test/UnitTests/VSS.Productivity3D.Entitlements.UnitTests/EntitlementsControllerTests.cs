﻿using System;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using VSS.Productivity3D.Entitlements.Abstractions;
using VSS.Productivity3D.Entitlements.Abstractions.Interfaces;
using VSS.Productivity3D.Entitlements.Abstractions.Models.Request;
using VSS.Productivity3D.Entitlements.Abstractions.Models.Response;
using VSS.Productivity3D.Entitlements.Common.Authentication;
using VSS.Productivity3D.Entitlements.WebApi.Controllers;
using Xunit;

namespace VSS.Productivity3D.Entitlements.UnitTests
{
  public class EntitlementsControllerTests : UnitTestsDIFixture<EntitlementsControllerTests>
  {
    [Fact]
    public async Task GetEntitlement_Success()
    {
      var userUid = Guid.NewGuid();
      var customerUid = Guid.NewGuid();

      var request = new EntitlementRequestModel
      {
        OrganizationIdentifier = customerUid.ToString(),
        UserUid = userUid.ToString(),
        UserEmail = "someone@somwhere.com",
        Sku = "some sku",
        Feature = "some feature"
      };

      mockConfigStore.Setup(c => c.GetValueBool(ConfigConstants.ENABLE_ENTITLEMENTS_CONFIG_KEY, false)).Returns(true);

      mockEmsClient.Setup(e => e.GetEntitlements(userUid, customerUid, request.Sku, request.Feature, It.IsAny<IHeaderDictionary>())).ReturnsAsync(HttpStatusCode.OK);

      mockAuthn.Setup(a => a.GetApplicationBearerToken()).Returns("some token");

      var controller = CreateEntitlementsController(userUid.ToString());
      var result = await controller.GetEntitlementInternal(request);
      Assert.NotNull(result);
      var response = (result as JsonResult)?.Value as EntitlementResponseModel;
      Assert.NotNull(response);
      Assert.Equal(request.OrganizationIdentifier, response.OrganizationIdentifier);
      Assert.Equal(request.UserUid, response.UserUid);
      Assert.Equal(request.UserEmail, response.UserEmail);
      Assert.Equal(request.Sku, response.Sku);
      Assert.Equal(request.Feature, response.Feature);
      Assert.True(response.IsEntitled);
    }

    [Fact]
    public async Task GetEntitlement_NotEntitled()
    {
      var userUid = Guid.NewGuid();
      var customerUid = Guid.NewGuid();

      var request = new EntitlementRequestModel
      {
        OrganizationIdentifier = customerUid.ToString(),
        UserUid = userUid.ToString(),
        UserEmail = "someone@somwhere.com",
        Sku = "some sku",
        Feature = "some feature"
      };

      mockConfigStore.Setup(c => c.GetValueBool(ConfigConstants.ENABLE_ENTITLEMENTS_CONFIG_KEY, false)).Returns(true);

      mockEmsClient.Setup(e => e.GetEntitlements(userUid, customerUid, request.Sku, request.Feature, It.IsAny<IHeaderDictionary>())).ReturnsAsync(HttpStatusCode.NoContent);

      mockAuthn.Setup(a => a.GetApplicationBearerToken()).Returns("some token");

      var controller = CreateEntitlementsController(userUid.ToString());
      var result = await controller.GetEntitlementInternal(request);
      Assert.NotNull(result);
      var response = (result as JsonResult)?.Value as EntitlementResponseModel;
      Assert.NotNull(response);
      Assert.Equal(request.OrganizationIdentifier, response.OrganizationIdentifier);
      Assert.Equal(request.UserUid, response.UserUid);
      Assert.Equal(request.UserEmail, response.UserEmail);
      Assert.Equal(request.Sku, response.Sku);
      Assert.Equal(request.Feature, response.Feature);
      Assert.False(response.IsEntitled);
    }

    [Fact]
    public async Task GetEntitlement_AllowedEmail()
    {
      var userUid = Guid.NewGuid();
      var customerUid = Guid.NewGuid();

      var request = new EntitlementRequestModel
      {
        OrganizationIdentifier = customerUid.ToString(),
        UserUid = userUid.ToString(),
        UserEmail = "allowed@nowhere.com",
        Sku = "some sku",
        Feature = "some feature"
      };

      mockConfigStore.Setup(c => c.GetValueString(ConfigConstants.ENTITLEMENTS_ACCEPT_EMAIL_KEY, string.Empty)).Returns("allowed@nowhere.com");

      var controller = CreateEntitlementsController(userUid.ToString());
      var result = await controller.GetEntitlementInternal(request);
      Assert.NotNull(result);
      var response = (result as JsonResult)?.Value as EntitlementResponseModel;
      Assert.NotNull(response);
      Assert.Equal(request.OrganizationIdentifier, response.OrganizationIdentifier);
      Assert.Equal(request.UserUid, response.UserUid);
      Assert.Equal(request.UserEmail, response.UserEmail);
      Assert.Equal(request.Sku, response.Sku);
      Assert.Equal(request.Feature, response.Feature);
      Assert.True(response.IsEntitled);
    }

    [Fact]
    public async Task GetEntitlement_CheckDisabled()
    {
      var userUid = Guid.NewGuid();
      var customerUid = Guid.NewGuid();

      var request = new EntitlementRequestModel
      {
        OrganizationIdentifier = customerUid.ToString(),
        UserUid = userUid.ToString(),
        UserEmail = "someone@somwhere.com",
        Sku = "some sku",
        Feature = "some feature"
      };

      mockConfigStore.Setup(c => c.GetValueBool(ConfigConstants.ENABLE_ENTITLEMENTS_CONFIG_KEY, false)).Returns(false);

      var controller = CreateEntitlementsController(userUid.ToString());
      var result = await controller.GetEntitlementInternal(request);
      Assert.NotNull(result);
      var response = (result as JsonResult)?.Value as EntitlementResponseModel;
      Assert.NotNull(response);
      Assert.Equal(request.OrganizationIdentifier, response.OrganizationIdentifier);
      Assert.Equal(request.UserUid, response.UserUid);
      Assert.Equal(request.UserEmail, response.UserEmail);
      Assert.Equal(request.Sku, response.Sku);
      Assert.Equal(request.Feature, response.Feature);
      Assert.True(response.IsEntitled);
    }

    [Fact]
    public async Task GetEntitlement_NoRequest()
    {
      var controller = CreateEntitlementsController(Guid.NewGuid().ToString());
      var result = await controller.GetEntitlementInternal(null);
      Assert.NotNull(result);
      var response = result as BadRequestResult;
      Assert.NotNull(response);
      Assert.Equal(400, response.StatusCode);
    }

    [Fact]
    public async Task GetEntitlement_DifferentUserUid()
    {
      var request = new EntitlementRequestModel
      {
        OrganizationIdentifier = Guid.NewGuid().ToString(),
        UserUid = Guid.NewGuid().ToString(),
        Sku = "some sku",
        Feature = "some feature"
      };

      var controller = CreateEntitlementsController(Guid.NewGuid().ToString());
      var result = await controller.GetEntitlementInternal(request);
      Assert.NotNull(result);
      var response = result as BadRequestObjectResult;
      Assert.NotNull(response);
      Assert.Equal(400, response.StatusCode);
      Assert.Equal("Provided uuid does not match JWT.", response.Value);
    }

    [Fact]
    public async Task GetExternalEntitlement_Success()
    {
      var userUid = Guid.NewGuid();
      var customerUid = Guid.NewGuid();

      var request = new ExternalEntitlementRequestModel
      {
        OrganizationIdentifier = customerUid.ToString(),
        UserEmail = "someone@somwhere.com",
        ApplicationName = "worksos"
      };

      var mockFeature = "some feature";
      var mockSku = "some sku";
      mockConfigStore.Setup(c => c.GetValueBool(ConfigConstants.ENABLE_ENTITLEMENTS_CONFIG_KEY, false)).Returns(true);
      mockConfigStore.Setup(c => c.GetValueString(ConfigConstants.ENTITLEMENTS_FEATURE_CONFIG_KEY, "FEA-CEC-WORKSOS")).Returns(mockFeature);
      mockConfigStore.Setup(c => c.GetValueString(ConfigConstants.ENTITLEMENTS_SKU_CONFIG_KEY, "HCC-WOS-MO")).Returns(mockSku);

      mockEmsClient.Setup(e => e.GetEntitlements(userUid, customerUid, mockSku, mockFeature, It.IsAny<IHeaderDictionary>())).ReturnsAsync(HttpStatusCode.OK);

      mockAuthn.Setup(a => a.GetApplicationBearerToken()).Returns("some token");

      var controller = CreateEntitlementsController(userUid.ToString());
      var result = await controller.GetEntitlementExternal(request);
      Assert.NotNull(result);
      var response = (result as JsonResult)?.Value as EntitlementResponseModel;
      Assert.NotNull(response);
      Assert.Equal(request.OrganizationIdentifier, response.OrganizationIdentifier);
      Assert.Equal(userUid.ToString(), response.UserUid);
      Assert.Equal(request.UserEmail, response.UserEmail);
      Assert.Equal(mockSku, response.Sku);
      Assert.Equal(mockFeature, response.Feature);
      Assert.True(response.IsEntitled);
    }

    [Fact]
    public async Task GetExternalEntitlement_UnknownApplication()
    {
      var userUid = Guid.NewGuid();
      var customerUid = Guid.NewGuid();

      var request = new ExternalEntitlementRequestModel
      {
        OrganizationIdentifier = customerUid.ToString(),
        UserEmail = "someone@somwhere.com",
        ApplicationName = "dummy"
      };

      var mockFeature = "some feature";
      var mockSku = "some sku";
      mockConfigStore.Setup(c => c.GetValueBool(ConfigConstants.ENABLE_ENTITLEMENTS_CONFIG_KEY, false)).Returns(true);
      mockConfigStore.Setup(c => c.GetValueString(ConfigConstants.ENTITLEMENTS_FEATURE_CONFIG_KEY, "FEA-CEC-WORKSOS")).Returns(mockFeature);
      mockConfigStore.Setup(c => c.GetValueString(ConfigConstants.ENTITLEMENTS_SKU_CONFIG_KEY, "HCC-WOS-MO")).Returns(mockSku);

      var controller = CreateEntitlementsController(userUid.ToString());
      var result = await controller.GetEntitlementExternal(request);
      Assert.NotNull(result);
      var response = result as BadRequestObjectResult;
      Assert.NotNull(response);
      Assert.Equal(400, response.StatusCode);
      Assert.Equal($"Unknown application {request.ApplicationName}", response.Value);
    }

    private EntitlementsController CreateEntitlementsController(string userUid)
    {
      var httpContext = new DefaultHttpContext();
      httpContext.RequestServices = ServiceProvider;
      httpContext.User = new EntitlementUserClaim(new GenericIdentity(userUid), null, null, false);
      var controllerContext = new ControllerContext();
      controllerContext.HttpContext = httpContext;
      var controller = new EntitlementsController();
      controller.ControllerContext = controllerContext;
      return controller;
    }
  }
}
