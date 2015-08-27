namespace Foam {
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Configuration;
	using System.Diagnostics;
	using System.Globalization;
	using System.Reflection;
	using ConfigManager;
	using Ezbob.Database;
	using Ezbob.Logger;

	class Program {
		static void Main(string[] args) {
			var env = new Ezbob.Context.Environment();

			new Log4Net(env).Init();

			log = new SafeILog(typeof(Program));

			db = new SqlConnection(env, log);

			var app = new Program();
			app.Run();
			app.Done();
		} // Main

		private Program() {
			NotifyStartStop("started");

			this.readyToRock = false;

			this.currentDate = null;
			this.amountEmailSent = new SortedDictionary<decimal, EmailSent>();
			this.emails = new List<string>();
			this.amountGranularity = 50000;

			Init();
		} // constructor

		private void Init() {
			this.emails.Clear();

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
				this.emails.AddRange(val.Split(','));

			if (this.emails.Count < 1) {
				log.Alert("No email addresses configured for alerts.");
				return;
			} // if

			log.Debug(
				"Sending email to {0} on every issued/approved {1}.",
				string.Join(", ", this.emails),
				this.amountGranularity.ToString("C0", culture)
			);

			this.readyToRock = true;
		} // Init

		private void Run() {
			if (!this.readyToRock) {
				log.Alert("Not ready for execution, exiting.");
				return;
			} // if

			// for (;;) {
				SetCurrentDate();

				Amounts amounts = LoadAmounts();

				foreach (var aes in this.amountEmailSent) {
					decimal amount = aes.Key;
					EmailSent emailSent = aes.Value;

					if (!emailSent.Approved && (amounts.Approved > 0) && (amount <= amounts.Approved)) {
						SendEmail("Approved", amount);
						emailSent.Approved = true;
					} // if

					if (!emailSent.Issued && (amounts.Issued > 0) && (amount <= amounts.Issued)) {
						SendEmail("Issued", amount);
						emailSent.Issued = true;
					} // if
				} // for each

				Sleep();
			// } // for ever
		} // Run

		private void SetCurrentDate() {
			DateTime localToday = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tzUK).Date;

			if (this.currentDate == localToday) {
				log.Debug("Current date is still {0}.", this.currentDate.Value.ToString("d MMM yyyy", culture));
				return;
			} // if

			this.currentDate = localToday;
		} // SetCurrentDate

		private Amounts LoadAmounts() {
			// TODO
			return new Amounts(0, 0);
		} // LoadAmounts

		private void SendEmail(string action, decimal amount) {
			// TODO
		} // SetCurrentDate

		private void Sleep() {
			// TODO
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

		private class Amounts {
			public Amounts(decimal approved, decimal issued) {
				Approved = approved;
				Issued = issued;
			} // constructor

			public decimal Approved { get; private set; }
			public decimal Issued { get; private set; }
		} // class Amounts

		private class EmailSent {
			public EmailSent() {
				Clear();
			} // constructor

			public void Clear() {
				Approved = false;
				Issued = false;
			} // Clear

			public bool Approved { get; set; }
			public bool Issued { get; set; }
		} // class EmailSent

		private DateTime? currentDate;
		private readonly SortedDictionary<decimal, EmailSent> amountEmailSent; 

		private decimal amountGranularity;
		private readonly List<string> emails;
		private bool readyToRock;

		private static AConnection db;
		private static ASafeLog log;
		private static readonly CultureInfo culture = new CultureInfo("en-GB", false);
		private static readonly TimeZoneInfo tzUK = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
	} // class Program
} // namespace
