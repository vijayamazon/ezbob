namespace Reports.StrategyRunningTime {
	using System;
	using System.Collections.Generic;
	using Ezbob.Database;

	class StrategyData {
		#region public

		#region constructor

		public StrategyData(int nActionID, SafeReader sr) {
			m_oData = new SortedDictionary<Guid, ActionData>();
			m_oSuccessStat = new Stat();
			m_oFailStat = new Stat();
			m_oTotalStat = new Stat();
			UnknownCount = 0;

			if (nActionID != 0) {
				ID = nActionID;
				sr.Fill(this);

				AddAction(sr);
			} // if
		} // constructor

		#endregion constructor

		public int ID { get; private set; }

		[FieldName("ActionName")]
		public string Name { get; set; }

		public bool IsSync { get; set; }

		public int UnknownCount { get; private set; }

		#region method AddAction

		public void AddAction(SafeReader sr) {
			Guid oActionID = sr["ActionID"];

			if (m_oData.ContainsKey(oActionID))
				m_oData[oActionID].LoadTime(sr);
			else
				m_oData[oActionID] = new ActionData(oActionID, sr);
		} // AddAction

		#endregion method AddAction

		#region method CalculateTimes

		public void CalculateTimes() {
			var oSuccess = new SortedDictionary<double, int>();
			var oFail = new SortedDictionary<double, int>();
			var oTotal = new SortedDictionary<double, int>();

			foreach (var pair in m_oData) {
				ActionData oData = pair.Value;

				if (!oData.IsSuccess.HasValue) {
					UnknownCount++;
					continue;
				} // if

				if (!oData.Length.HasValue) {
					UnknownCount++;
					continue;
				} // if

				SortedDictionary<double, int> oInstances = oData.IsSuccess.Value ? oSuccess : oFail;
				Stat oStat = oData.IsSuccess.Value ? m_oSuccessStat : m_oFailStat;

				oStat.Append(oData.Length.Value, oData.StartTime.Value);
				m_oTotalStat.Append(oData.Length.Value, oData.StartTime.Value);

				if (oInstances.ContainsKey(oData.Length.Value))
					oInstances[oData.Length.Value]++;
				else
					oInstances[oData.Length.Value] = 1;

				if (oTotal.ContainsKey(oData.Length.Value))
					oTotal[oData.Length.Value]++;
				else
					oTotal[oData.Length.Value] = 1;
			} // for each

			m_oSuccessStat.SetAverage();
			m_oFailStat.SetAverage();
			m_oTotalStat.SetAverage();

			m_oSuccessStat.Median = GetMedian(oSuccess, m_oSuccessStat.Count);
			m_oFailStat.Median = GetMedian(oFail, m_oFailStat.Count);
			m_oTotalStat.Median = GetMedian(oTotal, m_oTotalStat.Count);
		} // CalculateTimes

		#endregion method CalculateTimes

		#region method ToRow

		public void ToRow(List<object> row) {
			m_oSuccessStat.ToRow(row);
			m_oFailStat.ToRow(row);
			m_oTotalStat.ToRow(row);
			row.Add(UnknownCount);
		} // ToRow

		#endregion method ToRow

		#endregion public

		#region private

		private readonly SortedDictionary<Guid, ActionData> m_oData;
		private readonly Stat m_oSuccessStat;
		private readonly Stat m_oFailStat;
		private readonly Stat m_oTotalStat;

		#region method GetMedian

		private static double GetMedian(SortedDictionary<double, int> oInstances, int nTotalCount) {
			double nMedian = 0;

			int nReachCount = nTotalCount / 2;

			int nCurCount = 0;

			foreach (var pair in oInstances) {
				nCurCount += pair.Value;

				if (nCurCount >= nReachCount) {
					nMedian = pair.Key;
					break;
				} // if
			} // for each

			return nMedian;
		} // GetMedian

		#endregion method GetMedian

		#endregion private
	} // class StrategyData
} // namespace
