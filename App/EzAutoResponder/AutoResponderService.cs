namespace EzAutoResponder
{
	using System;
	using System.Collections.Generic;
	using System.ServiceProcess;
	using System.Diagnostics;
	using Ezbob.Logger;

	public partial class AutoResponderService : ServiceBase
	{
		private readonly Conf _cfg;
		private readonly ASafeLog _log;
		private readonly EventLog _eventLog = new EventLog("AutoResponderLog");
		private readonly List<ImapIdler> _imapIdlerList = new List<ImapIdler>(2); 

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
			string[] mails = _cfg.AutoResponderMails.Split(' ');
			string[] passwords = _cfg.AutoResponderPasswords.Split(' ');
			if (mails.Length != passwords.Length)
			{
				_eventLog.WriteEntry("Wrong emails passwords count", EventLogEntryType.Error);
				return;
			}

			for (int i = 0; i < mails.Length; i++)
			{
				ImapIdler ImapIdler;
				try
				{
					ImapIdler = new ImapIdler(mails[i], passwords[i], _eventLog);
				}
				catch (Exception)
				{
					continue;
				}
				_imapIdlerList.Add(ImapIdler);
			}
		}

		protected override void OnStart(string[] args)
		{
			_eventLog.WriteEntry("On Start", EventLogEntryType.Information);
		}

		protected override void OnStop()
		{
			_eventLog.WriteEntry("On Stop", EventLogEntryType.Information);
			_eventLog.Dispose();

			foreach (var imapIdler in _imapIdlerList)
			{
				imapIdler.StopIdle();
			}
			
		}
	}
}
