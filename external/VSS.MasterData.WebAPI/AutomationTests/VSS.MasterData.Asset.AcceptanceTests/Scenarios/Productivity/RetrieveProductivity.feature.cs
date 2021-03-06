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
namespace VSS.MasterData.Asset.AcceptanceTests.Scenarios.Productivity
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "1.9.0.77")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute()]
    public partial class RetrieveProductivityFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "RetrieveProductivity.feature"
#line hidden
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute()]
        public static void FeatureSetup(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext testContext)
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "RetrieveProductivity", "\r\nUser Story: 44878  Implementation - VisionLink Administrator - Productivity Tar" +
                    "gets\r\n\r\n------------------------------------------------------------------------" +
                    "-----------------------", ProgrammingLanguage.CSharp, ((string[])(null)));
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
                        && (TechTalk.SpecFlow.FeatureContext.Current.FeatureInfo.Title != "RetrieveProductivity")))
            {
                VSS.MasterData.Asset.AcceptanceTests.Scenarios.Productivity.RetrieveProductivityFeature.FeatureSetup(null);
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
        
        public virtual void RetrieveProductivityDetails(string description, string startDate, string endDate, string[] exampleTags)
        {
            string[] @__tags = new string[] {
                    "US44878",
                    "Automated",
                    "RetrieveProductivity",
                    "Positive"};
            if ((exampleTags != null))
            {
                @__tags = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Concat(@__tags, exampleTags));
            }
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("RetrieveProductivityDetails", @__tags);
#line 8
this.ScenarioSetup(scenarioInfo);
#line 9
testRunner.Given(string.Format("\'{0}\' is ready to verify", description), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 10
testRunner.And("Retrieve Productivity Details is setup with default valid values", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 11
testRunner.And(string.Format("I set startDate as \'{0}\' and EndDate as \'{1}\'", startDate, endDate), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 12
testRunner.When("I retrieve Productivity Details", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 13
testRunner.Then("Valid response should be received", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("RetrieveProductivityDetails")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "RetrieveProductivity")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("US44878")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Automated")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("RetrieveProductivity")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Positive")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "HappyPath")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Description", "HappyPath")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:StartDate", "1-1-2017")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:EndDate", "5-1-2017")]
        public virtual void RetrieveProductivityDetails_HappyPath()
        {
            this.RetrieveProductivityDetails("HappyPath", "1-1-2017", "5-1-2017", ((string[])(null)));
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("RetrieveProductivityDetails")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "RetrieveProductivity")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("US44878")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Automated")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("RetrieveProductivity")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Positive")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "NoAssetDetails")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Description", "NoAssetDetails")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:StartDate", "2-1-2018")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:EndDate", "2-1-2018")]
        public virtual void RetrieveProductivityDetails_NoAssetDetails()
        {
            this.RetrieveProductivityDetails("NoAssetDetails", "2-1-2018", "2-1-2018", ((string[])(null)));
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("RetrieveProductivityDetails")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "RetrieveProductivity")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("US44878")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Automated")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("RetrieveProductivity")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Positive")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "StartDateAndEndDateSame")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Description", "StartDateAndEndDateSame")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:StartDate", "2-1-2017")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:EndDate", "2-1-2017")]
        public virtual void RetrieveProductivityDetails_StartDateAndEndDateSame()
        {
            this.RetrieveProductivityDetails("StartDateAndEndDateSame", "2-1-2017", "2-1-2017", ((string[])(null)));
        }
        
        public virtual void RetrieveProductivityDetails_NoAssetDetails(string description, string assetUID, string[] exampleTags)
        {
            string[] @__tags = new string[] {
                    "US44878",
                    "Automated",
                    "RetrieveProductivity",
                    "Negative"};
            if ((exampleTags != null))
            {
                @__tags = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Concat(@__tags, exampleTags));
            }
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("RetrieveProductivityDetails_NoAssetDetails", @__tags);
#line 22
this.ScenarioSetup(scenarioInfo);
#line 23
testRunner.Given(string.Format("\'{0}\' is ready to verify", description), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 24
testRunner.And("Retrieve Productivity Details is setup with default valid values", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 25
testRunner.And("I modify  AssetUID as <\'assetUID\'>", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 26
testRunner.When("I retrieve Productivity Details", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 27
testRunner.Then("Valid  Error response should be received", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("RetrieveProductivityDetails_NoAssetDetails")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "RetrieveProductivity")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("US44878")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Automated")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("RetrieveProductivity")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Negative")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "assetUID_Null")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Description", "assetUID_Null")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:assetUID", "NULL_NULL")]
        public virtual void RetrieveProductivityDetails_NoAssetDetails_AssetUID_Null()
        {
            this.RetrieveProductivityDetails_NoAssetDetails("assetUID_Null", "NULL_NULL", ((string[])(null)));
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("RetrieveProductivityDetails_NoAssetDetails")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "RetrieveProductivity")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("US44878")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Automated")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("RetrieveProductivity")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Negative")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "assetUID_EmptySpace")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Description", "assetUID_EmptySpace")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:assetUID", "EMPTY_EMPTY")]
        public virtual void RetrieveProductivityDetails_NoAssetDetails_AssetUID_EmptySpace()
        {
            this.RetrieveProductivityDetails_NoAssetDetails("assetUID_EmptySpace", "EMPTY_EMPTY", ((string[])(null)));
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("RetrieveProductivityDetails_NoAssetDetails")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "RetrieveProductivity")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("US44878")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Automated")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("RetrieveProductivity")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Negative")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "assetUID_InvalidString")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Description", "assetUID_InvalidString")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:assetUID", "InvalidString")]
        public virtual void RetrieveProductivityDetails_NoAssetDetails_AssetUID_InvalidString()
        {
            this.RetrieveProductivityDetails_NoAssetDetails("assetUID_InvalidString", "InvalidString", ((string[])(null)));
        }
        
        public virtual void RetrieveProductivityDetails_InvalidDateRange(string description, string startDate, string endDate, string[] exampleTags)
        {
            string[] @__tags = new string[] {
                    "US44878",
                    "Automated",
                    "RetrieveProductivity",
                    "Negative"};
            if ((exampleTags != null))
            {
                @__tags = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Concat(@__tags, exampleTags));
            }
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("RetrieveProductivityDetails_InvalidDateRange", @__tags);
#line 36
this.ScenarioSetup(scenarioInfo);
#line 37
testRunner.Given(string.Format("\'{0}\' is ready to verify", description), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 38
testRunner.And("Retrieve Asset Details is setup with default valid values", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 39
testRunner.And(string.Format("I set startDate as \'{0}\' and EndDate as \'{1}\'", startDate, endDate), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 40
testRunner.When("I retrieve Asset Details", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 41
testRunner.Then("Valid Error response should be received", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("RetrieveProductivityDetails_InvalidDateRange")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "RetrieveProductivity")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("US44878")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Automated")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("RetrieveProductivity")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Negative")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "EndDateGreaterThanStartDate")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Description", "EndDateGreaterThanStartDate")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:StartDate", "5-1-2017")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:EndDate", "1-1-2017")]
        public virtual void RetrieveProductivityDetails_InvalidDateRange_EndDateGreaterThanStartDate()
        {
            this.RetrieveProductivityDetails_InvalidDateRange("EndDateGreaterThanStartDate", "5-1-2017", "1-1-2017", ((string[])(null)));
        }
        
        public virtual void AddProductivity_MultipleOverlap(string description, string addAssetStartDate, string addAssetEndDate, string updateAssetStartDate, string updateAssetEndDate, string multipleOverlapStartDate, string multipleOverlapEndDate, string retrieveStartDate, string retrieveEndDate, string[] exampleTags)
        {
            string[] @__tags = new string[] {
                    "US44878",
                    "Automated",
                    "AddProductivity",
                    "Positive"};
            if ((exampleTags != null))
            {
                @__tags = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Concat(@__tags, exampleTags));
            }
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("AddProductivity_MultipleOverlap", @__tags);
#line 48
this.ScenarioSetup(scenarioInfo);
#line 49
testRunner.Given(string.Format("\'{0}\' is ready to verify", description), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 50
testRunner.And("AddProductivity is setup with default valid values", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 51
testRunner.And(string.Format("I modify  startdate as \'{0}\' and EndDate as \'{1}\'", addAssetStartDate, addAssetEndDate), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 52
testRunner.And("I Put Valid Productivity details for asset", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 53
testRunner.And(string.Format("I modify  startdate as \'{0}\' and EndDate as \'{1}\'", updateAssetStartDate, updateAssetEndDate), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 54
testRunner.And("I Put Valid Productivity details for asset", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 55
testRunner.And(string.Format("I modify  startdate as \'{0}\' and EndDate as \'{1}\'", multipleOverlapStartDate, multipleOverlapEndDate), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 56
testRunner.And("I Put Valid Productivity details for asset", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 57
testRunner.When(string.Format("I try to retrieve Productivity details With Start Date as\'{0}\' and RetrieveEndDat" +
                        "e as \'{1}\'", retrieveStartDate, retrieveEndDate), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 58
testRunner.Then("Updated Productivity details should be shown", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("AddProductivity_MultipleOverlap")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "RetrieveProductivity")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("US44878")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Automated")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("AddProductivity")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Positive")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "MultipleOverlap_RetrieveStart")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Description", "MultipleOverlap_RetrieveStart")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:AddAssetStartDate", "2017-1-5")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:AddAssetEndDate", "2017-1-10")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:UpdateAssetStartDate", "2017-1-10")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:UpdateAssetEndDate", "2017-1-15")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:MultipleOverlapStartDate", "2017-1-5")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:MultipleOverlapEndDate", "2017-1-15")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RetrieveStartDate", "2017-1-1")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RetrieveEndDate", "2017-1-5")]
        public virtual void AddProductivity_MultipleOverlap_MultipleOverlap_RetrieveStart()
        {
            this.AddProductivity_MultipleOverlap("MultipleOverlap_RetrieveStart", "2017-1-5", "2017-1-10", "2017-1-10", "2017-1-15", "2017-1-5", "2017-1-15", "2017-1-1", "2017-1-5", ((string[])(null)));
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("AddProductivity_MultipleOverlap")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "RetrieveProductivity")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("US44878")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Automated")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("AddProductivity")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Positive")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "MultipleOverlap_RetrieveEnd")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Description", "MultipleOverlap_RetrieveEnd")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:AddAssetStartDate", "2017-1-5")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:AddAssetEndDate", "2017-1-10")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:UpdateAssetStartDate", "2017-1-10")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:UpdateAssetEndDate", "2017-1-15")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:MultipleOverlapStartDate", "2017-1-5")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:MultipleOverlapEndDate", "2017-1-15")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RetrieveStartDate", "2017-1-5")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RetrieveEndDate", "2017-1-10")]
        public virtual void AddProductivity_MultipleOverlap_MultipleOverlap_RetrieveEnd()
        {
            this.AddProductivity_MultipleOverlap("MultipleOverlap_RetrieveEnd", "2017-1-5", "2017-1-10", "2017-1-10", "2017-1-15", "2017-1-5", "2017-1-15", "2017-1-5", "2017-1-10", ((string[])(null)));
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("AddProductivity_MultipleOverlap")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "RetrieveProductivity")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("US44878")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Automated")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("AddProductivity")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Positive")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "MultipleOverlap_RetrieveMiddle")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Description", "MultipleOverlap_RetrieveMiddle")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:AddAssetStartDate", "2017-1-5")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:AddAssetEndDate", "2017-1-10")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:UpdateAssetStartDate", "2017-1-10")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:UpdateAssetEndDate", "2017-1-15")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:MultipleOverlapStartDate", "2017-1-5")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:MultipleOverlapEndDate", "2017-1-15")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RetrieveStartDate", "2017-1-11")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RetrieveEndDate", "2017-1-15")]
        public virtual void AddProductivity_MultipleOverlap_MultipleOverlap_RetrieveMiddle()
        {
            this.AddProductivity_MultipleOverlap("MultipleOverlap_RetrieveMiddle", "2017-1-5", "2017-1-10", "2017-1-10", "2017-1-15", "2017-1-5", "2017-1-15", "2017-1-11", "2017-1-15", ((string[])(null)));
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("AddProductivity_MultipleOverlap")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "RetrieveProductivity")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("US44878")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Automated")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("AddProductivity")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("Positive")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "MultipleOverlap_RetrieveEntireDateRange")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Description", "MultipleOverlap_RetrieveEntireDateRange")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:AddAssetStartDate", "2017-1-5")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:AddAssetEndDate", "2017-1-10")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:UpdateAssetStartDate", "2017-1-10")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:UpdateAssetEndDate", "2017-1-15")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:MultipleOverlapStartDate", "2017-1-5")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:MultipleOverlapEndDate", "2017-1-15")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RetrieveStartDate", "2017-1-5")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RetrieveEndDate", "2017-1-15")]
        public virtual void AddProductivity_MultipleOverlap_MultipleOverlap_RetrieveEntireDateRange()
        {
            this.AddProductivity_MultipleOverlap("MultipleOverlap_RetrieveEntireDateRange", "2017-1-5", "2017-1-10", "2017-1-10", "2017-1-15", "2017-1-5", "2017-1-15", "2017-1-5", "2017-1-15", ((string[])(null)));
        }
    }
}
#pragma warning restore
#endregion

