using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using EZBob.DatabaseLib;
using Newtonsoft.Json;
using Scorto.Web;
using StructureMap;
using log4net;

namespace EzBob.Web.Controllers {
	public class UiActionEventModel {
		public string userName { get; set; }
		public string controlName { get; set; }
		public string htmlID { get; set; }
		public string actionName { get; set; }
		public string eventTime { get; set; }
		public string eventID { get; set; }
		public string eventArgs { get; set; }

		#region method ToString

		public override string ToString() {
			var os = new StringBuilder();

			os.AppendFormat("[ user: {0},", userName);
			os.AppendFormat(" controlName: {0},",  controlName);
			os.AppendFormat(" htmlID: {0},", htmlID);
			os.AppendFormat(" actionName: {0},", actionName);
			os.AppendFormat(" eventTime: {0},", eventTime);
			os.AppendFormat(" eventID: {0},", eventID);
			os.AppendFormat(" eventArgs: {0} ]", eventArgs);

			return os.ToString();
		} // ToString

		#endregion method ToString
	} // class UiActionEventModel

	public class UiCachePkgModel {
		public string id { get; set; }
		public List<UiActionEventModel> data { get; set; }

		#region method ToString

		public override string ToString() {
			var os = new StringBuilder();

			os.AppendFormat("( id: {0}", id);

			if (data == null)
				os.Append("\n\t-- No data --");
			else {
				foreach (var oEvt in data)
					os.AppendFormat("\n\t{0}", oEvt.ToString());
			} // if

			os.Append("\n)");

			return os.ToString();
		} // ToString

		#endregion method ToString
	} // class UiCachePkgModel

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

			string sSessionID = Request.Cookies[m_sSessionCookieName] == null ? "" : Request.Cookies[m_sSessionCookieName].Value;

			ms_oLog.DebugFormat("{1} at {2}: UiActionController.Save(version: {0}), data:", version, sSessionID, RemoteIp());

			var oSavedPackages = new List<string>();

			foreach (var pair in oHistory) {
				ms_oLog.DebugFormat("\tkey: {0} pkg: {1}", pair.Key, pair.Value);

				oSavedPackages.Add(pair.Key);
			} // for each pkg

			return this.JsonNet(new { result = "success", saved = oSavedPackages });
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
