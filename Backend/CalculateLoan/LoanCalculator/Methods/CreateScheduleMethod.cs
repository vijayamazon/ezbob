namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Methods {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DbConstants;
	using Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using PaymentServices.Calculators;

	internal class CreateScheduleMethod : AMethod {
		
		public CreateScheduleMethod(ALoanCalculator calculator): base(calculator, false) {

			//if (loanModel == null)
			//	throw new NoInitialDataException();

			//if (loanModel.Loan == null)
			//	throw new NoInitialDataException();

		} // constructor

		/// <exception cref="NoInitialDataException">Condition. </exception>
		/// <exception cref="InvalidInitialAmountException">Condition. </exception>
		/// <exception cref="InvalidInitialInterestRateException">Condition. </exception>
		/// <exception cref="InvalidInitialRepaymentCountException">Condition. </exception>
		/// <exception cref="NoScheduleException">Condition. </exception>
		/// <exception cref="NoInstallmentFoundException">Condition. </exception>
		/// <exception cref="InvalidCastException"><paramref /> cannot be cast to the element type of the current <see cref="T:System.Array" />.</exception>
		public virtual void Execute() {

			NL_LoanHistory history = WorkingModel.Loan.LastHistory();

			if (history == null) {
				throw new NoInitialDataException();
			}

			if (history.Amount <= 0) {
				throw new InvalidInitialAmountException(history.Amount);
			}

			if (history.InterestRate == 0) {
				throw new InvalidInitialInterestRateException(history.InterestRate);
			}

			if (history.RepaymentCount == 0) {
				throw new InvalidInitialRepaymentCountException(history.RepaymentCount);
			}

			int interestOnlyRepayments = history.InterestOnlyRepaymentCount; //  default is 0 \ezbob\Integration\PaymentServices\Calculators\LoanScheduleCalculator.cs line 35
			
			// RepaymentCount,  EventTime, InterestRate, RepaymentIntervalType 
			history.SetDefaults();

			// LoanType, LoanFormulaID, RepaymentDate
			WorkingModel.Loan.SetDefaults();

			// TODO: LoanType balances \ezbob\Integration\PaymentServices\Calculators\LoanScheduleCalculator.cs line 44, 66, 68
			// decimal[] balances = loanType.GetBalances(total, Term, interestOnlyTerm).ToArray(); ???

			history.Schedule.Clear();

			RepaymentIntervalTypes intervalType = (RepaymentIntervalTypes)history.RepaymentIntervalTypeID;

			int principalPayments = history.RepaymentCount - interestOnlyRepayments;
			decimal iPrincipal = Math.Floor(history.Amount / principalPayments);
			decimal iFirstPrincipal = history.Amount - iPrincipal * (principalPayments - 1);
			List<decimal> discounts = WorkingModel.Offer.DiscountPlan;
			int discountCount = discounts.Count;
			decimal balance = history.Amount;
	
			// create Schedule 
			for (int i = 1; i <= history.RepaymentCount; i++) {

				decimal principal = iPrincipal;

				if (i <= interestOnlyRepayments)
					principal = 0;
				else if (i == interestOnlyRepayments + 1)
					principal = iFirstPrincipal;
				
				decimal r = (i <= discountCount) ? (history.InterestRate *= (1 + discounts[i-1])) : history.InterestRate;

				DateTime plannedDate = Calculator.AddRepaymentIntervals(i, history.EventTime, intervalType).Date;

				//decimal dailyInterestRate = Calculator.AverageDailyInterestRate(r, plannedDate); // dr' = r/daysDiff

				//int daysDiff = plannedDate.Date.Subtract(Calculator.PreviousScheduleDate(plannedDate)).Days;

				//decimal interest = dailyInterestRate * balance * daysDiff; //	r/daysDiff*balance*daysDiff ;  if r in percents => /100;

				balance -= principal;

				history.Schedule.Add(
					new NL_LoanSchedules() {
						InterestRate = r,
						PlannedDate = plannedDate.Date,
						Principal = principal, // intervals' principal
						LoanScheduleStatusID = (int)NLScheduleStatuses.StillToPay,
						Position = i,
						Balance = balance, //open principal, scheduled
					//	Interest = interest, //ei
					//	AmountDue = (interest + principal)
					});
			} // for

	
			if (history.Schedule == null) {
				Log.Error("No schedules created");
				throw new NoScheduleException();
			}

			// set scheduled interests (based on balance) and amountDue
			Calculator.BalanceBasedInterestCalculation = true;
			Calculator.schedule = history.Schedule;
			LoanEvent hEvent = new LoanEvent(new DateTime(history.EventTime.Year, history.EventTime.Month, history.EventTime.Day), history);
			Calculator.lastEvent = hEvent;
			int counter = 0;
			foreach (NL_LoanSchedules s in history.Schedule) {
				LoanEvent sEvent = new LoanEvent(new DateTime(s.PlannedDate.Year, s.PlannedDate.Month, s.PlannedDate.Day), s);
				decimal sBalance = s.Balance;
				s.Balance = counter == 0 ? Calculator.initialAmount : history.Schedule[counter-1].Balance; // use balance of previous item
				s.InterestScheduled = Calculator.InterestBtwnEvents(sEvent);
				//s.InterestScheduled = s.Interest;
				s.AmountDueScheduled = s.InterestScheduled + s.Principal;
				s.Balance = sBalance; // set balance back
				Calculator.lastEvent = sEvent;
				counter++;
			}
			
			int schedulesCount = history.Schedule.Count;

			// no fees defined
			if (WorkingModel.Offer.OfferFees == null || WorkingModel.Offer.OfferFees.Count == 0) {
				Log.Debug("No offer fees defined");
				return;
			}
	
			// extract offer-fees
			var offerFees = WorkingModel.Offer.OfferFees;

			// for now: only one-time or "spreaded" setup fees supported
			// add full fees 2.0 support later

			// don't create LoanFees if OfferFees Percent == 0 or AbsoluteAmount == 0
			var setupFee = offerFees.FirstOrDefault(f => f.LoanFeeTypeID == (int)NLFeeTypes.SetupFee && (f.Percent > 0 || f.AbsoluteAmount > 0));
			var servicingFee = offerFees.FirstOrDefault(f => f.LoanFeeTypeID == (int)NLFeeTypes.ServicingFee && (f.Percent > 0 || f.AbsoluteAmount > 0)); // equal to "setup spreaded"
			decimal? brokerFeePercent = WorkingModel.Offer.BrokerSetupFeePercent;

			// setup fee - add one NL_LoanFees entry 
			if (setupFee != null) {

				var feeCalculator = new SetupFeeCalculator(setupFee.Percent, brokerFeePercent);

				decimal setupFeeAmount = feeCalculator.Calculate(history.Amount);
				WorkingModel.BrokerComissions = feeCalculator.CalculateBrokerFee(history.Amount);

				//Log.Debug("setupFeeAmount: {0}, brokerComissions: {1}", setupFeeAmount, model.BrokerComissions);
				Log.Debug("setupFeeAmount: {0}", setupFeeAmount);

				WorkingModel.Loan.Fees.Add(
					new NL_LoanFees() {
						Amount = setupFeeAmount,
						AssignTime = history.EventTime,
						Notes = "setup fee one-part",
						LoanFeeTypeID = (int)NLFeeTypes.SetupFee,
						CreatedTime = Calculator.NowTime,
						AssignedByUserID = 1
					});
			}

			// servicing fees - distribute according to timing of schedule items
			if (servicingFee != null) {

				var feeCalculator = new SetupFeeCalculator(servicingFee.Percent, brokerFeePercent);

				decimal servicingFeeAmount = feeCalculator.Calculate(history.Amount);
				WorkingModel.BrokerComissions = feeCalculator.CalculateBrokerFee(history.Amount);

				Log.Debug("servicingFeeAmount: {0}", servicingFeeAmount); // "spreaded" amount

				decimal iFee = Math.Floor(servicingFeeAmount / schedulesCount);
				decimal firstFee = (servicingFeeAmount - iFee * (schedulesCount - 1));

				foreach (NL_LoanSchedules s in history.Schedule) {
					decimal feeAmount = (schedulesCount > 0) ? firstFee : iFee;
					WorkingModel.Loan.Fees.Add(
						new NL_LoanFees() {
							Amount = feeAmount,
							AssignTime = s.PlannedDate,
							Notes = "spread (servicing) fee",
							LoanFeeTypeID = (int)NLFeeTypes.ServicingFee,
							CreatedTime = Calculator.NowTime,
							AssignedByUserID = 1
						});

					s.Fees += feeAmount;
					s.AmountDue += s.Fees;

					schedulesCount = 0; // reset count, because it used as firstFee/iFee flag
				}
			}
		}
		
	} // class CreateScheduleMethod
} // namespace
