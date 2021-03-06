﻿using System;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.IO;
using Ezbob.Logger;
using OfficeOpenXml;

namespace Mailer {
	public class Mailer {
		public static void SendMail(string fromAddress, string fromPassword, string subject, string mailBody, string toAddress, ExcelPackage wb = null, ASafeLog oLog = null, int retries = 5) {
			SendMail(new MailAddress(fromAddress), fromPassword, subject, mailBody, toAddress, wb, oLog, retries);
		} // SendMail

		public static void SendMail(MailAddress oFrom, string fromPassword, string subject, string mailBody, string toAddress, ExcelPackage wb = null, ASafeLog oLog = null, int retries = 5) {
			string body = mailBody;
			var ostream = new MemoryStream();

			var smtp = new SmtpClient {
				Host = "smtp.gmail.com",
				Port = 587,
				EnableSsl = true,
				DeliveryMethod = SmtpDeliveryMethod.Network,
				UseDefaultCredentials = false,
				Credentials = new NetworkCredential(oFrom.Address, fromPassword)
			};

			using (var message = new MailMessage()) {
				message.From = oFrom;
				message.Subject = subject;
				message.Body = body;
				message.IsBodyHtml = true;

				foreach (string sAddr in toAddress.Split(','))
					message.To.Add(sAddr);

				if (wb != null) {
					message.Attachments.Clear();
					wb.SaveAs(ostream);
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
