namespace Ezbob.Backend.Strategies.Tasks {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG;
	using Ezbob.Utils.MimeTypes;
	using global::Reports;
	using MailApi;
	using MailApi.Model;

	public class WeeklyMaamMedalAndPricing : MaamMedalAndPricing {
		public WeeklyMaamMedalAndPricing(bool forceRunNow) {
			this.today = DateTime.UtcNow.Date;
			this.doRun = forceRunNow || this.today.DayOfWeek == DayOfWeek.Saturday;
		} // constructor

		public override string Name {
			get { return "DatedMaamMedalAndPricing"; }
		} // Name

		public override void Execute() {
			if (!this.doRun) {
				Log.Debug("Not running: neither Saturday nor forced.");
				return;
			} // if

			DateFrom = this.today.AddDays(-7);

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

			string baseAttachmentName = "weekly.automation." +
				this.today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

			var mime = new MimeTypeResolver();

			var attachments = new List<attachment> {
				new attachment {
					name = baseAttachmentName + ".xlsx",
					content = CreateXlsx(),
					type = mime[".xlsx"],
				}
			};

			new Mail().Send(
				rpt.ToEmail,
				"See attached file.",
				null,
				ConfigManager.CurrentValues.Instance.MailSenderEmail,
				ConfigManager.CurrentValues.Instance.MailSenderName,
				rpt.Title.Trim() + " " + this.today,
				attachments: attachments
			);
		} // Execute

		private string CreateXlsx() {
			string fileName = string.Format(
				"{0}.{1}.{2}",
				DateTime.UtcNow.ToString("yyyyMMdd-HHmmss.ffffff", CultureInfo.InvariantCulture),
				Guid.NewGuid().ToString("N"),
				Guid.NewGuid().ToString("N")
			);

			string filePath = Path.Combine(Path.GetTempPath(), fileName);

			Xlsx.SaveAs(new FileInfo(filePath));

			string result = Mail.EncodeAttachment(File.ReadAllBytes(filePath));

			File.Delete(filePath);

			return result;
		} // CreateXlsx

		private readonly bool doRun;
		private readonly DateTime today;
	} // class WeeklyMaamMedalAndPricing
} // namespace

