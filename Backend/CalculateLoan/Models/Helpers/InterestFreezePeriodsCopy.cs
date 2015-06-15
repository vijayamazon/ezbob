namespace Ezbob.Backend.CalculateLoan.Models.Helpers {
	using System;
	using Ezbob.ValueIntervals;

	public class InterestFreezePeriodsCopy {

		public InterestFreezePeriodsCopy() {
			this.periods = null;
		} // constructor

		public void Clear() {
			if (this.periods != null) {
				this.periods.Clear();
				this.periods = null;
			} // if
		} // Clear

		public void Add(DateTime? oStart, DateTime? oEnd, decimal nInterestRate) {
			var fi = new FreezeInterval(oStart, oEnd, nInterestRate);

			if (this.periods == null) {
				this.periods = new TDisjointIntervals<DateTime>(fi);
				return;
			} // if

			var oNew = new TDisjointIntervals<DateTime>();
			var oDiff = new TDisjointIntervals<DateTime>(fi);

			foreach (TInterval<DateTime> i in this.periods) {
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

			this.periods = oNew + oDiff;
		} // Add

		/// <summary>
		/// returns interest rate by sent date argument (contains)
		/// </summary>
		/// <param name="oDate"></param>
		/// <returns></returns>
		public decimal? GetInterest(DateTime oDate) {
			var d = new DateIntervalEdge(oDate, AIntervalEdge<DateTime>.EdgeType.Finite);

			if (this.periods == null)
				return null;

			foreach (TInterval<DateTime> i in this.periods) {
				if (i.Contains(d)) {
					var fi = i as FreezeInterval;

					return fi == null ? null : fi.InterestRate;
				} // if
			} // foreach

			return null;
		} // GetInterest

		public int Count { get { return this.periods == null ? 0 : this.periods.Count; } } // Count

		public override string ToString() {
			return this.periods.ToString();
		} // ToString

		//public void DeepCloneFrom(InterestFreezePeriods source) {
		//	if (source == null)
		//		return;

		//	Clear();

		//	if (source.periods != null) {
		//		this.periods = new TDisjointIntervals<DateTime>();
		//		this.periods.Add(source.periods);
		//	} // if
		//} // DeepCloneFrom

		private TDisjointIntervals<DateTime> periods;
	} // class InterestFreezePeriodsCopy
} // namespace
