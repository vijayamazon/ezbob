namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions {
	public class InvalidInitialAmountException : InvalidInitialDataException {
		public InvalidInitialAmountException(decimal amount)
			: base("Invalid initial loan amount of {0} was specified.", amount.ToString("C2", Library.Instance.Culture))
		{
		} // constructor
	} // class InvalidInitialAmountException
} // namespace
