namespace Reports.EarnedInterest {
	using System;
	using Ezbob.ValueIntervals;

	public class InterestFreezePeriods {

		public InterestFreezePeriods() {
			m_oPeriods = null;
		} // constructor

		public void Add(DateTime? oStart, DateTime? oEnd, decimal nInterestRate) {
			var fi = new FreezeInterval(oStart, oEnd, nInterestRate);

			if (m_oPeriods == null) {
				m_oPeriods = new TDisjointIntervals<DateTime>(fi);
				return;
			} // if

			var oNew = new TDisjointIntervals<DateTime>();
			var oDiff = new TDisjointIntervals<DateTime>(fi);

			foreach (TInterval<DateTime> i in m_oPeriods) {
				var oCurrent = i as FreezeInterval;

				if (null == i)
					continue; // should never happen

				oNew += oCurrent - fi;
				oNew += fi * oCurrent;

				var oNewDiff = new TDisjointIntervals<DateTime>();

				foreach (TInterval<DateTime> j in oDiff) {
					var oCurDiff = j as FreezeInterval;
					oNewDiff += oCurDiff - oCurrent;
				} // for each interval in difference

				oDiff = oNewDiff;
			} // foreach

			m_oPeriods = oNew + oDiff;
		} // Add

		public void ForEach(Action<FreezeInterval> it) {
			if (it == null)
				return;

			if (m_oPeriods == null)
				return;

			foreach (TInterval<DateTime> i in m_oPeriods)
				it.Invoke(i as FreezeInterval);
		} // ForEach

		public decimal? GetInterest(DateTime oDate) {
			var d = new DateIntervalEdge(oDate, AIntervalEdge<DateTime>.EdgeType.Finite);

			foreach (TInterval<DateTime> i in m_oPeriods) {
				if (i.Contains(d)) {
					var fi = i as FreezeInterval;

					return fi == null ? null : fi.InterestRate;
				} // if
			} // foreach

			return null;
		} // GetInterest

		public override string ToString() {
			return m_oPeriods.ToString();
		} // ToString

		public int Count { get { return m_oPeriods.Count; } } // Count

		private TDisjointIntervals<DateTime> m_oPeriods;

	} // class InterestFreezePeriods
} // namespace
