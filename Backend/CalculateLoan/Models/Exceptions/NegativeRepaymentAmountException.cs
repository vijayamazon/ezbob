namespace Ezbob.Backend.CalculateLoan.Models.Exceptions {
	public class NegativeRepaymentAmountException : ANegativeDecimalException {
		public NegativeRepaymentAmountException(decimal repaymentAmount)
			: base("Repayment amount is negative: {0}.", repaymentAmount) {
		} // constructor
	} // class NegativeRepaymentAmountException
} // namespace
