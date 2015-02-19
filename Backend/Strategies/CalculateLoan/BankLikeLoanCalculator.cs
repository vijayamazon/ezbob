namespace Ezbob.Backend.Strategies.CalculateLoan {
	using Ezbob.Backend.Strategies.CalculateLoan.DailyInterestRate;

	public class BankLikeLoanCalculator : ALoanCalculator {
		public BankLikeLoanCalculator(LoanCalculatorModel model) : base(model, new BankLikeCalculator()) {
		} // constructor
	} // class BankLikeCalculator
} // namespace
