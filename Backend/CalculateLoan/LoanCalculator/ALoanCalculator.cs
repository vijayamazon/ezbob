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
	using Ezbob.Utils.Extensions;

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

		public abstract string Name { get; }

		// date of loan state calculation - input
		public DateTime CalculationDate { get; set; }

		protected static ASafeLog Log { get { return Library.Instance.Log; } }
		private bool writeToLog;

		public virtual bool WriteToLog {
			get { return this.writeToLog; }
			set { this.writeToLog = value; }
		} // WriteToLog

		public virtual NL_Model WorkingModel { get; private set; }

		/// <summary>
		/// Set common loan defaults: last history as a default current history
		/// InterestCalculationDateStart: first history +1 day
		/// InterestCalculationDateEnd: last history's schedule item date
		/// totalPrincipal: first history amount
		/// openPrincipal: first history amount
		/// </summary>
		private void SetCalculatorDefaults() {

			// set definitions from the last history
			currentHistory = WorkingModel.Loan.LastHistory();

			// start to cumulate interest one day after LoanIssueDate 
			InterestCalculationDateStart = WorkingModel.Loan.FirstHistory().EventTime.AddDays(1);

			// default: end to cumulate interest at same date the last schedule planned (loan maturity date)
			var lastSchedule = WorkingModel.Loan.LastHistory().Schedule.OrderBy(s => s.PlannedDate).LastOrDefault();

			InterestCalculationDateEnd = (lastSchedule != null) ? lastSchedule.PlannedDate : InterestCalculationDateEnd;

			totalPrincipal = WorkingModel.Loan.FirstHistory().Amount;

			openPrincipal = totalPrincipal;

			lastEventDate = InterestCalculationDateStart;
		}


		//	schedules count
		//protected int n { get; set; }

		// Loan State helpers

		/// <summary>
		/// events sequence related to loan. последовательность событий, относящихся к кредиту.
		/// </summary>
		public List<LoanEvent> events; // protected TODO for tests only public

		/// <summary>
		/// period for interest calculation
		/// </summary>
		public DateTime InterestCalculationDateStart { get; set; }
		public DateTime InterestCalculationDateEnd { get; set; }

		/// <summary>
		/// date of last action (event). Needed for interest calculation дата последнего действия. нужна для расчета процентов
		/// </summary>
		protected DateTime lastEventDate { get; set; }

		protected List<NL_LoanSchedules> schedule = new List<NL_LoanSchedules>();

		protected List<NL_LoanFees> feesToPay = new List<NL_LoanFees>();

		private NL_LoanHistory currentHistory { get; set; }

		// A, i.e. total principal of loan == initialAmount
		protected decimal totalPrincipal { get; set; }
		// total fees assigned to loan
		protected decimal totalFees { get; set; }

		// earned interest till current event including
		protected decimal earnedInterest { get; set; }

		// Loan state public data

		/// <summary>
		/// Money that really have customer. Loan balance without interest and fees. деньги, которые реально находятся у клиента на руках. баланс кредита без процентов и fee. 
		/// </summary>
		protected decimal openPrincipal { get; set; }

		/// <summary>
		/// interest to paid
		/// </summary>
		protected decimal openInterest { get; set; }

		/// <summary>
		/// fees to paid
		/// </summary>
		protected decimal openFees { get; set; }

		// ### Loan state public data

		/// <summary>
		/// interest paid on CalculationDate
		/// </summary>
		protected decimal paidInterest { get; set; }
		/// <summary>
		/// principal paid on CalculationDate
		/// </summary>
		protected decimal paidPrincipal { get; set; }
		/// <summary>
		/// fees paid on CalculationDate
		/// </summary>
		protected decimal paidFees { get; set; }

		/// <summary>
		/// helper holds principal paid until t (some event time)
		/// </summary>
		protected decimal currentPaidPrincipal { get; set; }
		/// <summary>
		/// helper holds fees paid until t (some event time)
		/// </summary>
		protected decimal currentPaidFees { get; set; }


		/// <summary>
		/// holds interest paid until t (some event time)
		/// </summary>
		protected decimal currentPaidInterest { get; set; }

		/// <summary>
		/// hold payments registered for fees, updated during events looping
		/// </summary>
		protected List<NL_LoanFeePayments> feesPaymentsList = new List<NL_LoanFeePayments>();

		/// <summary>
		/// hold payments registered for schedules, updated during events looping
		/// </summary>
		protected List<NL_LoanSchedulePayments> schedulesPaymentsList = new List<NL_LoanSchedulePayments>();

		/// <summary>
		/// all late fees (not servicing/not arrangement) 
		/// </summary>
		protected List<NL_LoanFees> lateFeesList = new List<NL_LoanFees>();

		/// <summary>
		/// all distributed on time fees (servicing|arrangement) 
		/// </summary>
		protected List<NL_LoanFees> distributedFeesList = new List<NL_LoanFees>();

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
		public virtual void CreateSchedule() {
			new CreateScheduleMethod(this, WorkingModel).Execute();
		} // CreateScheduleAndPlan

		/*
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
		public virtual decimal CalculateApr(DateTime? aprDate = null, double? calculationAccuracy = null, ulong? maxIterationCount = null) {
			var method = new CalculateAprMethod(this, aprDate, WriteToLog);

			if (calculationAccuracy.HasValue)
				method.CalculationAccuracy = calculationAccuracy.Value;

			if (maxIterationCount.HasValue)
				method.MaxIterationLimit = maxIterationCount.Value;

			return method.Execute();
		} // CalculateApr


		public DateTime AddRepaymentIntervals(int intervals, DateTime issuedTime, RepaymentIntervalTypes intervalType = RepaymentIntervalTypes.Month) {
			return (intervalType == RepaymentIntervalTypes.Month) ? issuedTime.AddMonths(intervals) : issuedTime.AddDays(intervals * (int)intervalType);
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
		/// During changing customer's status to "bad", new freeze interest interval create, so "bad statuses" included to freezes
		/// </summary>
		/// <param name="theDate"></param>
		/// <exception cref="NoInstallmentFoundException"></exception>
		/// <returns>Daily interest rate with freeze interest check.</returns>
		public decimal DailyInterestRateForDate(DateTime theDate) {
			// the date is between?
			var freezePeriod = WorkingModel.Loan.FreezeInterestIntervals.FirstOrDefault(fr => fr.StartDate >= theDate.Date && theDate.Date <= fr.EndDate && fr.ActivationDate <= theDate.Date);
			if (freezePeriod != null) {
				Log.Debug("freezePeriod: {0}, theDate: {1}", freezePeriod, theDate);
				return 0;
			}
			// find relevant schedule (theDate inside it)
			var scheduleItem = this.schedule.FirstOrDefault(s => PreviousScheduleDate(s.PlannedDate.Date, (RepaymentIntervalTypes)currentHistory.RepaymentIntervalTypeID) >= theDate.Date && theDate.Date <= s.PlannedDate.Date);
			if (scheduleItem == null)
				throw new NoInstallmentFoundException(theDate);

			// dr' = r/daysDiff
			decimal dr = AverageDailyInterestRate(scheduleItem.InterestRate, scheduleItem.PlannedDate);
			//Log.Debug("DailyInterestRateForDate: theDate: {0}, dailyRate {1}", theDate, dr);
			return dr;
		}

		/// <summary> 
		/// Interest rate for a specific date, defined in schedule. If Schedule exist, returns scheduled rate, otherwise returns history defined rate
		/// </summary>
		/// <returns></returns>
		/// <summary>
		/// common: Calculate total interest to pay at specific event date, during looping through ordered events list
		/// calculate interest rate btwn 2 nearby events in the list day by day; add interest calculated to earnedInterest; set currentEvent date as lastEventDate
		/// </summary>
		/// <param name="currentEvent"></param>
		/// <exception cref="NoInstallmentFoundException">Condition. </exception>
		public virtual decimal GetInterestBtwnEvents(LoanEvent currentEvent) {

			int daysDiff = currentEvent.Date.Subtract(lastEventDate.Date).Days;
			if (daysDiff <= 0)
				return 0;

			decimal interestForPeriod = 0;
			DateTime start = lastEventDate.Date; // lastEventAdjusted.Date;
			DateTime rateDate = start.Date;

			Log.Debug("==========lastEventDate: {0}, currentEvent: {1},  days: {2}, rateDate: {3}", lastEventDate, currentEvent, daysDiff, rateDate);

			//	interest for period = dr'(t)*p'(t)

			for (int i = 1; i <= daysDiff; i++) {
				decimal dailyInterestRate = DailyInterestRateForDate(rateDate); //	daily interest  dr' 
				decimal interest = openPrincipal * dailyInterestRate; //openPrincipal for t'
				interestForPeriod += interest;
				rateDate = start.AddDays(i);

				Log.Info("---------------------rateDate: {0} interest: {1} openPrincipal: {2}, i={3}", rateDate, interest, openPrincipal, i);
			}

			return interestForPeriod;
		}



		/// <summary>
		/// Initialize loan events list and common loan data vars
		/// </summary>
		private void Init() {

			List<LoanEvent> historiesEvents = new List<LoanEvent>();
			List<LoanEvent> feesEvents = new List<LoanEvent>();
			List<LoanEvent> paymentEvents = new List<LoanEvent>();
			List<LoanEvent> schedulesEvents = new List<LoanEvent>();

			foreach (var h in WorkingModel.Loan.Histories) {
				historiesEvents.Add(new LoanEvent(new DateTime(h.EventTime.Year, h.EventTime.Month, h.EventTime.Day), h));
				h.Schedule.ForEach(e => schedulesEvents.Add(new LoanEvent(new DateTime(e.PlannedDate.Year, e.PlannedDate.Month, e.PlannedDate.Day), e)));
				h.Schedule.ForEach(s => this.schedule.Add(s));
			}

			foreach (var e in WorkingModel.Loan.Fees) {
				feesEvents.Add(new LoanEvent(new DateTime(e.AssignTime.Year, e.AssignTime.Month, e.AssignTime.Day), e));
				totalFees += e.Amount;

				if (e.LoanFeeTypeID != (int)NLFeeTypes.ServicingFee && e.LoanFeeTypeID != (int)NLFeeTypes.ArrangementFee && (e.DisabledTime == null && e.DeletedByUserID == null))
					this.lateFeesList.Add(e);

				if ((e.LoanFeeTypeID == (int)NLFeeTypes.ServicingFee || e.LoanFeeTypeID == (int)NLFeeTypes.ArrangementFee) && (e.DisabledTime == null && e.DeletedByUserID == null))
					this.distributedFeesList.Add(e);
			}

			foreach (var e in WorkingModel.Loan.Payments) {
				// events
				paymentEvents.Add(new LoanEvent(new DateTime(e.PaymentTime.Year, e.PaymentTime.Month, e.PaymentTime.Day), e));

				// paid schedules - interest
				paidInterest += e.SchedulePayments.Sum(p => p.InterestPaid);

				// paid schedules - principal
				paidPrincipal += e.SchedulePayments.Sum(p => p.PrincipalPaid);

				// paif fees
				paidFees += e.FeePayments.Sum(p => p.Amount);

				// fees payments list
				e.FeePayments.ForEach(fp => this.feesPaymentsList.Add(fp));

				// schedules payments list
				e.SchedulePayments.ForEach(sp => this.schedulesPaymentsList.Add(sp));
			}

			// combine ordered events list
			this.events = historiesEvents.Union(schedulesEvents).Union(paymentEvents).Union(feesEvents)
				// .Union(rollOverEvents) TODO add rolovers // .Union(reschedules) TODO add reschedules
				.OrderBy(e => e.Date).ThenBy(e => e.Priority).ToList();

			// clear null events - nulls should not exists in partial events lists
			this.events.RemoveAll(e => e == null);

			this.events.ForEach(e => Log.Debug(e));

			lastEventDate = InterestCalculationDateStart;

			HandleEvents();
		}


		private void HandleEvents() {

			lastEventDate = InterestCalculationDateStart;

			Log.Debug("---------------------HandleEvents------------------------");

			foreach (var e in this.events) {

				Log.Debug("=={0}", e);

				e.InterestForPeriod = GetInterestBtwnEvents(e);
				earnedInterest += e.InterestForPeriod;

				lastEventDate = e.Date;

				Log.Debug("======{0} lastEventDate: {1}, earnedInterest: {2}, e.InterestForPeriod {3}", e, lastEventDate, earnedInterest, e.InterestForPeriod);

				switch (e.GetTypeString()) {

				case "History":
					currentHistory = e.History;
					e.CurrentHistory = currentHistory;
					HandleHistoryEvent(e.History);
					//Log.Debug("HistoryEvent lastEventDate {0}, earnedInterest: {1}", lastEventDate, earnedInterest);
					break;

				case "Installment":
					e.CurrentHistory = currentHistory;
					HandleInstallmentEvent(e.Installment);
					//Log.Debug("InstallmentEvent lastEventDate: {0}, earnedInterest: {1}", lastEventDate, earnedInterest);
					break;

				case "Fee":
					e.CurrentHistory = currentHistory;
					HandleFeeEvent(e.Fee);
					//Log.Debug("FeeEvent lastEventDate: {0}, earnedInterest: {1}", lastEventDate, earnedInterest);
					break;

				case "Payment":
					e.CurrentHistory = currentHistory;
					HandlePaymentEvent(e.Payment);
					//Log.Debug("PaymentEvent lastEventDate: {0}, earnedInterest: {1}", lastEventDate, earnedInterest);
					break;

				case "Rollover":
					e.CurrentHistory = currentHistory;
					HandleRolloverEvent(e.Rollover);
					//Log.Debug("RolloverEvent lastEventDate: {0}, earnedInterest: {1}", lastEventDate, earnedInterest);
					break;

				case "Action":
					e.CurrentHistory = currentHistory;
					HandleActionEvent(e.Action);
					//Log.Debug("ActionEvent lastEventDate: {0}, earnedInterest: {1}", lastEventDate, earnedInterest);
					break;

				default:
					Log.Debug("Unknown event type: {0}", e);
					break;
				}
			}
		} // HandleEvents



		/// <summary>
		/// 1. cumulate paid fees into currentPaidFees
		/// 2. update openFees
		/// 1. cumulate paid principal into currentPaidPrincipal
		/// 2. update openPrincipal (p'(t) = A - sum(p(t')) where p'(t) - current open principal; A - initial amount taken, paid(t') - principal paid at this event; sum(p(t')) - principal paid untill this event 
		///
		/// Payments:
		/// 1. paying fees 
		/// 2. paying interest 
		/// 3. paying rollover TODO phase 2
		/// 4. paying principal
		/// </summary>
		/// <param name="payment"></param>
		private void HandlePaymentEvent(NL_Payments payment) {

			// cumulate fees paid on the payment to currentPaidFees
			currentPaidFees += payment.FeePayments.Sum(p => p.Amount);
			// update openFees (fees to pay)
			openFees = totalFees - currentPaidFees;

			// cumulate principal paid on the payment to currentPaidPrincipal
			currentPaidPrincipal += payment.SchedulePayments.Sum(p => p.PrincipalPaid); // TODO change to use this.schedulesPaymentsList
			// update openPrincipal (principal to pay)
			openPrincipal = totalPrincipal - currentPaidPrincipal;

			Log.Debug("totalPrincipal: {0}, openPrincipal: {1}, currentPaidPrincipal: {2}", totalPrincipal, openPrincipal, currentPaidPrincipal);

			// rest of money that can be distributed to pay loan
			decimal assignAmount = payment.Amount - payment.FeePayments.Sum(p => p.Amount) - payment.SchedulePayments.Sum(p => p.InterestPaid) - payment.SchedulePayments.Sum(p => p.PrincipalPaid);

			// 1. pay fees
			assignAmount = PayFees(payment, assignAmount);

			Log.Debug("payment balance after 'pay fees' {0}", assignAmount);

			// 2. pay interest

			// cumulate interest paid on the payment to currentPaidInterest
			//currentPaidInterest += payment.SchedulePayments.Sum(p => p.InterestPaid);

			assignAmount = PaySchedules(payment, assignAmount);

			// pay principal

		}

		private decimal PaySchedules(NL_Payments payment, decimal assignAmount) {

			if (assignAmount == 0) {
				Log.Info("====Amount to assign: {0}", assignAmount);
				return assignAmount;
			}

			// interest paid till this event time
			var schedulesPaidTillEvent = this.schedulesPaymentsList.Where(sp => sp.PaymentDate <= lastEventDate);

			// paid interest
			decimal interestPaidTillEvent = schedulesPaidTillEvent.Sum(sp => sp.InterestPaid);

			Log.Debug("interestPaidTillEvent: {0}, earnedInterest: {1}, assignAmount: {2}", interestPaidTillEvent, earnedInterest, assignAmount);

			// openInterest  = earned interest - paid interest (till event)

			// sch(planned-paid >0)
			IEnumerable<NL_LoanSchedules> unpaidSchedules = from s in this.schedule
															join p in this.schedulesPaymentsList on s.LoanScheduleID equals p.LoanScheduleID into g
															from x in g.DefaultIfEmpty()
															where x == null || s.Principal < x.PrincipalPaid // not paid at all or completely
															orderby s.PlannedDate
															select s;

			// interest to pay till event - interest paid till event
			decimal currentOpenInterest = earnedInterest - interestPaidTillEvent;

			// loop throught unpaid ir partial paid schedules
			foreach (var x in ForeachExt.WithIndex(unpaidSchedules)) {

				if (assignAmount <= 0) {
					Log.Debug("Payment {0} assigned to loan completely", payment);
					return 0;
				}

				NL_LoanSchedules s = x.Value;

				Log.Debug("unpaid schedule :{0}", s);

				decimal iAmount = Math.Min(assignAmount, currentOpenInterest);

				assignAmount -= iAmount;

				decimal principalbalance =  s.Principal - this.schedulesPaymentsList.Where(sp => sp.LoanScheduleID == s.LoanScheduleID).Sum(sp => sp.PrincipalPaid);

				decimal pAmount = Math.Min(assignAmount, principalbalance);

				assignAmount -= pAmount;

				// first unpaid or partially paid schedule item, add all interest to it
				if (x.Index == 0 && currentOpenInterest > 0) {
					currentOpenInterest = 0;
				}

				NL_LoanSchedulePayments schpayment = new NL_LoanSchedulePayments() {
					InterestPaid = iAmount, // TODO caclulate something real
					LoanScheduleID = s.LoanScheduleID,
					PaymentID = payment.PaymentID,
					NewSchedulePayment = true,
					PaymentDate = payment.PaymentTime,
					PrincipalPaid = pAmount
				};

				this.schedulesPaymentsList.Add(schpayment);
			} // foreach

			// unpaid schedules untill LastEventDate including
			return assignAmount;
		}

		/// <summary>
		/// pay fees
		/// </summary>
		/// <param name="payment"></param>
		/// <param name="assignAmount"></param>
		/// <returns>available money of the payment</returns>
		private decimal PayFees(NL_Payments payment, decimal assignAmount) {

			if (assignAmount == 0) {
				Log.Info("----No amount to assign: {0}", assignAmount);
				return assignAmount;
			}

			if (payment == null) {
				Log.Info("----No payment defined");
				return assignAmount;
			}

			// all unpaid late fees (not setup|servicing|arrangement), not related to calculation date
			assignAmount = AssignPaymentToFees(this.lateFeesList, payment, assignAmount);

			// unpaid servicing|arrangement untill LastEventDate including
			var distributedfeesUntillThisEvent = this.distributedFeesList.Where(f => f.AssignTime <= payment.PaymentTime);

			assignAmount = AssignPaymentToFees(distributedfeesUntillThisEvent.ToList(), payment, assignAmount);

			return assignAmount;
		}

		/// <exception cref="ArgumentNullException"><paramref name="source" /> or <paramref name="selector" /> is null.</exception>
		/// <exception cref="OverflowException">The sum is larger than <see cref="F:System.Decimal.MaxValue" />.</exception>
		public decimal AssignPaymentToFees(List<NL_LoanFees> feesList, NL_Payments payment, decimal assignAmount) {

			if (feesList.Count == 0) {
				Log.Info("No fees to pay on payment {0}", payment);
				return assignAmount;
			}

			if (payment == null) {
				Log.Info("+++No payment defined");
				return assignAmount;
			}

			foreach (NL_LoanFees f in feesList) {

				if (assignAmount == 0) {
					Log.Info("+++No amount to assign: {0}", assignAmount);
					return assignAmount;
				}

				decimal paidAmount = this.feesPaymentsList.Where(fp => fp.LoanFeeID == f.LoanFeeID).Sum(fp => fp.Amount);
				decimal paymentbalance = f.Amount - paidAmount;

				// fees not paid completely
				if (paymentbalance > 0) {

					decimal amount = Math.Min(assignAmount, paymentbalance);

					this.feesPaymentsList.Add(new NL_LoanFeePayments() {
						Amount = amount, // maximal possible money to pay the fee
						LoanFeeID = f.LoanFeeID,
						LoanFeePaymentID = payment.PaymentID,
						NewFeePayment = true // mark as "new" - for DB
					});

					// decrease paid amount from available payment amount
					assignAmount -= amount;
					var nlLoanFeePayments = this.feesPaymentsList.LastOrDefault();
					Log.Debug("fee to pay: {0}, feepayment: {1}", f, nlLoanFeePayments != null ? nlLoanFeePayments.ToString() : "");
				}
			}

			return assignAmount;
		}



		private void HandleInstallmentEvent(NL_LoanSchedules item) {
			//this.processedInstallments.Add(item);
			Log.Debug("HandleInstallmentEvent NotImplementedException");
		}

		private void HandleHistoryEvent(NL_LoanHistory history) {
			Log.Debug("HandleHistoryEvent NotImplementedException");
		}

		private void HandleFeeEvent(NL_LoanFees fee) {
			// el: if loan PaidOff, mark the charge as Expired
			// el: Arrangement and servicing fees should be treated different
			//if (_loan.Status == LoanStatus.PaidOff) {
			//	charge.State = "Expired";
			//	return;
			//}
			Log.Debug("HandleFeeEvent not implemented {0}", fee);
		}

		private void HandleRolloverEvent(NL_LoanRollovers rollover) {
			Log.Debug("HandleRolloverEvent not implemented {0}", rollover);
		}

		private void HandleActionEvent(object p) {
			Log.Debug("ActionEven NotImplementedException");
		}



	} // class ALoanCalculator
} // namespace
