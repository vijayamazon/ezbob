namespace Ezbob.Backend.CalculateLoan.Models.Helpers {
	using System;
	using System.Text;
	using Ezbob.ValueIntervals;

	public class BadPeriod : DateInterval, IEquatable<BadPeriod> {


		public BadPeriod(DateTime intervalStart, DateTime intervalEnd) : base( intervalStart, intervalEnd) {
			IntervalStart = intervalStart;
			IntervalEnd = intervalEnd;
		} // constructor

		public DateTime IntervalStart { get; set; }

		public DateTime IntervalEnd { get; set; }

		// Status change times should be added in chronological order

		/*public void Add(DateTime intervalStart, DateTime intervalEnd) {
			this.badPeriods.Add(new DateInterval(intervalStart, intervalEnd));
		} // Add

		public void Remove(DateTime intervalStart, DateTime intervalEnd) {
			this.badPeriods.Remove(new DateInterval(intervalStart, intervalEnd));
		} */// Remove

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(BadPeriod other) {
		
			if (Object.ReferenceEquals(other, null))
				return false;
		
			if (Object.ReferenceEquals(this, other))
				return true;
		
			return IntervalStart.Equals(other.IntervalStart) && IntervalEnd.Equals(other.IntervalEnd);
		}

		public override int GetHashCode() {
			int hashIntervalStart = IntervalStart.GetHashCode();
			int hashIntervalEnd = IntervalEnd.GetHashCode();
			return hashIntervalStart ^ hashIntervalEnd;
		}

		public override string ToString() {
			StringBuilder sb = new StringBuilder(this.GetType().Name + ": ");
			Type t = typeof(BadPeriod);
			foreach (var prop in t.GetProperties()) {
				if (prop.GetValue(this) != null)
					sb.Append(prop.Name).Append(": ").Append(prop.GetValue(this)).Append("; \t");
			}
			return sb.ToString();
		}
		
	} // class BadPeriods
} // namespace
