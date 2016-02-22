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
		protected ALoanCalculator(NL_Model model, DateTime? calculationDate = null, bool lastHistoryOnly = true) {

			if (model == null)
				throw new NoInitialDataException("nl model not found.");

			if (model.Loan == null)
				throw new NoInitialDataException("Loan in nl model not found.");

			WorkingModel = model;
			WriteToLog = true;

			currentHistory = WorkingModel.Loan.LastHistory()??null;

			if (currentHistory == null)
				throw new NoLoanHistoryException();

			if (currentHistory.EventTime == DateTime.MinValue)
				throw new NoInitialDataException("loan (history) creation date (EventTime) not found");

			if (currentHistory.InterestRate == 0)
				throw new InvalidInitialInterestRateException(currentHistory.InterestRate);

			if (decimal.Round(currentHistory.Amount) == 0)
				throw new InvalidInitialAmountException(currentHistory.Amount);

			NowTime = DateTime.UtcNow;

			CalculationDate = calculationDate ?? NowTime;

			InterestCalculationDateStart = currentHistory.EventTime;
			initialAmount = currentHistory.Amount;
			currentOpenPrincipal = initialAmount;

			LastHistoryOnly = lastHistoryOnly;
		}

		internal int decimalAccurancy = 6;

		public abstract string Name { get; }

		public bool LastHistoryOnly { get; private set; }

		/// <summary>
		/// loan state date - input
		/// </summary>
		public DateTime CalculationDate { get; internal set; }

		public DateTime NowTime { get; private set; }

		internal LoanEvent calculationDateEventEnd;

		public ASafeLog Log { get { return Library.Instance.Log ?? new ConsoleLog(); }}

		public bool WriteToLog { get; set; }

		public decimal TotalEarlyPayment { get; internal set; }

		public decimal NextEarlyPayment { get; internal set; }

		public decimal RolloverPayment { get; internal set; } // == AccumilatedInterest

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

		/// <summary>
		/// loan's events sequence
		/// </summary>
		internal List<LoanEvent> events;

		internal LoanEvent acceptedRollover;
		internal bool acceptedRolloverProcessed;

		/// <summary>
		/// period for interest calculation
		/// </summary>
		internal DateTime InterestCalculationDateStart { get; set; }

		/// <summary>
		/// helper contains all non-deleted schedules from all histories
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

		public NL_Model WorkingModel { get; internal set; }

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
		/// 
		/// </summary>
		/// <param name="model"></param>
		/// <param name="servicingFeeAmount"></param>
		/// <param name="createTime"></param>
		/// <param name="notes"></param>
		[ExcludeFromToString]
		public virtual void AttachDistributedFeesToLoanBySchedule(NL_Model model, decimal servicingFeeAmount, DateTime createTime, string notes = "spread (servicing) fee") {
			if (servicingFeeAmount == 0m) {
				Log.Debug("no servicingFeeAmount");
				return;
			}

			int schedulesCount = model.Loan.LastHistory().Schedule.Count;
			decimal iFee = Math.Floor(servicingFeeAmount / schedulesCount);
			decimal firstFee = (servicingFeeAmount - iFee * (schedulesCount - 1));

			foreach (NL_LoanSchedules s in model.Loan.LastHistory().Schedule) {
				decimal feeAmount = (schedulesCount > 0) ? firstFee : iFee;
				model.Loan.Fees.Add(
					new NL_LoanFees() {
						LoanID = model.Loan.LoanID,
						LoanFeeID = 0,
						Amount = feeAmount,
						AssignTime = s.PlannedDate,
						Notes = notes,
						LoanFeeTypeID = (int)NLFeeTypes.ServicingFee,
						CreatedTime = createTime, 
						AssignedByUserID = model.UserID ?? 1
					});

				s.Fees += feeAmount;
				s.AmountDue += s.Fees;

				schedulesCount = 0; // reset count, because it used as firstFee/iFee flag
			}
		}

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
		/// <exception cref="NoScheduleException">Condition. </exception>
		[ExcludeFromToString]
		internal decimal InterestRateForDate(DateTime theDate, NL_LoanSchedules item = null) {
			// relevant freeze interval
			var freezePeriod = WorkingModel.Loan.FreezeInterestIntervals.FirstOrDefault(fr => fr.StartDate >= theDate.Date && theDate.Date <= fr.EndDate && fr.ActivationDate <= theDate.Date);
			if (freezePeriod != null) {
				Log.Debug("freezePeriod: {0}, theDate: {1}", freezePeriod, theDate);
				return 0;
			}

			if (item == null)
				item = GetScheduleItemForDate(theDate);

			decimal interestRate = currentHistory.InterestRate;
			DateTime plannedDate = theDate;// InterestCalculationDateStart.Date;

			if (item != null) {
				interestRate = item.InterestRate;
				plannedDate = item.PlannedDate;
			}

			// dr' = r/daysDiff or BankLike
			decimal dr = AverageDailyInterestRate(interestRate, plannedDate);

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
		/// Interest rate for a specific date, specified in schedule item. If schedule exist, returns scheduled rate, otherwise returns histories' rate.
		/// common: Calculate total interest to pay at specific event date, during looping through ordered events list
		/// calculate interest rate btwn 2 nearby events in the list day by day; add interest calculated to currentEarnedInterest; set currentEvent date as lastEventDate
		/// </summary>
		/// <returns></returns>
		/// <param name="currentEvent"></param>
		/// <exception cref="NoScheduleException">Condition. </exception>
		/// [ExcludeFromToString]
		internal decimal InterestBtwnEvents(LoanEvent currentEvent) {

			DateTime start = (this.lastEvent == null ? InterestCalculationDateStart : this.lastEvent.EventTime).Date.AddDays(1);

			currentEvent.EarnedInterestForPeriod = InterestBtwnDates(start, currentEvent.EventTime, currentOpenPrincipal);

			Log.Debug("InterestBtwnEvents: start={0:d}, event={1:d}, interestForPeriod={2:F4}, currentOpenPrincipal={3:F6}", start.Date, currentEvent.EventTime.Date, currentEvent.EarnedInterestForPeriod, currentOpenPrincipal);

			return decimal.Round(currentEvent.EarnedInterestForPeriod, this.decimalAccurancy);
		}

		/// <exception cref="NoScheduleException">Condition. </exception>
		internal decimal InterestBtwnDates(DateTime startDate, DateTime endDate, decimal balance) {
			int daysDiff = endDate.Subtract(startDate).Days;

			if (daysDiff <= 0)
				return 0;

			decimal interestForPeriod = 0;
			DateTime rateDate = startDate.Date;

			//	interest for period = dr'(t)*p'(t)
			for (int i = 0; i <= daysDiff; i++) {
				decimal dailyInterestRate = InterestRateForDate(rateDate, GetScheduleItemForDate(rateDate));	//	daily interest  dr' 
				interestForPeriod += balance * dailyInterestRate;
				rateDate = startDate.AddDays(i).Date;
			}

			//Log.Debug("InterestBtwnDates: start={0:s}, end={1:s}, days={2}, interestForPeriod={3:F4}, balance={4:F6}", startDate.Date, endDate.Date, daysDiff, interestForPeriod, balance);

			return decimal.Round(interestForPeriod, this.decimalAccurancy);
		}

		/// <summary>
		/// Initialize loan events and common loan data vars
		/// </summary>
		/// <exception cref="NoScheduleException">Condition. </exception>
		[ExcludeFromToString]
		internal void InitEvents() {

			//prevent duplicate initializations
			if (this.events != null)
				return;

			bool loanClosed = false;
			try {
				CheckLoanClosed();
			} catch (LoanPendingStatusException) {
				loanClosed = true;
			} catch (LoanPaidOffStatusException) {
				loanClosed = true;
			} catch (LoanWriteOffStatusException) {
				loanClosed = true;
			}

			// for closed loan - move CalculationDate to loan maturity date
			if (loanClosed && WorkingModel.Loan.DateClosed != null)
				CalculationDate = (DateTime)WorkingModel.Loan.DateClosed;

			this.calculationDateEventEnd = new LoanEvent(CalculationDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59));

			List<LoanEvent> historiesEvents = new List<LoanEvent>();
			List<LoanEvent> feesEvents = new List<LoanEvent>();
			List<LoanEvent> paymentEvents = new List<LoanEvent>();
			List<LoanEvent> schedulesEvents = new List<LoanEvent>();
			List<LoanEvent> acceptedRolloverEvents = new List<LoanEvent>();
			List<LoanEvent> actionEvents = new List<LoanEvent>();

			this.acceptedRollover = null;
			this.schedule.Clear();

			currentPaidFees = 0;
			currentPaidInterest = 0;
			currentPaidPrincipal = 0;

			// TODO remove those from processing, not from model
			// remove future histories, payments, late fees
			//WorkingModel.Loan.Histories.RemoveAll(h => h.EventTime.Date > CalculationDate.Date);
			//WorkingModel.Loan.Payments.RemoveAll(p => p.PaymentTime.Date > CalculationDate.Date);
			//WorkingModel.Loan.Fees.RemoveAll(f => f.LoanFeeTypeID != (int)NLFeeTypes.ArrangementFee && f.LoanFeeTypeID != (int)NLFeeTypes.ServicingFee && f.AssignTime.Date > CalculationDate.Date);

			// histories => schedules
			foreach (var h in WorkingModel.Loan.Histories) {

				historiesEvents.Add(new LoanEvent(new DateTime(h.EventTime.Year, h.EventTime.Month, h.EventTime.Day), h));

				decimal initAmount = h.Amount;
				decimal plannedPrincipal = 0;

				foreach (NL_LoanSchedules s in h.Schedule.Where(s => !s.IsDeleted())) {

					s.Balance = initAmount - plannedPrincipal;
					plannedPrincipal += s.Principal;

					schedulesEvents.Add(new LoanEvent(new DateTime(s.PlannedDate.Year, s.PlannedDate.Month, s.PlannedDate.Day), s));
					this.schedule.Add(s);
				}
			}

			// fees
			foreach (var e in WorkingModel.Loan.Fees.OrderBy(f => f.AssignTime)) {

				feesEvents.Add(new LoanEvent(new DateTime(e.AssignTime.Year, e.AssignTime.Month, e.AssignTime.Day), e));

				if ((e.LoanFeeTypeID == (int)NLFeeTypes.ServicingFee || e.LoanFeeTypeID == (int)NLFeeTypes.ArrangementFee) /* && (e.DisabledTime == null && e.DeletedByUserID == null)*/) {
					this.distributedFeesList.Add(e);
				} else {
					this.lateFeesList.Add(e);

					// cumulate totalLateFees
					totalLateFees += e.Amount;
				}
			}

			// payments - by PaymentTime, PaymentID, only non-deleted
			foreach (NL_Payments p in WorkingModel.Loan.Payments.OrderBy(p => p.PaymentTime).ThenBy(p=>p.PaymentID)) {

				var item = GetScheduleItemForDate(p.PaymentTime);
				bool itemDeleted = false;

				// init currentPaidInterest, currentPaidPrincipal
				foreach (NL_LoanSchedulePayments sp in p.SchedulePayments) {

					if (item != null) {
						itemDeleted = item.IsDeleted();
					}

					if (itemDeleted) {
						sp.InterestPaid = 0;
						sp.PrincipalPaid = 0;
					}

					// init paid interest
					currentPaidInterest += sp.InterestPaid;
					// init paid principal
					currentPaidPrincipal += sp.PrincipalPaid;
				}

				// init currentPaidFees
				foreach (NL_LoanFeePayments fp in p.FeePayments) {
					currentPaidFees += fp.Amount;
				}

				// charge-back date
				if (p.PaymentStatusID == (int)NLPaymentStatuses.ChargeBack && !p.DeletionTime.Equals(DateTime.MinValue) && p.DeletionTime != null) {
					// prevent from registering real schedule payments or fee payments
					p.Amount = 0;

					// payment registered in the system
					// on this point principal paid by the payment, should be removed from currently open principal, i.e. "ha-kesef shakhaf etzlenu"
					paymentEvents.Add(new LoanEvent(new DateTime(p.PaymentTime.Year, p.PaymentTime.Month, p.PaymentTime.Day), p, 0, true, true));

					DateTime chargeBackTime = (DateTime)p.DeletionTime;

					// payment charged back from the system
					// on this point principal paid by the payment, should be added to the currently open principal
					paymentEvents.Add(new LoanEvent(new DateTime(chargeBackTime.Year, chargeBackTime.Month, chargeBackTime.Day), p, 0, true));

				} else {
					// regular payment
					paymentEvents.Add(new LoanEvent(new DateTime(p.PaymentTime.Year, p.PaymentTime.Month, p.PaymentTime.Day), p));
				}
			}

			// add event for accepted and non processed rollovers
			NL_LoanRollovers pendingRollover = WorkingModel.Loan.Rollovers.LastOrDefault(r => r.IsAccepted && r.CustomerActionTime.HasValue && r.DeletionTime == null && r.DeletedByUserID == null);

			if (pendingRollover != null) {

				DateTime acDate = pendingRollover.CustomerActionTime.Value;
				var historyEvent = WorkingModel.Loan.Histories.FirstOrDefault(h => h.EventTime.Date.Equals(acDate.Date));
				this.acceptedRolloverProcessed = true;

				if (historyEvent == null) {
					acceptedRolloverEvents.Add(new LoanEvent(
						new DateTime(pendingRollover.CustomerActionTime.Value.Year, pendingRollover.CustomerActionTime.Value.Month, pendingRollover.CustomerActionTime.Value.Day), pendingRollover));
					this.acceptedRollover = acceptedRolloverEvents.LastOrDefault();
					this.acceptedRolloverProcessed = false;
				}
			}

			if (this.calculationDateEventEnd != null)
				actionEvents.Add(this.calculationDateEventEnd);

			// combine ordered events list
			this.events = historiesEvents.Union(schedulesEvents)
				.Union(paymentEvents)
				.Union(feesEvents)
				.Union(acceptedRolloverEvents)
				//  .Union(reschedules) TODO add reschedules
				.Union(actionEvents)
				.OrderBy(e => e.EventTime).ThenBy(e => e.Priority).ToList();

			// actually nulls should not exists here
			this.events.RemoveAll(e => e == null);

			// remove events that happened before last history occured
			if (LastHistoryOnly) {
				this.events.RemoveAll(e => e.EventTime <= WorkingModel.Loan.LastHistory().EventTime);
			}

			/*Log.Debug("\n\n==================CALCULATOR EVENTS==========================");
			this.events.ForEach(e => Log.Debug(e));
			Log.Debug("\n==================this.schedule==========================");
			this.schedule.ForEach(e => Log.Debug(e));
			Log.Debug("###==================CALCULATOR EVENTS==========================\n\n");*/
		}

		/// <exception cref="NoLoanEventsException">Condition. </exception>
		/// <exception cref="NoScheduleException">Condition. </exception>
		/// <exception cref="OverflowException">The sum is larger than <see cref="F:System.Decimal.MaxValue" />.</exception>
		/// <exception cref="Exception">A delegate callback throws an exception. </exception>
		[ExcludeFromToString]
		internal virtual void HandleEvents() {

			InitEvents();

			if (this.events == null || this.events.Count == 0)
				throw new NoLoanEventsException();

			currentOpenPrincipal = initialAmount - currentPaidPrincipal;

			NL_LoanSchedules item = null;

			foreach (LoanEvent e in this.events) {

				e.CurrentHistory = currentHistory;

				currentEarnedInterest += InterestBtwnEvents(e);
				e.OpenPrincipalForPeriod = currentOpenPrincipal;

				Log.Debug("HandleEvents: {0} OpenPrincipal={1}", e, currentOpenPrincipal);

				this.lastEvent = e;

				switch (e.GetTypeString()) {

				case "History":
					currentHistory = e.History;
					break;

				case "Fee":
					break;

				case "Payment":
					ProcessPayment(e.Payment);
					break;

				case "ChargebackPaymentRecorded":
					// remove paid principal to current OpenPrincipal, i.e. decrease open principal for interest caclulation
					var paidPrincipalR = e.ChargebackPaymentRecorded.SchedulePayments.Sum(sp => sp.ResetPrincipalPaid);
					if (paidPrincipalR != null) {
						currentOpenPrincipal -= (decimal)paidPrincipalR;
					}
					break;

				case "ChargebackPaymentCancelled":
					// add paid principal to current OpenPrincipal, i.e. add unpaid principal open principal
					var paidPrincipal = e.ChargebackPaymentCancelled.SchedulePayments.Sum(sp => sp.ResetPrincipalPaid);
					if (paidPrincipal != null) {
						currentOpenPrincipal += (decimal)paidPrincipal;
					}
					break;

				case "ScheduleItem":
					e.ScheduleItem.InterestOP = currentEarnedInterest;
					e.ScheduleItem.Interest = e.ScheduleItem.InterestOP;
					if (item != null)
						e.ScheduleItem.Interest = e.ScheduleItem.InterestOP - item.InterestOP;
					item = e.ScheduleItem;
					Log.Debug("ScheduleID={0}, s.Interest={1}, InterestOP={2}", e.ScheduleItem.LoanScheduleID, e.ScheduleItem.Interest, e.ScheduleItem.InterestOP);
					break;

				case "Rollover":
					try {
						new RolloverReschedulingMethod(this).Execute();
						// ReSharper disable once CatchAllClause
					} catch (Exception exception) {
						Log.Error("Failed to process rollover rescheduling. err: {0}", exception);
					}

					break;

				case "Action":
					e.Action(); // execute the action defined
					break;

				default:
					Log.Debug("Unknown event type: {0}", e);
					break;
				}

				//Log.Debug("------------------------OpenPrincipal: {0}", currentOpenPrincipal);

			} //foreach this.events

			SetAmountsDue();
		}

		/// <summary>
		/// AmountDue for the first unpaid schedule:
		///  1. f - distributed fees planned for this schedule date (assigned-paid)
		///  2. i - schedule interest - schedule paid interest	
		///  3. p - unpaid principal of this schedule	
		/// </summary>
		private void SetAmountsDue() {

			this.lastEvent = null;

			foreach (LoanEvent e in this.events.Where(e => e.ScheduleItem != null).OrderBy(e => e.EventTime)) {

				e.ScheduleItem.FeesAssigned = 0;
				e.ScheduleItem.FeesPaid = 0;
				e.ScheduleItem.PrincipalPaid = 0;
				e.ScheduleItem.InterestPaid = 0;

				// 1. f - distributed fees planned for this schedule date (assigned-paid)
				var dFee = this.distributedFeesList.FirstOrDefault(f => f.AssignTime.Date.Equals(e.ScheduleItem.PlannedDate.Date));
				if (dFee != null) {

					e.ScheduleItem.FeesAssigned = dFee.Amount;

					dFee.PaidAmount = 0;
					WorkingModel.Loan.Payments.ForEach(p => dFee.PaidAmount += (p.FeePayments.Where(fp => fp.LoanFeeID == dFee.LoanFeeID).Sum(fp => fp.Amount)));

					e.ScheduleItem.Fees = dFee.Amount - dFee.PaidAmount;

					e.ScheduleItem.FeesPaid += dFee.PaidAmount;
				}

				foreach (NL_Payments p in WorkingModel.Loan.Payments.Where(p => p.PaymentStatusID == (int)NLPaymentStatuses.Active)) {
					// paid for item principal
					e.ScheduleItem.PrincipalPaid += p.SchedulePayments.Where(sp => sp.LoanScheduleID == e.ScheduleItem.LoanScheduleID).Sum(sp => sp.PrincipalPaid);

					// paid for item interest
					e.ScheduleItem.InterestPaid += p.SchedulePayments.Where(sp => sp.LoanScheduleID == e.ScheduleItem.LoanScheduleID).Sum(sp => sp.InterestPaid); // poschitat' Interest ??? na kakom balance?
				}

				//	2. i - schedule interest - schedule paid interest	
				//  3. p - unpaid principal of this schedule	
				e.ScheduleItem.AmountDue =
					  e.ScheduleItem.FeesAssigned - e.ScheduleItem.FeesPaid
					+ e.ScheduleItem.Interest - e.ScheduleItem.InterestPaid
					+ e.ScheduleItem.Principal - e.ScheduleItem.PrincipalPaid;

				// if fully paid  - reset AmountDue
				e.ScheduleItem.AmountDue = e.ScheduleItem.AmountDue < 0 ? 0 : e.ScheduleItem.AmountDue;

				this.lastEvent = e;
			}

		} // SetAmountsDue


		/// <summary>
		/// 1. pay fees 
		/// 2. pay schedule (interest and principal)
		/// 3. pay rollover - record as rollover fee payment + interest payment to nearby schedule
		/// </summary>
		/// <param name="payment"></param>
		/// <exception cref="ArgumentNullException"><paramref /> or <paramref /> is null.</exception>
		/// <exception cref="OverflowException">The sum is larger than <see cref="F:System.Decimal.MaxValue" />.</exception>
		[ExcludeFromToString]
		internal void ProcessPayment(NL_Payments payment) {

			if (payment.PaymentDestination.Equals(NLPaymentDestinations.Rollover.ToString()) && !this.acceptedRolloverProcessed) {
				Log.Debug("Rollover payment - rollover is not accepted yet => exit");
				return;
			}

			// money that can be distributed to pay loan
			decimal assignAmount = payment.Amount - payment.FeePayments.Sum(p => p.Amount) - payment.SchedulePayments.Sum(p => p.InterestPaid) - payment.SchedulePayments.Sum(p => p.PrincipalPaid);
			if (assignAmount <= 0)
				return;

			// 1. pay fees

			// all unpaid late fees (not setup|servicing|arrangement), not related to calculation date
			assignAmount = PayFees(this.lateFeesList, payment, assignAmount);
			if (assignAmount <= 0)
				return;

			// 7 January 2016 - according to Vitas, distributed fees should be paid in the same order as late fees, i.e. 
			//	1. fee
			//	2. interest
			//	3. principal
			// Moved to PaySchedules method

			// unpaid servicing|arrangement untill LastEventDate including
			//var distributedfeesUntillThisEvent = this.distributedFeesList.Where(f => f.AssignTime <= payment.PaymentTime);

			//assignAmount = PayFees(distributedfeesUntillThisEvent.ToList(), payment, assignAmount);
			//if (assignAmount <= 0)
			//	return;

			//Log.Debug("ProcessPayment: payment balance after PayFees = {0}", assignAmount);

			// 2. pay interest & principal
			PaySchedules(payment, assignAmount);

			//Log.Debug("ProcessPayment: payment balance after PaySchedules = {0}", assignAmount);
		}

		/// <summary>
		/// Assign current payment to loan schedules. 
		/// 1. Record new NL_LoanSchedulePayments entries to payment.SchedulePayments
		/// 2. cumulate paid principal into currentPaidPrincipal
		/// 3. update current OpenPrincipal (p'(t) = A - sum(p(t')) where p'(t) - current open principal; A - initial amount taken, paid(t') - principal paid at this event; sum(p(t')) - principal paid untill this event 
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

				DateTime plannedDate = s.PlannedDate;

				// unpaid servicing|arrangement RELATED TO THIS SCHEDULE ITEM DATE
				var distributedfee = this.distributedFeesList.Where(f => f.AssignTime.Date.Equals(plannedDate.Date));

				// f' (distributed)
				assignAmount = PayFees(distributedfee.ToList(), payment, assignAmount);

				if (assignAmount <= 0) {
					//Log.Debug("PaySchedules: amount assigned completely");
					return 0;
				}

				//Log.Debug("PaySchedules: currentEarnedInterest: {0}, currentPaidInterest: {1},  assignAmount: {2}, ScheduleToPay\n{3}{4}",currentEarnedInterest, currentPaidInterest, assignAmount, AStringable.PrintHeadersLine(typeof(NL_LoanSchedules)), s.ToStringAsTable());

				// i'
				decimal iAmount = decimal.Round(Math.Min(assignAmount, (currentEarnedInterest - currentPaidInterest)), this.decimalAccurancy);

				assignAmount -= iAmount;

				decimal pPaidAmount = 0;
				foreach (var p in WorkingModel.Loan.Payments.Where(p => p.PaymentStatusID == (int)NLPaymentStatuses.Active))
					pPaidAmount += p.SchedulePayments.Where(sp => sp.LoanScheduleID == s.LoanScheduleID).Sum(sp => sp.PrincipalPaid);

				//schedule (principal) paid
				if (s.Principal == pPaidAmount)
					continue;

				// p'
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

					// update current OpenPrincipal (principal to pay)
					currentOpenPrincipal = initialAmount - currentPaidPrincipal;

					//Log.Debug("PaySchedules: added new LoanSchedulePayment\n {0}{1}", AStringable.PrintHeadersLine(typeof(NL_LoanSchedulePayments)), schpayment.ToStringAsTable());

					Log.Debug("==============>PaySchedules: currentOpenPrincipal={0}, currentPaidPrincipal={1}", currentOpenPrincipal, currentPaidPrincipal);
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
				f.PaidAmount = 0m;
				foreach (var p in WorkingModel.Loan.Payments.Where(p => p.PaymentStatusID == (int)NLPaymentStatuses.Active))
					f.PaidAmount += p.FeePayments.Where(fp => fp.LoanFeeID == f.LoanFeeID).Sum(fp => fp.Amount);

				decimal fBalance = decimal.Round((f.Amount - f.PaidAmount), this.decimalAccurancy);

				if (fBalance == 0) {
					Log.Debug("FeeID: {0} paid", f.LoanFeeID);
					continue;
				}

				// fee disabled before the payment occured, i.e. the payment couldnt to pay the fee
				if (f.DisabledTime != null && payment.PaymentTime > f.DisabledTime) {
					Log.Debug("Tried to pay fee [ID]={0} disabled at {1} with payment [ID]={1}, paymentDate at {2:d}", f.LoanFeeID, f.DisabledTime, payment.PaymentID, payment.PaymentTime);
					continue;
				}

				// fee's assign date is later then payment's date, i.e. future fees
				// allowed to pay future distributed fees on "pay schedule"
				if (f.AssignTime.Date > payment.PaymentTime.Date && f.LoanFeeTypeID != (int)NLFeeTypes.ServicingFee && f.LoanFeeTypeID != (int)NLFeeTypes.ArrangementFee) {
					Log.Debug("Tried to pay fee [ID]={0}, assigned at {1}, created at {2} with payment [ID]={3}, Amount={4}, paymentDate at {5:d}", f.LoanFeeID, f.AssignTime, f.CreatedTime, payment.PaymentID, payment.Amount, payment.PaymentTime);
					continue;
				}

				// fees not paid (completely or partially)

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
		/// <exception cref="OverflowException">The sum is larger than <see cref="F:System.Decimal.MaxValue" />.</exception>
		/// <exception cref="Exception">A delegate callback throws an exception. </exception>
		/// Attach late amounts to the first non-late "StillToPay" schedule item:
		///  1a. f - all unpaid late fees assigned before schedule planned date (including)
		///  1b. f - all unpaid distributed fees assigned before schedule planned date (including)
		///  2.  i - (currently earned interest - currently paid interest)		 
		///  3a. p - all unpaid principals from previous schedules
		/// <exception cref="InvalidCastException"><paramref /> cannot be cast to the element type of the current <see cref="T:System.Array" />.</exception>
		[ExcludeFromToString]
		public void GetState() {

			InitEvents();

			// outstanding balance le siluk miyadi = Fees + Interest + Principal

			decimal earnedInterestForCalculationDate = 0;

			// fetch the state on this event 
			this.calculationDateEventEnd.Action = () => {

				earnedInterestForCalculationDate = currentEarnedInterest;

				// distributed fees not paid in time
				decimal tDistributedFees = this.distributedFeesList.Where(f => f.AssignTime.Date <= this.calculationDateEventEnd.EventTime.Date && f.DisabledTime == null && f.DeletedByUserID == null).Sum(f => f.Amount);

				Fees = totalLateFees + tDistributedFees - currentPaidFees;	// outstanding fees
				Interest = currentEarnedInterest - currentPaidInterest;		// outstanding interest 
				Principal = currentOpenPrincipal;							// outstanding principal 

				currentHistory.Amount = currentOpenPrincipal;
				currentHistory.OutstandingInterest = Interest;
				currentHistory.LateFees = this.lateFeesList.Where(f => f.AssignTime.Date <= this.calculationDateEventEnd.EventTime.Date && f.DisabledTime == null && f.DeletedByUserID == null).Sum(f => f.Amount) - this.lateFeesList.Where(f => f.AssignTime.Date <= this.calculationDateEventEnd.EventTime.Date && f.DisabledTime == null && f.DeletedByUserID == null).Sum(f => f.PaidAmount);
				currentHistory.DistributedFees = tDistributedFees - this.distributedFeesList.Where(f => f.AssignTime.Date <= this.calculationDateEventEnd.EventTime.Date && f.DisabledTime == null && f.DeletedByUserID == null).Sum(f => f.PaidAmount);

				Log.Debug("GetStateAction: Fees={0}, Interest={1}, Principal={2}, earnedInterestForCalculationDate={3:F4} currentHistory={4}", WorkingModel.Fees, WorkingModel.Interest, WorkingModel.Principal, earnedInterestForCalculationDate, currentHistory);
			};

			HandleEvents();

			// calc date schedule item
			NL_LoanSchedules calcItem = GetScheduleItemForDate(this.calculationDateEventEnd.EventTime.Date);

			if (calcItem != null) {

				Log.Debug("Calculation date schedule: \n{0}{1}", AStringable.PrintHeadersLine(typeof(NL_LoanSchedules)), calcItem.ToStringAsTable());

				// if this item closed, get next non-closed
				if (calcItem.Principal <= calcItem.PrincipalPaid) {
					Log.Debug("CalcDateItem is paid");
					calcItem = this.schedule.FirstOrDefault(s => s.Principal > s.PrincipalPaid);
				}

				if (calcItem != null) {

					// get late items (including current one)
					var lateItems = this.schedule.Where(s => s.PlannedDate.Date <= calcItem.PlannedDate.Date && s.Principal > s.PrincipalPaid).ToList();

					// attach late amounts to the first non-late "StillToPay" schedule item
					if (lateItems.Count > 1) {

						// reset AmountDue of late schedules (including the nearby)
						lateItems.ForEach(s => s.AmountDue = 0);

						// set accrued interest to pay
						calcItem.Interest = Interest;
						// set accrued fees to pay
						calcItem.Fees = Fees;
						// update AmountDue = f+i+late principals
						calcItem.AmountDue = calcItem.Fees + calcItem.Interest + lateItems.Sum(p => p.Principal) - lateItems.Sum(p => p.PrincipalPaid);
					}

					NextEarlyPayment = calcItem.AmountDue; //calcItem.Principal - calcItem.PrincipalPaid + calcItem.Fees + Interest + Fees;
					NextEarlyPaymentSavedAmount = calcItem.Interest - earnedInterestForCalculationDate;
				}
			}

			DateTime calcItemPlannedDate = (calcItem != null) ? calcItem.PlannedDate.Date : this.calculationDateEventEnd.EventTime.Date; // TODO  check what to use of 

			foreach (NL_LoanSchedules s in this.schedule) {
				if (s.Principal <= s.PrincipalPaid) {
					s.LoanScheduleStatusID = (int)NLScheduleStatuses.Paid;
					s.ClosedTime = NowTime;
					s.AmountDue = 0;
				} else {
					// set balanced AmountDue for fiture unpaid schedules
					if (s.PlannedDate.Date > calcItemPlannedDate && s.LoanScheduleStatusID == (int)NLScheduleStatuses.StillToPay) {
						s.Interest = InterestBtwnDates(PreviousScheduleDate(s.PlannedDate).AddDays(1), s.PlannedDate, (s.Balance - s.PrincipalPaid));
						s.AmountDue = s.Fees + s.Interest - s.InterestPaid + s.Principal - s.PrincipalPaid;
					}
				}
			}

			// + unpaid arrangement fees
			var arrangementFees = this.distributedFeesList.Where(f => f.LoanFeeTypeID == (int)NLFeeTypes.ArrangementFee).ToList();
			TotalEarlyPayment = Fees + Interest + Principal + arrangementFees.Sum(f => f.Amount) - arrangementFees.Sum(f => f.PaidAmount);

			RolloverPayment = Interest; // to display separated: 1. rollover fee (from cong.varilables); 2. late fees till "rolover opportunity" (calc.date) == (open Fees)

			// set outstanding balance - if pay as scheduled, (total amounts due for calculation date)
			WorkingModel.Loan.Histories.ForEach(h => h.Schedule.ForEach(s => Balance += s.AmountDue));

			// copy result data to NL_Model
			WorkingModel.Fees = Fees;
			// earned interest on CalculationDate
			WorkingModel.Interest = Interest;
			// outstanding principal on CalculationDate
			WorkingModel.Principal = Principal;
			// accrued interest
			WorkingModel.RolloverPayment = RolloverPayment;
			// outstanging balance for continue as planned
			WorkingModel.Balance = Balance;
			// sum  le siluk miyadi - immediate loan closing
			WorkingModel.TotalEarlyPayment = TotalEarlyPayment;
			WorkingModel.NextEarlyPayment = NextEarlyPayment;
			WorkingModel.NextEarlyPaymentSavedAmount = NextEarlyPaymentSavedAmount;

			bool loanPaid = false;
			this.schedule.ForEach(s => loanPaid = ((s.Principal - s.PrincipalPaid) == 0 && (s.Interest - s.InterestPaid) == 0 && (s.FeesAssigned - s.FeesPaid) == 0));

			// All previous installments are paid? i.e. check late
			if (HasLatesTillCalculationDate()) {
				loanPaid = false;
				return;
			}

			/*// Today is a payment day
			if (CalculationDateIsPlannedDate()) {
				return;
			}*/

			if (loanPaid) {
				WorkingModel.Loan.LoanStatusID = (int)NLLoanStatuses.PaidOff;
				WorkingModel.Loan.DateClosed = NowTime;

				Log.Info("Loan {0} marked as 'Paid' at {1:d}", WorkingModel.Loan.LoanID, NowTime);
			}
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

		/// <exception cref="NoScheduleException">Condition. </exception>
		internal bool HasLatesTillCalculationDate() {
			if (this.schedule == null)
				throw new NoScheduleException();

			return this.schedule.FirstOrDefault(s => s.PlannedDate.Date <= CalculationDate.Date && s.LoanScheduleStatusID == (int)NLScheduleStatuses.Late) != null;
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
