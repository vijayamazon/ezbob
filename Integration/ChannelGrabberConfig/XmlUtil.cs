using System;
using System.Xml;

namespace Integration.ChannelGrabberConfig {
	#region class XmlUtil

	public class XmlUtil {
		#region public

		public const string IdNode = "id";

		#region method GetString

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

		#endregion method GetString

		#region method GetInt

		public static int GetInt(XmlDocument doc, string sNodeName) {
			return XmlConvert.ToInt32(XmlUtil.GetString(doc, sNodeName));
		} // GetInt

		public static int GetInt(XmlNode oNode, string sNodeName) {
			return XmlConvert.ToInt32(XmlUtil.GetString(oNode, sNodeName));
		} // GetInt

		#endregion method GetInt

		#region method GetDouble

		public static double GetDouble(XmlDocument doc, string sNodeName) {
			return XmlConvert.ToDouble(XmlUtil.GetString(doc, sNodeName));
		} // GetDouble

		public static double GetDouble(XmlNode oNode, string sNodeName) {
			return XmlConvert.ToDouble(XmlUtil.GetString(oNode, sNodeName));
		} // GetDouble

		#endregion method GetDouble

		#region method GetDate

		public static DateTime GetDate(XmlDocument doc, string sNodeName) {
			return XmlConvert.ToDateTime(XmlUtil.GetString(doc, sNodeName), XmlDateTimeSerializationMode.Utc);
		} // GetDate

		public static DateTime GetDate(XmlNode oNode, string sNodeName) {
			return XmlConvert.ToDateTime(XmlUtil.GetString(oNode, sNodeName), XmlDateTimeSerializationMode.Utc);
		} // GetDate

		#endregion method GetDate

		#region method IsComplete

		public static bool IsComplete(XmlDocument doc) {
			return XmlUtil.IsEqual(doc, NodeName.Status, NodeValue.Complete);
		} // IsComplete

		#endregion method IsComplete

		#region method IsError

		public static bool IsError(XmlDocument doc) {
			return XmlUtil.IsEqual(doc, NodeName.Status, NodeValue.Error);
		} // IsError

		#endregion method IsError

		#region method GetError

		public static string GetError(XmlDocument doc) {
			return XmlUtil.GetString(doc, NodeName.Error);
		} // IsError

		#endregion method GetError

		#region method IsEqual

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

		#endregion method IsEqual

		#endregion public

		#region private

		#region enum NodeName

		enum NodeName {
			Status,
			Error
		} // enum NodeName

		#endregion enum NodeName

		#region enum NodeValue

		enum NodeValue {
			Complete,
			Error
		} // enum NodeValue

		#endregion enum NodeValue

		#endregion private
	} // class XmlUtil

	#endregion class XmlUtil
} // namespace Integration.ChannelGrabberConfig
