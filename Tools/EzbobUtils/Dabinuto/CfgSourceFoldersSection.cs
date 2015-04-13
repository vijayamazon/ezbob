namespace Ezbob.Dabinuto {
	using System.Configuration;

	class CfgSourceFoldersSection : ConfigurationSection {
		[ConfigurationProperty("Folders")]
		public FoldersCollection Folders {
			get { return (FoldersCollection)base["Folders"]; }
		} // Folders
	} // class CfgSourceFoldersSection
} // namespace
