namespace Ezbob.Backend.Strategies.Tasks {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using Ezbob.Backend.Strategies.AutomationVerification;
	using global::Reports;
	using MailApi;
	using MailApi.Model;

	public class WeeklyMaamMedalAndPricing : MaamMedalAndPricing {
		public WeeklyMaamMedalAndPricing(bool forceRunNow) : base(-1, -1) {
			DateTime dateTo = DateTime.UtcNow.Date;

			this.doRun = forceRunNow || dateTo.DayOfWeek == DayOfWeek.Saturday;

			this.today = dateTo.Date;

			DateTime dateFrom = dateTo.AddDays(-7);

			this.condition = string.Format(
				"AND '{0}' <= r.UnderwriterDecisionDate AND r.UnderwriterDecisionDate < '{1}'",
				dateFrom.Date.ToString("MMMM d yyyy", CultureInfo.InvariantCulture),
				this.today.ToString("MMMM d yyyy", CultureInfo.InvariantCulture)
			);
		} // constructor

		public override string Name {
			get { return "DatedMaamMedalAndPricing"; }
		} // Name

		public override void Execute() {
			if (!this.doRun) {
				Log.Debug("Not running: neither Saturday nor forced.");
				return;
			} // if

			Report rpt;

			try {
				rpt = new Report(DB, ReportType.RPT_WEEKLY_KPMG);
			} catch (Exception e) {
				Log.Warn(e, "Failed to load report by type '{0}'.", ReportType.RPT_WEEKLY_KPMG);
				return;
			} // try

			if (string.IsNullOrWhiteSpace(rpt.ToEmail)) {
				Log.Debug("Not running: no email recipients found.");
				return;
			} // if

			base.Execute();

			string emailText = string.Join(System.Environment.NewLine, CsvOutput);

			var attachments = new List<attachment> {
				new attachment {
					name = "weekly.kpmg." + this.today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + ".txt",
					content = emailText,
					type = "text/plain",
				}
			};

			new Mail().Send(
				rpt.ToEmail,
				emailText,
				null,
				ConfigManager.CurrentValues.Instance.MailSenderEmail,
				ConfigManager.CurrentValues.Instance.MailSenderName,
				rpt.Title.Trim() + " " + this.today,
				attachments: attachments
			);
		} // Execute

		protected override string Condition {
			get { return this.condition; }
		} // Condition

		private readonly string condition;

		private readonly DateTime today;

		private readonly bool doRun;
	} // class WeeklyMaamMedalAndPricing
} // namespace

