namespace Foam {
	using System;
	using System.Collections.Specialized;
	using System.Configuration;
	using System.Diagnostics;
	using System.Globalization;
	using System.Reflection;
	using System.Threading;
	using ConfigManager;
	using Ezbob.Database;
	using Ezbob.Logger;
	using MailApi;

	class Program {
		static void Main(string[] args) {
			var env = new Ezbob.Context.Environment();

			new Log4Net(env).Init();

			log = new SafeILog(typeof(Program));

			db = new SqlConnection(env, log);

			CurrentValues.Init(db, log);

			var app = new Program();
			app.Run();
			app.Done();
		} // Main

		private Program() {
			NotifyStartStop("started");

			this.readyToRock = false;

			this.mail = new Mail();
			this.currentDate = DateTime.UtcNow.AddMonths(-1);
			this.emails = null;
			this.amountGranularity = 50000;
			this.sleepTime = 5;

			Init();
		} // constructor

		private void Init() {
			var cfg = ConfigurationManager.GetSection("appConfig") as NameValueCollection;

			if (cfg == null) {
				log.Alert("'appConfig' section not found in app.config file.");
				return;
			} // if

			string val = cfg.Get("AmountGranularity");
			int granularity;

			if (!string.IsNullOrWhiteSpace(val) && int.TryParse(val, out granularity) && (granularity > 0))
				this.amountGranularity = granularity * 1000;

			val = cfg.Get("Emails");
			if (!string.IsNullOrWhiteSpace(val))
				this.emails = val;

			if (string.IsNullOrWhiteSpace(this.emails)) {
				log.Alert("No email addresses configured for alerts.");
				return;
			} // if

			val = cfg.Get("Sleep");
			int sleep;
			if (!string.IsNullOrWhiteSpace(val) && int.TryParse(val, out sleep) && (sleep > 0))
				this.sleepTime = sleep;

			log.Debug(
				"Sending email to {0} on every issued/approved {1}. Sleeping for {2} minute(s) between checks.",
				string.Join(", ", this.emails),
				this.amountGranularity.ToString("C0", Culture),
				this.sleepTime
			);

			this.readyToRock = true;
		} // Init

		private void Run() {
			if (!this.readyToRock) {
				log.Alert("Not ready for execution, exiting.");
				return;
			} // if

			for ( ; ; ) {
				SetCurrentDate();

				Amounts amounts = LoadAmounts();

				if ((amounts.Approved > 0) && (watermarks.Approved <= amounts.Approved)) {
					SendEmail("Approved", this.watermarks.Approved, amounts.Approved);
					this.watermarks.MoveApproved(amounts.Approved);
					log.Debug("New approved watermark is {0}.", this.watermarks.Approved.ToString("C0", Culture));
				} // if

				if ((amounts.Issued > 0) && (watermarks.Issued <= amounts.Issued)) {
					SendEmail("Issued", this.watermarks.Issued, amounts.Issued);
					this.watermarks.MoveIssued(amounts.Issued);
					log.Debug("New issued watermark is {0}.", this.watermarks.Issued.ToString("C0", Culture));
				} // if

				Sleep();
			} // for ever
		} // Run

		private void SetCurrentDate() {
			DateTime localToday = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tzUK).Date;

			if (this.currentDate == localToday) {
				log.Debug("Current date is still {0}.", this.currentDate.ToString(DateFormat, Culture));
				return;
			} // if

			log.Debug(
				"Current date is changed from {0} to {1}.",
				this.currentDate.ToString(DateFormat, Culture),
				localToday.ToString(DateFormat, Culture)
			);

			this.currentDate = localToday;
			this.watermarks = new Watermarks(this.amountGranularity);
		} // SetCurrentDate

		private Amounts LoadAmounts() {
			DateTime utcFrom = TimeZoneInfo.ConvertTimeToUtc(this.currentDate, tzUK);
			DateTime utcTo = TimeZoneInfo.ConvertTimeToUtc(this.currentDate.AddDays(1), tzUK);

			decimal approved = 0;
			decimal issued = 0;

			db.ForEachRowSafe(
				sr => {
					string rowType = sr["RowType"];
					decimal rowValue = sr["RowValue"];

					switch (rowType) {
					case "Approved":
						approved = rowValue;
						break;
					case "Issued":
						issued = rowValue;
						break;
					} // switch
				},
				"Foam_LoadAmounts",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@DateFrom", utcFrom),
				new QueryParameter("@DateTo", utcTo)
			);

			var result = new Amounts(approved, issued);

			log.Debug("Amounts are: {0}.", result);

			return result;
		} // LoadAmounts

		private void SendEmail(string action, decimal watermark, decimal amount) {
			string text = string.Format(
				"Total {0} today has reached {1} (exact amount {2}).",
				action,
				watermark.ToString("C0", Culture),
				amount.ToString("C0", Culture)
			);

			string subject = string.Format("Money out alert: {0} reached {1}", action, watermark.ToString("C0", Culture));

			this.mail.Send(
				this.emails,
				text,
				null,
				CurrentValues.Instance.MailSenderEmail,
				"Money out Alerter",
				subject
			);
		} // SetCurrentDate

		private void Sleep() {
			const int heartbeatSeconds = 20;
			const int minuteSleepCount = 60 / heartbeatSeconds;
			const int sleepInterval = heartbeatSeconds * 1000;

			log.Debug("Sleeping...");

			for (int i = 1; i <= this.sleepTime; i++) {
				for (int j = 0; j < minuteSleepCount; j++) {
					Thread.Sleep(sleepInterval);
					log.Debug("Sleeping heartbeat ({0}).", j + 1);
				} // for j

				log.Debug("{0} minute(s) out of {1} minute(s) to sleep passed.", i, this.sleepTime);
			} // for minutes

			log.Debug("Back to work.");
		} // Sleep

		private void Done() {
			NotifyStartStop("stopped");
		} // Done

		private static void NotifyStartStop(string sEvent) {
			log.Info(
				"Logging {0} for {1} v{5} on {2} as {3} with pid {4}.",
				sEvent,
				"EzService",
				Environment.MachineName,
				Environment.UserName,
				Process.GetCurrentProcess().Id,
				Assembly.GetCallingAssembly().GetName().Version.ToString(4)
			);
		} // NotifyStartStop

		private DateTime currentDate;

		private decimal amountGranularity;
		private readonly Mail mail;
		private string emails;
		private int sleepTime;
		private bool readyToRock;
		private Watermarks watermarks;

		private static AConnection db;
		private static ASafeLog log;
		private const string DateFormat = "d MMM yyyy";
		private static readonly TimeZoneInfo tzUK = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");

		public static readonly CultureInfo Culture = new CultureInfo("en-GB", false);
	} // class Program
} // namespace
