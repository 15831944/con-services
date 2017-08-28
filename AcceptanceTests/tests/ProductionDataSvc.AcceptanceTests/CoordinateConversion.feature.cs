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
    public partial class CoordinateConversionFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "CoordinateConversion.feature"
#line hidden
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute()]
        public static void FeatureSetup(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext testContext)
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "CoordinateConversion", "I should be able to convert coordinates from LL to NE or vice versa.", ProgrammingLanguage.CSharp, ((string[])(null)));
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
                        && (TechTalk.SpecFlow.FeatureContext.Current.FeatureInfo.Title != "CoordinateConversion")))
            {
                ProductionDataSvc.AcceptanceTests.CoordinateConversionFeature.FeatureSetup(null);
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
 testRunner.Given("the Coordinate Conversion service URI \"/api/v1/coordinateconversion\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("CoordinateConversion - LL to NE")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CoordinateConversion")]
        public virtual void CoordinateConversion_LLToNE()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("CoordinateConversion - LL to NE", ((string[])(null)));
#line 7
this.ScenarioSetup(scenarioInfo);
#line 4
this.FeatureBackground();
#line 8
 testRunner.Given("a project id 1001158", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 9
 testRunner.And("the coordinate conversion type \"LatLonToNorthEast\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "x",
                        "y"});
            table1.AddRow(new string[] {
                        "-2.007453062",
                        "0.631935272"});
            table1.AddRow(new string[] {
                        "-2.007483867",
                        "0.631929809"});
#line 10
 testRunner.And("these coordinates", ((string)(null)), table1, "And ");
#line 14
 testRunner.When("I request the coordinate conversion", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "x",
                        "y"});
            table2.AddRow(new string[] {
                        "2884.667",
                        "1193.966"});
            table2.AddRow(new string[] {
                        "2725.931",
                        "1159.225"});
#line 15
 testRunner.Then("the result should be these", ((string)(null)), table2, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("CoordinateConversion - NE to LL")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CoordinateConversion")]
        public virtual void CoordinateConversion_NEToLL()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("CoordinateConversion - NE to LL", ((string[])(null)));
#line 20
this.ScenarioSetup(scenarioInfo);
#line 4
this.FeatureBackground();
#line 21
 testRunner.Given("a project id 1001158", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 22
  testRunner.And("the coordinate conversion type \"NorthEastToLatLon\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                        "x",
                        "y"});
            table3.AddRow(new string[] {
                        "2884.667",
                        "1193.966"});
            table3.AddRow(new string[] {
                        "2725.931",
                        "1159.225"});
#line 23
  testRunner.And("these coordinates", ((string)(null)), table3, "And ");
#line 27
 testRunner.When("I request the coordinate conversion", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table4 = new TechTalk.SpecFlow.Table(new string[] {
                        "x",
                        "y"});
            table4.AddRow(new string[] {
                        "-2.007453062",
                        "0.631935272"});
            table4.AddRow(new string[] {
                        "-2.007483867",
                        "0.631929809"});
#line 28
 testRunner.Then("the result should be these", ((string)(null)), table4, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        public virtual void CoordinateConversion_BadRequestBadLL(string httpCode, string errorCode, string[] exampleTags)
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("CoordinateConversion - Bad Request (Bad LL)", exampleTags);
#line 33
this.ScenarioSetup(scenarioInfo);
#line 4
this.FeatureBackground();
#line 34
 testRunner.Given("a project id 1001158", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 35
  testRunner.And("the coordinate conversion type \"LatLonToNorthEast\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table5 = new TechTalk.SpecFlow.Table(new string[] {
                        "x",
                        "y"});
            table5.AddRow(new string[] {
                        "-5.007453062",
                        "0.631935272"});
            table5.AddRow(new string[] {
                        "-2.007483867",
                        "5.631929809"});
#line 36
  testRunner.And("these coordinates", ((string)(null)), table5, "And ");
#line 40
 testRunner.When(string.Format("I request the coordinate conversion expecting {0}", httpCode), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 41
 testRunner.Then(string.Format("the response should contain error code {0}", errorCode), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("CoordinateConversion - Bad Request (Bad LL)")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CoordinateConversion")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "400")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:httpCode", "400")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:errorCode", "-1")]
        public virtual void CoordinateConversion_BadRequestBadLL_400()
        {
            this.CoordinateConversion_BadRequestBadLL("400", "-1", ((string[])(null)));
        }
    }
}
#pragma warning restore
#endregion
