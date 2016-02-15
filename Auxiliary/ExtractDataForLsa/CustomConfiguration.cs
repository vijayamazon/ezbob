namespace ExtractDataForLsa {
	using System.Configuration;

	public class CustomConfiguration : ConfigurationSection {
		public static CustomConfiguration Load() {
			return (CustomConfiguration)System.Configuration.ConfigurationManager.GetSection("customConfiguration");
		} // Load

		[ConfigurationProperty(TargetPathCfgName, IsRequired = true)]
		public string TargetPath {
			get { return (string)this[TargetPathCfgName]; }
			set { this[TargetPathCfgName] = value; }
		} // TargetPath

		[ConfigurationProperty(DropboxRootPathCfgName, IsRequired = true)]
		public string DropboxRootPath {
			get { return (string)this[DropboxRootPathCfgName]; }
			set { this[DropboxRootPathCfgName] = value; }
		} // DropboxRootPath

		[ConfigurationProperty(ConfiguredEnvironmentCfgName, IsRequired = true)]
		public ConfiguredEnvironment Environment {
			get { return (ConfiguredEnvironment)this[ConfiguredEnvironmentCfgName]; }
			set { this[ConfiguredEnvironmentCfgName] = value; }
		} // Environment

		private const string DropboxRootPathCfgName = "dropboxRoot";
		private const string ConfiguredEnvironmentCfgName = "environment";
		private const string TargetPathCfgName = "targetPath";
	} // class CustomConfiguration
} // namespace
