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
    public partial class CompactionCellDatumFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "CompactionCellDatum.feature"
#line hidden
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute()]
        public static void FeatureSetup(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext testContext)
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "CompactionCellDatum", "I should be able to request compaction Cell Datum.", ProgrammingLanguage.CSharp, ((string[])(null)));
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
                        && (TechTalk.SpecFlow.FeatureContext.Current.FeatureInfo.Title != "CompactionCellDatum")))
            {
                ProductionDataSvc.AcceptanceTests.CompactionCellDatumFeature.FeatureSetup(null);
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
 testRunner.Given("the CompactionCellDatum service URI \"/api/v2/productiondata/cells/datum\" and resu" +
                    "lt repo \"CompactionCellDatumResponse.json\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
        }
        
        public virtual void CompactionCellDatum_GoodRequest(string requestName, string projectUID, string filterUID, string cutFillDesignUID, string displayMode, string latitude, string longitude, string resultName, string[] exampleTags)
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("CompactionCellDatum - Good Request", exampleTags);
#line 7
this.ScenarioSetup(scenarioInfo);
#line 4
this.FeatureBackground();
#line 8
testRunner.And(string.Format("projectUid \"{0}\"", projectUID), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 9
testRunner.And(string.Format("filterUid \"{0}\"", filterUID), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 10
testRunner.And(string.Format("cutfillDesignUid \"{0}\"", cutFillDesignUID), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 11
testRunner.And(string.Format("displayMode \"{0}\"", displayMode), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 12
testRunner.And(string.Format("lat \"{0}\"", latitude), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 13
testRunner.And(string.Format("lon \"{0}\"", longitude), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 14
testRunner.When("I request Compaction Cell Datum", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 15
testRunner.Then(string.Format("the Compaction Cell Datum response should match \"{0}\" result from the repository", resultName), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("CompactionCellDatum - Good Request")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CompactionCellDatum")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "Height")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "Height")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUID", "ff91dd40-1569-4765-a2bc-014321f76ace")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUID", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:CutFillDesignUID", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:DisplayMode", "0")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Latitude", "36.20696541")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Longitude", "-115.02021047")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "Height")]
        public virtual void CompactionCellDatum_GoodRequest_Height()
        {
            this.CompactionCellDatum_GoodRequest("Height", "ff91dd40-1569-4765-a2bc-014321f76ace", "", "", "0", "36.20696541", "-115.02021047", "Height", ((string[])(null)));
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("CompactionCellDatum - Good Request")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CompactionCellDatum")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "CMV")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "CMV")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUID", "ff91dd40-1569-4765-a2bc-014321f76ace")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUID", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:CutFillDesignUID", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:DisplayMode", "1")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Latitude", "36.20696541")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Longitude", "-115.02021047")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "CMV")]
        public virtual void CompactionCellDatum_GoodRequest_CMV()
        {
            this.CompactionCellDatum_GoodRequest("CMV", "ff91dd40-1569-4765-a2bc-014321f76ace", "", "", "1", "36.20696541", "-115.02021047", "CMV", ((string[])(null)));
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("CompactionCellDatum - Good Request")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CompactionCellDatum")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "CMVPercent")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "CMVPercent")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUID", "ff91dd40-1569-4765-a2bc-014321f76ace")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUID", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:CutFillDesignUID", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:DisplayMode", "2")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Latitude", "36.20696541")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Longitude", "-115.02021047")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "CMVPercent")]
        public virtual void CompactionCellDatum_GoodRequest_CMVPercent()
        {
            this.CompactionCellDatum_GoodRequest("CMVPercent", "ff91dd40-1569-4765-a2bc-014321f76ace", "", "", "2", "36.20696541", "-115.02021047", "CMVPercent", ((string[])(null)));
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("CompactionCellDatum - Good Request")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CompactionCellDatum")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "PassCount")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "PassCount")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUID", "ff91dd40-1569-4765-a2bc-014321f76ace")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUID", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:CutFillDesignUID", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:DisplayMode", "4")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Latitude", "36.20696541")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Longitude", "-115.02021047")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "PassCount")]
        public virtual void CompactionCellDatum_GoodRequest_PassCount()
        {
            this.CompactionCellDatum_GoodRequest("PassCount", "ff91dd40-1569-4765-a2bc-014321f76ace", "", "", "4", "36.20696541", "-115.02021047", "PassCount", ((string[])(null)));
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("CompactionCellDatum - Good Request")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CompactionCellDatum")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "PassCount2")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "PassCount2")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUID", "ff91dd40-1569-4765-a2bc-014321f76ace")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUID", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:CutFillDesignUID", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:DisplayMode", "4")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Latitude", "36.207105")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Longitude", "-115.018953")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "PassCount2")]
        public virtual void CompactionCellDatum_GoodRequest_PassCount2()
        {
            this.CompactionCellDatum_GoodRequest("PassCount2", "ff91dd40-1569-4765-a2bc-014321f76ace", "", "", "4", "36.207105", "-115.018953", "PassCount2", ((string[])(null)));
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("CompactionCellDatum - Good Request")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CompactionCellDatum")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "CutFill")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "CutFill")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUID", "ff91dd40-1569-4765-a2bc-014321f76ace")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUID", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:CutFillDesignUID", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:DisplayMode", "8")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Latitude", "36.20735707")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Longitude", "-115.01959313")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "CutFill")]
        public virtual void CompactionCellDatum_GoodRequest_CutFill()
        {
            this.CompactionCellDatum_GoodRequest("CutFill", "ff91dd40-1569-4765-a2bc-014321f76ace", "", "", "8", "36.20735707", "-115.01959313", "CutFill", ((string[])(null)));
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("CompactionCellDatum - Good Request")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CompactionCellDatum")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "Temperature")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "Temperature")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUID", "ff91dd40-1569-4765-a2bc-014321f76ace")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUID", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:CutFillDesignUID", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:DisplayMode", "10")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Latitude", "36.20696541")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Longitude", "-115.02021047")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "Temperature")]
        public virtual void CompactionCellDatum_GoodRequest_Temperature()
        {
            this.CompactionCellDatum_GoodRequest("Temperature", "ff91dd40-1569-4765-a2bc-014321f76ace", "", "", "10", "36.20696541", "-115.02021047", "Temperature", ((string[])(null)));
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("CompactionCellDatum - Good Request")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CompactionCellDatum")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "MDP")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "MDP")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUID", "ff91dd40-1569-4765-a2bc-014321f76ace")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUID", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:CutFillDesignUID", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:DisplayMode", "17")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Latitude", "36.207499")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Longitude", "-115.018843")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "MDP")]
        public virtual void CompactionCellDatum_GoodRequest_MDP()
        {
            this.CompactionCellDatum_GoodRequest("MDP", "ff91dd40-1569-4765-a2bc-014321f76ace", "", "", "17", "36.207499", "-115.018843", "MDP", ((string[])(null)));
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("CompactionCellDatum - Good Request")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CompactionCellDatum")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "MDPBoundary")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "MDPBoundary")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUID", "ff91dd40-1569-4765-a2bc-014321f76ace")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUID", "3ef41e3c-d1f5-40cd-b012-99d11ff432ef")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:CutFillDesignUID", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:DisplayMode", "17")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Latitude", "36.207499")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Longitude", "-115.018843")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "MDP2")]
        public virtual void CompactionCellDatum_GoodRequest_MDPBoundary()
        {
            this.CompactionCellDatum_GoodRequest("MDPBoundary", "ff91dd40-1569-4765-a2bc-014321f76ace", "3ef41e3c-d1f5-40cd-b012-99d11ff432ef", "", "17", "36.207499", "-115.018843", "MDP2", ((string[])(null)));
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("CompactionCellDatum - Good Request")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CompactionCellDatum")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "MachineSpeed")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "MachineSpeed")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUID", "ff91dd40-1569-4765-a2bc-014321f76ace")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUID", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:CutFillDesignUID", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:DisplayMode", "23")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Latitude", "36.20696541")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Longitude", "-115.02021047")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "MachineSpeed")]
        public virtual void CompactionCellDatum_GoodRequest_MachineSpeed()
        {
            this.CompactionCellDatum_GoodRequest("MachineSpeed", "ff91dd40-1569-4765-a2bc-014321f76ace", "", "", "23", "36.20696541", "-115.02021047", "MachineSpeed", ((string[])(null)));
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("CompactionCellDatum - Good Request")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CompactionCellDatum")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "CMVPercentChange")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "CMVPercentChange")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUID", "ff91dd40-1569-4765-a2bc-014321f76ace")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUID", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:CutFillDesignUID", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:DisplayMode", "27")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Latitude", "36.206958")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Longitude", "-115.020144")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "CMVPercentChange")]
        public virtual void CompactionCellDatum_GoodRequest_CMVPercentChange()
        {
            this.CompactionCellDatum_GoodRequest("CMVPercentChange", "ff91dd40-1569-4765-a2bc-014321f76ace", "", "", "27", "36.206958", "-115.020144", "CMVPercentChange", ((string[])(null)));
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("CompactionCellDatum - Good Request")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CompactionCellDatum")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "HeightOutsideFilter")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "HeightOutsideFilter")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUID", "ff91dd40-1569-4765-a2bc-014321f76ace")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUID", "154470b6-15ae-4cca-b281-eae8ac1efa6c")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:CutFillDesignUID", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:DisplayMode", "0")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Latitude", "36.20696541")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Longitude", "-115.02021047")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "HeightOutsideFilter")]
        public virtual void CompactionCellDatum_GoodRequest_HeightOutsideFilter()
        {
            this.CompactionCellDatum_GoodRequest("HeightOutsideFilter", "ff91dd40-1569-4765-a2bc-014321f76ace", "154470b6-15ae-4cca-b281-eae8ac1efa6c", "", "0", "36.20696541", "-115.02021047", "HeightOutsideFilter", ((string[])(null)));
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("CompactionCellDatum - Good Request")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CompactionCellDatum")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "CutFillWithDesign")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "CutFillWithDesign")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUID", "ff91dd40-1569-4765-a2bc-014321f76ace")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUID", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:CutFillDesignUID", "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:DisplayMode", "8")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Latitude", "36.20735707")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Longitude", "-115.01959313")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "CutFillWithDesign")]
        public virtual void CompactionCellDatum_GoodRequest_CutFillWithDesign()
        {
            this.CompactionCellDatum_GoodRequest("CutFillWithDesign", "ff91dd40-1569-4765-a2bc-014321f76ace", "", "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff", "8", "36.20735707", "-115.01959313", "CutFillWithDesign", ((string[])(null)));
        }
        
        public virtual void CompactionCellDatum_BadRequest(string requestName, string projectUID, string filterUID, string cutFillDesignUID, string displayMode, string latitude, string longitude, string httpCode, string errorCode, string[] exampleTags)
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("CompactionCellDatum - Bad Request", exampleTags);
#line 32
this.ScenarioSetup(scenarioInfo);
#line 4
this.FeatureBackground();
#line 33
testRunner.And(string.Format("projectUid \"{0}\"", projectUID), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 34
testRunner.And(string.Format("filterUid \"{0}\"", filterUID), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 35
testRunner.And(string.Format("cutfillDesignUid \"{0}\"", cutFillDesignUID), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 36
testRunner.And(string.Format("displayMode \"{0}\"", displayMode), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 37
testRunner.And(string.Format("lat \"{0}\"", latitude), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 38
testRunner.And(string.Format("lon \"{0}\"", longitude), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 39
testRunner.When(string.Format("I request Compaction Cell Datum I expect http error code {0}", httpCode), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 40
testRunner.Then(string.Format("the response should contain error code {0}", errorCode), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("CompactionCellDatum - Bad Request")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CompactionCellDatum")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "NoProjectUID")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "NoProjectUID")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUID", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUID", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:CutFillDesignUID", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:DisplayMode", "0")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Latitude", "36.20696541")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Longitude", "-115.02021047")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:httpCode", "400")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:errorCode", "-1")]
        public virtual void CompactionCellDatum_BadRequest_NoProjectUID()
        {
            this.CompactionCellDatum_BadRequest("NoProjectUID", "", "", "", "0", "36.20696541", "-115.02021047", "400", "-1", ((string[])(null)));
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("CompactionCellDatum - Bad Request")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CompactionCellDatum")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "NoFoundDesign")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "NoFoundDesign")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUID", "ff91dd40-1569-4765-a2bc-014321f76ace")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUID", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:CutFillDesignUID", "3d255208-8aa2-4172-9046-f97a36eff896")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:DisplayMode", "8")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Latitude", "36.20696541")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Longitude", "-115.02021047")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:httpCode", "400")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:errorCode", "-1")]
        public virtual void CompactionCellDatum_BadRequest_NoFoundDesign()
        {
            this.CompactionCellDatum_BadRequest("NoFoundDesign", "ff91dd40-1569-4765-a2bc-014321f76ace", "", "3d255208-8aa2-4172-9046-f97a36eff896", "8", "36.20696541", "-115.02021047", "400", "-1", ((string[])(null)));
        }
    }
}
#pragma warning restore
#endregion
