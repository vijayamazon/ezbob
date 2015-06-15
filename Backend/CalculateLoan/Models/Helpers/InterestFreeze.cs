namespace Ezbob.Backend.CalculateLoan.Models.Helpers {
	using System;
	using System.Text;
	using Ezbob.ValueIntervals;

	public class InterestFreeze : DateInterval, IEquatable<InterestFreeze> {

		public InterestFreeze(DateTime startDate, DateTime endDate, decimal? interestRate = null, bool isActive = true)
			: base(startDate, endDate) {

			StartDate = startDate;
			EndDate = endDate;
			InterestRate = interestRate;
			IsActive = isActive;

		} // constructor

		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public decimal? InterestRate { get; set; }
		public bool IsActive { get; set; }


		public decimal? GetInterest(DateTime ddate) {
			if (Contains(ddate)) {
				if (IsActive)
					return InterestRate;
			}
			return null;
		}

		public bool Equals(InterestFreeze other) {
			if (Object.ReferenceEquals(other, null))
				return false;
			if (Object.ReferenceEquals(this, other))
				return true;
			return StartDate.Equals(other.StartDate) && EndDate.Equals(other.EndDate);
		}

		public override int GetHashCode() {
			int hashStart = StartDate.GetHashCode();
			int hashEnd = EndDate.GetHashCode();
			return hashStart ^ hashEnd;
		}

		public override string ToString() {
			StringBuilder sb = new StringBuilder(this.GetType().Name + ": ");
			Type t = typeof(InterestFreeze);
			foreach (var prop in t.GetProperties()) {
				if (prop.GetValue(this) != null)
					sb.Append(prop.Name).Append(": ").Append(prop.GetValue(this)).Append("; \t");
			}
			return sb.ToString();
		}

	} // class InterestFreeze
} // namespace
