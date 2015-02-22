namespace Ezbob.Backend.Strategies.CalculateLoan {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using Ezbob.Backend.Strategies.CalculateLoan.DailyInterestRate;
	using Ezbob.Backend.Strategies.CalculateLoan.Helpers;

	public abstract class ALoanCalculator {
		/// <summary>
		/// Creates loan schedule by loan issue time, repayment count, repayment interval type and discount plan.
		/// </summary>
		public virtual void CreateSchedule() {
			if (WorkingModel.InterestOnlyMonths >= WorkingModel.RepaymentCount) {
				throw new ArgumentOutOfRangeException(
					"Interest only months count is not less than repayment count.",
					(Exception)null
				);
			} // if

			int principalRepaymentCount = WorkingModel.RepaymentCount - WorkingModel.InterestOnlyMonths;

			decimal otherPayments = Math.Floor(WorkingModel.LoanAmount / principalRepaymentCount);

			decimal firstPayment = WorkingModel.LoanAmount - otherPayments * (principalRepaymentCount - 1);

			WorkingModel.Schedule.Clear();

			for (int i = 1; i <= WorkingModel.RepaymentCount; i++) {
				var sp = new ScheduledPayment();

				sp.Date = AddPeriods(i).Date;

				if (i <= WorkingModel.InterestOnlyMonths)
					sp.Principal = 0;
				else if (i == WorkingModel.InterestOnlyMonths + 1)
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
		/// <returns>Loan plan.</returns>
		[SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
		public virtual List<Repayment> CalculatePlan(bool writeToLog = true) {
			DailyLoanStatus days = CreateDailyLoanStatus(true, false);

			var result = new List<Repayment>(
				WorkingModel.Schedule.Select(sp => new Repayment(sp.Date.Value, sp.Principal, 0, 0))
			);

			DateTime prevTime = WorkingModel.LoanIssueTime;

			for (int i = 0; i < result.Count; i++) {
				Repayment r = result[i];

				DateTime dt = prevTime; // This assignment is to prevent "access to modified closure" warning.

				r.Interest = days.Where(cls => dt < cls.Date && cls.Date <= r.Time).Sum(cls => cls.DailyInterest);

				prevTime = r.Time;
			} // for

			if (writeToLog) {
				Library.Instance.Log.Debug(
					"\n\nLoanCalculator.CreatePlan - begin:" +
					"\n\nLoan calculator model:\n{0}" +
					"\n\nLoan plan:\n\t\t{1}" +
					"\n\nDaily data:\n\t\t{2}" +
					"\n\nLoanCalculator.CreatePlan - end." +
					"\n\n",
					WorkingModel,
					string.Join("\n\t\t", result),
					string.Join("\n\t\t", days)
				);
			} // if

			return result;
		} // CalculatePlan

		/// <summary>
		/// Calculates current loan balance.
		/// </summary>
		/// <param name="requestedDate">Date to calculate balance to (current date if null).</param>
		/// <param name="writeToLog">Write result to log or not.</param>
		/// <returns>Current loan balance.</returns>
		public virtual decimal CalculateBalance(DateTime? requestedDate = null, bool writeToLog = true) {
			DateTime now = (requestedDate ?? DateTime.UtcNow).Date;

			if (now <= WorkingModel.LoanIssueTime.Date)
				return 0;

			DailyLoanStatus days = CreateDailyLoanStatus(true, false);

			if (days.IsEmpty)
				return 0;

			DateTime maxDate = now;

			if (WorkingModel.Repayments.Count > 0) {
				DateTime lastRepaymentDate = WorkingModel.Repayments.Last().Time.Date;

				if (lastRepaymentDate > maxDate)
					maxDate = lastRepaymentDate;
			} // if

			if (maxDate > days.LastKnownDate) {
				OneDayLoanStatus lastKnownDayData = days[days.LastKnownDate];

				if (lastKnownDayData.OpenPrincipal > 0) {
					for (DateTime dt = lastKnownDayData.Date.AddDays(1); dt.Date <= maxDate; dt = dt.AddDays(1)) {
						days.Add(new OneDayLoanStatus(dt, lastKnownDayData.OpenPrincipal) {
							DailyInterestRate = lastKnownDayData.DailyInterestRate,
						});
					} // for
				} // if
			} // if

			for (int i = 0; i < WorkingModel.Repayments.Count; i++) {
				Repayment rp = WorkingModel.Repayments[i];
				days[rp.Time.Date].AddRepayment(rp);
			} // for

			decimal balance = WorkingModel.LoanAmount + days.Days.Where(odls => odls.Date <= now).Sum(odls =>
				odls.DailyInterest
				+ odls.AssignedFees
				- odls.RepaidPrincipal
				- odls.RepaidInterest
				- odls.RepaidFees
			);

			if (writeToLog) {
				Library.Instance.Log.Debug(
					"\n\nLoanCalculator.CalculateBalance - begin:" +
					"\n\nLoan calculator model:\n{0}" +
					"\n\nBalance on {3}:\n\t\t{1}" +
					"\n\nDaily data:\n\t\t{2}" +
					"\n\nLoanCalculator.CalculateBalance - end." +
					"\n\n",
					WorkingModel,
					string.Join("\n\t\t", balance.ToString("C2", Library.Instance.Culture)),
					string.Join("\n\t\t", days),
					now.DateStr()
				);
			} // if

			return balance;
		} // CalculateBalance

		/// <summary>
		/// Calculates current loan earned interest.
		/// </summary>
		/// <returns>Current loan earned interest.</returns>
		public virtual decimal CalculateEarnedInterest() {
			return 0;
		} // CalculateEarnedInterest

		public virtual LoanCalculatorModel WorkingModel { get; private set; }

		protected ALoanCalculator(LoanCalculatorModel model, ADailyInterestRate dailyInterestRateCalculator) {
			if (model == null)
				throw new ArgumentNullException("model", "No data for loan calculation.");

			if (dailyInterestRateCalculator == null)
				throw new ArgumentNullException("dailyInterestRateCalculator", "No daily interest calculator specified.");

			WorkingModel = model;
			DailyInterestCalculator = dailyInterestRateCalculator;
		} // constructor

		protected virtual ADailyInterestRate DailyInterestCalculator { get; private set; } // DailyInterestCalculator

		/// <summary>
		/// Calculates date after requested number of periods have passed since loan issue date.
		/// Periods length is determined from WorkingModel.RepaymentIntervalType.
		/// </summary>
		/// <param name="periodCount">A number of periods to add.</param>
		/// <returns>Date after requested number of periods have been added to loan issue date.</returns>
		protected abstract DateTime AddPeriods(int periodCount);

		/// <summary>
		/// Calculates interest rate for one day based on monthly interest rate.
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
				return DailyInterestCalculator.GetRate(
					currentDate,
					monthlyInterestRate,
					periodStartDate,
					periodEndDate
				);
			} // if

			if (WorkingModel.BadPeriods.Contains(currentDate))
				return 0;

			return WorkingModel.FreezePeriods.GetInterest(currentDate) ?? DailyInterestCalculator.GetRate(
				currentDate,
				monthlyInterestRate,
				periodStartDate,
				periodEndDate
			);
		} // GetDailyInterestRate

		/// <summary>
		/// Creates daily loan status from loan issue date till the last payment.
		/// </summary>
		/// <param name="usePeriods">True, to take in account bad and interest freeze periods,
		/// or false, to ignore them.</param>
		/// <param name="reduceRepaidPrincipal">True, to reduce repaid principal from open principal
		/// of every affected day.</param>
		/// <returns></returns>
		[SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
		private DailyLoanStatus CreateDailyLoanStatus(bool usePeriods, bool reduceRepaidPrincipal) {
			if (WorkingModel.Schedule.Count < 1)
				throw new Exception("No loan schedule found.");

			for (int i = 0; i < WorkingModel.Schedule.Count; i++)
				if (WorkingModel.Schedule[i].Date == null)
					throw new Exception("No date specified for scheduled payment #" + (i + 1));

			DateTime firstInterestDay = WorkingModel.LoanIssueTime.Date.AddDays(1);

			DateTime lastInterestDay = WorkingModel.Schedule[WorkingModel.Schedule.Count - 1].Date.Value.Date;

			var days = new DailyLoanStatus();

			for (DateTime d = firstInterestDay; d <= lastInterestDay; d = d.AddDays(1))
				days.Add(new OneDayLoanStatus(d, WorkingModel.LoanAmount));

			DateTime prevTime = WorkingModel.LoanIssueTime;

			for (int i = 0; i < WorkingModel.Schedule.Count; i++) {
				ScheduledPayment sp = WorkingModel.Schedule[i];

				foreach (var cls in days.Where(cls => cls.Date > sp.Date.Value))
					cls.OpenPrincipal -= sp.Principal;

				DateTime preScheduleEnd = prevTime; // This assignment is to prevent "access to modified closure" warning.

				foreach (var cls in days.Where(cls => preScheduleEnd < cls.Date && cls.Date <= sp.Date.Value)) {
					cls.DailyInterestRate = GetDailyInterestRate(
						cls.Date,
						sp.InterestRate,
						usePeriods,
						preScheduleEnd,
						sp.Date.Value
					);
				} // for each

				prevTime = sp.Date.Value;
			} // for each scheduled payment

			if (reduceRepaidPrincipal) {
				for (var i = 0; i < WorkingModel.Repayments.Count; i++) {
					var rp = WorkingModel.Repayments[i];

					foreach (OneDayLoanStatus cls in days.Where(cls => cls.Date >= rp.Time.Date))
						cls.OpenPrincipal -= rp.Principal;
				} // for
			} // if

			return days;
		} // CreateDailyLoanStatus
	} // class LoanCalculator
} // namespace
