using System.Collections.Generic;
using Scorto.AutoNodes.NodeMail;
using Scorto.Email.Senders;
using Scorto.Export.Templates;

namespace Ezbob.MailNode.Runtime
{
	using System;
	using Scorto.Email.Accounts;
	using WorkflowObjects;
	using log4net;

	public class BatchMailerEzbob : BatchMailer
    {
		private IEmailSender _emailSender;
		static readonly ILog log = LogManager.GetLogger("Scorto.AutoNodes.NodeMail");

        protected override IEnumerable<EmailAttachment> SetAttachments(IEnumerable<ExportResult> results)
        {
            return new LinkedList<EmailAttachment>();
        }

		public new void Send(AccountBase account, NodeMailParams nodeMailParams, IEnumerable<ExportResult> results, ExportResult primary)
		{
			try
			{
				if (results == null) throw new ArgumentException("Results cannot be emty");

				_emailSender = EmailSenderFactory.CreateSender(account);
				if (_emailSender == null)
				{
					log.ErrorFormat("Cannot get sender for account of type {0}", account.GetType().FullName);
					return;
				}

				log.DebugFormat("Using sender '{0}'", _emailSender.GetType().FullName);

				log.InfoFormat("Sending email to {0}", nodeMailParams.To);

				var letter = Container.GetInstance<Letter>();

				letter.To = nodeMailParams.To;
				letter.CC = nodeMailParams.CC;
				letter.Subject = nodeMailParams.Subject;

				/*letter.SetAttachments(results.Select(er => new EmailAttachment(er.FileName, er.BinaryBody)));*/
				letter.SetAttachments(SetAttachments(results));

				letter.SetDocBody(primary.BinaryBody);

				_emailSender.Send(account, letter);
			}
			catch (Exception ex)
			{
				log.Error(ex);
				throw;
			}
		}
    }
}
