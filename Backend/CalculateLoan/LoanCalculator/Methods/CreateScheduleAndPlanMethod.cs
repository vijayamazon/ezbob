namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Methods {
	using System.Collections.Generic;
	using Ezbob.Backend.CalculateLoan.Models.Helpers;

	internal class CreateScheduleAndPlanMethod : AMethod {
		public CreateScheduleAndPlanMethod(ALoanCalculator calculator, bool writeToLog) : base(calculator, writeToLog) {
		} // constructor

		public List<ScheduledItemWithAmountDue> Execute() {
			new CreateScheduleMethod(Calculator).Execute();
			List<Repayment> payments = new CalculatePlanMethod(Calculator, WriteToLog).Execute();

			var result = new List<ScheduledItemWithAmountDue>();

			for (int i = 0; i < payments.Count; i++) {
				ScheduledItem si = WorkingModel.Schedule[i];
				Repayment payment = payments[i];

				result.Add(new ScheduledItemWithAmountDue(
					i + 1,
					si.Date,
					payment.Principal,
					si.InterestRate,
					payment.Interest
				));
			} // for each

			return result;
		} // Execute
	} // class CreateScheduleAndPlanMethod
} // namespace
