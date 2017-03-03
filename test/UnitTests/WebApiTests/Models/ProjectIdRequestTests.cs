﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using VSS.TagFileAuth.Service.WebApiModels.Models.RaptorServicesCommon;
using VSS.TagFileAuth.Service.WebApiModels.ResultHandling;

namespace VSS.TagFileAuth.Service.WebApiTests.Models
{
  [TestClass]
  public class ProjectIdRequestTests
  {
    [TestMethod]
    public void ValidateGetProjectIdRequest_ValidatorCase1()
    {
      GetProjectIdRequest projectIdRequest = GetProjectIdRequest.CreateGetProjectIdRequest(-1, 91, 181, 0, DateTime.MinValue, "");
      var ex = Assert.ThrowsException<ServiceException>(() => projectIdRequest.Validate());
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("Must contain one or more of assetId"));
    }

    [TestMethod]
    public void ValidateGetProjectIdRequest_ValidatorCase2()
    {
      GetProjectIdRequest projectIdRequest = GetProjectIdRequest.CreateGetProjectIdRequest(-1, 89, 179, 0, DateTime.MinValue, "");
      var ex = Assert.ThrowsException<ServiceException>(() => projectIdRequest.Validate());
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("Must contain one or more of assetId"));
    }

    [TestMethod]
    public void ValidateGetProjectIdRequest_ValidatorCase3()
    {
      GetProjectIdRequest projectIdRequest = GetProjectIdRequest.CreateGetProjectIdRequest(345345, -91, 179, 0, DateTime.MinValue, "");
      var ex = Assert.ThrowsException<ServiceException>(() => projectIdRequest.Validate());
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("Latitude value of"));
    }

    [TestMethod]
    public void ValidateGetProjectIdRequest_ValidatorCase4()
    {
      GetProjectIdRequest projectIdRequest = GetProjectIdRequest.CreateGetProjectIdRequest(345345, -89, 179, 0, DateTime.UtcNow.AddYears(-5).AddMonths(-1), "");
      var ex = Assert.ThrowsException<ServiceException>(() => projectIdRequest.Validate());
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("timeOfPosition must have occured"));
    }
    
    [TestMethod]
    public void ValidateProjectIdRequest_ValidatorCase5()
    {
      GetProjectIdRequest projectIdRequest = GetProjectIdRequest.CreateGetProjectIdRequest(345345, -89, 179, 0, DateTime.UtcNow.AddMonths(-1), "");
      projectIdRequest.Validate();
    }

    [TestMethod]
    public void ValidateProjectIdRequest_ValidatorCase6()
    {
      GetProjectIdRequest projectIdRequest = GetProjectIdRequest.CreateGetProjectIdRequest(0, -89, 179, 0, DateTime.UtcNow.AddMonths(-1), "dfgert34-dg43545");
      projectIdRequest.Validate();
    }

  }
}
