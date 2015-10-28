﻿namespace Ezbob.Backend.CalculateLoan.LoanCalculator {
	using System;
	using Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions;
	using Ezbob.Backend.ModelsWithDB.NewLoan;

	public class BankLikeLoanCalculator : ALoanCalculator {
		/// <exception cref="NoInitialDataException">Condition. </exception>
		public BankLikeLoanCalculator(NL_Model model): base(model) {
			
		} // constructor

		public override string Name { get { return "Bank-like calculator"; } }

		public override decimal AverageDailyInterestRate(decimal monthlyInterestRate, DateTime? periodEndDate = null) {
			return monthlyInterestRate * 12.0m / 365.0m;
		} // AverageDailyInterestRate

	} // class BankLikeCalculator
} // namespace
