namespace EzAutoResponder
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using Ezbob.Database;
	using Ezbob.Logger;
	using MailBee;
	using MailBee.ImapMail;
	using MailBee.Mime;
	using NUnit.Framework;

	[TestFixture]
	public class Test
	{
		private Conf _cfg;
		private readonly ASafeLog _log = new SafeLog();
		private Imap _imap;

		[SetUp]
		public void Init()
		{
			_cfg = new Conf(_log);
			_cfg.Init();
		}

		[Test]
		[Ignore]
		public void TestConfBase()
		{
			_cfg = new Conf(_log);
			_cfg.Init();
		}

		[Test]
		[Ignore]
		public void TestMailbee()
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
				//Mailer.Mailer.SendMail(_cfg.TestAddress, _cfg.TestPassword, "EzAutoresonder Error", e.ToString(), "stasdes@gmail.com");
			} // try

			_imap = new Imap { SslMode = MailBee.Security.SslStartupMode.OnConnect };

			// Connect to IMAP server
			_imap.Connect(_cfg.Server, _cfg.Port);
			_log.Info("Connected to the server");

			// Log into IMAP account
			_imap.Login("","");//todo enter a login
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

		[Test]
		[Ignore]
		public void TestDb()
		{
			var oDb = new SqlConnection();
			var time = oDb.ExecuteScalar<DateTime?>(Const.GetLastAutoresponderDateSpName,
				new QueryParameter(Const.EmailSpParam, "stasdes@gmail.com"));

			oDb.ExecuteNonQuery(Const.InsertAutoresponderLogSpName,
								new QueryParameter(Const.EmailSpParam, "stasdes@gmail.com"),
								new QueryParameter(Const.NameSpParam, "Stas Dulman"));

		}

		[Test]
		[Ignore]
		public void TestTime()
		{
			var dateReceived = (new DateTime(2013,8,27,21,58,00)).ToUniversalTime();
			var timeReceived = dateReceived.TimeOfDay;
			//sending only for mails that where recieved between 19:00 and 06:00
			//var test = "";//todo remove test remove commneted out returns
			if (timeReceived < new TimeSpan(_cfg.AutoRespondAfterHour, 0, 0) &&
				timeReceived > new TimeSpan(_cfg.AutoRespondBeforeHour, 0, 0))
			{
				Console.WriteLine("day {0}", DateTime.UtcNow.TimeOfDay);
			}
			else
			{
				Console.WriteLine("night {0}", DateTime.UtcNow.TimeOfDay);
			}

			var oDb = new SqlConnection();
			var time = oDb.ExecuteScalar<DateTime?>(Const.GetLastAutoresponderDateSpName,
				new QueryParameter(Const.EmailSpParam, "stasdes@gmail.com"));
			if (time.HasValue && time.Value > DateTime.UtcNow.AddDays(_cfg.AutoRespondCountDays))
			{
				Console.WriteLine("less than 3 days");
			}
			else
			{
				Console.WriteLine("more than 3 days");
			}
		}

		[Test]
		[Ignore]
		public void TestWeekend()
		{
			Console.WriteLine(_cfg.AutoRespondNightConstraintEnabled);
			var w = new WeekendMarker((DayOfWeek)Enum.Parse(typeof(DayOfWeek), _cfg.AutoRespondWeekendDayBegin), (DayOfWeek)Enum.Parse(typeof(DayOfWeek), _cfg.AutoRespondWeekendDayEnd));
			//var dateReceived = (new DateTime(2013, 8, 27, 21, 58, 00)).ToUniversalTime(); //tuesday
			//var dateReceived = (new DateTime(2013, 9, 13, 21, 58, 00)).ToUniversalTime(); // friday night
			//var dateReceived = (new DateTime(2013, 9, 14, 11, 58, 00)).ToUniversalTime(); // suturday evening
			//var dateReceived = (new DateTime(2013, 9, 15, 5, 00, 00)).ToUniversalTime(); // sunday morning
			var dateReceived = (new DateTime(2013, 9, 15, 22, 00, 00)).ToUniversalTime(); // sunday night
			var timeReceived = dateReceived.TimeOfDay;

			if ((w.IsWeekendBegin(dateReceived.DayOfWeek) && timeReceived >= new TimeSpan(_cfg.AutoRespondWeekendHourBegin, 0, 0)) || 
				 w.IsWeekendMiddle(dateReceived.DayOfWeek) ||
				(w.IsWeekendEnd(dateReceived.DayOfWeek) && timeReceived <= new TimeSpan(_cfg.AutoRespondWeekendHourEnd, 0, 0)))
			{
					Console.WriteLine("weekend");
					return;
			}

			Console.WriteLine("not weekend");

		}

		[Test]
		[Ignore]
		public void TestMandrill()
		{
			var m = new Mandrill(_log, _cfg.AutoRespondMandrillApiKey);
			var vars = new Dictionary<string, string>
				{
					{"FNAME", "Stas Dulman"},
				};
			m.Send(vars, "stasdes@gmail.com", "AutoresponderTest", "Autoresponder Test");
		}

		[Test]
		public void TestSplit()
		{
			string[] mails = "asd sdgf sdgs	dg$#S  DA".Split(' ');
		}
	}
}
