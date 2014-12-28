namespace IMailLib.Helpers {
	using System.Xml;

	public static class XmlNodeExtensions {
		public static string ToString(this XmlNode node, int indentCharsNum) {
			using (var sw = new System.IO.StringWriter()) {
				using (var xw = new System.Xml.XmlTextWriter(sw)) {
					xw.Formatting = System.Xml.Formatting.Indented;
					xw.Indentation = indentCharsNum;
					node.WriteContentTo(xw);
				}
				return sw.ToString();
			}
		}
	} // class XmlNodeExtensions
} // namespace Ezbob.Utils.XmlUtils
