namespace Ezbob.Utils.XmlUtils {
	using System.Xml;

	public static class XmlNodeExtensions {

		public static XmlNode Offspring(this XmlNode oRoot, NameList oChildNames) {
			if (ReferenceEquals(oRoot, null))
				return null;

			if (ReferenceEquals(oChildNames, null))
				return oRoot;

			if (oChildNames.Count < 1)
				return oRoot;

			XmlNode oCur = oRoot;

			foreach (string sName in oChildNames.Values) {
				oCur = oCur[sName];

				if (ReferenceEquals(oCur, null))
					return null;
			} // foreach

			return oCur;
		} // Offspring

		public static XmlNode Offspring(this XmlNode oRoot, params string[] sChildNames) {
			return Offspring(oRoot, new NameList(sChildNames));
		} // Offspring

	} // class XmlNodeExtensions
} // namespace Ezbob.Utils.XmlUtils
