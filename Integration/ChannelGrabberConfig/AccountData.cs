using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;

namespace Integration.ChannelGrabberConfig {
	public class AccountData : IJsonable {
		#region public

		#region constructor

		public AccountData() : this(null) {} // constructor

		public AccountData(VendorInfo oVendorInfo) {
			VendorInfo = oVendorInfo;
			m_nAccountID = 0;
		} // constructor

		#endregion constructor

		#region method AccountTypeName

		/// <summary>
		/// Gets account type name ( V o l u s i o n, P l a y.com, etc).
		/// Spaces are inserted to hide this line from search :-)
		/// </summary>
		/// <returns>Account type name.</returns>
		public virtual string AccountTypeName() { return VendorInfo.Name; } // AccountTypeName

		#endregion method AccountTypeName

		#region method VerifyRegistrationInProgresss

		/// <summary>
		/// Verifies that Channel Grabber service returned valid data as a reply for
		/// account registration request.
		/// Stores account id internally (it can be retrieved by calling Id() later) if data
		/// is good.
		/// </summary>
		/// <exception cref="ConfigException">Throws in case of invalid data.</exception>
		/// <param name="doc">Data to check.</param>
		public void VerifyRegistrationInProgress(XmlDocument doc) {
			if (null == doc)
				throw new ConfigException("Service response is not defined.");

			if (null == doc.DocumentElement)
				throw new ConfigException("Service response is not well formatted.");

			IsMe(doc.DocumentElement, true);

			m_nAccountID = XmlUtil.GetInt(doc, IdNode);

			if (m_nAccountID < 1)
				throw new ConfigException("Wrong id in the server response.");
		} // VerifyRegistrationInProgress

		#endregion method VerifyRegistrationInProgresss

		#region method VerifyNotExist

		/// <summary>
		/// Verifies that this account does not exist in the list. Completes successfully if
		/// account not found.
		/// </summary>
		/// <exception cref="ConfigException">Throws if account exists.</exception>
		/// <param name="doc">List of accounts.</param>
		public void VerifyNotExist(XmlDocument doc) {
			ScanAccounts(doc, ThrowAlreadyExists);
		} // VerifyNotExist

		#endregion method VerifyNotExist

		#region method VerifyAccountID

		/// <summary>
		/// Verifies that this account exists in the list. Stores account id internally if
		/// account found.
		/// </summary>
		/// <exception cref="ConfigException">Throws if account does not exist.</exception>
		/// <param name="doc">List of accounts.</param>
		public void VerifyAccountID(XmlDocument doc) {
			m_nAccountID = 0;

			ScanAccounts(doc, SaveAccountID);

			if (m_nAccountID < 1)
				throw new ConfigException("Shop is not registered.");
		} // VerifyAccountID

		#endregion method VerifyAccountID

		#region method Id

		/// <summary>
		/// Returns account id retrieved by the last VerifyRegistationInProgress call.
		/// </summary>
		/// <returns>Account id.</returns>
		public int Id() {
			return m_nAccountID; 
		} // Id

		#endregion method Id

		#region method Validate

		/// <summary>
		/// Checks that supplied credentials are ok and shop has been registered successfully.
		/// </summary>
		/// <param name="doc">Service output for validity request.</param>
		public void Validate(XmlDocument doc) {
			if (null == doc)
				throw new ConfigException("Service response is not defined.");

			if (null == doc.DocumentElement)
				throw new ConfigException("Service response is not well formatted.");

			if (!XmlUtil.IsComplete(doc))
				throw new ConfigException("Validation is not complete.");

			if (!XmlUtil.IsEqual(doc, "validity", "true"))
				throw new ConfigException("Cannot validate: invalid credentials.");
		} // Validate

		#endregion method Validate

		#region method ToJson

		public virtual object ToJson() {
			var oOutput = new Dictionary<string, string>();

			foreach (FieldInfo fi in VendorInfo.SecurityData.Fields)
				oOutput[fi.NodeName] = GetPropertyValue(fi);

			return oOutput;
		} // ToJson

