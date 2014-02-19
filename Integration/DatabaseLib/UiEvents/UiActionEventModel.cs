namespace EZBob.DatabaseLib {
	using System;
	using System.Globalization;
	using System.Text;
	using log4net;
	using Ezbob.Database;

	#region class UiActionEventModel

	public class UiActionEventModel {
		public string userName { get; set; }
		public string controlName { get; set; }
		public string htmlID { get; set; }
		public string actionName { get; set; }
		public string eventTime { get; set; }
		public string eventID { get; set; }
		public string eventArgs { get; set; }

		#region method Save

		public bool Save(AConnection oDB, int nBrowserVersionID, string sRemoteIP, string sSessionCookie, int nRetryCount) {
			long nRefNum = 0;

			if (!long.TryParse(eventID, out nRefNum)) {
				ms_oLog.ErrorFormat("Failed to save UI event: cannot parse eventID as long in {0}.", this);
				return false;
			} // if

			DateTime oTime;

			if (!DateTime.TryParseExact(eventTime, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out oTime)) {
				ms_oLog.ErrorFormat("Failed to save UI event: cannot parse event time in {0}.", this);
				return false;
			} // if failed to parse time

			for (int i = 1; i <= nRetryCount; i++) {
				try {
					string sResult = oDB.ExecuteScalar<string>(
						"UiEventSave",
						CommandSpecies.StoredProcedure,
						new QueryParameter("@ActionName", actionName),
						new QueryParameter("@UserName", userName),
						new QueryParameter("@ControlName", controlName),
						new QueryParameter("@EventArgs", eventArgs),
						new QueryParameter("@BrowserVersionID", nBrowserVersionID),
						new QueryParameter("@HtmlID", htmlID),
						new QueryParameter("@RefNum", nRefNum),
						new QueryParameter("@RemoteIP", sRemoteIP),
						new QueryParameter("@SessionCookie", sSessionCookie),
						new QueryParameter("@EventTime", oTime)
					);

					if (string.IsNullOrWhiteSpace(sResult))
						return true;

					ms_oLog.WarnFormat("Failed to save UI event: {0}", sResult);
				}
				catch (Exception e) {
					ms_oLog.Warn("Failed to save UI event.", e);
				} // try

				if (i < nRetryCount)
					ms_oLog.Debug("Retrying to save UI event.");
			} // for

			return false;
		} // Save

		#endregion method Save

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

		private static readonly ILog ms_oLog = LogManager.GetLogger(typeof(UiActionEventModel));
	} // class UiActionEventModel

	#endregion class UiActionEventModel
} // namespace EZBob.DatabaseLib
