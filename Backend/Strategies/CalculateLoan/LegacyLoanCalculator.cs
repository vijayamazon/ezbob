namespace Ezbob.Backend.Strategies.CalculateLoan {
	using Ezbob.Backend.Strategies.CalculateLoan.DailyInterestRate;

	public class LegacyLoanCalculator : ALoanCalculator {
		public LegacyLoanCalculator(LoanCalculatorModel model) : base(model, new LegacyCalculator()) {
		} // constructor
	} // class LegacyLoanCalculator
} // namespace
