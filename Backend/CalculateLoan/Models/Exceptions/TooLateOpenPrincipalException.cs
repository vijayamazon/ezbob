namespace Ezbob.Backend.CalculateLoan.Models.Exceptions {
	using System;
	using Ezbob.Backend.CalculateLoan.Models.Helpers;
	using Ezbob.Backend.Extensions;

	public class TooLateOpenPrincipalException : ALoanCalculatorModelException {
		public TooLateOpenPrincipalException(OpenPrincipal op, DateTime lastScheduledDate)
			: base(
				"There is no scheduled installment to cover open principal history item '" + op + "', " +
				"last scheduled date is '" + lastScheduledDate.DateStr() + "'."
			) {
		} // constructor
	} // class TooLateOpenPrincipalException
} // namespace
