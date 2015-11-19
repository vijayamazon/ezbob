namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Methods {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DbConstants;
	using Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using PaymentServices.Calculators;

	internal class CreateScheduleMethod : AMethod {

		public CreateScheduleMethod(ALoanCalculator calculator)
			: base(calculator, false) {

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
		/// <exception cref="OverflowException"><paramref /> represents a number that is less than <see cref="F:System.Decimal.MinValue" /> or greater than <see cref="F:System.Decimal.MaxValue" />. </exception>
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

			//int interestOnlyRepayments = history.InterestOnlyRepaymentCount; //  default is 0 \ezbob\Integration\PaymentServices\Calculators\LoanScheduleCalculator.cs line 35

			// RepaymentCount,  EventTime, InterestRate, RepaymentIntervalType 
			history.SetDefaults();

			// LoanType, LoanFormulaID, RepaymentDate
			WorkingModel.Loan.SetDefaults();

			// TODO: LoanType balances \ezbob\Integration\PaymentServices\Calculators\LoanScheduleCalculator.cs line 44, 66, 68
			// decimal[] balances = loanType.GetBalances(total, Term, interestOnlyTerm).ToArray(); ???

			if (WorkingModel.Loan.LoanFormulaID == (int)NLLoanFormulas.EqualPrincipal) {
				CalculateScheduleEqualPrincipalFormula(history);
			} else if (WorkingModel.Loan.LoanFormulaID == (int)NLLoanFormulas.FixedPayment) {
				CalculateScheduleFixedPaymentFormula(history);
			}

			if (history.Schedule == null) {
				Log.Error("No schedules created");
				throw new NoScheduleException();
			}

			// set scheduled interests (based on balance) and amountDue
			Calculator.events = new List<LoanEvent>();
			LoanEvent hEvent = new LoanEvent(new DateTime(history.EventTime.Year, history.EventTime.Month, history.EventTime.Day), history);
			Calculator.events.Add(hEvent);
			history.Schedule.ForEach(e => Calculator.events.Add(new LoanEvent(new DateTime(e.PlannedDate.Year, e.PlannedDate.Month, e.PlannedDate.Day), e)));
			Calculator.BalanceBasedAmountDue(history.Amount);

			AddFeesToScheduleItems(history);
		}

		private void CalculateScheduleFixedPaymentFormula(NL_LoanHistory history) {

			throw new NotImplementedException("FixedPayment formula not supported yet");

			if (WorkingModel.Loan.LastHistory().PaymentPerInterval == 0m) {
				throw new NoPaymentPerIntervalException();
			}

			// http://www.hughcalc.org/formula.php
			// Finding the Number of Periods given a Payment, Interest and Loan Amount
			/*
			 * This formula previously was not explicit enough!! The 1/q factor in there was to convert the number of periods into years. For number of payments this must actually be left out.
			Many people have asked me how to find N (number of payments) given the payment, interest and loan amount. I didn't know the answer and in my calculators I find it by doing a binary search over the payment formula above. However, Gary R. Walo ( nenonen5@southeast.net) found the answer to the actual formula in the book: The Vest Pocket Real Estate Advisor by Martin Miles (Prentice Hall). Here is the corrected formula:
			n = - (LN(1-(B/m)*(r/q)))/LN(1+(r/q))
			# years = - 1/q * (LN(1-(B/m)*(r/q)))/LN(1+(r/q))

			Where:

			q = amount of annual payment periods
			r = interest rate
			B = principal
			m = payment amount
			n = amount payment periods
			LN = natural logarithm
			*/
		}

		private void CalculateScheduleEqualPrincipalFormula(NL_LoanHistory history) {

			int interestOnlyRepayments = history.InterestOnlyRepaymentCount;
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

				decimal r = (i <= discountCount) ? (history.InterestRate *= (1 + discounts[i - 1])) : history.InterestRate;

				DateTime plannedDate = (i == 1) ? history.RepaymentDate.Date : Calculator.AddRepaymentIntervals(i, history.RepaymentDate, intervalType).Date;

				balance -= principal;

				history.Schedule.Add(new NL_LoanSchedules() {
						InterestRate = r,
						PlannedDate = plannedDate.Date,
						Principal = principal, // intervals' principal
						LoanScheduleStatusID = (int)NLScheduleStatuses.StillToPay,
						Position = i,
						Balance = balance		//local open principal, scheduled		
					});
			} // for
		}

		/// <summary>
		/// calculate setup/servicing (arrangement) fees and attach it to schedule items PlannedDate
		/// </summary>
		/// <param name="history"></param>
		private void AddFeesToScheduleItems(NL_LoanHistory history) {

			// no fees defined
			if (WorkingModel.Offer.OfferFees == null || WorkingModel.Offer.OfferFees.Count == 0) {
				Log.Info("No offer fees defined");
				return;
			}

			int schedulesCount = history.Schedule.Count;

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
