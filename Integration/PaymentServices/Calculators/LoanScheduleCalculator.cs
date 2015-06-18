namespace PaymentServices.Calculators {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ConfigManager;
	using EZBob.DatabaseLib.Model;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Loans;

	public class LoanScheduleCalculator {
		public int Term {
			get { return Math.Max(this.term, 1); }
			set { this.term = value; }
		} // Term

		public decimal Interest {
			get { return this.interest; }
			set { this.interest = value; }
		} // Interest

		/// <summary>
		/// Calculates loan schedule.
		/// </summary>
		/// <param name="total">Total amount, that is taken</param>
		/// <param name="loan">Loan object, or null. If loan is present, schedule is added to it.</param>
		/// <param name="startDate">Starting day of loan. First payment is month later.</param>
		/// <param name="interestOnlyTerm">Number of months in the beginning of the term to repay interest only.</param>
		/// <param name="spreadSetupFee">Spread set up fee as monthly fees or just not transfer set up fee amount to
		/// customer.</param>
		/// <returns>List of scheduled payments.</returns>
		public IList<LoanScheduleItem> Calculate(
			decimal total,
			Loan loan = null,
			DateTime? startDate = null,
			int interestOnlyTerm = 0,
			bool spreadSetupFee = false
		) {
			IList<LoanScheduleItem> schedule = loan == null ? new List<LoanScheduleItem>(Term) : loan.Schedule;

			if (!startDate.HasValue)
				startDate = DateTime.UtcNow;

			LoanType loanType = loan == null ? new StandardLoanType() : loan.LoanType;
			decimal[] balances = loanType.GetBalances(total, Term, interestOnlyTerm).ToArray();

			decimal[] discounts = GetDiscounts(loan);

			decimal[] setupFees = SpreadSetupFee(loan, spreadSetupFee);

			decimal balance = total;
			decimal repayment = balance;

			for (int m = 0; m < Term; m++) {
				LoanScheduleItem item = null;

				if (schedule.Count != Term) {
					item = new LoanScheduleItem { Status = LoanScheduleStatus.StillToPay };
					schedule.Add(item);
				} else
					item = schedule[m];

				decimal currentInterestRate = Interest + Interest * discounts[m];

				decimal roundedInterest = Math.Round(balance * currentInterestRate, 2);

				repayment = balance - balances[m];
				item.BalanceBeforeRepayment = balance;
				balance = balances[m];

				DateTime scheduledDate = startDate.Value.AddMonths(m + 1);

				decimal currentSetupFee = setupFees[m];

				if ((currentSetupFee > 0) && (loan != null)) {
					loan.Charges.Add(new LoanCharge {
						Amount = currentSetupFee,
						AmountPaid = 0,
						ChargesType = new ConfigurationVariable(CurrentValues.Instance.SpreadSetupFeeCharge),
						Date = scheduledDate,
						Description = "Set-up fee (spread)",
						Loan = loan,
						State = "Active",
					});
				} // if

				item.Loan = loan;
				item.Date = scheduledDate;
				item.Balance = balance;
				item.Interest = roundedInterest;
				item.InterestRate = currentInterestRate;
				item.AmountDue = repayment + roundedInterest + currentSetupFee;
				item.Fees = currentSetupFee;
				item.LoanRepayment = repayment;
			} // for

			if (loan != null) {
				loan.Interest = schedule.Sum(x => x.Interest);
				loan.LoanAmount = total;
				loan.Principal = total;
				loan.Balance = loan.LoanAmount + loan.Interest;
				loan.InterestRate = Interest;
				loan.Date = startDate.Value;
				loan.UpdateNexPayment();
			} // if

			return schedule;
		} // Calculate

		private decimal[] GetDiscounts(Loan loan) {
			var discounts = new decimal[Term];

			if ((loan == null) || (loan.CashRequest == null) || (loan.CashRequest.DiscountPlan == null))
				return discounts.ToArray();

			for (int i = 0; i < loan.CashRequest.DiscountPlan.Discounts.Length; i++) {
				if (i >= Term)
					break;

				discounts[i] = loan.CashRequest.DiscountPlan.Discounts[i] / 100.0m;
			} // for

			return discounts.ToArray();
		} // GetDiscounts

		private decimal[] SpreadSetupFee(Loan loan, bool spreadSetupFee) {
			var setupFee = new decimal[Term];

			if (!spreadSetupFee || (loan == null) || (loan.SetupFee <= 0))
				return setupFee;

			decimal other = Math.Floor(loan.SetupFee / Term);

			setupFee[0] = loan.SetupFee - other * (Term - 1);

			for (int i = 1; i < Term; i++)
				setupFee[i] = other;

			loan.Fees = loan.SetupFee;
			loan.SetupFee = 0;

			return setupFee;
		} // SpreadSetupFee

		private int term = 3;
		private decimal interest = 0.06M;
	} // class LoanScheduleCalculator
} // namespace
