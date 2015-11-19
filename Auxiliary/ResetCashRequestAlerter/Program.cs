namespace ResetCashRequestAlerter {
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Configuration;
	using System.Globalization;
	using System.Linq;
	using System.Net.Mail;
	using System.Threading;
	using ConfigManager;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Html.Tags;
	using Ezbob.Utils.Lingvo;

	class Program {
		static void Main(string[] args) {
			var app = new Program();

			if (app.Init(args))
				app.Run();

			app.Done();
		} // Main

		private Program() {
			Ezbob.Context.Environment environment = new Ezbob.Context.Environment();

			new Log4Net(environment).Init();

			this.log = new SafeILog(this);

			this.db = new SqlConnection(environment, this.log);

			this.alertList = new List<string>();
			this.cashRequestsToAlert = new List<ResetCashRequest>();
		} // constructor

		private bool Init(string[] args) {
			NameValueCollection settings =  ConfigurationManager.GetSection("rcra/prefs")
				as System.Collections.Specialized.NameValueCollection;

			if (settings == null) {
				this.log.Fatal("Preferences section not found in app.config.");
				return false;
			} // if

			foreach (string key in settings.AllKeys) {
				string val = settings[key];
				bool success;

				switch (key) {
				case "TimeSlice":
					success = DateTime.TryParseExact(
						val,
						"yyyy-MM-dd",
						culture,
						DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
						out this.timeSlice
					);

					if (!success) {
						this.log.Fatal("Failed to parse time slice configuration setting.");
						return false;
					} // if

					this.log.Info("Check only actions happened after {0}.", this.timeSlice.ToString("d/MMM/yyyy", culture));
					break;

				case "Sleep":
					success = int.TryParse(val, out this.sleepMinutes);

					if (!success || (this.sleepMinutes < 1) || (this.sleepMinutes > 15)) {
						this.log.Fatal("Failed to parse sleep time configuration setting.");
						return false;
					} // if

					this.log.Info("Sleep time between checks: {0}.", Grammar.Number(this.sleepMinutes, "minute"));

					break;

				case "AlertList":
					if (string.IsNullOrWhiteSpace(val)) {
						this.log.Fatal("No email recipients configured.");
						return false;
					} // if

					this.alertList.Clear();

					this.alertList.AddRange(
						val.Split(';').Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim())
					);

					if (this.alertList.Count < 1) {
						this.log.Fatal("Failed to parse email recipients.");
						return false;
					} // if

					this.log.Info(
						"Alert {0}: {1}.",
						Grammar.Number(this.alertList.Count, "recipient"),
						string.Join("; ", this.alertList)
					);

					break;
				} // switch
			} // for each key

			return true;
		} // Init

		private void Run() {
			const int stepSeconds = 20;

			for (;;) {
				try {
					RunOnce();
				} catch (Exception e) {
					this.log.Alert(e, "Something went terribly wrong while looking for reset cash requests.");
				} // try

				this.log.Debug("Sleeping...");

				for (int i = 0; i < this.sleepMinutes; i++) {
					for (int j = 0; j < 60; j += stepSeconds) {
						Thread.Sleep(20000);
						this.log.Debug(
							"Slept {0} {1} out of {2}.",
							Grammar.Number(i, "minute"),
							Grammar.Number(j + stepSeconds, "second"),
							Grammar.Number(this.sleepMinutes, "minute")
						);
					} // for j
				} // for i
			} // forever
		} // Run

		private void RunOnce() {
			this.cashRequestsToAlert.Clear();

			this.db.ForEachResult<ResetCashRequest>(
				rcr => this.cashRequestsToAlert.Add(rcr),
				"LoadResetCashRequests",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@TimeSlice", this.timeSlice)
			);

			if (this.cashRequestsToAlert.Count < 1) {
				this.log.Info("No cash requests to alert on.");
				return;
			} // if

			this.log.Info("{0} found to alert on.", Grammar.Number(this.cashRequestsToAlert.Count, "cash request"));

			var tbl = new Table();
			tbl.Append(
				new Thead().Append(
					new Tr()
						.Append(new Th().Append(new Text("Cash request ID")))
						.Append(new Th().Append(new Text("Decision time")))
						.Append(new Th().Append(new Text("Decision")))
						.Append(new Th().Append(new Text("Customer ID")))
						.Append(new Th().Append(new Text("Customer name")))
						.Append(new Th().Append(new Text("Customer email")))
						.Append(new Th().Append(new Text("Underwriter ID")))
						.Append(new Th().Append(new Text("Underwriter name")))
						.Append(new Th().Append(new Text("Note")))
				)
			);

			var tbody = new Tbody();
			tbl.Append(tbody);

			foreach (var rcr in this.cashRequestsToAlert) {
				string note;

				try {
					this.db.ExecuteNonQuery(
						"RestoreResetCashRequest",
						CommandSpecies.StoredProcedure,
						new QueryParameter("@CashRequestID", rcr.CashRequestID)
					);

					note = "Restored.";
				} catch (Exception e) {
					this.log.Alert(e, "Failed to restore cash request {0}.", rcr.CashRequestID);
					note = "Restoring failed: " + e.Message;
				} // try

				tbody.Append(new Tr()
					.Append(new Td().Append(new Text(rcr.CashRequestID.ToString())))
					.Append(new Td().Append(new Text(rcr.DecisionTime.ToString("d/MMM/yyyy H:mm:ss", culture))))
					.Append(new Td().Append(new Text(rcr.Decision.ToString())))
					.Append(new Td().Append(new Text(rcr.CustomerID.ToString())))
					.Append(new Td().Append(new Text(rcr.CustomerName)))
					.Append(new Td().Append(new Text(rcr.CustomerEmail)))
					.Append(new Td().Append(new Text(rcr.UnderwriterID.ToString())))
					.Append(new Td().Append(new Text(rcr.UnderwriterName)))
					.Append(new Td().Append(new Text(note)))
				);
			} // for each

			var email = new Html();

			email
				.Append(new Head())
				.Append(tbl);

			this.log.Debug("Sending an email...");

			Mailer.SendMail(
				sender,
				Password,
				"#Alert - reset cash requests " + DateTime.Now.ToString("d/MMM/yyyy H:mm:ss", culture),
				email.ToString(),
				string.Join(",", this.alertList),
				this.log
			);

			this.log.Debug("Sent.");
		} // RunOnce

		private void Done() {
			this.log.Dispose();
		} // Done

		private readonly ASafeLog log;
		private readonly AConnection db;

		private int sleepMinutes;
		private DateTime timeSlice;
		private readonly List<string> alertList;
		private readonly List<ResetCashRequest> cashRequestsToAlert; 

		private static readonly CultureInfo culture = new CultureInfo("en-GB", false);
		private static readonly MailAddress sender = new MailAddress("ezbob@ezbob.com", "Reset cash request alerter");
		private const string Password = "ezbob2012";
	} // class Program
} // namespace
