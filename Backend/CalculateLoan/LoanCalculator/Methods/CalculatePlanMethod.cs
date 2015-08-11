namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Methods {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DbConstants;
	using Ezbob.Backend.CalculateLoan.Models;
	using Ezbob.Backend.CalculateLoan.Models.Exceptions;
	using Ezbob.Backend.CalculateLoan.Models.Helpers;

	internal class CalculatePlanMethod : AMethod {
		public CalculatePlanMethod(ALoanCalculator calculator, bool writeToLog) : base(calculator, writeToLog) {
			DailyLoanStatus = new DailyLoanStatus();
		} // constructor

		public DailyLoanStatus DailyLoanStatus { get; private set; }

		/// <exception cref="NoScheduleException">Condition. </exception>
		/// <exception cref="WrongInstallmentOrderException">Condition. </exception>
		/// <exception cref="WrongFirstOpenPrincipalException">Condition. </exception>
		/// <exception cref="TooLateOpenPrincipalException">Condition. </exception>
		/// <exception cref="WrongOpenPrincipalOrderException">Condition. </exception>
		/// <exception cref="NegativeLoanAmountException">Condition. </exception>
		public virtual List<Repayment> Execute() {
			WorkingModel.ValidateSchedule();

			DateTime firstInterestDay = WorkingModel.LoanIssueTime.Date.AddDays(1);

			DateTime lastInterestDay = WorkingModel.LastScheduledDate;

			// fill in loan period (since first interest day until last interest day) with "one day" loan status
			for (DateTime d = firstInterestDay; d <= lastInterestDay; d = d.AddDays(1))
				DailyLoanStatus.Add(new OneDayLoanStatus(d, WorkingModel.LoanAmount, DailyLoanStatus.LastDailyLoanStatus));

			// init prevTime by loan issue date
			DateTime prevTime = WorkingModel.LoanIssueTime;

			// 
			for (int i = 0; i < WorkingModel.Schedule.Count; i++) {
				ScheduledItem scheduleItem = WorkingModel.Schedule[i];

				// decrease "one day" loan status entries by schedule planned Principal to be paid at schedule planned Date
				foreach (OneDayLoanStatus cls in DailyLoanStatus.Where(dd => dd.Date >= scheduleItem.Date)) {
					if (cls.Date != scheduleItem.Date)
						cls.OpenPrincipalForInterest -= scheduleItem.Principal;

					cls.OpenPrincipalAfterRepayments -= scheduleItem.Principal;
				} // for each

				DateTime preScheduleEnd = prevTime; // This assignment is to prevent "access to modified closure" warning.

				foreach (OneDayLoanStatus cls in DailyLoanStatus.Where(cls => preScheduleEnd < cls.Date && cls.Date <= scheduleItem.Date)) {
					cls.DailyInterestRate = Calculator.GetDailyInterestRate(
						cls.Date,
						scheduleItem.InterestRate,
						false, // considerBadPeriods
						false, // considerFreezeInterestPeriod
						preScheduleEnd,
						scheduleItem.Date
					);
				} // for each

				prevTime = scheduleItem.Date;
			} // for each scheduled payment

			var result = new List<Repayment>(
				WorkingModel.Schedule.Select(sp => new Repayment(sp.Date, sp.Principal, 0, 0))
			);

			prevTime = WorkingModel.LoanIssueTime;

			for (int i = 0; i < result.Count; i++) {
				Repayment r = result[i];

				DateTime dt = prevTime; // This assignment is to prevent "access to modified closure" warning.

				r.Interest = DailyLoanStatus.Where(cls => dt < cls.Date && cls.Date <= r.Time).Sum(cls => cls.DailyInterest);

				prevTime = r.Time;
			} // for

			foreach (Fee fee in WorkingModel.Fees) {
				if (fee.FeeType == FeeTypes.SetupFee)
					continue;

				Repayment scheduledPaymentToPayFee =
					result.FirstOrDefault(sp => sp.Date >= fee.AssignDate) ??
					result.Last();

				if (scheduledPaymentToPayFee == null) { // Should never happen but just in case...
					Library.Instance.Log.Alert(
						"{0}.CreatePlan: failed to find a scheduled payment to pay a fee.\n\tFee is: {1}." +
						"\n\tLoan calculator model:\n{2}",
						Calculator.Name,
						fee,
						WorkingModel
					);

					continue;
				} // if

				scheduledPaymentToPayFee.Fees += fee.Amount;
			} // for each fee

			if (WriteToLog) {
				DailyLoanStatus.AddScheduleNotes(WorkingModel);

				Log.Debug(
					"\n\n{3}.CreatePlan - begin:" +
					"\n\nLoan calculator model:\n{0}" +
					"\n\nLoan plan:\n\t\t{1}" +
					"\n\nDaily data:\n{2}" +
					"\n\n{3}.CreatePlan - end." +
					"\n\n",
					WorkingModel,
					string.Join("\n\t\t", result),
					DailyLoanStatus.ToFormattedString("\t\t"),
					Calculator.Name
				);
			} // if

			return result;
		} // Execute
	} // class CalculatePlanMethod
} // namespace
