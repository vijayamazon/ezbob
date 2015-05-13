﻿namespace Ezbob.Backend.CalculateLoan.LoanCalculator {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using Ezbob.Backend.CalculateLoan.Models;
	using Ezbob.Backend.CalculateLoan.Models.Extensions;
	using Ezbob.Backend.CalculateLoan.Models.Helpers;
	using Ezbob.Backend.Extensions;

	// TODO: missing functionality: calculate next scheduled payment.
	// TODO: missing functionality: calculate next scheduled payment state on specific date (e.g. in three days).
	// TODO: missing functionality: replace art of existing schedule with new schedule.

	public abstract class ALoanCalculator {
		public abstract string Name { get; }

		/// <summary>
		/// Creates loan schedule by loan issue time, repayment count, repayment interval type and discount plan.
		/// Schedule is stored in WorkingModel.Schedule.
		/// </summary>
		public virtual void CreateSchedule() {
			if (WorkingModel.InterestOnlyRepayments >= WorkingModel.RepaymentCount) {
				throw new ArgumentOutOfRangeException(
					"Interest only months count is not less than repayment count.",
					(Exception)null
				);
			} // if

			int principalRepaymentCount = WorkingModel.RepaymentCount - WorkingModel.InterestOnlyRepayments;

			decimal otherPayments = Math.Floor(WorkingModel.LoanAmount / principalRepaymentCount);

			decimal firstPayment = WorkingModel.LoanAmount - otherPayments * (principalRepaymentCount - 1);

			WorkingModel.Schedule.Clear();

			for (int i = 1; i <= WorkingModel.RepaymentCount; i++) {
				var sp = new Scheduled(AddPeriods(i).Date);

				if (i <= WorkingModel.InterestOnlyRepayments)
					sp.Principal = 0;
				else if (i == WorkingModel.InterestOnlyRepayments + 1)
					sp.Principal = firstPayment;
				else
					sp.Principal = otherPayments;

				sp.InterestRate = WorkingModel.MonthlyInterestRate;

				if (i <= WorkingModel.DiscountPlan.Count)
					sp.InterestRate *= 1 + WorkingModel.DiscountPlan[i - 1];

				WorkingModel.Schedule.Add(sp);
			} // for
		} // CreateSchedule

		/// <summary>
		/// Calculates loan plan.
		/// </summary>
		/// <param name="writeToLog">Write result to log or not.</param>
		/// <returns>Loan plan (list of repayments).</returns>
		[SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
		public virtual List<Repayment> CalculatePlan(bool writeToLog = true) {
			WorkingModel.ValidateSchedule();

			DateTime firstInterestDay = WorkingModel.LoanIssueTime.Date.AddDays(1);

			DateTime lastInterestDay = WorkingModel.Schedule[WorkingModel.Schedule.Count - 1].Date;

			var days = new DailyLoanStatus();

			for (DateTime d = firstInterestDay; d <= lastInterestDay; d = d.AddDays(1))
				days.Add(new OneDayLoanStatus(d, WorkingModel.LoanAmount, days.LastDailyLoanStatus));

			DateTime prevTime = WorkingModel.LoanIssueTime;

			for (int i = 0; i < WorkingModel.Schedule.Count; i++) {
				Scheduled sp = WorkingModel.Schedule[i];

				foreach (OneDayLoanStatus cls in days.Where(dd => dd.Date > sp.Date))
					cls.OpenPrincipal -= sp.Principal;

				DateTime preScheduleEnd = prevTime; // This assignment is to prevent "access to modified closure" warning.

				foreach (OneDayLoanStatus cls in days.Where(cls => preScheduleEnd < cls.Date && cls.Date <= sp.Date)) {
					cls.DailyInterestRate = GetDailyInterestRate(
						cls.Date,
						sp.InterestRate,
						false,
						preScheduleEnd,
						sp.Date
					);
				} // for each

				prevTime = sp.Date;
			} // for each scheduled payment

			var result = new List<Repayment>(
				WorkingModel.Schedule.Select(sp => new Repayment(sp.Date, sp.Principal, 0, 0))
			);

			prevTime = WorkingModel.LoanIssueTime;

			for (int i = 0; i < result.Count; i++) {
				Repayment r = result[i];

				DateTime dt = prevTime; // This assignment is to prevent "access to modified closure" warning.

				r.Interest = days.Where(cls => dt < cls.Date && cls.Date <= r.Time).Sum(cls => cls.DailyInterest);

				prevTime = r.Time;
			} // for

			if (writeToLog) {
				AddScheduleNotes(days);

				Library.Instance.Log.Debug(
					"\n\n{3}.CreatePlan - begin:" +
					"\n\nLoan calculator model:\n{0}" +
					"\n\nLoan plan:\n\t\t{1}" +
					"\n\nDaily data:\n{2}" +
					"\n\nLoanCalculator.CreatePlan - end." +
					"\n\n",
					WorkingModel,
					string.Join("\n\t\t", result),
					days.ToFormattedString("\t\t"),
					Name
				);
			} // if

			return result;
		} // CalculatePlan

		/// <summary>
		/// Calculates current loan balance.
		/// </summary>
		/// <param name="requestedDate">Date to calculate balance on (current date if null).</param>
		/// <param name="writeToLog">Write result to log or not.</param>
		/// <returns>Loan balance on specific date.</returns>
		public virtual decimal CalculateBalance(DateTime? requestedDate = null, bool writeToLog = true) {
			DateTime now = (requestedDate ?? DateTime.UtcNow).Date;

			if (now <= WorkingModel.LoanIssueTime.Date)
				return 0;

			DailyLoanStatus days = CreateActualDailyLoanStatus(now);

			decimal balance = days[now].CurrentBalance;

			if (writeToLog) {
				AddScheduleNotes(days);
				AddFeeNotes(days);
				AddPaymentNotes(days);
				days.AddNote(now, "Requested balance date.");

				Library.Instance.Log.Debug(
					"\n\n{4}.CalculateBalance - begin:" +
					"\n\nLoan calculator model:\n{0}" +
					"\n\nBalance on {3}:\n\t\t{1}" +
					"\n\nDaily data:\n{2}" +
					"\n\nLoanCalculator.CalculateBalance - end." +
					"\n\n",
					WorkingModel,
					string.Join("\n\t\t", balance.ToString("C2", Library.Instance.Culture)),
					days.ToFormattedString("\t\t"),
					now.DateStr(),
					Name
				);
			} // if

			return balance;
		} // CalculateBalance

		/// <summary>
		/// Calculates current (for requested date) payment.
		/// Method logic: https://drive.google.com/open?id=0B1Io_qu9i44SaWlHX0FKQy0tcWM&authuser=0
		/// </summary>
		/// <param name="requestedDate">Date to calculate payment on (current date if null).</param>
		/// <param name="setClosedDateFromPayments">Update scheduled payment closed date from actual payments
		/// or leave it as is.</param>
		/// <param name="writeToLog">Write result to log or not.</param>
		/// <returns>Loan balance on specific date.</returns>
		public virtual CurrentPaymentModel AmountToCharge(
			DateTime? requestedDate = null,
			bool setClosedDateFromPayments = false,
			bool writeToLog = true
		) {
			DateTime today = (requestedDate ?? DateTime.UtcNow).Date;

			var cpm = new CurrentPaymentModel(0);

			if (today <= WorkingModel.LoanIssueTime.Date) {
				cpm.IsError = true;
				return cpm;
			} // if

			DailyLoanStatus days = CreateActualDailyLoanStatus(today);

			if (days.IsEmpty) {
				cpm.IsError = true;
				return cpm;
			} // if

			if (setClosedDateFromPayments)
				WorkingModel.SetScheduleCloseDatesFromPayments();

			bool allPreviousPaymentsAreClosed = WorkingModel.Schedule
				.Where(s => s.Date < today)
				.All(s => s.IsClosedOn(today));

			Scheduled currentPayment = WorkingModel.Schedule.FindByDate(today);

			if (currentPayment == null) { // today is not a payment date.
				if (allPreviousPaymentsAreClosed) { // Delta scenario.
					Scheduled firstOpen = WorkingModel.Schedule
						.FirstOrDefault(s => (s.Date > today) && !s.ClosedDate.HasValue);

					// ReSharper disable once PossibleInvalidOperationException
					// If firstOpen is not null then firstOpen.Date is not null:
					// this is checked during CreateActualDailyLoanStatus().
					OneDayLoanStatus thatDay = firstOpen == null ? null : days[firstOpen.Date];
					OneDayLoanStatus thisDay = days[today];

					cpm.LoanIsClosed = firstOpen == null;

					if (firstOpen == null)
						cpm.Amount = 0;
					else {
						// If firstOpen is not null then both thisDay is not null
						// because of how CreateActualDailyLoanStatus works: thisDay corresponds to
						// today which is inserted as it is a requested date.
						cpm.Amount = firstOpen.Principal + thisDay.TotalExpectedNonprincipalPayment;
					} // if

					cpm.IsLate = false;

					cpm.SavedAmount = thatDay == null
						? 0
						: thatDay.TotalExpectedNonprincipalPayment - thisDay.TotalExpectedNonprincipalPayment;
				} else { // Echo scenario.
					cpm.LoanIsClosed = false;

					OneDayLoanStatus thisDay = days[today];

					cpm.Amount =
						WorkingModel.Schedule.Where(s => s.Date < today).Sum(s => s.Principal) -
						thisDay.TotalRepaidPrincipal +
						thisDay.TotalExpectedNonprincipalPayment;

					cpm.IsLate = true;

					cpm.SavedAmount = 0;
				} // if
			} else { // today is a payment date.
				if (allPreviousPaymentsAreClosed) {
					if (currentPayment.IsClosedOn(today)) { // Alpha scenario.
					} else { // Bravo scenario.
					} // if
				} else { // Charlie scenario.
					cpm.IsLate = true;
				} // if
			} // if

			return cpm;
		} // AmountToCharge

		/// <summary>
		/// Calculates loan earned interest between two dates including both dates.
		/// </summary>
		/// <param name="startDate">First day of the calculation period; loan issue date is used if omitted.</param>
		/// <param name="endDate">Last day of the calculation period; last scheduled payment date is used is omitted.</param>
		/// <param name="writeToLog">Write result to log or not.</param>
		/// <returns>Loan earned interest during specific date range.</returns>
		public virtual decimal CalculateEarnedInterest(DateTime? startDate, DateTime? endDate, bool writeToLog = true) {
			WorkingModel.ValidateSchedule();

			DateTime firstDay = (startDate ?? WorkingModel.LoanIssueTime).Date;

			DateTime lastDay = (endDate ?? WorkingModel.LastScheduledDate).Date;

			DailyLoanStatus days = CreateActualDailyLoanStatus(lastDay);

			decimal earnedInterest = days.Days
				.Where(odls => firstDay <= odls.Date && odls.Date <= lastDay)
				.Sum(odls => odls.DailyInterest);

			if (writeToLog) {
				AddScheduleNotes(days);
				AddPaymentNotes(days);
				days.AddNote(firstDay, "First earned interest period date.");
				days.AddNote(lastDay, "Last earned interest period date.");

				Library.Instance.Log.Debug(
					"\n\n{5}.CalculateEarnedInterest - begin:" +
					"\n\nLoan calculator model:\n{0}" +
					"\n\nEarned interest between {3} and {4}:\n\t\t{1}" +
					"\n\nDaily data:\n{2}" +
					"\n\nLoanCalculator.CalculateEarnedInterest - end." +
					"\n\n",
					WorkingModel,
					string.Join("\n\t\t", earnedInterest.ToString("C2", Library.Instance.Culture)),
					days.ToFormattedString("\t\t"),
					firstDay.DateStr(),
					lastDay.DateStr(),
					Name
				);
			} // if

			return earnedInterest;
		} // CalculateEarnedInterest

		public virtual LoanCalculatorModel WorkingModel { get; private set; }

		protected ALoanCalculator(LoanCalculatorModel model) {
			if (model == null)
				throw new ArgumentNullException("model", "No data for loan calculation.");

			WorkingModel = model;
		} // constructor

		/// <summary>
		/// Calculates date after requested number of periods have passed since loan issue date.
		/// Periods length is determined from WorkingModel.RepaymentIntervalType.
		/// </summary>
		/// <param name="periodCount">A number of periods to add.</param>
		/// <returns>Date after requested number of periods have been added to loan issue date.</returns>
		protected abstract DateTime AddPeriods(int periodCount);

		/// <summary>
		/// Calculates interest rate for one day based on monthly interest rate.
		/// Bad periods and interest freeze periods are ignored.
		/// </summary>
		/// <param name="currentDate">Current date.</param>
		/// <param name="monthlyInterestRate">Monthly interest rate.</param>
		/// <param name="periodStartDate">Period start date (the first day of the period).</param>
		/// <param name="periodEndDate">Period end date (the last day of the period).</param>
		/// <returns>Daily interest rate.</returns>
		protected abstract decimal CalculateDailyInterestRate(
			DateTime currentDate,
			decimal monthlyInterestRate,
			DateTime? periodStartDate = null,
			DateTime? periodEndDate = null
		);

		/// <summary>
		/// Calculates interest rate for one day based on monthly interest rate.
		/// Bad periods and interest freeze periods can be ignored (<paramref name="usePeriods"/>).
		/// </summary>
		/// <param name="currentDate">Current date.</param>
		/// <param name="monthlyInterestRate">Monthly interest rate.</param>
		/// <param name="usePeriods">True, to take in account bad and interest freeze periods,
		/// or false, to ignore them.</param>
		/// <param name="periodStartDate">Period start date (the first day of the period).</param>
		/// <param name="periodEndDate">Period end date (the last day of the period).</param>
		/// <returns>Daily interest rate.</returns>
		private decimal GetDailyInterestRate(
			DateTime currentDate,
			decimal monthlyInterestRate,
			bool usePeriods,
			DateTime? periodStartDate = null,
			DateTime? periodEndDate = null
		) {
			if (!usePeriods) {
				return CalculateDailyInterestRate(
					currentDate,
					monthlyInterestRate,
					periodStartDate,
					periodEndDate
				);
			} // if

			if (WorkingModel.BadPeriods.Contains(currentDate))
				return 0;

			return CalculateDailyInterestRate(
				currentDate,
				monthlyInterestRate,
				periodStartDate,
				periodEndDate
			);
		} // GetDailyInterestRate

		[SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
		private DailyLoanStatus CreateActualDailyLoanStatus(DateTime now) {
			WorkingModel.ValidateSchedule();

			DateTime firstInterestDay = WorkingModel.LoanIssueTime.Date.AddDays(1);

			DateTime lastInterestDay = WorkingModel.Schedule[WorkingModel.Schedule.Count - 1].Date;

			var days = new DailyLoanStatus();

			// Step 1. Create a list of days within scheduled period.
			// Open principal is equal to full loan amount.
			// Daily interest is 0 at this point.

			days.Add(new OneDayLoanStatus(WorkingModel.LoanIssueTime, 0, null));

			for (DateTime d = firstInterestDay; d <= lastInterestDay; d = d.AddDays(1))
				days.Add(new OneDayLoanStatus(d, WorkingModel.LoanAmount, days.LastDailyLoanStatus));

			DateTime prevTime = WorkingModel.LoanIssueTime;

			// Step 2. Fill daily interest for each day within scheduled period.
			// Open principal is not changed.

			for (int i = 0; i < WorkingModel.Schedule.Count; i++) {
				Scheduled sp = WorkingModel.Schedule[i];

				DateTime preScheduleEnd = prevTime; // This assignment is to prevent "access to modified closure" warning.

				foreach (var cls in days.Where(cls => preScheduleEnd < cls.Date && cls.Date <= sp.Date)) {
					cls.DailyInterestRate = GetDailyInterestRate(
						cls.Date,
						sp.InterestRate,
						true,
						preScheduleEnd,
						sp.Date
					);
				} // for each

				prevTime = sp.Date;
			} // for each scheduled payment

			// Step 3. Find maximum of requested date, last payment date, and last fee date.

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

			// Step 4. If found maximum date is outside scheduled period append missing days in the actual date list.
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

			// Step 5. Add fees to the actual date list.

			foreach (Fee fee in WorkingModel.Fees)
				days[fee.AssignDate].AssignedFees += fee.Amount;

			// Step 6. Take into account repayments: reduce repaid principal and store repayment data on repayment date.

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

		private void AddScheduleNotes(DailyLoanStatus days) {
			for(int i = 0; i < WorkingModel.Schedule.Count; i++) {
				var sp = WorkingModel.Schedule[i];

				days.AddNote(
					sp.Date,
					i == WorkingModel.Schedule.Count - 1 ? "Last scheduled payment." : "Scheduled payment."
				);
			} // for each
		} // AddScheduleNotes

		private void AddPaymentNotes(DailyLoanStatus days) {
			for(int i = 0; i < WorkingModel.Repayments.Count; i++) {
				var rp = WorkingModel.Repayments[i];

				days.AddNote(
					rp.Date,
					"Repaid: " + rp.Amount.ToString("C2", Library.Instance.Culture)
				);
			} // for each
		} // AddPaymentNotes

		private void AddFeeNotes(DailyLoanStatus days) {
			for(int i = 0; i < WorkingModel.Fees.Count; i++) {
				var fee = WorkingModel.Fees[i];

				days.AddNote(
					fee.AssignDate,
					"Fee assigned: " + fee.Amount.ToString("C2", Library.Instance.Culture)
				);
			} // for each
		} // AddFeeNotes
	} // class LoanCalculator
} // namespace
