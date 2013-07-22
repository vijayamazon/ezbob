﻿using System;
using System.Threading;
using Ezbob.Logger;
using MailBee;
using MailBee.ImapMail;
using MailBee.Mime;

namespace PacnetBalance {
	#region class Processor

	public class Processor : SafeLog {
		#region public

		#region constructor

		public Processor(Conf cfg, ASafeLog oLog = null)
			: base(oLog) {
			m_oConf = cfg;
			m_oTotalWaitingTime = TimeSpan.FromMinutes(cfg.TotalWaitingTime == 0 ? 120 : cfg.TotalWaitingTime);

			Debug("\n\n***\n*** Pacnet.Processor configuration\n***\n");

			Debug("Mailbee licence key: {0}", cfg.MailBeeLicenseKey);
			Debug("Email details: {0}:{1} as {2}.", cfg.Server, cfg.Port, cfg.LoginAddress);
			Debug("Check for email every {0} seconds.", cfg.MailboxReconnectionIntervalSeconds);
			Debug("Wait for email no more than {0}.", m_oTotalWaitingTime);

			Debug("\n\n***\n*** End of Pacnet.Processor configuration\n***\n");
		} // constructor

		#endregion constructor

		#region method Init

		public bool Init() {
			// Licensing IMAP component. If the license key is invalid, it'll throw MailBeeLicenseException
			try {
				Global.LicenseKey = m_oConf.MailBeeLicenseKey;
			}
			catch (MailBeeLicenseException e) {
				Error("License key is invalid: {0}", e);
				return false;
			} // try

			return true;
		} // Init

		#endregion method Init

		#region method Run

		public void Run() {
			DateTime oStartTime = DateTime.UtcNow;

			for ( ; ; ) {
				DateTime oNow = DateTime.UtcNow;

				if (oNow.Subtract(oStartTime).CompareTo(m_oTotalWaitingTime) > 0) {
					Error("Email did not arrive, not waiting for it any more.");
					break;
				} // if

				try {
					var imap = new Imap {
						// Enable SSL/TLS if necessary
						SslMode = MailBee.Security.SslStartupMode.OnConnect
					};

					// Connect to IMAP server
					imap.Connect(m_oConf.Server, m_oConf.Port);
					Info("Connected to the server");

					// Log into IMAP account
					imap.Login(m_oConf.LoginAddress, m_oConf.LoginPassword);
					Info("Logged into the server");

					// Select Inbox folder
					imap.SelectFolder("Inbox");

					UidCollection uids = (UidCollection)imap.Search(true, "UNSEEN", null);

					Debug("Unseen email count: {0}", uids.Count);

					if (uids.Count > 0) {
						Debug("Fetching unseen messages...");

						MailMessageCollection msgs = imap.DownloadEntireMessages(uids.ToString(), true);

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
									} // if appropriate attachment type
								} // foreach attachment
							} // if has attachment
						} // foreach email

						if (handledMail)
							PacNetBalance.SavePacNetBalanceToDb();

						Debug("Fetching unseen messages complete.");

						if (handledMail)
							break;
					} // if

					imap.Disconnect();
				}
				catch (Exception e) {
					Error("Failed to fetch messages: {0}", e);
				} // try

				Debug("Sleeping...");
				Thread.Sleep(m_oConf.MailboxReconnectionIntervalSeconds * 1000);
			} // for
		} // Run

		#endregion method Run

		#region method Done

		public void Done() {

		} // Done

		#endregion method Done

		#endregion public

		#region protected
		#endregion protected

		#region private

		private TimeSpan m_oTotalWaitingTime;
		private Conf m_oConf;

		#endregion private
	} // class Processor

	#endregion class Processor
} // namespace PacnetBalance
