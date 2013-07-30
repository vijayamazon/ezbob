using System.Net.Mail;
using Aspose.Cells;
using Ezbob.Logger;
using Html;
using Html.Attributes;
using Html.Tags;

namespace Reports {
	#region class BaseReportSender

	public class BaseReportSender : SafeLog {
		#region public

		public struct MailTemplate {
			public ATag HtmlBody;
			public ATag Title;
			public ATag ReportBody;
		} // struct MailTemplate

		#region const

		public const string DefaultToEMail = "dailyreports@ezbob.com";
		public const string DefaultFromEMailPassword = "EZ!reports2013";
		public const string DefaultFromEMail = "reports@ezbob.com";
		public const string DefaultFromName = "Report Daemon";

		#endregion const

		public BaseReportSender(ASafeLog log = null) : base(log) {} // constructor

		#region method Send

		public void Send(string subject, ATag mailBody, string toAddressStr = DefaultToEMail, string period = "Daily", Workbook wb = null) {
			var email = new Html.Tags.Html();

			email
				.Append(new Head().Append(Report.GetStyle()))
				.Append(mailBody);

			email.MoveCssInline(Report.ParseStyle());

			var oSender = new MailAddress(DefaultFromEMail, DefaultFromName);

			lock (typeof(BaseReportHandler)) {
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

			Debug("Mail {0} sent to: {1}", subject, toAddressStr);
		} // Send

		#endregion method Send

		#region method CreateMailTemplate

		public MailTemplate CreateMailTemplate() {
			var template = new MailTemplate();

			template.HtmlBody = new Body().Add<Class>("Body");

			var oTbl = new Table().Add<Class>("Header");
			template.HtmlBody.Append(oTbl);

			var oImgLogo = new Img()
				.Add<Class>("Logo")
				.Add<Src>("http://www.ezbob.com/wp-content/themes/ezbob/images/ezbob_logo.png");

			var oLogoLink = new A()
				.Add<Href>("http://www.ezbob.com/")
				.Add<Class>("logo_ezbob")
				.Add<Class>("indent_text")
				.Add<ID>("ezbob_logo")
				.Add<Html.Attributes.Title>("Fast business loans for Ebay and Amazon merchants")
				.Add<Alt>("Fast business loans for Ebay and Amazon merchants")
				.Append(oImgLogo);

			var oTr = new Tr();
			oTbl.Append(oTr);

			oTr.Append(new Td().Append(oLogoLink));

			template.Title = new H1();

			oTr.Append(new Td().Append(template.Title));

			template.ReportBody = new P().Add<Class>("Body");

			template.HtmlBody.Append(template.ReportBody);

			return template;
		} // CreateMailTemplate

		#endregion method CreateMailTemplate

		#endregion public
	} // class BaseReportSender

	#endregion class BaseReportSender
} // namespace Reports
