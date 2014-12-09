namespace EzBob.Web.Controllers {
	using System;
	using System.Collections.Generic;
	using System.Web.Mvc;
	using Code;
	using Ezbob.Logger;
	using Newtonsoft.Json;
	using Ezbob.Database;
	using Code.UiEvents;

	public class UiActionController : Controller {

		[HttpPost]
		public JsonResult Save(string version, string history) {
			Dictionary<string, UiCachePkgModel> oHistory = null;
			try {
				oHistory = JsonConvert.DeserializeObject<Dictionary<string, UiCachePkgModel>>(history);
			}
			catch (Exception ex) {
				ms_oLog.Warn(ex, "Failed to deserialize history: {0}", history);
			}

			if ((oHistory == null) || (oHistory.Count < 1)) {
				ms_oLog.Warn("No data received, nothing done.");
				return Json(new { result = "success" });
			} // if

			string sSessionID = this.GetSessionID();

			string sRemoteIP = this.GetRemoteIP();

			AConnection oDB = DbConnectionGenerator.Get(ms_oLog);

			int nBrowserVersionID = this.GetBrowserVersionID(version, oDB, ms_oLog);

			if (nBrowserVersionID == 0)
				return Json(new { result = "Failed to save browser version." });

			// ms_oLog.Debug("{1} at {2}: UiActionController.Save(version: {3} - {0}), data:", oBrowserVersion.UserAgent, sSessionID, sRemoteIP, oBrowserVersion.ID );

			var oSavedPackages = new List<string>();
			var oFailedPackages = new List<UiCachePkgModel.SaveResult>();

			foreach (KeyValuePair<string, UiCachePkgModel> pair in oHistory) {
				// ms_oLog.Debug("\tkey: {0} pkg: {1}", pair.Key, pair.Value);

				UiCachePkgModel.SaveResult oResult = pair.Value.Save(oDB, nBrowserVersionID, sRemoteIP, sSessionID);

				if (oResult.Overall())
					oSavedPackages.Add(pair.Key);
				else
					oFailedPackages.Add(oResult);
			} // for each pkg

			return Json(new { result = "success", saved = oSavedPackages, failures = oFailedPackages });
		} // Save

		private static readonly ASafeLog ms_oLog = new SafeILog(typeof(UiActionController));

	} // class UiActionController
} // namespace
