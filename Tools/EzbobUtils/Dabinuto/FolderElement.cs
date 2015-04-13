namespace Ezbob.Dabinuto {
	using System.Configuration;

	public class FolderElement : ConfigurationElement {
		[ConfigurationProperty("name", DefaultValue = "", IsKey = true, IsRequired = true)]
		public string Name {
			get { return (string)base["name"]; }
			set { base["name"] = value; }
		} // Name
	} // class FolderElement
} // namespace