		#endregion method ToJson

		#region method ToString

		public override string ToString() {
			var args = new List<object>();

			for (int i = 1; i < VendorInfo.SecurityData.ToStringArguments.Count; i++)
				args.Add(GetPropertyValue(VendorInfo.SecurityData.ToStringArguments[i]));

			return string.Format(VendorInfo.SecurityData.ToStringArguments[0], args.ToArray());
		} // ToString

		#endregion method ToString

		#region properties

		public string Name { get; set; }
		public string URL { get; set; }
		public string Login { get; set; }
		public string Password { get; set; }
		public int LimitDays { get; set; }
		public string AuxLogin { get; set; }
		public string AuxPassword { get; set; }
		public int RealmID { get; set; }

		#endregion properties

		public VendorInfo VendorInfo { get; set; }

		#region method UniqueID

		public string UniqueID() {
			var oUniqueID = new SortedDictionary<int, string>();

			VendorInfo.SecurityData.Fields.ForEach(fi => {
				if (fi.UniqueIDPosition >= 0)
					oUniqueID[fi.UniqueIDPosition] = GetPropertyValue(fi);
			});

			return string.Join(":|:", oUniqueID.Values);
		} // UniqueID

		#endregion method UniqueID

		#endregion public

		#region private

		#region method IsMe

		private bool IsMe(XmlNode oNode, bool bThrowIfNot) {
			bool bResult = true;

			foreach (FieldInfo fi in VendorInfo.SecurityData.Fields) {
				if (fi.UniqueIDPosition < 0)
					continue;

				object oMyValue = GetPropertyValue(fi);
				string sReturnedValue = XmlUtil.GetString(oNode, fi.NodeName);

				if (oMyValue.ToString() != sReturnedValue) {
					bResult = false;

					if (bThrowIfNot)
						throw new ConfigException(string.Format("{0}'s value differs in the service response in {1} node.", fi.PropertyName, fi.NodeName));
				} // if
			} // foreach

			return bResult;
		} // IsMe

		#endregion method IsMe

		#region method ScanAccounts

		private void ScanAccounts(XmlDocument doc, Func<XmlNode, bool> onFound) {
			if (null == doc)
				throw new ConfigException("Service response is not defined.");

			if (null == doc.DocumentElement)
				throw new ConfigException("Service response is not well formatted.");

			foreach (XmlNode oNode in doc.DocumentElement.ChildNodes) {
				if (IsMe(oNode, false)) {
					if (!XmlUtil.IsEqual(oNode, "validity", "true"))
						throw new ConfigException("Cannot validate: invalid credentials.");

					if (onFound(oNode))
						break;
				} // if
			} // foreach
		} // ScanAccounts

		#endregion method ScanAccounts

		#region method ThrowAlreadyExists

		private bool ThrowAlreadyExists(XmlNode oNode) {
			throw new ConfigException("Shop already registered.");
		} // ThrowAlreadyExists

		#endregion method ThrowAlreadyExists

		#region method SaveAccountID

		private bool SaveAccountID(XmlNode oNode) {
			m_nAccountID = XmlUtil.GetInt(oNode, IdNode);
			return true;
		} // SaveAccountID

		#endregion method SaveAccountID

		#region method GetPropertyValue

		private string GetPropertyValue(FieldInfo fi) {
			return GetPropertyValue(fi.PropertyName);
		} // GetPropertyValue

		private string GetPropertyValue(string sPropertyName) {
			PropertyInfo prop = this.GetType().GetProperty(sPropertyName);
			return prop.GetGetMethod().Invoke(this, null).ToString();
		} // GetPropertyValue

		#endregion method GetPropertyValue

		private int m_nAccountID;

		private const string IdNode = "id";

		#endregion private
	} // class AccountData
} // namespace Integration.ChannelGrabberConfig
