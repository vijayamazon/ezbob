namespace EzBob.Web.Code {
	using System;
	using System.Web.Mvc;
	using System.Web.Security;
	using ApplicationMng.Model;
	using EzServiceReference;
	using Ezbob.Logger;
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

			try {
				m_oLog.Debug("Checking whether '{0}' is a broker email.", sContactEmail);

				bar = m_oServiceClient.Instance.IsBroker(sContactEmail);
			}
			catch (Exception e) {
				m_oLog.Warn(e, "Failed to determine whether '{0}' is a broker email.", sContactEmail);
			} // try

			var bIsBroker = (bar != null) && bar.Value;

			m_oLog.Debug("'{0}' is {1}a broker email.", sContactEmail, bIsBroker ? "" : "not ");

			return bIsBroker;
		} // IsBroker

		#endregion method IsBroker

		#region method TryLogin

		public bool TryLogin(string sLoginEmail, string sPassword) {
			m_oLog.Debug("Trying to login as broker '{0}'...", sLoginEmail);

			try {
				m_oServiceClient.Instance.BrokerLogin(sLoginEmail, sPassword);
			}
			catch (Exception e) {
				m_oLog.Alert(e, "Failed to login as a broker '{0}'.", sLoginEmail);
				return false;
			} // try

			FormsAuthentication.SetAuthCookie(sLoginEmail, true);

			m_oLog.Debug("Succeeded to login as broker '{0}'.", sLoginEmail);

			return true;
		} // TryLogin

		#endregion method TryLogin

		#endregion public

		#region private

		private readonly ServiceClient m_oServiceClient;
		private readonly ASafeLog m_oLog;

		#endregion private
	} // class BrokerHelper
} // namespace
