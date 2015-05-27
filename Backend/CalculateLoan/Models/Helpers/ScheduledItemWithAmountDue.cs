namespace Ezbob.Backend.CalculateLoan.Models.Helpers {
	using System;

	public class ScheduledItemWithAmountDue {
		public ScheduledItemWithAmountDue(
			int position,
			DateTime date,
			decimal principal,
			decimal interestRate,
			decimal accruedInterest
		) {
			Position = position;
			Date = date.Date;
			Principal = principal;
			InterestRate = interestRate;
			AccruedInterest = accruedInterest;
		} // constructor

		public int Position { get; private set; }

		public DateTime Date { get; private set; }

		public decimal Principal { get; private set; }

		public decimal InterestRate { get; private set; }
		public decimal AccruedInterest { get; private set; }

		public decimal Amount { get { return Principal + AccruedInterest; } }
	} // class ScheduledItemWithAmountDue
} // namespace
