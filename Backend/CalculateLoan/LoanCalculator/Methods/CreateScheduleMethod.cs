namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Methods {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions;
	using Ezbob.Backend.CalculateLoan.Models.Helpers;

	internal class CreateScheduleMethod : AMethod {
		public CreateScheduleMethod(ALoanCalculator calculator) : base(calculator, false) {
		} // constructor

		/// <exception cref="InterestOnlyMonthsCountException">Condition. </exception>
		public virtual List<ScheduledItem> Execute() {
			if (WorkingModel.InterestOnlyRepayments >= WorkingModel.RepaymentCount)
				throw new InterestOnlyMonthsCountException(WorkingModel.InterestOnlyRepayments, WorkingModel.RepaymentCount);

			int principalRepaymentCount = WorkingModel.RepaymentCount - WorkingModel.InterestOnlyRepayments;

			decimal otherPayments = Math.Floor(WorkingModel.LoanAmount / principalRepaymentCount);

			decimal firstPayment = WorkingModel.LoanAmount - otherPayments * (principalRepaymentCount - 1);

			WorkingModel.Schedule.Clear();

			for (int i = 1; i <= WorkingModel.RepaymentCount; i++) {
				var sp = new ScheduledItem(Calculator.AddPeriods(i).Date);

				if (i <= WorkingModel.InterestOnlyRepayments)
					sp.Principal = 0;
				else if (i == WorkingModel.InterestOnlyRepayments + 1)
					sp.Principal = firstPayment;
				else
					sp.Principal = otherPayments;

				sp.InterestRate = WorkingModel.MonthlyInterestRate;

				if (i <= WorkingModel.DiscountPlan.Count)
					sp.InterestRate *= 1 + WorkingModel.DiscountPlan[i - 1];

				WorkingModel.Schedule.Add(sp);
			} // for

			return WorkingModel.Schedule;
		} // Execute
	} // class CreateScheduleMethod
} // namespace
