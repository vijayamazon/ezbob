namespace EzAutoResponder
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using Ezbob.Database;
	using Ezbob.Logger;
	using MailBee;
	using MailBee.ImapMail;
	using MailBee.Mime;
	using Mailer;

	public class ImapIdler
	{
		private Imap _imap;
		private readonly Conf _cfg;
		private readonly ASafeLog _log;
		private readonly EventLog _eventLog;
		private readonly TimeSpan _timeout;
		private DateTime _startTime;
		private bool _exiting; // Flag. If true, terminating the application is in progress and we should stop idle without attempt to download new messages
		private readonly SqlConnection _oDb;
		public ImapIdler(string address, string password, EventLog eventLog)
		{
			_log = new SafeLog();
			_cfg = new Conf(_log);
			_cfg.Init();
			_eventLog = eventLog;
			_timeout = new TimeSpan(0, _cfg.AutoResponderIdleTimeoutMinutes, _cfg.AutoResponderIdleTimeoutSeconds); // 30 sec timeout for idle
			_oDb = new SqlConnection();
			InitImap(address, password);
		}

		public void StopIdle()
		{
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

		private void InitImap(string address, string password)
		{
			_eventLog.WriteEntry("Init Imap " + address, EventLogEntryType.Information);
			try
			{
				Global.LicenseKey = _cfg.MailBeeLicenseKey;
			}
			catch (MailBeeLicenseException e)
			{
				_eventLog.WriteEntry(string.Format("License key is invalid: {0}", e), EventLogEntryType.Error);
				Mailer.SendMail(address, password, "EzAutoresonder Error", e.ToString(), "stasdes@gmail.com");
			} // try

			_imap = new Imap { SslMode = MailBee.Security.SslStartupMode.OnConnect };

			// Connect to IMAP server
			_imap.Connect(_cfg.Server, _cfg.Port);
			_eventLog.WriteEntry("Connected to the server", EventLogEntryType.Information);

			try
			{
				if (!_imap.Login(address, password))
				{
					_eventLog.WriteEntry("Wrong mail credentials Login failed to " + address, EventLogEntryType.Error);
					throw new Exception();
				}
			}
			catch (Exception ex)
			{
				_eventLog.WriteEntry("Login exception (Wrong mail credentials for " + address + ") " + ex, EventLogEntryType.Error);
				throw ex;
			}
			// Log into IMAP account

			_eventLog.WriteEntry("Logged into the server", EventLogEntryType.Information);

			// Select Inbox folder
			_imap.SelectFolder("Inbox");

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
			//_eventLog.WriteEntry("Handle Message begin", EventLogEntryType.Information);
			if (IsExcludedMail(msg.From.Email))
			{
				return;//Ignore mails from ezbob@ezbob.com and mailer-daemon@googlemail.com
			}

			var dateReceived = msg.DateReceived.ToUniversalTime();

			if (dateReceived < DateTime.Today.AddDays(_cfg.AutoRespondCountDays))
			{
				//_eventLog.WriteEntry("Old Unseen mail", EventLogEntryType.Information);
				return;
			}

			if (!IsNotExceededCount(dateReceived, msg.From.Email))
			{
				return;
			}

			if (!IsWeekend(dateReceived) && !IsNight(dateReceived))
			{
				return;
			}

			_oDb.ExecuteNonQuery(Const.InsertAutoresponderLogSpName,
								new QueryParameter(Const.EmailSpParam, msg.From.Email),
								new QueryParameter(Const.NameSpParam, msg.From.DisplayName));

			var mandrill = new Mandrill(_log, _cfg.AutoRespondMandrillApiKey);
			var vars = new Dictionary<string, string>
				{
					{"FNAME", msg.From.DisplayName ?? msg.From.AsString},
				};

			_eventLog.WriteEntry("Autoresponding to " + msg.From.Email, EventLogEntryType.Information);
			mandrill.Send(vars, msg.From.Email, IsWeekend(dateReceived) ? _cfg.AutoRespondMandrillWeekendTemplate : _cfg.AutoRespondMandrillNightTemplate);
		}

		private static bool IsExcludedMail(string from)
		{
			return Const.ExcludedSendersEmails.Any(em => @from == em);
		}

		private bool IsWeekend(DateTime dateReceived)
		{
			if (!_cfg.AutoRespondWeekendConstraintEnabled)
			{
				return false;
			}
			var timeReceived = dateReceived.TimeOfDay;
			var w = new WeekendMarker((DayOfWeek)Enum.Parse(typeof(DayOfWeek), _cfg.AutoRespondWeekendDayBegin),
									  (DayOfWeek)Enum.Parse(typeof(DayOfWeek), _cfg.AutoRespondWeekendDayEnd));
			if ((w.IsWeekendBegin(dateReceived.DayOfWeek) && timeReceived >= new TimeSpan(_cfg.AutoRespondWeekendHourBegin, 0, 0)) ||
				w.IsWeekendMiddle(dateReceived.DayOfWeek) ||
				(w.IsWeekendEnd(dateReceived.DayOfWeek) && timeReceived <= new TimeSpan(_cfg.AutoRespondWeekendHourEnd, 0, 0)))
			{
				return true;
			}

			return false;
		}

		private bool IsNight(DateTime dateReceived)
		{
			if (!_cfg.AutoRespondNightConstraintEnabled)
			{
				return false;
			}
			var timeReceived = dateReceived.TimeOfDay;
			if (timeReceived < new TimeSpan(_cfg.AutoRespondAfterHour, 0, 0) &&
				timeReceived > new TimeSpan(_cfg.AutoRespondBeforeHour, 0, 0))
			{
				return false;
			}

			return true;
		}

		private bool IsNotExceededCount(DateTime dateReceived, string email)
		{
			if (!_cfg.AutoRespondCountConstraintEnabled)
			{
				return false;
			}

			var time = _oDb.ExecuteScalar<DateTime?>(Const.GetLastAutoresponderDateSpName,
													new QueryParameter(Const.EmailSpParam, email));

			//sending autoresponse only once in three days 
			if (time.HasValue && time.Value > dateReceived.AddDays(_cfg.AutoRespondCountDays))
			{
				return false;
			}

			return true;
		}
	}
}
