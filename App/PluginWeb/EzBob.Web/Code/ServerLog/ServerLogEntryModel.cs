namespace EzBob.Web.Code.ServerLog {
	using System;
	using System.Globalization;
	using Ezbob.Logger;

	public class ServerLogEntryModel {
		public string msg { get; set; }
		public string severity { get; set; }
		public string userName { get; set; }
		public string eventTime { get; set; }
		public string eventID { get; set; }

		public void Write(ASafeLog oLog) {
			DateTime oTime;

			if (!DateTime.TryParseExact(eventTime, "yyyy-MM-dd=HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out oTime)) {
				oLog.Alert("Failed to parse entry time '{0}', using current UTC time instead.", eventTime);
				oTime = DateTime.UtcNow;
			} // if failed to parse time

			Severity nSeverity;

			if (!Enum.TryParse<Severity>(severity, true, out nSeverity)) {
				oLog.Alert("Failed to parse severity '{0}', using Info instead.", severity);
				nSeverity = Severity.Info;
			} // if failed to parse severity

			oLog.Say(
				nSeverity,
				"FROM CLIENT at {0}, user '{1}', event id {2}: {3}",
				oTime.ToString("dd/MMM/yyyy HH:mm:ss", CultureInfo.InvariantCulture),
				userName, eventID, msg
			);
		} // Save

	} // class ServerLogEntryModel
} // namespace
