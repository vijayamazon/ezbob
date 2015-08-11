namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Methods {
	using System;
	using System.Linq;
	using Ezbob.Backend.CalculateLoan.Models;
	using Ezbob.Backend.CalculateLoan.Models.Extensions;
	using Ezbob.Backend.CalculateLoan.Models.Helpers;
	using Ezbob.Backend.Extensions;

	internal abstract class AGetAmountToChargeMethod : AWithActualDailyLoanStatusMethod {
		public virtual CurrentPaymentModel Execute() {
			var cpm = new CurrentPaymentModel();

			// TODO: revive

			/*

			if (this.today <= WorkingModel.LoanIssueTime.Date) {
				cpm.IsError = true;
				return cpm;
			} // if

			DailyLoanStatus days = CreateActualDailyLoanStatus(this.today);

			if (days.IsEmpty) {
				cpm.IsError = true;
				return cpm;
			} // if

			if (this.setClosedDateFromPayments)
				WorkingModel.SetScheduleCloseDatesFromPayments();

			cpm.Balance = days[this.today].CurrentBalance;
			cpm.AccruedInterest = days[this.today].AccruedInterest;

			bool allPreviousPaymentsAreClosed = WorkingModel.Schedule
				.Where(s => s.Date < this.today)
				.All(s => s.IsClosedOn(this.today));

			ScheduledItem currentPayment = WorkingModel.Schedule.FindByDate(this.today);

			if (currentPayment == null) { // today is not a payment date.
				if (allPreviousPaymentsAreClosed) // Delta scenario.
					AlphaDelta(cpm, days, "Delta");
				else { // Echo scenario.
					cpm.ScenarioName = "Echo";

					cpm.LoanIsClosed = false;

					OneDayLoanStatus thisDay = days[this.today];

					cpm.Amount =
						WorkingModel.Schedule.Where(s => s.Date < this.today).Sum(s => s.Principal) -
						thisDay.TotalRepaidPrincipal +
						thisDay.TotalExpectedNonprincipalPayment;

					cpm.IsLate = true;

					cpm.SavedAmount = 0;
				} // if
			} else { // today is a payment date.
				if (allPreviousPaymentsAreClosed) {
					if (currentPayment.IsClosedOn(this.today)) // Alpha scenario.
						AlphaDelta(cpm, days, "Alpha");
					else { // Bravo scenario.
						cpm.ScenarioName = "Bravo";
						cpm.LoanIsClosed = false;
						cpm.Amount = currentPayment.Principal + days[this.today].TotalExpectedNonprincipalPayment;
						cpm.IsLate = false;
						cpm.SavedAmount = 0;
					} // if
				} else { // Charlie scenario.
					cpm.ScenarioName = "Charlie";

					cpm.LoanIsClosed = false;

					OneDayLoanStatus thisDay = days[this.today];

					cpm.Amount = WorkingModel.Schedule.Where(s => s.Date < this.today).Sum(s => s.Principal) -
						thisDay.TotalRepaidPrincipal +
						thisDay.TotalExpectedNonprincipalPayment +
						currentPayment.Principal;

					cpm.IsLate = true;
					cpm.SavedAmount = 0;
				} // if
			} // if

			if (WriteToLog) {
				days.AddScheduleNotes(WorkingModel);
				days.AddFeeNotes(WorkingModel);
				days.AddPaymentNotes(WorkingModel);
				days.AddNote(this.today, "Requested balance date.");

				Log.Debug(
					"\n\n{4}.GetAmountToChargeOptions - begin:" +
					"\n\nLoan calculator model:\n{0}" +
					"\n\nPayment options on {3}:\n\t\t{1}" +
					"\n\nDaily data:\n{2}" +
					"\n\n{4}.GetAmountToChargeOptions - end." +
					"\n\n",
					WorkingModel,
					cpm,
					days.ToFormattedString("\t\t"), this.today.DateStr(),
					Calculator.Name
				);
			} // if
			*/

			return cpm;
		} // GetAmountToCharge

		protected AGetAmountToChargeMethod(
			ALoanCalculator calculator,
			DateTime today,
			bool setClosedDateFromPayments,
			bool writeToLog
		) : base(calculator, writeToLog) {
			this.today = today.Date;
			this.setClosedDateFromPayments = setClosedDateFromPayments;
		} // constructor

		protected enum RequesterType {
			CustomerDashboard,
			AutoCharger,
		} // enum RequesterType

		protected abstract RequesterType MyRequester { get; }

		// TODO: revive

		/*

		private void AlphaDelta(
			CurrentPaymentModel cpm,
			DailyLoanStatus days,
			string scenarioName
		) {
			cpm.ScenarioName = scenarioName;

			ScheduledItem firstOpen = WorkingModel.Schedule
				.FirstOrDefault(s => (s.Date > this.today) && !s.ClosedDate.HasValue);

			// ReSharper disable once PossibleInvalidOperationException
			// If firstOpen is not null then firstOpen.Date is not null:
			// this is checked during CreateActualDailyLoanStatus().
			OneDayLoanStatus thatDay = firstOpen == null ? null : days[firstOpen.Date];
			OneDayLoanStatus thisDay = days[this.today];

			cpm.LoanIsClosed = firstOpen == null;

			if ((firstOpen == null) || (MyRequester == RequesterType.AutoCharger))
				cpm.Amount = 0;
			else {
				// If firstOpen is not null then thisDay is not null
				// because of how CreateActualDailyLoanStatus works: thisDay corresponds to
				// today which is inserted as it is a requested date.
				cpm.Amount = firstOpen.OpenPrincipal + thisDay.TotalExpectedNonprincipalPayment;
			} // if

			cpm.IsLate = false;

			cpm.SavedAmount = ((thatDay == null) || (MyRequester == RequesterType.AutoCharger))
				? 0
				: thatDay.TotalExpectedNonprincipalPayment - thisDay.TotalExpectedNonprincipalPayment;
		} // AlphaDelta

		*/

		private readonly DateTime today;
		private readonly bool setClosedDateFromPayments;
	} // class AGetAmountToChargeMethod
} // namespace
