﻿namespace Ezbob.Backend.CalculateLoan.LoanCalculator {
	public class NoLoanHistoryException : InvalidInitialDataException {
		public NoLoanHistoryException()
			: base("History data not found")
		{
		} // constructor
	} // class NoLoanHistoryException
} // namespace
