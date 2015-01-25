namespace PaymentServices.Calculators {
	using System;
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;

	public static class PayEarlyExtensions {
		public static decimal NextEarlyPayment(this Loan loan, DateTime? term = null, int amountToChargeFrom = 0) {
			return new LoanRepaymentScheduleCalculator(loan, term, amountToChargeFrom).NextEarlyPayment();
		}

		public static decimal TotalEarlyPayment(this Loan loan, DateTime? term = null, int amountToChargeFrom = 0) {
			return new LoanRepaymentScheduleCalculator(loan, term, amountToChargeFrom).TotalEarlyPayment();
		}

		public static decimal TotalEarlyPayment(this Customer customer, DateTime? term = null) {
			return customer.Loans.Sum(l => l.TotalEarlyPayment(term));
		}
	}
}