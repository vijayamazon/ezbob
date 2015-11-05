namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Methods {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DbConstants;
	using Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using PaymentServices.Calculators;

	internal class CreateScheduleMethodCopy : AMethod {


		public CreateScheduleMethodCopy(ALoanCalculator calculator): base(calculator, false) {

			//if (loanModel == null)
			//	throw new NoInitialDataException();

			//if (loanModel.Loan == null)
			//	throw new NoInitialDataException();

		} // constructor

		/// <exception cref="NoInitialDataException">Condition. </exception>
		/// <exception cref="InvalidInitialAmountException">Condition. </exception>
		/// <exception cref="InvalidInitialInterestRateException">Condition. </exception>
		/// <exception cref="InvalidInitialRepaymentCountException">Condition. </exception>
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

				decimal dailyInterestRate = Calculator.AverageDailyInterestRate(r, plannedDate); // dr' = r/daysDiff

				int daysDiff = plannedDate.Date.Subtract(Calculator.PreviousScheduleDate(plannedDate)).Days;

				decimal interest = dailyInterestRate * balance * daysDiff; //	r/daysDiff*balance*daysDiff ;  if r in percents => /100; 

				balance -= principal;

				history.Schedule.Add(
					new NL_LoanSchedules() {
						InterestRate = r,
						PlannedDate = plannedDate.Date,
						Principal = principal, // intervals' principal
						LoanScheduleStatusID = (int)NLScheduleStatuses.StillToPay,
						Position = i,
						Balance = balance, //open principal
						Interest = interest, //ei
						AmountDue = (interest + principal)
					});
			} // for

			if (history.Schedule == null) {
				Log.Debug("No schedules defined");
				return;
			}

			int schedulesCount = history.Schedule.Count;

			// no fees defined
			if (WorkingModel.Offer.OfferFees == null || WorkingModel.Offer.OfferFees.Count == 0) {
				Log.Debug("No offer fees defined");
				return;
			}

			DateTime nowTime = DateTime.UtcNow;

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
						CreatedTime = nowTime,
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
							CreatedTime = nowTime,
							AssignedByUserID = 1
						});

					s.FeesAmount += feeAmount;
					s.AmountDue += s.FeesAmount;

					schedulesCount = 0; // reset count, because it used as firstFee/iFee flag
				}
			}
		}
		
	} // class CreateScheduleMethod
} // namespace
