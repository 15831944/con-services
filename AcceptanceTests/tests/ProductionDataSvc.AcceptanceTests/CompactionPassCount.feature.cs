﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:2.3.2.0
//      SpecFlow Generator Version:2.3.0.0
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
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.3.2.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute()]
    public partial class CompactionPassCountFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private Microsoft.VisualStudio.TestTools.UnitTesting.TestContext _testContext;
        
#line 1 "CompactionPassCount.feature"
#line hidden
        
        public virtual Microsoft.VisualStudio.TestTools.UnitTesting.TestContext TestContext
        {
            get
            {
                return this._testContext;
            }
            set
            {
                this._testContext = value;
            }
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute()]
        public static void FeatureSetup(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext testContext)
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner(null, 0);
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "CompactionPassCount", "I should be able to request Pass Count compaction data", ProgrammingLanguage.CSharp, ((string[])(null)));
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
            if (((testRunner.FeatureContext != null) 
                        && (testRunner.FeatureContext.FeatureInfo.Title != "CompactionPassCount")))
            {
                global::ProductionDataSvc.AcceptanceTests.CompactionPassCountFeature.FeatureSetup(null);
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
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<Microsoft.VisualStudio.TestTools.UnitTesting.TestContext>(TestContext);
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        public virtual void CompactionGetPasscountSummary_NoDesignFilter(string requestName, string projectUID, string resultName, string[] exampleTags)
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Compaction Get Passcount Summary - No Design Filter", exampleTags);
#line 5
this.ScenarioSetup(scenarioInfo);
#line 6
testRunner.Given("the Compaction service URI \"/api/v2/passcounts/summary\" for operation \"PassCountS" +
                    "ummary\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 7
testRunner.And("the result file \"CompactionGetPassCountDataResponse.json\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 8
testRunner.And(string.Format("projectUid \"{0}\"", projectUID), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 9
testRunner.When("I request result", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 10
testRunner.Then(string.Format("the result should match the \"{0}\" from the repository", resultName), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Compaction Get Passcount Summary - No Design Filter: ")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CompactionPassCount")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUID", "ff91dd40-1569-4765-a2bc-014321f76ace")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "NoDesignFilter_Summary")]
        public virtual void CompactionGetPasscountSummary_NoDesignFilter_()
        {
#line 5
this.CompactionGetPasscountSummary_NoDesignFilter("", "ff91dd40-1569-4765-a2bc-014321f76ace", "NoDesignFilter_Summary", ((string[])(null)));
#line hidden
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Compaction Get Passcount Summary - No Design Filter: ProjectSettings")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CompactionPassCount")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "ProjectSettings")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "ProjectSettings")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUID", "3335311a-f0e2-4dbe-8acd-f21135bafee4")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "NoDesignFilter_Summary_PS")]
        public virtual void CompactionGetPasscountSummary_NoDesignFilter_ProjectSettings()
        {
#line 5
this.CompactionGetPasscountSummary_NoDesignFilter("ProjectSettings", "3335311a-f0e2-4dbe-8acd-f21135bafee4", "NoDesignFilter_Summary_PS", ((string[])(null)));
#line hidden
        }
        
        public virtual void CompactionGetPasscountSummary(string requestName, string projectUID, string filterUID, string resultName, string[] exampleTags)
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Compaction Get Passcount Summary", exampleTags);
#line 16
this.ScenarioSetup(scenarioInfo);
#line 17
testRunner.Given("the Compaction service URI \"/api/v2/passcounts/summary\" for operation \"PassCountS" +
                    "ummary\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 18
testRunner.And("the result file \"CompactionGetPassCountDataResponse.json\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 19
testRunner.And(string.Format("projectUid \"{0}\"", projectUID), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 20
testRunner.And(string.Format("filterUid \"{0}\"", filterUID), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 21
testRunner.When("I request result", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 22
testRunner.Then(string.Format("the result should match the \"{0}\" from the repository", resultName), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Compaction Get Passcount Summary: DesignOutside")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CompactionPassCount")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "DesignOutside")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "DesignOutside")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUID", "7925f179-013d-4aaf-aff4-7b9833bb06d6")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUID", "1cf81668-1739-42d5-b068-ea025588796a")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "DesignOutside_Summary")]
        public virtual void CompactionGetPasscountSummary_DesignOutside()
        {
#line 16
this.CompactionGetPasscountSummary("DesignOutside", "7925f179-013d-4aaf-aff4-7b9833bb06d6", "1cf81668-1739-42d5-b068-ea025588796a", "DesignOutside_Summary", ((string[])(null)));
#line hidden
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Compaction Get Passcount Summary: DesignIntersects")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CompactionPassCount")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "DesignIntersects")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "DesignIntersects")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUID", "7925f179-013d-4aaf-aff4-7b9833bb06d6")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUID", "3d9086f2-3c04-4d92-9141-5134932b1523")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "DesignIntersects_Summary")]
        public virtual void CompactionGetPasscountSummary_DesignIntersects()
        {
#line 16
this.CompactionGetPasscountSummary("DesignIntersects", "7925f179-013d-4aaf-aff4-7b9833bb06d6", "3d9086f2-3c04-4d92-9141-5134932b1523", "DesignIntersects_Summary", ((string[])(null)));
#line hidden
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Compaction Get Passcount Summary: FilterArea")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CompactionPassCount")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "FilterArea")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "FilterArea")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUID", "ff91dd40-1569-4765-a2bc-014321f76ace")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUID", "a37f3008-65e5-44a8-b406-9a078ec62ece")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "BoundaryFilter_Summary")]
        public virtual void CompactionGetPasscountSummary_FilterArea()
        {
#line 16
this.CompactionGetPasscountSummary("FilterArea", "ff91dd40-1569-4765-a2bc-014321f76ace", "a37f3008-65e5-44a8-b406-9a078ec62ece", "BoundaryFilter_Summary", ((string[])(null)));
#line hidden
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Compaction Get Passcount Summary: AsAtToday")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CompactionPassCount")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "AsAtToday")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "AsAtToday")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUID", "ff91dd40-1569-4765-a2bc-014321f76ace")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUID", "c638018c-5026-44be-af0b-006ecad65462")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "BoundaryFilter_Summary")]
        public virtual void CompactionGetPasscountSummary_AsAtToday()
        {
#line 16
this.CompactionGetPasscountSummary("AsAtToday", "ff91dd40-1569-4765-a2bc-014321f76ace", "c638018c-5026-44be-af0b-006ecad65462", "BoundaryFilter_Summary", ((string[])(null)));
#line hidden
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Compaction Get Passcount Summary: AsAtCustom")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CompactionPassCount")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "AsAtCustom")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "AsAtCustom")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUID", "ff91dd40-1569-4765-a2bc-014321f76ace")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUID", "a8405aca-71f1-463d-8821-c2415d67e78c")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "AsAtCustom_Summary")]
        public virtual void CompactionGetPasscountSummary_AsAtCustom()
        {
#line 16
this.CompactionGetPasscountSummary("AsAtCustom", "ff91dd40-1569-4765-a2bc-014321f76ace", "a8405aca-71f1-463d-8821-c2415d67e78c", "AsAtCustom_Summary", ((string[])(null)));
#line hidden
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Compaction Get Passcount Summary: AlignmentFilter")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CompactionPassCount")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "AlignmentFilter")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "AlignmentFilter")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUID", "ff91dd40-1569-4765-a2bc-014321f76ace")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUID", "2811c7c3-d270-4d63-97e2-fc3340bf6c7a")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "AlignmentFilter_Summary")]
        public virtual void CompactionGetPasscountSummary_AlignmentFilter()
        {
#line 16
this.CompactionGetPasscountSummary("AlignmentFilter", "ff91dd40-1569-4765-a2bc-014321f76ace", "2811c7c3-d270-4d63-97e2-fc3340bf6c7a", "AlignmentFilter_Summary", ((string[])(null)));
#line hidden
        }
        
        public virtual void CompactionGetPasscountDetails_NoDesignFilter(string requestName, string projectUID, string resultName, string[] exampleTags)
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Compaction Get Passcount Details - No Design Filter", exampleTags);
#line 33
this.ScenarioSetup(scenarioInfo);
#line 34
testRunner.Given("the Compaction service URI \"/api/v2/passcounts/details\" for operation \"PassCountD" +
                    "etails\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 35
testRunner.And("the result file \"CompactionGetPassCountDataResponse.json\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 36
testRunner.And(string.Format("projectUid \"{0}\"", projectUID), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 37
testRunner.When("I request result", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 38
testRunner.Then(string.Format("the result should match the \"{0}\" from the repository", resultName), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Compaction Get Passcount Details - No Design Filter: ")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CompactionPassCount")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUID", "ff91dd40-1569-4765-a2bc-014321f76ace")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "NoDesignFilter_Details")]
        public virtual void CompactionGetPasscountDetails_NoDesignFilter_()
        {
#line 33
this.CompactionGetPasscountDetails_NoDesignFilter("", "ff91dd40-1569-4765-a2bc-014321f76ace", "NoDesignFilter_Details", ((string[])(null)));
#line hidden
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Compaction Get Passcount Details - No Design Filter: ProjectSettings")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CompactionPassCount")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "ProjectSettings")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "ProjectSettings")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUID", "3335311a-f0e2-4dbe-8acd-f21135bafee4")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "NoDesignFilter_Details_PS")]
        public virtual void CompactionGetPasscountDetails_NoDesignFilter_ProjectSettings()
        {
#line 33
this.CompactionGetPasscountDetails_NoDesignFilter("ProjectSettings", "3335311a-f0e2-4dbe-8acd-f21135bafee4", "NoDesignFilter_Details_PS", ((string[])(null)));
#line hidden
        }
        
        public virtual void CompactionGetPasscountDetails(string requestName, string projectUID, string filterUID, string resultName, string[] exampleTags)
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Compaction Get Passcount Details", exampleTags);
#line 44
this.ScenarioSetup(scenarioInfo);
#line 45
testRunner.Given("the Compaction service URI \"/api/v2/passcounts/details\" for operation \"PassCountD" +
                    "etails\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 46
testRunner.And("the result file \"CompactionGetPassCountDataResponse.json\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 47
testRunner.And(string.Format("projectUid \"{0}\"", projectUID), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 48
testRunner.And(string.Format("filterUid \"{0}\"", filterUID), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 49
testRunner.When("I request result", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 50
testRunner.Then(string.Format("the result should match the \"{0}\" from the repository", resultName), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Compaction Get Passcount Details: DesignOutside")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CompactionPassCount")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "DesignOutside")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "DesignOutside")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUID", "7925f179-013d-4aaf-aff4-7b9833bb06d6")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUID", "1cf81668-1739-42d5-b068-ea025588796a")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "DesignOutside_Details")]
        public virtual void CompactionGetPasscountDetails_DesignOutside()
        {
#line 44
this.CompactionGetPasscountDetails("DesignOutside", "7925f179-013d-4aaf-aff4-7b9833bb06d6", "1cf81668-1739-42d5-b068-ea025588796a", "DesignOutside_Details", ((string[])(null)));
#line hidden
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Compaction Get Passcount Details: DesignIntersects")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CompactionPassCount")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "DesignIntersects")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "DesignIntersects")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUID", "7925f179-013d-4aaf-aff4-7b9833bb06d6")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUID", "3d9086f2-3c04-4d92-9141-5134932b1523")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "DesignIntersects_Details")]
        public virtual void CompactionGetPasscountDetails_DesignIntersects()
        {
#line 44
this.CompactionGetPasscountDetails("DesignIntersects", "7925f179-013d-4aaf-aff4-7b9833bb06d6", "3d9086f2-3c04-4d92-9141-5134932b1523", "DesignIntersects_Details", ((string[])(null)));
#line hidden
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Compaction Get Passcount Details: FilterArea")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CompactionPassCount")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "FilterArea")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "FilterArea")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUID", "ff91dd40-1569-4765-a2bc-014321f76ace")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUID", "154470b6-15ae-4cca-b281-eae8ac1efa6c")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "BoundaryFilterPCDetails")]
        public virtual void CompactionGetPasscountDetails_FilterArea()
        {
#line 44
this.CompactionGetPasscountDetails("FilterArea", "ff91dd40-1569-4765-a2bc-014321f76ace", "154470b6-15ae-4cca-b281-eae8ac1efa6c", "BoundaryFilterPCDetails", ((string[])(null)));
#line hidden
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Compaction Get Passcount Details: AsAtToday")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CompactionPassCount")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "AsAtToday")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "AsAtToday")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUID", "ff91dd40-1569-4765-a2bc-014321f76ace")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUID", "3c836562-bcd5-4a35-99a5-cb5655572be7")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "BoundaryFilterPCDetails")]
        public virtual void CompactionGetPasscountDetails_AsAtToday()
        {
#line 44
this.CompactionGetPasscountDetails("AsAtToday", "ff91dd40-1569-4765-a2bc-014321f76ace", "3c836562-bcd5-4a35-99a5-cb5655572be7", "BoundaryFilterPCDetails", ((string[])(null)));
#line hidden
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Compaction Get Passcount Details: AsAtCustom")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CompactionPassCount")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "AsAtCustom")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "AsAtCustom")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUID", "ff91dd40-1569-4765-a2bc-014321f76ace")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUID", "a8405aca-71f1-463d-8821-c2415d67e78c")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "AsAtCustomPCDetails")]
        public virtual void CompactionGetPasscountDetails_AsAtCustom()
        {
#line 44
this.CompactionGetPasscountDetails("AsAtCustom", "ff91dd40-1569-4765-a2bc-014321f76ace", "a8405aca-71f1-463d-8821-c2415d67e78c", "AsAtCustomPCDetails", ((string[])(null)));
#line hidden
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Compaction Get Passcount Details: FilterAreaMachine")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CompactionPassCount")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "FilterAreaMachine")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "FilterAreaMachine")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUID", "ff91dd40-1569-4765-a2bc-014321f76ace")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUID", "9c27697f-ea6d-478a-a168-ed20d6cd9a20")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "BoundaryMachineFilterPCDetails")]
        public virtual void CompactionGetPasscountDetails_FilterAreaMachine()
        {
#line 44
this.CompactionGetPasscountDetails("FilterAreaMachine", "ff91dd40-1569-4765-a2bc-014321f76ace", "9c27697f-ea6d-478a-a168-ed20d6cd9a20", "BoundaryMachineFilterPCDetails", ((string[])(null)));
#line hidden
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Compaction Get Passcount Details: AlignmentFilter")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CompactionPassCount")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "AlignmentFilter")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "AlignmentFilter")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUID", "ff91dd40-1569-4765-a2bc-014321f76ace")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUID", "2811c7c3-d270-4d63-97e2-fc3340bf6c6b")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "AlignmentFilter_Details")]
        public virtual void CompactionGetPasscountDetails_AlignmentFilter()
        {
#line 44
this.CompactionGetPasscountDetails("AlignmentFilter", "ff91dd40-1569-4765-a2bc-014321f76ace", "2811c7c3-d270-4d63-97e2-fc3340bf6c6b", "AlignmentFilter_Details", ((string[])(null)));
#line hidden
        }
    }
}
#pragma warning restore
#endregion
