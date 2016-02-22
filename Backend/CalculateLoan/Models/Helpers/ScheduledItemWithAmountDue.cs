namespace Ezbob.Backend.CalculateLoan.Models.Helpers {
	using System;
	using System.Globalization;
	using Ezbob.Backend.Extensions;

	public class ScheduledItemWithAmountDue {
		public ScheduledItemWithAmountDue(
			int position,
			DateTime date,
			decimal principal,
			decimal interestRate,
			decimal accruedInterest,
			decimal? fee
		) {
			Position = position;
			Date = date.Date;
			Principal = principal;
			InterestRate = interestRate;
			AccruedInterest = accruedInterest;
			Fee = fee ?? 0m;
		} // constructor

		public int Position { get; private set; }

		public DateTime Date { get; private set; }

		public decimal Principal { get; private set; }

		public decimal InterestRate { get; private set; }
		public decimal AccruedInterest { get; private set; }

		public decimal? Fee { get; private set; }

        // AmountDue
		public decimal Amount { get { return (decimal)(Principal + AccruedInterest + Fee); } }

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString() {
			return string.Format(
				"{0,2}: on {1}: {2} = p{3} + i{4} +f{5} (at {6})",
				Position,
				Date.DateStr(),
				Amount.ToString("C2", Culture),
				Principal.ToString("C2", Culture),
				AccruedInterest.ToString("C2", Culture),
				Fee,
				InterestRate.ToString("P2", Culture)
			);
		} // ToString

		private static CultureInfo Culture {
			get { return Models.Library.Instance.Culture; }
		}
	} // class ScheduledItemWithAmountDue
} // namespace
