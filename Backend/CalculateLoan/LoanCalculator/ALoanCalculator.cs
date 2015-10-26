namespace Ezbob.Backend.CalculateLoan.LoanCalculator {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DbConstants;
	using Ezbob.Backend.CalculateLoan.LoanCalculator.Methods;
	using Ezbob.Backend.CalculateLoan.LoanCalculator.Models;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Logger;
	using MoreLinq;

	/// <summary>
	/// This class is implemented as Facade pattern. https://en.wikipedia.org/wiki/Facade_pattern
	/// </summary>
	public abstract partial class ALoanCalculator {
		/// <exception cref="NoInitialDataException">Condition. </exception>
		protected ALoanCalculator(NL_Model model, DateTime? calculationDate = null) {
			//if (model == null)
			//	Log.Msg("No model specified for loan calculator, using a new empty model.");

			if (model == null)
				throw new NoInitialDataException();

			if (model.Loan == null)
				throw new NoInitialDataException();

			WorkingModel = model;
			this.writeToLog = true;

			// use now as default 
			if (calculationDate == DateTime.MinValue)
				CalculationDate = DateTime.UtcNow;

			// reset internal vars
			initialLoanAmount = 0;
			//openPrincipal = 0;
			//totalFees = 0;
			//totalPrincipal = 0;
			//totalInterest = 0;
			//paidFees = 0;
			//paidInterest = 0;
			//paidPrincipal = 0;

			List<LoanEvent> historiesEvents = new List<LoanEvent>();
			List<LoanEvent> feesEvents = new List<LoanEvent>();
			List<LoanEvent> paymentEvents = new List<LoanEvent>();
			List<LoanEvent> schedulesEvents = new List<LoanEvent>();

			if (model.Loan.Histories.Count > 0) {
				model.Loan.Histories.ForEach(e => historiesEvents.Add(new LoanEvent(new DateTime(e.EventTime.Year, e.EventTime.Month, e.EventTime.Day), e)));
			}

			if (model.Loan.Fees.Count > 0) {
				model.Loan.Fees.ForEach(e => feesEvents.Add(new LoanEvent(new DateTime(e.AssignTime.Year, e.AssignTime.Month, e.AssignTime.Day), e)));
				// handled in AddLoan strategy 
				//feesEvents.RemoveAll(e => e.Fee.LoanFeeTypeID == (int)NLFeeTypes.SetupFee); // TODO: mark setup fees that not for charge, somehow, or place in other table

				totalFees = 0;
				model.Loan.Fees.Where(f => f.DisabledTime == null).ForEach(f => totalFees += f.Amount);
			}

			if (model.Loan.Payments.Count > 0)
				model.Loan.Payments.ForEach(e => paymentEvents.Add(new LoanEvent(new DateTime(e.PaymentTime.Year, e.PaymentTime.Month, e.PaymentTime.Day), e)));

			if (model.Loan.Histories.Count > 0) {
				model.Loan.Histories.ForEach(h => h.Schedule.ForEach(e => schedulesEvents.Add(new LoanEvent(new DateTime(e.PlannedDate.Year, e.PlannedDate.Month, e.PlannedDate.Day), e))));

				initialLoanAmount = model.Loan.FirstHistory().Amount;
				totalPrincipal = initialLoanAmount;
			}

			// TODO adust to all cases
			//if (model.Loan.FirstHistory() != null) {
			//	eventDayStart = new LoanEvent(model.Loan.FirstHistory().EventTime.Date); // _term
			//	eventDayEnd = new LoanEvent(eventDayStart.Date.AddHours(23).AddMinutes(59).AddSeconds(59));
			//}

			this.events = historiesEvents
				.Union(schedulesEvents)
				.Union(paymentEvents)
				.Union(feesEvents)
				// .Union(rollOverEvents) TODO add rolovers
				// .Union(reschedules) TODO add reschedules
				// .Union(reschedules) TODO add reschedules
				//.Union(new[] { eventDayStart, eventDayEnd })
				.OrderBy(e => e.Date)
				.ThenBy(e => e.Priority)
				.ToList();

			//this.events.ForEach(e => Log.Debug(e));

		} // constructor

		public abstract string Name { get; }

		// date of loan state calculation
		public DateTime CalculationDate { get; set; }

		// initial amount, amount taken
		protected decimal initialLoanAmount { get; set; }

		//	schedules count
		protected int n { get; set; }

		// A, i.e. total principal of loan == initialAmount
		protected decimal totalPrincipal { get; set; }
		// total fees assigned to loan
		protected decimal totalFees { get; set; }
		// total loan interest (earned interest). Доход банка за все время заема
		protected decimal totalInterest { get; set; } // _totalInterestToPay

		/// <summary>
		/// последовательность событий, относящихся к кредиту
		/// events sequence related to loan
		/// </summary>
		public List<LoanEvent> events; // protected TODO for tests only public

		// date of last action (event). Needed for interest calculation дата последнего действия. нужна для расчета процентов
		protected DateTime lastEventDate { get; set; }

		// TODO check usage
		//protected LoanEvent eventDayStart { get; set; }
		//protected LoanEvent eventDayEnd { get; set; }

		protected static ASafeLog Log { get { return Library.Instance.Log; } }
		private bool writeToLog;

		public virtual bool WriteToLog {
			get { return this.writeToLog; }
			set { this.writeToLog = value; }
		} // WriteToLog

		public virtual NL_Model WorkingModel { get; private set; }

		/// <summary>
		/// Create schedule by amount/issue date/schedules number (+interest only)/interval type
		/// If input model contains offer-fee data and contains the ony one history - creates setup/servicing loan fees. 
		/// On fees 2.0 full support - complete logic accordingly
		/// Expected model structure: 
		/// 1. loanModel.Histories (List<NL_LoanHistory/>) includes at least one history
		/// 2. history with: Amount, InterestRate, RepaymentCount // EventTime, RepaymentIntervalTypeID - default now/month
		/// 3. optional: Loan.InterestOnlyRepaymentCount
		/// 4. optional: DiscountPlan (List<decimal/>)
		/// 5. optional: model.Fees (List<NLFeeItem/>) with list of NL_OfferFees
		/// 6. optional: model.Offer.BrokerSetupFeePercent (decimal?)
		/// </summary>
		/// <exception cref="NoInitialDataException">Condition. </exception>
		/// <exception cref="InvalidInitialRepaymentCountException">Condition. </exception>
		/// <exception cref="InvalidInitialInterestRateException">Condition. </exception>
		/// <exception cref="InvalidInitialAmountException">Condition. </exception>
		public virtual void CreateSchedule() {
			new CreateScheduleMethod(this, WorkingModel).Execute();
		} // CreateScheduleAndPlan

		/*/// <summary>
		/// </summary>
		/// <exception cref="NoInitialDataException">Condition. </exception>
		/// <exception cref="NoLoanHistoryException">Condition. </exception>
		/// <exception cref="NoScheduleException">Condition. </exception>
		//public virtual void CalculateSchedule() {
		//	new CalculateScheduleMethod(this, WorkingModel).Execute();
		//} // CreateScheduleAndPlan*/

		/// <summary>
		/// Calculates current loan balance.
		/// </summary>
		/// <param name="today">Date to calculate balance on.</param>
		/// <returns>Loan balance on specific date.</returns>
		public virtual void CalculateBalance(DateTime today) {
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
		public virtual void GetAmountToChargeForDashboard(DateTime today, bool setClosedDateFromPayments = false) {
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
		public virtual void GetAmountToChargeForAutoCharger(DateTime today, bool setClosedDateFromPayments = false) {
			// new GetAmountToChargeForAutoChargerMethod(this, today, setClosedDateFromPayments, WriteToLog).Execute();
		} // GetAmountToChargeForAutoCharger

		/// <summary>
		/// Calculates loan earned interest between two dates including both dates.
		/// </summary>
		/// <param name="startDate">First day of the calculation period; loan issue date is used if omitted.</param>
		/// <param name="endDate">Last day of the calculation period; last scheduled payment date is used is omitted.</param>
		/// <returns>Loan earned interest during specific date range.</returns>
		public virtual decimal CalculateEarnedInterest(DateTime? startDate, DateTime? endDate) {
			//return new CalculateEarnedInterestMethod(this, startDate, endDate, WriteToLog).Execute();
			return 0m;
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

		public DateTime AddRepaymentIntervals(int intervals, DateTime issuedTime, RepaymentIntervalTypes intervalType = RepaymentIntervalTypes.Month) {
			return (intervalType == RepaymentIntervalTypes.Month)
				? issuedTime.AddMonths(intervals)
				: issuedTime.AddDays(intervals * (int)intervalType);
		} // AddRepaymentIntervals


		public DateTime PreviousScheduleDate(DateTime theDate, RepaymentIntervalTypes intervalType = RepaymentIntervalTypes.Month) {
			return (intervalType == RepaymentIntervalTypes.Month) ? theDate.AddMonths(-1) : theDate.AddDays(-(int)intervalType);
		} // PreviousScheduleDate

		/// <summary>
		/// Calculates average daily interest rate (r'), based on monthlyInterestRate (r) and periodEndDate (for example, scheduleItem.PlannedDate)
		/// </summary>
		/// <param name="monthlyInterestRate"></param>
		/// <param name="periodEndDate"></param>
		/// <returns></returns>
		public abstract decimal CalculateDailyInterestRate(decimal monthlyInterestRate, DateTime? periodEndDate = null);


		/// <summary>
		/// Calculates interest rate for one day based on monthly interest rate.
		/// </summary>
		/// <param name="currentDate">Current date.</param>
		/// <param name="monthlyInterestRate">Monthly interest rate.</param>
		/// <returns>Daily interest rate with bad status/freeze interest check.</returns>
		internal decimal InterestRateForDate(DateTime currentDate, decimal monthlyInterestRate) {
			// TODO: check freeze and reset interest if need. "bad status" phase 2
			//if (WorkingModel.BadPeriods.Any(bp => bp.Contains(currentDate)))
			//	return 0;
			decimal dr = CalculateDailyInterestRate(monthlyInterestRate, currentDate);
			return dr;
		}


		/// <summary>
		/// common: Calculate total interest to pay at specific event date, during looping through ordered events list
		/// calculate interest rate btwn 2 nearby events in the list day by day; add interest calculated to totalInterest; set currentEvent date as lastEventDate
		/// </summary>
		/// <param name="currentEvent"></param>
		/// <param name="monthlyInterestrate"></param>
		//public virtual void InterestBtwnEvents(DateTime currentEvent, decimal monthlyInterestrate) {

		//	int daysDiff = currentEvent.Date.Subtract(lastEventDate).Days;
		//	DateTime start = lastEventDate;

		//	Log.Info("daysDiff: {0}, lastEventDate: {1}, currentEvent: {2}", daysDiff, initialLoanAmount, currentEvent);

		//	//	interest for period
		//	for (int i = 0; i < daysDiff; i++) {

		//		DateTime rateDate = start.AddDays(i);
		//		decimal dailyInterestRate = InterestRateForDate(rateDate, monthlyInterestrate); // dr' = r/daysDiff
		//		decimal interest = openPrincipal * dailyInterestRate; //daily interest
		//		totalInterest += interest;

		//		Log.Info("rateDate: {0} interest: {1} totalInterest: {2} openPrincipal: {3}, i={4}", rateDate, interest, totalInterest, openPrincipal, i);
		//	}

		//	//	last event datetime
		//	lastEventDate = currentEvent;
		//}



		/*
		 * internal decimal GetDailyInterestRate(
		DateTime currentDate,
		decimal monthlyInterestRate,
		bool considerBadPeriods,
		bool considerFreezeInterestPeriods,
		DateTime? periodStartDate = null,
		DateTime? periodEndDate = null	) {
		// TODO: revive
		return 0;
			
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
		
	} */
		// GetDailyInterestRate


	} // class ALoanCalculator
} // namespace
