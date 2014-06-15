namespace Reports.EarnedInterest {
	using System;
	using Ezbob.ValueIntervals;

	public class InterestFreezePeriods {
		#region public

		#region constructor

		public InterestFreezePeriods() {
			m_oPeriods = null;
		} // constructor

		#endregion constructor

		#region method Add

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

		#endregion method Add

		#region method ForEach

		public void ForEach(Action<FreezeInterval> it) {
			if (it == null)
				return;

			if (m_oPeriods == null)
				return;

			foreach (TInterval<DateTime> i in m_oPeriods)
				it.Invoke(i as FreezeInterval);
		} // ForEach

		#endregion method ForEach

		#region method GetInterest

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

		#endregion method GetInterest

		#region method ToString

		public override string ToString() {
			return m_oPeriods.ToString();
		} // ToString

		#endregion method ToString

		#region property Count

		public int Count { get { return m_oPeriods.Count; } } // Count

		#endregion property Count

		#endregion public

		#region private

		private TDisjointIntervals<DateTime> m_oPeriods;

		#endregion private
	} // class InterestFreezePeriods
} // namespace
