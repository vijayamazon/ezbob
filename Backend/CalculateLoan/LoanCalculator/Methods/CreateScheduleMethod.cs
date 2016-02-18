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
		} // constructor

		/// <exception cref="NoInitialDataException">Condition. </exception>
		/// <exception cref="InvalidInitialAmountException">Condition. </exception>
		/// <exception cref="InvalidInitialInterestRateException">Condition. </exception>
		/// <exception cref="InvalidInitialRepaymentCountException">Condition. </exception>
		/// <exception cref="NoScheduleException">Condition. </exception>
		public virtual void Execute() {

			Calculator.currentHistory = WorkingModel.Loan.LastHistory();

			if (Calculator.currentHistory == null) {
				throw new NoInitialDataException();
			}

			if (Calculator.currentHistory.Amount <= 0) {
				throw new InvalidInitialAmountException(Calculator.currentHistory.Amount);
			}

			if (Calculator.currentHistory.InterestRate == 0) {
				throw new InvalidInitialInterestRateException(Calculator.currentHistory.InterestRate);
			}

			if (Calculator.currentHistory.RepaymentCount == 0) {
				throw new InvalidInitialRepaymentCountException(Calculator.currentHistory.RepaymentCount);
			}

			//int interestOnlyRepayments = history.InterestOnlyRepaymentCount; //  default is 0 \ezbob\Integration\PaymentServices\Calculators\LoanScheduleCalculator.cs line 35

			// RepaymentCount,  EventTime, InterestRate, RepaymentIntervalType 
			Calculator.currentHistory.SetDefaults();

			// LoanType, LoanFormulaID, RepaymentDate
			WorkingModel.Loan.SetDefaults();

			// TODO: LoanType balances \ezbob\Integration\PaymentServices\Calculators\LoanScheduleCalculator.cs line 44, 66, 68
			// decimal[] balances = loanType.GetBalances(total, Term, interestOnlyTerm).ToArray(); ???

			if (WorkingModel.Loan.LoanFormulaID == (int)NLLoanFormulas.EqualPrincipal) {
				CalculateScheduleEqualPrincipalFormula();
			} else if (WorkingModel.Loan.LoanFormulaID == (int)NLLoanFormulas.FixedPayment) {
				CalculateScheduleFixedPaymentFormula();
			}

			if (Calculator.currentHistory.Schedule == null) {
				Log.Error("No schedules created");
				throw new NoScheduleException();
			}

			AddFeesToScheduleItems();

			WorkingModel.Loan.Histories.Insert(0, Calculator.currentHistory);
		}

		/// <summary>
		/// calculates schedules according to keren shava formula (EqualPrincipal)
		/// </summary>
		private void CalculateScheduleEqualPrincipalFormula() {

			int interestOnlyRepayments = Calculator.currentHistory.InterestOnlyRepaymentCount;
			Calculator.currentHistory.Schedule.Clear();

			RepaymentIntervalTypes intervalType = (RepaymentIntervalTypes)Calculator.currentHistory.RepaymentIntervalTypeID;

			int principalPayments = Calculator.currentHistory.RepaymentCount - interestOnlyRepayments;
			decimal iPrincipal = Math.Floor(Calculator.currentHistory.Amount / principalPayments);
			decimal iFirstPrincipal = Calculator.currentHistory.Amount - iPrincipal * (principalPayments - 1);
			List<decimal> discounts = (WorkingModel.Offer != null && WorkingModel.Offer.DiscountPlan != null) ? WorkingModel.Offer.DiscountPlan : null;

			if (discounts != null) {
				discounts.ForEach(d => Log.Debug(d));

				int discountCount = discounts.Count;
				decimal balance = Calculator.currentHistory.Amount;
				Calculator.schedule = new List<NL_LoanSchedules>();

				// create Schedule 
				for (int i = 1; i <= Calculator.currentHistory.RepaymentCount; i++) {
					decimal principal = iPrincipal;

					if (i <= interestOnlyRepayments)
						principal = 0;
					else if (i == interestOnlyRepayments + 1)
						principal = iFirstPrincipal;

					decimal r = Calculator.currentHistory.InterestRate;
					if (i <= discountCount)
						r *= (1 + discounts[i - 1]);

					DateTime plannedDate = Calculator.AddRepaymentIntervals(i - 1, Calculator.currentHistory.RepaymentDate, intervalType).Date;
					DateTime prevScheduleDate = Calculator.PreviousScheduleDate(plannedDate, intervalType);

					NL_LoanSchedules item = new NL_LoanSchedules() {
						InterestRate = r,
						PlannedDate = plannedDate.Date,
						Principal = principal, // intervals' principal
						LoanScheduleStatusID = (int)NLScheduleStatuses.StillToPay,
						Position = i,
						Balance = balance, //local open principal, scheduled
						Interest = Calculator.InterestBtwnDates(plannedDate, prevScheduleDate, balance)
					};

					balance -= principal;

					Calculator.schedule.Add(item);

					Calculator.currentHistory.Schedule.Add(item);
				} // for
			}
		}

		/// <summary>
		/// calculates schedules according to shpitzer formula (FixedPayment)
		/// </summary>
		private void CalculateScheduleFixedPaymentFormula() {

			if (Calculator.currentHistory.PaymentPerInterval == 0m) {
				throw new NoPaymentPerIntervalException();
			}

			throw new NotImplementedException("FixedPayment formula not supported yet");

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

		/// <summary>
		/// calculate setup/servicing (arrangement) fees and attach it to schedule items PlannedDate
		/// </summary>
		private void AddFeesToScheduleItems() {

			// no fees defined
			if (WorkingModel.Offer.OfferFees == null || WorkingModel.Offer.OfferFees.Count == 0) {
				Log.Info("No offer fees defined");
				return;
			}

			int schedulesCount = Calculator.currentHistory.Schedule.Count;

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
				var fees = new SetupFeeCalculator(setupFee.Percent, brokerFeePercent)
					.CalculateTotalAndBroker(Calculator.currentHistory.Amount);

				decimal setupFeeAmount = fees.Total;
				WorkingModel.BrokerComissions = fees.Broker;

				//Log.Debug("setupFeeAmount: {0}, brokerComissions: {1}", setupFeeAmount, model.BrokerComissions);
				Log.Debug("setupFeeAmount: {0}", setupFeeAmount);

				WorkingModel.Loan.Fees.Add(
					new NL_LoanFees() {
						Amount = setupFeeAmount,
						AssignTime = Calculator.currentHistory.EventTime,
						Notes = "setup fee one-part",
						LoanFeeTypeID = (int)NLFeeTypes.SetupFee,
						CreatedTime = Calculator.NowTime,
						AssignedByUserID = 1
					});
			}

			// servicing fees - distribute according to timing of schedule items
			if (servicingFee != null) {
				var fees = new SetupFeeCalculator(servicingFee.Percent, brokerFeePercent)
					.CalculateTotalAndBroker(Calculator.currentHistory.Amount);

				decimal servicingFeeAmount = fees.Total;
				WorkingModel.BrokerComissions = fees.Broker;

				Log.Debug("servicingFeeAmount: {0}", servicingFeeAmount); // "spreaded" amount

				decimal iFee = Math.Floor(servicingFeeAmount / schedulesCount);
				decimal firstFee = (servicingFeeAmount - iFee * (schedulesCount - 1));

				foreach (NL_LoanSchedules s in Calculator.currentHistory.Schedule) {
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
