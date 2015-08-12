namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions {
	public class InvalidInitialRepaymentCountException : InvalidInitialDataException {
		public InvalidInitialRepaymentCountException(int repaymentCount)
			: base("Invalid initial repayment count of {0} was specified.", repaymentCount)
		{
		} // constructor
	} // class InvalidInitialRepaymentCountException
} // namespace
