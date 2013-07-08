using System;
using MailBee;
using MailBee.ImapMail;
using MailBee.Mime;
using System.Threading;
using Ezbob.Logger;

namespace MailListener {
	public abstract class MailListener : SafeLog {
		private Imap imp;                                          // Imap object reference
		private bool exiting;                                      // Flag. If true, terminating the application is in progress and we should stop idle without attempt to download new messages
		private DateTime startTime;                                // Time when idle started. Used for restarting idle by timeout
		private readonly TimeSpan timeout = new TimeSpan(0, 5, 0); // 5 minutes timeout for idle

		protected long LastId;
		private readonly int mailboxReconnectionIntervalSeconds, port;
		private readonly string server, loginAddress, loginPassword, mailBeeLicenseKey;

		protected MailListener(int mailboxReconnectionIntervalSeconds, string server, int port, string loginAddress, string loginPassword, string mailBeeLicenseKey, ASafeLog oLog = null) : base(oLog) {
			this.mailboxReconnectionIntervalSeconds = mailboxReconnectionIntervalSeconds;
			this.server = server;
			this.port = port;
			this.loginAddress = loginAddress;
			this.loginPassword = loginPassword;
			this.mailBeeLicenseKey = mailBeeLicenseKey;
			AppDomain.CurrentDomain.ProcessExit += (s, e) => exiting = true;
			LicenseImap();
			ConnectToMailboxLoop();
		} // constructor

		protected UidCollection Search(string cond) {
			return (UidCollection)imp.Search(true, cond, null);
		} // Search

		private void ConnectToMailboxLoop() {
			while (true) {
				try {
					if (ConnectToMailbox())
						return;
				}
				catch (Exception e) {
					Error("Failed connecting to mailbox with error:", e);
					Info("Waiting {0} seconds", mailboxReconnectionIntervalSeconds);
					Thread.Sleep(TimeSpan.FromSeconds(mailboxReconnectionIntervalSeconds));
				}
			} // while
		} // ConnectToMailboxLoop

		private bool ConnectToMailbox() {
			imp = new Imap();

			// Enable SSL/TLS if necessary
			imp.SslMode = MailBee.Security.SslStartupMode.OnConnect;

			// Connect to IMAP server
			imp.Connect(server, port);
			Info("Connected to the server");

			// Check for IDLE support
			if (imp.GetExtension("IDLE") == null) {
				Info("IDLE not supported");
				imp.Disconnect();
				return false;
			} // if

			// Log into IMAP account
			imp.Login(loginAddress, loginPassword);
			Info("Logged into the server");

			// Select Inbox folder
			imp.SelectFolder("Inbox");

			// Add message status event handler which will "listen to server" while idling
			imp.MessageStatus += imp_MessageStatus;

			// Add idling event handler which is used for timer
			imp.Idling += imp_Idling;

			// Start idle timer
			TimerStart();

			// Start idling
			imp.BeginIdle(IdleCallback, null);
			Info("Started idling");
			return true;
		} // ConnectToMailbox

		private void LicenseImap() {
			// Licensing IMAP component. If the license key is invalid, it'll throw MailBeeLicenseException
			try {
				Global.LicenseKey = mailBeeLicenseKey;
			}
			catch (MailBeeLicenseException e) {
				Error("License key is invalid:{0}", e);
				Environment.Exit(-1);
			} // try
		} // LicenseImap

		/// <summary>
		/// Idling event handler. Ticks every 10 milliseconds while in idle state.
		/// </summary>
		private void imp_Idling(object sender, ImapIdlingEventArgs e) {
			try {
				// If the difference between start timer time and the current time >= timeout, initiate stopping idle
				if (DateTime.Now.Subtract(startTime) >= timeout) {
					imp.StopIdle();
					Info("Initiated stopping idle by TIMER");
				}
			}
			catch (Exception ex) {
				Error("Error occured while in imp_Idling:{0}", ex);
				Info("Trying to reconnect to mailbox");
				SafeDispose();
				ConnectToMailboxLoop();
			} // try
		} // imp_Idling

		/// <summary>
		/// MessageStatus event handler which receives notifications from IMAP server. Learns about new messages in idling state
		/// </summary>
		private void imp_MessageStatus(object sender, ImapMessageStatusEventArgs e) {
			try {
				Info("Got {0} status update", e.StatusID);

				// RECENT status means new messages have just arrived to IMAP account. Initiate stopping idle and IdleCallback will download the messages
				if ("RECENT" == e.StatusID || "EXISTS" == e.StatusID) {
					imp.StopIdle();
					Info("Initiated stopping idle");
				} // if
			}
			catch (Exception ex) {
				Error("Error occured while in imp_MessageStatus:{0}", ex);
				Info("Trying to reconnect to mailbox");
				SafeDispose();
				ConnectToMailboxLoop();
			} // try
		} // imp_MessageStatus

		/// <summary>
		/// Callback for BeginIdle. It'll be called after stopping idle and will download new messages
		/// </summary>
		private void IdleCallback(IAsyncResult result) {
			try {
				imp.EndIdle();

				// If not exiting, i.e. just stopping idle and we should try to download new messages
				// Exiting means the application is being terminated and we shouldn't try downloading new messages
				if (!exiting) {
					TimerStop();
					Info("Stopped idling, will download messages");

					UidCollection uids = FillUids();

					if (uids.Count != 0 && LastId != uids[uids.Count - 1]) {
						LastId = uids[uids.Count - 1];
						Info("Last received mail:{0}", LastId);
						MailMessageCollection msgs = imp.DownloadEntireMessages(uids.ToString(), true);
						HandleMessages(msgs);

						if (imp.IsIdle) {
							TimerStop();
							imp.StopIdle();
						} // if
					} // if

					// Messages have been downloaded and idling starts again
					TimerStart();
					imp.BeginIdle(IdleCallback, null);
					Info("Started idling again");
				}
				else {
					// If exiting is in progress, disconnect after stopping idle
					imp.Disconnect();
				} // if
			}
			catch (Exception ex) {
				Error("Error occured while in IdleCallback:{0}", ex);
				Info("Trying to reconnect to mailbox");
				SafeDispose();
				ConnectToMailboxLoop();
			} // try
		} // IdleCallback

		private void SafeDispose() {
			try {
				if (imp != null) {
					imp.MessageStatus -= imp_MessageStatus;
					imp.Idling -= imp_Idling;
					imp.Dispose();
				}
			}
			catch (Exception e) {
				Error("Failed disposing imap object:{0}", e);
			} // try
		} // SafeDispose

		private void TimerStart() {
			// Store the current time
			startTime = DateTime.Now;
			Info("TIMER started");
		} // TimerStart

		private void TimerStop() {
			Info("TIMER stopped");
		} // TimerStop

		protected abstract UidCollection FillUids();

		protected abstract void HandleMessages(MailMessageCollection msgs);
	} // class MailListener
} // namespace
