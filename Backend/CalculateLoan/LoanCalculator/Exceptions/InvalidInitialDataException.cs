namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions {
	public class InvalidInitialDataException : ALoanCalculatorModelException {
		protected internal InvalidInitialDataException(string msgFormat, params object[] args)
			: base(string.Format(msgFormat, args))
		{
		} // constructor
	} // class InvalidInitialDataException
} // namespace
