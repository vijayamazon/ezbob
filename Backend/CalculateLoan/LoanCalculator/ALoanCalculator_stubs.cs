namespace Ezbob.Backend.CalculateLoan.LoanCalculator {
	using System.Collections.Generic;
	using Ezbob.Backend.CalculateLoan.Models.Helpers;
	using Ezbob.Backend.Models.NewLoan;

	public abstract partial class ALoanCalculator {
		// TODO: missing functionality


		public virtual decimal BalanceForRescheduling(ReschedulingArgument reschedulingArgument) {
			// input WorkingModel
			return 25575m;
		} // BalanceForRescheduling


		public virtual List<ScheduledItemWithAmountDue> RescheduleToIntervals(ReschedulingArgument reschedulingArgument) {
			// input WorkingModel
			List<ScheduledItemWithAmountDue> rescheduledItems = new List<ScheduledItemWithAmountDue>();
			int maxPositions = 5;
			for (int shPositions = 1; shPositions >= maxPositions; shPositions++) {
				rescheduledItems.Add(new ScheduledItemWithAmountDue(5, reschedulingArgument.ReschedulingDate.AddMonths(shPositions), 150, 35, 220));
			}
			return rescheduledItems;
		} // RescheduleToIntervals

	} // class ALoanCalculator
} // namespace
