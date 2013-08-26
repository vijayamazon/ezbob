using NUnit.Framework;

namespace EzAutoResponder
{
	using System;
	using System.Globalization;
	using System.ServiceProcess;
	using Ezbob.Logger;
	using MailBee;
	using MailBee.ImapMail;
	using MailBee.Mime;


	[TestFixture]
	public class Test
	{
		private Conf _cfg;
		private readonly ASafeLog _log = new SafeLog();
		private Imap _imap;

		[Test]
		public void confBaseTest()
		{
			_cfg = new Conf(_log);
			_cfg.Init();
		}

		[Test]
		public void mailbeeTest()
		{
			_cfg = new Conf(_log);
			_cfg.Init();

			try
			{
				Global.LicenseKey = _cfg.MailBeeLicenseKey;
			}
			catch (MailBeeLicenseException e)
			{
				_log.Error("License key is invalid: {0}", e);
				Mailer.Mailer.SendMail(_cfg.TestAddress, _cfg.TestPassword, "EzAutoresonder Error", e.ToString(), "stasdes@gmail.com");
			} // try


			_imap = new Imap { SslMode = MailBee.Security.SslStartupMode.OnConnect };

			// Connect to IMAP server
			_imap.Connect(_cfg.Server, _cfg.Port);
			_log.Info("Connected to the server");


			// Log into IMAP account
			_imap.Login(_cfg.TestAddress, _cfg.TestPassword);
			_log.Info("Logged into the server");

			// Select Inbox folder
			_imap.ExamineFolder("Inbox");
			var uids = (UidCollection)_imap.Search(true, "UNSEEN", null);

			if (uids.Count > 0)
			{
				// Download all the messages found by Search method
				MailMessageCollection msgs = _imap.DownloadMessageHeaders(uids.ToString(), true);

				// Iterate througn the messages collection and display info about them
				foreach (MailMessage msg in msgs)
				{
					Console.WriteLine("Message #" + msg.IndexOnServer.ToString(CultureInfo.InvariantCulture) +
					                  " has subject: " + msg.Subject + "  from: " + msg.From.Email + " received on: " +
					                  msg.DateReceived);
				}
			}
		}
	}
}
