using System;
using System.Reflection;
using System.Xml;
namespace Integration.ChannelGrabberAPI {
	public abstract class AAccountData : AJsonable, IAccountData {
		#region public

		#region method VerifyRegistrationInProgresss

		public void VerifyRegistrationInProgress(XmlDocument doc) {
			if (null == doc)
				throw new ChannelGrabberApiException("Service response is not defined.");

			if (null == doc.DocumentElement)
				throw new ChannelGrabberApiException("Service response is not well formatted.");

			PropertyInfo[] oPropList = GetType().GetProperties(
				BindingFlags.Public | BindingFlags.Instance
			);

			foreach (var p in oPropList) {
				if (!p.CanRead || (typeof(string) != p.PropertyType))
					continue;

				if (p.GetValue(this, null).ToString() != API.GetString(doc, p.Name))
					throw new ChannelGrabberApiException(string.Format("{0}'s value differs in the service response.", p.Name));
			} // foreach

			m_nAccountID = API.GetInt(doc, IdNode);

			if (m_nAccountID < 1)
				throw new ChannelGrabberApiException("Wrong id in the server response.");
		} // VerifyRegistrationInProgress

		#endregion method VerifyRegistrationInProgresss

		#region method VerifyNotExist

		public void VerifyNotExist(XmlDocument doc) {
			ScanAccounts(doc, ThrowAlreadyExists);
		} // VerifyNotExist

		#endregion method VerifyNotExist

		#region method VerifyAccountID

		public void VerifyAccountID(XmlDocument doc) {
			m_nAccountID = 0;

			ScanAccounts(doc, SaveAccountID);

			if (m_nAccountID < 1)
				throw new ChannelGrabberApiException("Shop is not registered.");
		} // VerifyAccountID

		#endregion method VerifyAccountID

		#region method Id

		public int Id() {
			return m_nAccountID; 
		} // Id

		#endregion method Id

		#region method Validate

		public void Validate(XmlDocument doc) {
			if (null == doc)
				throw new ChannelGrabberApiException("Service response is not defined.");

			if (null == doc.DocumentElement)
				throw new ChannelGrabberApiException("Service response is not well formatted.");

			if (!API.IsComplete(doc))
				throw new ChannelGrabberApiException("Validation is not complete.");

			if (!API.IsEqual(doc, "validity", "true"))
				throw new ChannelGrabberApiException("Cannot validate: invalid credentials.");
		} // Validate

		#endregion method Validate

		#endregion public

		#region protected

		#region constructor

		protected AAccountData() {
			m_nAccountID = 0;
		} // constructor

		#endregion constructor

		#region method VerifyNotEqual

		protected abstract bool IsMe(XmlNode oNode);

		#endregion method VerifyNotEqual

		#endregion protected

		#region private

		#region method ScanAccounts

		private void ScanAccounts(XmlDocument doc, Func<XmlNode, bool> onFound) {
			if (null == doc)
				throw new ChannelGrabberApiException("Service response is not defined.");

			if (null == doc.DocumentElement)
				throw new ChannelGrabberApiException("Service response is not well formatted.");

			foreach (XmlNode oNode in doc.DocumentElement.ChildNodes) {
				if (IsMe(oNode)) {
					if (onFound(oNode))
						break;
				} // if
			} // foreach
		} // ScanAccounts

		#endregion method ScanAccounts

		#region method ThrowAlreadyExists

		private bool ThrowAlreadyExists(XmlNode oNode) {
			throw new ChannelGrabberApiException("Shop already registered.");
		} // ThrowAlreadyExists

		#endregion method ThrowAlreadyExists

		#region method SaveAccountID

		private bool SaveAccountID(XmlNode oNode) {
			m_nAccountID = API.GetInt(oNode, IdNode);
			return true;
		} // SaveAccountID

		#endregion method SaveAccountID

		private int m_nAccountID;

		private const string IdNode = "id";

		#endregion private
	} // class AAccountData
} // namespace Integration.ChannelGrabberAPI
