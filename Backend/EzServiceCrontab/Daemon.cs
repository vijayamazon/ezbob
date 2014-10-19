namespace EzServiceCrontab {
	using System;
	using System.Threading;
	using ArgumentTypes;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Lingvo;

	public class Daemon {
		#region public

		#region constructor

		public Daemon(AConnection oDB, ASafeLog oLog) {
			if (oDB == null)
				throw new NullReferenceException("Database connection not specified.");

			m_oLastCheckedMinute = null;

			m_oStopLock = new object();
			m_bStopFlag = false;

			m_oDB = oDB;
			m_oLog = oLog ?? new SafeLog();

			m_oTypeRepo = new TypeRepository(m_oLog);

		} // constructor

		#endregion constructor

		#region method Execute

		public void Execute() {
			for ( ; ; ) {
				if (IsNewMinute) {
					if (!StopFlag)
						Reinit();

					if (!StopFlag)
						StartAll();
				} // if

				if (StopFlag)
					break;

				int nSleepTime = Math.Abs(1000 - DateTime.UtcNow.Millisecond);

				if (nSleepTime < 200)
					nSleepTime += 1000;

				Thread.Sleep(nSleepTime);

				if (StopFlag)
					break;
			} // while
		} // Execute

		#endregion method Execute

		#region method Shutdown

		public void Shutdown() {
			StopFlag = true;
		} // Shutdown

		#endregion method Shutdown

		#endregion public

		#region private

		#region method Reinit

		private void Reinit() {
			try {
				m_oLog.Debug("Updating crontab job list...");

				var oAllJobs = new JobSet();

				var oSp = new LoadEzServiceCrontab(m_oDB, m_oLog) { IncludeRunning = true, };

				oSp.ForEachRowSafe(sr => {
					try {
						long nJobID = sr["JobID"];

						if (oAllJobs.Contains(nJobID))
							oAllJobs[nJobID].AddArgument(sr, m_oTypeRepo);
						else
							oAllJobs[nJobID] = new Job(sr, m_oTypeRepo);
					}
					catch (Exception e) {
						m_oLog.Alert(e, "Failed to load a crontab job or its argument from DB.");
					} // try
				}); // for each row

				if (oAllJobs.HasChanged(m_oJobs)) {
					if (oAllJobs.Count == 0)
						m_oLog.Debug("No jobs loaded from DB.");
					else {
						m_oLog.Debug("{0} loaded from DB:", Grammar.Number(oAllJobs.Count, "crontab job"));

						oAllJobs.Iterate((nJobID, oJob) => m_oLog.Debug("Job: {0}", oJob));

						m_oLog.Debug("End of crontab job list.");
					} // if
				} // if

				m_oJobs = oAllJobs;

				m_oLog.Debug("Updating crontab job list complete.");
			}
			catch (Exception e) {
				m_oLog.Alert(e, "Exception caught during crontab daemon reinitialisation.");
			} // try
		} // Reinit

		#endregion method Reinit

		#region property StopFlag

		private bool StopFlag {
			get {
				lock (m_oStopLock)
					return m_bStopFlag;
			} // get

			set {
				lock (m_oStopLock)
					m_bStopFlag = value;
			} // set
		} // StopFlag

		private bool m_bStopFlag;
		private readonly object m_oStopLock;

		#endregion property StopFlag

		#region property IsNewMinute

		private bool IsNewMinute {
			get {
				if (!m_oLastCheckedMinute.HasValue) {
					m_oLastCheckedMinute = DateTime.UtcNow;
					return true;
				} // if

				DateTime oNow = DateTime.UtcNow;

				bool bIsNew =
					(oNow.Year != m_oLastCheckedMinute.Value.Year) ||
					(oNow.DayOfYear != m_oLastCheckedMinute.Value.DayOfYear) ||
					(oNow.Hour != m_oLastCheckedMinute.Value.Hour) ||
					(oNow.Minute != m_oLastCheckedMinute.Value.Minute);

				if (bIsNew)
					m_oLastCheckedMinute = oNow;

				return bIsNew;
			} // get
		} // IsNewMinute

		private DateTime? m_oLastCheckedMinute;

		#endregion property IsNewMinute

		#region method StartAll

		private void StartAll() {
			try {
				m_oLog.Debug("Crontab.StartAll started with {0} in the crontab.", Grammar.Number(m_oJobs.Count, "job"));

				m_oLog.Debug("Crontab.StartAll complete.");
			}
			catch (Exception e) {
				m_oLog.Alert(e, "Exception caught during starting a task via crontab daemon.");
			} // try
		} // StartAll

		#endregion method StartAll

		private readonly AConnection m_oDB;
		private readonly ASafeLog m_oLog;
		private readonly TypeRepository m_oTypeRepo;

		private JobSet m_oJobs;

		#endregion private
	} // class Daemon
} // namespace
