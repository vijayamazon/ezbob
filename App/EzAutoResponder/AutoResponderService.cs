namespace EzAutoResponder
{
	using System;
	using System.Collections.Generic;
	using System.ServiceProcess;
	using Ezbob.Database;
	using MailBee.ImapMail;
	using MailBee.Mime;
	using System.Diagnostics;
	using Ezbob.Logger;
	using MailBee;

	public partial class AutoResponderService : ServiceBase
	{
		private readonly Conf _cfg;
		private readonly ASafeLog _log;
		private Imap _imap;
		private readonly EventLog _eventLog = new EventLog("AutoResponderLog");
		private readonly TimeSpan _timeout = new TimeSpan(0, 0, 30); // 30 sec timeout for idle
		private DateTime _startTime;
		private bool _exiting; // Flag. If true, terminating the application is in progress and we should stop idle without attempt to download new messages

		public AutoResponderService()
		{
			InitializeComponent();
			_log = new SafeLog();
			_cfg = new Conf(_log);
			_cfg.Init();

			if (!EventLog.SourceExists("AutoResponderServiceSource"))
			{
				EventLog.CreateEventSource("AutoResponderServiceSource", "AutoResponderServiceLog");
			}

			_eventLog.Source = "AutoResponderServiceSource";
			_eventLog.Log = "AutoResponderServiceLog";

			InitImap();
		}

		private void InitImap()
		{
			try
			{
				Global.LicenseKey = _cfg.MailBeeLicenseKey;
			}
			catch (MailBeeLicenseException e)
			{
				_eventLog.WriteEntry(string.Format("License key is invalid: {0}", e), EventLogEntryType.Error);
				Mailer.Mailer.SendMail(_cfg.TestAddress, _cfg.TestPassword, "EzAutoresonder Error", e.ToString(), "stasdes@gmail.com");
			} // try

			_imap = new Imap {SslMode = MailBee.Security.SslStartupMode.OnConnect};

			// Connect to IMAP server
			_imap.Connect(_cfg.Server, _cfg.Port);
			_log.Info("Connected to the server");


			// Log into IMAP account
			_imap.Login(_cfg.TestAddress, _cfg.TestPassword);
			_log.Info("Logged into the server");

			// Select Inbox folder
			_imap.ExamineFolder("Inbox");

			// Add message status event handler which will "listen to server" while idling
			_imap.MessageStatus += new ImapMessageStatusEventHandler(_imap_MessageStatus);

			// Add idling event handler which is used for timer
			_imap.Idling += new ImapIdlingEventHandler(_imap_Idling);

			// Start idle timer
			TimerStart();

			// Start idling
			_imap.BeginIdle(new AsyncCallback(IdleCallback), null);
		}

		/// <summary>
		/// Start idle timer
		/// </summary>
		private void TimerStart()
		{
			// Store the current time
			_startTime = DateTime.Now;
		}

		/// <summary>
		/// Stop idle timer
		/// </summary>
		private void TimerStop()
		{
		}

		/// <summary>
		/// Idling event handler. Ticks every 10 milliseconds while in idle state.
		/// </summary>
		private void _imap_Idling(object sender, ImapIdlingEventArgs e)
		{
			// If the difference between start timer time and the current time >= timeout, initiate stopping idle
			if (DateTime.Now.Subtract(_startTime) >= _timeout)
			{
				_imap.StopIdle();
			}
		}

		/// <summary>
		/// MessageStatus event handler which receives notifications from IMAP server. Learns about new messages in idling state
		/// </summary>
		private void _imap_MessageStatus(object sender, ImapMessageStatusEventArgs e)
		{
			// RECENT status means new messages have just arrived to IMAP account. Initiate stopping idle and IdleCallback will download the messages
			if ("RECENT" == e.StatusID)
			{
				_imap.StopIdle();
			}
		}

		/// <summary>
		/// Callback for BeginIdle. It'll be called after stopping idle and will download new messages
		/// </summary>
		private void IdleCallback(IAsyncResult result)
		{
			_imap.EndIdle();

			// If not exiting, i.e. just stopping idle and we should try to download new messages
			// Exiting means the application is being terminated and we shouldn't try downloading new messages
			if (!_exiting)
			{
				TimerStop();

				// Search for UNSEEN (i.e. new) messages
				var uids = (UidCollection)_imap.Search(true, "UNSEEN", null);
				
				if (uids.Count > 0)
				{
					// Download all the messages found by Search method
					MailMessageCollection msgs = _imap.DownloadMessageHeaders(uids.ToString(), true);

					// Iterate througn the messages collection and display info about them
					foreach (MailMessage msg in msgs)
					{
						HandleMessage(msg);
					}
				}

				// Messages have been downloaded and idling starts again
				TimerStart();
				_imap.BeginIdle(new AsyncCallback(IdleCallback), null);
			}
			else
			{
				// If exiting is in progress, disconnect after stopping idle
				_imap.Disconnect();
			}
		}

		private void HandleMessage(MailMessage msg)
		{
			_eventLog.WriteEntry("Handle Message begin", EventLogEntryType.Information);
			var dateReceived = msg.DateReceived.ToUniversalTime();
			var timeReceived = dateReceived.TimeOfDay;
			//sending only for mails that where recieved between 19:00 and 06:00
			//var test = "";//todo remove test remove commneted out returns
			if (timeReceived < new TimeSpan(Const.HourAfter, 0, 0) &&
				timeReceived > new TimeSpan(Const.HourBefore, 0, 0))
			{
				_eventLog.WriteEntry("Time Constraint", EventLogEntryType.Information);
				return;
				//test += "Day Constraint (not sending);";
			}

			var oDb = new SqlConnection();
				var time = oDb.ExecuteScalar<DateTime?>(Const.GetLastAutoresponderDateSpName,
				                                        new QueryParameter(Const.EmailSpParam, msg.From.Email));

				//sending autoresponse only once in three days 
				if (time.HasValue && time.Value > dateReceived.AddDays(Const.ThreeDays))
				{
					_eventLog.WriteEntry("Count Constraint", EventLogEntryType.Information);
					//test += "Count in three days Constraint (not sending);";
					return;
				}

				oDb.ExecuteNonQuery(Const.InsertAutoresponderLogSpName,
				                    new QueryParameter(Const.EmailSpParam, msg.From.Email),
				                    new QueryParameter(Const.NameSpParam, msg.From.DisplayName));

			var mandrill = new Mandrill(_log);
			var vars = new Dictionary<string, string>
				{
					{"FNAME", msg.From.DisplayName ?? msg.From.AsString},
				};
			//todo change to sender email (msg.From.Email)
			mandrill.Send(vars, msg.From.Email, Const.MandrillAutoResponseTemplate);
		}

		protected override void OnStart(string[] args)
		{
			_eventLog.WriteEntry("On Start", EventLogEntryType.Information);
		}

		protected override void OnStop()
		{
			_eventLog.WriteEntry("On Stop", EventLogEntryType.Information);
			_eventLog.Dispose();
			if (_imap != null)
			{
				// If we're still idling, stop it and close the connection
				if (_imap.IsIdle)
				{
					_exiting = true;
					TimerStop();
					_imap.StopIdle();
				}
			}
		}
	}
}
