namespace Ezbob.Backend.CalculateLoan.LoanCalculator {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions;
	using Ezbob.Backend.CalculateLoan.LoanCalculator.Methods;
	using Ezbob.Backend.CalculateLoan.Models;
	using Ezbob.Backend.CalculateLoan.Models.Helpers;

	/// <summary>
	/// This class is implemented as Facade pattern.
	/// https://en.wikipedia.org/wiki/Facade_pattern
	/// </summary>
	public abstract partial class ALoanCalculator {
		public abstract string Name { get; }

		public virtual LoanCalculatorModel WorkingModel { get; private set; }

		/// <summary>
		/// Creates loan schedule by loan issue time, repayment count, repayment interval type and discount plan.
		/// Schedule is stored in WorkingModel.Schedule.
		/// Each element in Schedule is ScheduledItem
		/// </summary>
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

		/// <summary>
		/// Calculates loan APR relative to specific date.
		/// </summary>
		/// <param name="aprDate">Date to calculate APR. Current date if omitted.</param>
		/// <param name="calculationAccuracy">Calculation stops when this accuracy was reached.
		/// If not specified default (1e-7) is used.
		/// </param>
		/// <param name="maxIterationCount">
		/// Calculation stops if this number of iterations was reached.
		/// Zero means iterate as many as needed.
		/// If not specified default (100k) is used.
		/// </param>
		/// <param name="writeToLog">Write result to log or not.</param>
		/// <returns>Loan APR.</returns>
		public virtual decimal CalculateApr(
			DateTime? aprDate = null,
			double? calculationAccuracy = null,
			ulong? maxIterationCount = null,
			bool writeToLog = true
		) {
			var method = new CalculateAprMethod(this, aprDate, writeToLog);

			if (calculationAccuracy.HasValue)
				method.CalculationAccuracy = calculationAccuracy.Value;

			if (maxIterationCount.HasValue)
				method.MaxIterationLimit = maxIterationCount.Value;

			return method.Execute();
		} // CalculateApr

		/// <summary>
		/// Calculates interest rate for one day based on monthly interest rate.
		/// Bad periods and interest freeze periods can be ignored (<paramref />).
		/// </summary>
		/// <param name="currentDate">Current date.</param>
		/// <param name="monthlyInterestRate">Monthly interest rate.</param>
		/// <param name="considerBadPeriods">True, to take in account bad periods,
		/// or false, to ignore them.</param>
		/// <param name="considerFreezeInterestPeriods">True, to take in account interest freeze periods,
		/// or false, to ignore them.</param>
		/// <param name="periodStartDate">Period start date (the first day of the period).</param>
		/// <param name="periodEndDate">Period end date (the last day of the period).</param>
		/// <returns>Daily interest rate.</returns>
		internal decimal GetDailyInterestRate(
			DateTime currentDate,
			decimal monthlyInterestRate,
			bool considerBadPeriods,
			bool considerFreezeInterestPeriods,
			DateTime? periodStartDate = null,
			DateTime? periodEndDate = null
		) {
			if (considerBadPeriods && WorkingModel.BadPeriods.Any(bp => bp.Contains(currentDate)))
				return 0;

			if (considerFreezeInterestPeriods && (WorkingModel.FreezePeriods.Count > 0)) {
				InterestFreeze interval = WorkingModel.FreezePeriods.First(i => i.Contains(currentDate));

				var interest = (interval == null)
					? CalculateDailyInterestRate(currentDate, monthlyInterestRate, periodStartDate, periodEndDate)
					: interval.GetInterest(currentDate);

				if (interest != null)
					return (decimal)interest;
			} // if

			return CalculateDailyInterestRate(currentDate, monthlyInterestRate, periodStartDate, periodEndDate);
		} // GetDailyInterestRate

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
	} // class ALoanCalculator
} // namespace
