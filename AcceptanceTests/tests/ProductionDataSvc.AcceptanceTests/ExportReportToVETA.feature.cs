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
    public partial class ExportReportToVETAFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "ExportReportToVETA.feature"
#line hidden
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute()]
        public static void FeatureSetup(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext testContext)
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "ExportReportToVETA", "I should be able to request production data export report for import to VETA.", ProgrammingLanguage.CSharp, ((string[])(null)));
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
                        && (TechTalk.SpecFlow.FeatureContext.Current.FeatureInfo.Title != "ExportReportToVETA")))
            {
                ProductionDataSvc.AcceptanceTests.ExportReportToVETAFeature.FeatureSetup(null);
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
 testRunner.Given("the Export Report To VETA service URI \"/api/v2/export/veta\" and the result file \"" +
                    "ExportReportToVETAResponse.json\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
        }
        
        public virtual void ExportReportToVETA_GoodRequest(string requetsName, string projectUID, string startDate, string endDate, string machineNames, string fileName, string resultName, string[] exampleTags)
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("ExportReportToVETA - Good Request", exampleTags);
#line 7
this.ScenarioSetup(scenarioInfo);
#line 4
this.FeatureBackground();
#line 8
  testRunner.And(string.Format("projectUid \"{0}\"", projectUID), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 9
 testRunner.And(string.Format("startUtc \"{0}\" and endUtc \"{1}\"", startDate, endDate), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 10
 testRunner.And(string.Format("machineNames \"{0}\"", machineNames), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 11
 testRunner.And(string.Format("fileName is \"{0}\"", fileName), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 12
 testRunner.When("I request an Export Report To VETA", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 13
 testRunner.Then(string.Format("the report result should match the \"{0}\" from the repository", resultName), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("ExportReportToVETA - Good Request")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "ExportReportToVETA")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "Selected Machines")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequetsName", "Selected Machines")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUID", "7925f179-013d-4aaf-aff4-7b9833bb06d6")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:StartDate", "2012-11-05T00:00:00")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:EndDate", "2012-11-06T00:00:00")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:MachineNames", "D61 SATD PE,KOMATSU PC210,ACOM,LIEBHERR 924C,CAT CS56B,VOLVO G946B,CASE CX160C,LI" +
            "EBHERR724,JD 764 CV")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FileName", "Test")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "AllMachinesLongDates")]
        public virtual void ExportReportToVETA_GoodRequest_SelectedMachines()
        {
            this.ExportReportToVETA_GoodRequest("Selected Machines", "7925f179-013d-4aaf-aff4-7b9833bb06d6", "2012-11-05T00:00:00", "2012-11-06T00:00:00", "D61 SATD PE,KOMATSU PC210,ACOM,LIEBHERR 924C,CAT CS56B,VOLVO G946B,CASE CX160C,LI" +
                    "EBHERR724,JD 764 CV", "Test", "AllMachinesLongDates", ((string[])(null)));
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("ExportReportToVETA - Good Request")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "ExportReportToVETA")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "All Machines")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequetsName", "All Machines")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUID", "7925f179-013d-4aaf-aff4-7b9833bb06d6")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:StartDate", "2012-11-05T00:00:00")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:EndDate", "2012-11-06T00:00:00")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:MachineNames", "All")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FileName", "Test")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "AllMachinesLongDates")]
        public virtual void ExportReportToVETA_GoodRequest_AllMachines()
        {
            this.ExportReportToVETA_GoodRequest("All Machines", "7925f179-013d-4aaf-aff4-7b9833bb06d6", "2012-11-05T00:00:00", "2012-11-06T00:00:00", "All", "Test", "AllMachinesLongDates", ((string[])(null)));
        }
        
        public virtual void ExportReportToVETA_GoodRequest_NoMachines(string requetsName, string projectUID, string startDate, string endDate, string fileName, string resultName, string[] exampleTags)
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("ExportReportToVETA - Good Request - No Machines", exampleTags);
#line 19
this.ScenarioSetup(scenarioInfo);
#line 4
this.FeatureBackground();
#line 20
  testRunner.And(string.Format("projectUid \"{0}\"", projectUID), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 21
 testRunner.And(string.Format("startUtc \"{0}\" and endUtc \"{1}\"", startDate, endDate), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 22
 testRunner.And(string.Format("fileName is \"{0}\"", fileName), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 23
 testRunner.When("I request an Export Report To VETA", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 24
 testRunner.Then(string.Format("the report result should match the \"{0}\" from the repository", resultName), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("ExportReportToVETA - Good Request - No Machines")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "ExportReportToVETA")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequetsName", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUID", "7925f179-013d-4aaf-aff4-7b9833bb06d6")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:StartDate", "2012-11-05T00:00:00")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:EndDate", "2012-11-06T00:00:00")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FileName", "Test")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "NoMachinesLongDates")]
        public virtual void ExportReportToVETA_GoodRequest_NoMachines_()
        {
            this.ExportReportToVETA_GoodRequest_NoMachines("", "7925f179-013d-4aaf-aff4-7b9833bb06d6", "2012-11-05T00:00:00", "2012-11-06T00:00:00", "Test", "NoMachinesLongDates", ((string[])(null)));
        }
        
        public virtual void ExportReportToVETA_BadRequest_NoProjectUID(string requetsName, string startDate, string endDate, string machineNames, string fileName, string errorCode, string errorMessage, string[] exampleTags)
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("ExportReportToVETA - Bad Request - NoProjectUID", exampleTags);
#line 29
this.ScenarioSetup(scenarioInfo);
#line 4
this.FeatureBackground();
#line 30
 testRunner.And(string.Format("startUtc \"{0}\" and endUtc \"{1}\"", startDate, endDate), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 31
 testRunner.And(string.Format("machineNames \"{0}\"", machineNames), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 32
 testRunner.And(string.Format("fileName is \"{0}\"", fileName), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 33
 testRunner.When("I request an Export Report To VETA expecting Unauthorized", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 34
 testRunner.Then(string.Format("the report result should contain error code {0} and error message \"{1}\"", errorCode, errorMessage), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("ExportReportToVETA - Bad Request - NoProjectUID")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "ExportReportToVETA")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequetsName", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:StartDate", "2005-01-01")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:EndDate", "2017-06-23")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:MachineNames", "All")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FileName", "Test")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ErrorCode", "-5")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ErrorMessage", "Missing Project or project does not belong to specified customer or don\'t have ac" +
            "cess to the project")]
        public virtual void ExportReportToVETA_BadRequest_NoProjectUID_()
        {
            this.ExportReportToVETA_BadRequest_NoProjectUID("", "2005-01-01", "2017-06-23", "All", "Test", "-5", "Missing Project or project does not belong to specified customer or don\'t have ac" +
                    "cess to the project", ((string[])(null)));
        }
        
        public virtual void ExportReportToVETA_BadRequest_NoDateRange(string requetsName, string projectUID, string machineNames, string fileName, string errorCode, string errorMessage, string[] exampleTags)
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("ExportReportToVETA - Bad Request - NoDateRange", exampleTags);
#line 39
this.ScenarioSetup(scenarioInfo);
#line 4
this.FeatureBackground();
#line 40
 testRunner.And(string.Format("projectUid \"{0}\"", projectUID), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 41
 testRunner.And(string.Format("machineNames \"{0}\"", machineNames), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 42
 testRunner.And(string.Format("fileName is \"{0}\"", fileName), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 43
 testRunner.When("I request an Export Report To VETA expecting BadRequest", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 44
 testRunner.Then(string.Format("the report result should contain error code {0} and error message \"{1}\"", errorCode, errorMessage), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("ExportReportToVETA - Bad Request - NoDateRange")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "ExportReportToVETA")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequetsName", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUID", "7925f179-013d-4aaf-aff4-7b9833bb06d6")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:MachineNames", "All")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FileName", "Test")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ErrorCode", "-4")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ErrorMessage", "Failed to get requested export data")]
        public virtual void ExportReportToVETA_BadRequest_NoDateRange_()
        {
            this.ExportReportToVETA_BadRequest_NoDateRange("", "7925f179-013d-4aaf-aff4-7b9833bb06d6", "All", "Test", "-4", "Failed to get requested export data", ((string[])(null)));
        }
        
        public virtual void ExportReportToVETA_BadRequest_NoFileName(string requetsName, string projectUID, string startDate, string endDate, string machineNames, string errorCode, string errorMessage, string[] exampleTags)
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("ExportReportToVETA - Bad Request - NoFileName", exampleTags);
#line 49
this.ScenarioSetup(scenarioInfo);
#line 4
this.FeatureBackground();
#line 50
 testRunner.And(string.Format("projectUid \"{0}\"", projectUID), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 51
  testRunner.And(string.Format("startUtc \"{0}\" and endUtc \"{1}\"", startDate, endDate), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 52
 testRunner.And(string.Format("machineNames \"{0}\"", machineNames), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 53
 testRunner.When("I request an Export Report To VETA expecting BadRequest", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 54
 testRunner.Then(string.Format("the report result should contain error code {0} and error message \"{1}\"", errorCode, errorMessage), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("ExportReportToVETA - Bad Request - NoFileName")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "ExportReportToVETA")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequetsName", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUID", "7925f179-013d-4aaf-aff4-7b9833bb06d6")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:StartDate", "2005-01-01")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:EndDate", "2017-06-23")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:MachineNames", "All")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ErrorCode", "-4")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ErrorMessage", "Failed to get requested export data")]
        public virtual void ExportReportToVETA_BadRequest_NoFileName_()
        {
            this.ExportReportToVETA_BadRequest_NoFileName("", "7925f179-013d-4aaf-aff4-7b9833bb06d6", "2005-01-01", "2017-06-23", "All", "-4", "Failed to get requested export data", ((string[])(null)));
        }
    }
}
#pragma warning restore
#endregion
