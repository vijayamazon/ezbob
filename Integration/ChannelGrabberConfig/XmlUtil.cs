using System;
using System.Xml;

namespace Integration.ChannelGrabberConfig {

	public class XmlUtil {

		public const string IdNode = "id";

		public static string GetString(XmlDocument doc, string sNodeName) {
			return GetString(doc.DocumentElement, sNodeName);
		} // GetString

		private static string GetString(XmlDocument doc, NodeName nNodeName) {
			return GetString(doc.DocumentElement, nNodeName.ToString().ToLower());
		} // GetString

		public static string GetString(XmlNode oParent, string sNodeName) {
			if (null == oParent)
				throw new ConfigException("Parent node not found.");

			XmlNode oNode = oParent.SelectSingleNode(sNodeName);

			if (null == oNode)
				throw new ConfigException(string.Format("{0} is not found in the service response.", sNodeName));

			return oNode.InnerText;
		} // GetString

		public static int GetInt(XmlDocument doc, string sNodeName) {
			return XmlConvert.ToInt32(XmlUtil.GetString(doc, sNodeName));
		} // GetInt

		public static int GetInt(XmlNode oNode, string sNodeName) {
			return XmlConvert.ToInt32(XmlUtil.GetString(oNode, sNodeName));
		} // GetInt

		public static double GetDouble(XmlDocument doc, string sNodeName) {
			return XmlConvert.ToDouble(XmlUtil.GetString(doc, sNodeName));
		} // GetDouble

		public static double GetDouble(XmlNode oNode, string sNodeName) {
			return XmlConvert.ToDouble(XmlUtil.GetString(oNode, sNodeName));
		} // GetDouble

		public static DateTime GetDate(XmlDocument doc, string sNodeName) {
			return XmlConvert.ToDateTime(XmlUtil.GetString(doc, sNodeName), XmlDateTimeSerializationMode.Utc);
		} // GetDate

		public static DateTime GetDate(XmlNode oNode, string sNodeName) {
			return XmlConvert.ToDateTime(XmlUtil.GetString(oNode, sNodeName), XmlDateTimeSerializationMode.Utc);
		} // GetDate

		public static bool IsComplete(XmlDocument doc) {
			return XmlUtil.IsEqual(doc, NodeName.Status, NodeValue.Complete);
		} // IsComplete

		public static bool IsError(XmlDocument doc) {
			return XmlUtil.IsEqual(doc, NodeName.Status, NodeValue.Error);
		} // IsError

		public static string GetError(XmlDocument doc) {
			return XmlUtil.GetString(doc, NodeName.Error);
		} // IsError

		private static bool IsEqual(XmlDocument doc, NodeName nNodeName, NodeValue nValue) {
			return XmlUtil.IsEqual(doc.DocumentElement, nNodeName.ToString().ToLower(), nValue.ToString().ToLower());
		} // IsEqual

		private static bool IsEqual(XmlNode oNode, NodeName nNodeName, NodeValue nValue) {
			return XmlUtil.IsEqual(oNode, nNodeName.ToString().ToLower(), nValue.ToString().ToLower());
		} // IsEqual

		public static bool IsEqual(XmlDocument doc, string sNodeName, string sValue) {
			return XmlUtil.IsEqual(doc.DocumentElement, sNodeName, sValue);
		} // IsEqual

		public static bool IsEqual(XmlNode oNode, string sNodeName, string sValue) {
			return sValue.ToLower() == XmlUtil.GetString(oNode, sNodeName).ToLower();
		} // IsEqual

		enum NodeName {
			Status,
			Error
		} // enum NodeName

		enum NodeValue {
			Complete,
			Error
		} // enum NodeValue

	} // class XmlUtil

} // namespace Integration.ChannelGrabberConfig
