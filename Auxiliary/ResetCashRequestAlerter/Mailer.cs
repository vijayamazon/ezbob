namespace ResetCashRequestAlerter {
	using System;
	using System.Net;
	using System.Net.Mail;
	using System.Threading;
	using Ezbob.Logger;

	public class Mailer {
		public static void SendMail(
			MailAddress oFrom,
			string fromPassword,
			string subject,
			string mailBody,
			string toAddress,
			ASafeLog oLog,
			int retries = 5
		) {
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
				message.Body = mailBody;
				message.IsBodyHtml = true;

				foreach (string sAddr in toAddress.Split(','))
					message.To.Add(sAddr);

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
		} // SendMail
	} // class Mailer
} // namespace Mailer
