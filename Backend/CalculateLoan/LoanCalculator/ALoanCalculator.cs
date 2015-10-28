namespace Ezbob.Backend.CalculateLoan.LoanCalculator {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DbConstants;
	using Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions;
	using Ezbob.Backend.CalculateLoan.LoanCalculator.Methods;
	using Ezbob.Backend.CalculateLoan.LoanCalculator.Models;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Logger;

	/// <summary>
	/// This class is implemented as Facade pattern. https://en.wikipedia.org/wiki/Facade_pattern
	/// </summary>
	public abstract partial class ALoanCalculator {

		/// <exception cref="NoInitialDataException">Condition. </exception>
		/// <exception cref="InvalidInitialInterestRateException">Condition. </exception>
		/// <exception cref="NoLoanHistoryException">Condition. </exception>
		protected ALoanCalculator(NL_Model model, DateTime? calculationDate = null) {
			//if (model == null)
			//	Log.Msg("No model specified for loan calculator, using a new empty model.");

			if (model == null)
				throw new NoInitialDataException();

			if (model.Loan == null)
				throw new NoInitialDataException();

			if (model.Loan.LastHistory() == null)
				throw new NoLoanHistoryException();

			if (model.Loan.LastHistory().InterestRate == 0)
				throw new InvalidInitialInterestRateException(0);

			// use now as default 
			if (calculationDate == DateTime.MinValue)
				CalculationDate = DateTime.UtcNow;

			WorkingModel = model;
			this.writeToLog = true;

			SetCalculatorDefaults();

			// events
			Init();
		}

		private void SetCalculatorDefaults() {

			// set definitions from the last history
			currentHistory = WorkingModel.Loan.LastHistory();

			// init common data
			//LoanInterestrate = currentHistory.InterestRate;
			//InitialLoanAmount = currentHistory.Amount;

			// start to cumulate interest one day after LoanIssueDate 
			InterestCalculationDateStart = WorkingModel.Loan.FirstHistory().EventTime.AddDays(1);

			// default: end to cumulate interest at same date the last schedule planned (loan maturity date)
			var lastSchedule = WorkingModel.Loan.LastHistory().Schedule.OrderBy(s => s.PlannedDate).LastOrDefault();

			InterestCalculationDateEnd = (lastSchedule != null) ? lastSchedule.PlannedDate : InterestCalculationDateEnd;

			totalPrincipal = WorkingModel.Loan.FirstHistory().Amount;

			openPrincipal = totalPrincipal;
		}

		public abstract string Name { get; }

		// date of loan state calculation
		public DateTime CalculationDate { get; set; }

		// Common loan data
		//protected decimal InitialLoanAmount { get; set; }
		// from history
		//public decimal LoanInterestrate { get; set; }
		//	schedules count
		protected int n { get; set; }


		// Loan State helpers data

		/// <summary>
		/// последовательность событий, относящихся к кредиту. events sequence related to loan
		/// </summary>
		public List<LoanEvent> events; // protected TODO for tests only public

		// period for interest calculation
		public DateTime InterestCalculationDateStart { get; set; }
		public DateTime InterestCalculationDateEnd { get; set; }

		// date of last action (event). Needed for interest calculation дата последнего действия. нужна для расчета процентов
		protected DateTime lastEventDate { get; set; }

		protected List<NL_LoanSchedules> schedule = new List<NL_LoanSchedules>();

		protected List<NL_LoanSchedules> processedInstallments = new List<NL_LoanSchedules>();
		protected List<NL_LoanFees> feesToPay = new List<NL_LoanFees>();

		private NL_LoanHistory currentHistory { get; set; }

		// A, i.e. total principal of loan == initialAmount
		protected decimal totalPrincipal { get; set; }
		// total fees assigned to loan
		protected decimal totalFees { get; set; }
		// total loan interest (earned interest). Доход банка за все время заема
		protected decimal totalInterest { get; set; } // _totalInterestToPay

		// Loan state public data

		//	Money that really have customer. Loan balance without interest and fees. деньги, которые реально находятся у клиента на руках. баланс кредита без процентов и fee. 
		protected decimal openPrincipal { get; set; } //private decimal _principal;
		// interest to paid
		protected decimal openInterest { get; set; }
		// fees to paid
		protected decimal openFees { get; set; }

		// ### Loan state public data

		// interest paid on CalculationDate
		protected decimal paidInterest { get; set; }
		// principal paid on CalculationDate
		protected decimal paidPrincipal { get; set; }
		// fees paid on CalculationDate
		protected decimal paidFees { get; set; }

		// ### Loan State helpers data

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
		/// Calculates average daily interest rate (r'), based on monthlyInterestRate (r) and periodEndDate
		/// </summary>
		/// <param name="monthlyInterestRate"></param>
		/// <param name="periodEndDate"></param>
		/// <returns></returns>
		public abstract decimal AverageDailyInterestRate(decimal monthlyInterestRate, DateTime? periodEndDate = null);


		/// <summary>
		/// Calculates interest rate for specific day based on loan data: 
		/// 1. Check freeze intervals
		/// 2. Find relevant schedule item
		/// 3. If schedule not found - use rate defined in the last history
		/// </summary>
		/// <param name="theDate"></param>
		/// <returns>Daily interest rate with freeze interest check.</returns>
		public decimal DailyInterestRateForDate(DateTime theDate) {

			// During changing customer's status to "bad", new freeze interest interval create, so "bad statuses" included to freezes

			// the date is between?
			var freezePeriod = WorkingModel.Loan.FreezeInterestIntervals.FirstOrDefault(fr => fr.StartDate >= theDate.Date && theDate.Date <= fr.EndDate && fr.ActivationDate <= theDate.Date);

			Log.Debug("freezePeriod: {0}, theDate: {1}", freezePeriod, theDate);

			if (freezePeriod != null)
				return 0;

			// find relevant schedule (theDate inside it)
			var scheduleItem = this.schedule.FirstOrDefault(s => PreviousScheduleDate(s.PlannedDate.Date, (RepaymentIntervalTypes)currentHistory.RepaymentIntervalTypeID) >= theDate.Date && theDate.Date <= s.PlannedDate.Date);

			Log.Debug("theDate: {0}, scheduleItem {1}", theDate, scheduleItem);

			// dr' = r/daysDiff

			decimal dr = AverageDailyInterestRate(currentHistory.InterestRate); 

			if (scheduleItem != null) 
				dr = AverageDailyInterestRate(scheduleItem.Interest, scheduleItem.PlannedDate);

			Log.Debug("theDate: {0}, dailyRate {1}", theDate, dr);

			return dr;
		}

		/// <summary> 
		/// Interest rate for a specific date, defined in schedule. If Schedule exist, returns scheduled rate, otherwise returns history defined rate
		/// </summary>
		/// <returns></returns>
		/// <summary>
		/// common: Calculate total interest to pay at specific event date, during looping through ordered events list
		/// calculate interest rate btwn 2 nearby events in the list day by day; add interest calculated to totalInterest; set currentEvent date as lastEventDate
		/// </summary>
		/// <param name="currentEvent"></param>
		public virtual decimal GetInterestBtwnEvents(LoanEvent currentEvent) {

			int daysDiff = currentEvent.Date.Subtract(lastEventDate).Days;
			DateTime start = lastEventDate; // actually could be found in the list of events
			decimal interestForPeriod = 0;

			Log.Debug("lastEventDate: {0}, currentEvent: {1}, days: {2}", lastEventDate, currentEvent, daysDiff);

			//	interest for period  dr'(t)*p'(t)
			DateTime rateDate = start;

			for (int i = 0; i < daysDiff; i++) {

				decimal dailyInterestRate = DailyInterestRateForDate(rateDate); 
				decimal interest = openPrincipal * dailyInterestRate; //	daily interest dr'(t)*pP(t)
				interestForPeriod += interest;
				rateDate = start.AddDays(i);
				//totalInterest += interest;

				Log.Info("rateDate: {0} interest: {1} totalInterest: {2} openPrincipal: {3}, i={4}", rateDate, interest, totalInterest, openPrincipal, i);
			}

			return interestForPeriod;
		}

		

		/// <summary>
		/// Initialize loan events list
		/// </summary>
		private void Init() {

			totalFees = 0;

			paidFees = 0;
			paidInterest = 0;
			paidPrincipal = 0;

			List<LoanEvent> historiesEvents = new List<LoanEvent>();
			List<LoanEvent> feesEvents = new List<LoanEvent>();
			List<LoanEvent> paymentEvents = new List<LoanEvent>();
			List<LoanEvent> schedulesEvents = new List<LoanEvent>();

			if (WorkingModel.Loan.Histories.Count > 0) {
				WorkingModel.Loan.Histories.ForEach(e => historiesEvents.Add(new LoanEvent(new DateTime(e.EventTime.Year, e.EventTime.Month, e.EventTime.Day), e)));
			}

			if (WorkingModel.Loan.Fees.Count > 0) {
				foreach (var e in WorkingModel.Loan.Fees) {
					feesEvents.Add(new LoanEvent(new DateTime(e.AssignTime.Year, e.AssignTime.Month, e.AssignTime.Day), e));
					totalFees += e.Amount;
				}
			}

			if (WorkingModel.Loan.Payments.Count > 0) {
				foreach (var e in WorkingModel.Loan.Payments) {
					// events
					paymentEvents.Add(new LoanEvent(new DateTime(e.PaymentTime.Year, e.PaymentTime.Month, e.PaymentTime.Day)));

					// paid schedules - interest
					paidInterest += e.SchedulePayments.Sum(p => p.InterestPaid);

					// paid schedules - principal
					paidPrincipal += e.SchedulePayments.Sum(p => p.PrincipalPaid);

					// paif fees
					paidFees +=e.FeePayments.Sum(p => p.Amount);
				}
			}

			if (WorkingModel.Loan.Histories.Count > 0) {
				foreach (var h in WorkingModel.Loan.Histories) {
					h.Schedule.ForEach(e=> schedulesEvents.Add(new LoanEvent(new DateTime(e.PlannedDate.Year, e.PlannedDate.Month, e.PlannedDate.Day), e)));
					h.Schedule.ForEach(s => this.schedule.Add(s));
				}
			}

			// combine ordered events list
			this.events = historiesEvents
				.Union(schedulesEvents)
				.Union(paymentEvents)
				.Union(feesEvents)
				// .Union(rollOverEvents) TODO add rolovers
				// .Union(reschedules) TODO add reschedules
				.OrderBy(e => e.Date)
				.ThenBy(e => e.Priority)
				.ToList();

			// clear null events
			this.events.RemoveAll(e => e == null);
			
			foreach (var e in this.events) {
				Log.Debug("{0}", e);

				HandleEvent(e);

				decimal interestBtwnEvents = GetInterestBtwnEvents(e);
				Log.Debug("accruedInterest: {0}", interestBtwnEvents);

				lastEventDate = e.Date;
			}
		}

		private void HandleEvent(LoanEvent e) {

			string etype = e.GetTypeString();

			switch (etype) {

			case "History":

				currentHistory = e.History;
				e.CurrentHistory = currentHistory;
				HandleHistoryEvent(e.History);
				break;

			case "Installment":

				e.CurrentHistory = currentHistory;
				HandleInstallmentEvent(e.Installment);

				Log.Debug("InstallmentEvent lastEventDate: {0}", lastEventDate);
				break;

			case "Fee":

				e.CurrentHistory = currentHistory;
				HandleFeeEvent(e.Fee);
				//	lastEventDate = e.Fee.AssignTime;

				Log.Debug("FeeEvent lastEventDate: {0}", lastEventDate);

				break;

			case "Payment":

				e.CurrentHistory = currentHistory;
				HandlePaymentEvent(e.Payment);
				lastEventDate = e.Payment.PaymentTime;
				Log.Debug("PaymentEvent lastEventDate: {0}", lastEventDate);
				break;

			case "Rollover":
				e.CurrentHistory = currentHistory;
				HandleRolloverEvent(e.Rollover);
				Log.Debug("RolloverEvent lastEventDate: {0}", lastEventDate);
				break;

			case "Action":
				e.CurrentHistory = currentHistory;
				HandleActionEvent(e.Action);
				lastEventDate = e.Date;
				Log.Debug("ActionEvent lastEventDate: {0}", lastEventDate);
				break;

			default:
				Log.Debug("Unknown event type: {0}", e);
				break;
			}
		}
		

		private void HandleHistoryEvent(NL_LoanHistory history) {
			Log.Debug("HandleHistoryEvent NotImplementedException");
		}

		/// <summary>
		/// Payments:
		/// 1. paying fees (update openFees) 
		/// 2. paying interest (update openInterest) 
		/// 3. paying rollover
		/// 4. paying principal  (update openPrincipal) 
		/// </summary>
		/// <param name="payment"></param>
		private void HandlePaymentEvent(NL_Payments payment) {

			decimal paidPrincipalOnEvent = payment.SchedulePayments.Sum(p => p.PrincipalPaid);

			Log.Debug("totalPrincipal: {0}, openPrincipal: {1}, paidPrincipalOnEvent: {2}", totalPrincipal, openPrincipal, paidPrincipalOnEvent);

			openPrincipal = totalPrincipal - paidPrincipalOnEvent;
			
			Log.Debug("HandlePaymentEvent NotImplementedException");
		}

		private void HandleInstallmentEvent(NL_LoanSchedules item) {

			this.processedInstallments.Add(item);

			//lastEventDate = item.PlannedDate;
			//Log.Debug("InstallmentEvent lastEventDate: {0}", lastEventDate);
		}

		private void HandleFeeEvent(NL_LoanFees fee) {
	
			// el: if loan PaidOff, mark the charge as Expired
			// el: Arrangement and servicing fees should be treated different
			//if (_loan.Status == LoanStatus.PaidOff) {
			//	charge.State = "Expired";
			//	return;
			//}

			// el: add fee to the first unpaid schedule item
			var unpaidScheduleItem = this.processedInstallments.LastOrDefault(s => s.LoanScheduleStatusID == (int)NLScheduleStatuses.Late || s.LoanScheduleStatusID == (int)NLScheduleStatuses.StillToPay);
			if (unpaidScheduleItem != null) {
				if (unpaidScheduleItem.LoanScheduleStatusID == (int)NLScheduleStatuses.Late || (unpaidScheduleItem.LoanScheduleStatusID == (int)NLScheduleStatuses.StillToPay && this.schedule.Count == this.processedInstallments.Count)) {
					unpaidScheduleItem.AmountDue += fee.Amount;
					//unpaidScheduleItem.Fees += fee.Amount;
				}
			}

			// el: reset AmountPaid of the charge
			//fee.AmountPaid = 0;
			// el: add to list of _chargesToPay - handles in RecordFeesPayment
			this.feesToPay.Add(fee);
		}

		private void HandleRolloverEvent(NL_LoanRollovers rollover) {
			//if (rollover == null) return;

			Log.Debug("HandleRolloverEvent not implemented {0}", rollover);
		}


		private void HandleActionEvent(object p) {
			Log.Debug("ActionEven NotImplementedException");
		}


		/*/// <summary> 
/// Interest rate for a specific date, defined in schedule. If Schedule exist, returns scheduled rate, otherwise returns history defined rate
/// </summary>
/// <returns></returns>
/// <summary>
/// common: Calculate total interest to pay at specific event date, during looping through ordered events list
/// calculate interest rate btwn 2 nearby events in the list day by day; add interest calculated to totalInterest; set currentEvent date as lastEventDate
/// </summary>
/// <param name="currentEvent"></param>
/// <param name="monthlyInterestrate"></param>
public virtual decimal InterestBtwnEvents(DateTime currentEvent, decimal monthlyInterestrate = 0) {

	int daysDiff = currentEvent.Date.Subtract(lastEventDate).Days;
	DateTime start = lastEventDate;
	decimal interestForPeriod = 0;

	Log.Info("lastEventDate: {0}, currentEvent: {1}, days: {2}", lastEventDate, currentEvent, daysDiff);

	//	interest for period  dr'(t)*p'(t)

	for (int i = 0; i < daysDiff; i++) {

		DateTime rateDate = start.AddDays(i);
		//decimal dailyInterestRate = InterestRateForDate(rateDate, monthlyInterestrate); // dr' = r/daysDiff
		decimal dailyInterestRate = InterestRateForDate(rateDate); // dr' = r/daysDiff
		decimal interest = openPrincipal * dailyInterestRate; //daily interest
		//totalInterest += interest;
		interestForPeriod += interest;

		Log.Info("rateDate: {0} interest: {1} totalInterest: {2} openPrincipal: {3}, i={4}", rateDate, interest, totalInterest, openPrincipal, i);
	}

	//	last event datetime
	//lastEventDate = currentEvent;

	return interestForPeriod;
}*/
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
				? AverageDailyInterestRate(currentDate, monthlyInterestRate, periodStartDate, periodEndDate)
				: interval.GetInterest(currentDate);

			if (interest != null)
				return (decimal)interest;
		} // if

		return AverageDailyInterestRate(currentDate, monthlyInterestRate, periodStartDate, periodEndDate);
		
	} */
		// GetDailyInterestRate


	} // class ALoanCalculator
} // namespace
