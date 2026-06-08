namespace Gigapixel_Zipper.Properties {
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.0.3.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        public static Settings Default { get { return defaultInstance; } }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string ZipLocation { get { return (string)this["ZipLocation"]; } set { this["ZipLocation"] = value; } }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string GigapixelLocation { get { return (string)this["GigapixelLocation"]; } set { this["GigapixelLocation"] = value; } }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("400*400")]
        public string IgnoredSize { get { return (string)this["IgnoredSize"]; } set { this["IgnoredSize"] = value; } }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("preview")]
        public string IgnoredWords { get { return (string)this["IgnoredWords"]; } set { this["IgnoredWords"] = value; } }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("50")]
        public int BatchSize { get { return (int)this["BatchSize"]; } set { this["BatchSize"] = value; } }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string PhotoAIZipLocation { get { return (string)this["PhotoAIZipLocation"]; } set { this["PhotoAIZipLocation"] = value; } }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("C:\\Program Files\\Topaz Labs LLC\\Topaz Photo\\Topaz Photo.exe")]
        public string PhotoAIAppLocation { get { return (string)this["PhotoAIAppLocation"]; } set { this["PhotoAIAppLocation"] = value; } }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("400*400")]
        public string PhotoAIIgnoredSize { get { return (string)this["PhotoAIIgnoredSize"]; } set { this["PhotoAIIgnoredSize"] = value; } }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("preview")]
        public string PhotoAIIgnoredWords { get { return (string)this["PhotoAIIgnoredWords"]; } set { this["PhotoAIIgnoredWords"] = value; } }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("5")]
        public int PhotoAIBatchSize { get { return (int)this["PhotoAIBatchSize"]; } set { this["PhotoAIBatchSize"] = value; } }
    }
}
