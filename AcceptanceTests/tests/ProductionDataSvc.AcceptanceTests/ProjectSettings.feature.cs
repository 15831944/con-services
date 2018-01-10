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
    public partial class ProjectSettingsFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "ProjectSettings.feature"
#line hidden
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute()]
        public static void FeatureSetup(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext testContext)
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "ProjectSettings", "I should be able to validate project settings", ProgrammingLanguage.CSharp, ((string[])(null)));
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
                        && (TechTalk.SpecFlow.FeatureContext.Current.FeatureInfo.Title != "ProjectSettings")))
            {
                ProductionDataSvc.AcceptanceTests.ProjectSettingsFeature.FeatureSetup(null);
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
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Project Settings Validate Default Settings")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "ProjectSettings")]
        public virtual void ProjectSettingsValidateDefaultSettings()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Project Settings Validate Default Settings", ((string[])(null)));
#line 4
this.ScenarioSetup(scenarioInfo);
#line 5
 testRunner.Given("the Project Settings Validation service URI \"/api/v2/validatesettings\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 6
 testRunner.And("a projectUid \"ff91dd40-1569-4765-a2bc-014321f76ace\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 7
  testRunner.And("a projectSettings \"\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 8
 testRunner.When("I request settings validation", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 9
 testRunner.Then("the settings validation result should be", "{\n  \"Code\": 0,\n  \"Message\": \"Project settings are valid\"\n}", ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Project Settings Validate Partial Custom Settings")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "ProjectSettings")]
        public virtual void ProjectSettingsValidatePartialCustomSettings()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Project Settings Validate Partial Custom Settings", ((string[])(null)));
#line 17
this.ScenarioSetup(scenarioInfo);
#line 18
 testRunner.Given("the Project Settings Validation service URI \"/api/v2/validatesettings\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 19
 testRunner.And("a projectUid \"ff91dd40-1569-4765-a2bc-014321f76ace\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 20
  testRunner.And("a projectSettings \"{ useMachineTargetPassCount : false, customTargetPassCountMini" +
                    "mum : 5, customTargetPassCountMaximum : 7 }\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 21
 testRunner.When("I request settings validation", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 22
 testRunner.Then("the settings validation result should be", "{\n  \"Code\": 0,\n  \"Message\": \"Project settings are valid\"\n}", ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Project Settings Validate Full Custom Settings")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "ProjectSettings")]
        public virtual void ProjectSettingsValidateFullCustomSettings()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Project Settings Validate Full Custom Settings", ((string[])(null)));
#line 30
this.ScenarioSetup(scenarioInfo);
#line 31
 testRunner.Given("the Project Settings Validation service URI \"/api/v2/validatesettings\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 32
 testRunner.And("a projectUid \"ff91dd40-1569-4765-a2bc-014321f76ace\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 33
  testRunner.And("a projectSettings (multiline)", @"{
  useMachineTargetPassCount: false,
  customTargetPassCountMinimum: 5,
  customTargetPassCountMaximum: 7,
  useMachineTargetTemperature: false,
  customTargetTemperatureMinimum: 75,
  customTargetTemperatureMaximum: 150,
  useMachineTargetCmv: false,
  customTargetCmv: 77,
  useMachineTargetMdp: false,
  customTargetMdp: 88,
  useDefaultTargetRangeCmvPercent: false,
  customTargetCmvPercentMinimum: 75,
  customTargetCmvPercentMaximum: 105,
  useDefaultTargetRangeMdpPercent: false,
  customTargetMdpPercentMinimum: 85,
  customTargetMdpPercentMaximum: 115,
  useDefaultTargetRangeSpeed: false,
  customTargetSpeedMinimum: 10,
  customTargetSpeedMaximum: 30,
  useDefaultCutFillTolerances: false,
  customCutFillTolerances: [3,2,1,0,-1,-2,-3],
  useDefaultVolumeShrinkageBulking: false,
  customShrinkagePercent: 5,
  customBulkingPercent: 7.5,
  useDefaultPassCountTargets: false,
  customPassCountTargets: [1,3,5,8,11,16,20,25]
}", ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 64
 testRunner.When("I request settings validation", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 65
 testRunner.Then("the settings validation result should be", "{\n  \"Code\": 0,\n  \"Message\": \"Project settings are valid\"\n}", ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Project Settings Validate Invalid Settings Missing Values")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "ProjectSettings")]
        public virtual void ProjectSettingsValidateInvalidSettingsMissingValues()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Project Settings Validate Invalid Settings Missing Values", ((string[])(null)));
#line 73
this.ScenarioSetup(scenarioInfo);
#line 74
 testRunner.Given("the Project Settings Validation service URI \"/api/v2/validatesettings\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 75
 testRunner.And("a projectUid \"ff91dd40-1569-4765-a2bc-014321f76ace\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 76
  testRunner.And("a projectSettings \"{ useMachineTargetPassCount : false, customTargetPassCountMini" +
                    "mum : 5 }\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 77
 testRunner.When("I request settings validation expecting bad request", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 78
 testRunner.Then("I should get error code -1 and message \"Both minimum and maximum target pass coun" +
                    "t must be specified\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Project Settings Validate Invalid Settings Out Of Range Values")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "ProjectSettings")]
        public virtual void ProjectSettingsValidateInvalidSettingsOutOfRangeValues()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Project Settings Validate Invalid Settings Out Of Range Values", ((string[])(null)));
#line 80
 this.ScenarioSetup(scenarioInfo);
#line 81
 testRunner.Given("the Project Settings Validation service URI \"/api/v2/validatesettings\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 82
 testRunner.And("a projectUid \"ff91dd40-1569-4765-a2bc-014321f76ace\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 83
  testRunner.And("a projectSettings \"{ useMachineTargetPassCount : false, customTargetPassCountMini" +
                    "mum : 0, customTargetPassCountMaximum : 7 }\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 84
 testRunner.When("I request settings validation expecting bad request", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 85
 testRunner.Then("I should get error code -1 and message \"The field customTargetPassCountMinimum mu" +
                    "st be between 1 and 80.\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Project Settings Validate Invalid Settings Out Of Order Values")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "ProjectSettings")]
        public virtual void ProjectSettingsValidateInvalidSettingsOutOfOrderValues()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Project Settings Validate Invalid Settings Out Of Order Values", ((string[])(null)));
#line 87
this.ScenarioSetup(scenarioInfo);
#line 88
 testRunner.Given("the Project Settings Validation service URI \"/api/v2/validatesettings\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 89
 testRunner.And("a projectUid \"ff91dd40-1569-4765-a2bc-014321f76ace\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 90
  testRunner.And("a projectSettings \"{ useDefaultCutFillTolerances : false, customCutFillTolerances" +
                    " : [3,2,1,0,-1,-3,-2] }\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 91
 testRunner.When("I request settings validation expecting bad request", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 92
 testRunner.Then("I should get error code -1 and message \"Cut-fill tolerances must be in order of h" +
                    "ighest cut to lowest fill\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
