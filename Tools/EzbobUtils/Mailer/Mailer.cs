namespace Mailer
{
    using System;
    using System.Net;
    using System.Net.Mail;
    using System.Threading;
    using Logger;

    public class Mailer
    {
        public static void SendMail(string fromAddress, string fromPassword, string subject, string mailBody, string toAddress, int retries = 5)
        {
            string body = mailBody;

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress, fromPassword)
            };

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
            {
                int tryCounter = 0;
                while (tryCounter < retries)
                {
                    try
                    {
                        smtp.Send(message);
                        tryCounter = retries;
                    }
                    catch (Exception e)
                    {
                        Logger.Error(string.Format("Error sending mail:{0}", e));
                        ++tryCounter;
                        if (tryCounter < retries)
                        {
                            Logger.Info("Will wait 5 seconds and retry to send the mail");
                            Thread.Sleep(TimeSpan.FromSeconds(5));
                        }
                    }
                }
            }
        }
    }
}
