namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Methods {
	using System;
	using System.Collections.Generic;
	using DbConstants;
	using Ezbob.Backend.Models.NewLoan;
	using Ezbob.Utils;

	internal class RescheduleToIntervalsMethod : AMethod {

		public RescheduleToIntervalsMethod(ALoanCalculator calculator, ReschedulingArgument reschedulingArgument, bool writeToLog)
			: base(calculator, writeToLog) {
			this.reschedulingArgument = reschedulingArgument;
		} // constructor

		public void /* List<ScheduledItemWithAmountDue> */ Execute() {

			//if (this.reschedulingArgument.ReschedulingBalance != null) {
			//	WorkingModel.LoanAmount = (decimal)this.reschedulingArgument.ReschedulingBalance;
			//}

			//if (this.reschedulingArgument.ReschedulingRepaymentIntervalType == RepaymentIntervalTypes.Month) {
			//	WorkingModel.RepaymentCount = MiscUtils.DateDiffInMonths(this.reschedulingArgument.ReschedulingDate, this.reschedulingArgument.LoanCloseDate);
			//	Console.WriteLine("months intervals for balance {0} is {1}, loan original close date ('maturity date'): {2}, reschedule date: {3}", 
			//		this.reschedulingArgument.ReschedulingBalance, WorkingModel.RepaymentCount,
			//		this.reschedulingArgument.LoanCloseDate, this.reschedulingArgument.ReschedulingDate);
			//}

			//if (this.reschedulingArgument.ReschedulingRepaymentIntervalType == RepaymentIntervalTypes.Week) {
			//	WorkingModel.RepaymentCount = MiscUtils.DateDiffInWeeks(this.reschedulingArgument.ReschedulingDate, this.reschedulingArgument.LoanCloseDate);
			//	Console.WriteLine("weeks intervals for balance {0} is {1}, loan original close date ('maturity date'): {2}, reschedule date: {3}", 
			//		this.reschedulingArgument.ReschedulingBalance, WorkingModel.RepaymentCount, this.reschedulingArgument.LoanCloseDate, this.reschedulingArgument.ReschedulingDate);
			//}

			// CreateScheduleAndPlanMethod 
			//new CreateScheduleMethod(Calculator).Execute();

			//var method = new CalculatePlanMethod(Calculator, false);

			//List<Repayment> payments = method.Execute();

			// var result = new List<ScheduledItemWithAmountDue>();

			//for (int i = 0; i < payments.Count; i++) {
			//	ScheduledItem si = WorkingModel.Schedule[i];
			//	Repayment payment = payments[i];

			//	result.Add(new ScheduledItemWithAmountDue(
			//		i + 1,
			//		si.Date,
			//		payment.Principal,
			//		si.InterestRate,
			//		payment.Interest
			//	));
			//} 

			//if (WriteToLog) {
			//	Library.Instance.Log.Debug(
			//		"\n\n{3}.CreateScheduleAndPlan - begin:" +
			//		"\n\nLoan calculator model:\n{0}" +
			//		"\n\nSchedule + plan:\n\t{1}" +
			//		"\n\nDaily data:\n{2}" +
			//		"\n\n{3}.CreateScheduleAndPlan - end." +
			//		"\n\n",
			//		WorkingModel,
			//		string.Join("\n\t", result),
			//		method.DailyLoanStatus.ToFormattedString("\t\t"),
			//		Calculator.Name
			//	);
			//} // if

			// return result;

		} // Execute

		private readonly ReschedulingArgument reschedulingArgument;

	} // class CreateScheduleAndPlanMethod
} // namespace
