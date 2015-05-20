﻿namespace Ezbob.Backend.CalculateLoan.Models.Exceptions {
	public class NegativeMonthlyInterestRateException : ANegativeDecimalException {
		public NegativeMonthlyInterestRateException(decimal loanAmount)
			: base("Monthly interest rate is negative: {0}.", loanAmount) {
		} // constructor
	} // class NegativeMonthlyInterestRateException
} // namespace
