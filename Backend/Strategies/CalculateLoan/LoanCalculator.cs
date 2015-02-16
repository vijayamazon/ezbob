namespace Ezbob.Backend.Strategies.CalculateLoan {
	using System;

	public class LoanCalculator {
		public LoanCalculator(LoanCalculatorModel model) {
			if (model == null)
				throw new ArgumentNullException("model", "No data for loan calculation.");

			Model = model;
		} // constructor

		/// <summary>
		/// Creates loan schedule by loan issue time, repayment count, repayment interval type and discount plan.
		/// </summary>
		public virtual void CreateSchedule() {
			if (Model.InterestOnlyMonths >= Model.RepaymentCount) {
				throw new ArgumentOutOfRangeException(
					"Interest only months count is not less than repayment count.",
					(Exception)null
				);
			} // if

			int principalRepaymentCount = Model.RepaymentCount - Model.InterestOnlyMonths;

			decimal otherPayments = Math.Floor(Model.LoanAmount / principalRepaymentCount);

			decimal firstPayment = Model.LoanAmount - otherPayments * (principalRepaymentCount - 1);

			Model.Schedule.Clear();

			for (int i = 1; i <= Model.RepaymentCount; i++) {
				var sp = new ScheduledPayment();

				sp.Date = (
					Model.IsMonthly
						? Model.LoanIssueTime.AddMonths(i)
						: Model.LoanIssueTime.AddDays(i * (int)Model.RepaymentIntervalType)
				).Date;

				if (i <= Model.InterestOnlyMonths)
					sp.Amount = 0;
				else if (i == Model.InterestOnlyMonths + 1)
					sp.Amount = firstPayment;
				else
					sp.Amount = otherPayments;

				sp.InterestRate = Model.InterestRate;

				if (i <= Model.DiscountPlan.Count)
					sp.InterestRate *= 1 + Model.DiscountPlan[i - 1];

				Model.Schedule.Add(sp);
			} // for
		} // CreateSchedule

		/// <summary>
		/// Calculates current loan balance.
		/// </summary>
		/// <returns>Current loan balance.</returns>
		public virtual decimal CalculateBalance() {
			return 0;
		} // CalculateBalance

		/// <summary>
		/// Calculates current loan earned interest.
		/// </summary>
		/// <returns>Current loan earned interest.</returns>
		public virtual decimal CalculateEarnedInterest() {
			return 0;
		} // CalculateEarnedInterest

		public virtual LoanCalculatorModel Model { get; private set; }
	} // class LoanCalculator
} // namespace
