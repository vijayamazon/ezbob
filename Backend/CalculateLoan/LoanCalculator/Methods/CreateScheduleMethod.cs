namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Methods {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DbConstants;
	using Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions;
	using Ezbob.Backend.CalculateLoan.Models.Exceptions;
	using Ezbob.Backend.CalculateLoan.Models.Helpers;

	internal class CreateScheduleMethod : AMethod {
		public CreateScheduleMethod(ALoanCalculator calculator) : base(calculator, false) {
		} // constructor

		public virtual List<ScheduledItem> Execute() {
			return new List<ScheduledItem>();

			// TODO: revive

			/*

			if (WorkingModel.InterestOnlyRepayments >= WorkingModel.RepaymentCount)
				throw new InterestOnlyMonthsCountException(WorkingModel.InterestOnlyRepayments, WorkingModel.RepaymentCount);

			int principalRepaymentCount = WorkingModel.RepaymentCount - WorkingModel.InterestOnlyRepayments;

			decimal otherPayments = Math.Floor(WorkingModel.LoanAmount / principalRepaymentCount);

			decimal firstPayment = WorkingModel.LoanAmount - otherPayments * (principalRepaymentCount - 1);

			WorkingModel.Schedule.Clear();

			for (int i = 1; i <= WorkingModel.RepaymentCount; i++) {
				var sp = new ScheduledItem(Calculator.AddRepaymentIntervals(i).Date);

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

			return WorkingModel.Schedule;

			*/
		} // Execute

		/// <summary>
		/// Calculates date after requested number of periods have passed since loan issue date.
		/// Periods length is determined from WorkingModel.RepaymentIntervalType.
		/// </summary>
		/// <returns>Date after requested number of periods have been added to loan issue date.</returns>
		/// <param name="periodCount">A number of periods to add.</param>
		/// <returns>Date after requested number of periods have been added to loan issue date.</returns>
		private DateTime AddRepaymentIntervals(int periodCount) {
			var lh = WorkingModel.LoanHistory.Last();

			return lh.RepaymentIntervalType == RepaymentIntervalTypes.Month
				? lh.Time.AddMonths(periodCount)
				: lh.Time.AddDays(periodCount * (int)lh.RepaymentIntervalType);
		} // AddRepaymentIntervals
	} // class CreateScheduleMethod
} // namespace
