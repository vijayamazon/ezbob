namespace Ezbob.Backend.Strategies.Misc {
	using System;
	using System.Threading;
	using Ezbob.Backend.Strategies.Exceptions;
	using Ezbob.Database;
	using MailBee;
	using MailBee.ImapMail;
	using MailBee.Mime;
	using MailApi;


	public class PacnetTopUpMailProcessor : AStrategy{
		/// <summary>
		/// listening to email message from pacnet with money transfer confirmation
		/// </summary>
		/// <param name="underwriterId">underwriter's system identifier</param>
		/// <param name="amount">amount of transferred money</param>
		/// <param name="reqMailSubject">email subject of message sent to pacnet</param>
		/// <param name="datesentreq">date of message sent to pacnet</param>
		public PacnetTopUpMailProcessor(int underwriterId, decimal amount, string reqMailSubject, DateTime dateSentReq) {
			this.underwriterId = underwriterId;
			this.amount = amount;
			this.totalWaitingTime = TimeSpan.FromMinutes(ConfigManager.CurrentValues.Instance.MaxTimeToWaitForPacnetrConfirmation);
			this.loginAddress = ConfigManager.CurrentValues.Instance.MailBeeLoginAddress;
			this.loginPassword = ConfigManager.CurrentValues.Instance.MailBeeLoginPassword;
			this.server = ConfigManager.CurrentValues.Instance.MailBeeServer;
			this.port = ConfigManager.CurrentValues.Instance.MailBeePort;
			this.mailboxReconnectionIntervalSeconds = ConfigManager.CurrentValues.Instance.MailBeeMailboxReconnectionIntervalSeconds;
			this.isLicenseKeyValid = IsLicenseKeyValid();
			this.reqMailSubject = reqMailSubject;
			this.dateSentReq = dateSentReq;
		} // constructor

		public override string Name {
			get { return "Send email/SMS to top-up when no answer from pacnet for an hour"; }
		} // Name

		private bool IsLicenseKeyValid() {
			// Licensing IMAP component. If the license key is invalid, it'll throw MailBeeLicenseException
			try {
				Global.LicenseKey = ConfigManager.CurrentValues.Instance.MailBeeLicenseKey;
			} catch (MailBeeLicenseException e) {
				Log.Info("License key is invalid: {0}", e);
				return false;
			} // try

			return true;
		} // IsLicenseKeyValid
		
		public override void Execute() {
			if (this.isLicenseKeyValid) {
				Mail mail = new Mail();
				TimeZoneInfo dublinTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
				DateTime startTime = DateTime.UtcNow;
				DateTime dublinStartTime = TimeZoneInfo.ConvertTimeFromUtc(startTime, dublinTimeZone);

				//Irish pacnet office boundary hour to receive pacnet confirmation regarding money transfer
				const int boundaryHour = 15;
				const int boundaryMinutes = 20;

				DateTime irishOfficeBoundaryMoment = startTime.Date.AddHours(boundaryHour).AddMinutes(boundaryMinutes);
				
				for (;;) {

					DateTime now = DateTime.UtcNow;
					DateTime dublinNow = TimeZoneInfo.ConvertTimeFromUtc(now, dublinTimeZone);

					bool boundaryMomentHasPassed = dublinNow > irishOfficeBoundaryMoment;

					//checking either has an hour passed since request for confirmation or is it later than 15:20 GMT and it is necessary to notify the top-up  
					if (now.Subtract(startTime).CompareTo(this.totalWaitingTime) > 0 || (dublinStartTime < irishOfficeBoundaryMoment && boundaryMomentHasPassed)) {
						Log.Info("Confirmation email did not arrive, not waiting for it any more. Alert Ezbob top-up");

						var topup = new TopUpDelivery(this.underwriterId, this.amount, 2);
						topup.Execute();

						break;
					} // if

					try {
						var imap = new Imap {
							// Enable SSL/TLS if necessary
							SslMode = MailBee.Security.SslStartupMode.OnConnect
						};

						imap.Connect(this.server, this.port);
						Log.Info("Connected to the server");

						imap.Login(this.loginAddress, this.loginPassword);
						Log.Info("Logged into the server");

						imap.SelectFolder("Inbox");

						if (FetchMaessages(imap))
							break;

						imap.Disconnect();
					} catch (PacnetMailProcessorException pex) {
						Log.Info("PacnetMailProcessorException: {0}", pex);
					} catch (MailBeeStreamException mex) {
						Log.Info("MailBeeStreamException: {0}", mex);
					} catch (Exception e) {
						Log.Info("Some generic Exception: {0}", e);
					} // try

					Log.Info("Sleeping...");
					
					Thread.Sleep(this.mailboxReconnectionIntervalSeconds * 1000);
				} // for
			} 
		}

		private bool FetchMaessages(Imap imap) {
			UidCollection uids = (UidCollection)imap.Search(true, "UNSEEN", null);

			Log.Info("Unseen email count: {0}", uids.Count);

			bool handledMail = false;

			if (uids.Count > 0) {
				Log.Info("Fetching unseen messages...");

				MailMessageCollection msgs = imap.DownloadEntireMessages(uids.ToString(), true);

				foreach (MailMessage msg in msgs) {
					Log.Info("Recent message index: {0} Subject: {1}", msg.IndexOnServer, msg.Subject);

					if (msg.Subject.Contains(this.reqMailSubject)) {
						Log.Info("Pacnet confirmation message has been received with subject: {0}", msg.Subject);
						handledMail = true;
						DB.ExecuteNonQuery(
							"SetPacnetTopUpConfirmationRequestConfirmed",
							CommandSpecies.StoredProcedure,
							new QueryParameter("DateSent", this.dateSentReq),
							new QueryParameter("DateConfirmed", TimeZoneInfo.ConvertTimeToUtc(msg.DateReceived))
						);
					} // if has matching subject 
				} // foreach email

				Log.Info("Fetching unseen messages complete.");

			} // if
			return handledMail;
		}

		private readonly int underwriterId;
		private readonly decimal amount;
		private readonly TimeSpan totalWaitingTime;
		private readonly string loginAddress;
		private readonly string loginPassword;
		private readonly string server;
		private readonly int port;
		private readonly int mailboxReconnectionIntervalSeconds;
		private readonly bool isLicenseKeyValid;
		private readonly string reqMailSubject;
		private readonly DateTime dateSentReq;
	}
}
