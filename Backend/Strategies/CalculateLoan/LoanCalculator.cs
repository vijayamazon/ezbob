namespace Ezbob.Backend.Strategies.CalculateLoan {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;

	public class LoanCalculator {
		public LoanCalculator(LoanCalculatorModel model) {
			if (model == null)
				throw new ArgumentNullException("model", "No data for loan calculation.");

			WorkingModel = model;
		} // constructor

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

				sp.Date = (
					WorkingModel.IsMonthly
						? WorkingModel.LoanIssueTime.AddMonths(i)
						: WorkingModel.LoanIssueTime.AddDays(i * (int)WorkingModel.RepaymentIntervalType)
				).Date;

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
		/// Calculates loan plan (the one that is written in loan agreement).
		/// Repayments, fees, bad periods, and interest freeze periods are ignored.
		/// </summary>
		/// <returns>Loan plan.</returns>
		[SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
		public virtual List<Repayment> CalculatePlan() {
			if (WorkingModel.Schedule.Count < 1)
				throw new Exception("No loan schedule found.");

			for (int i = 0; i < WorkingModel.Schedule.Count; i++)
				if (WorkingModel.Schedule[i].Date == null)
					throw new Exception("No date specified for scheduled payment #" + (i + 1));

			DateTime firstInterestDay = WorkingModel.LoanIssueTime.Date.AddDays(1);

			DateTime lastInterestDay = WorkingModel.Schedule[WorkingModel.Schedule.Count - 1].Date.Value.Date;

			var days = new List<CurrentLoanStatus>();

			for (DateTime d = firstInterestDay; d <= lastInterestDay; d = d.AddDays(1))
				days.Add(new CurrentLoanStatus(d, WorkingModel.LoanAmount));

			DateTime prevTime = WorkingModel.LoanIssueTime;

			for (int i = 0; i < WorkingModel.Schedule.Count; i++) {
				ScheduledPayment sp = WorkingModel.Schedule[i];

				foreach (var cls in days.Where(cls => cls.Date > sp.Date.Value))
					cls.OpenPrincipal -= sp.Principal;

				DateTime preScheduleEnd = prevTime; // This assignment is to prevent "access to modified closure" warning.

				foreach (var cls in days.Where(cls => preScheduleEnd < cls.Date && cls.Date <= sp.Date.Value))
					cls.DailyInterestRate = GetDailyInterestRate(sp.InterestRate, preScheduleEnd, sp.Date.Value);

				prevTime = sp.Date.Value;
			} // for each scheduled payment

			var result = new List<Repayment>(
				WorkingModel.Schedule.Select(sp => new Repayment(sp.Date.Value, sp.Principal, 0, 0))
			);

			prevTime = WorkingModel.LoanIssueTime;

			for (int i = 0; i < result.Count; i++) {
				Repayment r = result[i];

				r.Interest = days.Where(cls => prevTime < cls.Date && cls.Date <= r.Time).Sum(cls => cls.DailyInterest);

				prevTime = r.Time;
			} // for

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

			return result;
		} // CalculatePlan

		/// <summary>
		/// Calculates current loan balance.
		/// </summary>
		/// <returns>Current loan balance.</returns>
		public virtual decimal CalculateBalance() {
			return 0;
		} // CalculateBalance

		/// <summary>
		/// Calculates current loan earned interest.
		/// </summary>
		/// <returns>Current loan earned interest.</returns>
		public virtual decimal CalculateEarnedInterest() {
			return 0;
		} // CalculateEarnedInterest

		public virtual LoanCalculatorModel WorkingModel { get; private set; }

		/// <summary>
		/// Calculates interest rate for one day based on monthly interest rate.
		/// If either of period start date or end date is null both dates are considered to be null.
		/// In current implementation period dates are completely ignored: we just multiply monthly rate
		/// by number of months in year and divide by number of days in year.
		/// </summary>
		/// <param name="monthlyInterestRate">Monthly interest rate.</param>
		/// <param name="periodStartDate">Period start date (the first day of the period).</param>
		/// <param name="periodEndDate">Period end date (the last day of the period).</param>
		/// <returns>Daily interest rate.</returns>
		protected virtual decimal GetDailyInterestRate(
			decimal monthlyInterestRate,
			DateTime? periodStartDate = null,
			DateTime? periodEndDate = null
		) {
			return monthlyInterestRate * 12.0m / 365.0m;
		} // GetDailyInterestRate
	} // class LoanCalculator
} // namespace
