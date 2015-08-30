namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Methods {
	using System;
	using System.Linq;
	using DbConstants;
	using Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using PaymentServices.Calculators;

	internal class CreateScheduleMethod : AMethod {

		/// <exception cref="NoInitialDataException">Condition. </exception>
		public CreateScheduleMethod(ALoanCalculator calculator, NL_Model loanModel)
			: base(calculator, false) {

			if (loanModel == null)
				throw new NoInitialDataException();

			this.loanModel = loanModel;

		} // constructor

		/// <exception cref="NoInitialDataException">Condition. </exception>
		/// <exception cref="InvalidInitialAmountException">Condition. </exception>
		/// <exception cref="InvalidInitialInterestRateException">Condition. </exception>
		/// <exception cref="InvalidInitialRepaymentCountException">Condition. </exception>
		/// <exception cref="InvalidInitialInterestOnlyRepaymentCountException">Condition. </exception>
		public virtual void Execute() {

			//NL_LoanHistory lastHistory = this.loanModel.Histories.OrderBy(h => h.EventTime).LastOrDefault();
			NL_LoanHistory lastHistory = this.loanModel.Loan.LastHistory();

			if (lastHistory == null) {
				throw new NoInitialDataException();
			}

			if (lastHistory.Amount <= 0)
				throw new InvalidInitialAmountException(lastHistory.Amount);

			if (lastHistory.InterestRate <= 0)
				throw new InvalidInitialInterestRateException(lastHistory.InterestRate);

			if (lastHistory.RepaymentCount < 1)
				throw new InvalidInitialRepaymentCountException(lastHistory.RepaymentCount);

			int interestOnlyRepaymentCount = lastHistory.InterestOnlyRepaymentCount;
			if ((interestOnlyRepaymentCount < 0) || (interestOnlyRepaymentCount >= lastHistory.RepaymentCount)) {
				throw new InvalidInitialInterestOnlyRepaymentCountException(interestOnlyRepaymentCount, lastHistory.RepaymentCount);
			}

			DateTime nowTime = DateTime.UtcNow;
			lastHistory.Schedule.Clear();

			RepaymentIntervalTypes intervalType = (RepaymentIntervalTypes)lastHistory.RepaymentIntervalTypeID;

			int principalPayments = lastHistory.RepaymentCount - interestOnlyRepaymentCount;
			decimal otherPayments = Math.Floor(lastHistory.Amount / principalPayments);
			decimal firstPayment = lastHistory.Amount - otherPayments * (principalPayments - 1);
			int discountPayments = this.loanModel.DiscountPlan.Count;
			decimal balance = lastHistory.Amount;

			// create Schedule - by initial or re-scheduling data (last history)
			for (int i = 1; i <= lastHistory.RepaymentCount; i++) {

				decimal principal = otherPayments;

				if (i <= interestOnlyRepaymentCount)
					principal = 0;
				else if (i == interestOnlyRepaymentCount + 1)
					principal = firstPayment;

				balance -= principal;

				decimal r = (i <= discountPayments) ? (lastHistory.InterestRate *= 1 + this.loanModel.DiscountPlan[i]) : lastHistory.InterestRate;

				DateTime plannedDate = Calculator.AddRepaymentIntervals(i, lastHistory.EventTime, intervalType).Date;

				decimal dailyInterestRate = Calculator.CalculateDailyInterestRate(r, plannedDate); // r'

				decimal interest = balance * dailyInterestRate;

				lastHistory.Schedule.Add(
					new NL_LoanSchedules() {
						InterestRate = r,
						PlannedDate = plannedDate,
						Principal = principal,
						LoanScheduleStatusID = (int)NLScheduleStatuses.StillToPay,
						Position = i,
						Balance = balance,
						Interest = interest,
						AmountDue = (interest + principal)
					});
			} // for

			if (lastHistory.Schedule == null) {
				Log.Debug("No schedules defined");
				return;
			}

			int schedulesCount = lastHistory.Schedule.Count;

			// no fees defined
			if (this.loanModel.Offer.OfferFees == null || this.loanModel.Offer.OfferFees.Count == 0) {
				Log.Debug("No offer fees defined");
				return;
			}

			// extract offer-fees
			var offerFees = this.loanModel.Offer.OfferFees;

			// for now: only one-time or "spreaded" setup fees supported
			// add full fees 2.0 support later

			var setupFee = offerFees.FirstOrDefault(f => f.LoanFeeTypeID == (int)NLFeeTypes.SetupFee);
			var servicingFee = offerFees.FirstOrDefault(f => f.LoanFeeTypeID == (int)NLFeeTypes.ServicingFee); // equal to "setup spreaded"
			decimal? brokerFeePercent = this.loanModel.Offer.BrokerSetupFeePercent;

			// setup fee - add one NL_LoanFees entry 
			if (setupFee != null) {

				var feeCalculator = new SetupFeeCalculator(setupFee.Percent, brokerFeePercent);

				decimal setupFeeAmount = feeCalculator.Calculate(lastHistory.Amount);
				this.loanModel.BrokerComissions = feeCalculator.CalculateBrokerFee(lastHistory.Amount);

				//Log.Debug("setupFeeAmount: {0}, brokerComissions: {1}", setupFeeAmount, model.BrokerComissions);
				Log.Debug("setupFeeAmount: {0}", setupFeeAmount);

				this.loanModel.Loan.Fees.Add(
					new NL_LoanFees() {
						Amount = setupFeeAmount,
						AssignTime = lastHistory.EventTime,
						Notes = "setup fee one-part",
						LoanFeeTypeID = (int)NLFeeTypes.SetupFee,
						CreatedTime = nowTime,
						AssignedByUserID = 1
					});
			}

			// servicing fees - distribute according to timing of schedule items
			if (servicingFee != null) {

				var feeCalculator = new SetupFeeCalculator(servicingFee.Percent, brokerFeePercent);

				decimal servicingFeeAmount = feeCalculator.Calculate(lastHistory.Amount);
				this.loanModel.BrokerComissions = feeCalculator.CalculateBrokerFee(lastHistory.Amount);

				Log.Debug("servicingFeeAmount: {0}", servicingFeeAmount); // "spreaded" amount

				decimal iFee = Math.Floor(servicingFeeAmount / schedulesCount);
				decimal firstFee = (servicingFeeAmount - iFee * (schedulesCount - 1));

				foreach (NL_LoanSchedules s in lastHistory.Schedule) {
					decimal feeAmount = (schedulesCount > 0) ? firstFee : iFee;
					this.loanModel.Loan.Fees.Add(
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







		private readonly NL_Model loanModel;




		//public CreateScheduleMethod(ALoanCalculator calculator, NL_Model loanModel)
		//	: base(calculator, false) {
		//	if (loanModel == null)
		//		throw new NoInitialDataException();

		//	this.amount = loanModel.InitialAmount;
		//	if (this.amount <= 0)
		//		throw new InvalidInitialAmountException(this.amount);

		//	this.interestRate = loanModel.InitialInterestRate;
		//	if (this.interestRate <= 0)
		//		throw new InvalidInitialInterestRateException(this.interestRate);

		//	this.repaymentCount = loanModel.InitialRepaymentCount;
		//	if (this.repaymentCount < 1)
		//		throw new InvalidInitialRepaymentCountException(this.repaymentCount);

		//	this.interestOnlyRepaymentCount = loanModel.Loan.InterestOnlyRepaymentCount;
		//	if ((this.interestOnlyRepaymentCount < 0) || (this.interestOnlyRepaymentCount >= this.repaymentCount)) {
		//		throw new InvalidInitialInterestOnlyRepaymentCountException(
		//			this.interestOnlyRepaymentCount,
		//			this.repaymentCount
		//		);
		//	} // if

		//	this.repaymentIntervalType = (RepaymentIntervalTypes)loanModel.InitialRepaymentIntervalTypeID;

		//	this.issuedTime = loanModel.IssuedTime;

		//	this.discountPlan = new List<decimal>();

		//	if ((loanModel.DiscountPlan != null) && (loanModel.DiscountPlan.Length > 0))
		//		this.discountPlan.AddRange(loanModel.DiscountPlan);
		//} // constructor


		//public virtual void /* List<ScheduledItem> */ Execute() {
		//	/*
		//	LoanHistory loan = WorkingModel.LoanHistory.Last();

		//	loan.Clear();

		//	loan.Amount = this.amount;
		//	loan.RepaymentCount = this.repaymentCount;
		//	loan.MonthlyInterestRate = this.interestRate;
		//	loan.RepaymentIntervalType = this.repaymentIntervalType;
		//	loan.IsFirst = WorkingModel.LoanHistory.Count == 1;

		//	if (loan.IsFirst) {
		//		WorkingModel.InterestOnlyRepayments = this.interestOnlyRepaymentCount;
		//		WorkingModel.DiscountPlan.Clear();
		//		WorkingModel.DiscountPlan.AddRange(this.discountPlan);
		//	} // if

		//	int principalRepaymentCount = this.repaymentCount - this.interestOnlyRepaymentCount;

		//	decimal otherPayments = Math.Floor(this.amount / principalRepaymentCount);

		//	decimal firstPayment = this.amount - otherPayments * (principalRepaymentCount - 1);

		//	for (int i = 1; i <= this.repaymentCount; i++) {
		//		var sp = new ScheduledItem(AddRepaymentIntervals(i).Date);

		//		if (i <= this.interestOnlyRepaymentCount)
		//			sp.Principal = 0;
		//		else if (i == this.interestOnlyRepaymentCount + 1)
		//			sp.Principal = firstPayment;
		//		else
		//			sp.Principal = otherPayments;

		//		sp.InterestRate = this.interestRate;

		//		if (i <= this.discountPlan.Count)
		//			sp.InterestRate *= 1 + this.discountPlan[i - 1];

		//		loan.Schedule.Add(sp);
		//	} // for

		//	return loan.Schedule;
		//	*/
		//} // Execute



		//private readonly decimal amount;

		//private readonly decimal interestRate;
		//private readonly int repaymentCount;
		//private readonly int interestOnlyRepaymentCount;

		//private readonly List<decimal> discountPlan;
	} // class CreateScheduleMethod
} // namespace
