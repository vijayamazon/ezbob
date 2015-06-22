namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Methods {
	using System;

	internal class GetAmountToChargeForAutoChargerMethod : AGetAmountToChargeMethod {
		public GetAmountToChargeForAutoChargerMethod(
			ALoanCalculator calculator,
			DateTime today,
			bool setClosedDateFromPayments,
			bool writeToLog
		) : base(calculator, today, setClosedDateFromPayments, writeToLog) {
		} // constructor

		protected override RequesterType MyRequester { get { return RequesterType.AutoCharger; } }
	} // class GetAmountToChargeForAutoChargerMethod
} // namespace
