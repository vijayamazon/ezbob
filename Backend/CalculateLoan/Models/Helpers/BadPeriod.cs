namespace Ezbob.Backend.CalculateLoan.Models.Helpers {
	using System;
	using Ezbob.Backend.Extensions;
	using Ezbob.ValueIntervals;

	public class BadPeriod : DateInterval, IEquatable<BadPeriod> {
		public BadPeriod(DateTime intervalStart, DateTime intervalEnd) : base(intervalStart, intervalEnd) {
			IntervalStart = intervalStart;
			IntervalEnd = intervalEnd;
		} // constructor

		public DateTime IntervalStart { get; set; }

		public DateTime IntervalEnd { get; set; }

		public BadPeriod DeepClone() {
			return new BadPeriod(IntervalStart, IntervalEnd);
		} // DeepClone

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(BadPeriod other) {
			if (other == null)
				return false;
		
			if (ReferenceEquals(this, other))
				return true;
		
			return IntervalStart.Equals(other.IntervalStart) && IntervalEnd.Equals(other.IntervalEnd);
		} // Equals

		public override int GetHashCode() {
			return IntervalStart.GetHashCode() ^ IntervalEnd.GetHashCode();
		} // GetHashCode

		public override string ToString() {
			return string.Format("{0} - {1}", IntervalStart.MomentStr(), IntervalEnd.MomentStr());
		} // ToString
	} // class BadPeriods
} // namespace
