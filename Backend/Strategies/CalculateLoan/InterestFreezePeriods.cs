namespace Ezbob.Backend.Strategies.CalculateLoan {
	using System;
	using Ezbob.ValueIntervals;

	public class InterestFreezePeriods {
		public InterestFreezePeriods() {
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

		public void ForEach(Action<FreezeInterval> it) {
			if (it == null)
				return;

			if (this.periods == null)
				return;

			foreach (TInterval<DateTime> i in this.periods)
				it.Invoke(i as FreezeInterval);
		} // ForEach

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

		public override string ToString() {
			return this.periods.ToString();
		} // ToString

		public int Count { get { return this.periods == null ? 0 : this.periods.Count; } } // Count

		public void DeepCloneFrom(InterestFreezePeriods source) {
			if (source == null)
				return;

			Clear();

			if (source.periods != null) {
				this.periods = new TDisjointIntervals<DateTime>();
				this.periods.Add(source.periods);
			} // if
		} // DeepCloneFrom

		private TDisjointIntervals<DateTime> periods;
	} // class InterestFreezePeriods
} // namespace
