namespace EzServiceCrontab {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using ArgumentTypes;
	using EzService;
	using Ezbob.Database;
	using Ezbob.Utils.Lingvo;

	internal class Job {
		#region constructor

		public Job(SafeReader sr, TypeRepository oTypeRepo) {
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
				string sResult;

				switch (RepetitionType) {
				case RepetitionType.Monthly:
					string sSuffix = "th";

					switch (RepetitionTime.Day % 10) {
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

					sResult = string.Format("monthly on {0}{1} at {2}", RepetitionTime.Day, sSuffix, RepetitionTime.ToString("H:mm", CultureInfo.InvariantCulture));
					break;

				case RepetitionType.Daily:
					sResult = string.Format("daily at {0}", RepetitionTime.ToString("H:mm", CultureInfo.InvariantCulture));
					break;

				case RepetitionType.EveryXMinutes:
					string sTime;

					if (RepetitionTime.Hour > 0) {
						int nMinutes = RepetitionTime.Hour * 60 + RepetitionTime.Minute;

						sTime = string.Format(
							"{0} {1} (i.e. {2})",
							Grammar.Number(RepetitionTime.Hour, "hour"),
							Grammar.Number(RepetitionTime.Minute, "minute"),
							Grammar.Number(nMinutes, "minute")
						);
					}
					else
						sTime = Grammar.Number(RepetitionTime.Minute, "minute");

					sResult = string.Format("every {0}", sTime);
					break;

				default:
					throw new ArgumentOutOfRangeException();
				} // switch

				return sResult;
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

		#region private

		private readonly List<JobArgument> m_oArguments;

		#endregion private
	} // class Job
} // namespace
