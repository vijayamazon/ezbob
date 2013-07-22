using System;
using System.Net;
using System.Net.Mail;
using System.Threading;
using Aspose.Cells;
using System.IO;
using Ezbob.Logger;

namespace Mailer {
	public class Mailer {
		public static void SendMail(string fromAddress, string fromPassword, string subject, string mailBody, string toAddress, Workbook wb = null, ASafeLog oLog = null, int retries = 5) {
			string body = mailBody;
			var ostream = new MemoryStream();

			var smtp = new SmtpClient {
				Host = "smtp.gmail.com",
				Port = 587,
				EnableSsl = true,
				DeliveryMethod = SmtpDeliveryMethod.Network,
				UseDefaultCredentials = false,
				Credentials = new NetworkCredential(fromAddress, fromPassword)
			};

			using (var message = new MailMessage(fromAddress, toAddress) {
				Subject = subject,
				Body = body,
				IsBodyHtml = true
			}) {
				if (wb != null) {
					message.Attachments.Clear();
					wb.Save(ostream, FileFormatType.Excel2007Xlsx);
					ostream.Position = 0;
					var attachment = new Attachment(ostream, subject + ".xlsx", "Application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
					message.Attachments.Add(attachment);
				} // if workbook is not null

				int tryCounter = 0;

				while (tryCounter < retries) {
					try {
						smtp.Send(message);
						tryCounter = retries;
					}
					catch (Exception e) {
						oLog.Error(string.Format("Error sending mail: {0}", e));
						++tryCounter;

						if (tryCounter < retries) {
							oLog.Info("Will wait 5 seconds and retry to send the mail");
							Thread.Sleep(TimeSpan.FromSeconds(5));
						} // if
					} // try
				} // while retries left
			} // using

			ostream.Close();
		} // SendMail
	} // class Mailer
} // namespace Mailer
