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
    public partial class CompactionSummaryVolumesFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "CompactionSummaryVolumes.feature"
#line hidden
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute()]
        public static void FeatureSetup(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext testContext)
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "CompactionSummaryVolumes", "I should be able to request Summary Volumes.", ProgrammingLanguage.CSharp, ((string[])(null)));
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
                        && (TechTalk.SpecFlow.FeatureContext.Current.FeatureInfo.Title != "CompactionSummaryVolumes")))
            {
                ProductionDataSvc.AcceptanceTests.CompactionSummaryVolumesFeature.FeatureSetup(null);
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
        
        public virtual void CompactionGetSummaryVolumes(string requestName, string projectUid, string designUid, string filterUid, string filterUid2, string resultName, string httpCode, string[] exampleTags)
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Compaction Get Summary volumes", exampleTags);
#line 5
this.ScenarioSetup(scenarioInfo);
#line 6
testRunner.Given("the Compaction service URI \"/api/v2/volumes/summary\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 7
testRunner.And("the result file \"CompactionSummaryVolumeResponse.json\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 8
testRunner.And(string.Format("project \"{0}\"", projectUid), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 9
testRunner.And(string.Format("filter \"{0}\"", filterUid), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 10
testRunner.And(string.Format("design \"{0}\"", designUid), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 11
testRunner.And(string.Format("to filter \"{0}\"", filterUid2), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 12
testRunner.When(string.Format("I request result \"{0}\"", httpCode), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 13
testRunner.Then(string.Format("the result should match the \"{0}\" from the repository", resultName), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Compaction Get Summary volumes")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CompactionSummaryVolumes")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "SimpleVolumeSummary")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "SimpleVolumeSummary")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUid", "ff91dd40-1569-4765-a2bc-014321f76ace")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:DesignUid", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUid", "f07ed071-f8a1-42c3-804a-1bde7a78be5b")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUid2", "f07ed071-f8a1-42c3-804a-1bde7a78be5b")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "SimpleVolumeSummary")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:httpCode", "200")]
        public virtual void CompactionGetSummaryVolumes_SimpleVolumeSummary()
        {
            this.CompactionGetSummaryVolumes("SimpleVolumeSummary", "ff91dd40-1569-4765-a2bc-014321f76ace", "", "f07ed071-f8a1-42c3-804a-1bde7a78be5b", "f07ed071-f8a1-42c3-804a-1bde7a78be5b", "SimpleVolumeSummary", "200", ((string[])(null)));
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Compaction Get Summary volumes")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CompactionSummaryVolumes")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "GroundToGround")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "GroundToGround")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUid", "ff91dd40-1569-4765-a2bc-014321f76ace")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:DesignUid", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUid", "a37f3008-65e5-44a8-b406-9a078ec62ece")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUid2", "a37f3008-65e5-44a8-b406-9a078ec62ece")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "GroundToGround")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:httpCode", "200")]
        public virtual void CompactionGetSummaryVolumes_GroundToGround()
        {
            this.CompactionGetSummaryVolumes("GroundToGround", "ff91dd40-1569-4765-a2bc-014321f76ace", "", "a37f3008-65e5-44a8-b406-9a078ec62ece", "a37f3008-65e5-44a8-b406-9a078ec62ece", "GroundToGround", "200", ((string[])(null)));
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Compaction Get Summary volumes")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CompactionSummaryVolumes")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "SummaryVolumesFilterNull20121101")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "SummaryVolumesFilterNull20121101")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUid", "ff91dd40-1569-4765-a2bc-014321f76ace")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:DesignUid", "e2c7381d-1a2e-4dc7-8c0e-45df2f92ba0e")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUid", "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUid2", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "SummaryVolumesFilterNull20121101")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:httpCode", "200")]
        public virtual void CompactionGetSummaryVolumes_SummaryVolumesFilterNull20121101()
        {
            this.CompactionGetSummaryVolumes("SummaryVolumesFilterNull20121101", "ff91dd40-1569-4765-a2bc-014321f76ace", "e2c7381d-1a2e-4dc7-8c0e-45df2f92ba0e", "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff", "", "SummaryVolumesFilterNull20121101", "200", ((string[])(null)));
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Compaction Get Summary volumes")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CompactionSummaryVolumes")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "GroundToGroundNoData")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "GroundToGroundNoData")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUid", "ff91dd40-1569-4765-a2bc-014321f76ace")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:DesignUid", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUid", "a54e5945-1aaa-4921-9cc1-c9d8c0a343d3")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUid2", "a54e5945-1aaa-4921-9cc1-c9d8c0a343d3")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "EmptyJsonResponse")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:httpCode", "200")]
        public virtual void CompactionGetSummaryVolumes_GroundToGroundNoData()
        {
            this.CompactionGetSummaryVolumes("GroundToGroundNoData", "ff91dd40-1569-4765-a2bc-014321f76ace", "", "a54e5945-1aaa-4921-9cc1-c9d8c0a343d3", "a54e5945-1aaa-4921-9cc1-c9d8c0a343d3", "EmptyJsonResponse", "200", ((string[])(null)));
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Compaction Get Summary volumes")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CompactionSummaryVolumes")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "GroundToGroundNullUid")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "GroundToGroundNullUid")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUid", "ff91dd40-1569-4765-a2bc-014321f76ace")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:DesignUid", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUid", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUid2", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "FilterAndInvalidDesign")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:httpCode", "400")]
        public virtual void CompactionGetSummaryVolumes_GroundToGroundNullUid()
        {
            this.CompactionGetSummaryVolumes("GroundToGroundNullUid", "ff91dd40-1569-4765-a2bc-014321f76ace", "", "", "", "FilterAndInvalidDesign", "400", ((string[])(null)));
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Compaction Get Summary volumes")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CompactionSummaryVolumes")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "FilterAndDesign")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "FilterAndDesign")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUid", "ff91dd40-1569-4765-a2bc-014321f76ace")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:DesignUid", "ea89be4b-0efb-4b8f-ba33-03f0973bfc7b")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUid", "f07ed071-f8a1-42c3-804a-1bde7a78be5b")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUid2", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "FilterAndDesign")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:httpCode", "200")]
        public virtual void CompactionGetSummaryVolumes_FilterAndDesign()
        {
            this.CompactionGetSummaryVolumes("FilterAndDesign", "ff91dd40-1569-4765-a2bc-014321f76ace", "ea89be4b-0efb-4b8f-ba33-03f0973bfc7b", "f07ed071-f8a1-42c3-804a-1bde7a78be5b", "", "FilterAndDesign", "200", ((string[])(null)));
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Compaction Get Summary volumes")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CompactionSummaryVolumes")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "FilterAndInvalidDesign")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "FilterAndInvalidDesign")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUid", "ff91dd40-1569-4765-a2bc-014321f76ace")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:DesignUid", "00000000-0000-0000-0000-000000000000")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUid", "f07ed071-f8a1-42c3-804a-1bde7a78be5b")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUid2", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "FilterAndInvalidDesign")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:httpCode", "400")]
        public virtual void CompactionGetSummaryVolumes_FilterAndInvalidDesign()
        {
            this.CompactionGetSummaryVolumes("FilterAndInvalidDesign", "ff91dd40-1569-4765-a2bc-014321f76ace", "00000000-0000-0000-0000-000000000000", "f07ed071-f8a1-42c3-804a-1bde7a78be5b", "", "FilterAndInvalidDesign", "400", ((string[])(null)));
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Compaction Get Summary volumes")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CompactionSummaryVolumes")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "InvalidFilterAndDesign")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "InvalidFilterAndDesign")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUid", "ff91dd40-1569-4765-a2bc-014321f76ace")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:DesignUid", "12e86a90-b301-446e-8e37-7879f1d8fd39")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUid", "00000000-0000-0000-0000-000000000000")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUid2", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "FilterAndInvalidDesign")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:httpCode", "400")]
        public virtual void CompactionGetSummaryVolumes_InvalidFilterAndDesign()
        {
            this.CompactionGetSummaryVolumes("InvalidFilterAndDesign", "ff91dd40-1569-4765-a2bc-014321f76ace", "12e86a90-b301-446e-8e37-7879f1d8fd39", "00000000-0000-0000-0000-000000000000", "", "FilterAndInvalidDesign", "400", ((string[])(null)));
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Compaction Get Summary volumes")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CompactionSummaryVolumes")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "CustomBulkingAndShrinkage")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "CustomBulkingAndShrinkage")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUid", "3335311a-f0e2-4dbe-8acd-f21135bafee4")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:DesignUid", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUid", "A54E5945-1AAA-4921-9CC1-C9D8C0A343D3")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUid2", "A54E5945-1AAA-4921-9CC1-C9D8C0A343D3")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "CustomBulkingAndShrinkage")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:httpCode", "200")]
        public virtual void CompactionGetSummaryVolumes_CustomBulkingAndShrinkage()
        {
            this.CompactionGetSummaryVolumes("CustomBulkingAndShrinkage", "3335311a-f0e2-4dbe-8acd-f21135bafee4", "", "A54E5945-1AAA-4921-9CC1-C9D8C0A343D3", "A54E5945-1AAA-4921-9CC1-C9D8C0A343D3", "CustomBulkingAndShrinkage", "200", ((string[])(null)));
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Compaction Get Summary volumes")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CompactionSummaryVolumes")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "DesignToDesign")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "DesignToDesign")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUid", "ff91dd40-1569-4765-a2bc-014321f76ace")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:DesignUid", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUid", "ea89be4b-0efb-4b8f-ba33-03f0973bfc7b")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUid2", "ea89be4b-0efb-4b8f-ba33-03f0973bfc7b")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "EmptyJsonResponse")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:httpCode", "200")]
        public virtual void CompactionGetSummaryVolumes_DesignToDesign()
        {
            this.CompactionGetSummaryVolumes("DesignToDesign", "ff91dd40-1569-4765-a2bc-014321f76ace", "", "ea89be4b-0efb-4b8f-ba33-03f0973bfc7b", "ea89be4b-0efb-4b8f-ba33-03f0973bfc7b", "EmptyJsonResponse", "200", ((string[])(null)));
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Compaction Get Summary volumes")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CompactionSummaryVolumes")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "DesignToGround")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "DesignToGround")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUid", "ff91dd40-1569-4765-a2bc-014321f76ace")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:DesignUid", "ea89be4b-0efb-4b8f-ba33-03f0973bfc7b")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUid", "a54e5945-1aaa-4921-9cc1-c9d8c0a343d3")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUid2", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "EmptyJsonResponse")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:httpCode", "200")]
        public virtual void CompactionGetSummaryVolumes_DesignToGround()
        {
            this.CompactionGetSummaryVolumes("DesignToGround", "ff91dd40-1569-4765-a2bc-014321f76ace", "ea89be4b-0efb-4b8f-ba33-03f0973bfc7b", "a54e5945-1aaa-4921-9cc1-c9d8c0a343d3", "", "EmptyJsonResponse", "200", ((string[])(null)));
        }
    }
}
#pragma warning restore
#endregion
