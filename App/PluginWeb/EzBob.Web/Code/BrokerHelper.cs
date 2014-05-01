namespace EzBob.Web.Code {
	using System;
	using System.Security.Principal;
	using System.Web;
	using System.Web.Security;
	using Ezbob.Backend.Models;
	using Ezbob.Logger;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;
	using log4net;

	public class BrokerHelper {
		#region public

		#region constructor

		public BrokerHelper(ServiceClient oServiceClient = null, ASafeLog oLog = null) {
			m_oLog = oLog ?? new SafeILog(LogManager.GetLogger(typeof(BrokerHelper)));
			m_oServiceClient = oServiceClient ?? new ServiceClient();
		} // constructor

		#endregion constructor

		#region method IsBroker

		public bool IsBroker(string sContactEmail) {
			BoolActionResult bar = null;

			if (!string.IsNullOrWhiteSpace(sContactEmail)) {
				try {
					m_oLog.Debug("Checking whether '{0}' is a broker email.", sContactEmail);

					bar = m_oServiceClient.Instance.IsBroker(sContactEmail);
				}
				catch (Exception e) {
					m_oLog.Warn(e, "Failed to determine whether '{0}' is a broker email.", sContactEmail);
				} // try
			} // if

			var bIsBroker = (bar != null) && bar.Value;

			m_oLog.Debug("'{0}' is {1}a broker email.", sContactEmail, bIsBroker ? "" : "not ");

			return bIsBroker;
		} // IsBroker

		#endregion method IsBroker

		#region method TryLogin

		public BrokerProperties TryLogin(string sLoginEmail, string sPassword) {
			m_oLog.Debug("Trying to login as broker '{0}'...", sLoginEmail);

			BrokerPropertiesActionResult bp = null;

			try {
				bp = m_oServiceClient.Instance.BrokerLogin(sLoginEmail, sPassword);
			}
			catch (Exception e) {
				m_oLog.Alert(e, "Failed to login as a broker '{0}'.", sLoginEmail);
				return null;
			} // try

			if ((bp != null) && (bp.Properties != null) && (bp.Properties.BrokerID > 0)) {
				FormsAuthentication.SetAuthCookie(sLoginEmail, true);
				m_oLog.Debug("Succeeded to login as broker '{0}'.", sLoginEmail);
				return bp.Properties;
			} // if

			m_oLog.Alert("Failed to login as a broker '{0}'.", sLoginEmail);
			return null;
		} // TryLogin

		#endregion method TryLogin

		#region method Logoff

		public void Logoff(string sCurrentLogin, HttpContextBase oContext) {
			m_oLog.Debug("Broker {0} signed out.", sCurrentLogin);
			FormsAuthentication.SignOut();
			oContext.User = new GenericPrincipal(new GenericIdentity(string.Empty), null);
		} // Logoff

		#endregion method Logoff

		#endregion public

		#region private

		private readonly ServiceClient m_oServiceClient;
		private readonly ASafeLog m_oLog;

		#endregion private
	} // class BrokerHelper
} // namespace
