namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions {
	public class InvalidInitialAmountException : ALoanCalculatorModelException {
		public InvalidInitialAmountException(decimal amount)
			: base(string.Format("Invalid initial loan amount of {0} was specified.", amount.ToString("C2", Library.Instance.Culture)))
		{
		} // constructor
	} // class InvalidInitialAmountException
} // namespace
