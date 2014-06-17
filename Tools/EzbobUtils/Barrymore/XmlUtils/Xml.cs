namespace Ezbob.Utils.XmlUtils {
	using System;
	using System.Xml;
	using Exceptions;

	public static class Xml {
		#region method Parse

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

		#endregion method Parse

		#region method ParseRoot

		public static XmlNode ParseRoot(string sXml) {
			return Parse(sXml).DocumentElement;
		} // ParseRoot

		#endregion method ParseRoot
	} // class Xml
} // namespace
