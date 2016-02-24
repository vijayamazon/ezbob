namespace EzBob.Web.Code {
	using System.Configuration;
	using System.Web;
	using System.Web.Configuration;
	using System.Web.Mvc;
	using Ezbob.Database;
	using Ezbob.Logger;

	public static class ControllerExt {

		public static string GetRemoteIP(this Controller oController) {
			string ip = oController.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

			if (string.IsNullOrEmpty(ip))
				ip = oController.Request.ServerVariables["REMOTE_ADDR"];

			ip = (ip ?? string.Empty).Trim();

			if (string.IsNullOrEmpty(ip))
				ip = "UNKNOWN SOURCE";

			return ip;
		} // GetRemoteIP

		public static string GetSessionID(this Controller oController) {
			string sSessionID = oController.Request.Cookies[SessionCookieName] == null
				? "NO SESSION COOKIE"
				: (oController.Request.Cookies[SessionCookieName].Value ?? string.Empty).Trim();

			if (string.IsNullOrEmpty(sSessionID))
				sSessionID = "EMPTY SESSION COOKIE";

			return sSessionID;
		} // GetSessionID

		public static int GetBrowserVersionID(this Controller oController, string sUserAgent, AConnection oDB, ASafeLog oLog) {
			int nBrowserVersionID = 0;

			var oRetryer = new SqlRetryer(log: oLog);

			oRetryer.Retry(() => {
				nBrowserVersionID = oDB.ExecuteScalar<int>(
					"UiEventBrowserVersion",
					CommandSpecies.StoredProcedure,
					new QueryParameter("@Version", sUserAgent)
				);
			}, "saving browser version");

			return nBrowserVersionID;
		} // GetBrowserVersionID

		private static string SessionCookieName {
			get {
				if (!string.IsNullOrWhiteSpace(ms_sSessionCookieName))
					return ms_sSessionCookieName;

				lock (ms_oSessionCookieNameLock) {
					if (string.IsNullOrWhiteSpace(ms_sSessionCookieName)) {
						var sessionStateSection = (SessionStateSection)ConfigurationManager.GetSection("system.web/sessionState");

						ms_sSessionCookieName = sessionStateSection.CookieName;
					} // if
				} // lock

				return ms_sSessionCookieName;
			} // get
		} // SessionCookieName

		private static string ms_sSessionCookieName;
		private static readonly object ms_oSessionCookieNameLock = new object();

	} // class ControllerExt
} // namespace
