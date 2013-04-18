using System.Reflection;
using System.Xml;

namespace Integration.ChannelGrabberAPI {
	public interface IAccountData : IJsonable {
		void VerifyRegistrationInProgress(XmlDocument doc);
		int Id();
		void Validate(XmlDocument doc);
	} // IAccountData

	public abstract class AAccountData : AJsonable, IAccountData {
		protected AAccountData() {
			m_nAccountID = 0;
		} // constructor

		public void VerifyRegistrationInProgress(XmlDocument doc) {
			if (null == doc)
				throw new ChannelGrabberApiException("Service response is not defined.");

			if (null == doc.DocumentElement)
				throw new ChannelGrabberApiException("Service response is not well formatted.");

			PropertyInfo[] oPropList = this.GetType().GetProperties(
				BindingFlags.Public | BindingFlags.Instance
			);

			foreach (var p in oPropList) {
				if (!p.CanRead || (typeof(string) != p.PropertyType))
					continue;

				if (p.GetValue(this, null).ToString() != this.GetValue(doc, p.Name))
					throw new ChannelGrabberApiException(string.Format("{0}'s value differs in the service response.", p.Name));
			} // foreach

			m_nAccountID = XmlConvert.ToInt32(this.GetValue(doc, "id"));

			if (m_nAccountID < 1)
				throw new ChannelGrabberApiException("Wrong id in the server response.");
		} // VerifyRegistrationInProgress

		public int Id() {
			return m_nAccountID; 
		} // Id

		public void Validate(XmlDocument doc) {
			if (null == doc)
				throw new ChannelGrabberApiException("Service response is not defined.");

			if (null == doc.DocumentElement)
				throw new ChannelGrabberApiException("Service response is not well formatted.");

			if ("complete" != this.GetValue(doc, "status").ToLower())
				throw new ChannelGrabberApiException("Validation is not complete.");

			if ("true" != this.GetValue(doc, "validity").ToLower())
				throw new ChannelGrabberApiException("Cannot validate: invalid credentials.");
		} // Validate

		private string GetValue(XmlDocument doc, string sNodeName) {
			XmlNode oNode = doc.DocumentElement.SelectSingleNode(sNodeName);

			if (null == oNode)
				throw new ChannelGrabberApiException(string.Format("{0} is not found in the service response.", sNodeName));

			return oNode.InnerText;
		} // GetValue

		private int m_nAccountID;
	} // class AAccountData

	#region class VolusionAccountData

	public class VolusionAccountData : AAccountData {
		public string name     { get; set; }
		public string endpoint { get; set; }
		public string username { get; set; }
		public string password { get; set; }

		public override string ToString() {
			return string.Format("{0} ({1} @ {2})", name, username, endpoint);
		} // ToString
	} // class VolusionAccountData

	#endregion class VolusionAccountData
} // namespace Integration.ChannelGrabberAPI
