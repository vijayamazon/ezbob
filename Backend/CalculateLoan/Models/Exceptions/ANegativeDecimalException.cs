namespace Ezbob.Backend.CalculateLoan.Models.Exceptions {
	public abstract class ANegativeDecimalException : ALoanCalculatorModelException {
		protected ANegativeDecimalException(string format, decimal val) : base(string.Format(format, val)) {
		} // constructor
	} // class ANegativeDecimalException
} // namespace
