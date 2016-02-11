namespace Ezbob.Backend.CalculateLoan.LoanCalculator {
	using System;

    public interface ILoanCalculator {

		void CreateSchedule();  // result in model updated

		double CalculateApr(DateTime? aprDate = null, double? calculationAccuracy = null, ulong? maxIterationCount = null);

		void GetState(); // result in model updated

		//void AmountToPay(bool showSavedAmount = false); // read result in public fields AmountToCharge, SavedAmount

		//decimal NextEarlyPayment();

		//decimal TotalEarlyPayment();
	}
}