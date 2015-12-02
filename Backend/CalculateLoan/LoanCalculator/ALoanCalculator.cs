namespace Ezbob.Backend.CalculateLoan.LoanCalculator {
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Linq;
	using System.Text;
	using DbConstants;
	using Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions;
	using Ezbob.Backend.CalculateLoan.LoanCalculator.Methods;
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Logger;
	using Ezbob.Utils.Attributes;
	using Ezbob.Utils.Extensions;

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

			if (decimal.Round(model.Loan.LastHistory().Amount) == 0)
				throw new InvalidInitialAmountException(model.Loan.LastHistory().Amount);

			WorkingModel = model;
			WriteToLog = true;

			NowTime = DateTime.UtcNow;

			CalculationDate = calculationDate ?? NowTime;

			currentHistory = WorkingModel.Loan.LastHistory();

			InterestCalculationDateStart = WorkingModel.Loan.FirstHistory().EventTime;

			initialAmount = WorkingModel.Loan.FirstHistory().Amount;

			currentOpenPrincipal = initialAmount;

			BalanceBasedInterestCalculation = false;
		}

		internal int decimalAccurancy = 6;

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
		//public decimal AmountToCharge { get; internal set; }



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

		public DateTime NowTime { get; private set; }

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

		/// <exception cref="InvalidEnumArgumentException">Condition. </exception>
		/// <exception cref="OverflowException">Condition. </exception>
		[ExcludeFromToString]
		public DateTime AddRepaymentIntervals(int intervals, DateTime issuedTime, RepaymentIntervalTypes intervalType = RepaymentIntervalTypes.Month) {
			try {
				return (intervalType == RepaymentIntervalTypes.Month) ? issuedTime.AddMonths(intervals) : issuedTime.AddDays(intervals * Convert.ToInt32(intervalType.DescriptionAttr()));
			} catch (InvalidEnumArgumentException enumArgumentException) {
				throw new InvalidEnumArgumentException(string.Format("intervalType {0} days number not found", intervalType), enumArgumentException);
			} catch (OverflowException overflowException) {
				throw new OverflowException(string.Format("intervalType {0} days number not found", intervalType), overflowException);
			}
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
		[ExcludeFromToString]
		internal decimal InterestBtwnEvents(LoanEvent currentEvent) {

			DateTime start = this.lastEvent == null ? InterestCalculationDateStart : this.lastEvent.EventTime;
			start = start.Date.AddDays(1);
			int daysDiff = currentEvent.EventTime.Subtract(start).Days;

			if (daysDiff <= 0)
				return 0;

			decimal interestForPeriod = 0;
			DateTime rateDate = start.Date;
			decimal principalBase = 0;

			//Log.Debug("InterestBtwnEvents: start: {0}, currentEvent: {1}, days: {2}", start, currentEvent, daysDiff);

			//	interest for period = dr'(t)*p'(t)
			for (int i = 0; i <= daysDiff; i++) {

				NL_LoanSchedules item = currentEvent.ScheduleItem ?? GetScheduleItemForDate(rateDate);

				decimal dailyInterestRate = InterestRateForDate(rateDate, item);	//	daily interest  dr' 

				// BalanceBasedInterestCalculation true used from CreateScheduleMethod
				principalBase = ((BalanceBasedInterestCalculation && item != null) ? item.Balance : currentOpenPrincipal);
				decimal interest = principalBase * dailyInterestRate; //((BalanceBasedInterestCalculation && item != null) ? item.Balance : currentOpenPrincipal) * dailyInterestRate;

				interestForPeriod += interest;
				rateDate = start.AddDays(i);

				//Log.Info("InterestBtwnEventsaaa: --------------------rateDate: {0:d} interest: {1:F4} currentOpenPrincipal: {2:C2}, i={3}, dr={4:F6}", rateDate, interest, currentOpenPrincipal, i, interestForPeriod);
			}

			Log.Debug("InterestBtwnEvents: start={0:s}, currentEvent={1:s}, days={2}, interestForPeriod={3:F4}, open principal/balance={4:F6}", start.Date, currentEvent.EventTime.Date, daysDiff, interestForPeriod, principalBase);

			return decimal.Round(interestForPeriod, this.decimalAccurancy);
		}

		/// <summary>
		/// Initialize loan events and common loan data vars
		/// </summary>
		[ExcludeFromToString]
		internal void IntiEvents() {

			//prevent duplicate initializations
			if (this.events != null)
				return;

			List<LoanEvent> historiesEvents = new List<LoanEvent>();
			List<LoanEvent> feesEvents = new List<LoanEvent>();
			List<LoanEvent> paymentEvents = new List<LoanEvent>();
			List<LoanEvent> schedulesEvents = new List<LoanEvent>();
			List<LoanEvent> acceptedRolloverEvents = new List<LoanEvent>();
			List<LoanEvent> actionEvents = new List<LoanEvent>();

			// histories => schedules
			foreach (var h in WorkingModel.Loan.Histories) {
				historiesEvents.Add(new LoanEvent(new DateTime(h.EventTime.Year, h.EventTime.Month, h.EventTime.Day), h));
				foreach (NL_LoanSchedules e in h.Schedule.Where(s => s.LoanScheduleStatusID != (int)NLScheduleStatuses.DeletedOnReschedule && s.LoanScheduleStatusID != (int)NLScheduleStatuses.ClosedOnReschedule)) {
					schedulesEvents.Add(new LoanEvent(new DateTime(e.PlannedDate.Year, e.PlannedDate.Month, e.PlannedDate.Day), e));
					this.schedule.Add(e);
				}
			}

			// fees
			foreach (var e in WorkingModel.Loan.Fees.OrderBy(f => f.AssignTime)) {

				// ignore fees disabled before CalculationDate ???  TODO check
				//if (e.DisabledTime != null && (e.DisabledTime.Value.Date <= CalculationDate.Date))
				//	continue;

				feesEvents.Add(new LoanEvent(new DateTime(e.AssignTime.Year, e.AssignTime.Month, e.AssignTime.Day), e));

				if ((e.LoanFeeTypeID == (int)NLFeeTypes.ServicingFee || e.LoanFeeTypeID == (int)NLFeeTypes.ArrangementFee) /* && (e.DisabledTime == null && e.DeletedByUserID == null)*/) {
					this.distributedFeesList.Add(e);
				} else {
					this.lateFeesList.Add(e);

					// cumulate totalLateFees
					totalLateFees += e.Amount;
				}
			}

			// payments - by PaymentTime, only non-deleted
			foreach (NL_Payments p in WorkingModel.Loan.Payments.OrderBy(p => p.PaymentTime)) {

				// init currentPaidInterest, currentPaidPrincipal
				foreach (NL_LoanSchedulePayments sp in p.SchedulePayments) {
					currentPaidInterest += sp.InterestPaid;
					currentPaidPrincipal += sp.PrincipalPaid;
				}

				// init currentPaidFees
				foreach (NL_LoanFeePayments fp in p.FeePayments)
					currentPaidFees += fp.Amount;

				// charge-back date
				if (p.PaymentStatusID == (int)NLPaymentStatuses.ChargeBack && !p.DeletionTime.Equals(DateTime.MinValue) && p.DeletionTime != null) {
					// prevent from registering real schedule payments or fee payments
					p.Amount = 0;

					// on this point principal paid by the payment, should be removed from currently open principal, i.e. "ha-kesef shakhaf etzlenu"
					paymentEvents.Add(item: new LoanEvent(new DateTime(p.PaymentTime.Year, p.PaymentTime.Month, p.PaymentTime.Day), payment: p, priority: 0, chargeBackPayment: true, chargeBackPaymentRecorded: true));

					DateTime chargeBackTime = (DateTime)p.DeletionTime;

					// on this point principal paid by the payment, should be added to the currently open principal
					paymentEvents.Add(item: new LoanEvent(new DateTime(chargeBackTime.Year, chargeBackTime.Month, chargeBackTime.Day), payment: p, priority: 0, chargeBackPayment: true, chargeBackPaymentRecorded: false));
				} else
					paymentEvents.Add(new LoanEvent(new DateTime(p.PaymentTime.Year, p.PaymentTime.Month, p.PaymentTime.Day), p));
			}

			// accepted rollovers
			List<NL_LoanRollovers> acceptedRollovers = WorkingModel.Loan.AcceptedRollovers.Where(e => e.IsAccepted).ToList();
			if (acceptedRollovers.Count > 0)
				acceptedRollovers.ForEach(e => acceptedRolloverEvents.Add(new LoanEvent(new DateTime(e.CustomerActionTime.Value.Year, e.CustomerActionTime.Value.Month, e.CustomerActionTime.Value.Day), e)));

			if (this.calculationDateEventEnd != null)
				actionEvents.Add(this.calculationDateEventEnd);

			// combine ordered events list
			this.events = historiesEvents.Union(schedulesEvents)
				.Union(paymentEvents)
				.Union(feesEvents)
				.Union(actionEvents)
				.Union(acceptedRolloverEvents)
				//  .Union(reschedules) TODO add reschedules
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

				Log.Debug("HandleEvents: {0} EarnedInterestForPeriod={1} OpenPrincipal={2}", e, e.EarnedInterestForPeriod, currentOpenPrincipal);

				this.lastEvent = e;

				switch (e.GetTypeString()) {

				case "History":
					currentHistory = e.History;
					break;

				case "ScheduleItem":
					ProcessScheduleItem(e.ScheduleItem);
					break;

				case "Fee":
					break;

				case "Payment":
					ProcessPayment(e.Payment);
					break;

				case "ChargebackPaymentRecorded":
					// remove paid principal to currentOpenPrincipal, i.e. decrease open principal for interest caclulation
					var paidPrincipalR = e.ChargebackPaymentRecorded.SchedulePayments.Sum(sp => sp.ResetPrincipalPaid);
					if (paidPrincipalR != null) {
						currentOpenPrincipal -= (decimal)paidPrincipalR;
					}
					break;

				case "ChargebackPaymentCancelled":
					// add paid principal to currentOpenPrincipal, i.e. add unpaid principal open principal
					var paidPrincipal = e.ChargebackPaymentCancelled.SchedulePayments.Sum(sp => sp.ResetPrincipalPaid);
					if (paidPrincipal != null) {
						currentOpenPrincipal += (decimal)paidPrincipal;
					}
					break;

				case "Rollover":
					break;

				case "Action":
					e.Action(); // execute the action defined
					break;

				default:
					Log.Debug("Unknown event type: {0}", e);
					break;
				}

				Log.Debug("------------------------OpenPrincipal: {0}", currentOpenPrincipal);

			}

			// finalize schedules statuses/AmountDue after processing of all events (mainly payments)
			foreach (LoanEvent e in this.events.Where(e => e.ScheduleItem != null)) {
				UpdateScheduleItem(e.ScheduleItem);
			}

			// close loan? // TODO when loan is closed?
			if (Principal == 0m && Fees == 0m && Interest == 0m) {
				WorkingModel.Loan.LoanStatusID = (int)NLLoanStatuses.PaidOff;
				WorkingModel.Loan.DateClosed = CalculationDate;
			}

			// balance 
			this.lastEvent = null;
			BalanceBasedAmountDue(initialAmount);
		}

		/// set SCHEDULED (balance based) interest and amount due for each non-closed installment
		/// <exception cref="NoInstallmentFoundException">Condition. </exception>
		/// <exception cref="NoScheduleException">Condition. </exception>
		internal void BalanceBasedAmountDue(decimal balance) {
			BalanceBasedInterestCalculation = true;
			foreach (var s in this.events.Where(e => e.ScheduleItem != null)) {
				if (s.ScheduleItem.LoanScheduleStatusID != (int)NLScheduleStatuses.Paid) {
					s.ScheduleItem.Balance = balance;
					s.ScheduleItem.Interest = InterestBtwnEvents(s);
					balance -= s.ScheduleItem.Principal;
					s.ScheduleItem.Balance = balance;
					s.ScheduleItem.AmountDue = s.ScheduleItem.Principal - s.ScheduleItem.PrincipalPaid + s.ScheduleItem.Interest - s.ScheduleItem.InterestPaid + s.ScheduleItem.Fees - s.ScheduleItem.FeesPaid;

					s.ScheduleItem.AmountDue = s.ScheduleItem.AmountDue < 0 ? s.ScheduleItem.AmountDueOP : s.ScheduleItem.AmountDue;

					this.lastEvent = s;
				}
			}
			// set interest calculation mode back to rela open principal 
			BalanceBasedInterestCalculation = false;
		}

		/// <summary>
		/// 1. pay fees 
		/// 2. pay schedule (interest and principal)
		/// 3. pay rollover - record as rollover fee payment + interest payment to nearby schedule
		/// </summary>
		/// <param name="payment"></param>
		/// <exception cref="ArgumentNullException"><paramref /> or <paramref /> is null.</exception>
		/// <exception cref="OverflowException">The sum is larger than <see cref="F:System.Decimal.MaxValue" />.</exception>
		/// <exception cref="InvalidCastException"><paramref /> cannot be cast to the element type of the current <see cref="T:System.Array" />.</exception>
		[ExcludeFromToString]
		internal void ProcessPayment(NL_Payments payment) {

			// money that can be distributed to pay loan
			decimal assignAmount = payment.Amount - payment.FeePayments.Sum(p => p.Amount) - payment.SchedulePayments.Sum(p => p.InterestPaid) - payment.SchedulePayments.Sum(p => p.PrincipalPaid);
			if (assignAmount <= 0)
				return;
			// 1. pay fees

			// all unpaid late fees (not setup|servicing|arrangement), not related to calculation date
			assignAmount = PayFees(this.lateFeesList, payment, assignAmount);
			if (assignAmount <= 0)
				return;

			// unpaid servicing|arrangement untill LastEventDate including
			var distributedfeesUntillThisEvent = this.distributedFeesList.Where(f => f.AssignTime <= payment.PaymentTime);

			assignAmount = PayFees(distributedfeesUntillThisEvent.ToList(), payment, assignAmount);
			if (assignAmount <= 0)
				return;
			//Log.Debug("ProcessPayment: payment balance after PayFees = {0}", assignAmount);

			// 2. pay interest & principal
			PaySchedules(payment, assignAmount);

			//Log.Debug("ProcessPayment: payment balance after PaySchedules = {0}", assignAmount);
		}

		/// <summary>
		/// Assign current payment to loan schedules. 
		/// 1. Record new NL_LoanSchedulePayments entries to payment.SchedulePayments
		/// 2. cumulate paid principal into currentPaidPrincipal
		/// 3. update currentOpenPrincipal (p'(t) = A - sum(p(t')) where p'(t) - current open principal; A - initial amount taken, paid(t') - principal paid at this event; sum(p(t')) - principal paid untill this event 
		/// </summary>
		/// <param name="payment"></param>
		/// <param name="assignAmount"></param>
		/// <returns>decimal assignAmount</returns>
		/// <exception cref="OverflowException">The result is outside the range of a <see cref="T:System.Decimal" />.</exception>
		[ExcludeFromToString]
		internal decimal PaySchedules(NL_Payments payment, decimal assignAmount) {

			assignAmount = decimal.Round(assignAmount, this.decimalAccurancy);

			if (assignAmount <= 0 || payment == null) {
				//Log.Info("PaySchedules: No amount to assign: {0}", assignAmount);
				return assignAmount;
			}

			// Record schedules payments

			// find unpaid and partial paid schedules
			foreach (NL_LoanSchedules s in this.schedule) {

				if (assignAmount <= 0) {
					//Log.Debug("PaySchedules: amount assigned completely");
					return 0;
				}
				
				//Log.Debug("PaySchedules: currentEarnedInterest: {0}, currentPaidInterest: {1},  assignAmount: {2}, ScheduleToPay\n{3}{4}",currentEarnedInterest, currentPaidInterest, assignAmount, AStringable.PrintHeadersLine(typeof(NL_LoanSchedules)), s.ToStringAsTable());

				decimal iAmount = decimal.Round(Math.Min(assignAmount, (currentEarnedInterest - currentPaidInterest)), this.decimalAccurancy);

				assignAmount -= iAmount;

				decimal pPaidAmount = 0;
				foreach (var p in WorkingModel.Loan.Payments.Where(p => p.PaymentStatusID == (int)NLPaymentStatuses.Active))
					pPaidAmount += p.SchedulePayments.Where(sp => sp.LoanScheduleID == s.LoanScheduleID).Sum(sp => sp.PrincipalPaid);

				//schedule (principal) paid
				if (s.Principal == pPaidAmount)
					continue;

				// balance of principal p'
				decimal pAmount = decimal.Round(Math.Min(assignAmount, (s.Principal - pPaidAmount)), this.decimalAccurancy);

				assignAmount -= pAmount;

				// new schedule payments
				if (iAmount > 0 || pAmount > 0) {

					NL_LoanSchedulePayments schpayment = new NL_LoanSchedulePayments {
						InterestPaid = iAmount,
						LoanScheduleID = s.LoanScheduleID,
						PaymentID = payment.PaymentID,
						PrincipalPaid = pAmount,
						NewEntry = true
					};

					payment.SchedulePayments.Add(schpayment);

					// cumulate interest paid on the payment to currentPaidInterest
					currentPaidInterest += iAmount;

					// cumulate principal paid on the payment to currentPaidPrincipal
					currentPaidPrincipal += pAmount;

					// update currentOpenPrincipal (principal to pay)
					currentOpenPrincipal = initialAmount - currentPaidPrincipal;

					// update oi = ei - sum(paidi) (oi - outstanding interest; eo - earned interest, paidi - all paid interest till t') - after payment
					s.InterestOP = currentEarnedInterest - currentPaidInterest;

					//Log.Debug("PaySchedules: added new LoanSchedulePayment\n {0}{1}", AStringable.PrintHeadersLine(typeof(NL_LoanSchedulePayments)), schpayment.ToStringAsTable());
				}
			}

			return assignAmount;
		}

		/// <summary>
		/// 1. pay late fees
		/// 2. pay distributed fees that should be paid till event date
		/// 3. Record new NL_LoanFeePayments entry(s) to payment
		/// 4. Cumulate paid fees into currentPaidFees
		/// </summary>
		/// <param name="feesList"></param>
		/// <param name="payment"></param>
		/// <param name="assignAmount"></param>
		/// <returns>rest of available money of the payment</returns>
		/// <exception cref="OverflowException">The sum is larger than <see cref="F:System.Decimal.MaxValue" />.</exception>
		/// <exception cref="InvalidCastException"><paramref /> cannot be cast to the element type of the current <see cref="T:System.Array" />.</exception>
		[ExcludeFromToString]
		internal decimal PayFees(List<NL_LoanFees> feesList, NL_Payments payment, decimal assignAmount) {

			assignAmount = decimal.Round(assignAmount, this.decimalAccurancy);

			if (feesList.Count == 0 || payment == null || assignAmount <= 0) {
				//Log.Info("PayFees: No payment defined");
				return assignAmount;
			}

			foreach (NL_LoanFees f in feesList) {

				if (assignAmount <= 0) {
					return assignAmount;
				}

				// how much paid for the fee
				decimal fPaid = 0m;
				foreach (var p in WorkingModel.Loan.Payments.Where(p => p.PaymentStatusID == (int)NLPaymentStatuses.Active))
					fPaid += p.FeePayments.Where(fp => fp.LoanFeeID == f.LoanFeeID).Sum(fp => fp.Amount);
				
				decimal fBalance = decimal.Round((f.Amount - fPaid), this.decimalAccurancy);

				if (fBalance == 0) {
					Log.Debug("FeeID: {0} paid", f.LoanFeeID);
					continue;
				}

				// fee disabled before the payment occured, i.e. the payment couldnt to pay the fee
				if (f.DisabledTime != null && payment.PaymentTime > f.DisabledTime) {
					Log.Debug("Trying to pay disabled fee: {0}\n{1} with payment: {2}\n{3}", AStringable.PrintHeadersLine(typeof(NL_LoanFees)), f.ToStringAsTable(), AStringable.PrintHeadersLine(typeof(NL_Payments)), payment.ToStringAsTable());
					continue;
				}

				// fee's assign date is later then payment's date
				if (f.AssignTime.Date > payment.PaymentTime.Date) {
					Log.Debug("Tried to pay fee: {0}\n{1} with payment: {2}\n{3}", AStringable.PrintHeadersLine(typeof(NL_LoanFees)), f.ToStringAsTable(), AStringable.PrintHeadersLine(typeof(NL_Payments)), payment.ToStringAsTable());
					continue;
				}

				// fees not paid completely

				decimal fAmount = decimal.Round(Math.Min(assignAmount, fBalance), this.decimalAccurancy);

				NL_LoanFeePayments fpayment = new NL_LoanFeePayments {
					Amount = fAmount,
					LoanFeeID = f.LoanFeeID,
					LoanFeePaymentID = payment.PaymentID,
					PaymentID = payment.PaymentID,
					NewEntry = true
				};

				payment.FeePayments.Add(fpayment);

				// decrease paid amount from available payment amount
				assignAmount -= fAmount;

				//	cumulate fees paid on the payment to currentPaidFees
				currentPaidFees += fAmount;

				//Log.Debug("PayFees: for fee\n {0}{1} feePayment recorded\n {2}{3}", AStringable.PrintHeadersLine(typeof(NL_LoanFees)), f.ToStringAsTable(), AStringable.PrintHeadersLine(typeof(NL_LoanFeePayments)), fpayment.ToStringAsTable());
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

				decimal tDistributedFees = this.distributedFeesList.Where(f => f.AssignTime.Date <= this.calculationDateEventEnd.EventTime.Date).Sum(f => f.Amount);

				// outstanding balance = Fees + Interest + Principal
				Fees = (totalLateFees + tDistributedFees - currentPaidFees);
				Interest = (currentEarnedInterest - currentPaidInterest);
				Principal = currentOpenPrincipal;

				TotalEarlyPayment = Fees + Interest + Principal;

				// copy result data to NL_Model 
				WorkingModel.Fees = Fees;
				WorkingModel.Interest = Interest;
				WorkingModel.Principal = Principal;
				WorkingModel.TotalEarlyPayment = TotalEarlyPayment;

				//Log.Debug("GetState Action: Fees={0}, Interest={1}, Principal={2}, TotalEarlyPayment={3:F4}, RolloverPayment={4:F4}", WorkingModel.Fees, WorkingModel.Interest, WorkingModel.Principal, WorkingModel.TotalEarlyPayment, WorkingModel.RolloverPayment);
			};

			HandleEvents();

			CheckLoanClosed();

			// AmountDue - in each schedule

			// calc date schedule item
			NL_LoanSchedules calcDateItem = GetScheduleItemForDate(CalculationDate);

			if (calcDateItem != null) {

				//Log.Debug("calcDateItem: \n{0}{1}", AStringable.PrintHeadersLine(typeof(NL_LoanSchedules)), calcDateItem.ToStringAsTable());

				NextEarlyPayment = calcDateItem.Principal - calcDateItem.PrincipalPaid + Interest + Fees;

				NextEarlyPaymentSavedAmount = calcDateItem.Interest - earnedInterestForCalculationDate;

				TotalEarlyPayment = currentEarnedInterest - calcDateItem.Interest; // TODO check???

				WorkingModel.NextEarlyPayment = NextEarlyPayment;
				WorkingModel.NextEarlyPaymentSavedAmount = NextEarlyPaymentSavedAmount;
				WorkingModel.TotalEarlyPayment = TotalEarlyPayment;
			}

			RolloverPayment = Interest; // to display separated: 1. rollover fee (from cong.varilables); 2. late fees till "rolover opportunity" (calc.date) == (open Fees)

			// set outstanding balance - if pay as scheduled, this is the total amount due for calculation date
			WorkingModel.Loan.Histories.ForEach(h => h.Schedule.ForEach(s => Balance += s.AmountDue));

			WorkingModel.Balance = Balance;

			/*// All previous installments are paid? i.e. check late
			if (HasLatesTillCalculationDate()) {
				return;
			}

			// Today is a payment day
			if (CalculationDateIsPlannedDate()) {
				return;
			}*/
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
		[ExcludeFromToString]
		internal void ProcessScheduleItem(NL_LoanSchedules item) {
			// i'- current open interest, payments based (== real open principal)
			// update oi = ei - sum(paidi) (oi - outstanding interest; eo - earned interest, paidi - all paid interest till t') - before payment
			item.InterestOP = currentEarnedInterest - currentPaidInterest;
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

			var closeItem = ((item.Principal - item.PrincipalPaid) == 0 && item.Fees == 0);

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
		/// Set real AmountDue
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

			item.PrincipalPaid = 0;
			item.InterestPaid = 0;
			item.FeesPaid = 0;

			decimal lateFeesPaid = 0;

			var distributedFee = this.distributedFeesList.FirstOrDefault(f => f.AssignTime.Date == item.PlannedDate.Date);
			var lateFee = this.lateFeesList.Where(f => f.AssignTime.Date <= item.PlannedDate.Date).ToList();

			foreach (NL_Payments p in WorkingModel.Loan.Payments) {
				// paid for item principal
				item.PrincipalPaid += p.SchedulePayments.Where(sp => sp.LoanScheduleID == item.LoanScheduleID).Sum(sp => sp.PrincipalPaid);

				// paid for item interest
				item.InterestPaid += p.SchedulePayments.Where(sp => sp.LoanScheduleID == item.LoanScheduleID).Sum(sp => sp.InterestPaid);

				// paid for item distributed fee
				if (distributedFee != null) {
					item.FeesPaid += p.FeePayments.Where(fp => fp.LoanFeeID == distributedFee.LoanFeeID).Sum(fp => fp.Amount);
				}

				// all "late fees" pays
				foreach (NL_LoanFees f in lateFee) {
					lateFeesPaid += p.FeePayments.Where(fp => fp.LoanFeeID == f.LoanFeeID).Sum(fp => fp.Amount);
				}
			}

			item.FeesPaid += lateFeesPaid;

			item.Fees = distributedFee == null ? 0 : distributedFee.Amount;

			// attached all unpaid late fees to first (nearby) not paid installment - f'
			var lateFeesSchedule = WorkingModel.Loan.LastHistory().Schedule.FirstOrDefault(s => s.LateFeesAttached);

			// attach late fees
			if (lateFeesSchedule == null && (totalLateFees - lateFeesPaid) > 0) {
				// current open fees total (late+distributed)
				item.Fees += (totalLateFees - lateFeesPaid);
				item.LateFeesAttached = true;
			}

			item.AmountDueOP = item.Principal - item.PrincipalPaid + item.InterestOP + item.Fees - item.FeesPaid;

			//Log.Debug("ScheduleItemOutstandingData \n {0}{1}", AStringable.PrintHeadersLine(typeof(NL_LoanSchedules)), item.ToStringAsTable());
		}

		/// <exception cref="OverflowException"><paramref /> represents a number that is less than <see cref="F:System.Int32.MinValue" /> or greater than <see cref="F:System.Int32.MaxValue" />. </exception>
		public void RolloverRescheduling() {

			// get balance
			try {
				GetState();
			} catch (Exception ex) {
				Log.Error("Faled to GetState calculator, err: {0}", ex);
				return;
			}

			NL_LoanHistory prevHistory = WorkingModel.Loan.LastHistory();

			NL_LoanHistory rolloverHistory = new NL_LoanHistory {
				Amount = Balance,
				InterestRate = prevHistory.InterestRate,
				Description = "rollover",
				EventTime = CalculationDate, // TODO check +month?
				LoanID = prevHistory.LoanID,
				LoanLegalID = prevHistory.LoanLegalID,
				AgreementModel = prevHistory.AgreementModel,
				Agreements = prevHistory.Agreements,
				RepaymentIntervalTypeID = prevHistory.RepaymentIntervalTypeID,
				UserID = 1
			};

			rolloverHistory.LoanHistoryID = 0;
			rolloverHistory.Amount = Balance;
			rolloverHistory.Description = "rollover";
			rolloverHistory.EventTime = CalculationDate;

			int deletedItems = 0;

			string intervalEnumName = Enum.GetName(typeof(RepaymentIntervalTypes), rolloverHistory.RepaymentIntervalTypeID);

			var daysInInterval = Convert.ToInt32(Enum.Parse(typeof(RepaymentIntervalTypes), intervalEnumName).DescriptionAttr());

			var removeList = WorkingModel.Loan.LastHistory().Schedule.Where(s => s.PlannedDate >= CalculationDate);

			// copy future schedules to new history; mark future schedules schedules as DeletedOnReschedule + close time??? 
			foreach (NL_LoanSchedules s in removeList) {

				NL_LoanSchedules s1 = s.ShallowCopy();

				s1.PlannedDate = (rolloverHistory.RepaymentIntervalTypeID == (int)RepaymentIntervalTypes.Month) ? s.PlannedDate.AddMonths(1) : s.PlannedDate.AddDays(daysInInterval);
				s1.LoanScheduleID = 0;

				rolloverHistory.Schedule.Add(s1);

				// get info about payments for this schedule teim
				ScheduleItemOutstandingData(s);

				// mark removed item
				s.LoanScheduleStatusID = (s.AmountDue == 0) ? (int)NLScheduleStatuses.ClosedOnReschedule : (int)NLScheduleStatuses.DeletedOnReschedule;
				s.ClosedTime = CalculationDate; // TODO check

				deletedItems++;
			}

			rolloverHistory.RepaymentCount = deletedItems;

			WorkingModel.Loan.Histories.Add(rolloverHistory);

			try {
				this.events = null;
				GetState();
			} catch (Exception ex) {
				Log.Error("Faled to GetState calculator, err: {0}", ex);
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
