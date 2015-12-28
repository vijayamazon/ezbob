namespace EzBob.Web.Code {
	using System;
	using System.Security.Principal;
	using System.Web;
	using System.Web.Security;
	using Ezbob.Backend.Models;
	using Ezbob.Logger;
	using EzBob.Web.Infrastructure;
	using EZBob.DatabaseLib.Model.Database;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;
	using log4net;
	using StructureMap;

	public class BrokerHelper {
		public static void SetAuth(string sLoginEmail, HttpContextBase oContext = null, string sRole = "Broker") {
			GenericPrincipal oNewUser;

			if (string.IsNullOrWhiteSpace(sLoginEmail)) {
				FormsAuthentication.SignOut();
				oNewUser = new GenericPrincipal(new GenericIdentity(string.Empty), null);
				ObjectFactory.GetInstance<IEzbobWorkplaceContext>().RemoveSessionOrigin();
			} else {
				FormsAuthentication.SetAuthCookie(sLoginEmail, true);
				oNewUser = new GenericPrincipal(new GenericIdentity(sLoginEmail), new [] { sRole });

				Uri requestUrl = (oContext != null) ? oContext.Request.Url : HttpContext.Current.Request.Url;
				CustomerOrigin uio = UiCustomerOrigin.Get(requestUrl);
				ObjectFactory.GetInstance<IEzbobWorkplaceContext>().SetSessionOrigin(uio.GetOrigin());
			} // if

			if (oContext == null)
				HttpContext.Current.User = oNewUser;
			else
				oContext.User = oNewUser;
		} // SetAuth

		public BrokerHelper(ServiceClient oServiceClient = null, ASafeLog oLog = null) {
			m_oLog = oLog ?? new SafeILog(LogManager.GetLogger(typeof(BrokerHelper)));
			m_oServiceClient = oServiceClient ?? new ServiceClient();
		} // constructor

		public bool IsBroker(string sContactEmail) {
			BoolActionResult bar = null;

			CustomerOrigin uio = UiCustomerOrigin.Get();

			if (!string.IsNullOrWhiteSpace(sContactEmail)) {
				try {
					m_oLog.Debug(
						"Checking whether '{0}' with origin '{1}' is a broker email.",
						sContactEmail,
						uio.Stringify()
					);

					bar = m_oServiceClient.Instance.IsBroker(sContactEmail, uio.CustomerOriginID);
				}
				catch (Exception e) {
					m_oLog.Warn(
						e,
						"Failed to determine whether '{0}' with origin '{1}' is a broker email.",
						sContactEmail,
						uio.Stringify()
					);
				} // try
			} // if

			var bIsBroker = (bar != null) && bar.Value;

			m_oLog.Debug(
				"'{0}' with origin '{2}' is {1}a broker email.",
				sContactEmail,
				bIsBroker ? "" : "not ",
				uio.Stringify()
			);

			return bIsBroker;
		} // IsBroker

		public BrokerProperties TryLogin(
			string sLoginEmail,
			string sPassword,
			string promotionName,
			DateTime? promotionPageVisitTime
		) {
			var uio = UiCustomerOrigin.Get();

			m_oLog.Debug("Trying to login as broker '{0}' with origin '{1}'...", sLoginEmail, uio.Stringify());

			BrokerPropertiesActionResult bp = null;

			try {
				bp = m_oServiceClient.Instance.BrokerLogin(
					sLoginEmail,
					new DasKennwort(sPassword), 
					promotionName,
					promotionPageVisitTime,
					uio.CustomerOriginID
				);
			} catch (Exception e) {
				m_oLog.Warn(
					e,
					"Error encountered while trying to login as a broker '{0}' with origin '{1}'.",
					sLoginEmail,
					uio.Stringify()
				);
				return null;
			} // try

			if ((bp != null) && (bp.Properties != null) && (bp.Properties.BrokerID > 0)) {
				SetAuth(sLoginEmail);

				m_oLog.Debug(
					"Succeeded to login as broker '{0}' with origin '{1}', authenticated name is '{2}'.",
					sLoginEmail,
					uio.Stringify(),
					HttpContext.Current.User.Identity.Name
				);
				return bp.Properties;
			} // if

			m_oLog.Warn("Failed to login as a broker '{0}' with origin '{1}'.", sLoginEmail, uio.Stringify());
			return null;
		} // TryLogin

		public void Logoff(string sCurrentLogin, HttpContextBase oContext) {
			m_oLog.Debug("Broker {0} signed out.", sCurrentLogin);
			SetAuth(null, oContext);
		} // Logoff

		private readonly ServiceClient m_oServiceClient;
		private readonly ASafeLog m_oLog;
	} // class BrokerHelper
} // namespace
