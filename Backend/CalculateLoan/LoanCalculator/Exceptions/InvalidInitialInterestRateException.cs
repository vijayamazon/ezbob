namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions {
	public class InvalidInitialInterestRateException : ALoanCalculatorModelException {
		public InvalidInitialInterestRateException(decimal rate)
			: base(string.Format("Invalid initial interest rate of {0} was specified.", rate.ToString("C2", Library.Instance.Culture))){
		} 
	} // class InvalidInitialInterestRateException
} // namespace
