﻿
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.Common.ResultHandling;

namespace VSS.Raptor.Service.WebApiTests.Common.Models
{
  [TestClass()]
  public class DesignDescriptorTests
  {

    [TestMethod()]
    public void CanCreateDesignDescriptorTest()
    {
      var validator = new DataAnnotationsValidator();
      DesignDescriptor design = DesignDescriptor.CreateDesignDescriptor(1234, null, 0);
      ICollection<ValidationResult> results;
      Assert.IsTrue(validator.TryValidate(design, out results));
    }

    [TestMethod()]
    public void ValidateSuccessTest()
    {
      DesignDescriptor design = DesignDescriptor.CreateDesignDescriptor(1234, null, 0);
      design.Validate();

      design = DesignDescriptor.CreateDesignDescriptor(0, 
        FileDescriptor.CreateFileDescriptor("u72003136-d859-4be8-86de-c559c841bf10",
        "BC Data/Sites/Integration10/Designs", "Cycleway.ttm"), 0);
      design.Validate();
    }

    [TestMethod()]
    [ExpectedException(typeof(ServiceException))]
    public void ValidateFailEmptyTest()
    {
      //empty design descriptor
      DesignDescriptor design = DesignDescriptor.CreateDesignDescriptor(0, null, 0);
      design.Validate();
    }


  }
}
