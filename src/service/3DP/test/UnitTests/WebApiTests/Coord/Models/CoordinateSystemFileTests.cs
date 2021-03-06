﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.Models.Models.Coords;
using VSS.Productivity3D.Productivity3D.Models.Validation;

namespace VSS.Productivity3D.WebApiTests.Coord.Models
{
  [TestClass]
  public class CoordinateSystemFileTests
  {
    [TestMethod]
    public void CanCreateCoordinateSystemFileTest()
    {
      var validator = new DataAnnotationsValidator();
      string fileName = "test.dc";
      byte[] fileContent = { 0, 1, 2, 3, 4, 5, 6, 7 };

      // Test the CreateCoordinateSystemFile() method with valid parameters... 
      CoordinateSystemFile coordSystemFile = new CoordinateSystemFile(1, fileContent, "test.dc");
      Assert.IsTrue(validator.TryValidate(coordSystemFile, out ICollection<ValidationResult> results));

      // Test the CreateCoordinateSystemFile() method with an invalid projectID... 
      coordSystemFile = new CoordinateSystemFile(-1, fileContent, fileName);
      Assert.IsFalse(validator.TryValidate(coordSystemFile, out results));

      // Test the CreateCoordinateSystemFile() method with a file name length exceeds 256 characters... 
      string prefix = "overlimit";
      //int maxCount = (int)(CoordinateSystemFile.MAX_FILE_NAME_LENGTH / prefix.Length);

      for (int i = 1; prefix.Length <= CoordinateSystemFile.MAX_FILE_NAME_LENGTH; i++)
        prefix += prefix;

      coordSystemFile = new CoordinateSystemFile(1, fileContent, prefix + fileName);
      Assert.IsFalse(validator.TryValidate(coordSystemFile, out results));

      // Test the CreateCoordinateSystemFile() method with no file name provided... 
      coordSystemFile = new CoordinateSystemFile(1, fileContent, string.Empty);
      Assert.IsFalse(validator.TryValidate(coordSystemFile, out results));

      // Test the CreateCoordinateSystemFile() method with no content provided... 
      coordSystemFile = new CoordinateSystemFile(1, null, fileName);
      Assert.IsFalse(validator.TryValidate(coordSystemFile, out results));
    }

    [TestMethod]
    public void CoordinateSystemFileValidationRequestTest()
    {
      var validator = new DataAnnotationsValidator();
      string fileName = "test.dc";
      byte[] fileContent = { 0, 1, 2, 3, 4, 5, 6, 7 };

      // Test the CreateCoordinateSystemFileValidationRequest() method with valid parameters... 
      CoordinateSystemFileValidationRequest coordSystemFileValidation = new CoordinateSystemFileValidationRequest(fileContent, "test.dc");
      Assert.IsTrue(validator.TryValidate(coordSystemFileValidation, out ICollection<ValidationResult> results));

      // Test the CreateCoordinateSystemFileValidationRequest() method with a file name length exceeds 256 characters... 
      string prefix = "overlimit";
      //int maxCount = (int)(CoordinateSystemFile.MAX_FILE_NAME_LENGTH / prefix.Length);

      for (int i = 1; prefix.Length <= CoordinateSystemFileValidationRequest.MAX_FILE_NAME_LENGTH; i++)
        prefix += prefix;

      coordSystemFileValidation = new CoordinateSystemFileValidationRequest(fileContent, prefix + fileName);
      Assert.IsFalse(validator.TryValidate(coordSystemFileValidation, out results));

      // Test the CreateCoordinateSystemFileValidationRequest() method with no file name provided... 
      coordSystemFileValidation = new CoordinateSystemFileValidationRequest(fileContent, string.Empty);
      Assert.IsFalse(validator.TryValidate(coordSystemFileValidation, out results));

      // Test the CreateCoordinateSystemFileValidationRequest() method with no content provided... 
      coordSystemFileValidation = new CoordinateSystemFileValidationRequest(null, fileName);
      Assert.IsFalse(validator.TryValidate(coordSystemFileValidation, out results));
    }
  }
}
