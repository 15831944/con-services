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
    public partial class ProjectSettingsFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private Microsoft.VisualStudio.TestTools.UnitTesting.TestContext _testContext;
        
#line 1 "ProjectSettings.feature"
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
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "ProjectSettings", "\tI should be able to validate project settings", ProgrammingLanguage.CSharp, ((string[])(null)));
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
                        && (testRunner.FeatureContext.FeatureInfo.Title != "ProjectSettings")))
            {
                global::ProductionDataSvc.AcceptanceTests.ProjectSettingsFeature.FeatureSetup(null);
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
        
        public virtual void ProjectSettingsValidateDefaultSettings(string requestName, string projectUID, string projectSettingsType, string code, string message, string[] exampleTags)
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Project Settings Validate Default Settings", exampleTags);
#line 4
this.ScenarioSetup(scenarioInfo);
#line 5
 testRunner.Given("the Project Settings Validation service URI \"/api/v2/validatesettings\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 6
 testRunner.And(string.Format("a projectUid \"{0}\"", projectUID), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 7
  testRunner.And("a projectSettings \"\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 8
  testRunner.And(string.Format("a settingsType \"{0}\"", projectSettingsType), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 9
 testRunner.When("I request settings validation", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 10
  testRunner.Then(string.Format("the result should contain code {0} and message \"{1}\"", code, message), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Project Settings Validate Default Settings: Targets")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "ProjectSettings")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "Targets")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "Targets")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUID", "ff91dd40-1569-4765-a2bc-014321f76ace")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectSettingsType", "1")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Code", "0")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Message", "Project settings Targets are valid")]
        public virtual void ProjectSettingsValidateDefaultSettings_Targets()
        {
#line 4
this.ProjectSettingsValidateDefaultSettings("Targets", "ff91dd40-1569-4765-a2bc-014321f76ace", "1", "0", "Project settings Targets are valid", ((string[])(null)));
#line hidden
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Project Settings Validate Default Settings: Colors")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "ProjectSettings")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "Colors")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "Colors")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUID", "ff91dd40-1569-4765-a2bc-014321f76ace")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectSettingsType", "3")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Code", "0")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Message", "Project settings Colors are valid")]
        public virtual void ProjectSettingsValidateDefaultSettings_Colors()
        {
#line 4
this.ProjectSettingsValidateDefaultSettings("Colors", "ff91dd40-1569-4765-a2bc-014321f76ace", "3", "0", "Project settings Colors are valid", ((string[])(null)));
#line hidden
        }
        
        public virtual void ProjectSettingsValidatePartialCustomSettings(string requestName, string projectUID, string projectSettings, string projectSettingsType, string code, string message, string[] exampleTags)
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Project Settings Validate Partial Custom Settings", exampleTags);
#line 16
this.ScenarioSetup(scenarioInfo);
#line 17
 testRunner.Given("the Project Settings Validation service URI \"/api/v2/validatesettings\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 18
 testRunner.And(string.Format("a projectUid \"{0}\"", projectUID), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 19
  testRunner.And(string.Format("a projectSettings \"{0}\"", projectSettings), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 20
  testRunner.And(string.Format("a settingsType \"{0}\"", projectSettingsType), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 21
 testRunner.When("I request settings validation", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 22
  testRunner.Then(string.Format("the result should contain code {0} and message \"{1}\"", code, message), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Project Settings Validate Partial Custom Settings: Targets")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "ProjectSettings")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "Targets")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "Targets")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUID", "ff91dd40-1569-4765-a2bc-014321f76ace")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectSettings", "{ useMachineTargetPassCount : false, customTargetPassCountMinimum : 5, customTarg" +
            "etPassCountMaximum : 7 }")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectSettingsType", "1")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Code", "0")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Message", "Project settings Targets are valid")]
        public virtual void ProjectSettingsValidatePartialCustomSettings_Targets()
        {
#line 16
this.ProjectSettingsValidatePartialCustomSettings("Targets", "ff91dd40-1569-4765-a2bc-014321f76ace", "{ useMachineTargetPassCount : false, customTargetPassCountMinimum : 5, customTarg" +
                    "etPassCountMaximum : 7 }", "1", "0", "Project settings Targets are valid", ((string[])(null)));
#line hidden
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Project Settings Validate Partial Custom Settings: Colors")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "ProjectSettings")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "Colors")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "Colors")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUID", "ff91dd40-1569-4765-a2bc-014321f76ace")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectSettings", "{ useDefaultMDPSummaryColors : false, mdpOnTargetColor : 0x8BC34A, mdpOverTargetC" +
            "olor : 0xD50000, mdpUnderTargetColor : 0x1579B }")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectSettingsType", "3")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Code", "0")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Message", "Project settings Colors are valid")]
        public virtual void ProjectSettingsValidatePartialCustomSettings_Colors()
        {
#line 16
this.ProjectSettingsValidatePartialCustomSettings("Colors", "ff91dd40-1569-4765-a2bc-014321f76ace", "{ useDefaultMDPSummaryColors : false, mdpOnTargetColor : 0x8BC34A, mdpOverTargetC" +
                    "olor : 0xD50000, mdpUnderTargetColor : 0x1579B }", "3", "0", "Project settings Colors are valid", ((string[])(null)));
#line hidden
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Project Settings Validate Full Custom Settings Targets")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "ProjectSettings")]
        public virtual void ProjectSettingsValidateFullCustomSettingsTargets()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Project Settings Validate Full Custom Settings Targets", ((string[])(null)));
#line 41
this.ScenarioSetup(scenarioInfo);
#line 42
 testRunner.Given("the Project Settings Validation service URI \"/api/v2/validatesettings\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 43
 testRunner.And("a projectUid \"ff91dd40-1569-4765-a2bc-014321f76ace\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 44
  testRunner.And("a settingsType \"1\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 45
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
  customPassCountTargets: [1,3,5,8,11,16,20,25],
  useDefaultCMVTargets: false, 
  customCMVTargets: [0,20,50,100,130],
  useDefaultTemperatureTargets: false, 
  customTemperatureTargets: [0,40,80,120,160,200,240]
}", ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 80
 testRunner.When("I request settings validation", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 81
 testRunner.Then("the settings validation result should be", "{\r\n  \"Code\": 0,\r\n  \"Message\": \"Project settings Targets are valid\"\r\n}", ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Project Settings Validate Full Custom Settings Colors")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "ProjectSettings")]
        public virtual void ProjectSettingsValidateFullCustomSettingsColors()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Project Settings Validate Full Custom Settings Colors", ((string[])(null)));
#line 89
this.ScenarioSetup(scenarioInfo);
#line 90
 testRunner.Given("the Project Settings Validation service URI \"/api/v2/validatesettings\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 91
 testRunner.And("a projectUid \"ff91dd40-1569-4765-a2bc-014321f76ace\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 92
  testRunner.And("a settingsType \"3\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 93
  testRunner.And("a projectSettings (multiline)", "{\r\n  useDefaultElevationColors: true,\r\n  elevationColors: [0xC80000, 0xFF0000, 0x" +
                    "FF3C00, 0xFF5A00, 0xFF8200, 0xFFAA00, 0xFFC800, \r\n                      0xFFDC00" +
                    ", 0xFAE600, 0xDCE600, 0xD2E600, 0xC8E600, 0xB4E600, 0x96E600, \r\n                " +
                    "      0x82E600, 0x64F000, 0x00FF00, 0x00F064, 0x00E682, 0x00E696, 0x00E6B4,\r\n   " +
                    "                   0x00E6C8, 0x00E6D2, 0x00DCDC, 0x00E6E6, 0x00C8E6, 0x00B4F0, 0" +
                    "x0096F5,\r\n                      0x0078FA, 0x005AFF, 0x0046FF, 0x0000FF],\r\n  useD" +
                    "efaultCMVDetailsColors: true,\r\n  cmvDetailsColors: [0x01579B, 0x6BACD5, 0x99CB65" +
                    ", 0xF6A3A8, 0xD50000],\r\n  useDefaultCMVSummaryColors: true,\r\n  cmvOnTargetColor:" +
                    " 0x8BC34A,\r\n  cmvOverTargetColor: 0xD50000,\r\n  cmvUnderTargetColor: 0x1579B,\r\n  " +
                    "useDefaultCMVPercentColors: true,\r\n  cmvPercentColors: [0xD50000, 0xE57373, 0xFF" +
                    "CDD2, 0x8BC34A, 0xB3E5FC, 0x005AFF, 0x039BE5, 0x01579B],\r\n  useDefaultPassCountD" +
                    "etailsColors: true,\r\n  passCountDetailsColors: [0x2D5783, 0x439BDC, 0xBEDFF1, 0x" +
                    "9DCE67, 0x6BA03E, 0x3A6B25, 0xF6CED3, 0xD57A7C, 0xC13037],\r\n  useDefaultPassCoun" +
                    "tSummaryColors: true,\r\n  passCountOnTargetColor: 0x8BC34A,\r\n  passCountOverTarge" +
                    "tColor: 0xD50000,\r\n  passCountUnderTargetColor: 0x1579B,\r\n  useDefaultCutFillCol" +
                    "ors: true,\r\n  cutFillColors: [0xD50000, 0xE57373, 0xFFCDD2, 0x8BC34A, 0xB3E5FC, " +
                    "0x039BE5, 0x01579B],\r\n  useDefaultTemperatureSummaryColors: true,\r\n  temperature" +
                    "OnTargetColor: 0x8BC34A,\r\n  temperatureOverTargetColor: 0xD50000,\r\n  temperature" +
                    "UnderTargetColor: 0x1579B,\r\n  useDefaultTemperatureDetailsColors: true,\r\n  tempe" +
                    "ratureDetailsColors: [0x01579B, 0x039BE5, 0xB3E5FC, 0x99CB65, 0xF6A3A8, 0x00F064" +
                    ", 0xD50000],\r\n  useDefaultSpeedSummaryColors: true,\r\n  speedOnTargetColor: 0x8BC" +
                    "34A,\r\n  speedOverTargetColor: 0xD50000,\r\n  speedUnderTargetColor: 0x1579B,\r\n  us" +
                    "eDefaultMDPSummaryColors: true,\r\n  mdpOnTargetColor: 0x8BC34A,\r\n  mdpOverTargetC" +
                    "olor: 0xD50000,\r\n  mdpUnderTargetColor: 0x1579B\r\n}", ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 134
 testRunner.When("I request settings validation", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 135
 testRunner.Then("the settings validation result should be", "{\r\n  \"Code\": 0,\r\n  \"Message\": \"Project settings Colors are valid\"\r\n}", ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        public virtual void ProjectSettingsValidateInvalidSettingsMissingValues(string requestName, string projectUID, string projectSettings, string projectSettingsType, string code, string message, string[] exampleTags)
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Project Settings Validate Invalid Settings Missing Values", exampleTags);
#line 144
this.ScenarioSetup(scenarioInfo);
#line 145
 testRunner.Given("the Project Settings Validation service URI \"/api/v2/validatesettings\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 146
 testRunner.And(string.Format("a projectUid \"{0}\"", projectUID), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 147
  testRunner.And(string.Format("a projectSettings \"{0}\"", projectSettings), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 148
  testRunner.And(string.Format("a settingsType \"{0}\"", projectSettingsType), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 149
 testRunner.When("I request settings validation expecting bad request", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 151
  testRunner.Then(string.Format("the result should contain code {0} and message \"{1}\"", code, message), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Project Settings Validate Invalid Settings Missing Values: Targets")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "ProjectSettings")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "Targets")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "Targets")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUID", "ff91dd40-1569-4765-a2bc-014321f76ace")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectSettings", "{ useMachineTargetPassCount : false, customTargetPassCountMinimum : 5 }")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectSettingsType", "1")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Code", "-1")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Message", "Both minimum and maximum target pass count must be specified")]
        public virtual void ProjectSettingsValidateInvalidSettingsMissingValues_Targets()
        {
#line 144
this.ProjectSettingsValidateInvalidSettingsMissingValues("Targets", "ff91dd40-1569-4765-a2bc-014321f76ace", "{ useMachineTargetPassCount : false, customTargetPassCountMinimum : 5 }", "1", "-1", "Both minimum and maximum target pass count must be specified", ((string[])(null)));
#line hidden
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Project Settings Validate Invalid Settings Missing Values: Colors")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "ProjectSettings")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "Colors")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "Colors")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUID", "ff91dd40-1569-4765-a2bc-014321f76ace")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectSettings", "{ useDefaultMDPSummaryColors : false, mdpOnTargetColor : 0x8BC34A, mdpOverTargetC" +
            "olor : 0xD50000 }")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectSettingsType", "3")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Code", "-1")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Message", "mdpUnderTargetColor colour values must be specified")]
        public virtual void ProjectSettingsValidateInvalidSettingsMissingValues_Colors()
        {
#line 144
this.ProjectSettingsValidateInvalidSettingsMissingValues("Colors", "ff91dd40-1569-4765-a2bc-014321f76ace", "{ useDefaultMDPSummaryColors : false, mdpOnTargetColor : 0x8BC34A, mdpOverTargetC" +
                    "olor : 0xD50000 }", "3", "-1", "mdpUnderTargetColor colour values must be specified", ((string[])(null)));
#line hidden
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Project Settings Validate Invalid Settings Out Of Range Values")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "ProjectSettings")]
        public virtual void ProjectSettingsValidateInvalidSettingsOutOfRangeValues()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Project Settings Validate Invalid Settings Out Of Range Values", ((string[])(null)));
#line 158
 this.ScenarioSetup(scenarioInfo);
#line 159
 testRunner.Given("the Project Settings Validation service URI \"/api/v2/validatesettings\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 160
 testRunner.And("a projectUid \"ff91dd40-1569-4765-a2bc-014321f76ace\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 161
  testRunner.And("a projectSettings \"{ useMachineTargetPassCount : false, customTargetPassCountMini" +
                    "mum : 0, customTargetPassCountMaximum : 7 }\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 162
  testRunner.And("a settingsType \"1\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 163
 testRunner.When("I request settings validation expecting bad request", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 164
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
#line 166
this.ScenarioSetup(scenarioInfo);
#line 167
 testRunner.Given("the Project Settings Validation service URI \"/api/v2/validatesettings\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 168
 testRunner.And("a projectUid \"ff91dd40-1569-4765-a2bc-014321f76ace\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 169
  testRunner.And("a projectSettings \"{ useDefaultCutFillTolerances : false, customCutFillTolerances" +
                    " : [3,2,1,0,-1,-3,-2] }\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 170
  testRunner.And("a settingsType \"1\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 171
 testRunner.When("I request settings validation expecting bad request", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 172
 testRunner.Then("I should get error code -1 and message \"Cut-fill tolerances must be in order of h" +
                    "ighest cut to lowest fill\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
