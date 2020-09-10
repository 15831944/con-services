﻿using CCSS.Productivity3D.Service.Common.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.WebApi.Models.Extensions;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.WebApiTests.ProductionData.Models.Extensions
{
  [TestClass]
  public class FileDataExtensionsTests
  {
    [TestMethod]
    [DataRow(ImportedFileType.DesignSurface, true)]
    [DataRow(ImportedFileType.SurveyedSurface, true)]
    [DataRow(ImportedFileType.ReferenceSurface, true)]
    [DataRow(ImportedFileType.Alignment, false)]
    [DataRow(ImportedFileType.Linework, false)]
    [DataRow(ImportedFileType.MassHaulPlan, false)]
    [DataRow(ImportedFileType.MobileLinework, false)]
    [DataRow(ImportedFileType.SiteBoundary, false)]
    public void Should_validate_correctly_When_given_an_imported_filetype(ImportedFileType importedFileType,
      bool expectedResult)
    {
      var fileData = new FileData { ImportedFileType = importedFileType };

      Assert.AreEqual(expectedResult, fileData.IsProfileSupportedFileType());
    }
  }
}
