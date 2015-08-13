namespace Ezbob.Backend.CalculateLoan.LoanCalculator {
	using System;
	using Ezbob.Backend.ModelsWithDB.NewLoan;

	public class BankLikeLoanCalculator : ALoanCalculator {
		public BankLikeLoanCalculator(NL_Model model) : base(model) {
		} // constructor

		public override string Name { get { return "Bank-like calculator"; } }
		
		protected override decimal CalculateDailyInterestRate(
			DateTime currentDate,
			decimal monthlyInterestRate,
			DateTime? periodStartDate = null,
			DateTime? periodEndDate = null
		) {
			return monthlyInterestRate * 12.0m / 365.0m;
		} // CalculateDailyInterestRate
	} // class BankLikeCalculator
} // namespace
