<<<<<<< HEAD:src/TagFileHarvesterService/TagFileHarvesterServiceSettings.Designer.cs
﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace VSS.Productivity3D.TagFileHarvester {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.3.0.0")]
    public sealed partial class TagFileHarvesterServiceSettings : global::System.Configuration.ApplicationSettingsBase {
        
        private static TagFileHarvesterServiceSettings defaultInstance = ((TagFileHarvesterServiceSettings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new TagFileHarvesterServiceSettings())));
        
        public static TagFileHarvesterServiceSettings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Machine Control Data")]
        public string TCCSynchMachineControlFolder {
            get {
                return ((string)(this["TCCSynchMachineControlFolder"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Production-Data (Archived)")]
        public string TCCSynchProductionDataArchivedFolder {
            get {
                return ((string)(this["TCCSynchProductionDataArchivedFolder"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(".Production-Data")]
        public string TCCSynchProductionDataFolder {
            get {
                return ((string)(this["TCCSynchProductionDataFolder"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("TrimbleSynchronizerData")]
        public string TCCSynchFilespaceShortName {
            get {
                return ((string)(this["TCCSynchFilespaceShortName"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool TCCArchiveFiles {
            get {
                return ((bool)(this["TCCArchiveFiles"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("00:03:00")]
        public global::System.TimeSpan TCCRequestTimeout {
            get {
                return ((global::System.TimeSpan)(this["TCCRequestTimeout"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("16")]
        public int MaxThreadsToProcessTagFiles {
            get {
                return ((int)(this["MaxThreadsToProcessTagFiles"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("00:05:00")]
        public global::System.TimeSpan TagFileSubmitterRunTimeout {
            get {
                return ((global::System.TimeSpan)(this["TagFileSubmitterRunTimeout"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("00:15:00")]
        public global::System.TimeSpan TagFileSubmitterTasksTimeout {
            get {
                return ((global::System.TimeSpan)(this["TagFileSubmitterTasksTimeout"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("150")]
        public int NumberOfFilesInPackage {
            get {
                return ((int)(this["NumberOfFilesInPackage"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("00:00:01")]
        public global::System.TimeSpan OrgProcessingDelay {
            get {
                return ((global::System.TimeSpan)(this["OrgProcessingDelay"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("true")]
        public string ArchiveOnlySuccessfullFiles {
            get {
                return ((string)(this["ArchiveOnlySuccessfullFiles"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("LimitedConcurrencyTaskScheduler")]
        public string ITaskHarvester {
            get {
                return ((string)(this["ITaskHarvester"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("00:20:00")]
        public global::System.TimeSpan RefreshOrgsDelay {
            get {
                return ((global::System.TimeSpan)(this["RefreshOrgsDelay"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("00:30:00")]
        public global::System.TimeSpan BookmarkTolerance {
            get {
                return ((global::System.TimeSpan)(this["BookmarkTolerance"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool EnableHardScanningLogic {
            get {
                return ((bool)(this["EnableHardScanningLogic"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("00:01:00")]
        public global::System.TimeSpan BadFilesToleranceRollback {
            get {
                return ((global::System.TimeSpan)(this["BadFilesToleranceRollback"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool CacheEnabled {
            get {
                return ((bool)(this["CacheEnabled"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool FilenameDumpEnabled {
            get {
                return ((bool)(this["FilenameDumpEnabled"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("64.00:00:00")]
        public global::System.TimeSpan FolderSearchTimeSpan {
            get {
                return ((global::System.TimeSpan)(this["FolderSearchTimeSpan"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool UseModifyTimeInsteadOfCreateTime {
            get {
                return ((bool)(this["UseModifyTimeInsteadOfCreateTime"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("D:\\Bookmarks")]
        public string BookmarkPath {
            get {
                return ((string)(this["BookmarkPath"]));
            }
        }
    }
}
=======
﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TagFileHarvester {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.6.0.0")]
    public sealed partial class TagFileHarvesterServiceSettings : global::System.Configuration.ApplicationSettingsBase {
        
        private static TagFileHarvesterServiceSettings defaultInstance = ((TagFileHarvesterServiceSettings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new TagFileHarvesterServiceSettings())));
        
        public static TagFileHarvesterServiceSettings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Machine Control Data")]
        public string TCCSynchMachineControlFolder {
            get {
                return ((string)(this["TCCSynchMachineControlFolder"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Production-Data (Archived)")]
        public string TCCSynchProductionDataArchivedFolder {
            get {
                return ((string)(this["TCCSynchProductionDataArchivedFolder"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(".Production-Data")]
        public string TCCSynchProductionDataFolder {
            get {
                return ((string)(this["TCCSynchProductionDataFolder"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("TrimbleSynchronizerData")]
        public string TCCSynchFilespaceShortName {
            get {
                return ((string)(this["TCCSynchFilespaceShortName"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("00:05:00")]
        public global::System.TimeSpan TagFileSubmitterRunTimeout {
            get {
                return ((global::System.TimeSpan)(this["TagFileSubmitterRunTimeout"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("00:00:01")]
        public global::System.TimeSpan OrgProcessingDelay {
            get {
                return ((global::System.TimeSpan)(this["OrgProcessingDelay"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("true")]
        public string ArchiveOnlySuccessfullFiles {
            get {
                return ((string)(this["ArchiveOnlySuccessfullFiles"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("LimitedConcurrencyTaskScheduler")]
        public string ITaskHarvester {
            get {
                return ((string)(this["ITaskHarvester"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("00:20:00")]
        public global::System.TimeSpan RefreshOrgsDelay {
            get {
                return ((global::System.TimeSpan)(this["RefreshOrgsDelay"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("00:30:00")]
        public global::System.TimeSpan BookmarkTolerance {
            get {
                return ((global::System.TimeSpan)(this["BookmarkTolerance"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool EnableHardScanningLogic {
            get {
                return ((bool)(this["EnableHardScanningLogic"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("00:01:00")]
        public global::System.TimeSpan BadFilesToleranceRollback {
            get {
                return ((global::System.TimeSpan)(this["BadFilesToleranceRollback"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool CacheEnabled {
            get {
                return ((bool)(this["CacheEnabled"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool FilenameDumpEnabled {
            get {
                return ((bool)(this["FilenameDumpEnabled"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("64.00:00:00")]
        public global::System.TimeSpan FolderSearchTimeSpan {
            get {
                return ((global::System.TimeSpan)(this["FolderSearchTimeSpan"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool UseModifyTimeInsteadOfCreateTime {
            get {
                return ((bool)(this["UseModifyTimeInsteadOfCreateTime"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("C:\\Bookmarks")]
        public string BookmarkPath {
            get {
                return ((string)(this["BookmarkPath"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("127.0.0.1")]
        public string SecondaryTagProcSvcIpAddr {
            get {
                return ((string)(this["SecondaryTagProcSvcIpAddr"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("testorg")]
        public string ShortOrgName {
            get {
                return ((string)(this["ShortOrgName"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("00:05:00")]
        public global::System.TimeSpan TCCRequestTimeout {
            get {
                return ((global::System.TimeSpan)(this["TCCRequestTimeout"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("20")]
        public int MaxThreadsToProcessTagFiles {
            get {
                return ((int)(this["MaxThreadsToProcessTagFiles"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("00:30:00")]
        public global::System.TimeSpan TagFileSubmitterTasksTimeout {
            get {
                return ((global::System.TimeSpan)(this["TagFileSubmitterTasksTimeout"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1500")]
        public int NumberOfFilesInPackage {
            get {
                return ((int)(this["NumberOfFilesInPackage"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Project Boundary (Issue)")]
        public string TCCSynchProjectBoundaryIssueFolder {
            get {
                return ((string)(this["TCCSynchProjectBoundaryIssueFolder"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Subscription (Issue)")]
        public string TCCSynchSubscriptionIssueFolder {
            get {
                return ((string)(this["TCCSynchSubscriptionIssueFolder"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Other... (Issue)")]
        public string TCCSynchOtherIssueFolder {
            get {
                return ((string)(this["TCCSynchOtherIssueFolder"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("7")]
        public byte TagFilesFolderLifeSpanInDays {
            get {
                return ((byte)(this["TagFilesFolderLifeSpanInDays"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://10.97.96.42:4001/v2/tagfile")]
        public string TagFileEndpoint {
            get {
                return ((string)(this["TagFileEndpoint"]));
            }
        }
    }
}
>>>>>>> webapi_support:TagFileHarvesterService/TagFileHarvesterServiceSettings.Designer.cs
