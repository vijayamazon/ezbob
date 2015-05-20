namespace Ezbob.Backend.CalculateLoan.Models.Exceptions {
	using Ezbob.Backend.CalculateLoan.Models.Helpers;

	public class WrongFirstOpenPrincipalException : ALoanCalculatorModelException {
		public WrongFirstOpenPrincipalException(OpenPrincipal firstOp, OpenPrincipal loan)
			: base(
				"First open principal history item '" + firstOp + "' does not correspond to loan characteristics " +
				"'" + loan + "'."
			) {
		} // constructor
	} // class WrongFirstOpenPrincipalException
} // namespace
