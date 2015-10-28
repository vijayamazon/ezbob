namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions {
	using System;
	using Ezbob.Backend.Extensions;

	public class TooEarlyPrincipalRepaymentException : ALoanCalculatorModelException {
		public TooEarlyPrincipalRepaymentException(DateTime repaymentTime, DateTime lastKnownTime)
			: base(
				string.Format("Cannot add principal repayment at '{0}' before repayment at '{1}'.",
				repaymentTime.MomentStr(),
				lastKnownTime.MomentStr()
			)
		) {
		} // constructor
	} // class TooEarlyPrincipalRepaymentException
} // namespace
