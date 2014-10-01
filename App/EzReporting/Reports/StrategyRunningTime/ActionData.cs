namespace Reports.StrategyRunningTime {
	using System;
	using Ezbob.Database;

	class ActionData {
		#region constructor

		public ActionData(Guid oActionID, SafeReader sr) {
			m_bHasValue = false;

			ID = oActionID;

			StartTime = null;
			EndTime = null;
			IsSuccess = null;

			LoadTime(sr);
		} // constructor

		#endregion constructor

		public Guid ID { get; private set; }
		public DateTime? StartTime { get; private set; }
		public DateTime? EndTime { get; private set; }
		public bool? IsSuccess { get; private set; }

		#region method LoadTime

		public void LoadTime(SafeReader sr) {
			int nStatusID = sr["ActionStatusID"];

			if (nStatusID == 1)
				StartTime = sr["EntryTime"];
			else {
				IsSuccess = nStatusID == 2;
				EndTime = sr["EntryTime"];
			} // if
		} // LoadTime

		#endregion method LoadTime

		#region property Length

		public double? Length {
			get {
				if (m_bHasValue)
					return m_nLength;

				m_bHasValue = true;

				if (StartTime.HasValue && EndTime.HasValue)
					m_nLength = (EndTime.Value - StartTime.Value).TotalMilliseconds;

				return m_nLength;
			} // get
		} // Length

		private double? m_nLength;
		private bool m_bHasValue;

		#endregion method Length
	} // class ActionData
} // namespace
