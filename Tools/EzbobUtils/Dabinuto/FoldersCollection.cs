namespace Ezbob.Dabinuto {
	using System.Configuration;

	[ConfigurationCollection(typeof(FolderElement))]
	public class FoldersCollection : ConfigurationElementCollection {
		protected override ConfigurationElement CreateNewElement() {
			return new FolderElement();
		} // CreateNewElement

		protected override object GetElementKey(ConfigurationElement element) {
			return ((FolderElement)element).Name;
		} // GetElementKey

		public FolderElement this[int idx] {
			get { return (FolderElement)BaseGet(idx); }
		} // indexer
	} // class FoldersCollection
} // namespace
