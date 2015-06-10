namespace Ezbob.Backend.CalculateLoan.LoanCalculator {
	using System;
	using System.Collections.Generic;
	using DbConstants;
	using Ezbob.Backend.CalculateLoan.Models.Helpers;
	using Ezbob.Backend.Models.NewLoan;
	using Ezbob.Utils;

	public abstract partial class ALoanCalculator {
		// TODO: missing functionality


		public virtual decimal BalanceForRescheduling(ReschedulingArgument reschedulingArgument) {
			// input WorkingModel
			return 25575m;
		} // BalanceForRescheduling


		public virtual List<ScheduledItemWithAmountDue> RescheduleToIntervals(ReschedulingArgument reschedulingArgument) {

			List<ScheduledItemWithAmountDue> rescheduledItems = new List<ScheduledItemWithAmountDue>();

			// "loan maturity date" i.e. planned loan close date - today)/interval type
			if (reschedulingArgument.ReschedulingRepaymentIntervalType == RepaymentIntervalTypes.Month) {

				var intervalsNum = MiscUtils.DateDiffInMonths(reschedulingArgument.LoanCloseDate, reschedulingArgument.ReschedulingDate);

				Console.WriteLine("intervals for balance {0} is {1}, loan original close date ('maturity date'): {2}, reschedule date: {3}", reschedulingArgument.ReschedulingBalance, intervalsNum,
					reschedulingArgument.LoanCloseDate, reschedulingArgument.ReschedulingDate);

				decimal monthlyPrincipal = (decimal)(reschedulingArgument.ReschedulingBalance / intervalsNum);

				for (int shPositions = 1; shPositions <= intervalsNum; shPositions++) {
					rescheduledItems.Add(new ScheduledItemWithAmountDue(shPositions, reschedulingArgument.ReschedulingDate.AddMonths(shPositions), monthlyPrincipal, 30, 14));
				}
			}
			return rescheduledItems;

		} // RescheduleToIntervals

	} // class ALoanCalculator
} // namespace
