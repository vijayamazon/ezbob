using System.Collections.Generic;
using System.Configuration;
using System.Web.Configuration;
using System.Web.Mvc;
using EZBob.DatabaseLib;
using Newtonsoft.Json;
using Scorto.Web;
using StructureMap;
using log4net;

namespace EzBob.Web.Controllers {
	public class UiActionController : Controller {
		#region public

		#region constructor

		public UiActionController() {
			SessionStateSection sessionStateSection =
				(System.Web.Configuration.SessionStateSection)ConfigurationManager.GetSection("system.web/sessionState");

			m_sSessionCookieName = sessionStateSection.CookieName;
		} // constructor

		#endregion constructor

		#region method Save

		[Transactional]
		public JsonNetResult Save(string version, string history) {
			var helper = ObjectFactory.GetInstance<DatabaseDataHelper>();

			Dictionary<string, UiCachePkgModel> oHistory = JsonConvert.DeserializeObject<Dictionary<string, UiCachePkgModel>>(history);

			if ((oHistory == null) || (oHistory.Count < 1)) {
				ms_oLog.Warn("No data received, nothing done.");
				return this.JsonNet(new { result = "success" });
			} // if

			string sSessionID = Request.Cookies[m_sSessionCookieName] == null ? "NO SESSION COOKIE" : (Request.Cookies[m_sSessionCookieName].Value ?? string.Empty).Trim();

			if (string.IsNullOrEmpty(sSessionID))
				sSessionID = "EMPTY SESSION COOKIE";

			string sRemoteIP = (RemoteIp() ?? string.Empty).Trim();

			if (string.IsNullOrEmpty(sRemoteIP))
				sRemoteIP = "UNKNOWN SOURCE";

			BrowserVersion oBrowserVersion = helper.BrowserVersionRepository.FindByName(version);

			if (oBrowserVersion == null) {
				oBrowserVersion = new BrowserVersion { UserAgent = version };
				helper.BrowserVersionRepository.SaveOrUpdate(oBrowserVersion);
			} // if

			ms_oLog.DebugFormat("{1} at {2}: UiActionController.Save(version: {3} - {0}), data:",
				oBrowserVersion.UserAgent, sSessionID, sRemoteIP, oBrowserVersion.ID
			);

			var oSavedPackages = new List<string>();
			var oFailedPackages = new List<UiCachePkgModel.SaveResult>();

			foreach (var pair in oHistory) {
				ms_oLog.DebugFormat("\tkey: {0} pkg: {1}", pair.Key, pair.Value);

				UiCachePkgModel.SaveResult oResult = pair.Value.Save(helper, oBrowserVersion, sRemoteIP, sSessionID);

				if (oResult.Overall())
					oSavedPackages.Add(pair.Key);
				else
					oFailedPackages.Add(oResult);
			} // for each pkg

			return this.JsonNet(new { result = "success", saved = oSavedPackages, failures = oFailedPackages });
		} // Save

		#endregion method Save

		#endregion public

		#region private

		#region method RemoteIp

		private string RemoteIp() {
			string ip = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

			if (string.IsNullOrEmpty(ip))
				ip = Request.ServerVariables["REMOTE_ADDR"];

			return ip;
		} // RemoteIp

		#endregion method RemoteIp

		private readonly string m_sSessionCookieName;
		private static readonly ILog ms_oLog = LogManager.GetLogger(typeof(UiActionController));

		#endregion private
	} // class UiActionController
} // namespace
