namespace Reports {
	using System.Net.Mail;
	using Ezbob.Logger;
	using Ezbob.Utils.Html;
	using Ezbob.Utils.Html.Tags;
	using OfficeOpenXml;

	public class BaseReportSender : SafeLog {

		public BaseReportSender(ASafeLog log = null) : base(log) {} // constructor

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

		private const string DefaultFromEMailPassword = "EZ!reports2013";
		private const string DefaultFromEMail = "reports@ezbob.com";
		private const string DefaultFromName = "Report Daemon";

	} // class BaseReportSender
} // namespace Reports
