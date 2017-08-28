﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:1.9.0.77
//      SpecFlow Generator Version:1.9.0.0
//      Runtime Version:4.0.30319.42000
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace ProductionDataSvc.AcceptanceTests
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "1.9.0.77")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute()]
    public partial class ExportGriddedCSVFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "ExportGriddedCSV.feature"
#line hidden
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute()]
        public static void FeatureSetup(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext testContext)
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "ExportGriddedCSV", "I should be able to request Gridded CSV Exports for a project.", ProgrammingLanguage.CSharp, ((string[])(null)));
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.ClassCleanupAttribute()]
        public static void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute()]
        public virtual void TestInitialize()
        {
            if (((TechTalk.SpecFlow.FeatureContext.Current != null) 
                        && (TechTalk.SpecFlow.FeatureContext.Current.FeatureInfo.Title != "ExportGriddedCSV")))
            {
                ProductionDataSvc.AcceptanceTests.ExportGriddedCSVFeature.FeatureSetup(null);
            }
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute()]
        public virtual void ScenarioTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioSetup(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioStart(scenarioInfo);
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        public virtual void FeatureBackground()
        {
#line 4
#line 5
 testRunner.Given("the Export Gridded CSV service URI \"/api/v1/export/gridded/csv\", request repo \"Ex" +
                    "portGriddedCSVRequest.json\" and result repo \"ExportGriddedCSVResponse.json\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
        }
        
        public virtual void ExportGriddedCSV_GoodRequest(string requestName, string resultName, string[] exampleTags)
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("ExportGriddedCSV - Good Request", exampleTags);
#line 7
this.ScenarioSetup(scenarioInfo);
#line 4
this.FeatureBackground();
#line 8
 testRunner.When(string.Format("I request Export Gridded CSV supplying \"{0}\" from the request repository", requestName), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 9
 testRunner.Then(string.Format("the result should match \"{0}\" from the result repository", resultName), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("ExportGriddedCSV - Good Request")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "ExportGriddedCSV")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "FullProjectLatestDateElevationOnlyGriddedCSVExport")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "FullProjectLatestDateElevationOnlyGriddedCSVExport")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "FullProjectLatestDateElevationOnlyGriddedCSVExport")]
        public virtual void ExportGriddedCSV_GoodRequest_FullProjectLatestDateElevationOnlyGriddedCSVExport()
        {
            this.ExportGriddedCSV_GoodRequest("FullProjectLatestDateElevationOnlyGriddedCSVExport", "FullProjectLatestDateElevationOnlyGriddedCSVExport", ((string[])(null)));
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("ExportGriddedCSV - Good Request")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "ExportGriddedCSV")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "FullProjectSpecificDateElevationOnlyGriddedCSVExport")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "FullProjectSpecificDateElevationOnlyGriddedCSVExport")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "FullProjectSpecificDateElevationOnlyGriddedCSVExport")]
        public virtual void ExportGriddedCSV_GoodRequest_FullProjectSpecificDateElevationOnlyGriddedCSVExport()
        {
            this.ExportGriddedCSV_GoodRequest("FullProjectSpecificDateElevationOnlyGriddedCSVExport", "FullProjectSpecificDateElevationOnlyGriddedCSVExport", ((string[])(null)));
        }
        
        public virtual void ExportGriddedCSV_BadRequest(string requestName, string errorCode, string errorMessage, string[] exampleTags)
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("ExportGriddedCSV - Bad Request", exampleTags);
#line 15
this.ScenarioSetup(scenarioInfo);
#line 4
this.FeatureBackground();
#line 16
 testRunner.When(string.Format("I request Export Gridded CSV supplying \"{0}\" from the request repository expectin" +
                        "g BadRequest", requestName), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 17
 testRunner.Then(string.Format("the result should contain error code {0} and error message \"{1}\"", errorCode, errorMessage), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("ExportGriddedCSV - Bad Request")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "ExportGriddedCSV")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "BadRequestNoReportType")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "BadRequestNoReportType")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ErrorCode", "-1")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ErrorMessage", "Grid report type must be either 1 (\'Gridded\') or 2 (\'Alignment\'). Actual value su" +
            "pplied: 0")]
        public virtual void ExportGriddedCSV_BadRequest_BadRequestNoReportType()
        {
            this.ExportGriddedCSV_BadRequest("BadRequestNoReportType", "-1", "Grid report type must be either 1 (\'Gridded\') or 2 (\'Alignment\'). Actual value su" +
                    "pplied: 0", ((string[])(null)));
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("ExportGriddedCSV - Bad Request")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "ExportGriddedCSV")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "BadRequestUnknownReportType")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "BadRequestUnknownReportType")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ErrorCode", "-1")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ErrorMessage", "Grid report type must be either 1 (\'Gridded\') or 2 (\'Alignment\'). Actual value su" +
            "pplied: 10")]
        public virtual void ExportGriddedCSV_BadRequest_BadRequestUnknownReportType()
        {
            this.ExportGriddedCSV_BadRequest("BadRequestUnknownReportType", "-1", "Grid report type must be either 1 (\'Gridded\') or 2 (\'Alignment\'). Actual value su" +
                    "pplied: 10", ((string[])(null)));
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("ExportGriddedCSV - Bad Request")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "ExportGriddedCSV")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "BadRequestIntervalTooSmall")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "BadRequestIntervalTooSmall")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ErrorCode", "-1")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ErrorMessage", "Interval must be >= 0.1m and <= 100.0m. Actual value: 0.09")]
        public virtual void ExportGriddedCSV_BadRequest_BadRequestIntervalTooSmall()
        {
            this.ExportGriddedCSV_BadRequest("BadRequestIntervalTooSmall", "-1", "Interval must be >= 0.1m and <= 100.0m. Actual value: 0.09", ((string[])(null)));
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("ExportGriddedCSV - Bad Request")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "ExportGriddedCSV")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "BadRequestIntervalTooLarge")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "BadRequestIntervalTooLarge")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ErrorCode", "-1")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ErrorMessage", "Interval must be >= 0.1m and <= 100.0m. Actual value: 101")]
        public virtual void ExportGriddedCSV_BadRequest_BadRequestIntervalTooLarge()
        {
            this.ExportGriddedCSV_BadRequest("BadRequestIntervalTooLarge", "-1", "Interval must be >= 0.1m and <= 100.0m. Actual value: 101", ((string[])(null)));
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("ExportGriddedCSV - Bad Request")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "ExportGriddedCSV")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "BadRequestNoOutputFieldsConfigured")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "BadRequestNoOutputFieldsConfigured")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ErrorCode", "-1")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ErrorMessage", "There are no selected fields to be reported on")]
        public virtual void ExportGriddedCSV_BadRequest_BadRequestNoOutputFieldsConfigured()
        {
            this.ExportGriddedCSV_BadRequest("BadRequestNoOutputFieldsConfigured", "-1", "There are no selected fields to be reported on", ((string[])(null)));
        }
    }
}
#pragma warning restore
#endregion
