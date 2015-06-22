namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Methods {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using Ezbob.Backend.CalculateLoan.Models.Exceptions;
	using Ezbob.Backend.CalculateLoan.Models.Helpers;

	internal abstract class AWithActualDailyLoanStatusMethod : AMethod {
		protected AWithActualDailyLoanStatusMethod(ALoanCalculator calculator, bool writeToLog)
			: base(calculator, writeToLog) {
		} // constructor

		/// <exception cref="NoScheduleException">Condition. </exception>
		/// <exception cref="WrongInstallmentOrderException">Condition. </exception>
		/// <exception cref="WrongFirstOpenPrincipalException">Condition. </exception>
		/// <exception cref="TooLateOpenPrincipalException">Condition. </exception>
		/// <exception cref="WrongOpenPrincipalOrderException">Condition. </exception>
		/// <exception cref="NegativeLoanAmountException">Condition. </exception>
		[SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
		protected virtual DailyLoanStatus CreateActualDailyLoanStatus(DateTime now) {

			WorkingModel.ValidateSchedule();

			DateTime firstInterestDay = WorkingModel.LoanIssueTime.Date.AddDays(1);

			DateTime lastInterestDay = WorkingModel.Schedule.Last().Date;

			var days = new DailyLoanStatus();

			// Step 1. Create a list of days within scheduled period.
			// Open principal is equal to 0.
			// Daily interest is 0 at this point.

			days.Add(new OneDayLoanStatus(WorkingModel.LoanIssueTime, 0, null));

			for (DateTime d = firstInterestDay; d <= lastInterestDay; d = d.AddDays(1))
				days.Add(new OneDayLoanStatus(d, 0, days.LastDailyLoanStatus));

			// Step 2. Fill open principal according to open principal history.

			foreach (OpenPrincipal op in WorkingModel.OpenPrincipalHistory) {
				DateTime curOpDate = op.Date;

				foreach (OneDayLoanStatus cls in days.Where(dd => dd.Date >= curOpDate))
					cls.OpenPrincipal = op.Amount;
			} // for each

			// Step 3. Fill daily interest for each day within scheduled period.
			// Open principal is not changed.

			DateTime prevTime = WorkingModel.LoanIssueTime;

			for (int i = 0; i < WorkingModel.Schedule.Count; i++) {
				ScheduledItem sp = WorkingModel.Schedule[i];

				DateTime preScheduleEnd = prevTime; // This assignment is to prevent "access to modified closure" warning.

				foreach (OneDayLoanStatus cls in days.Where(cls => preScheduleEnd < cls.Date && cls.Date <= sp.Date)) {
					cls.DailyInterestRate = Calculator.GetDailyInterestRate(
						cls.Date,
						sp.InterestRate,
						true, // considerBadPeriods
						true, // considerFreezeInterestPeriod
						preScheduleEnd,
						sp.Date
					);
				} // for each

				prevTime = sp.Date;
			} // for each scheduled payment

			// Step 4. Find maximum of requested date, last payment date, and last fee date.

			DateTime maxDate = now;

			if (WorkingModel.Repayments.Count > 0) {
				DateTime lastRepaymentDate = WorkingModel.Repayments.Last().Date;

				if (lastRepaymentDate > maxDate)
					maxDate = lastRepaymentDate;
			} // if

			if (WorkingModel.Fees.Count > 0) {
				DateTime lastFeeDate = WorkingModel.Fees.Last().AssignDate;

				if (lastFeeDate > maxDate)
					maxDate = lastFeeDate;
			} // if

			// Step 5. If found maximum date is outside scheduled period append missing days in the actual date list.
			// Open principal for missing days is full loan amount, daily interest is the last scheduled day interest.

			if (maxDate > days.LastKnownDate) {
				OneDayLoanStatus lastKnownDayData = days[days.LastKnownDate];

				if (lastKnownDayData.OpenPrincipal > 0) {
					for (DateTime dt = lastKnownDayData.Date.AddDays(1); dt.Date <= maxDate; dt = dt.AddDays(1)) {
						days.Add(new OneDayLoanStatus(dt, WorkingModel.LoanAmount, days.LastDailyLoanStatus) {
							DailyInterestRate = lastKnownDayData.DailyInterestRate,
						});
					} // for
				} // if
			} // if

			// Step 6. Add fees to the actual date list.

			foreach (Fee fee in WorkingModel.Fees)
				days[fee.AssignDate].AssignedFees += fee.Amount;

			// Step 7. Take into account repayments: reduce repaid principal and store repayment data on repayment date.

			for (var i = 0; i < WorkingModel.Repayments.Count; i++) {
				Repayment rp = WorkingModel.Repayments[i];

				foreach (OneDayLoanStatus odls in days.Where(dd => dd.Date > rp.Date))
					odls.OpenPrincipal -= rp.Principal;

				if (days.Contains(rp.Date))
					days[rp.Date].AddRepayment(rp);
				else {
					Library.Instance.Log.Alert(
						"days list ain't no contains repayment date {0} for model {1}",
						rp.Date,
						WorkingModel
					);
				} // if
			} // for

			return days;
		} // CreateActualDailyLoanStatus
	} // class AWithActualDailyLoanStatusMethod
} // namespace
