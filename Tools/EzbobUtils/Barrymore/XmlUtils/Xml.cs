namespace Ezbob.Utils.XmlUtils {
	using System;
	using System.Xml;
	using Exceptions;

	public static class Xml {

		public static XmlDocument Parse(string sXml) {
			if (string.IsNullOrWhiteSpace(sXml))
				throw new SeldenException("No XML string specified.", new ArgumentNullException("sXml"));

			var doc = new XmlDocument();

			try {
				doc.LoadXml(sXml);
			}
			catch (Exception e) {
				throw new SeldenException("Failed to parse XML string.", e);
			} // try

			if (ReferenceEquals(doc.DocumentElement, null))
				throw new SeldenException("No root element found in the parsed document.");

			return doc;
		} // Parse

		public static XmlNode ParseRoot(string sXml) {
			return Parse(sXml).DocumentElement;
		} // ParseRoot

	} // class Xml
} // namespace
