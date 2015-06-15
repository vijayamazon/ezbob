namespace Ezbob.Backend.CalculateLoan.LoanCalculator {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions;
	using Ezbob.Backend.CalculateLoan.LoanCalculator.Methods;
	using Ezbob.Backend.CalculateLoan.Models;
	using Ezbob.Backend.CalculateLoan.Models.Exceptions;
	using Ezbob.Backend.CalculateLoan.Models.Helpers;

	/// <summary>
	/// This class is implemented as Facade pattern.
	/// https://en.wikipedia.org/wiki/Facade_pattern
	/// </summary>
	public abstract partial class ALoanCalculator {
		public abstract string Name { get; }

		/// <summary>
		/// Creates loan schedule by loan issue time, repayment count, repayment interval type and discount plan.
		/// Schedule is stored in WorkingModel.Schedule.
		/// Each element in Schedule is ScheduledItem
		/// </summary>
		/// <exception cref="InterestOnlyMonthsCountException">Condition. </exception>
		/// <exception cref="NegativeMonthlyInterestRateException">Condition. </exception>
		/// <exception cref="NegativeLoanAmountException">Condition. </exception>
		/// <exception cref="NegativeRepaymentCountException">Condition. </exception>
		/// <exception cref="NegativeInterestOnlyRepaymentCountException">Condition. </exception>
		public virtual List<ScheduledItem> CreateSchedule() {
			return new CreateScheduleMethod(this).Execute();
		} // CreateSchedule

		/// <summary>
		/// Calculates loan plan.
		/// </summary>
		/// <param name="writeToLog">Write result to log or not.</param>
		/// <returns>Loan plan (list of repayments).</returns>
		public virtual List<Repayment> CalculatePlan(bool writeToLog = true) {
			return new CalculatePlanMethod(this, writeToLog).Execute();
		} // CalculatePlan

		/// <summary>
		/// Creates loan schedule by loan issue time, repayment count, repayment interval type and discount plan.
		/// Also calculates loan plan.
		/// Schedule is stored in WorkingModel.Schedule.
		/// </summary>
		public virtual List<ScheduledItemWithAmountDue> CreateScheduleAndPlan(bool writeToLog = true) {
			return new CreateScheduleAndPlanMethod(this, writeToLog).Execute();
		} // CreateScheduleAndPlan

		/// <summary>
		/// Calculates current loan balance.
		/// </summary>
		/// <param name="today">Date to calculate balance on.</param>
		/// <param name="writeToLog">Write result to log or not.</param>
		/// <returns>Loan balance on specific date.</returns>
		public virtual decimal CalculateBalance(DateTime today, bool writeToLog = true) {
			return new CalculateBalanceMethod(this, today, writeToLog).Execute();
		} // CalculateBalance

		/// <summary>
		/// Calculates payment options (late/current/next installment/full balance) for requested date.
		/// Method logic: https://drive.google.com/open?id=0B1Io_qu9i44SaWlHX0FKQy0tcWM&amp;authuser=0
		/// This method is used to calculate charge options that should be displayed to customer in the dashboard.
		/// </summary>
		/// <param name="today">Date to calculate payment on.</param>
		/// <param name="setClosedDateFromPayments">Update scheduled payment closed date from actual payments
		/// or leave it as is.</param>
		/// <param name="writeToLog">Write result to log or not.</param>
		/// <returns>Loan balance on specific date.</returns>
		public virtual CurrentPaymentModel GetAmountToChargeForDashboard(
			DateTime today,
			bool setClosedDateFromPayments = false,
			bool writeToLog = true
		) {
			return new GetAmountToChargeForDashboardMethod(this, today, setClosedDateFromPayments, writeToLog).Execute();
		} // GetAmountToChargeForDashboard

		/// <summary>
		/// Calculates payment options (late/current installment) for requested date.
		/// Method logic: https://drive.google.com/open?id=0B1Io_qu9i44SaWlHX0FKQy0tcWM&amp;authuser=0
		/// This method is used to determine what amount should be charged automatically by charger.
		/// </summary>
		/// <param name="today">Date to calculate payment on.</param>
		/// <param name="setClosedDateFromPayments">Update scheduled payment closed date from actual payments
		/// or leave it as is.</param>
		/// <param name="writeToLog">Write result to log or not.</param>
		/// <returns>Loan balance on specific date.</returns>
		public virtual CurrentPaymentModel GetAmountToChargeForAutoCharger(
			DateTime today,
			bool setClosedDateFromPayments = false,
			bool writeToLog = true
		) {
			return new GetAmountToChargeForAutoChargerMethod(this, today, setClosedDateFromPayments, writeToLog).Execute();
		} // GetAmountToChargeForAutoCharger

		/// <summary>
		/// Calculates loan earned interest between two dates including both dates.
		/// </summary>
		/// <param name="startDate">First day of the calculation period; loan issue date is used if omitted.</param>
		/// <param name="endDate">Last day of the calculation period; last scheduled payment date is used is omitted.</param>
		/// <param name="writeToLog">Write result to log or not.</param>
		/// <returns>Loan earned interest during specific date range.</returns>
		public virtual decimal CalculateEarnedInterest(DateTime? startDate, DateTime? endDate, bool writeToLog = true) {
			return new CalculateEarnedInterestMethod(this, startDate, endDate, writeToLog).Execute();
		} // CalculateEarnedInterest

		public virtual LoanCalculatorModel WorkingModel { get; private set; }

		/// <summary>
		/// Calculates date after requested number of periods have passed since loan issue date.
		/// Periods length is determined from WorkingModel.RepaymentIntervalType.
		/// </summary>
		/// <returns>Date after requested number of periods have been added to loan issue date.</returns>
		/// <summary>
		/// Calculates date after requested number of periods have passed since loan issue date.
		/// Periods length is determined from WorkingModel.RepaymentIntervalType.
		/// </summary>
		/// <param name="periodCount">
		///     A number of periods to add.
		///     A number of periods to add.
		/// </param>
		/// <returns>Date after requested number of periods have been added to loan issue date.</returns>
		public virtual DateTime AddRepaymentIntervals(int periodCount) {
			return
			WorkingModel.IsMonthly
			? WorkingModel.LoanIssueTime.AddMonths(periodCount)
			: WorkingModel.LoanIssueTime.AddDays(periodCount * (int)WorkingModel.RepaymentIntervalType);
		} // AddRepaymentIntervals

		/// <summary>
		/// Calculates interest rate for one day based on monthly interest rate.
		/// Bad periods and interest freeze periods can be ignored (<paramref name="usePeriods"/>).
		/// </summary>
		/// <param name="currentDate">Current date.</param>
		/// <param name="monthlyInterestRate">Monthly interest rate.</param>
		/// <param>True, to take in account bad and interest freeze periods,
		/// or false, to ignore them.</param>
		/// <param name="considerFreezeInterestPeriod"></param>
		/// <param name="periodStartDate">Period start date (the first day of the period).</param>
		/// <param name="periodEndDate">Period end date (the last day of the period).</param>
		/// <param name="considerBadPeriods"></param>
		/// <returns>Daily interest rate.</returns>
		internal decimal GetDailyInterestRate(
			DateTime currentDate,
			decimal monthlyInterestRate,
			bool considerBadPeriods,
			bool considerFreezeInterestPeriod,
			DateTime? periodStartDate = null,
			DateTime? periodEndDate = null
		) {
			//return (usePeriods && WorkingModel.BadPeriods.Contains(currentDate))
			//	? 0
			//	: CalculateDailyInterestRate(currentDate, monthlyInterestRate, periodStartDate, periodEndDate);

			if (considerBadPeriods)
				return 0;

			if (considerFreezeInterestPeriod) {
				InterestFreeze interval = WorkingModel.FreezePeriods.First(i => i.Contains(currentDate));
				var interest = (interval == null) ? CalculateDailyInterestRate(currentDate, monthlyInterestRate, periodStartDate, periodEndDate) : interval.GetInterest(currentDate);
				if (interest != null) {
					return (decimal)interest;
				}
			}

			return CalculateDailyInterestRate(currentDate, monthlyInterestRate, periodStartDate, periodEndDate);

		} // GetDailyInterestRate

		/// <exception cref="NullLoanCalculatorModelException">Condition. </exception>
		protected ALoanCalculator(LoanCalculatorModel model) {
			if (model == null)
				throw new NullLoanCalculatorModelException();

			WorkingModel = model;
		} // constructor

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




		//public virtual List<ScheduledItemWithAmountDue> RescheduleToIntervals(ReschedulingArgument reschedulingArgument, bool writeToLog = true) {
		//	Console.WriteLine("intervals for balance {0} is {1}, loan original close date ('maturity date'): {2}, reschedule date: {3}, intervat type: {4}",
		//			reschedulingArgument.ReschedulingBalance, WorkingModel.RepaymentCount, reschedulingArgument.LoanCloseDate, reschedulingArgument.ReschedulingDate
		//			,reschedulingArgument.ReschedulingRepaymentIntervalType.ToString());

		//	return new RescheduleToIntervalsMethod(this, reschedulingArgument, writeToLog).Execute();
		//} // RescheduleToIntervals

	} // class LoanCalculator
} // namespace
