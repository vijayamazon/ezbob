namespace Ezbob.Backend.CalculateLoan.LoanCalculator {
	using System;

	interface ILoanCalculator {

		void CreateSchedule();  // result in model updated

		decimal CalculateApr(DateTime? aprDate = null, double? calculationAccuracy = null, ulong? maxIterationCount = null);

		void GetState(); // result in model updated

		void AmountToPay(DateTime calculationDate, bool getSavedAmount = false); // read result in public fields AmountToCharge, SavedAmount

		decimal NextEarlyPayment(DateTime calculationDate);

		decimal TotalEarlyPayment(DateTime calculationDate);
	}
}