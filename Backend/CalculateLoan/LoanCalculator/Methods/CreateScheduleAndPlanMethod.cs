namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Methods {
	using System.Collections.Generic;
	using Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions;
	using Ezbob.Backend.CalculateLoan.Models.Exceptions;
	using Ezbob.Backend.CalculateLoan.Models.Helpers;

	internal class CreateScheduleAndPlanMethod : AMethod {
		public CreateScheduleAndPlanMethod(ALoanCalculator calculator, bool writeToLog) : base(calculator, writeToLog) {
		} // constructor

		/// <exception cref="InterestOnlyMonthsCountException">Condition. </exception>
		/// <exception cref="NegativeMonthlyInterestRateException">Condition. </exception>
		/// <exception cref="NegativeLoanAmountException">Condition. </exception>
		/// <exception cref="NegativeRepaymentCountException">Condition. </exception>
		/// <exception cref="NegativeInterestOnlyRepaymentCountException">Condition. </exception>
		/// <exception cref="NoScheduleException">Condition. </exception>
		/// <exception cref="WrongInstallmentOrderException">Condition. </exception>
		/// <exception cref="WrongFirstOpenPrincipalException">Condition. </exception>
		/// <exception cref="TooLateOpenPrincipalException">Condition. </exception>
		/// <exception cref="WrongOpenPrincipalOrderException">Condition. </exception>
		public List<ScheduledItemWithAmountDue> Execute() {

			new CreateScheduleMethod(Calculator).Execute();

			var method = new CalculatePlanMethod(Calculator, false);

			List<Repayment> payments = method.Execute();

			var result = new List<ScheduledItemWithAmountDue>();

			for (int i = 0; i < payments.Count; i++) {
				ScheduledItem si = WorkingModel.Schedule[i];
				Repayment payment = payments[i];

				result.Add(new ScheduledItemWithAmountDue(
					i + 1,
					si.Date,
					payment.Principal,
					si.InterestRate,
					payment.Interest,
					payment.Fees
				));
			} // for each

			if (WriteToLog) {
				Log.Debug(
					"\n\n{3}.CreateScheduleAndPlan - begin:" +
					"\n\nLoan calculator model:\n{0}" +
					"\n\nSchedule + plan:\n\t{1}" +
					"\n\nDaily data:\n{2}" +
					"\n\n{3}.CreateScheduleAndPlan - end." +
					"\n\n",
					WorkingModel,
					string.Join("\n\t", result),
					method.DailyLoanStatus.ToFormattedString("\t\t"),
					Calculator.Name
				);
			} // if

			return result;
		} // Execute
	} // class CreateScheduleAndPlanMethod
} // namespace
