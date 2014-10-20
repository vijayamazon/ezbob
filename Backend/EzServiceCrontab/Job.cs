namespace EzServiceCrontab {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using ArgumentTypes;
	using EzBob.Backend.Strategies;
	using EzService;
	using EzService.EzServiceImplementation;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using Ezbob.Utils.Lingvo;

	internal class Job {
		#region constructor

		public Job(EzServiceInstanceRuntimeData oRuntimeData, SafeReader sr, TypeRepository oTypeRepo) {
			WhyNotStarting = "never checked whether it is time to start";

			m_oRuntimeData = oRuntimeData;

			m_oArguments = new List<JobArgument>();

			sr.Fill(this);

			AddArgument(sr, oTypeRepo);
		} // constructor

		#endregion constructor

		#region method AddArgument

		public void AddArgument(SafeReader sr, TypeRepository oTypeRepo) {
			long nArgID = sr["ArgumentID"];

			if (nArgID > 0)
				m_oArguments.Add(new JobArgument(sr, oTypeRepo));
		} // AddArgument

		#endregion method AddArgument

		#region properties

		#region from DB

		// ID property must be declared before RepetitionTypeID because properties are filled from DB in order of declaration.
		[FieldName("JobID")]
		public long ID { get; set; }

		public string ActionName { get; set; }
		public int ActionNameID { get; set; }

		#region property RepetitionTypeID

		public int RepetitionTypeID {
			get { return m_nRepetitionTypeID; }
			set {
				m_nRepetitionTypeID = value;

				try {
					RepetitionType = (RepetitionType)Enum.ToObject(typeof (RepetitionType), m_nRepetitionTypeID);
				}
				catch (Exception e) {
					throw new InvalidCastException("Invalid repetition type " + m_nRepetitionTypeID + " for job " + ID + ".", e);
				} // try
			} // set
		} // RepetitionTypeID

		private int m_nRepetitionTypeID;

		#endregion property RepetitionTypeID

		public DateTime RepetitionTime { get; set; }

		#region property LastActionStatusID

		public int? LastActionStatusID {
			get { return m_nLastActionStatusID; }
			set {
				m_nLastActionStatusID = value;

				if (m_nLastActionStatusID == null)
					LastActionStatus = ActionStatus.Unknown;
				else {
					try {
						LastActionStatus = (ActionStatus)Enum.ToObject(typeof (ActionStatus), m_nLastActionStatusID.Value);
					}
					catch (Exception e) {
						throw new InvalidCastException("Invalid action status " + m_nLastActionStatusID.Value + " for job " + ID + ".", e);
					} // try
				} // if
			} // set
		} // LastActionStatusID

		private int? m_nLastActionStatusID;

		#endregion property LastActionStatusID

		public DateTime? LastStartTime { get; set; }
		public DateTime? LastEndTime { get; set; }
		public bool IsInProgress { get; set; }

		#endregion from DB

		public RepetitionType RepetitionType { get; private set; }
		public ActionStatus LastActionStatus { get; private set; }

		#region property RepetitionStr

		public string RepetitionStr {
			get {
				switch (RepetitionType) {
				case RepetitionType.Monthly:
					return GetMonthlyRepetitionTimeStr(RepetitionTime, true);

				case RepetitionType.Daily:
					return GetDailyRepetitionTimeStr(RepetitionTime, true);

				case RepetitionType.EveryXMinutes:
					return GetXMinutesRepetitionTimeStr(RepetitionTime.Hour, RepetitionTime.Minute, RepetitionMinutes, true);

				default:
					throw new ArgumentOutOfRangeException();
				} // switch
			} // get
		} // RepetitionStr

		#endregion property RepetitionStr

		#endregion properties

		#region method ToString

		public override string ToString() {
			string sArgs;

			if (m_oArguments.Count == 0)
				sArgs = " Without arguments.";
			else
				sArgs = " " + Grammar.Number(m_oArguments.Count, "argument") + ":\n\t" + string.Join("\n\t", m_oArguments);

			return string.Format(
				"{0}: {1}({2}) {3}. Status: {7} ({4}). Times: {5} - {6}.{8}",
				ID,
				ActionName,
				ActionNameID,
				RepetitionStr,
				LastActionStatus,
				LastStartTime == null ? "never" : LastStartTime.Value.ToString("MMM d yyyy H:mm:ss", CultureInfo.InvariantCulture),
				LastEndTime == null ? "none" : LastEndTime.Value.ToString("MMM d yyyy H:mm:ss", CultureInfo.InvariantCulture),
				IsInProgress ? "running" : "stopped",
				sArgs
			);
		} // ToString

		#endregion method ToString

		#region method Differs

		public bool Differs(Job oPrevious) {
			if (oPrevious == null)
				return true;

			if (this.ActionNameID != oPrevious.ActionNameID)
				return true;

			if (this.RepetitionType != oPrevious.RepetitionType)
				return true;

			if (this.RepetitionStr != oPrevious.RepetitionStr)
				return true;

			if (this.m_oArguments.Count != oPrevious.m_oArguments.Count)
				return true;

			for (int i = 0; i < m_oArguments.Count; i++)
				if (this.m_oArguments[i].Differs(oPrevious.m_oArguments[i]))
					return true;

			return false;
		} // Differs

		#endregion method Differs

		#region method IsTimeToStart

		public bool IsTimeToStart(DateTime oNow) {
			bool bStart;

			switch (RepetitionType) {
			case RepetitionType.Monthly:
				bStart = (oNow.Day == RepetitionTime.Day) && (oNow.Hour == RepetitionTime.Hour) && (oNow.Minute == RepetitionTime.Minute);

				WhyNotStarting = bStart
					? Starting
					: string.Format("the task should run {0} while now is {1}", RepetitionStr, GetMonthlyRepetitionTimeStr(oNow, false));

				return bStart;

			case RepetitionType.Daily:
				bStart = (oNow.Hour == RepetitionTime.Hour) && (oNow.Minute == RepetitionTime.Minute);

				WhyNotStarting = bStart
					? Starting
					: string.Format("the task should run {0} while now is {1}", RepetitionStr, GetDailyRepetitionTimeStr(oNow, false));

				return bStart;

			case RepetitionType.EveryXMinutes:
				if (LastEndTime == null) {
					WhyNotStarting = Starting;
					return true;
				} // if

				TimeSpan oDelta = oNow - LastEndTime.Value;

				if (oDelta.TotalMinutes >= RepetitionMinutes) {
					WhyNotStarting = Starting;
					return true;
				} // if

				WhyNotStarting = string.Format("the task should run {0} while it has completed {1} ago",
					RepetitionStr,
					GetXMinutesRepetitionTimeStr(oDelta.Hours, oDelta.Minutes, (int)oDelta.TotalMinutes, false)
				);

				return false;

			default:
				throw new ArgumentOutOfRangeException();
			} // switch
		} // IsTimeToStart

		#endregion method IsTimeToStart

		#region property WhyNotStarting

		public string WhyNotStarting { get; private set; }

		#endregion property WhyNotStarting

		#region method Start

		public void Start() {
			try {
				Type oStrategyType = TypeUtils.FindType(ActionName);

				if (oStrategyType == null)
					Log.Alert("Job.Start: could not start the job {0}: strategy not found by name '{1}'.", this, ActionName);
				else {
					var oArgs = new ExecuteArguments {
						StrategyType = oStrategyType,
						OnInit = OnInit,
						OnException = OnException,
						OnLaunch = OnLaunch,
						OnSuccess = OnSuccess,
						OnFail = OnFail,
					};

					foreach (var oArg in m_oArguments)
						oArgs.StrategyArguments.Add(oArg.UnderlyingType.CreateInstance(oArg.Value));

					new EzServiceImplementation(m_oRuntimeData).Execute(oArgs);
				} // if
			}
			catch (Exception e) {
				Log.Alert(e, "Job.Start: failed to start the job {0}.", this);
			} // try
		} // Start

		#endregion method Start

		#region private

		#region property DB

		private AConnection DB {
			get { return m_oRuntimeData.DB; }
		} // DB

		#endregion property DB

		#region property Log

		private ASafeLog Log {
			get { return m_oRuntimeData.Log; }
		} // Log

		#endregion property Log

		private readonly EzServiceInstanceRuntimeData m_oRuntimeData;

		private readonly List<JobArgument> m_oArguments;

		#region property RepetitionMinutes

		private int RepetitionMinutes {
			get { return RepetitionTime.Hour * 60 + RepetitionTime.Minute; }
		} // RepetitionMinutes

		#endregion property RepetitionMinutes

		#region method OnInit

		private void OnInit(AStrategy oInstance, ActionMetaData oMetaData) {
			DB.ExecuteNonQuery(
				"UpdateCronjobStarted",
				CommandSpecies.StoredProcedure,
				new QueryParameter("JobID", ID),
				new QueryParameter("ActionStatusID", (int)ActionStatus.InProgress),
				new QueryParameter("ActionID", oMetaData.ActionID),
				new QueryParameter("Now", DateTime.UtcNow)
			);
		} // OnInit

		#endregion method OnInit

		#region method OnSuccess

		private void OnSuccess(AStrategy oInstance, ActionMetaData oMetaData) {
			DB.ExecuteNonQuery(
				"UpdateCronjobEnded",
				CommandSpecies.StoredProcedure,
				new QueryParameter("JobID", ID),
				new QueryParameter("ActionStatusID", (int)ActionStatus.Done),
				new QueryParameter("ActionID", oMetaData.ActionID),
				new QueryParameter("Now", DateTime.UtcNow)
			);
		} // OnSuccess

		#endregion method OnSuccess

		#region method OnException

		private void OnException(ActionMetaData oMetaData) {
			DB.ExecuteNonQuery(
				"UpdateCronjobEnded",
				CommandSpecies.StoredProcedure,
				new QueryParameter("JobID", ID),
				new QueryParameter("ActionStatusID", (int)ActionStatus.Finished),
				new QueryParameter("ActionID", oMetaData.ActionID),
				new QueryParameter("Now", DateTime.UtcNow)
			);
		} // OnException

		#endregion method OnException

		#region method OnFail

		private void OnFail(ActionMetaData oMetaData) {
			DB.ExecuteNonQuery(
				"UpdateCronjobEnded",
				CommandSpecies.StoredProcedure,
				new QueryParameter("JobID", ID),
				new QueryParameter("ActionStatusID", (int)ActionStatus.Failed),
				new QueryParameter("ActionID", oMetaData.ActionID),
				new QueryParameter("Now", DateTime.UtcNow)
			);
		} // OnFail

		#endregion method OnFail

		#region method OnLaunch

		private void OnLaunch(ActionMetaData oMetaData) {
			DB.ExecuteNonQuery(
				"UpdateCronjobStatus",
				CommandSpecies.StoredProcedure,
				new QueryParameter("JobID", ID),
				new QueryParameter("ActionStatusID", (int)ActionStatus.Launched),
				new QueryParameter("ActionID", oMetaData.ActionID),
				new QueryParameter("Now", DateTime.UtcNow)
			);
		} // OnLaunch

		#endregion method OnLaunch

		#region method GetMonthlyRepetitionTimeStr

		private static string GetMonthlyRepetitionTimeStr(DateTime oTime, bool bAddRepetition) {
			string sSuffix = "th";

			switch (oTime.Day % 10) {
			case 1:
				sSuffix = "st";
				break;
			case 2:
				sSuffix = "nd";
				break;
			case 3:
				sSuffix = "rd";
				break;
			} // switch

			return string.Format(
				"{3}{0}{1} at {2}",
				oTime.Day,
				sSuffix,
				oTime.ToString("H:mm", CultureInfo.InvariantCulture),
				bAddRepetition ? "monthly on " : string.Empty
			);
		} // GetMonthlyRepetitionTimeStr

		#endregion method GetMonthlyRepetitionTimeStr

		#region method GetDailyRepetitionTimeStr

		private static string GetDailyRepetitionTimeStr(DateTime oTime, bool bAddRepetition) {
			return string.Format(
				"{1}{0}",
				oTime.ToString("H:mm", CultureInfo.InvariantCulture),
				bAddRepetition ? "daily at " : string.Empty
			);
		} // GetDailyRepetitionTimeStr

		#endregion method GetDailyRepetitionTimeStr

		#region method GetXMinutesRepetitionTimeStr

		private static string GetXMinutesRepetitionTimeStr(int nHour, int nMinute, int nTotalMinutes, bool bAddRepetition) {
			string sTime;

			if (nHour > 0) {
				sTime = string.Format(
					"{0} {1} (i.e. {2})",
					Grammar.Number(nHour, "hour"),
					Grammar.Number(nMinute, "minute"),
					Grammar.Number(nTotalMinutes, "minute")
				);
			}
			else
				sTime = Grammar.Number(nMinute, "minute");

			return string.Format("{1}{0}", sTime, bAddRepetition ? "every " : string.Empty);
		} // GetXMinutesRepetitionTimeStr

		#endregion method GetXMinutesRepetitionTimeStr

		private const string Starting = "starting";

		#endregion private
	} // class Job
} // namespace
