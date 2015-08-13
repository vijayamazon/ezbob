namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Methods {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DbConstants;
	using Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions;
	using Ezbob.Backend.ModelsWithDB.NewLoan;

	internal class CreateScheduleMethod : AMethod {
		public CreateScheduleMethod(ALoanCalculator calculator, NL_Model loanToCreateModel) : base(calculator, false) {
			if (loanToCreateModel == null)
				throw new NoInitialDataException();

			this.amount = loanToCreateModel.InitialAmount;
			if (this.amount <= 0)
				throw new InvalidInitialAmountException(this.amount);

			this.interestRate = loanToCreateModel.InitialInterestRate;
			if (this.interestRate <= 0)
				throw new InvalidInitialInterestRateException(this.interestRate);

			this.repaymentCount = loanToCreateModel.InitialRepaymentCount;
			if (this.repaymentCount < 1)
				throw new InvalidInitialRepaymentCountException(this.repaymentCount);

			this.interestOnlyRepaymentCount = loanToCreateModel.Loan.InterestOnlyRepaymentCount;
			if ((this.interestOnlyRepaymentCount < 0) || (this.interestOnlyRepaymentCount >= this.repaymentCount)) {
				throw new InvalidInitialInterestOnlyRepaymentCountException(
					this.interestOnlyRepaymentCount,
					this.repaymentCount
				);
			} // if

			this.repaymentIntervalType = (RepaymentIntervalTypes)loanToCreateModel.InitialRepaymentIntervalTypeID;

			this.issuedTime = loanToCreateModel.IssuedTime;

			this.discountPlan = new List<decimal>();

			if ((loanToCreateModel.DiscountPlan != null) && (loanToCreateModel.DiscountPlan.Length > 0))
				this.discountPlan.AddRange(loanToCreateModel.DiscountPlan);
		} // constructor

		public virtual void /* List<ScheduledItem> */ Execute() {
			/*
			LoanHistory loan = WorkingModel.LoanHistory.Last();

			loan.Clear();

			loan.Amount = this.amount;
			loan.RepaymentCount = this.repaymentCount;
			loan.MonthlyInterestRate = this.interestRate;
			loan.RepaymentIntervalType = this.repaymentIntervalType;
			loan.IsFirst = WorkingModel.LoanHistory.Count == 1;

			if (loan.IsFirst) {
				WorkingModel.InterestOnlyRepayments = this.interestOnlyRepaymentCount;
				WorkingModel.DiscountPlan.Clear();
				WorkingModel.DiscountPlan.AddRange(this.discountPlan);
			} // if

			int principalRepaymentCount = this.repaymentCount - this.interestOnlyRepaymentCount;

			decimal otherPayments = Math.Floor(this.amount / principalRepaymentCount);

			decimal firstPayment = this.amount - otherPayments * (principalRepaymentCount - 1);

			for (int i = 1; i <= this.repaymentCount; i++) {
				var sp = new ScheduledItem(AddRepaymentIntervals(i).Date);

				if (i <= this.interestOnlyRepaymentCount)
					sp.Principal = 0;
				else if (i == this.interestOnlyRepaymentCount + 1)
					sp.Principal = firstPayment;
				else
					sp.Principal = otherPayments;

				sp.InterestRate = this.interestRate;

				if (i <= this.discountPlan.Count)
					sp.InterestRate *= 1 + this.discountPlan[i - 1];

				loan.Schedule.Add(sp);
			} // for

			return loan.Schedule;
			*/
		} // Execute

		/// <summary>
		/// Calculates date after requested number of periods have passed since loan issue date.
		/// Periods length is determined from WorkingModel.RepaymentIntervalType.
		/// </summary>
		/// <returns>Date after requested number of periods have been added to loan issue date.</returns>
		/// <param name="periodCount">A number of periods to add.</param>
		/// <returns>Date after requested number of periods have been added to loan issue date.</returns>
		private DateTime AddRepaymentIntervals(int periodCount) {
			return this.repaymentIntervalType == RepaymentIntervalTypes.Month
				? this.issuedTime.AddMonths(periodCount)
				: this.issuedTime.AddDays(periodCount * (int)this.repaymentIntervalType);
		} // AddRepaymentIntervals

		private readonly decimal amount;
		private readonly DateTime issuedTime;
		private readonly decimal interestRate;
		private readonly int repaymentCount;
		private readonly int interestOnlyRepaymentCount;
		private readonly RepaymentIntervalTypes repaymentIntervalType;
		private readonly List<decimal> discountPlan;
	} // class CreateScheduleMethod
} // namespace
