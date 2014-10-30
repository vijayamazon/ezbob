namespace Reports {
	using System.Net.Mail;
	using Ezbob.Logger;
	using Html;
	using Html.Tags;
	using OfficeOpenXml;

	public class BaseReportSender : SafeLog {
		#region public

		#region constructor

		public BaseReportSender(ASafeLog log = null) : base(log) {} // constructor

		#endregion constructor

		#region method Send

		public void Send(string subject, ATag mailBody, ExcelPackage wb, string toAddressStr, string period = "Daily") {
			var email = new Html();

			email
				.Append(new Head().Append(Report.GetStyle()))
				.Append(mailBody);

			email.MoveCssInline(Report.ParseStyle());

			var oSender = new MailAddress(DefaultFromEMail, DefaultFromName);

			if (!string.IsNullOrWhiteSpace(toAddressStr)) {
				lock (typeof (BaseReportHandler)) {
					Mailer.Mailer.SendMail(
						oSender,
						DefaultFromEMailPassword,
						"EZBOB " + period + " " + subject + " Client Report",
						email.ToString(),
						toAddressStr,
						wb,
						this
					);
				} // lock
			} // if

			Debug("Mail {0} sent to: {1}", subject, toAddressStr);
		} // Send

		#endregion method Send

		#endregion public

		#region private

		#region const

		private const string DefaultFromEMailPassword = "EZ!reports2013";
		private const string DefaultFromEMail = "reports@ezbob.com";
		private const string DefaultFromName = "Report Daemon";

		#endregion const

		#endregion private
	} // class BaseReportSender
} // namespace Reports
