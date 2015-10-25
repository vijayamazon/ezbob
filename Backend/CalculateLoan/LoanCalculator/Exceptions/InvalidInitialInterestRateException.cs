namespace Ezbob.Backend.CalculateLoan.LoanCalculator {
	public class InvalidInitialInterestRateException : InvalidInitialDataException {
		public InvalidInitialInterestRateException(decimal rate)
			: base("Invalid initial interest rate of {0} was specified.", rate.ToString("C2", Library.Instance.Culture))
		{
		} // constructor
	} // class InvalidInitialInterestRateException
} // namespace
