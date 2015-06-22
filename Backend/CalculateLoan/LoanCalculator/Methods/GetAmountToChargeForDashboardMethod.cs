namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Methods {
	using System;

	internal class GetAmountToChargeForDashboardMethod : AGetAmountToChargeMethod {
		public GetAmountToChargeForDashboardMethod(
			ALoanCalculator calculator,
			DateTime today,
			bool setClosedDateFromPayments,
			bool writeToLog
		) : base(calculator, today, setClosedDateFromPayments, writeToLog) {
		} // constructor

		protected override RequesterType MyRequester { get { return RequesterType.CustomerDashboard; } }
	} // class GetAmountToChargeForDashboardMethod
} // namespace
