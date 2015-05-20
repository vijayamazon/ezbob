namespace Ezbob.Backend.CalculateLoan.Models.Exceptions {
	using Ezbob.Backend.CalculateLoan.Models.Helpers;

	public class WrongOpenPrincipalOrderException : ALoanCalculatorModelException {
		public WrongOpenPrincipalOrderException(OpenPrincipal previous, OpenPrincipal next)
			: base(
				"Open principal history item '" + previous +
				"' is not before open principal history item '" + next + "'."
			) {
		} // constructor
	} // class WrongOpenPrincipalOrderException
} // namespace
