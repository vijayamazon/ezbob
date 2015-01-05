namespace EzBob.Web.Areas.Broker.Controllers {

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Web.Mvc;
	using Code;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;

	public partial class BrokerHomeController : Controller {

		static BrokerHomeController() {
			ms_oLog = new SafeILog(typeof(BrokerHomeController));
			m_oDB = DbConnectionGenerator.Get(ms_oLog);
		} // static constructor

		private BrokerForJsonResult IsAuth(string sRequestDescription, string sContactEmail) {
			return IsAuth<BrokerForJsonResult>(sRequestDescription, sContactEmail);
		} // IsAuth
		
		/// <summary>
		/// Checks if broker is logged in session if OK returned null else return error message
		/// </summary>
		private T IsAuth<T>(string sRequestDescription, string sContactEmail) where T : BrokerForJsonResult {
			if (!User.Identity.IsAuthenticated || (User.Identity.Name != sContactEmail)) {
				ms_oLog.Alert(
					"{0} request with contact email {1}: {2}.",
					sRequestDescription,
					sContactEmail,
					User.Identity.IsAuthenticated ? "authorised as " + User.Identity.Name : "not authenticated"
				);

				ConstructorInfo ci = typeof(T).GetConstructors().FirstOrDefault();

				if (ci == null) {
					ms_oLog.Alert("Failed to find a constructor for {0}.", typeof(T));
					return (T)new BrokerForJsonResult("Not authorised.", false) { is_auth = false };
				} // if

				int nParamCount = ci.GetParameters().Length;

				var oConstructorArgs = new object[nParamCount];

				for (int i = 0; i < nParamCount; i++)
					oConstructorArgs[i] = null;

				T oResult = (T)ci.Invoke(oConstructorArgs);

				oResult.error = "Not authorised.";
				oResult.is_auth = false;

				return oResult;
			} // if

			return null;
		} // IsAuth

		private readonly ServiceClient m_oServiceClient;
		private readonly BrokerHelper m_oHelper;

		private static readonly ASafeLog ms_oLog;
		private static readonly AConnection m_oDB;

		// ReSharper disable InconsistentNaming

		public class BrokerForJsonResult {

			public static implicit operator JsonResult(BrokerForJsonResult oResult) {
				ms_oLog.Debug(
					"Controller output:\n\ttype: {0}\n\terror msg: {1}",
					oResult.GetType(), oResult.error
				);

				return new JsonResult {
					Data = oResult,
					ContentType = null,
					ContentEncoding = null,
					JsonRequestBehavior = JsonRequestBehavior.AllowGet,
				};
			} // operator JsonResult

			public BrokerForJsonResult(string sErrorMsg = "", bool? bExplicitSuccess = null) {
				m_bIsAuth = true;
				SetError(sErrorMsg);
				m_bSuccess = bExplicitSuccess;
			} // constructor

			public virtual bool success {
				get { return m_bSuccess.HasValue ? m_bSuccess.Value : string.IsNullOrWhiteSpace(error); }
			} // success

			private bool? m_bSuccess;

			public virtual string error {
				get { return m_sError; }
				set { SetError(value); }
			} // error

			private void SetError(string sError) {
				m_sError = (sError ?? string.Empty).Trim();
			} // SetError

			private string m_sError;

			public virtual bool is_auth {
				get { return m_bIsAuth; }
				set { m_bIsAuth = value; }
			} // is_auth

			private bool m_bIsAuth;

			public virtual string antiforgery_token { get; set; } // antiforgery_token

		} // BrokerForJsonResult

		public class PropertiesBrokerForJsonResult : BrokerForJsonResult {
			public PropertiesBrokerForJsonResult(
				string sErrorMsg = "",
				bool? bExplicitSuccess = null,
				BrokerProperties oProperties = null
			)
				: base(sErrorMsg, bExplicitSuccess) {
				properties = oProperties ?? new BrokerProperties();
			} // constructor

			public virtual BrokerProperties properties { get; private set; } // customers
		} // PropertiesBrokerForJsonResult

		public class CustomerListBrokerForJsonResult : BrokerForJsonResult {
			public CustomerListBrokerForJsonResult(
				string sErrorMsg = "",
				bool? bExplicitSuccess = null,
				BrokerCustomerEntry[] oCustomers = null
			)
				: base(sErrorMsg, bExplicitSuccess) {
				customers = oCustomers ?? new BrokerCustomerEntry[0];
			} // constructor

			public virtual BrokerCustomerEntry[] customers { get; private set; } // customers
		} // CustomerListBrokerForJsonResult

		public class CustomerDetailsBrokerForJsonResult : BrokerForJsonResult {
			public CustomerDetailsBrokerForJsonResult(
				string sErrorMsg = "",
				bool? bExplicitSuccess = null,
				BrokerCustomerDetails oDetails = null,
				Esigner[] oPotentialSigners = null 
			) : base(sErrorMsg, bExplicitSuccess) {
				crm_data = oDetails == null ? null : oDetails.CrmData;
				personal_data = oDetails == null ? null : oDetails.PersonalData;
				potential_signers = oPotentialSigners ?? new Esigner[0];
			} // constructor

			public virtual List<BrokerCustomerCrmEntry> crm_data { get; private set; }

			public virtual BrokerCustomerPersonalData personal_data { get; private set; }

			public virtual Esigner[] potential_signers { get; private set; }
		} // CustomerDetailsBrokerForJsonResult

		public class FileListBrokerForJsonResult : BrokerForJsonResult {
			public FileListBrokerForJsonResult(string sErrorMsg = "", bool? bExplicitSuccess = null, BrokerCustomerFile[] oFileList = null)
				: base(sErrorMsg, bExplicitSuccess) {
				file_list = oFileList ?? new BrokerCustomerFile[0];
			} // constructor

			public virtual BrokerCustomerFile[] file_list { get; private set; } // file_list
		} // FileListBrokerForJsonResult

		public class SignedTermsBrokerForJsonResult : BrokerForJsonResult {
			public SignedTermsBrokerForJsonResult(
				string sErrorMsg = "",
				bool? bExplicitSuccess = null,
				string sTerms = "",
				string sSignedTime = ""
			) : base(sErrorMsg, bExplicitSuccess) {
				terms = sTerms;
				signedTime = sSignedTime;
			} // constructor

			public virtual string terms { get; private set; } // terms

			public virtual string signedTime { get; private set; } // signedTime
		} // SignedTermsBrokerForJsonResult

		// ReSharper restore InconsistentNaming

		private class MarketingFiles {
			public MarketingFiles(ServiceClient oServiceClient) {
				IEnumerable<FileDescription> oFiles = Load(oServiceClient);

				m_oAlphabetical = new SortedDictionary<string, FileDescription>();

				foreach (FileDescription fd in oFiles)
					m_oAlphabetical[fd.FileID] = fd;
			} // constructor

			public FileDescription Find(string sKey) {
				if (m_oAlphabetical == null)
					return null;

				return m_oAlphabetical.ContainsKey(sKey) ? m_oAlphabetical[sKey] : null;
			} // Find

			private readonly SortedDictionary<string, FileDescription> m_oAlphabetical;

			private IEnumerable<FileDescription> Load(ServiceClient oServiceClient) {
				ms_oLog.Debug("Loading broker marketing files...");

				BrokerStaticDataActionResult flar = null;

				try {
					flar = oServiceClient.Instance.BrokerLoadStaticData(true);
				}
				catch (Exception e) {
					ms_oLog.Alert(e, "Failed to load broker marketing files.");
				} // try

				FileDescription[] oResult = (flar == null ? null : flar.Files) ?? new FileDescription[0];

				ms_oLog.Debug("Loading broker marketing files complete, {0} file(s) loaded.", oResult.Length);

				return oResult;
			} // Load
		} // class MarketingFiles

	} // class BrokerHomeController
} // namespace
