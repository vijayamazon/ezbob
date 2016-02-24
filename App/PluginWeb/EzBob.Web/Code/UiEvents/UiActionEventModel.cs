namespace EzBob.Web.Code.UiEvents {
	using System;
	using System.Globalization;
	using System.Text;
	using Ezbob.Logger;
	using Ezbob.Database;

	public class UiActionEventModel {
		public const string NoControl = "no HTML control";

		public string userName { get; set; }
		public string controlName { get; set; }
		public string htmlID { get; set; }
		public string actionName { get; set; }
		public string eventTime { get; set; }
		public string eventID { get; set; }
		public string eventArgs { get; set; }

		public bool Save(AConnection oDB, int nBrowserVersionID, string sRemoteIP, string sSessionCookie) {
			long nRefNum;

			if (!long.TryParse(eventID, out nRefNum)) {
				log.Error("Failed to save UI event: cannot parse eventID as long in {0}.", this);
				return false;
			} // if

			DateTime oTime;

			bool validDate = DateTime.TryParseExact(
				eventTime,
				"yyyyMMddHHmmss",
				CultureInfo.InvariantCulture,
				DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
				out oTime
			);

			if (!validDate) {
				log.Error("Failed to save UI event: cannot parse event time in {0}.", this);
				return false;
			} // if failed to parse time

			string sResult = null;

			var oRetryer = new SqlRetryer(log: log);

			string evtArgs = string.IsNullOrWhiteSpace(eventArgs) ? null : eventArgs;

			if ((evtArgs != null) && (evtArgs.Length > MaxAllowedLength)) {
				string id = Guid.NewGuid().ToString("N").ToLowerInvariant();

				evtArgs = string.Format(
					"Trimmed to {0} chars, trim id is '{1}': {2}",
					MaxAllowedLength,
					id,
					evtArgs.Substring(0, MaxAllowedLength)
				);

				log.Debug(
					"Event arguments are too long, trimmed to {0} chars, trim id is '{1}', untrimmed value is: {2}",
					MaxAllowedLength,
					id,
					eventArgs
				);
			} // if

			oRetryer.Retry(() => {
				sResult = oDB.ExecuteScalar<string>(
					"UiEventSave",
					CommandSpecies.StoredProcedure,
					new QueryParameter("@ActionName", actionName),
					new QueryParameter("@UserName", userName),
					new QueryParameter("@ControlName", controlName),
					new QueryParameter("@EventArgs", evtArgs),
					new QueryParameter("@BrowserVersionID", nBrowserVersionID),
					new QueryParameter("@HtmlID", htmlID),
					new QueryParameter("@RefNum", nRefNum),
					new QueryParameter("@RemoteIP", sRemoteIP),
					new QueryParameter("@SessionCookie", sSessionCookie),
					new QueryParameter("@EventTime", oTime)
				);
			}, "saving UI event");

			if (string.IsNullOrWhiteSpace(sResult))
				return true;

			log.Warn("Failed to save UI event: {0}", sResult);

			return false;
		} // Save

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

		private static readonly ASafeLog log = new SafeILog(typeof(UiActionEventModel));
		private const int MaxAllowedLength = 4000;
	} // class UiActionEventModel
} // namespace EZBob.DatabaseLib
