using System;
using System.Xml;

namespace Integration.ChannelGrabberAPI {
	#region class API

	class API {
		#region public

		public const string IdNode = "id";

		#region method GetString

		public static string GetString(XmlDocument doc, string sNodeName) {
			return GetString(doc.DocumentElement, sNodeName);
		} // GetString

		public static string GetString(XmlNode oParent, string sNodeName) {
			if (null == oParent)
				throw new ChannelGrabberApiException("Parent node not found.");

			XmlNode oNode = oParent.SelectSingleNode(sNodeName);

			if (null == oNode)
				throw new ChannelGrabberApiException(string.Format("{0} is not found in the service response.", sNodeName));

			return oNode.InnerText;
		} // GetString

		#endregion method GetString

		#region method GetInt

		public static int GetInt(XmlDocument doc, string sNodeName) {
			return XmlConvert.ToInt32(API.GetString(doc, sNodeName));
		} // GetInt

		public static int GetInt(XmlNode oNode, string sNodeName) {
			return XmlConvert.ToInt32(API.GetString(oNode, sNodeName));
		} // GetInt

		#endregion method GetInt

		#region method GetDouble

		public static double GetDouble(XmlDocument doc, string sNodeName) {
			return XmlConvert.ToDouble(API.GetString(doc, sNodeName));
		} // GetDouble

		public static double GetDouble(XmlNode oNode, string sNodeName) {
			return XmlConvert.ToDouble(API.GetString(oNode, sNodeName));
		} // GetDouble

		#endregion method GetDouble

		#region method GetDate

		public static DateTime GetDate(XmlDocument doc, string sNodeName) {
			return XmlConvert.ToDateTime(API.GetString(doc, sNodeName), XmlDateTimeSerializationMode.Utc);
		} // GetDate

		public static DateTime GetDate(XmlNode oNode, string sNodeName) {
			return XmlConvert.ToDateTime(API.GetString(oNode, sNodeName), XmlDateTimeSerializationMode.Utc);
		} // GetDate

		#endregion method GetDate

		#region method IsComplete

		public static bool IsComplete(XmlDocument doc) {
			return API.IsEqual(doc, "status", "complete");
		} // IsComplete

		#endregion method IsComplete

		#region method IsEqual

		public static bool IsEqual(XmlDocument doc, string sNodeName, string sValue) {
			return sValue.ToLower() == API.GetString(doc, sNodeName).ToLower();
		} // IsEqual

		#endregion method IsEqual

		#endregion public
	} // class API

	#endregion class API
} // namespace Integration.ChannelGrabberAPI
