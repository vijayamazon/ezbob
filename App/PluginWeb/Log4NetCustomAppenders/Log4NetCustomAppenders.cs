using System;
using System.Net;
using System.Net.Mail;
using Scorto.Configuration;
using log4net.Appender;

namespace Log4NetCustomAppenders
{
    public class MailAppenderWithSSL : SmtpAppender
    {
        

        protected override void SendEmail(string messageBody)
        {
            try
            {
                var smtpClient = new SmtpClient
                {
                    Host = SmtpHost,
                    Port = Port,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(Username, Password.Decrypt()),
                    EnableSsl = true
                };

                var addressSplit = To.Split(';');
                var addressCollection = new MailAddressCollection();
                foreach (var val in addressSplit)
                {
                    addressCollection.Add(val);
                }

                smtpClient.Send(new MailMessage
                {
                    Body = messageBody,
                    From = new MailAddress(From),
                    To =  { To} ,
                    Subject = Subject,
                    Priority = MailPriority.High
                });
            }
            catch (Exception e)
            {
                ErrorHandler.Error(e.ToString());
            }
            
        }
    }
}
