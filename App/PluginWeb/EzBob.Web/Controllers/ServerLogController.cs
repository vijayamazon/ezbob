namespace EzBob.Web.Controllers {
	using System;
	using System.Collections.Generic;
	using System.Web.Mvc;
	using Code.ServerLog;
	using Ezbob.Logger;
	using Newtonsoft.Json;
	using log4net;

	public class ServerLogController : Controller {
		#region public

		#region method Say

		[HttpPost]
		public JsonResult Say(string version, string history) {
			Dictionary<string, ServerLogCachePkgModel> oHistory = null;

			try {
				oHistory = JsonConvert.DeserializeObject<Dictionary<string, ServerLogCachePkgModel>>(history);
			}
			catch (Exception ex) {
				ms_oLog.Warn(ex, "Failed to deserialize history: {0}", history);
			}
			
			ms_oLog.Debug("Client package for logging from remote address '{0}' - begin", RemoteIp());

			var oSavedPackages = new List<string>();

			if ((oHistory == null) || (oHistory.Count < 1))
				ms_oLog.Warn("No data received, nothing done.");
			else {
				foreach (var pair in oHistory) {
					ms_oLog.Debug("Browser version: '{0}'.", version);
					pair.Value.Save(ms_oLog);
					oSavedPackages.Add(pair.Key);
				} // for each pkg
			} // if

			ms_oLog.Debug("Client package for logging from remote address '{0}' - end", RemoteIp());

			return Json(new { result = "success", saved = oSavedPackages, });
		} // Say

		#endregion method Say

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

		private static readonly ASafeLog ms_oLog = new SafeILog(LogManager.GetLogger(typeof(ServerLogController)));

		#endregion private
	} // class ServerLogController
} // namespace
