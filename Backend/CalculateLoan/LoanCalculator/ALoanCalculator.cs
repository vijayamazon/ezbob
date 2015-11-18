namespace Ezbob.Backend.CalculateLoan.LoanCalculator {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DbConstants;
	using Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions;
	using Ezbob.Backend.CalculateLoan.LoanCalculator.Methods;
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Logger;
	using Ezbob.Utils.Attributes;

	/// <summary>
	/// This class is implemented as Facade pattern. https://en.wikipedia.org/wiki/Facade_pattern
	/// </summary>
	public abstract class ALoanCalculator : ILoanCalculator {
		/// <exception cref="NoInitialDataException">Condition. </exception>
		/// <exception cref="NoLoanHistoryException">Condition. </exception>
		/// <exception cref="InvalidInitialAmountException">Condition. </exception>
		/// <exception cref="OverflowException">The result is outside the range of a <see cref="T:System.Decimal" />.</exception>
		/// <exception cref="InvalidInitialInterestRateException">Condition. </exception>
		protected ALoanCalculator(NL_Model model, DateTime? calculationDate = null) {

			if (model == null)
				throw new NoInitialDataException("nl model not found.");

			if (model.Loan == null)
				throw new NoInitialDataException("Loan in nl model not found.");

			if (model.Loan.LastHistory() == null)
				throw new NoLoanHistoryException();

			if (model.Loan.FirstHistory().EventTime == DateTime.MinValue)
				throw new NoInitialDataException("loan (history) creation date (EventTime) not found");

			if (model.Loan.LastHistory().InterestRate == 0)
				throw new InvalidInitialInterestRateException(model.Loan.LastHistory().InterestRate);

			if (Math.Round(model.Loan.LastHistory().Amount) == 0)
				throw new InvalidInitialAmountException(model.Loan.LastHistory().Amount);

			WorkingModel = model;
			WriteToLog = true;

			NowTime = DateTime.UtcNow;

			CalculationDate = calculationDate ?? NowTime;
			Log.Debug("calculationDate: {0}", CalculationDate);

			currentHistory = WorkingModel.Loan.LastHistory();

			InterestCalculationDateStart = WorkingModel.Loan.FirstHistory().EventTime;

			initialAmount = WorkingModel.Loan.FirstHistory().Amount;

			currentOpenPrincipal = initialAmount;

			BalanceBasedInterestCalculation = false;
		}

		public abstract string Name { get; }

		/// <summary>
		/// loan state date - input
		/// </summary>
		public DateTime CalculationDate { get; internal set; }

		protected static ASafeLog Log { get { return Library.Instance.Log; } }

		public bool WriteToLog { get; set; }

		public NL_Model WorkingModel { get; internal set; }

		/// <summary>
		/// true - use schedule balance for i' calculation in InterestBtwnEvents method; default - false, i.e. use real currentOpenPrincipal that depends on payments only 
		/// </summary>
		internal bool BalanceBasedInterestCalculation { get; set; }

		/// <summary>
		/// for autocharger-amount to charge at t date. How much should pay at t date.
		/// </summary>
		public decimal AmountToCharge { get; internal set; }
		
		public decimal TotalEarlyPayment { get; internal set; }
		
		public decimal NextEarlyPayment { get; internal set; }

		public decimal RolloverPayment { get; internal set; }

		/// <summary>
		/// for backworks compatibility with LoanRepaymentScheduleCalculator and "old" Loan model - sum of all AmountDue(s)
		/// </summary>
		public decimal Balance { get; internal set; }

		/// <summary>
		/// for customer dashboards - amount to be saved for NextEarlyPayment
		/// </summary>
		public decimal NextEarlyPaymentSavedAmount { get; internal set; }

		/// <summary>
		/// for customer dashboards - amount to be saved for TotalEarlyPayment
		/// </summary>
		public decimal TotalEarlyPaymentSavedAmount { get; internal set; }

		public DateTime NowTime { get; private set; }

		/// <summary>
		/// open fees at t'
		/// </summary>
		public decimal Fees { get; internal set; }

		/// <summary>
		/// open interest at t'
		/// </summary>
		public decimal Interest { get; internal set; }

		/// <summary>
		/// open principal at t'
		/// </summary>
		public decimal Principal { get; internal set; }

		//private LoanEvent calculationDateEventStart;
		internal LoanEvent calculationDateEventEnd;

		/// <summary>
		/// loan's events sequence
		/// </summary>
		internal List<LoanEvent> events;

		/// <summary>
		/// period for interest calculation
		/// </summary>
		internal DateTime InterestCalculationDateStart { get; set; }
		//protected DateTime InterestCalculationDateEnd { get; set; }

		/// <summary>
		/// helper, don't do assignment through it
		/// </summary>
		internal List<NL_LoanSchedules> schedule = new List<NL_LoanSchedules>();

		internal NL_LoanHistory currentHistory { get; set; }

		/// <summary>
		/// A = total initial principal
		/// </summary>
		public decimal initialAmount { get; internal set; }

		/// <summary>
		/// earned interest till current event (t') including
		/// </summary>
		public decimal currentEarnedInterest { get; internal set; }

		// Loan state public data

		/// <summary>
		/// open principal till current event (t') including
		/// </summary>
		public decimal currentOpenPrincipal { get; internal set; }

		// ### Loan state public data

		/// <summary>
		/// helper: holds principal paid till t' (event)
		/// </summary>
		public decimal currentPaidPrincipal { get; internal set; }
		/// <summary>
		/// helper: holds fees paid till t' (event)
		/// </summary>
		public decimal currentPaidFees { get; internal set; }

		/// <summary>
		/// holds interest paid till t' (event)
		/// </summary>
		public decimal currentPaidInterest { get; internal set; }

		/// <summary>
		/// helper: list of all late fees (not servicing/not arrangement) 
		/// </summary>
		public List<NL_LoanFees> lateFeesList = new List<NL_LoanFees>();

		public decimal totalLateFees { get; internal set; }

		/// <summary>
		/// helper: list of all distributed on time fees (servicing|arrangement) 
		/// </summary>
		internal List<NL_LoanFees> distributedFeesList = new List<NL_LoanFees>();

		/// <summary>
		/// hold current event
		/// </summary>
		internal LoanEvent lastEvent;

		// ### Loan State helpers

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
		/// <exception cref="NoScheduleException">Condition. </exception>
		/// <exception cref="NoInstallmentFoundException">Condition. </exception>
		/// <exception cref="OverflowException"><paramref /> represents a number that is less than <see cref="F:System.Decimal.MinValue" /> or greater than <see cref="F:System.Decimal.MaxValue" />. </exception>
		[ExcludeFromToString]
		public virtual void CreateSchedule() {
			new CreateScheduleMethod(this).Execute();
		} // CreateSchedule

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
		/// <exception cref="NoScheduleException">Condition. </exception>
		[ExcludeFromToString]
		public virtual double CalculateApr(DateTime? aprDate = null, double? calculationAccuracy = null, ulong? maxIterationCount = null) {
			var method = new CalculateAprMethod(this, aprDate);

			if (calculationAccuracy.HasValue)
				method.CalculationAccuracy = calculationAccuracy.Value;

			if (maxIterationCount.HasValue)
				method.MaxIterationLimit = maxIterationCount.Value;

			return method.Execute();
		} // CalculateApr


		[ExcludeFromToString]
		public DateTime AddRepaymentIntervals(int intervals, DateTime issuedTime, RepaymentIntervalTypes intervalType = RepaymentIntervalTypes.Month) {
			return (intervalType == RepaymentIntervalTypes.Month) ? issuedTime.AddMonths(intervals) : issuedTime.AddDays(intervals * (int)intervalType);
		} // AddRepaymentIntervals


		[ExcludeFromToString]
		public DateTime PreviousScheduleDate(DateTime theDate, RepaymentIntervalTypes intervalType = RepaymentIntervalTypes.Month) {
			return (intervalType == RepaymentIntervalTypes.Month) ? theDate.AddMonths(-1) : theDate.AddDays(-(int)intervalType);
		} // PreviousScheduleDate

		/// <summary>
		/// Calculates average daily interest rate (r'), based on monthlyInterestRate (r) and periodEndDate
		/// </summary>
		/// <param name="monthlyInterestRate"></param>
		/// <param name="periodEndDate"></param>
		/// <returns></returns>
		[ExcludeFromToString]
		public abstract decimal AverageDailyInterestRate(decimal monthlyInterestRate, DateTime? periodEndDate = null);

		/// <summary>
		/// Calculates interest rate for specific day based on loan data: 
		/// 1. Check freeze intervals
		/// 2. Find relevant schedule item
		/// 3. If schedule not found - use rate defined in the last history
		/// During changing customer's status to "bad", new freeze interest interval create, so "bad statuses" included to freezes
		/// </summary>
		/// <param name="theDate"></param>
		/// <param name="item"></param>
		/// <returns>Daily interest rate with freeze interest check.</returns>
		/// <exception cref="NoInstallmentFoundException">Condition. </exception>
		/// <exception cref="NoScheduleException">Condition. </exception>
		[ExcludeFromToString]
		internal decimal InterestRateForDate(DateTime theDate, NL_LoanSchedules item = null) {
			// relevant freeze interval
			var freezePeriod = WorkingModel.Loan.FreezeInterestIntervals.FirstOrDefault(fr => fr.StartDate >= theDate.Date && theDate.Date <= fr.EndDate && fr.ActivationDate <= theDate.Date);
			if (freezePeriod != null) {
				Log.Debug("freezePeriod: {0}, theDate: {1}", freezePeriod, theDate);
				return 0;
			}

			item = item ?? GetScheduleItemForDate(theDate);

			if (item == null)
				throw new NoInstallmentFoundException(theDate);

			// dr' = r/daysDiff or BankLike
			decimal dr = AverageDailyInterestRate(item.InterestRate, item.PlannedDate);

			//Log.Debug("InterestRateForDate: theDate: {0:d}, dailyRate {1:C4}, scheduleItem:{2}", theDate, dr, item.ToStringAsTable());

			return dr;
		}

		/// <summary>
		/// Find schedule item specific date is belongs to.
		/// </summary>
		/// <param name="theDate"></param>
		/// <returns></returns>
		/// <exception cref="NoScheduleException">Condition. </exception>
		[ExcludeFromToString]
		internal NL_LoanSchedules GetScheduleItemForDate(DateTime theDate) {
			if (this.schedule == null)
				throw new NoScheduleException();

			return this.schedule.FirstOrDefault(s => theDate <= s.PlannedDate);
		}

		/// <summary>
		/// Returns true if CalculationDate set to one of schedule items PlannedDate. Overwise false.
		/// </summary>
		/// <returns>bool</returns>
		/// <exception cref="NoScheduleException">Condition. </exception>
		[ExcludeFromToString]
		internal bool CalculationDateIsPlannedDate() {
			if (this.schedule == null)
				throw new NoScheduleException();

			return this.schedule.FirstOrDefault(s => CalculationDate.Date == s.PlannedDate.Date) == null;
		}

		/// <exception cref="NoScheduleException">Condition. </exception>
		internal bool HasLatesTillCalculationDate() {
			if (this.schedule == null)
				throw new NoScheduleException();

			return WorkingModel.Loan.LastHistory().Schedule.FirstOrDefault(s => s.PlannedDate.Date <= CalculationDate.Date && s.LoanScheduleStatusID == (int)NLScheduleStatuses.Late) == null;
		}

		/// <summary> 
		/// Interest rate for a specific date, specified in schedule item. If schedule exist, returns scheduled rate, otherwise returns histories' rate.
		/// common: Calculate total interest to pay at specific event date, during looping through ordered events list
		/// calculate interest rate btwn 2 nearby events in the list day by day; add interest calculated to currentEarnedInterest; set currentEvent date as lastEventDate
		/// </summary>
		/// <returns></returns>
		/// <param name="currentEvent"></param>
		/// <exception cref="NoInstallmentFoundException">Condition. </exception>
		/// <exception cref="NoScheduleException">Condition. </exception>
		/// <exception cref="OverflowException"><paramref /> represents a number that is less than <see cref="F:System.Decimal.MinValue" /> or greater than <see cref="F:System.Decimal.MaxValue" />. </exception>
		[ExcludeFromToString]
		internal decimal InterestBtwnEvents(LoanEvent currentEvent) {

			DateTime start = this.lastEvent == null ? InterestCalculationDateStart : this.lastEvent.EventTime;
			start = start.Date.AddDays(1);
			int daysDiff = currentEvent.EventTime.Subtract(start).Days;

			if (daysDiff <= 0)
				return 0;

			decimal interestForPeriod = 0;
			DateTime rateDate = start.Date;

			//Log.Debug("InterestBtwnEvents: start: {0}, currentEvent: {1}, days: {2}", start, currentEvent, daysDiff);

			//	interest for period = dr'(t)*p'(t)
			for (int i = 0; i <= daysDiff; i++) {

				decimal dailyInterestRate = InterestRateForDate(rateDate, currentEvent.ScheduleItem);	//	daily interest  dr' 

				NL_LoanSchedules item = null;
				if (BalanceBasedInterestCalculation) {
					item = currentEvent.ScheduleItem ?? GetScheduleItemForDate(rateDate);
				}
				// BalanceBasedInterestCalculation true used from CreateScheduleMethod
				decimal interest = ((BalanceBasedInterestCalculation && item != null) ? item.Balance : currentOpenPrincipal) * dailyInterestRate;

				interestForPeriod += interest;
				rateDate = start.AddDays(i);

				//Log.Info("InterestBtwnEventsaaa: --------------------rateDate: {0:d} interest: {1:F4} currentOpenPrincipal: {2:C2}, i={3}, dr={4:F6}", rateDate, interest, currentOpenPrincipal, i, Math.Round(interestForPeriod, 4));
			}

			Log.Debug("InterestBtwnEvents: start={0:d}, currentEvent={1:d}, days={2}, interestForPeriod={3:F4}", start, currentEvent, daysDiff, Math.Round(interestForPeriod, 4));

			return Math.Round(interestForPeriod, 4);
		}

		/// <summary>
		/// Initialize loan events list and common loan data vars
		/// </summary>
		[ExcludeFromToString]
		internal void IntiEvents() {

			// prevent duplicate initializations
			if (this.events != null)
				return;

			List<LoanEvent> historiesEvents = new List<LoanEvent>();
			List<LoanEvent> feesEvents = new List<LoanEvent>();
			List<LoanEvent> paymentEvents = new List<LoanEvent>();
			List<LoanEvent> schedulesEvents = new List<LoanEvent>();
			List<LoanEvent> actionEvents = new List<LoanEvent>();
			currentPaidInterest = 0;

			// histories => schedules
			foreach (var h in WorkingModel.Loan.Histories) {
				historiesEvents.Add(new LoanEvent(new DateTime(h.EventTime.Year, h.EventTime.Month, h.EventTime.Day), h));

				h.Schedule.ForEach(e => schedulesEvents.Add(new LoanEvent(new DateTime(e.PlannedDate.Year, e.PlannedDate.Month, e.PlannedDate.Day), e)));
				h.Schedule.ForEach(s => this.schedule.Add(s));
			}

			// fees
			foreach (var e in WorkingModel.Loan.Fees) {

				// ignore fees disabled before CalculationDate ???  TODO check
				if(e.DisabledTime != null && (e.DisabledTime.Value.Date <= CalculationDate.Date))
					continue;

				feesEvents.Add(new LoanEvent(new DateTime(e.AssignTime.Year, e.AssignTime.Month, e.AssignTime.Day), e));

				if (e.LoanFeeTypeID != (int)NLFeeTypes.ServicingFee && e.LoanFeeTypeID != (int)NLFeeTypes.ArrangementFee/* && (e.DisabledTime == null && e.DeletedByUserID == null)*/) {
					this.lateFeesList.Add(e);

					// init totalLateFees
					totalLateFees += e.Amount;
				}

				if ((e.LoanFeeTypeID == (int)NLFeeTypes.ServicingFee || e.LoanFeeTypeID == (int)NLFeeTypes.ArrangementFee)/* && (e.DisabledTime == null && e.DeletedByUserID == null)*/)
					this.distributedFeesList.Add(e);
			}

			// payments - by PaymentTime, only non-deleted
			foreach (var e in WorkingModel.Loan.Payments.Where(p=>p.DeletionTime==null && p.DeletedByUserID==null)) {

				paymentEvents.Add(new LoanEvent(new DateTime(e.PaymentTime.Year, e.PaymentTime.Month, e.PaymentTime.Day), e));

				// init currentPaidInterest
				foreach (NL_LoanSchedulePayments sp in e.SchedulePayments) {
					currentPaidInterest += sp.InterestPaid;
				}
				// init currentPaidFees
				foreach (NL_LoanFeePayments fp in e.FeePayments) {
					currentPaidFees += fp.Amount;
				}
			}

			if (this.calculationDateEventEnd != null)
				actionEvents.Add(this.calculationDateEventEnd);

			// combine ordered events list
			this.events = historiesEvents.Union(schedulesEvents)
				.Union(paymentEvents)
				.Union(feesEvents)
				.Union(actionEvents)
				// .Union(rollOverEvents) TODO add rolovers // .Union(reschedules) TODO add reschedules
				//.Union(new[] {this.calculationDateEventStart, this.calculationDateEventEnd })
				//	.Union(new[] { this.calculationDateEventEnd })
				.OrderBy(e => e.EventTime).ThenBy(e => e.Priority).ToList();

			// actually nulls should not exists here
			this.events.RemoveAll(e => e == null);

			this.events.ForEach(e => Log.Debug(e));
		}

		/// <exception cref="NoLoanEventsException">Condition. </exception>
		/// <exception cref="NoScheduleException">Condition. </exception>
		/// <exception cref="InvalidCastException"><paramref /> cannot be cast to the element type of the current <see cref="T:System.Array" />.</exception>
		/// <exception cref="NoInstallmentFoundException">Condition. </exception>
		/// <exception cref="OverflowException">The sum is larger than <see cref="F:System.Decimal.MaxValue" />.</exception>
		/// <exception cref="Exception">A delegate callback throws an exception. </exception>
		[ExcludeFromToString]
		internal virtual void HandleEvents() {

			IntiEvents();

			if (this.events == null || this.events.Count == 0)
				throw new NoLoanEventsException();

			currentOpenPrincipal = initialAmount;

			foreach (var e in this.events) {

				e.CurrentHistory = currentHistory;

				e.EarnedInterestForPeriod = InterestBtwnEvents(e);
				currentEarnedInterest += e.EarnedInterestForPeriod;

				this.lastEvent = e;

				Log.Debug("HandleEvents: event: {0} lastEvent: {1}, EarnedInterestForPeriod={2}", e, this.lastEvent, e.EarnedInterestForPeriod);

				switch (e.GetTypeString()) {

				case "History":
					currentHistory = e.History;
					break;

				case "ScheduleItem":

					ProcessScheduleItem(e.ScheduleItem);
					// TODO calculate saved amounts
					// saved amount for "next early" and "total early" payments
					/*if (savedAmount && CalculationDate != DateTime.MinValue && CalculationDate.Date <= e.EventTime.Date) {

						decimal earnedinterestTillCalculationDate = this.events.Where(ev => ev.EventTime <= this.calculationDateEventEnd.EventTime)
							.Sum(ev => ev.EarnedInterestForPeriod);

						var nextEarlyPaymentSchedule = this.schedule.FirstOrDefault(s => CalculationDate.Date >= s.PlannedDate);

						if (nextEarlyPaymentSchedule != null && (nextEarlyPaymentSchedule.PlannedDate.Date == e.ScheduleItem.PlannedDate))
							NextEarlyPaymentSavedAmount = currentEarnedInterest - earnedinterestTillCalculationDate;

						var totalEarlyPaymentSchedule = this.schedule.LastOrDefault();

						if (totalEarlyPaymentSchedule != null && (totalEarlyPaymentSchedule.PlannedDate.Date == e.ScheduleItem.PlannedDate))
							TotalEarlyPaymentSavedAmount = currentEarnedInterest - earnedinterestTillCalculationDate;
					}*/
					break;

				case "Fee":
					break;

				case "Payment":
					ProcessPayment(e.Payment);
					break;

				case "Rollover":
					//HandleRolloverEvent(e.Rollover);
					break;

				case "Action":
					e.Action(); // execute the action defined
					break;

				default:
					Log.Debug("Unknown event type: {0}", e);
					break;
				}
			}

			// finalize schedules statuses after processing of all events (maily payments)
			foreach (LoanEvent e in this.events.Where(e => e.ScheduleItem != null)) {
				UpdateScheduleItem(e.ScheduleItem);
			}

			// close loan? // TODO when loan is closed?
			if (Principal == 0m && Fees == 0m && Interest == 0m) {
				WorkingModel.Loan.LoanStatusID = (int)NLLoanStatuses.PaidOff;
				WorkingModel.Loan.DateClosed = CalculationDate;
			}

			// AmountDue
			this.lastEvent = null;
			BalanceBasedAmountDue(initialAmount);
		}
		

		/// set SCHEDULED (balance based) interest and amount due for each non-closed installment
		/// <exception cref="NoInstallmentFoundException">Condition. </exception>
		/// <exception cref="NoScheduleException">Condition. </exception>
		/// <exception cref="OverflowException"><paramref /> represents a number that is less than <see cref="F:System.Decimal.MinValue" /> or greater than <see cref="F:System.Decimal.MaxValue" />. </exception>
		internal void BalanceBasedAmountDue(decimal balance) {
			BalanceBasedInterestCalculation = true;
			foreach (var s in this.events.Where(e => e.ScheduleItem != null)) {
				if (s.ScheduleItem.LoanScheduleStatusID != (int)NLScheduleStatuses.Paid) {
					s.ScheduleItem.Balance = balance;
					s.ScheduleItem.Interest = InterestBtwnEvents(s);
					balance -= s.ScheduleItem.Principal;
					s.ScheduleItem.Balance = balance;
					s.ScheduleItem.AmountDue = s.ScheduleItem.OpenPrincipal + s.ScheduleItem.Interest + s.ScheduleItem.Fees;
					this.lastEvent = s;
				}
			}
			// set interest calculation mode back to rela open principal 
			BalanceBasedInterestCalculation = false;
		}
		
		/// <summary>
		/// 1. pay fees 
		/// 2. pay schedule (interest and principal)
		/// 3. pay rollover TODO phase 2
		/// </summary>
		/// <param name="payment"></param>
		/// <exception cref="ArgumentNullException"><paramref /> or <paramref /> is null.</exception>
		/// <exception cref="OverflowException">The sum is larger than <see cref="F:System.Decimal.MaxValue" />.</exception>
		/// <exception cref="InvalidCastException"><paramref /> cannot be cast to the element type of the current <see cref="T:System.Array" />.</exception>
		[ExcludeFromToString]
		internal void ProcessPayment(NL_Payments payment) {

			// money that can be distributed to pay loan
			decimal assignAmount = payment.Amount - payment.FeePayments.Sum(p => p.Amount) - payment.SchedulePayments.Sum(p => p.InterestPaid) - payment.SchedulePayments.Sum(p => p.PrincipalPaid);

			// 1. pay fees
			assignAmount = PayFees(payment, assignAmount);

			Log.Debug("ProcessPayment: payment balance after PayFees = {0}", assignAmount);

			// 2. pay interest & principal
			assignAmount = PaySchedules(payment, assignAmount);

			Log.Debug("ProcessPayment: payment balance after PaySchedules = {0}", assignAmount);
		}

		/// <summary>
		/// Assign current payment to loan schedules. 
		/// TODO: "pay rollover" should be after "pay interest" 
		/// 1. Record new NL_LoanSchedulePayments entries to payment.SchedulePayments
		/// 2. cumulate paid principal into currentPaidPrincipal
		/// 3. update currentOpenPrincipal (p'(t) = A - sum(p(t')) where p'(t) - current open principal; A - initial amount taken, paid(t') - principal paid at this event; sum(p(t')) - principal paid untill this event 
		/// </summary>
		/// <param name="payment"></param>
		/// <param name="assignAmount"></param>
		/// <returns></returns>
		/// <exception cref="OverflowException">The result is outside the range of a <see cref="T:System.Decimal" />.</exception>
		/// <exception cref="InvalidCastException"><paramref /> cannot be cast to the element type of the current <see cref="T:System.Array" />.</exception>
		[ExcludeFromToString]
		internal decimal PaySchedules(NL_Payments payment, decimal assignAmount) {

			if (Math.Round(assignAmount) <= 0) {
				Log.Info("PaySchedules: No amount to assign: {0}", assignAmount);
				return assignAmount;
			}

			if (payment == null) {
				Log.Info("PaySchedules: No payment defined");
				return assignAmount;
			}

			// sch(planned-paid > 0)
			IEnumerable<NL_LoanSchedules> unpaidSchedules = from s in this.schedule
															join p in payment.SchedulePayments on s.LoanScheduleID equals p.LoanScheduleID into g
															from x in g.DefaultIfEmpty()
															where x == null || s.Principal < x.PrincipalPaid // not paid at all or partially paid
															orderby s.PlannedDate
															select s;
			// Record schedules payments

			// loop throught unpaid and partial paid schedules
			foreach (NL_LoanSchedules s in unpaidSchedules) {

				if (Math.Round(assignAmount) <= 0m) {
					Log.Debug("PaySchedules: amount assigned completely");
					return 0;
				}

				Log.Debug("PaySchedules: currentEarnedInterest: {0}, currentPaidInterest: {1},  assignAmount: {2}, ScheduleToPay\n{3}{4}",
					currentEarnedInterest, currentPaidInterest, assignAmount, AStringable.PrintHeadersLine(typeof(NL_LoanSchedules)), s.ToStringAsTable());

				decimal iAmount = Math.Min(assignAmount, (currentEarnedInterest - currentPaidInterest));

				assignAmount -= iAmount;

				decimal pPaidAmount = 0;
				foreach (var p in WorkingModel.Loan.Payments)
					pPaidAmount += p.SchedulePayments.Where(sp => sp.LoanScheduleID == s.LoanScheduleID).Sum(sp => sp.PrincipalPaid);

				// balance of principal p'
				decimal pAmount = Math.Min(assignAmount, (s.Principal - pPaidAmount));

				assignAmount -= pAmount;

				// new schedule payments
				if (Math.Round(iAmount) > 0 || Math.Round(pAmount) > 0) {

					NL_LoanSchedulePayments schpayment = new NL_LoanSchedulePayments() {
						InterestPaid = iAmount,
						LoanScheduleID = s.LoanScheduleID,
						PaymentID = payment.PaymentID,
						PaymentDate = payment.PaymentTime,
						PrincipalPaid = pAmount,
						//NewEntry = true
					};

					payment.SchedulePayments.Add(schpayment);

					// cumulate interest paid on the payment to currentPaidInterest
					currentPaidInterest += iAmount;
					//currentPaidInterest = decimal.Round(currentPaidInterest, 4); 

					// cumulate principal paid on the payment to currentPaidPrincipal
					currentPaidPrincipal += pAmount;
					//currentPaidPrincipal = decimal.Round(currentPaidPrincipal, 4); 

					// update currentOpenPrincipal (principal to pay)
					currentOpenPrincipal = initialAmount - currentPaidPrincipal;

					Log.Debug("PaySchedules: added new LoanSchedulePayment\n {0}{1}", AStringable.PrintHeadersLine(typeof(NL_LoanSchedulePayments)), schpayment.ToStringAsTable());
				}

			} // foreach

			return decimal.Round(assignAmount, 4);
		}

		/// <summary>
		/// 1. pay late fees
		/// 2. pay distributed fees that should be paid till event date
		/// </summary>
		/// <param name="payment"></param>
		/// <param name="assignAmount"></param>
		/// <returns>rest of available money of the payment</returns>
		/// <exception cref="OverflowException">The sum is larger than <see cref="F:System.Decimal.MaxValue" />.</exception>
		/// <exception cref="InvalidCastException"><paramref /> cannot be cast to the element type of the current <see cref="T:System.Array" />.</exception>
		[ExcludeFromToString]
		internal decimal PayFees(NL_Payments payment, decimal assignAmount) {

			if (Math.Round(assignAmount) == 0) {
				Log.Info("PayFees: No amount to assign: {0}", assignAmount);
				return assignAmount;
			}

			if (payment == null) {
				Log.Info("PayFees: No payment defined");
				return assignAmount;
			}

			// all unpaid late fees (not setup|servicing|arrangement), not related to calculation date
			assignAmount = AddFeesPayments(this.lateFeesList, payment, assignAmount);

			// unpaid servicing|arrangement untill LastEventDate including
			var distributedfeesUntillThisEvent = this.distributedFeesList.Where(f => f.AssignTime <= payment.PaymentTime);

			assignAmount = AddFeesPayments(distributedfeesUntillThisEvent.ToList(), payment, assignAmount);

			return assignAmount;
		}

		/// <summary>
		/// 1. Record new NL_LoanFeePayments entry(s) to payment
		/// 2. Cumulate paid fees into currentPaidFees
		/// <exception cref="ArgumentNullException"><paramref /> or <paramref /> is null.</exception>
		/// <exception cref="OverflowException">The sum is larger than <see cref="F:System.Decimal.MaxValue" />.</exception>
		/// </summary>
		/// <exception cref="InvalidCastException"><paramref /> cannot be cast to the element type of the current <see cref="T:System.Array" />.</exception>
		[ExcludeFromToString]
		internal decimal AddFeesPayments(List<NL_LoanFees> feesList, NL_Payments payment, decimal assignAmount) {

			if (feesList.Count == 0) {
				Log.Info("AddFeesPayments: No fees in list on payment {0}", payment.BaseString());
				return assignAmount;
			}
			if (payment == null) {
				Log.Info("AddFeesPayments: No payment defined");
				return assignAmount;
			}

			if (Math.Round(assignAmount) <= 0) {
				Log.Info("AddFeesPayments1: Amount assigned: {0}", assignAmount);
				return assignAmount;
			}

			foreach (NL_LoanFees f in feesList) {
				if (Math.Round(assignAmount) <= 0) {
					Log.Info("AddFeesPayments: Amount assigned: {0}", assignAmount);
					return assignAmount;
				}

				// how much paid for the fee
				decimal fPaid = 0m;
				foreach (var p in WorkingModel.Loan.Payments)
					fPaid += p.FeePayments.Where(fp => fp.LoanFeeID == f.LoanFeeID).Sum(fp => fp.Amount);

				decimal fBalance = f.Amount - fPaid;

				// fees not paid completely
				if (Math.Round(fBalance) > 0) {

					decimal fAmount = Math.Min(assignAmount, fBalance);

					NL_LoanFeePayments fpayment = new NL_LoanFeePayments() {
						Amount = fAmount,			// maximal possible money to pay the fee
						LoanFeeID = f.LoanFeeID,
						LoanFeePaymentID = payment.PaymentID,
						PaymentID = payment.PaymentID,
						//NewEntry = true
					};

					payment.FeePayments.Add(fpayment);

					// decrease paid amount from available payment amount
					assignAmount -= fAmount;

					//	cumulate fees paid on the payment to currentPaidFees
					currentPaidFees += fAmount;

					Log.Debug("AddFeesPayments: feePayment recorded for fee {0}{1}: {2}{3}", AStringable.PrintHeadersLine(typeof(NL_LoanFees)), f.ToStringAsTable(), AStringable.PrintHeadersLine(typeof(NL_LoanFeePayments)), fpayment.ToStringAsTable());
				}
			}
			return assignAmount;
		}

		/// <summary>
		/// Returns loan state and outstanding balance at CalculationDate (t'): F, I, P; schedules, fees and payments updated data
		/// </summary>
		/// <exception cref="NoLoanEventsException">Condition. </exception>
		/// <exception cref="NoScheduleException">Condition. </exception>
		/// <exception cref="InvalidCastException"><paramref /> cannot be cast to the element type of the current <see cref="T:System.Array" />.</exception>
		/// <exception cref="NoInstallmentFoundException">Condition. </exception>
		/// <exception cref="OverflowException">The sum is larger than <see cref="F:System.Decimal.MaxValue" />.</exception>
		/// <exception cref="Exception">A delegate callback throws an exception. </exception>
		/// <exception cref="LoanPaidOffStatusException">Condition. </exception>
		/// <exception cref="LoanWriteOffStatusException">Condition. </exception>
		/// <exception cref="LoanPendingStatusException">Condition. </exception>
		[ExcludeFromToString]
		public void GetState() {

			this.calculationDateEventEnd = new LoanEvent(CalculationDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59));

			IntiEvents();

			decimal earnedInterestForCalculationDate = 0;

			// fetch the state at this event 
			this.calculationDateEventEnd.Action = () => {

				earnedInterestForCalculationDate = currentEarnedInterest;

				// all local var calculated untill this event included
				//decimal tDistributedFees = 0;
				//foreach (NL_LoanFees f in this.distributedFeesList.Where(f => f.AssignTime.Date <= this.calculationDateEventEnd.EventTime.Date))
				//	tDistributedFees += f.Amount;

				decimal tDistributedFees = this.distributedFeesList.Where(f => f.AssignTime.Date <= this.calculationDateEventEnd.EventTime.Date).Sum(f => f.Amount);

				// outstanding balance = Fees + Interest + Principal
				Fees = (totalLateFees + tDistributedFees - currentPaidFees);
				Interest = (currentEarnedInterest - currentPaidInterest);
				Principal = currentOpenPrincipal;

				TotalEarlyPayment = Fees + Interest + Principal;

				Log.Debug("GetState Action: Fees={0}, Interest={1}, Principal={2}, TotalEarlyPayment={3:F4}, RolloverPayment={4:F4}", Fees, Interest, Principal, TotalEarlyPayment, RolloverPayment);
			};

			HandleEvents();

			CheckLoanClosed();

			// AmountDue - in each schedule

			// calc date schedule item
			NL_LoanSchedules calcDateItem = GetScheduleItemForDate(CalculationDate);

			Log.Debug("calcDateItem: \n{0}{1}", AStringable.PrintHeadersLine(typeof(NL_LoanSchedules)), calcDateItem);

			NextEarlyPayment = calcDateItem.OpenPrincipal + Interest + Fees;

			RolloverPayment = Interest; // to display separated: 1. rollover fee (from cong.varilables); 2. late fees till "rolover opportunity" (calc.date) == (open Fees)

			// set outstanding balance - if pay as scheduled, this is the total amount due for calculation date
			WorkingModel.Loan.Histories.ForEach(h => h.Schedule.ForEach(s => Balance += s.AmountDue));
		
			// All previous installments are paid? i.e. check late
			if (HasLatesTillCalculationDate()) {
				return;
			}

			// Today is a payment day
			if (CalculationDateIsPlannedDate()) {
				return;
			}

			NextEarlyPaymentSavedAmount = earnedInterestForCalculationDate - calcDateItem.Interest;

			TotalEarlyPayment = currentEarnedInterest - calcDateItem.Interest; // TODO check???
		}


		/// <summary>
		/// throw appropriate error if loan not started (pending) or closed (paid, writeoff)
		/// </summary>
		/// <exception cref="LoanPaidOffStatusException">Condition. </exception>
		/// <exception cref="LoanWriteOffStatusException">Condition. </exception>
		/// <exception cref="LoanPendingStatusException">Condition. </exception>
		[ExcludeFromToString]
		internal void CheckLoanClosed() {
			// loan paid off, no need to pay more
			if (WorkingModel.Loan.LoanStatusID == (int)NLLoanStatuses.PaidOff)
				throw new LoanPaidOffStatusException(WorkingModel.Loan.LoanID);

			// loan writed off, no need to pay more
			if (WorkingModel.Loan.LoanStatusID == (int)NLLoanStatuses.WriteOff)
				throw new LoanWriteOffStatusException(WorkingModel.Loan.LoanID);

			// loan is pending, no need to pay yet
			if (WorkingModel.Loan.LoanStatusID == (int)NLLoanStatuses.Pending)
				throw new LoanPendingStatusException(WorkingModel.Loan.LoanID);
		}

		/// <summary>
		/// calculates and sets "AmountDue" per item (p, i, f) during events processing (related to t' earned interest, open principal etc.)
		/// </summary>
		/// <param name="item"></param>
		/// <exception cref="InvalidCastException"><paramref /> cannot be cast to the element type of the current <see cref="T:System.Array" />.</exception>
		/// <exception cref="OverflowException">The sum is larger than <see cref="F:System.Decimal.MaxValue" />.</exception>
		[ExcludeFromToString]
		internal void ProcessScheduleItem(NL_LoanSchedules item) {

			ScheduleItemOutstandingData(item);

			// i'- current open interest, payments based (== real open principal)
			item.InterestOutstandingPrincipalBased = currentEarnedInterest - currentPaidInterest;

			item.AmountDueOutstandingPrincipalBased = item.Fees + item.InterestOutstandingPrincipalBased + item.OpenPrincipal;

			Log.Info("ProcessScheduleItem: \n{0}{1}", AStringable.PrintHeadersLine(typeof(NL_LoanSchedules)), item.ToStringAsTable());
		}

		/// <summary>
		/// calculates and sets "AmountDue" per item (p, i, f) after events processing and close item if possible
		/// </summary>
		/// <param name="item"></param>
		/// <exception cref="InvalidCastException"><paramref /> cannot be cast to the element type of the current <see cref="T:System.Array" />.</exception>
		/// <exception cref="OverflowException">The sum is larger than <see cref="F:System.Decimal.MaxValue" />.</exception>
		[ExcludeFromToString]
		internal void UpdateScheduleItem(NL_LoanSchedules item) {

			ScheduleItemOutstandingData(item);

			var closeItem = (item.OpenPrincipal == 0 && item.Fees == 0);

			// if principal and relevant fees completely paid, close
			if (closeItem) {
				item.LoanScheduleStatusID = (int)NLScheduleStatuses.Paid;
				item.ClosedTime = NowTime;
				item.AmountDue = 0;
				Log.Info("Schedule item marked as 'Paid' at {0}:\n{1}{2} ", NowTime, AStringable.PrintHeadersLine(typeof(NL_LoanSchedules)), item.ToStringAsTable());
			} else {
				// payment cancelled
				item.LoanScheduleStatusID = (int)NLScheduleStatuses.StillToPay;
				item.ClosedTime = null;
			}
		}

		/// <summary>
		/// Called from within events loop
		/// counts distributed fees should be paid and really paid for this schedule item at this t'
		/// counts late fees should be paid and really paid for this schedule item at this t'
		/// counts principal should be paid and really paid for this schedule item at this t'
		/// </summary>
		/// <param name="item"></param>
		/// <exception cref="ArgumentNullException"><paramref /> or <paramref /> is null.</exception>
		/// <exception cref="OverflowException">The sum is larger than <see cref="F:System.Decimal.MaxValue" />.</exception>
		[ExcludeFromToString]
		internal void ScheduleItemOutstandingData(NL_LoanSchedules item) {

			decimal principalPaid = 0;
			decimal distributedFeesPaid = 0;
			decimal lateFeesPaid = 0;

			var distributedFee = this.distributedFeesList.FirstOrDefault(f => f.AssignTime == item.PlannedDate);

			foreach (NL_Payments p in WorkingModel.Loan.Payments) {
				// paid for item principal
				principalPaid += p.SchedulePayments.Where(sp => sp.LoanScheduleID == item.LoanScheduleID).Sum(sp => sp.PrincipalPaid);

				// paid for item distributed fee
				if (distributedFee != null) {
					distributedFeesPaid += p.FeePayments.Where(fp => fp.LoanFeeID == distributedFee.LoanID).Sum(fp => fp.Amount);
				}

				// all "late fees" pays
				lateFeesPaid += p.FeePayments.Sum(fp => fp.Amount);
			}

			// current open principal for p'
			item.OpenPrincipal = item.Principal - principalPaid;
			// current open distributed fees
			item.Fees = distributedFee == null ? 0 : (distributedFee.Amount - distributedFeesPaid);

			// attached all unpaid late fees to first (nearby) not paid installment
			var lateFeesSchedule = WorkingModel.Loan.LastHistory().Schedule.FirstOrDefault(s => s.LateFeesAttached);

			// attach late fees
			if (lateFeesSchedule == null && (totalLateFees - lateFeesPaid) > 0) {
				// current open fees total (late+distributed)
				item.Fees += (totalLateFees - lateFeesPaid);
				item.LateFeesAttached = true;
			}
		}


		public override string ToString() {
			StringBuilder sb = new StringBuilder().Append("Calculator data:").Append(Environment.NewLine);
			var props = AStringable.FilterPrintable(typeof(ALoanCalculator));
			foreach (var p in props) {
				sb.Append(p.Name).Append(":\t").Append(p.GetValue(this)).Append(Environment.NewLine);
			}
			return sb.ToString();
		}

	} // class ALoanCalculator
} // namespace
