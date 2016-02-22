namespace ExtractDataForLsa {
	using System.Configuration;
	using Ezbob.Context;

	public class ConfiguredEnvironment : ConfigurationElement {
		[ConfigurationProperty(NameCfgName, IsRequired = true)]
		public Name Name {
			get { return (Name)this[NameCfgName]; }
			set { this[NameCfgName] = value; }
		} // Name

		[ConfigurationProperty(VariantCfgName, IsRequired = false)]
		public string Variant {
			get { return (string)this[VariantCfgName]; }
			set { this[VariantCfgName] = value; }
		} // Variant

		private const string NameCfgName = "name";
		private const string VariantCfgName = "variant";
	} // class ConfiguredEnvironment
} // namespace
