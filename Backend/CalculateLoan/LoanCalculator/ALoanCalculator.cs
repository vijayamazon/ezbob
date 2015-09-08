namespace Ezbob.Backend.CalculateLoan.LoanCalculator {
	using System;
	using DbConstants;
	using Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions;
	using Ezbob.Backend.CalculateLoan.LoanCalculator.Methods;
	using Ezbob.Backend.CalculateLoan.Models.Exceptions;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Logger;

	/// <summary>
	/// This class is implemented as Facade pattern.
	/// https://en.wikipedia.org/wiki/Facade_pattern
	/// </summary>
	public abstract partial class ALoanCalculator {
		public abstract string Name { get; }

		public virtual bool WriteToLog {
			get { return this.writeToLog; }
			set { this.writeToLog = value; }
		} // WriteToLog

		public virtual NL_Model WorkingModel { get; private set; }

		/// <summary>
		/// Create shcedule by amount/issue date/schedules number (+interest only)/interval type
		/// If input model contains offer-fee data and contains the ony one history - creates setup/servicing loan fees. 
		/// On fees 2.0 full support - complete logic accordingly
		/// 
		/// Expected model structure: 
		/// 1. loanModel.Histories (List<NL_LoanHistory>) includes at least one history
		/// 2. history with: EventTime, Amount, InterestRate, RepaymentCount, RepaymentIntervalTypeID
		/// 3. optional: Loan.InterestOnlyRepaymentCount
		/// 4. optional: DiscountPlan (List<decimal>)
		/// 5. optional: model.Fees (List<NLFeeItem>) with list of NL_OfferFees
		/// 6. optional: model.Offer.BrokerSetupFeePercent (decimal?)
		/// 
		/// </summary>
		/// <exception cref="NoInitialDataException">Condition. </exception>
		/// <exception cref="InvalidInitialInterestOnlyRepaymentCountException">Condition. </exception>
		/// <exception cref="InvalidInitialRepaymentCountException">Condition. </exception>
		/// <exception cref="InvalidInitialInterestRateException">Condition. </exception>
		/// <exception cref="InvalidInitialAmountException">Condition. </exception>
		public virtual void CreateSchedule() {
			new CreateScheduleMethod(this, WorkingModel).Execute();
		} // CreateScheduleAndPlan

		/// <summary>
		/// </summary>
		/// <exception cref="NoInitialDataException">Condition. </exception>
		/// <exception cref="NoLoanHistoryException">Condition. </exception>
		/// <exception cref="NoScheduleException">Condition. </exception>
		public virtual void CalculateSchedule() {
			new CalculateScheduleMethod(this, WorkingModel).Execute();
		} // CreateScheduleAndPlan

		/// <summary>
		/// Calculates current loan balance.
		/// </summary>
		/// <param name="today">Date to calculate balance on.</param>
		/// <returns>Loan balance on specific date.</returns>
		public virtual void CalculateBalance(DateTime today) {
			// TODO: revive
			// new CalculateBalanceMethod(this, today, WriteToLog).Execute();
		} // CalculateBalance

		/// <summary>
		/// Calculates payment options (late/current/next installment/full balance) for requested date.
		/// Method logic: https://drive.google.com/open?id=0B1Io_qu9i44SaWlHX0FKQy0tcWM&amp;authuser=0
		/// This method is used to calculate charge options that should be displayed to customer in the dashboard.
		/// </summary>
		/// <param name="today">Date to calculate payment on.</param>
		/// <param name="setClosedDateFromPayments">Update scheduled payment closed date from actual payments
		/// or leave it as is.</param>
		/// <returns>Loan balance on specific date.</returns>
		public virtual void /* CurrentPaymentModel */ GetAmountToChargeForDashboard(
			DateTime today,
			bool setClosedDateFromPayments = false) {
			// TODO: revive
			// new GetAmountToChargeForDashboardMethod(this, today, setClosedDateFromPayments, WriteToLog).Execute();
		} // GetAmountToChargeForDashboard

		/// <summary>
		/// Calculates payment options (late/current installment) for requested date.
		/// Method logic: https://drive.google.com/open?id=0B1Io_qu9i44SaWlHX0FKQy0tcWM&amp;authuser=0
		/// This method is used to determine what amount should be charged automatically by charger.
		/// </summary>
		/// <param name="today">Date to calculate payment on.</param>
		/// <param name="setClosedDateFromPayments">Update scheduled payment closed date from actual payments
		/// or leave it as is.</param>
		/// <returns>Loan balance on specific date.</returns>
		public virtual void /* CurrentPaymentModel */ GetAmountToChargeForAutoCharger(
			DateTime today,
			bool setClosedDateFromPayments = false
		) {
			// TODO: revive
			// new GetAmountToChargeForAutoChargerMethod(this, today, setClosedDateFromPayments, WriteToLog).Execute();
		} // GetAmountToChargeForAutoCharger

		/// <summary>
		/// Calculates loan earned interest between two dates including both dates.
		/// </summary>
		/// <param name="startDate">First day of the calculation period; loan issue date is used if omitted.</param>
		/// <param name="endDate">Last day of the calculation period; last scheduled payment date is used is omitted.</param>
		/// <returns>Loan earned interest during specific date range.</returns>
		public virtual decimal CalculateEarnedInterest(DateTime? startDate, DateTime? endDate) {
			return new CalculateEarnedInterestMethod(this, startDate, endDate, WriteToLog).Execute();
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
		/// <returns>Loan APR.</returns>
		public virtual decimal CalculateApr(
			DateTime? aprDate = null,
			double? calculationAccuracy = null,
			ulong? maxIterationCount = null
			) {
			var method = new CalculateAprMethod(this, aprDate, WriteToLog);

			if (calculationAccuracy.HasValue)
				method.CalculationAccuracy = calculationAccuracy.Value;

			if (maxIterationCount.HasValue)
				method.MaxIterationLimit = maxIterationCount.Value;

			return method.Execute();
		} // CalculateApr

		

		protected ALoanCalculator(NL_Model model) {
			if (model == null)
				Log.Msg("No model specified for loan calculator, using a new empty model.");

			WorkingModel = model;
			this.writeToLog = true;
		} // constructor



		public DateTime AddRepaymentIntervals(int periodCount, DateTime issuedTime, RepaymentIntervalTypes intervalType = RepaymentIntervalTypes.Month) {
			return (intervalType == RepaymentIntervalTypes.Month)
				? issuedTime.AddMonths(periodCount)
				: issuedTime.AddDays(periodCount * (int)intervalType);
		} // AddRepaymentIntervals


		public DateTime PreviousScheduleDate(DateTime theDate, RepaymentIntervalTypes intervalType = RepaymentIntervalTypes.Month) {
			return (intervalType == RepaymentIntervalTypes.Month)? theDate.AddMonths(-1): theDate.AddDays(-(int)intervalType);
		} // AddRepaymentIntervals

		/// <summary>
		/// Calculates average daily interest rate (r'), based on monthlyInterestRate (r) and periodEndDate (for example, scheduleItem.PlannedDate)
		/// </summary>
		/// <param name="monthlyInterestRate"></param>
		/// <param name="periodEndDate"></param>
		/// <returns></returns>
		public abstract decimal CalculateDailyInterestRate(decimal monthlyInterestRate, DateTime? periodEndDate = null);

		// TODO
		protected decimal CalculateDailyInterestForOpenPrincipal() {
			return 0m;
		} // GetDailyInterestForOpenPrincipal

		protected static ASafeLog Log { get { return Library.Instance.Log; } }

		private bool writeToLog ;

	} // class ALoanCalculator
} // namespace
