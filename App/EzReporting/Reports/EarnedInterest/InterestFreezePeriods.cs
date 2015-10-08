namespace Reports.EarnedInterest {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.ValueIntervals;

	// Alex, August 31 2015: according to sudden change in interest freeze implementation, interest freeze interval cannot
	// specify any interest rate (as it was until now) and frozen interest rate is always 0. Therefore interest freeze
	// intervals are allowed to intersect now.

	public class InterestFreezePeriods {
		public InterestFreezePeriods() {
			this.zeroInterestIntervals = null;
		} // constructor

		public void Add(DateTime? oStart, DateTime? oEnd) {
			if (this.zeroInterestIntervals == null)
				this.zeroInterestIntervals = new List<DateInterval>();

			this.zeroInterestIntervals.Add(new DateInterval(oStart, oEnd));
		} // Add

		public void ForEach(Action<DateInterval> it) {
			if (it == null)
				return;

			if (this.zeroInterestIntervals == null)
				return;

			foreach (DateInterval i in this.zeroInterestIntervals)
				it.Invoke(i as FreezeInterval);
		} // ForEach

		public decimal? GetInterest(DateTime oDate) {
			return this.zeroInterestIntervals.Any(i => i.Contains(oDate)) ? 0 : (decimal?)null;
		} // GetInterest

		public override string ToString() {
			return this.zeroInterestIntervals.ToString();
		} // ToString

		public int Count { get { return this.zeroInterestIntervals.Count; } } // Count

		private List<DateInterval> zeroInterestIntervals;
	} // class InterestFreezePeriods
} // namespace
