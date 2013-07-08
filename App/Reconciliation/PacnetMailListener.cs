using System;
using Ezbob.Logger;
using MailBee.ImapMail;
using MailBee.Mime;

namespace Reconciliation {
	public class PacnetMailListener : MailListener.MailListener {
		public PacnetMailListener(Action onAfterSave, ASafeLog oLog = null)
			: base(
				Conf.MailboxReconnectionIntervalSeconds,
				Conf.Server,
				Conf.Port,
				Conf.LoginAddress,
				Conf.LoginPassword,
				Conf.MailBeeLicenseKey,
				oLog
			)
		{
			m_onAfterSave = onAfterSave;
		} // constructor

		protected override UidCollection FillUids() {
			return Search(Consts.MailSearchCondition);
		} // FillUids

		protected override void HandleMessages(MailMessageCollection msgs) {
			bool handledMail = false;

			foreach (MailMessage msg in msgs) {
				Info("Recent message index: {0} Subject: {1}", msg.IndexOnServer, msg.Subject);

				if (msg.HasAttachments) {
					foreach (Attachment attachment in msg.Attachments) {
						if (Consts.AttachmentContentTypes.Contains(attachment.ContentType)) {
							Info("Has pdf attachment {0}", attachment.Filename);
							byte[] data = attachment.GetData();
							ParsePacNetText.ParsePdf(data);
							handledMail = true;
						} // if
					} // foreach
				} // if
			} // foreach

			if (handledMail) {
				PacNetBalance.SavePacNetBalanceToDb();

				if (m_onAfterSave != null)
					m_onAfterSave();

				Environment.Exit(0);
			} // if
		} // HandleMessages

		private Action m_onAfterSave;
	} // class PacnetMailListener
} // namespace
